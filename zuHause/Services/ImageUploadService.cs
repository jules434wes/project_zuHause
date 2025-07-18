using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using zuHause.DTOs;
using zuHause.Enums;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Services
{
    /// <summary>
    /// 統一圖片上傳服務實作 - 支援所有實體類型的圖片上傳
    /// </summary>
    public class ImageUploadService : IImageUploadService
    {
        private readonly ZuHauseContext _context;
        private readonly IImageProcessor _imageProcessor;
        private readonly IEntityExistenceChecker _entityExistenceChecker;
        private readonly IDisplayOrderManager _displayOrderManager;
        private readonly ILogger<ImageUploadService> _logger;

        // 上傳限制常數
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB
        private const int MaxImageCount = 15;
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public ImageUploadService(
            ZuHauseContext context,
            IImageProcessor imageProcessor,
            IEntityExistenceChecker entityExistenceChecker,
            IDisplayOrderManager displayOrderManager,
            ILogger<ImageUploadService> logger)
        {
            _context = context;
            _imageProcessor = imageProcessor;
            _entityExistenceChecker = entityExistenceChecker;
            _displayOrderManager = displayOrderManager;
            _logger = logger;
        }

        /// <summary>
        /// 批次上傳圖片
        /// </summary>
        public async Task<List<ImageUploadResult>> UploadImagesAsync(
            IFormFileCollection files, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category, 
            int? uploadedByMemberId = null)
        {
            var results = new List<ImageUploadResult>();

            try
            {
                // 1. 基本驗證
                var validationResult = await ValidateUploadRequestAsync(files, entityType, entityId, category);
                if (validationResult.Any(r => !r.IsSuccess))
                {
                    return validationResult;
                }

                // 2. 實體存在性驗證
                var entityExists = await _entityExistenceChecker.ExistsAsync(entityType, entityId);
                if (!entityExists)
                {
                    results.Add(ImageUploadResult.Failure("", $"{entityType} ID {entityId} 不存在"));
                    return results;
                }

                // 3. 檢查目前圖片數量
                var currentImageCount = await _context.Images
                    .CountAsync(i => i.EntityType == entityType && i.EntityId == entityId && i.IsActive);

                if (currentImageCount + files.Count > MaxImageCount)
                {
                    results.Add(ImageUploadResult.Failure("", 
                        $"超過圖片數量限制，目前已有 {currentImageCount} 張，最多允許 {MaxImageCount} 張"));
                    return results;
                }

                // 4. 處理每個檔案
                var uploadedImageIds = new List<long>();
                
                // 只在非 InMemory 資料庫使用交易
                var isInMemory = _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
                var transaction = isInMemory ? null : await _context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var file in files)
                    {
                        var result = await ProcessSingleImageAsync(file, entityType, entityId, category, uploadedByMemberId);
                        results.Add(result);

                        if (result.IsSuccess && result.ImageId.HasValue)
                        {
                            uploadedImageIds.Add(result.ImageId.Value);
                        }
                        else if (!result.IsSuccess)
                        {
                            _logger.LogWarning("圖片處理失敗: {FileName} - {Error}", file.FileName, result.ErrorMessage);
                        }
                    }

                    // 5. 批次分配 DisplayOrder
                    if (uploadedImageIds.Any())
                    {
                        var assignResult = await _displayOrderManager.AssignDisplayOrdersAsync(
                            entityType, 
                            entityId, 
                            category,
                            uploadedImageIds,
                            ConcurrencyControlStrategy.OptimisticLock);
                        
                        if (!assignResult.IsSuccess)
                        {
                            _logger.LogWarning("DisplayOrder 分配失敗: {Error}", assignResult.ErrorMessage);
                        }
                        else
                        {
                            // 更新結果中的 DisplayOrder 和 IsMainImage 資訊
                            UpdateResultsWithDisplayOrder(results, assignResult.AssignedOrders);
                        }
                    }

                    if (transaction != null)
                    {
                        await transaction.CommitAsync();
                    }
                    _logger.LogInformation("成功處理 {Count} 張圖片，實體: {EntityType} ID: {EntityId}", 
                        uploadedImageIds.Count, entityType, entityId);
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync();
                    }
                    _logger.LogError(ex, "批次圖片處理失敗，實體: {EntityType} ID: {EntityId}", entityType, entityId);
                    throw;
                }
                finally
                {
                    transaction?.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳圖片時發生未預期錯誤，實體: {EntityType} ID: {EntityId}", entityType, entityId);
                results.Add(ImageUploadResult.Failure("", $"系統錯誤: {ex.Message}"));
            }

            return results;
        }

        /// <summary>
        /// 單一圖片上傳
        /// </summary>
        public async Task<ImageUploadResult> UploadImageAsync(
            IFormFile file, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category, 
            int? uploadedByMemberId = null)
        {
            var files = new FormFileCollection { file };
            var results = await UploadImagesAsync(files, entityType, entityId, category, uploadedByMemberId);
            return results.FirstOrDefault() ?? ImageUploadResult.Failure(file.FileName, "無法處理圖片");
        }

        /// <summary>
        /// 從串流上傳圖片
        /// </summary>
        public async Task<ImageUploadResult> UploadImageFromStreamAsync(
            Stream imageStream, 
            string originalFileName, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category, 
            int? uploadedByMemberId = null)
        {
            try
            {
                // 實體存在性驗證
                var entityExists = await _entityExistenceChecker.ExistsAsync(entityType, entityId);
                if (!entityExists)
                {
                    return ImageUploadResult.EntityNotFound(originalFileName, entityType, entityId);
                }

                // 處理圖片
                return await ProcessImageFromStreamAsync(imageStream, originalFileName, entityType, entityId, category, uploadedByMemberId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從串流上傳圖片失敗: {FileName}", originalFileName);
                return ImageUploadResult.Failure(originalFileName, $"系統錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 刪除圖片
        /// </summary>
        public async Task<bool> DeleteImageAsync(long imageId, bool hardDelete = false)
        {
            try
            {
                var image = await _context.Images.FindAsync(imageId);
                if (image == null)
                {
                    _logger.LogWarning("嘗試刪除不存在的圖片: {ImageId}", imageId);
                    return false;
                }

                if (hardDelete)
                {
                    _context.Images.Remove(image);
                }
                else
                {
                    image.IsActive = false;
                }

                await _context.SaveChangesAsync();
                
                // 移除後調整 DisplayOrder
                var removeResult = await _displayOrderManager.RemoveImageAndAdjustOrdersAsync(imageId);
                
                _logger.LogInformation("成功刪除圖片: {ImageId} (硬刪除: {HardDelete})", imageId, hardDelete);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除圖片失敗: {ImageId}", imageId);
                return false;
            }
        }

        /// <summary>
        /// 批次刪除實體的所有圖片
        /// </summary>
        public async Task<bool> DeleteImagesByEntityAsync(
            EntityType entityType, 
            int entityId, 
            ImageCategory? category = null, 
            bool hardDelete = false)
        {
            try
            {
                var query = _context.Images.Where(i => 
                    i.EntityType == entityType && 
                    i.EntityId == entityId && 
                    i.IsActive);

                if (category.HasValue)
                {
                    query = query.Where(i => i.Category == category.Value);
                }

                var images = await query.ToListAsync();
                
                if (!images.Any())
                {
                    _logger.LogInformation("沒有找到要刪除的圖片: {EntityType} ID: {EntityId}", entityType, entityId);
                    return true;
                }

                if (hardDelete)
                {
                    _context.Images.RemoveRange(images);
                }
                else
                {
                    foreach (var image in images)
                    {
                        image.IsActive = false;
                    }
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功刪除 {Count} 張圖片: {EntityType} ID: {EntityId} (硬刪除: {HardDelete})", 
                    images.Count, entityType, entityId, hardDelete);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次刪除圖片失敗: {EntityType} ID: {EntityId}", entityType, entityId);
                return false;
            }
        }

        /// <summary>
        /// 設定主圖
        /// </summary>
        public async Task<bool> SetMainImageAsync(long imageId)
        {
            try
            {
                var image = await _context.Images.FindAsync(imageId);
                if (image == null)
                {
                    _logger.LogWarning("嘗試設定不存在的圖片為主圖: {ImageId}", imageId);
                    return false;
                }

                // 將該圖片移動到第一位 (DisplayOrder = 1)
                var moveResult = await _displayOrderManager.MoveImageToPositionAsync(imageId, 1);
                
                if (moveResult.IsSuccess)
                {
                    _logger.LogInformation("成功設定主圖: {ImageId}", imageId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("設定主圖失敗: {ImageId} - {Error}", imageId, moveResult.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定主圖失敗: {ImageId}", imageId);
                return false;
            }
        }

        /// <summary>
        /// 重新排序圖片
        /// </summary>
        public async Task<bool> ReorderImagesAsync(EntityType entityType, int entityId, List<long> imageIds)
        {
            try
            {
                // 重新分配 DisplayOrder
                var assignResult = await _displayOrderManager.AssignDisplayOrdersAsync(
                    entityType, 
                    entityId, 
                    ImageCategory.Gallery, // 使用預設分類
                    imageIds,
                    ConcurrencyControlStrategy.PessimisticLock);
                
                if (assignResult.IsSuccess)
                {
                    _logger.LogInformation("成功重新排序 {Count} 張圖片: {EntityType} ID: {EntityId}", 
                        imageIds.Count, entityType, entityId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("重新排序失敗: {EntityType} ID: {EntityId} - {Error}", 
                        entityType, entityId, assignResult.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新排序圖片失敗: {EntityType} ID: {EntityId}", entityType, entityId);
                return false;
            }
        }

        /// <summary>
        /// 驗證上傳限制
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateUploadLimitAsync(
            EntityType entityType, 
            int entityId, 
            int additionalCount)
        {
            try
            {
                var currentCount = await _context.Images
                    .CountAsync(i => i.EntityType == entityType && i.EntityId == entityId && i.IsActive);

                if (currentCount + additionalCount > MaxImageCount)
                {
                    return (false, $"超過圖片數量限制，目前已有 {currentCount} 張，最多允許 {MaxImageCount} 張");
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證上傳限制失敗: {EntityType} ID: {EntityId}", entityType, entityId);
                return (false, "系統錯誤");
            }
        }

        // === 私有輔助方法 ===

        /// <summary>
        /// 驗證上傳請求
        /// </summary>
        private async Task<List<ImageUploadResult>> ValidateUploadRequestAsync(
            IFormFileCollection files, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category)
        {
            var results = new List<ImageUploadResult>();

            if (files == null || files.Count == 0)
            {
                results.Add(ImageUploadResult.Failure("", "沒有選擇任何檔案"));
                return results;
            }

            if (files.Count > MaxImageCount)
            {
                results.Add(ImageUploadResult.Failure("", $"一次最多只能上傳 {MaxImageCount} 張圖片"));
                return results;
            }

            // 驗證每個檔案
            foreach (var file in files)
            {
                var fileValidation = ValidateFile(file);
                if (fileValidation != null)
                {
                    results.Add(fileValidation);
                }
            }

            return results;
        }

        /// <summary>
        /// 驗證單一檔案
        /// </summary>
        private ImageUploadResult? ValidateFile(IFormFile file)
        {
            var fileName = file.FileName ?? "";

            if (file.Length == 0)
            {
                return ImageUploadResult.ValidationFailure(fileName, "檔案大小為 0");
            }

            if (file.Length > MaxFileSize)
            {
                return ImageUploadResult.ValidationFailure(fileName, $"檔案大小超過 {MaxFileSize / 1024 / 1024}MB 限制");
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return ImageUploadResult.ValidationFailure(fileName, $"不支援的檔案格式，僅支援: {string.Join(", ", AllowedExtensions)}");
            }

            if (!AllowedMimeTypes.Contains(file.ContentType))
            {
                return ImageUploadResult.ValidationFailure(fileName, $"無效的檔案類型: {file.ContentType}");
            }

            // 檔案驗證通過，返回 null 表示沒有錯誤
            return null;
        }

        /// <summary>
        /// 處理單一圖片
        /// </summary>
        private async Task<ImageUploadResult> ProcessSingleImageAsync(
            IFormFile file, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category, 
            int? uploadedByMemberId)
        {
            using var inputStream = file.OpenReadStream();
            return await ProcessImageFromStreamAsync(inputStream, file.FileName, entityType, entityId, category, uploadedByMemberId);
        }

        /// <summary>
        /// 從串流處理圖片
        /// </summary>
        private async Task<ImageUploadResult> ProcessImageFromStreamAsync(
            Stream imageStream, 
            string originalFileName, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category, 
            int? uploadedByMemberId)
        {
            try
            {
                // 生成唯一識別碼
                var imageGuid = Guid.NewGuid();
                var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();
                var storedFileName = $"{imageGuid}{fileExtension}";

                // 使用圖片處理器將圖片轉換為 WebP
                var processingResult = await _imageProcessor.ConvertToWebPAsync(imageStream, 1200, 90);
                if (!processingResult.Success)
                {
                    return ImageUploadResult.Failure(originalFileName, $"圖片處理失敗: {processingResult.ErrorMessage}");
                }

                // 儲存到資料庫
                var image = new Image
                {
                    ImageGuid = imageGuid,
                    EntityType = entityType,
                    EntityId = entityId,
                    Category = category,
                    MimeType = $"image/{processingResult.ProcessedFormat}",
                    OriginalFileName = originalFileName,
                    FileSizeBytes = processingResult.SizeBytes,
                    Width = processingResult.Width,
                    Height = processingResult.Height,
                    DisplayOrder = null, // 稍後批次分配
                    IsActive = true,
                    UploadedByMemberId = uploadedByMemberId,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                return ImageUploadResult.Success(
                    image.ImageId,
                    imageGuid,
                    originalFileName,
                    storedFileName,
                    entityType,
                    entityId,
                    category,
                    processingResult.SizeBytes,
                    processingResult.Width,
                    processingResult.Height,
                    $"image/{processingResult.ProcessedFormat}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理圖片失敗: {FileName}", originalFileName);
                return ImageUploadResult.Failure(originalFileName, $"處理失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新結果中的 DisplayOrder 資訊
        /// </summary>
        private void UpdateResultsWithDisplayOrder(List<ImageUploadResult> results, Dictionary<long, int> assignedOrders)
        {
            foreach (var result in results.Where(r => r.IsSuccess && r.ImageId.HasValue))
            {
                if (assignedOrders.TryGetValue(result.ImageId!.Value, out var displayOrder))
                {
                    result.DisplayOrder = displayOrder;
                    result.IsMainImage = displayOrder == 1; // 第一位為主圖
                }
            }
        }
    }
}