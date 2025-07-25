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
        private readonly ILogger<TempImageController> _logger;

        public TempImageController(
            IImageUploadService imageUploadService,
            ITempSessionService tempSessionService,
            IImageQueryService imageQueryService,
            ILogger<TempImageController> logger)
        {
            _imageUploadService = imageUploadService;
            _tempSessionService = tempSessionService;
            _imageQueryService = imageQueryService;
            _logger = logger;
        }

        /// <summary>
        /// 上傳圖片至臨時區域
        /// 用於表單提交前的圖片預處理
        /// </summary>
        /// <param name="files">圖片檔案列表</param>
        /// <param name="category">圖片分類（預設為 property）</param>
        /// <returns>臨時圖片上傳結果</returns>
        [HttpPost("temp-upload")]
        public async Task<IActionResult> UploadTempImages(
            IFormFileCollection files,
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

                // 使用臨時實體 ID（後續會被實際實體 ID 替換）
                const int tempEntityId = 0;

                // 上傳圖片至臨時區域
                var uploadResults = await _imageUploadService.UploadImagesAsync(
                    files,
                    EntityType.Property, // 暫時使用 Property，後續可根據 category 調整
                    tempEntityId,
                    imageCategory,
                    skipEntityValidation: true // 跳過實體驗證，因為是臨時上傳
                );

                // 處理上傳結果
                var successResults = uploadResults.Where(r => r.Success).ToList();
                var failedResults = uploadResults.Where(r => !r.Success).ToList();

                if (!successResults.Any())
                {
                    return BadRequest(new { 
                        Success = false, 
                        Message = "所有圖片上傳失敗", 
                        Errors = failedResults.Select(r => new { 
                            FileName = r.OriginalFileName, 
                            Error = r.ErrorMessage 
                        })
                    });
                }

                // 建構回應資料
                var response = new
                {
                    Success = true,
                    Message = $"成功上傳 {successResults.Count} 張圖片",
                    TempSessionId = tempSessionId,
                    Images = successResults.Select(r => new
                    {
                        ImageId = r.ImageId,
                        ImageGuid = r.ImageGuid,
                        FileName = r.OriginalFileName,
                        StoredFileName = r.StoredFileName,
                        PreviewUrl = _imageQueryService.GenerateImageUrl(r.StoredFileName, ImageSize.Medium),
                        ThumbnailUrl = _imageQueryService.GenerateImageUrl(r.StoredFileName, ImageSize.Thumbnail),
                        FileSizeBytes = r.FileSizeBytes,
                        Width = r.Width,
                        Height = r.Height,
                        DisplayOrder = r.DisplayOrder,
                        IsMainImage = r.IsMainImage
                    }).ToList(),
                    FailedUploads = failedResults.Any() ? failedResults.Select(r => new { 
                        FileName = r.OriginalFileName, 
                        Error = r.ErrorMessage 
                    }).ToList() : null
                };

                _logger.LogInformation("臨時圖片上傳完成: {SuccessCount} 成功, {FailedCount} 失敗, TempSessionId: {TempSessionId}", 
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

    }
}