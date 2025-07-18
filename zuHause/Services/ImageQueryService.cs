using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// 圖片查詢服務實作
    /// 提供統一的圖片查詢、主圖判斷和 URL 生成功能
    /// </summary>
    public class ImageQueryService : IImageQueryService
    {
        private readonly ZuHauseContext _context;
        private readonly string _baseImageUrl;

        public ImageQueryService(ZuHauseContext context, IConfiguration configuration)
        {
            _context = context;
            _baseImageUrl = configuration["ImageSettings:BaseUrl"] ?? "/images/";
        }

        /// <summary>
        /// 根據實體類型和實體ID獲取所有圖片
        /// </summary>
        public async Task<List<Image>> GetImagesByEntityAsync(EntityType entityType, int entityId)
        {
            if (entityId <= 0) return new List<Image>();

            return await _context.Images
                .Where(img => img.EntityType == entityType && 
                             img.EntityId == entityId && 
                             img.IsActive)
                .OrderBy(img => img.DisplayOrder ?? int.MaxValue)
                .ThenBy(img => img.ImageId)
                .ToListAsync();
        }

        /// <summary>
        /// 根據實體類型和實體ID獲取主圖
        /// 主圖定義：DisplayOrder 最小值的圖片
        /// </summary>
        public async Task<Image?> GetMainImageAsync(EntityType entityType, int entityId)
        {
            if (entityId <= 0) return null;

            return await _context.Images
                .Where(img => img.EntityType == entityType && 
                             img.EntityId == entityId && 
                             img.IsActive)
                .OrderBy(img => img.DisplayOrder ?? int.MaxValue)
                .ThenBy(img => img.ImageId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根據實體類型、實體ID和圖片分類獲取圖片
        /// </summary>
        public async Task<List<Image>> GetImagesByCategoryAsync(EntityType entityType, int entityId, ImageCategory category)
        {
            if (entityId <= 0) return new List<Image>();

            return await _context.Images
                .Where(img => img.EntityType == entityType && 
                             img.EntityId == entityId && 
                             img.Category == category && 
                             img.IsActive)
                .OrderBy(img => img.DisplayOrder ?? int.MaxValue)
                .ThenBy(img => img.ImageId)
                .ToListAsync();
        }

        /// <summary>
        /// 根據圖片GUID獲取圖片
        /// </summary>
        public async Task<Image?> GetImageByGuidAsync(Guid imageGuid)
        {
            if (imageGuid == Guid.Empty) return null;

            return await _context.Images
                .Where(img => img.ImageGuid == imageGuid && img.IsActive)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根據圖片ID獲取圖片
        /// </summary>
        public async Task<Image?> GetImageByIdAsync(long imageId)
        {
            if (imageId <= 0) return null;

            return await _context.Images
                .Where(img => img.ImageId == imageId && img.IsActive)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 檢查指定實體是否存在圖片
        /// </summary>
        public async Task<bool> HasImagesAsync(EntityType entityType, int entityId)
        {
            if (entityId <= 0) return false;

            return await _context.Images
                .AnyAsync(img => img.EntityType == entityType && 
                                img.EntityId == entityId && 
                                img.IsActive);
        }

        /// <summary>
        /// 獲取指定實體的圖片數量
        /// </summary>
        public async Task<int> GetImageCountAsync(EntityType entityType, int entityId)
        {
            if (entityId <= 0) return 0;

            return await _context.Images
                .CountAsync(img => img.EntityType == entityType && 
                                  img.EntityId == entityId && 
                                  img.IsActive);
        }

        /// <summary>
        /// 生成圖片存取 URL
        /// </summary>
        public string GenerateImageUrl(string storedFileName, ImageSize size = ImageSize.Original)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
                return string.Empty;

            var sizeFolder = size switch
            {
                ImageSize.Thumbnail => "thumbnails",
                ImageSize.Medium => "medium",
                ImageSize.Large => "large",
                ImageSize.Original => "original",
                _ => "original"
            };

            return $"{_baseImageUrl.TrimEnd('/')}/{sizeFolder}/{storedFileName}";
        }

        /// <summary>
        /// 批量生成圖片 URL
        /// </summary>
        public Dictionary<long, string> GenerateImageUrls(List<Image> images, ImageSize size = ImageSize.Original)
        {
            if (images == null || !images.Any())
                return new Dictionary<long, string>();

            return images.ToDictionary(
                img => img.ImageId,
                img => GenerateImageUrl(img.StoredFileName, size)
            );
        }
    }
}