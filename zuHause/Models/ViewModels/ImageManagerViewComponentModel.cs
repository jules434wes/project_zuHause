using zuHause.Enums;
using zuHause.DTOs.ImageManager;

namespace zuHause.Models.ViewModels
{
    public class ImageManagerViewComponentModel
    {
        public EntityType EntityType { get; set; }
        public int EntityId { get; set; }
        public ImageCategory Category { get; set; } = ImageCategory.Gallery;
        public int MaxFiles { get; set; } = 10;
        public long MaxFileSize { get; set; } = 5 * 1024 * 1024; // 5MB
        public string[] AllowedFileTypes { get; set; } = { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        public bool AllowMultiple { get; set; } = true;
        public bool ShowExistingImages { get; set; } = true;
        public bool EnableDragSort { get; set; } = true;
        public bool EnableBatchOperations { get; set; } = true;
        public string ContainerCssClass { get; set; } = "image-manager";
        public string Theme { get; set; } = "default";

        public List<ImageManagerResponseDto> ExistingImages { get; set; } = new List<ImageManagerResponseDto>();
        public ImageManagerResponseDto? MainImage { get; set; }

        public string ApiUploadUrl { get; set; } = "/api/imagemanager/upload";
        public string ApiDeleteUrl { get; set; } = "/api/imagemanager/delete";
        public string ApiReorderUrl { get; set; } = "/api/imagemanager/reorder";
        public string ApiListUrl { get; set; } = "/api/imagemanager/list";

        public string? CallbackOnUpload { get; set; }
        public string? CallbackOnDelete { get; set; }
        public string? CallbackOnReorder { get; set; }

        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();

        public string GetFileTypesString()
        {
            return string.Join(",", AllowedFileTypes);
        }

        public string GetMaxFileSizeFormatted()
        {
            if (MaxFileSize >= 1024 * 1024)
            {
                return $"{MaxFileSize / (1024 * 1024)}MB";
            }
            else if (MaxFileSize >= 1024)
            {
                return $"{MaxFileSize / 1024}KB";
            }
            return $"{MaxFileSize}B";
        }

        public string GetUniqueId()
        {
            return $"image-manager-{EntityType}-{EntityId}-{Category}";
        }

        public object GetDataAttributes()
        {
            return new
            {
                entityType = EntityType.ToString(),
                entityId = EntityId,
                category = Category.ToString(),
                maxFiles = MaxFiles,
                maxFileSize = MaxFileSize,
                allowedTypes = GetFileTypesString(),
                allowMultiple = AllowMultiple,
                enableDragSort = EnableDragSort,
                enableBatchOps = EnableBatchOperations,
                apiUpload = ApiUploadUrl,
                apiDelete = ApiDeleteUrl,
                apiReorder = ApiReorderUrl,
                apiList = ApiListUrl,
                theme = Theme
            };
        }
    }

    public class ImageManagerConfigModel
    {
        public int MaxFiles { get; set; } = 10;
        public long MaxFileSize { get; set; } = 5 * 1024 * 1024;
        public string[] AllowedFileTypes { get; set; } = { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        public bool AllowMultiple { get; set; } = true;
        public bool EnableDragSort { get; set; } = true;
        public bool EnableBatchOperations { get; set; } = true;
        public string Theme { get; set; } = "default";
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();

        public static ImageManagerConfigModel Default => new ImageManagerConfigModel();

        public static ImageManagerConfigModel Property => new ImageManagerConfigModel
        {
            MaxFiles = 20,
            MaxFileSize = 10 * 1024 * 1024, // 10MB
            AllowedFileTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/heic" }
        };

        public static ImageManagerConfigModel Member => new ImageManagerConfigModel
        {
            MaxFiles = 1,
            MaxFileSize = 2 * 1024 * 1024, // 2MB
            AllowMultiple = false,
            EnableDragSort = false,
            EnableBatchOperations = false
        };

        public static ImageManagerConfigModel Furniture => new ImageManagerConfigModel
        {
            MaxFiles = 15,
            MaxFileSize = 8 * 1024 * 1024, // 8MB
            AllowedFileTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" }
        };
    }
}