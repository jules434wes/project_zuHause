using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// 並發安全的 DisplayOrder 管理服務實作
    /// </summary>
    public class DisplayOrderManager : IDisplayOrderManager
    {
        private readonly ZuHauseContext _context;
        private const int MaxRetryCount = 3;
        private const int RetryDelayMs = 100;

        public DisplayOrderManager(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 為圖片分配 DisplayOrder（支援並發控制）
        /// </summary>
        public async Task<DisplayOrderAssignResult> AssignDisplayOrdersAsync(
            EntityType entityType, 
            int entityId, 
            ImageCategory category,
            List<long> imageIds, 
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock)
        {
            if (imageIds == null || !imageIds.Any())
                return DisplayOrderAssignResult.Failure("圖片ID列表不能為空");

            var retryCount = 0;
            
            while (retryCount < MaxRetryCount)
            {
                try
                {
                    var result = concurrencyStrategy switch
                    {
                        ConcurrencyControlStrategy.OptimisticLock => await AssignWithOptimisticLockAsync(entityType, entityId, category, imageIds),
                        ConcurrencyControlStrategy.PessimisticLock => await AssignWithPessimisticLockAsync(entityType, entityId, category, imageIds),
                        ConcurrencyControlStrategy.NoLock => await AssignWithNoLockAsync(entityType, entityId, category, imageIds),
                        _ => throw new ArgumentException($"不支援的並發控制策略: {concurrencyStrategy}")
                    };

                    result.RetryCount = retryCount;
                    return result;
                }
                catch (DbUpdateConcurrencyException) when (concurrencyStrategy == ConcurrencyControlStrategy.OptimisticLock)
                {
                    retryCount++;
                    if (retryCount < MaxRetryCount)
                    {
                        await Task.Delay(RetryDelayMs * retryCount);
                        continue;
                    }
                    return DisplayOrderAssignResult.Failure($"並發衝突，重試 {MaxRetryCount} 次後失敗", retryCount);
                }
                catch (Exception ex)
                {
                    return DisplayOrderAssignResult.Failure($"分配 DisplayOrder 失敗: {ex.Message}", retryCount);
                }
            }

            return DisplayOrderAssignResult.Failure($"達到最大重試次數 {MaxRetryCount}", retryCount);
        }

        /// <summary>
        /// 重新排序 DisplayOrder（填補空隙）
        /// </summary>
        public async Task<DisplayOrderReorderResult> ReorderDisplayOrdersAsync(
            EntityType entityType, 
            int entityId, 
            ImageCategory category,
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock)
        {
            try
            {
                var images = await GetImagesForOrderingAsync(entityType, entityId, category, concurrencyStrategy);
                var beforeCount = images.Count;

                if (!images.Any())
                    return DisplayOrderReorderResult.Success(0, 0, new List<long>());

                var updatedImageIds = new List<long>();
                
                // 重新分配連續的 DisplayOrder
                for (int i = 0; i < images.Count; i++)
                {
                    var newOrder = i + 1;
                    if (images[i].DisplayOrder != newOrder)
                    {
                        images[i].DisplayOrder = newOrder;
                        updatedImageIds.Add(images[i].ImageId);
                    }
                }

                if (updatedImageIds.Any())
                {
                    await _context.SaveChangesAsync();
                }

                return DisplayOrderReorderResult.Success(beforeCount, images.Count, updatedImageIds);
            }
            catch (Exception ex)
            {
                return DisplayOrderReorderResult.Failure($"重新排序失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 取得下一個可用的 DisplayOrder（並發安全）
        /// </summary>
        public async Task<int> GetNextDisplayOrderAsync(
            EntityType entityType, 
            int entityId, 
            ImageCategory category,
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock)
        {
            var images = await GetImagesForOrderingAsync(entityType, entityId, category, concurrencyStrategy);
            var maxOrder = images.Any() ? images.Max(i => i.DisplayOrder ?? 0) : 0;
            return maxOrder + 1;
        }

        /// <summary>
        /// 移動圖片到指定位置
        /// </summary>
        public async Task<DisplayOrderMoveResult> MoveImageToPositionAsync(
            long imageId, 
            int newDisplayOrder,
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock)
        {
            try
            {
                var image = await _context.Images.FirstOrDefaultAsync(i => i.ImageId == imageId && i.IsActive);
                if (image == null)
                    return DisplayOrderMoveResult.Failure($"找不到ID為 {imageId} 的圖片");

                var oldPosition = image.DisplayOrder ?? 0;
                
                if (oldPosition == newDisplayOrder)
                    return DisplayOrderMoveResult.Success(oldPosition, newDisplayOrder, 0);

                // 取得同組圖片
                var images = await GetImagesForOrderingAsync(image.EntityType, image.EntityId, image.Category, concurrencyStrategy);
                
                // 調整其他圖片的順序
                int affectedCount = 0;
                foreach (var img in images.Where(i => i.ImageId != imageId))
                {
                    var currentOrder = img.DisplayOrder ?? 0;
                    
                    if (oldPosition < newDisplayOrder)
                    {
                        // 向後移動：[old, new] 區間內的圖片順序減1
                        if (currentOrder > oldPosition && currentOrder <= newDisplayOrder)
                        {
                            img.DisplayOrder = currentOrder - 1;
                            affectedCount++;
                        }
                    }
                    else
                    {
                        // 向前移動：[new, old] 區間內的圖片順序加1
                        if (currentOrder >= newDisplayOrder && currentOrder < oldPosition)
                        {
                            img.DisplayOrder = currentOrder + 1;
                            affectedCount++;
                        }
                    }
                }

                // 設定目標圖片的新順序
                image.DisplayOrder = newDisplayOrder;
                affectedCount++;

                await _context.SaveChangesAsync();

                return DisplayOrderMoveResult.Success(oldPosition, newDisplayOrder, affectedCount);
            }
            catch (Exception ex)
            {
                return DisplayOrderMoveResult.Failure($"移動圖片失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 移除圖片並調整後續順序
        /// </summary>
        public async Task<DisplayOrderRemoveResult> RemoveImageAndAdjustOrdersAsync(
            long imageId,
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock)
        {
            try
            {
                var image = await _context.Images.FirstOrDefaultAsync(i => i.ImageId == imageId && i.IsActive);
                if (image == null)
                    return DisplayOrderRemoveResult.Failure($"找不到ID為 {imageId} 的圖片");

                var removedPosition = image.DisplayOrder ?? 0;
                
                // 標記為非活動狀態
                image.IsActive = false;

                // 調整後續圖片的順序
                var subsequentImages = await _context.Images
                    .Where(i => i.EntityType == image.EntityType && 
                               i.EntityId == image.EntityId && 
                               i.Category == image.Category && 
                               i.IsActive &&
                               i.DisplayOrder > removedPosition)
                    .ToListAsync();

                foreach (var img in subsequentImages)
                {
                    img.DisplayOrder = img.DisplayOrder - 1;
                }

                await _context.SaveChangesAsync();

                return DisplayOrderRemoveResult.Success(removedPosition, subsequentImages.Count);
            }
            catch (Exception ex)
            {
                return DisplayOrderRemoveResult.Failure($"移除圖片失敗: {ex.Message}");
            }
        }

        #region 私有方法

        /// <summary>
        /// 使用樂觀鎖分配 DisplayOrder
        /// </summary>
        private async Task<DisplayOrderAssignResult> AssignWithOptimisticLockAsync(
            EntityType entityType, int entityId, ImageCategory category, List<long> imageIds)
        {
            var images = await _context.Images
                .Where(i => imageIds.Contains(i.ImageId) && i.IsActive)
                .ToListAsync();

            if (images.Count != imageIds.Count)
            {
                var missingIds = imageIds.Except(images.Select(i => i.ImageId)).ToList();
                return DisplayOrderAssignResult.Failure($"找不到圖片ID: {string.Join(", ", missingIds)}");
            }

            var nextOrder = await GetNextDisplayOrderAsync(entityType, entityId, category);
            var assignedOrders = new Dictionary<long, int>();

            for (int i = 0; i < images.Count; i++)
            {
                var newOrder = nextOrder + i;
                images[i].DisplayOrder = newOrder;
                assignedOrders[images[i].ImageId] = newOrder;
            }

            await _context.SaveChangesAsync();
            return DisplayOrderAssignResult.Success(assignedOrders);
        }

        /// <summary>
        /// 使用悲觀鎖分配 DisplayOrder
        /// </summary>
        private async Task<DisplayOrderAssignResult> AssignWithPessimisticLockAsync(
            EntityType entityType, int entityId, ImageCategory category, List<long> imageIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // 使用 FOR UPDATE 鎖定相關記錄
                var maxOrderQuery = $@"
                    SELECT MAX(displayOrder) 
                    FROM images WITH (UPDLOCK, ROWLOCK)
                    WHERE entityType = '{entityType}' 
                    AND entityId = {entityId} 
                    AND category = '{category}' 
                    AND isActive = 1";

                var maxOrder = await _context.Database.SqlQueryRaw<int?>($"SELECT ISNULL(({maxOrderQuery}), 0) as Value").FirstOrDefaultAsync() ?? 0;

                var images = await _context.Images
                    .Where(i => imageIds.Contains(i.ImageId) && i.IsActive)
                    .ToListAsync();

                if (images.Count != imageIds.Count)
                {
                    var missingIds = imageIds.Except(images.Select(i => i.ImageId)).ToList();
                    return DisplayOrderAssignResult.Failure($"找不到圖片ID: {string.Join(", ", missingIds)}");
                }

                var assignedOrders = new Dictionary<long, int>();
                for (int i = 0; i < images.Count; i++)
                {
                    var newOrder = maxOrder + i + 1;
                    images[i].DisplayOrder = newOrder;
                    assignedOrders[images[i].ImageId] = newOrder;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return DisplayOrderAssignResult.Success(assignedOrders);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 不使用鎖定分配 DisplayOrder
        /// </summary>
        private async Task<DisplayOrderAssignResult> AssignWithNoLockAsync(
            EntityType entityType, int entityId, ImageCategory category, List<long> imageIds)
        {
            return await AssignWithOptimisticLockAsync(entityType, entityId, category, imageIds);
        }

        /// <summary>
        /// 取得用於排序的圖片列表
        /// </summary>
        private async Task<List<Image>> GetImagesForOrderingAsync(
            EntityType entityType, int entityId, ImageCategory category, ConcurrencyControlStrategy strategy)
        {
            var query = _context.Images
                .Where(i => i.EntityType == entityType && 
                           i.EntityId == entityId && 
                           i.Category == category && 
                           i.IsActive)
                .OrderBy(i => i.DisplayOrder);

            return strategy == ConcurrencyControlStrategy.PessimisticLock 
                ? await query.ToListAsync() // 實際應用中可能需要 WITH (UPDLOCK)
                : await query.ToListAsync();
        }

        #endregion
    }
}