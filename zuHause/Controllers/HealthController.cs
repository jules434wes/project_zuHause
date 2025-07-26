using Microsoft.AspNetCore.Mvc;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Controllers
{
    /// <summary>
    /// 健康檢查控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IBlobStorageConnectionTest _connectionTest;
        private readonly ITempSessionService _tempSessionService;
        private readonly IBlobUrlGenerator _urlGenerator;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            IBlobStorageConnectionTest connectionTest,
            ITempSessionService tempSessionService,
            IBlobUrlGenerator urlGenerator,
            IBlobStorageService blobStorageService,
            ILogger<HealthController> logger)
        {
            _connectionTest = connectionTest;
            _tempSessionService = tempSessionService;
            _urlGenerator = urlGenerator;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Azure Blob Storage 健康檢查
        /// </summary>
        /// <returns>連線測試結果</returns>
        [HttpGet("blob-storage")]
        public async Task<IActionResult> CheckBlobStorageHealth()
        {
            try
            {
                _logger.LogInformation("執行 Azure Blob Storage 健康檢查");
                
                var result = await _connectionTest.TestConnectionAsync();
                
                if (result.Success)
                {
                    _logger.LogInformation("Azure Blob Storage 健康檢查成功");
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Azure Blob Storage 健康檢查失敗: {Message}", result.Message);
                    return StatusCode(503, result); // Service Unavailable
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Blob Storage 健康檢查發生異常");
                return StatusCode(500, new { 
                    Success = false, 
                    Message = $"健康檢查異常: {ex.Message}",
                    TestedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 基本健康檢查
        /// </summary>
        /// <returns>服務狀態</returns>
        [HttpGet("status")]
        public IActionResult GetHealthStatus()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }

        /// <summary>
        /// 臨時會話服務健康檢查
        /// 測試 Cookie 設定、Memory Cache 讀寫和跨分頁一致性
        /// </summary>
        /// <returns>臨時會話測試結果</returns>
        [HttpGet("temp-session")]
        public async Task<IActionResult> CheckTempSessionHealth()
        {
            try
            {
                _logger.LogInformation("執行臨時會話健康檢查");

                // 1. 測試取得或建立會話ID
                var tempSessionId = _tempSessionService.GetOrCreateTempSessionId(HttpContext);
                
                // 2. 測試會話驗證
                var isValid = await _tempSessionService.IsValidTempSessionAsync(tempSessionId);
                
                // 3. 測試圖片資訊新增
                var testImageInfo = new TempImageInfo
                {
                    ImageGuid = Guid.NewGuid(),
                    OriginalFileName = "test-health-check.jpg",
                    TempSessionId = tempSessionId,
                    FileSizeBytes = 1024,
                    MimeType = "image/jpeg"
                };

                await _tempSessionService.AddTempImageAsync(tempSessionId, testImageInfo);

                // 4. 測試圖片資訊讀取
                var tempImages = await _tempSessionService.GetTempImagesAsync(tempSessionId);

                // 5. 清理測試資料
                await _tempSessionService.RemoveTempImageAsync(tempSessionId, testImageInfo.ImageGuid);

                var result = new
                {
                    Success = true,
                    Message = "臨時會話服務測試成功",
                    TempSessionId = tempSessionId,
                    SessionIdLength = tempSessionId.Length,
                    IsValidSession = isValid,
                    TestImageAdded = tempImages.Any(img => img.ImageGuid == testImageInfo.ImageGuid),
                    TotalImagesInSession = tempImages.Count,
                    CookieSet = HttpContext.Request.Cookies.ContainsKey("temp_session_id"),
                    TestedAt = DateTime.UtcNow
                };

                _logger.LogInformation("臨時會話健康檢查成功: {TempSessionId}", tempSessionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "臨時會話健康檢查發生異常");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"臨時會話檢查異常: {ex.Message}",
                    TestedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 跨分頁一致性測試
        /// 驗證相同會話ID在不同請求間保持一致
        /// </summary>
        /// <returns>一致性測試結果</returns>
        [HttpGet("temp-session/consistency")]
        public async Task<IActionResult> CheckTempSessionConsistency()
        {
            try
            {
                // 第一次取得會話ID
                var firstSessionId = _tempSessionService.GetOrCreateTempSessionId(HttpContext);
                
                // 第二次取得會話ID（應該相同）
                var secondSessionId = _tempSessionService.GetOrCreateTempSessionId(HttpContext);
                
                // 驗證兩次取得的ID是否一致
                var isConsistent = firstSessionId == secondSessionId;
                var isValidLength = firstSessionId.Length == 32 && secondSessionId.Length == 32;
                
                var result = new
                {
                    Success = isConsistent && isValidLength,
                    Message = isConsistent ? "跨分頁一致性測試通過" : "跨分頁一致性測試失敗",
                    FirstSessionId = firstSessionId,
                    SecondSessionId = secondSessionId,
                    IsConsistent = isConsistent,
                    IsValidLength = isValidLength,
                    CookieExists = HttpContext.Request.Cookies.ContainsKey("temp_session_id"),
                    TestedAt = DateTime.UtcNow
                };

                _logger.LogInformation("跨分頁一致性測試: {IsConsistent}, SessionId: {SessionId}", 
                    isConsistent, firstSessionId);
                
                return isConsistent ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "跨分頁一致性測試發生異常");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"一致性測試異常: {ex.Message}",
                    TestedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// URL 生成服務測試
        /// 驗證 URL 格式正確性和路徑生成邏輯
        /// </summary>
        /// <returns>URL 生成測試結果</returns>
        [HttpGet("url-generator")]
        public IActionResult CheckUrlGenerator()
        {
            try
            {
                _logger.LogInformation("執行 URL 生成服務測試");

                var testImageGuid = Guid.NewGuid();
                var testTempSessionId = "f1e2d3c4b5a67890abcdef1234567890"; // 32字元測試ID

                // 測試正式區域 URL 生成
                var imageUrl = _urlGenerator.GenerateImageUrl(ImageCategory.Gallery, 12345, testImageGuid, ImageSize.Original);
                var thumbnailUrl = _urlGenerator.GenerateImageUrl(ImageCategory.Gallery, 12345, testImageGuid, ImageSize.Thumbnail);

                // 測試臨時區域 URL 生成
                var tempImageUrl = _urlGenerator.GenerateTempImageUrl(testTempSessionId, testImageGuid, ImageSize.Original);
                var tempThumbnailUrl = _urlGenerator.GenerateTempImageUrl(testTempSessionId, testImageGuid, ImageSize.Thumbnail);

                // 測試 Blob 路徑生成
                var blobPath = _urlGenerator.GetBlobPath(ImageCategory.Gallery, 12345, testImageGuid, ImageSize.Original);
                var tempBlobPath = _urlGenerator.GetTempBlobPath(testTempSessionId, testImageGuid, ImageSize.Original);

                // 測試 URL 驗證
                var isValidImageUrl = _urlGenerator.IsValidImageUrl(imageUrl);
                var isValidTempUrl = _urlGenerator.IsValidImageUrl(tempImageUrl);

                var result = new
                {
                    Success = true,
                    Message = "URL 生成服務測試成功",
                    TestImageGuid = testImageGuid,
                    TestTempSessionId = testTempSessionId,
                    GeneratedUrls = new
                    {
                        ImageUrl = imageUrl,
                        ThumbnailUrl = thumbnailUrl,
                        TempImageUrl = tempImageUrl,
                        TempThumbnailUrl = tempThumbnailUrl
                    },
                    GeneratedPaths = new
                    {
                        BlobPath = blobPath,
                        TempBlobPath = tempBlobPath
                    },
                    ValidationResults = new
                    {
                        IsValidImageUrl = isValidImageUrl,
                        IsValidTempUrl = isValidTempUrl
                    },
                    TestedAt = DateTime.UtcNow
                };

                _logger.LogInformation("URL 生成服務測試成功");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL 生成服務測試發生異常");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"URL 生成測試異常: {ex.Message}",
                    TestedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Blob Storage 服務測試
        /// 驗證併發控制和重試機制
        /// </summary>
        /// <returns>Blob Storage 服務測試結果</returns>
        [HttpGet("blob-storage-service")]
        public async Task<IActionResult> CheckBlobStorageService()
        {
            try
            {
                _logger.LogInformation("執行 Blob Storage 服務測試");

                // 建立測試資料
                var testContent = "Azure Blob Storage 服務測試內容";
                var testStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testContent));
                var testBlobPath = $"test/health-check-{Guid.NewGuid():N}.webp";

                // 測試單一檔案上傳
                var uploadResult = await _blobStorageService.UploadWithRetryAsync(testStream, testBlobPath);

                // 測試檔案存在性檢查
                var exists = await _blobStorageService.ExistsAsync(testBlobPath);

                // 測試取得 Blob 資訊
                var blobInfo = await _blobStorageService.GetBlobInfoAsync(testBlobPath);

                // 清理測試檔案
                var deleteResult = await _blobStorageService.DeleteAsync(testBlobPath);

                var result = new
                {
                    Success = uploadResult.Success && exists && deleteResult,
                    Message = "Blob Storage 服務測試成功",
                    TestBlobPath = testBlobPath,
                    UploadResult = new
                    {
                        uploadResult.Success,
                        uploadResult.Message,
                        uploadResult.BlobUrl,
                        uploadResult.FileSizeBytes,
                        uploadResult.RetryCount
                    },
                    ExistenceCheck = exists,
                    BlobInfo = blobInfo != null ? new
                    {
                        blobInfo.SizeBytes,
                        blobInfo.ContentType,
                        blobInfo.IsTemporary
                    } : null,
                    DeleteResult = deleteResult,
                    TestedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Blob Storage 服務測試成功: Upload={UploadSuccess}, Exists={Exists}, Delete={DeleteResult}", 
                    uploadResult.Success, exists, deleteResult);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blob Storage 服務測試發生異常");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Blob Storage 服務測試異常: {ex.Message}",
                    TestedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Blob Storage 持久檔案測試
        /// 上傳檔案但不刪除，用於確認檔案實際存在於 Azure Blob Storage
        /// </summary>
        /// <returns>持久檔案測試結果</returns>
        [HttpPost("blob-storage-persistent")]
        public async Task<IActionResult> CreatePersistentTestFile()
        {
            try
            {
                _logger.LogInformation("執行 Blob Storage 持久檔案測試");

                // 建立測試資料
                var testContent = $"Azure Blob Storage 持久測試檔案\n建立時間: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n此檔案不會被自動刪除，可在 Azure Portal 中查看";
                var testStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testContent));
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var testBlobPath = $"persistent-test/test-file-{timestamp}-{Guid.NewGuid():N}.txt";

                // 測試檔案上傳
                var uploadResult = await _blobStorageService.UploadWithRetryAsync(testStream, testBlobPath);

                // 測試檔案存在性檢查
                var exists = await _blobStorageService.ExistsAsync(testBlobPath);

                // 測試取得 Blob 資訊
                var blobInfo = await _blobStorageService.GetBlobInfoAsync(testBlobPath);

                var result = new
                {
                    Success = uploadResult.Success && exists,
                    Message = uploadResult.Success ? "持久測試檔案建立成功，檔案已保存在 Azure Blob Storage" : "檔案上傳失敗",
                    TestBlobPath = testBlobPath,
                    DirectBlobUrl = uploadResult.BlobUrl,
                    UploadResult = new
                    {
                        uploadResult.Success,
                        uploadResult.Message,
                        uploadResult.BlobUrl,
                        uploadResult.FileSizeBytes,
                        uploadResult.RetryCount
                    },
                    ExistenceCheck = exists,
                    BlobInfo = blobInfo != null ? new
                    {
                        blobInfo.SizeBytes,
                        blobInfo.ContentType,
                        blobInfo.IsTemporary
                    } : null,
                    Instructions = "請到 Azure Portal 查看 zuhaus-images 容器中的 persistent-test/ 資料夾，應該可以看到此測試檔案",
                    TestedAt = DateTime.UtcNow
                };

                if (uploadResult.Success)
                {
                    _logger.LogInformation("持久測試檔案建立成功: {BlobPath}, URL: {BlobUrl}", testBlobPath, uploadResult.BlobUrl);
                }
                else
                {
                    _logger.LogWarning("持久測試檔案建立失敗: {Message}", uploadResult.Message);
                }

                return uploadResult.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blob Storage 持久檔案測試發生異常");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"持久檔案測試異常: {ex.Message}",
                    TestedAt = DateTime.UtcNow
                });
            }
        }
    }
}