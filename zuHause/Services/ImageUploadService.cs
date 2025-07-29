using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using zuHause.DTOs;
using zuHause.Enums;
using zuHause.Interfaces;
using zuHause.Models;
using Microsoft.Extensions.Hosting;

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
        private readonly IBlobStorageService _blobStorageService;
        private readonly ITempSessionService _tempSessionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageQueryService _imageQueryService;

        // 上傳限制常數
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB
        private const int MaxImageCount = 15;
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "application/pdf" };
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

        public ImageUploadService(
            ZuHauseContext context,
            IImageProcessor imageProcessor,
            IEntityExistenceChecker entityExistenceChecker,
            IDisplayOrderManager displayOrderManager,
            ILogger<ImageUploadService> logger,
            IBlobStorageService blobStorageService,
            ITempSessionService tempSessionService,
            IHttpContextAccessor httpContextAccessor,
            IImageQueryService imageQueryService)
        {
            _context = context;
            _imageProcessor = imageProcessor;
            _entityExistenceChecker = entityExistenceChecker;
            _displayOrderManager = displayOrderManager;
            _logger = logger;
            _blobStorageService = blobStorageService;
            _tempSessionService = tempSessionService;
            _httpContextAccessor = httpContextAccessor;
            _imageQueryService = imageQueryService;
        }

        /// <summary>
        /// 批次上傳圖片
        /// </summary>
        public async Task<List<ImageUploadResult>> UploadImagesAsync(
            IFormFileCollection files, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category, 
            int? uploadedByMemberId = null,
            bool skipEntityValidation = false)
        {
            var results = new List<ImageUploadResult>();

            try
            {
                // 1. 基本驗證
                var validationResult = await ValidateUploadRequestAsync(files, entityType, entityId, category);
                if (validationResult.Any(r => !r.Success))
                {
                    return validationResult;
                }

                // 2. 實體存在性驗證 (可選)
                if (!skipEntityValidation)
                {
                    var entityExists = await _entityExistenceChecker.ExistsAsync(entityType, entityId);
                    if (!entityExists)
                    {
                        results.Add(ImageUploadResult.CreateFailure("", $"{entityType} ID {entityId} 不存在"));
                        return results;
                    }
                }

                // 3. 檢查目前圖片數量
                var currentImageCount = await _context.Images
                    .CountAsync(i => i.EntityType == entityType && i.EntityId == entityId && i.IsActive);

                if (currentImageCount + files.Count > MaxImageCount)
                {
                    results.Add(ImageUploadResult.CreateFailure("", 
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

                        if (result.Success && result.ImageId.HasValue)
                        {
                            uploadedImageIds.Add(result.ImageId.Value);
                        }
                        else if (!result.Success)
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
                results.Add(ImageUploadResult.CreateFailure("", $"系統錯誤: {ex.Message}"));
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
            int? uploadedByMemberId = null,
            bool skipEntityValidation = false)
        {
            var files = new FormFileCollection { file };
            var results = await UploadImagesAsync(files, entityType, entityId, category, uploadedByMemberId, skipEntityValidation);
            return results.FirstOrDefault() ?? ImageUploadResult.CreateFailure(file.FileName, "無法處理圖片");
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
            int? uploadedByMemberId = null,
            bool skipEntityValidation = false)
        {
            try
            {
                // 實體存在性驗證 (可選)
                if (!skipEntityValidation)
                {
                    var entityExists = await _entityExistenceChecker.ExistsAsync(entityType, entityId);
                    if (!entityExists)
                    {
                        return ImageUploadResult.EntityNotFound(originalFileName, entityType, entityId);
                    }
                }

                // 處理圖片
                return await ProcessImageFromStreamAsync(imageStream, originalFileName, entityType, entityId, category, uploadedByMemberId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從串流上傳圖片失敗: {FileName}", originalFileName);
                return ImageUploadResult.CreateFailure(originalFileName, $"系統錯誤: {ex.Message}");
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

                // 記錄是否為主圖
                bool wasMainImage = image.DisplayOrder == 1 && 
                                   image.EntityType == EntityType.Property && 
                                   image.Category == ImageCategory.Gallery;
                int propertyId = image.EntityId;

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
                
                // 如果刪除的是房源主圖，更新 Property.PreviewImageUrl
                if (wasMainImage)
                {
                    await UpdatePropertyPreviewImageAsync(propertyId);
                }
                
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
                    
                    // 如果是房源圖片，自動更新 Property.PreviewImageUrl
                    if (image.EntityType == EntityType.Property)
                    {
                        await UpdatePropertyPreviewImageAsync(image.EntityId);
                    }
                    
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
            const int maxRetryAttempts = 3;
            
            for (int retryCount = 0; retryCount < maxRetryAttempts; retryCount++)
            {
                try
                {
                    if (imageIds == null || !imageIds.Any())
                    {
                        _logger.LogWarning("重新排序失敗: 圖片ID列表為空");
                        return false;
                    }

                    // 驗證所有圖片是否存在並屬於指定實體（每次重試都重新讀取）
                    var images = await _context.Images
                        .Where(i => imageIds.Contains(i.ImageId) && 
                                   i.EntityType == entityType && 
                                   i.EntityId == entityId && 
                                   i.IsActive)
                        .ToListAsync();

                    if (images.Count != imageIds.Count)
                    {
                        var missingIds = imageIds.Except(images.Select(i => i.ImageId)).ToList();
                        _logger.LogWarning("重新排序失敗: 找不到圖片ID: {MissingIds}", string.Join(", ", missingIds));
                        return false;
                    }

                    // 使用併發安全的方式直接設定新的 DisplayOrder
                    // 不依賴 category，直接根據 imageIds 順序設定
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        // 使用 Dictionary 優化查找性能
                        var imageDict = images.ToDictionary(img => img.ImageId, img => img);
                        
                        for (int i = 0; i < imageIds.Count; i++)
                        {
                            if (imageDict.TryGetValue(imageIds[i], out var image))
                            {
                                image.DisplayOrder = i + 1;
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        // 如果是房源圖片重新排序，更新 Property.PreviewImageUrl
                        if (entityType == EntityType.Property)
                        {
                            await UpdatePropertyPreviewImageAsync(entityId);
                        }
                        
                        _logger.LogInformation("成功重新排序 {Count} 張圖片: {EntityType} ID: {EntityId} (重試次數: {RetryCount})", 
                            imageIds.Count, entityType, entityId, retryCount);
                        return true;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "重新排序發生併發衝突: {EntityType} ID: {EntityId} (第 {RetryCount}/{MaxRetries} 次重試)", 
                        entityType, entityId, retryCount + 1, maxRetryAttempts);
                    
                    if (retryCount == maxRetryAttempts - 1)
                    {
                        _logger.LogError("重新排序失敗: 達到最大重試次數 {MaxRetries}: {EntityType} ID: {EntityId}", 
                            maxRetryAttempts, entityType, entityId);
                        return false;
                    }
                    
                    // 短暫延遲後重試
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * (retryCount + 1)));
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "重新排序圖片失敗: {EntityType} ID: {EntityId}", entityType, entityId);
                    return false;
                }
            }
            
            return false;
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

        /// <summary>
        /// 更新房源的預覽圖 URL
        /// </summary>
        private async Task UpdatePropertyPreviewImageAsync(int propertyId)
        {
            try
            {
                var property = await _context.Properties.FindAsync(propertyId);
                if (property == null)
                {
                    _logger.LogWarning("嘗試更新不存在房源的預覽圖: PropertyId={PropertyId}", propertyId);
                    return;
                }

                // 查詢排序為 1 的房源圖片（主圖）
                var mainImage = await _context.Images
                    .Where(img => img.EntityType == EntityType.Property &&
                                  img.EntityId == propertyId &&
                                  img.Category == ImageCategory.Gallery &&
                                  img.DisplayOrder == 1)
                    .FirstOrDefaultAsync();

                if (mainImage != null)
                {
                    // 生成新的預覽圖 URL
                    var previewImageUrl = _imageQueryService.GenerateImageUrl(mainImage.StoredFileName, ImageSize.Medium);
                    
                    _logger.LogInformation("更新房源預覽圖: PropertyId={PropertyId}, ImageId={ImageId}, URL={PreviewUrl}",
                        propertyId, mainImage.ImageId, previewImageUrl);

                    property.PreviewImageUrl = previewImageUrl;
                }
                else
                {
                    _logger.LogInformation("未找到主圖，清空房源預覽圖: PropertyId={PropertyId}", propertyId);
                    property.PreviewImageUrl = null;
                }

                // 保存變更
                await _context.SaveChangesAsync();
                _logger.LogInformation("房源預覽圖已更新: PropertyId={PropertyId}, PreviewImageUrl={PreviewImageUrl}", 
                    propertyId, property.PreviewImageUrl ?? "NULL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新房源預覽圖失敗: PropertyId={PropertyId}", propertyId);
                throw;
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
                results.Add(ImageUploadResult.CreateFailure("", "沒有選擇任何檔案"));
                return results;
            }

            if (files.Count > MaxImageCount)
            {
                results.Add(ImageUploadResult.CreateFailure("", $"一次最多只能上傳 {MaxImageCount} 張圖片"));
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
        /// 從串流處理圖片或文件
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

                // 檢查是否為 PDF 文件
                var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();
                var isPdfFile = fileExtension == ".pdf";

                string processedFormat;
                string mimeType;
                long fileSizeBytes;
                int width = 0;
                int height = 0;
                Stream uploadStream;

                if (isPdfFile)
                {
                    // 對於 PDF 文件，跳過圖片處理
                    processedFormat = "pdf";
                    mimeType = "application/pdf";
                    fileSizeBytes = imageStream.Length;
                    
                    // 確保流位置在開始處
                    if (imageStream.CanSeek)
                    {
                        imageStream.Position = 0;
                    }
                    uploadStream = imageStream;
                    // PDF 文件不需要尺寸資訊
                }
                else
                {
                    // 對於圖片文件，使用圖片處理器將圖片轉換為 WebP
                    var processingResult = await _imageProcessor.ConvertToWebPAsync(imageStream, 1200, 90);
                    if (!processingResult.Success)
                    {
                        return ImageUploadResult.CreateFailure(originalFileName, $"圖片處理失敗: {processingResult.ErrorMessage}");
                    }

                    processedFormat = processingResult.ProcessedFormat.ToLowerInvariant(); // webp
                    mimeType = $"image/{processedFormat}";
                    fileSizeBytes = processingResult.SizeBytes;
                    width = processingResult.Width;
                    height = processingResult.Height;
                    uploadStream = processingResult.ProcessedStream!;
                }

                // === 上傳至 Azure Blob Storage 臨時區域 ===
                try
                {
                    // 獲取臨時會話 ID
                    var httpContext = _httpContextAccessor.HttpContext;
                    if (httpContext == null)
                    {
                        _logger.LogError("無法取得 HttpContext");
                        return ImageUploadResult.CreateFailure(originalFileName, "系統錯誤：無法取得會話資訊");
                    }
                    
                    var tempSessionId = _tempSessionService.GetOrCreateTempSessionId(httpContext);
                    
                    // 建立多尺寸串流字典
                    var streams = new Dictionary<ImageSize, Stream>
                    {
                        { ImageSize.Original, uploadStream }
                    };
                    
                    // 使用 N 格式 (無破折號) 的 GUID
                    var guidNoDash = imageGuid.ToString("N");

                    // 建立基礎路徑：temp/{session}/{category}/{entityId}/{guid}
                    var basePath = $"temp/{tempSessionId}/{category.ToString().ToLowerInvariant()}/{entityId}/{guidNoDash}";
                    
                    // 上傳至臨時區域的多個尺寸
                    var uploadResult = await _blobStorageService.UploadMultipleSizesAsync(
                        streams,
                        basePath,
                        mimeType);
                    
                    if (!uploadResult.Success)
                    {
                        _logger.LogError("上傳至 Azure Blob Storage 失敗: {Error}", uploadResult.Message);
                        return ImageUploadResult.CreateFailure(originalFileName, $"雲端儲存失敗: {uploadResult.Message}");
                    }

                    // 儲存 Blob 路徑作為 StoredFileName（用於後續識別）
                    // StoredFileName 也採用無破折號 GUID
                    var storedFileName = $"temp/{tempSessionId}/{category.ToString().ToLowerInvariant()}/{entityId}/{guidNoDash}.{processedFormat}";

                    // 儲存到資料庫
                    var image = new Image
                    {
                        ImageGuid = imageGuid,
                        EntityType = entityType,
                        EntityId = entityId,
                        Category = category,
                        MimeType = mimeType,
                        OriginalFileName = originalFileName,
                        StoredFileName = storedFileName,
                        FileSizeBytes = fileSizeBytes,
                        Width = width,
                        Height = height,
                        DisplayOrder = null, // 稍後批次分配
                        IsActive = true,
                        UploadedByMemberId = uploadedByMemberId,
                        UploadedAt = DateTime.UtcNow
                    };

                    _context.Images.Add(image);
                    await _context.SaveChangesAsync();

                    return ImageUploadResult.CreateSuccess(
                        image.ImageId,
                        imageGuid,
                        originalFileName,
                        storedFileName,
                        entityType,
                        entityId,
                        category,
                        fileSizeBytes,
                        width,
                        height,
                        mimeType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "上傳圖片至雲端儲存失敗: {FileName}", originalFileName);
                    return ImageUploadResult.CreateFailure(originalFileName, $"雲端儲存失敗: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理圖片失敗: {FileName}", originalFileName);
                return ImageUploadResult.CreateFailure(originalFileName, $"處理失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新結果中的 DisplayOrder 資訊
        /// </summary>
        private void UpdateResultsWithDisplayOrder(List<ImageUploadResult> results, Dictionary<long, int> assignedOrders)
        {
            foreach (var result in results.Where(r => r.Success && r.ImageId.HasValue))
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