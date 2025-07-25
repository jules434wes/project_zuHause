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
        private readonly IBlobUrlGenerator _blobUrlGenerator;

        public ImageQueryService(ZuHauseContext context, IBlobUrlGenerator blobUrlGenerator)
        {
            _context = context;
            _blobUrlGenerator = blobUrlGenerator;
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

            // 檢查是否為新的 Blob 路徑格式（包含 temp/ 或分類路徑）
            if (IsNewBlobFormat(storedFileName))
            {
                // 新格式：解析 Blob 路徑並生成 Azure Blob Storage URL
                return GenerateBlobUrl(storedFileName, size);
            }
            else
            {
                // 舊格式：向下相容，假設為舊的本地檔案格式
                // 直接使用 Azure Blob Storage 的基礎 URL 加上檔名
                return GenerateLegacyUrl(storedFileName, size);
            }
        }

        /// <summary>
        /// 檢查是否為新的 Blob 路徑格式
        /// </summary>
        /// <param name="storedFileName">儲存檔名</param>
        /// <returns>是否為新格式</returns>
        private bool IsNewBlobFormat(string storedFileName)
        {
            // 新格式特徵：包含路徑分隔符且不只是檔名
            return storedFileName.Contains("/") || 
                   storedFileName.StartsWith("temp/") ||
                   storedFileName.StartsWith("property/") ||
                   storedFileName.StartsWith("furniture/") ||
                   storedFileName.StartsWith("legacy/");
        }

        /// <summary>
        /// 根據 Blob 路徑生成 URL
        /// </summary>
        /// <param name="storedFileName">Blob 路徑</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>完整 URL</returns>
        private string GenerateBlobUrl(string storedFileName, ImageSize size)
        {
            try
            {
                // 解析 Blob 路徑格式
                var parts = storedFileName.Split('/');
                
                if (parts.Length >= 4 && parts[0] == "temp")
                {
                    // 臨時檔案格式：temp/{sessionId}/{category}/{entityId}/{imageGuid}.webp
                    var tempSessionId = parts[1];
                    var fileName = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);
                    if (Guid.TryParse(fileName, out var imageGuid))
                    {
                        return _blobUrlGenerator.GenerateTempImageUrl(tempSessionId, imageGuid, size);
                    }
                }
                else if (parts.Length >= 3)
                {
                    // 正式檔案格式：{category}/{entityId}/{imageGuid}.webp
                    var categoryStr = parts[0];
                    if (Enum.TryParse<ImageCategory>(categoryStr, true, out var category) &&
                        int.TryParse(parts[1], out var entityId))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);
                        if (Guid.TryParse(fileName, out var imageGuid))
                        {
                            return _blobUrlGenerator.GenerateImageUrl(category, entityId, imageGuid, size);
                        }
                    }
                }

                // 無法解析，使用原始路徑
                return GenerateLegacyUrl(storedFileName, size);
            }
            catch
            {
                // 解析失敗，使用原始路徑
                return GenerateLegacyUrl(storedFileName, size);
            }
        }

        /// <summary>
        /// 生成舊格式的 URL（向下相容）
        /// </summary>
        /// <param name="storedFileName">儲存檔名</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>URL</returns>
        private string GenerateLegacyUrl(string storedFileName, ImageSize size)
        {
            // 使用 Azure Blob Storage 的基礎 URL
            // 這裡暫時使用硬編碼，實際上應該從配置中取得
            var baseUrl = "https://zuhauseimg.blob.core.windows.net/zuhaus-images";
            
            var sizeFolder = size switch
            {
                ImageSize.Thumbnail => "thumbnails",
                ImageSize.Medium => "medium", 
                ImageSize.Large => "large",
                ImageSize.Original => "original",
                _ => "original"
            };

            return $"{baseUrl}/legacy/{sizeFolder}/{storedFileName}";
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