using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using zuHause.DTOs;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Services
{
    /// <summary>
    /// 房源圖片處理服務 - 專注於圖片上傳、格式轉換、多尺寸生成和檔案儲存
    /// </summary>
    public class PropertyImageService
    {
        private readonly ZuHauseContext _context;
        private readonly IImageProcessor _imageProcessor;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PropertyImageService> _logger;

        // 檔案驗證常數
        private const int MaxFileSize = 2 * 1024 * 1024; // 2MB
        private const int MaxFileCount = 15;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png" };

        // 圖片尺寸設定
        private const int OriginalMaxWidth = 1200;
        private const int MediumWidth = 800;
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 200;

        public PropertyImageService(
            ZuHauseContext context,
            IImageProcessor imageProcessor,
            IWebHostEnvironment environment,
            ILogger<PropertyImageService> logger)
        {
            _context = context;
            _imageProcessor = imageProcessor;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// 上傳房源圖片並處理為多種尺寸
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="files">上傳的圖片檔案</param>
        /// <returns>處理結果列表</returns>
        public async Task<List<PropertyImageResult>> UploadPropertyImagesAsync(int propertyId, IFormFileCollection files)
        {
            var results = new List<PropertyImageResult>();

            try
            {
                // 1. 基本驗證
                var validationResult = ValidateUploadRequest(propertyId, files);
                if (validationResult.Any(r => !r.Success))
                {
                    return validationResult;
                }

                // 2. 檢查房源是否存在
                var propertyExists = await _context.Properties.AnyAsync(p => p.PropertyId == propertyId);
                if (!propertyExists)
                {
                    results.Add(PropertyImageResult.CreateFailure("", $"房源 ID {propertyId} 不存在"));
                    return results;
                }

                // 3. 檢查目前圖片數量
                var currentImageCount = await _context.PropertyImages
                    .CountAsync(pi => pi.PropertyId == propertyId);

                if (currentImageCount + files.Count > MaxFileCount)
                {
                    results.Add(PropertyImageResult.CreateFailure("", 
                        $"超過圖片數量限制，目前已有 {currentImageCount} 張，最多允許 {MaxFileCount} 張"));
                    return results;
                }

                // 4. 確保上傳目錄存在
                var uploadDir = GetPropertyUploadDirectory(propertyId);
                Directory.CreateDirectory(uploadDir);

                // 5. 取得下一個顯示順序
                var nextDisplayOrder = await GetNextDisplayOrderAsync(propertyId);

                // 6. 處理每個檔案
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        var isMainImage = currentImageCount == 0 && i == 0; // 第一張設為主圖
                        var displayOrder = nextDisplayOrder + i;

                        var result = await ProcessSingleImageAsync(propertyId, file, displayOrder, isMainImage);
                        results.Add(result);

                        if (!result.Success)
                        {
                            _logger.LogWarning("圖片處理失敗: {FileName} - {Error}", file.FileName, result.ErrorMessage);
                        }
                    }

                    // 7. 如果所有圖片都處理成功，提交事務
                    var successResults = results.Where(r => r.Success).ToList();
                    if (successResults.Any())
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("成功處理 {Count} 張圖片，房源ID: {PropertyId}", successResults.Count, propertyId);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("所有圖片處理失敗，回滾事務，房源ID: {PropertyId}", propertyId);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "圖片處理過程發生錯誤，房源ID: {PropertyId}", propertyId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳房源圖片時發生未預期錯誤，房源ID: {PropertyId}", propertyId);
                results.Add(PropertyImageResult.CreateFailure("", $"系統錯誤: {ex.Message}"));
            }

            return results;
        }

        /// <summary>
        /// 驗證上傳請求
        /// </summary>
        private List<PropertyImageResult> ValidateUploadRequest(int propertyId, IFormFileCollection files)
        {
            var results = new List<PropertyImageResult>();

            if (propertyId <= 0)
            {
                results.Add(PropertyImageResult.CreateFailure("", "無效的房源ID"));
                return results;
            }

            if (files == null || files.Count == 0)
            {
                results.Add(PropertyImageResult.CreateFailure("", "沒有選擇任何檔案"));
                return results;
            }

            if (files.Count > MaxFileCount)
            {
                results.Add(PropertyImageResult.CreateFailure("", $"一次最多只能上傳 {MaxFileCount} 張圖片"));
                return results;
            }

            // 驗證每個檔案
            foreach (var file in files)
            {
                var fileValidation = ValidateFile(file);
                if (!fileValidation.Success)
                {
                    results.Add(fileValidation);
                }
            }

            return results;
        }

        /// <summary>
        /// 驗證單一檔案
        /// </summary>
        private PropertyImageResult ValidateFile(IFormFile file)
        {
            var fileName = file.FileName ?? "";

            if (file.Length == 0)
            {
                return PropertyImageResult.CreateFailure(fileName, "檔案大小為 0");
            }

            if (file.Length > MaxFileSize)
            {
                return PropertyImageResult.CreateFailure(fileName, $"檔案大小超過 {MaxFileSize / 1024 / 1024}MB 限制");
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return PropertyImageResult.CreateFailure(fileName, $"不支援的檔案格式，僅支援: {string.Join(", ", AllowedExtensions)}");
            }

            if (!AllowedMimeTypes.Contains(file.ContentType))
            {
                return PropertyImageResult.CreateFailure(fileName, $"無效的檔案類型: {file.ContentType}");
            }

            return PropertyImageResult.CreateSuccess(fileName, 0, "", "", "", false, 0, 0, 0, 0);
        }

        /// <summary>
        /// 處理單一圖片
        /// </summary>
        private async Task<PropertyImageResult> ProcessSingleImageAsync(int propertyId, IFormFile file, int displayOrder, bool isMainImage)
        {
            var fileName = file.FileName ?? "";
            
            try
            {
                using var inputStream = file.OpenReadStream();
                
                // 1. 生成唯一檔案名稱
                var fileId = Guid.NewGuid().ToString();
                var baseFileName = $"{fileId}";

                // 2. 生成三種尺寸的圖片
                var originalResult = await _imageProcessor.ConvertToWebPAsync(inputStream, OriginalMaxWidth, 90);
                if (!originalResult.Success)
                {
                    return PropertyImageResult.CreateFailure(fileName, $"原圖處理失敗: {originalResult.ErrorMessage}");
                }

                inputStream.Position = 0;
                var mediumResult = await _imageProcessor.ConvertToWebPAsync(inputStream, MediumWidth, 85);
                if (!mediumResult.Success)
                {
                    return PropertyImageResult.CreateFailure(fileName, $"中圖處理失敗: {mediumResult.ErrorMessage}");
                }

                inputStream.Position = 0;
                var thumbnailResult = await _imageProcessor.GenerateThumbnailAsync(inputStream, ThumbnailWidth, ThumbnailHeight);
                if (!thumbnailResult.Success)
                {
                    return PropertyImageResult.CreateFailure(fileName, $"縮圖處理失敗: {thumbnailResult.ErrorMessage}");
                }

                // 3. 儲存檔案到檔案系統
                var originalPath = await SaveImageToFileSystemAsync(propertyId, baseFileName + "_original.webp", originalResult.ProcessedStream!);
                var mediumPath = await SaveImageToFileSystemAsync(propertyId, baseFileName + "_medium.webp", mediumResult.ProcessedStream!);
                var thumbnailPath = await SaveImageToFileSystemAsync(propertyId, baseFileName + "_thumbnail.webp", thumbnailResult.ProcessedStream!);

                // 4. 儲存到資料庫
                var propertyImage = new PropertyImage
                {
                    PropertyId = propertyId,
                    ImagePath = originalPath, // 資料庫儲存原圖路徑
                    DisplayOrder = displayOrder,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PropertyImages.Add(propertyImage);
                await _context.SaveChangesAsync();

                // 5. 計算處理後總檔案大小
                var processedSize = originalResult.SizeBytes + mediumResult.SizeBytes + thumbnailResult.SizeBytes;

                return PropertyImageResult.CreateSuccess(
                    fileName,
                    propertyImage.ImageId,
                    originalPath,
                    mediumPath,
                    thumbnailPath,
                    isMainImage,
                    file.Length,
                    processedSize,
                    originalResult.Width,
                    originalResult.Height
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理圖片時發生錯誤: {FileName}", fileName);
                return PropertyImageResult.CreateFailure(fileName, $"處理失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 儲存圖片到檔案系統
        /// </summary>
        private async Task<string> SaveImageToFileSystemAsync(int propertyId, string fileName, Stream imageStream)
        {
            var directory = GetPropertyUploadDirectory(propertyId);
            var filePath = Path.Combine(directory, fileName);
            
            using var fileStream = new FileStream(filePath, FileMode.Create);
            imageStream.Position = 0;
            await imageStream.CopyToAsync(fileStream);
            
            // 返回相對路徑
            return $"/uploads/properties/{propertyId}/{fileName}";
        }

        /// <summary>
        /// 取得房源上傳目錄的完整路徑
        /// </summary>
        private string GetPropertyUploadDirectory(int propertyId)
        {
            return Path.Combine(_environment.WebRootPath, "uploads", "properties", propertyId.ToString());
        }

        /// <summary>
        /// 取得下一個顯示順序
        /// </summary>
        private async Task<int> GetNextDisplayOrderAsync(int propertyId)
        {
            var maxOrder = await _context.PropertyImages
                .Where(pi => pi.PropertyId == propertyId)
                .MaxAsync(pi => (int?)pi.DisplayOrder);
            
            return (maxOrder ?? 0) + 1;
        }
    }
}