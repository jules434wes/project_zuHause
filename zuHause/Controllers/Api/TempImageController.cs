using Microsoft.AspNetCore.Mvc;
using zuHause.Interfaces;
using zuHause.Enums;
using zuHause.Models;

namespace zuHause.Controllers.Api
{
    /// <summary>
    /// 臨時圖片上傳 API Controller
    /// 提供表單合併提交前的圖片預上傳功能
    /// </summary>
    [ApiController]
    [Route("api/images")]
    public class TempImageController : ControllerBase
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly ITempSessionService _tempSessionService;
        private readonly IImageQueryService _imageQueryService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IImageProcessor _imageProcessor;
        private readonly IBlobUrlGenerator _urlGenerator;
        private readonly ILogger<TempImageController> _logger;

        public TempImageController(
            IImageUploadService imageUploadService,
            ITempSessionService tempSessionService,
            IImageQueryService imageQueryService,
            IBlobStorageService blobStorageService,
            IImageProcessor imageProcessor,
            IBlobUrlGenerator urlGenerator,
            ILogger<TempImageController> logger)
        {
            _imageUploadService = imageUploadService;
            _tempSessionService = tempSessionService;
            _imageQueryService = imageQueryService;
            _blobStorageService = blobStorageService;
            _imageProcessor = imageProcessor;
            _urlGenerator = urlGenerator;
            _logger = logger;
        }

        /// <summary>
        /// 上傳圖片至臨時區域
        /// 用於表單提交前的圖片預處理
        /// 重要：第一階段只上傳到 blob storage，不寫入資料庫
        /// </summary>
        /// <param name="files">圖片檔案列表</param>
        /// <param name="category">圖片分類（預設為 property）</param>
        /// <returns>臨時圖片上傳結果</returns>
        [HttpPost("temp-upload")]
        public async Task<IActionResult> UploadTempImages(
            [FromForm] IFormFileCollection files,
            [FromForm] string category = "property")
        {
            try
            {
                // 驗證參數
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { 
                        Success = false, 
                        Message = "沒有選擇任何檔案" 
                    });
                }

                // 解析圖片分類
                if (!Enum.TryParse<ImageCategory>(category, true, out var imageCategory))
                {
                    return BadRequest(new { 
                        Success = false, 
                        Message = $"無效的圖片分類: {category}" 
                    });
                }

                // 取得臨時會話 ID
                var tempSessionId = _tempSessionService.GetOrCreateTempSessionId(HttpContext);

                _logger.LogInformation("開始第一階段臨時上傳: TempSessionId={TempSessionId}, Category={Category}, FileCount={FileCount}", 
                    tempSessionId, imageCategory, files.Count);

                var successResults = new List<dynamic>();
                var failedResults = new List<dynamic>();

                foreach (var file in files)
                {
                    try
                    {
                        // 驗證檔案
                        var validationResult = await ValidateFileAsync(file);
                        if (!validationResult.IsValid)
                        {
                            failedResults.Add(new { 
                                FileName = file.FileName, 
                                Error = validationResult.ErrorMessage 
                            });
                            continue;
                        }

                        // 產生圖片 GUID
                        var imageGuid = Guid.NewGuid();
                        
                        // 處理圖片並上傳到 blob storage
                        var uploadResult = await ProcessAndUploadToBlobOnlyAsync(file, tempSessionId, imageGuid, imageCategory);
                        
                        if (uploadResult.Success)
                        {
                            successResults.Add(new
                            {
                                ImageGuid = imageGuid,
                                FileName = file.FileName,
                                StoredFileName = uploadResult.StoredFileName,
                                PreviewUrl = uploadResult.PreviewUrl,
                                ThumbnailUrl = uploadResult.ThumbnailUrl,
                                FileSizeBytes = uploadResult.FileSizeBytes,
                                Width = uploadResult.Width,
                                Height = uploadResult.Height,
                                DisplayOrder = successResults.Count + 1
                            });

                            // 記錄到臨時會話（僅記錄檔案信息，不寫資料庫）
                            await _tempSessionService.AddTempImageAsync(tempSessionId, new TempImageInfo
                            {
                                ImageGuid = imageGuid,
                                OriginalFileName = file.FileName,
                                TempSessionId = tempSessionId,
                                Category = imageCategory,
                                FileSizeBytes = uploadResult.FileSizeBytes,
                                MimeType = file.ContentType,
                                UploadedAt = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            failedResults.Add(new { 
                                FileName = file.FileName, 
                                Error = uploadResult.ErrorMessage 
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "處理檔案 {FileName} 時發生錯誤", file.FileName);
                        failedResults.Add(new { 
                            FileName = file.FileName, 
                            Error = $"檔案處理錯誤: {ex.Message}" 
                        });
                    }
                }

                if (!successResults.Any())
                {
                    return BadRequest(new { 
                        Success = false, 
                        Message = "所有圖片上傳失敗", 
                        Errors = failedResults
                    });
                }

                // 建構回應資料
                var response = new
                {
                    Success = true,
                    Message = $"成功上傳 {successResults.Count} 張圖片到臨時區域",
                    TempSessionId = tempSessionId,
                    Images = successResults,
                    FailedUploads = failedResults.Any() ? failedResults : null
                };

                _logger.LogInformation("第一階段臨時上傳完成: {SuccessCount} 成功, {FailedCount} 失敗, TempSessionId: {TempSessionId}", 
                    successResults.Count, failedResults.Count, tempSessionId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "臨時圖片上傳時發生錯誤");
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "系統錯誤，請稍後重試" 
                });
            }
        }

