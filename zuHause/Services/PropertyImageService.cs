using Microsoft.AspNetCore.Http;
using zuHause.DTOs;
using zuHause.Enums;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Services
{
    /// <summary>
    /// 房源圖片處理服務 - 純 Facade 模式，委託給統一圖片管理系統
    /// 保持向後相容性，內部完全委託給 IImageUploadService 和 IImageQueryService
    /// </summary>
    public class PropertyImageService : IPropertyImageService
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly IImageQueryService _imageQueryService;
        private readonly ILogger<PropertyImageService> _logger;

        public PropertyImageService(
            IImageUploadService imageUploadService,
            IImageQueryService imageQueryService,
            ILogger<PropertyImageService> logger)
        {
            _imageUploadService = imageUploadService;
            _imageQueryService = imageQueryService;
            _logger = logger;
        }

        /// <summary>
        /// 上傳房源圖片 - Facade 方法，完全委託給統一圖片上傳服務
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="files">上傳的圖片檔案</param>
        /// <returns>處理結果列表，轉換為 PropertyImageResult 保持向後相容性</returns>
        public async Task<List<PropertyImageResult>> UploadPropertyImagesAsync(int propertyId, IFormFileCollection files)
        {
            try
            {
                _logger.LogInformation("開始上傳房源圖片，房源ID: {PropertyId}, 檔案數量: {FileCount}", propertyId, files.Count);

                // 完全委託給統一圖片上傳服務
                var uploadResults = await _imageUploadService.UploadImagesAsync(
                    files, 
                    EntityType.Property, 
                    propertyId, 
                    ImageCategory.Gallery);

                // 轉換為 PropertyImageResult 以保持向後相容性
                var propertyResults = uploadResults.Select(ConvertToPropertyImageResult).ToList();

                var successCount = propertyResults.Count(r => r.Success);
                _logger.LogInformation("房源圖片上傳完成，房源ID: {PropertyId}, 成功: {SuccessCount}/{TotalCount}", 
                    propertyId, successCount, propertyResults.Count);

                return propertyResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳房源圖片時發生錯誤，房源ID: {PropertyId}", propertyId);
                return new List<PropertyImageResult> 
                { 
                    PropertyImageResult.CreateFailure("", $"系統錯誤: {ex.Message}") 
                };
            }
        }

        /// <summary>
        /// 獲取房源主圖 - Facade 方法，委託給統一圖片查詢服務
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>主圖資訊，如果沒有則返回 null</returns>
        public async Task<Image?> GetMainPropertyImageAsync(int propertyId)
        {
            try
            {
                return await _imageQueryService.GetMainImageAsync(EntityType.Property, propertyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取房源主圖失敗，房源ID: {PropertyId}", propertyId);
                return null;
            }
        }

        /// <summary>
        /// 獲取房源所有圖片 - Facade 方法，委託給統一圖片查詢服務
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>房源圖片列表</returns>
        public async Task<List<Image>> GetPropertyImagesAsync(int propertyId)
        {
            try
            {
                return await _imageQueryService.GetImagesByEntityAsync(EntityType.Property, propertyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取房源圖片列表失敗，房源ID: {PropertyId}", propertyId);
                return new List<Image>();
            }
        }

        /// <summary>
        /// 獲取房源圖片數量 - 新增的便利方法
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>圖片數量</returns>
        public async Task<int> GetPropertyImageCountAsync(int propertyId)
        {
            try
            {
                return await _imageQueryService.GetImageCountAsync(EntityType.Property, propertyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取房源圖片數量失敗，房源ID: {PropertyId}", propertyId);
                return 0;
            }
        }

        /// <summary>
        /// 檢查房源是否有圖片 - 新增的便利方法
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>是否有圖片</returns>
        public async Task<bool> HasPropertyImagesAsync(int propertyId)
        {
            try
            {
                return await _imageQueryService.HasImagesAsync(EntityType.Property, propertyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查房源圖片存在性失敗，房源ID: {PropertyId}", propertyId);
                return false;
            }
        }

        /// <summary>
        /// 刪除房源圖片 - Facade 方法，委託給統一圖片上傳服務
        /// </summary>
        /// <param name="imageId">圖片ID</param>
        /// <returns>刪除是否成功</returns>
        public async Task<bool> DeletePropertyImageAsync(long imageId)
        {
            try
            {
                _logger.LogInformation("開始刪除房源圖片，圖片ID: {ImageId}", imageId);
                var result = await _imageUploadService.DeleteImageAsync(imageId);
                
                if (result)
                {
                    _logger.LogInformation("成功刪除房源圖片，圖片ID: {ImageId}", imageId);
                }
                else
                {
                    _logger.LogWarning("刪除房源圖片失敗，圖片ID: {ImageId}", imageId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除房源圖片時發生錯誤，圖片ID: {ImageId}", imageId);
                return false;
            }
        }

        /// <summary>
        /// 刪除房源所有圖片 - Facade 方法，委託給統一圖片上傳服務
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>刪除是否成功</returns>
        public async Task<bool> DeleteAllPropertyImagesAsync(int propertyId)
        {
            try
            {
                _logger.LogInformation("開始刪除房源所有圖片，房源ID: {PropertyId}", propertyId);
                var result = await _imageUploadService.DeleteImagesByEntityAsync(
                    EntityType.Property, 
                    propertyId, 
                    ImageCategory.Gallery);
                
                if (result)
                {
                    _logger.LogInformation("成功刪除房源所有圖片，房源ID: {PropertyId}", propertyId);
                }
                else
                {
                    _logger.LogWarning("刪除房源所有圖片失敗，房源ID: {PropertyId}", propertyId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除房源所有圖片時發生錯誤，房源ID: {PropertyId}", propertyId);
                return false;
            }
        }

        /// <summary>
        /// 設定房源主圖 - Facade 方法，委託給統一圖片上傳服務
        /// </summary>
        /// <param name="imageId">要設為主圖的圖片ID</param>
        /// <returns>設定是否成功</returns>
        public async Task<bool> SetMainPropertyImageAsync(long imageId)
        {
            try
            {
                _logger.LogInformation("開始設定房源主圖，圖片ID: {ImageId}", imageId);
                var result = await _imageUploadService.SetMainImageAsync(imageId);
                
                if (result)
                {
                    _logger.LogInformation("成功設定房源主圖，圖片ID: {ImageId}", imageId);
                }
                else
                {
                    _logger.LogWarning("設定房源主圖失敗，圖片ID: {ImageId}", imageId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定房源主圖時發生錯誤，圖片ID: {ImageId}", imageId);
                return false;
            }
        }

        /// <summary>
        /// 重新排序房源圖片 - Facade 方法，委託給統一圖片上傳服務
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="imageIds">按新順序排列的圖片ID列表</param>
        /// <returns>重新排序是否成功</returns>
        public async Task<bool> ReorderPropertyImagesAsync(int propertyId, List<long> imageIds)
        {
            try
            {
                _logger.LogInformation("開始重新排序房源圖片，房源ID: {PropertyId}, 圖片數量: {ImageCount}", 
                    propertyId, imageIds.Count);
                
                var result = await _imageUploadService.ReorderImagesAsync(EntityType.Property, propertyId, imageIds);
                
                if (result)
                {
                    _logger.LogInformation("成功重新排序房源圖片，房源ID: {PropertyId}", propertyId);
                }
                else
                {
                    _logger.LogWarning("重新排序房源圖片失敗，房源ID: {PropertyId}", propertyId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新排序房源圖片時發生錯誤，房源ID: {PropertyId}", propertyId);
                return false;
            }
        }

        /// <summary>
        /// 生成房源圖片 URL - Facade 方法，委託給統一圖片查詢服務
        /// </summary>
        /// <param name="storedFileName">儲存檔名</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>圖片 URL</returns>
        public string GeneratePropertyImageUrl(string storedFileName, ImageSize size = ImageSize.Original)
        {
            try
            {
                return _imageQueryService.GenerateImageUrl(storedFileName, size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成房源圖片 URL 失敗，檔名: {FileName}", storedFileName);
                return string.Empty;
            }
        }

        // === 私有輔助方法 ===

        /// <summary>
        /// 將 ImageUploadResult 轉換為 PropertyImageResult 以保持向後相容性
        /// </summary>
        private PropertyImageResult ConvertToPropertyImageResult(ImageUploadResult uploadResult)
        {
            if (!uploadResult.Success)
            {
                return PropertyImageResult.CreateFailure(uploadResult.OriginalFileName, uploadResult.ErrorMessage ?? "未知錯誤");
            }

            // 為了向後相容性，我們需要生成圖片路徑
            // 這裡使用 URL 生成器來建立相容的路徑格式
            var originalPath = GeneratePropertyImageUrl(uploadResult.StoredFileName, ImageSize.Original);
            var mediumPath = GeneratePropertyImageUrl(uploadResult.StoredFileName, ImageSize.Medium);
            var thumbnailPath = GeneratePropertyImageUrl(uploadResult.StoredFileName, ImageSize.Thumbnail);

            return PropertyImageResult.CreateSuccess(
                uploadResult.OriginalFileName,
                (int)uploadResult.ImageId!.Value, // 轉換為 int 以保持向後相容性
                originalPath,
                mediumPath,
                thumbnailPath,
                uploadResult.IsMainImage,
                uploadResult.FileSizeBytes,
                uploadResult.FileSizeBytes, // 向後相容性：使用相同的檔案大小
                uploadResult.Width,
                uploadResult.Height
            );
        }
    }
}