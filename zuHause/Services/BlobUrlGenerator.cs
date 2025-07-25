using Microsoft.Extensions.Options;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Options;
using zuHause.Enums;

namespace zuHause.Services
{
    /// <summary>
    /// Blob Storage URL 生成服務實作
    /// 提供統一的圖片 URL 生成邏輯，支援臨時區域和正式區域
    /// </summary>
    public class BlobUrlGenerator : IBlobUrlGenerator
    {
        private readonly BlobStorageOptions _options;
        private readonly ILogger<BlobUrlGenerator> _logger;

        public BlobUrlGenerator(
            IOptions<BlobStorageOptions> options,
            ILogger<BlobUrlGenerator> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// 生成正式區域的圖片 URL
        /// 格式: https://zuhauseimg.blob.core.windows.net/zuhaus-images/{category}/{entityId}/{size}/{imageGuid}.webp
        /// </summary>
        public string GenerateImageUrl(ImageCategory category, int entityId, Guid imageGuid, ImageSize size)
        {
            try
            {
                var blobPath = GetBlobPath(category, entityId, imageGuid, size);
                var url = $"{_options.BaseUrl.TrimEnd('/')}/{blobPath}";
                
                _logger.LogDebug("生成正式區域 URL: {URL}", url);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成正式區域 URL 失敗: Category={Category}, EntityId={EntityId}, ImageGuid={ImageGuid}, Size={Size}", 
                    category, entityId, imageGuid, size);
                throw;
            }
        }

        /// <summary>
        /// 生成臨時區域的圖片 URL
        /// 格式: https://zuhauseimg.blob.core.windows.net/zuhaus-images/temp/{tempSessionId}/{size}/{imageGuid}.webp
        /// </summary>
        public string GenerateTempImageUrl(string tempSessionId, Guid imageGuid, ImageSize size)
        {
            try
            {
                if (string.IsNullOrEmpty(tempSessionId) || tempSessionId.Length != 32)
                {
                    throw new ArgumentException("臨時會話 ID 必須是 32 字元", nameof(tempSessionId));
                }

                if (imageGuid == Guid.Empty)
                {
                    throw new ArgumentException("圖片 GUID 不能為空", nameof(imageGuid));
                }

                var blobPath = GetTempBlobPath(tempSessionId, imageGuid, size);
                var url = $"{_options.BaseUrl.TrimEnd('/')}/{blobPath}";
                
                _logger.LogDebug("生成臨時區域 URL: {URL}", url);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成臨時區域 URL 失敗: TempSessionId={TempSessionId}, ImageGuid={ImageGuid}, Size={Size}", 
                    tempSessionId, imageGuid, size);
                throw;
            }
        }