        /// <summary>
        /// 刪除臨時圖片
        /// </summary>
        /// <param name="imageId">圖片 ID</param>
        /// <returns>刪除結果</returns>
        [HttpDelete("temp/{imageId:long}")]
        public async Task<IActionResult> DeleteTempImage(long imageId)
        {
            try
            {
                var result = await _imageUploadService.DeleteImageAsync(imageId, hardDelete: true);
                
                if (result)
                {
                    return Ok(new { 
                        Success = true, 
                        Message = "圖片刪除成功" 
                    });
                }
                else
                {
                    return NotFound(new { 
                        Success = false, 
                        Message = "找不到指定的圖片" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除臨時圖片時發生錯誤: ImageId {ImageId}", imageId);
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "系統錯誤，請稍後重試" 
                });
            }
        }

        /// <summary>
        /// 驗證檔案的格式和大小
        /// </summary>
        private Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file)
        {
            // 檔案大小限制
            const int maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
            {
                return Task.FromResult((false, $"檔案 {file.FileName} 大小超過 10MB 限制"));
            }

            // 檔案類型檢查
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Task.FromResult((false, $"檔案 {file.FileName} 格式不支援，僅支援 JPG, PNG, WebP, PDF"));
            }

            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp", "application/pdf" };
            if (!allowedMimeTypes.Contains(file.ContentType))
            {
                return Task.FromResult((false, $"檔案 {file.FileName} MIME 類型不支援"));
            }

