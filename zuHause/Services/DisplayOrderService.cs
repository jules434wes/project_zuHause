using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Services.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// 顯示順序管理服務實作
    /// </summary>
    public class DisplayOrderService : IDisplayOrderService
    {
        private readonly ZuHauseContext _context;

        public DisplayOrderService(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 為新圖片分配下一個可用的顯示順序
        /// </summary>
        public async Task<int> GetNextDisplayOrderAsync(EntityType entityType, int entityId, ImageCategory category)
        {
            var maxOrder = await _context.Images
                .Where(i => i.EntityType == entityType && 
                           i.EntityId == entityId && 
                           i.Category == category && 
                           i.IsActive)
                .MaxAsync(i => (int?)i.DisplayOrder);

            return (maxOrder ?? 0) + 1;
        }

        /// <summary>
        /// 重新排列指定實體和分類的所有圖片順序
        /// </summary>
        public async Task<DisplayOrderResult> ReorderImagesAsync(EntityType entityType, int entityId, ImageCategory category, List<long> imageIds)
        {
            if (imageIds == null || !imageIds.Any())
                return DisplayOrderResult.Failure("圖片ID列表不能為空");

            try
            {
                // 檢查所有圖片是否存在且屬於指定的實體和分類
                var existingImages = await _context.Images
                    .Where(i => imageIds.Contains(i.ImageId) && 
                               i.EntityType == entityType && 
                               i.EntityId == entityId && 
                               i.Category == category && 
                               i.IsActive)
                    .ToListAsync();

                if (existingImages.Count != imageIds.Count)
                {
                    var missingIds = imageIds.Except(existingImages.Select(i => i.ImageId)).ToList();
                    return DisplayOrderResult.Failure($"找不到圖片ID: {string.Join(", ", missingIds)}");
                }

                // 更新顯示順序
                for (int i = 0; i < imageIds.Count; i++)
                {
                    var image = existingImages.First(img => img.ImageId == imageIds[i]);
                    image.DisplayOrder = i + 1;
                }

                await _context.SaveChangesAsync();

                return DisplayOrderResult.Success(existingImages.Count, imageIds);
            }
            catch (Exception ex)
            {
                return DisplayOrderResult.Failure($"重新排列失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 設定主圖（將指定圖片設為 DisplayOrder = 1，其他圖片順序後移）
        /// </summary>
        public async Task<DisplayOrderResult> SetAsPrimaryImageAsync(long imageId)
        {
            try
            {
                var targetImage = await _context.Images
                    .FirstOrDefaultAsync(i => i.ImageId == imageId && i.IsActive);

                if (targetImage == null)
                    return DisplayOrderResult.Failure($"找不到ID為 {imageId} 的圖片");

                // 獲取同一實體和分類的所有圖片，按當前顯示順序排序
                var relatedImages = await _context.Images
                    .Where(i => i.EntityType == targetImage.EntityType && 
                               i.EntityId == targetImage.EntityId && 
                               i.Category == targetImage.Category && 
                               i.IsActive)
                    .OrderBy(i => i.DisplayOrder)
                    .ToListAsync();

                var updatedImageIds = new List<long>();

                // 將目標圖片設為第一張
                if (targetImage.DisplayOrder != 1)
                {
                    targetImage.DisplayOrder = 1;
                    updatedImageIds.Add(targetImage.ImageId);
                }

                // 其他圖片順序後移
                int newOrder = 2;
                foreach (var image in relatedImages.Where(i => i.ImageId != imageId))
                {
                    if (image.DisplayOrder != newOrder)
                    {
                        image.DisplayOrder = newOrder;
                        updatedImageIds.Add(image.ImageId);
                    }
                    newOrder++;
                }

                await _context.SaveChangesAsync();

                return DisplayOrderResult.Success(updatedImageIds.Count, updatedImageIds);
            }
            catch (Exception ex)
            {
                return DisplayOrderResult.Failure($"設定主圖失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次更新圖片顯示順序
        /// </summary>
        public async Task<DisplayOrderResult> BatchUpdateDisplayOrderAsync(List<DisplayOrderUpdate> updates)
        {
            if (updates == null || !updates.Any())
                return DisplayOrderResult.Failure("更新列表不能為空");

            try
            {
                var imageIds = updates.Select(u => u.ImageId).ToList();
                var images = await _context.Images
                    .Where(i => imageIds.Contains(i.ImageId) && i.IsActive)
                    .ToListAsync();

                if (images.Count != updates.Count)
                {
                    var missingIds = imageIds.Except(images.Select(i => i.ImageId)).ToList();
                    return DisplayOrderResult.Failure($"找不到圖片ID: {string.Join(", ", missingIds)}");
                }

                // 檢查新的顯示順序是否有效
                var invalidOrders = updates.Where(u => u.NewDisplayOrder <= 0).ToList();
                if (invalidOrders.Any())
                {
                    var invalidIds = invalidOrders.Select(u => u.ImageId);
                    return DisplayOrderResult.Failure($"無效的顯示順序，圖片ID: {string.Join(", ", invalidIds)}");
                }

                // 更新顯示順序
                var updatedImageIds = new List<long>();
                foreach (var update in updates)
                {
                    var image = images.First(i => i.ImageId == update.ImageId);
                    if (image.DisplayOrder != update.NewDisplayOrder)
                    {
                        image.DisplayOrder = update.NewDisplayOrder;
                        updatedImageIds.Add(image.ImageId);
                    }
                }

                await _context.SaveChangesAsync();

                return DisplayOrderResult.Success(updatedImageIds.Count, updatedImageIds);
            }
            catch (Exception ex)
            {
                return DisplayOrderResult.Failure($"批次更新失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 移除圖片時重新整理順序（填補空隙）
        /// </summary>
        public async Task<DisplayOrderResult> CompactDisplayOrderAsync(EntityType entityType, int entityId, ImageCategory category, int removedDisplayOrder)
        {
            try
            {
                // 獲取被移除圖片之後的所有圖片
                var imagesToUpdate = await _context.Images
                    .Where(i => i.EntityType == entityType && 
                               i.EntityId == entityId && 
                               i.Category == category && 
                               i.IsActive &&
                               i.DisplayOrder > removedDisplayOrder)
                    .OrderBy(i => i.DisplayOrder)
                    .ToListAsync();

                var updatedImageIds = new List<long>();

                // 將後續圖片的順序前移一位
                foreach (var image in imagesToUpdate)
                {
                    image.DisplayOrder = image.DisplayOrder - 1;
                    updatedImageIds.Add(image.ImageId);
                }

                await _context.SaveChangesAsync();

                return DisplayOrderResult.Success(updatedImageIds.Count, updatedImageIds);
            }
            catch (Exception ex)
            {
                return DisplayOrderResult.Failure($"整理順序失敗: {ex.Message}");
            }
        }
    }
}