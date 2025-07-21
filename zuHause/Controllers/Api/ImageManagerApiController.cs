using Microsoft.AspNetCore.Mvc;
using zuHause.DTOs.ImageManager;
using zuHause.Enums;
using zuHause.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace zuHause.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageManagerApiController : ControllerBase
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly IImageQueryService _imageQueryService;
        private readonly IDisplayOrderManager _displayOrderManager;
        private readonly ILogger<ImageManagerApiController> _logger;

        public ImageManagerApiController(
            IImageUploadService imageUploadService,
            IImageQueryService imageQueryService,
            IDisplayOrderManager displayOrderManager,
            ILogger<ImageManagerApiController> logger)
        {
            _imageUploadService = imageUploadService;
            _imageQueryService = imageQueryService;
            _displayOrderManager = displayOrderManager;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponseWrapper<ImageManagerUploadResultDto>>> UploadImages(
            [FromForm] ImageManagerUploadRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponseWrapper<ImageManagerUploadResultDto>.ValidationErrorResponse(errors));
                }

                var result = new ImageManagerUploadResultDto();

                foreach (var file in request.Files)
                {
                    try
                    {
                        var uploadResult = await _imageUploadService.UploadImageAsync(
                            file,
                            request.EntityType,
                            request.EntityId,
                            request.Category,
                            null, // uploadedByMemberId - 演示用途暫不指定
                            request.SkipEntityValidation
                        );

                        if (uploadResult.Success)
                        {
                            var imageDto = MapFromUploadResult(uploadResult);
                            result.SuccessfulUploads.Add(imageDto);
                        }
                        else
                        {
                            result.FailedUploads.Add(new ImageUploadErrorDto
                            {
                                FileName = file.FileName,
                                ErrorMessage = uploadResult.ErrorMessage ?? "上傳失敗",
                                ErrorCode = uploadResult.ErrorCode
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "上傳圖片時發生錯誤: {FileName}", file.FileName);
                        result.FailedUploads.Add(new ImageUploadErrorDto
                        {
                            FileName = file.FileName,
                            ErrorMessage = "系統錯誤，請稍後再試",
                            ErrorCode = "SYSTEM_ERROR"
                        });
                    }
                }

                if (result.SuccessCount > 0)
                {
                    return Ok(ApiResponseWrapper<ImageManagerUploadResultDto>.SuccessResponse(
                        result, 
                        $"成功上傳 {result.SuccessCount} 張圖片" + 
                        (result.FailureCount > 0 ? $"，{result.FailureCount} 張失敗" : "")
                    ));
                }
                else
                {
                    return BadRequest(ApiResponseWrapper<ImageManagerUploadResultDto>.ErrorResponse(
                        "所有圖片上傳失敗", 400, result.FailedUploads.Select(x => x.ErrorMessage).ToList()
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量上傳圖片時發生系統錯誤");
                return StatusCode(500, ApiResponseWrapper<ImageManagerUploadResultDto>.ErrorResponse(
                    "系統錯誤，請稍後再試", 500
                ));
            }
        }

        [HttpGet("list")]
        public async Task<ActionResult<ApiResponseWrapper<ImageManagerListResponseDto>>> GetImages(
            [Required] EntityType entityType,
            [Required] int entityId,
            ImageCategory? category = null)
        {
            try
            {
                var images = category.HasValue 
                    ? await _imageQueryService.GetImagesByCategoryAsync(entityType, entityId, category.Value)
                    : await _imageQueryService.GetImagesByEntityAsync(entityType, entityId);
                var mainImage = await _imageQueryService.GetMainImageAsync(entityType, entityId);

                var response = new ImageManagerListResponseDto
                {
                    Images = images.Select(img => MapToResponseDto(img, mainImage?.ImageId)).ToList(),
                    TotalCount = images.Count,
                    EntityType = entityType,
                    EntityId = entityId,
                    Category = category,
                    MainImage = mainImage != null ? MapToResponseDto(mainImage, mainImage?.ImageId) : null
                };

                return Ok(ApiResponseWrapper<ImageManagerListResponseDto>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢圖片列表時發生錯誤: {EntityType}-{EntityId}", entityType, entityId);
                return StatusCode(500, ApiResponseWrapper<ImageManagerListResponseDto>.ErrorResponse(
                    "查詢圖片列表失敗", 500
                ));
            }
        }

        [HttpDelete("delete/{imageId}")]
        public async Task<ActionResult<ApiResponseWrapper<ImageManagerOperationResultDto>>> DeleteImage(long imageId)
        {
            try
            {
                // 先獲取圖片資訊
                var image = await _imageQueryService.GetImageByIdAsync(imageId);
                if (image == null)
                {
                    return NotFound(ApiResponseWrapper<ImageManagerOperationResultDto>.ErrorResponse(
                        "找不到指定的圖片", 404
                    ));
                }

                // 執行刪除
                var deleteResult = await _imageUploadService.DeleteImageAsync(imageId);
                
                if (deleteResult)
                {
                    // 獲取更新後的圖片列表
                    var updatedImages = await _imageQueryService.GetImagesByCategoryAsync(
                        image.EntityType, image.EntityId, image.Category);
                    var mainImage = await _imageQueryService.GetMainImageAsync(image.EntityType, image.EntityId);

                    var result = new ImageManagerOperationResultDto
                    {
                        Success = true,
                        Message = "圖片刪除成功",
                        AffectedImageIds = new List<long> { imageId },
                        UpdatedImageList = new ImageManagerListResponseDto
                        {
                            Images = updatedImages.Select(img => MapToResponseDto(img, mainImage?.ImageId)).ToList(),
                            TotalCount = updatedImages.Count,
                            EntityType = image.EntityType,
                            EntityId = image.EntityId,
                            Category = image.Category,
                            MainImage = mainImage != null ? MapToResponseDto(mainImage, mainImage?.ImageId) : null
                        }
                    };

                    return Ok(ApiResponseWrapper<ImageManagerOperationResultDto>.SuccessResponse(result));
                }
                else
                {
                    return BadRequest(ApiResponseWrapper<ImageManagerOperationResultDto>.ErrorResponse(
                        "刪除圖片失敗", 400
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除圖片時發生錯誤: {ImageId}", imageId);
                return StatusCode(500, ApiResponseWrapper<ImageManagerOperationResultDto>.ErrorResponse(
                    "刪除圖片失敗", 500
                ));
            }
        }

        [HttpPost("setMain")]
        public async Task<ActionResult<ApiResponseWrapper<ImageManagerOperationResultDto>>> SetMainImage(
            [FromBody] SetMainImageRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponseWrapper<ImageManagerOperationResultDto>.ValidationErrorResponse(errors));
                }

                // 執行設定主圖
                var setMainResult = await _imageUploadService.SetMainImageAsync(request.ImageId);

                if (setMainResult)
                {
                    // 獲取更新後的圖片列表
                    var updatedImages = request.Category.HasValue 
                        ? await _imageQueryService.GetImagesByCategoryAsync(request.EntityType, request.EntityId, request.Category.Value)
                        : await _imageQueryService.GetImagesByEntityAsync(request.EntityType, request.EntityId);
                    var mainImage = await _imageQueryService.GetMainImageAsync(request.EntityType, request.EntityId);

                    var result = new ImageManagerOperationResultDto
                    {
                        Success = true,
                        Message = "主圖設定成功",
                        AffectedImageIds = new List<long> { request.ImageId },
                        UpdatedImageList = new ImageManagerListResponseDto
                        {
                            Images = updatedImages.Select(img => MapToResponseDto(img, mainImage?.ImageId)).ToList(),
                            TotalCount = updatedImages.Count,
                            EntityType = request.EntityType,
                            EntityId = request.EntityId,
                            Category = request.Category,
                            MainImage = mainImage != null ? MapToResponseDto(mainImage, mainImage?.ImageId) : null
                        }
                    };

                    return Ok(ApiResponseWrapper<ImageManagerOperationResultDto>.SuccessResponse(result));
                }
                else
                {
                    return BadRequest(ApiResponseWrapper<ImageManagerOperationResultDto>.ErrorResponse(
                        "設定主圖失敗", 400
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定主圖時發生錯誤: {ImageId}", request.ImageId);
                return StatusCode(500, ApiResponseWrapper<ImageManagerOperationResultDto>.ErrorResponse(
                    "設定主圖失敗", 500
                ));
            }
        }

        [HttpPost("reorder")]
        public async Task<ActionResult<ApiResponseWrapper<ImageManagerOperationResultDto>>> ReorderImages(
            [FromBody] ImageReorderRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponseWrapper<ImageManagerOperationResultDto>.ValidationErrorResponse(errors));
                }

                // 執行重新排序 - 暫時使用簡單成功回應，後續需要實作具體邏輯
                var reorderResult = new { Success = true, Message = "圖片順序調整成功" };

                if (reorderResult.Success)
                {
                    // 獲取更新後的圖片列表
                    var updatedImages = await _imageQueryService.GetImagesByEntityAsync(
                        request.EntityType, request.EntityId);
                    var mainImage = await _imageQueryService.GetMainImageAsync(request.EntityType, request.EntityId);

                    var result = new ImageManagerOperationResultDto
                    {
                        Success = true,
                        Message = "圖片順序調整成功",
                        AffectedImageIds = request.ImageDisplayOrders.Keys.Select(k => (long)k).ToList(),
                        UpdatedImageList = new ImageManagerListResponseDto
                        {
                            Images = updatedImages.Select(img => MapToResponseDto(img, mainImage?.ImageId)).ToList(),
                            TotalCount = updatedImages.Count,
                            EntityType = request.EntityType,
                            EntityId = request.EntityId,
                            MainImage = mainImage != null ? MapToResponseDto(mainImage, mainImage?.ImageId) : null
                        }
                    };

                    return Ok(ApiResponseWrapper<ImageManagerOperationResultDto>.SuccessResponse(result));
                }
                else
                {
                    return BadRequest(ApiResponseWrapper<ImageManagerOperationResultDto>.ErrorResponse(
                        reorderResult.Message ?? "調整圖片順序失敗", 400
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "調整圖片順序時發生錯誤");
                return StatusCode(500, ApiResponseWrapper<ImageManagerOperationResultDto>.ErrorResponse(
                    "調整圖片順序失敗", 500
                ));
            }
        }

        private async Task<ImageManagerResponseDto> MapToResponseDtoAsync(zuHause.Models.Image image)
        {
            // 獲取主圖來判斷是否為主圖
            var mainImage = await _imageQueryService.GetMainImageAsync(image.EntityType, image.EntityId);
            
            return new ImageManagerResponseDto
            {
                ImageId = image.ImageId,
                ImageGuid = image.ImageGuid,
                FileName = image.OriginalFileName,
                StoredFileName = image.StoredFileName,
                MimeType = image.MimeType,
                FileSize = image.FileSizeBytes,
                EntityType = image.EntityType,
                EntityId = image.EntityId,
                Category = image.Category,
                IsMainImage = mainImage?.ImageId == image.ImageId,
                DisplayOrder = image.DisplayOrder ?? 0,
                IsActive = image.IsActive,
                CreatedAt = image.UploadedAt,
                UpdatedAt = null, // 現有模型沒有 UpdatedAt
                OriginalUrl = _imageQueryService.GenerateImageUrl(image.StoredFileName, ImageSize.Original),
                LargeUrl = _imageQueryService.GenerateImageUrl(image.StoredFileName, ImageSize.Large),
                MediumUrl = _imageQueryService.GenerateImageUrl(image.StoredFileName, ImageSize.Medium),
                ThumbnailUrl = _imageQueryService.GenerateImageUrl(image.StoredFileName, ImageSize.Thumbnail)
            };
        }
        
        private ImageManagerResponseDto MapToResponseDto(zuHause.Models.Image image, long? mainImageId = null)
        {
            return new ImageManagerResponseDto
            {
                ImageId = image.ImageId,
                ImageGuid = image.ImageGuid,
                FileName = image.OriginalFileName,
                StoredFileName = image.StoredFileName,
                MimeType = image.MimeType,
                FileSize = image.FileSizeBytes,
                EntityType = image.EntityType,
                EntityId = image.EntityId,
                Category = image.Category,
                IsMainImage = mainImageId.HasValue && image.ImageId == mainImageId.Value,
                DisplayOrder = image.DisplayOrder ?? 0,
                IsActive = image.IsActive,
                CreatedAt = image.UploadedAt,
                UpdatedAt = null, // 現有模型沒有 UpdatedAt
                OriginalUrl = _imageQueryService.GenerateImageUrl(image.StoredFileName, ImageSize.Original),
                LargeUrl = _imageQueryService.GenerateImageUrl(image.StoredFileName, ImageSize.Large),
                MediumUrl = _imageQueryService.GenerateImageUrl(image.StoredFileName, ImageSize.Medium),
                ThumbnailUrl = _imageQueryService.GenerateImageUrl(image.StoredFileName, ImageSize.Thumbnail)
            };
        }

        private ImageManagerResponseDto MapFromUploadResult(zuHause.DTOs.ImageUploadResult uploadResult)
        {
            return new ImageManagerResponseDto
            {
                ImageId = uploadResult.ImageId ?? 0,
                ImageGuid = uploadResult.ImageGuid ?? Guid.Empty,
                FileName = uploadResult.OriginalFileName,
                StoredFileName = uploadResult.StoredFileName,
                MimeType = uploadResult.MimeType,
                FileSize = uploadResult.FileSizeBytes,
                EntityType = uploadResult.EntityType,
                EntityId = uploadResult.EntityId,
                Category = uploadResult.Category,
                IsMainImage = uploadResult.IsMainImage,
                DisplayOrder = uploadResult.DisplayOrder ?? 0,
                IsActive = true,
                CreatedAt = uploadResult.UploadedAt,
                UpdatedAt = null,
                OriginalUrl = _imageQueryService.GenerateImageUrl(uploadResult.StoredFileName, ImageSize.Original),
                LargeUrl = _imageQueryService.GenerateImageUrl(uploadResult.StoredFileName, ImageSize.Large),
                MediumUrl = _imageQueryService.GenerateImageUrl(uploadResult.StoredFileName, ImageSize.Medium),
                ThumbnailUrl = _imageQueryService.GenerateImageUrl(uploadResult.StoredFileName, ImageSize.Thumbnail)
            };
        }
    }

    public class SetMainImageRequestDto
    {
        [Required(ErrorMessage = "圖片 ID 為必填")]
        [Range(1, long.MaxValue, ErrorMessage = "圖片 ID 必須大於 0")]
        public long ImageId { get; set; }

        [Required(ErrorMessage = "實體類型為必填")]
        public EntityType EntityType { get; set; }

        [Required(ErrorMessage = "實體 ID 為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "實體 ID 必須大於 0")]
        public int EntityId { get; set; }

        public ImageCategory? Category { get; set; }
    }

    public class ImageReorderRequestDto
    {
        [Required(ErrorMessage = "實體類型為必填")]
        public EntityType EntityType { get; set; }

        [Required(ErrorMessage = "實體 ID 為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "實體 ID 必須大於 0")]
        public int EntityId { get; set; }

        [Required(ErrorMessage = "圖片順序資訊為必填")]
        [MinLength(1, ErrorMessage = "至少需要一筆圖片順序資訊")]
        public Dictionary<int, int> ImageDisplayOrders { get; set; } = new Dictionary<int, int>();
    }
}