            return Task.FromResult((true, string.Empty));
        }

        /// <summary>
        /// 處理檔案並只上傳到 blob storage（不寫資料庫）
        /// </summary>
        private async Task<TempUploadResult> ProcessAndUploadToBlobOnlyAsync(
            IFormFile file, 
            string tempSessionId, 
            Guid imageGuid, 
            ImageCategory category)
        {
            try
            {
                using var stream = file.OpenReadStream();
                
                // 檔案大小
                var fileSizeBytes = file.Length;
                var width = 0;
                var height = 0;

                // 如果是圖片，處理並獲取尺寸
                if (file.ContentType.StartsWith("image/"))
                {
                    var processingResult = await _imageProcessor.ProcessMultipleSizesAsync(
                        stream, 
                        imageGuid,
                        file.FileName);

                    if (!processingResult.Success)
                    {
                        return TempUploadResult.CreateFailure($"圖片處理失敗: {processingResult.ErrorMessage}");
                    }

                    // 由於沒有直接的尺寸信息，設為預設值或從第一個串流推斷
                    width = 0; // 暫時設為 0，可以後續從 blob metadata 取得
                    height = 0;

                    // 上傳各種尺寸到臨時區域
                    var uploadTasks = new List<Task<BlobUploadResult>>();
                    
                    foreach (var sizeResult in processingResult.ProcessedStreams)
                    {
                        _logger.LogInformation("🔍 [PATH_DEBUG] 即將生成臨時路徑: TempSessionId={TempSessionId}, ImageGuid={ImageGuid}, Size={Size}", 
                            tempSessionId, imageGuid, sizeResult.Key);
                            
                        var tempPath = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, sizeResult.Key);
                        
                        _logger.LogInformation("🔍 [PATH_DEBUG] 生成的臨時路徑: {TempPath}", tempPath);
                        
                        var uploadTask = _blobStorageService.UploadWithRetryAsync(
                            sizeResult.Value, 
                            tempPath, 
                            "image/webp");
                        uploadTasks.Add(uploadTask);
                    }

                    var uploadResults = await Task.WhenAll(uploadTasks);
                    
                    // 檢查是否所有尺寸都上傳成功
                    var failedUploads = uploadResults.Where(r => !r.Success).ToList();
                    if (failedUploads.Any())
                    {
                        var errorMessages = string.Join(", ", failedUploads.Select(f => f.Message));
                        return TempUploadResult.CreateFailure($"Blob 上傳失敗: {errorMessages}");
                    }

                    // 成功，生成 URL
                    _logger.LogInformation("🔍 [PATH_DEBUG] 即將生成預覽URL路徑: TempSessionId={TempSessionId}, ImageGuid={ImageGuid}", 
                        tempSessionId, imageGuid);
                        
                    var originalBlobPath = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, ImageSize.Original);
                    var previewUrl = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, ImageSize.Medium);
                    var thumbnailUrl = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, ImageSize.Thumbnail);
                    
                    _logger.LogInformation("🔍 [PATH_DEBUG] 生成的預覽URL路徑: Original={Original}, Preview={Preview}, Thumbnail={Thumbnail}", 
                        originalBlobPath, previewUrl, thumbnailUrl);

                    return TempUploadResult.CreateSuccess(
                        storedFileName: $"TEMP_{imageGuid}",
                        previewUrl: previewUrl,
                        thumbnailUrl: thumbnailUrl,
                        fileSizeBytes: fileSizeBytes,
                        width: width,
                        height: height);
                }
                else
                {
                    // PDF 檔案直接上傳
                    stream.Position = 0;
                    
                    _logger.LogInformation("🔍 [PATH_DEBUG] 即將生成PDF臨時路徑: TempSessionId={TempSessionId}, ImageGuid={ImageGuid}, Size=Original", 
                        tempSessionId, imageGuid);
                        
                    var tempPath = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, ImageSize.Original);
                    
                    _logger.LogInformation("🔍 [PATH_DEBUG] 生成的PDF臨時路徑: {TempPath}", tempPath);
                    
                    var uploadResult = await _blobStorageService.UploadWithRetryAsync(
                        stream, 
                        tempPath, 
                        file.ContentType);

                    if (!uploadResult.Success)
                    {
                        return TempUploadResult.CreateFailure($"PDF 上傳失敗: {uploadResult.Message}");
                    }

                    return TempUploadResult.CreateSuccess(
                        storedFileName: $"TEMP_{imageGuid}",
                        previewUrl: tempPath,
                        thumbnailUrl: tempPath,
                        fileSizeBytes: fileSizeBytes,
                        width: 0,
                        height: 0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理並上傳檔案時發生錯誤");
                return TempUploadResult.CreateFailure($"處理錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 臨時上傳結果模型
        /// </summary>
        private class TempUploadResult
        {
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
            public string? StoredFileName { get; set; }
            public string? PreviewUrl { get; set; }
            public string? ThumbnailUrl { get; set; }
            public long FileSizeBytes { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public static TempUploadResult CreateSuccess(
                string storedFileName, 
                string previewUrl, 
                string thumbnailUrl, 
                long fileSizeBytes, 
                int width, 
                int height)
            {
                return new TempUploadResult
                {
                    Success = true,
                    StoredFileName = storedFileName,
                    PreviewUrl = previewUrl,
                    ThumbnailUrl = thumbnailUrl,
                    FileSizeBytes = fileSizeBytes,
                    Width = width,
                    Height = height
                };
            }

            public static TempUploadResult CreateFailure(string errorMessage)
            {
                return new TempUploadResult
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }
        }

    }
}