        /// <summary>
        /// 取得 Blob 路徑（不含基礎 URL）
        /// 格式: {category}/{entityId}/{size}/{imageGuid}.webp
        /// </summary>
        public string GetBlobPath(ImageCategory category, int entityId, Guid imageGuid, ImageSize size)
        {
            try
            {
                if (entityId <= 0)
                {
                    throw new ArgumentException("實體 ID 必須大於 0", nameof(entityId));
                }

                if (imageGuid == Guid.Empty)
                {
                    throw new ArgumentException("圖片 GUID 不能為空", nameof(imageGuid));
                }

                var categoryName = GetCategoryName(category);
                var sizeName = GetSizeName(size);
                var fileName = $"{imageGuid:N}.webp";

                return $"{categoryName}/{entityId}/{sizeName}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成 Blob 路徑失敗: Category={Category}, EntityId={EntityId}, ImageGuid={ImageGuid}, Size={Size}", 
                    category, entityId, imageGuid, size);
                throw;
            }
        }

        /// <summary>
        /// 取得臨時 Blob 路徑（不含基礎 URL）
        /// 格式: temp/{tempSessionId}/{size}/{imageGuid}.webp
        /// </summary>
        public string GetTempBlobPath(string tempSessionId, Guid imageGuid, ImageSize size)
        {
            try
            {
                if (string.IsNullOrEmpty(tempSessionId) || tempSessionId.Length != 32)
                {
                    throw new ArgumentException("臨時會話 ID 必須是 32 字元", nameof(tempSessionId));
                }

                if (imageGuid == Guid.Empty)
                {
                    throw new ArgumentException("圖片 GUID 不能為空", nameof(imageGuid));
                }

                var sizeName = GetSizeName(size);
                var fileName = $"{imageGuid:N}.webp";

                return $"temp/{tempSessionId}/{sizeName}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成臨時 Blob 路徑失敗: TempSessionId={TempSessionId}, ImageGuid={ImageGuid}, Size={Size}", 
                    tempSessionId, imageGuid, size);
                throw;
            }
        }

        /// <summary>
        /// 驗證 URL 格式是否正確
        /// </summary>
        public bool IsValidImageUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            try
            {
                var uri = new Uri(url);
                
                // 檢查是否為 HTTPS
                if (uri.Scheme != "https")
                {
                    return false;
                }

                // 檢查主機名稱
                if (uri.Host != "zuhauseimg.blob.core.windows.net")
                {
                    return false;
                }

                // 檢查容器名稱
                var pathSegments = uri.AbsolutePath.Trim('/').Split('/');
                if (pathSegments.Length < 1 || pathSegments[0] != "zuhaus-images")
                {
                    return false;
                }

                // 檢查檔案副檔名
                if (!url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // 檢查路徑結構
                if (pathSegments.Length >= 2 && pathSegments[1] == "temp")
                {
                    // 臨時區域：temp/{tempSessionId}/{size}/{imageGuid}.webp
                    return pathSegments.Length == 5 && 
                           pathSegments[2].Length == 32 &&  // TempSessionId
                           IsValidSizeName(pathSegments[3]) &&  // Size
                           IsValidGuidFileName(pathSegments[4]);  // ImageGuid.webp
                }
                else
                {
                    // 正式區域：{category}/{entityId}/{size}/{imageGuid}.webp
                    return pathSegments.Length == 5 &&
                           IsValidCategoryName(pathSegments[1]) &&  // Category
                           int.TryParse(pathSegments[2], out var entityId) && entityId > 0 &&  // EntityId
                           IsValidSizeName(pathSegments[3]) &&  // Size
                           IsValidGuidFileName(pathSegments[4]);  // ImageGuid.webp
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得圖片分類名稱
        /// </summary>
        private static string GetCategoryName(ImageCategory category)
        {
            return category switch
            {
                ImageCategory.BedRoom => "BedRoom",
                ImageCategory.Kitchen => "Kitchen",
                ImageCategory.Living => "Living",
                ImageCategory.Balcony => "Balcony",
                ImageCategory.Avatar => "Avatar",
                ImageCategory.Gallery => "Gallery",
                ImageCategory.Product => "Product",
                _ => throw new ArgumentException($"不支援的圖片分類: {category}")
            };
        }

        /// <summary>
        /// 取得圖片尺寸名稱
        /// </summary>
        private static string GetSizeName(ImageSize size)
        {
            return size switch
            {
                ImageSize.Original => "original",
                ImageSize.Large => "large", 
                ImageSize.Medium => "medium",
                ImageSize.Thumbnail => "thumbnail",
                _ => throw new ArgumentException($"不支援的圖片尺寸: {size}")
            };
        }

        /// <summary>
        /// 驗證分類名稱是否有效
        /// </summary>
        private static bool IsValidCategoryName(string categoryName)
        {
            return categoryName switch
            {
                "BedRoom" or "Kitchen" or "Living" or "Balcony" or
                "Avatar" or "Gallery" or "Product" => true,
                _ => false
            };
        }

        /// <summary>
        /// 驗證尺寸名稱是否有效
        /// </summary>
        private static bool IsValidSizeName(string sizeName)
        {
            return sizeName switch
            {
                "original" or "large" or "medium" or "thumbnail" => true,
                _ => false
            };
        }

        /// <summary>
        /// 驗證 GUID 檔案名稱是否有效
        /// </summary>
        private static bool IsValidGuidFileName(string fileName)
        {
            if (!fileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var guidPart = fileName.Substring(0, fileName.Length - 5); // 移除 .webp
            return guidPart.Length == 32 && Guid.TryParse(guidPart.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-"), out _);
        }
    }
}