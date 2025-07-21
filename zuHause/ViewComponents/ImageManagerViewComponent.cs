using Microsoft.AspNetCore.Mvc;
using zuHause.DTOs.ImageManager;
using zuHause.Enums;
using zuHause.Interfaces;
using zuHause.Models.ViewModels;

namespace zuHause.ViewComponents
{
    public class ImageManagerViewComponent : ViewComponent
    {
        private readonly IImageQueryService _imageQueryService;
        private readonly ILogger<ImageManagerViewComponent> _logger;

        public ImageManagerViewComponent(
            IImageQueryService imageQueryService,
            ILogger<ImageManagerViewComponent> logger)
        {
            _imageQueryService = imageQueryService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            EntityType entityType,
            int entityId,
            ImageCategory category = ImageCategory.Gallery,
            ImageManagerConfigModel? config = null)
        {
            try
            {
                // 使用提供的配置或根據實體類型選擇預設配置
                config ??= GetDefaultConfigForEntityType(entityType);

                // 建立 ViewModel
                var model = new ImageManagerViewComponentModel
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Category = category,
                    MaxFiles = config.MaxFiles,
                    MaxFileSize = config.MaxFileSize,
                    AllowedFileTypes = config.AllowedFileTypes,
                    AllowMultiple = config.AllowMultiple,
                    EnableDragSort = config.EnableDragSort,
                    EnableBatchOperations = config.EnableBatchOperations,
                    Theme = config.Theme,
                    CustomSettings = config.CustomSettings
                };

                // 如果需要顯示現有圖片，則載入它們
                if (model.ShowExistingImages)
                {
                    try
                    {
                        var existingImages = await _imageQueryService.GetImagesByCategoryAsync(entityType, entityId, category);
                        model.ExistingImages = existingImages.Select(MapToResponseDto).ToList();

                        var mainImage = await _imageQueryService.GetMainImageAsync(entityType, entityId);
                        model.MainImage = mainImage != null ? MapToResponseDto(mainImage) : null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "載入現有圖片時發生錯誤: {EntityType}-{EntityId}", entityType, entityId);
                        // 不阻擋 ViewComponent 載入，只是不顯示現有圖片
                        model.ExistingImages = new List<ImageManagerResponseDto>();
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImageManagerViewComponent 執行時發生錯誤: {EntityType}-{EntityId}", entityType, entityId);
                
                // 回傳最小可用的模型
                var fallbackModel = new ImageManagerViewComponentModel
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Category = category
                };

                return View(fallbackModel);
            }
        }

        private ImageManagerConfigModel GetDefaultConfigForEntityType(EntityType entityType)
        {
            return entityType switch
            {
                EntityType.Property => ImageManagerConfigModel.Property,
                EntityType.Member => ImageManagerConfigModel.Member,
                EntityType.Furniture => ImageManagerConfigModel.Furniture,
                EntityType.Announcement => ImageManagerConfigModel.Default,
                _ => ImageManagerConfigModel.Default
            };
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
    }
}