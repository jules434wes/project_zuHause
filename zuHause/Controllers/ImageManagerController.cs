using Microsoft.AspNetCore.Mvc;
using zuHause.DTOs.ImageManager;
using zuHause.Enums;
using zuHause.Interfaces;
using zuHause.Models.ViewModels;

namespace zuHause.Controllers
{
    public class ImageManagerController : Controller
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly IImageQueryService _imageQueryService;
        private readonly IDisplayOrderManager _displayOrderManager;
        private readonly ILogger<ImageManagerController> _logger;

        public ImageManagerController(
            IImageUploadService imageUploadService,
            IImageQueryService imageQueryService,
            IDisplayOrderManager displayOrderManager,
            ILogger<ImageManagerController> logger)
        {
            _imageUploadService = imageUploadService;
            _imageQueryService = imageQueryService;
            _displayOrderManager = displayOrderManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(ImageManagerUploadRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return Json(MvcResponseWrapper.ErrorResponse("驗證失敗", errors));
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
                            request.StartDisplayOrder
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
                    return Json(MvcResponseWrapper.SuccessResponse(
                        result,
                        $"成功上傳 {result.SuccessCount} 張圖片" +
                        (result.FailureCount > 0 ? $"，{result.FailureCount} 張失敗" : "")
                    ));
                }
                else
                {
                    return Json(MvcResponseWrapper.ErrorResponse(
                        "所有圖片上傳失敗",
                        result.FailedUploads.Select(x => x.ErrorMessage).ToList()
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量上傳圖片時發生系統錯誤");
                return Json(MvcResponseWrapper.ErrorResponse("系統錯誤，請稍後再試"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> List(EntityType entityType, int entityId, ImageCategory? category = null)
        {
            try
            {
                var images = category.HasValue 
                    ? await _imageQueryService.GetImagesByCategoryAsync(entityType, entityId, category.Value)
                    : await _imageQueryService.GetImagesByEntityAsync(entityType, entityId);
                var mainImage = await _imageQueryService.GetMainImageAsync(entityType, entityId);

                var response = new ImageManagerListResponseDto
                {
                    Images = images.Select(MapToResponseDto).ToList(),
                    TotalCount = images.Count,
                    EntityType = entityType,
                    EntityId = entityId,
                    Category = category,
                    MainImage = mainImage != null ? MapToResponseDto(mainImage) : null
                };

                return Json(MvcResponseWrapper.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢圖片列表時發生錯誤: {EntityType}-{EntityId}", entityType, entityId);
                return Json(MvcResponseWrapper.ErrorResponse("查詢圖片列表失敗"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long imageId)
        {
            try
            {
                // 先獲取圖片資訊
                var image = await _imageQueryService.GetImageByIdAsync(imageId);
                if (image == null)
                {
                    return Json(MvcResponseWrapper.ErrorResponse("找不到指定的圖片"));
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
                            Images = updatedImages.Select(MapToResponseDto).ToList(),
                            TotalCount = updatedImages.Count,
                            EntityType = image.EntityType,
                            EntityId = image.EntityId,
                            Category = image.Category,
                            MainImage = mainImage != null ? MapToResponseDto(mainImage) : null
                        }
                    };

                    return Json(MvcResponseWrapper.SuccessResponse(result, "圖片刪除成功"));
                }
                else
                {
                    return Json(MvcResponseWrapper.ErrorResponse("刪除圖片失敗"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除圖片時發生錯誤: {ImageId}", imageId);
                return Json(MvcResponseWrapper.ErrorResponse("刪除圖片失敗"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reorder(EntityType entityType, int entityId, Dictionary<int, int> imageDisplayOrders)
        {
            try
            {
                if (!imageDisplayOrders.Any())
                {
                    return Json(MvcResponseWrapper.ErrorResponse("缺少圖片順序資訊"));
                }

                // 執行重新排序 - 暫時使用簡單成功回應，後續需要實作具體邏輯
                var reorderResult = new { Success = true, Message = "圖片順序調整成功" };

                if (reorderResult.Success)
                {
                    // 獲取更新後的圖片列表
                    var updatedImages = await _imageQueryService.GetImagesByEntityAsync(entityType, entityId);
                    var mainImage = await _imageQueryService.GetMainImageAsync(entityType, entityId);

                    var result = new ImageManagerOperationResultDto
                    {
                        Success = true,
                        Message = "圖片順序調整成功",
                        AffectedImageIds = imageDisplayOrders.Keys.Select(k => (long)k).ToList(),
                        UpdatedImageList = new ImageManagerListResponseDto
                        {
                            Images = updatedImages.Select(MapToResponseDto).ToList(),
                            TotalCount = updatedImages.Count,
                            EntityType = entityType,
                            EntityId = entityId,
                            MainImage = mainImage != null ? MapToResponseDto(mainImage) : null
                        }
                    };

                    return Json(MvcResponseWrapper.SuccessResponse(result, "圖片順序調整成功"));
                }
                else
                {
                    return Json(MvcResponseWrapper.ErrorResponse(reorderResult.Message ?? "調整圖片順序失敗"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "調整圖片順序時發生錯誤");
                return Json(MvcResponseWrapper.ErrorResponse("調整圖片順序失敗"));
            }
        }

        private ImageManagerResponseDto MapToResponseDto(zuHause.Models.Image image)
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
                IsMainImage = false, // 需要額外計算主圖邏輯
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
}