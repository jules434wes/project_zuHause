using zuHause.Enums;
using zuHause.Interfaces;

namespace zuHause.Options
{
    /// <summary>
    /// 圖片上傳相關配置選項
    /// </summary>
    public class ImageUploadOptions
    {
        /// <summary>
        /// 配置節點名稱
        /// </summary>
        public const string SectionName = "ImageUpload";

        /// <summary>
        /// 預設並發控制策略
        /// </summary>
        public ConcurrencyControlStrategy DefaultConcurrencyStrategy { get; set; } = ConcurrencyControlStrategy.OptimisticLock;

        /// <summary>
        /// 最大重試次數
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// 重試延遲時間（毫秒）
        /// </summary>
        public int RetryDelayMs { get; set; } = 100;

        /// <summary>
        /// 允許的 MIME 類型
        /// </summary>
        public List<string> AllowedMimeTypes { get; set; } = new()
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        /// <summary>
        /// 允許的副檔名
        /// </summary>
        public List<string> AllowedExtensions { get; set; } = new()
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        /// <summary>
        /// 各分類的檔案大小限制（位元組）
        /// </summary>
        public Dictionary<ImageCategory, long> MaxFileSizes { get; set; } = new()
        {
            { ImageCategory.Avatar, 2 * 1024 * 1024 },     // 2MB
            { ImageCategory.Gallery, 10 * 1024 * 1024 },   // 10MB
            { ImageCategory.BedRoom, 10 * 1024 * 1024 },   // 10MB
            { ImageCategory.Living, 10 * 1024 * 1024 },    // 10MB
            { ImageCategory.Kitchen, 10 * 1024 * 1024 },   // 10MB
            { ImageCategory.Balcony, 10 * 1024 * 1024 },   // 10MB
            { ImageCategory.Product, 5 * 1024 * 1024 }     // 5MB
        };

        /// <summary>
        /// 各分類的圖片尺寸限制
        /// </summary>
        public Dictionary<ImageCategory, ImageDimensionLimit> DimensionLimits { get; set; } = new()
        {
            { ImageCategory.Avatar, new ImageDimensionLimit(100, 100, 1000, 1000) },
            { ImageCategory.Gallery, new ImageDimensionLimit(400, 300, 4000, 3000) },
            { ImageCategory.BedRoom, new ImageDimensionLimit(400, 300, 4000, 3000) },
            { ImageCategory.Living, new ImageDimensionLimit(400, 300, 4000, 3000) },
            { ImageCategory.Kitchen, new ImageDimensionLimit(400, 300, 4000, 3000) },
            { ImageCategory.Balcony, new ImageDimensionLimit(400, 300, 4000, 3000) },
            { ImageCategory.Product, new ImageDimensionLimit(300, 300, 2000, 2000) }
        };

        /// <summary>
        /// 各分類的圖片數量限制
        /// </summary>
        public Dictionary<ImageCategory, int> MaxImageCounts { get; set; } = new()
        {
            { ImageCategory.Avatar, 1 },
            { ImageCategory.Gallery, 20 },
            { ImageCategory.BedRoom, 10 },
            { ImageCategory.Living, 10 },
            { ImageCategory.Kitchen, 10 },
            { ImageCategory.Balcony, 10 },
            { ImageCategory.Product, 10 }
        };

        /// <summary>
        /// 啟用圖片處理
        /// </summary>
        public bool EnableImageProcessing { get; set; } = true;

        /// <summary>
        /// 生成縮圖
        /// </summary>
        public bool GenerateThumbnails { get; set; } = true;

        /// <summary>
        /// 縮圖尺寸
        /// </summary>
        public ImageSize ThumbnailSize { get; set; } = new(150, 150);

        /// <summary>
        /// 生成中等尺寸圖片
        /// </summary>
        public bool GenerateMediumImages { get; set; } = true;

        /// <summary>
        /// 中等尺寸圖片尺寸
        /// </summary>
        public ImageSize MediumSize { get; set; } = new(800, 600);

        /// <summary>
        /// 圖片品質（1-100）
        /// </summary>
        public int ImageQuality { get; set; } = 85;

        /// <summary>
        /// 是否保留原始圖片
        /// </summary>
        public bool KeepOriginalImage { get; set; } = true;

        /// <summary>
        /// 臨時上傳路徑
        /// </summary>
        public string TempUploadPath { get; set; } = Path.Combine(Path.GetTempPath(), "zuHause", "uploads");

        /// <summary>
        /// 上傳逾時時間（秒）
        /// </summary>
        public int UploadTimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// 單次上傳最大檔案數量
        /// </summary>
        public int MaxFilesPerUpload { get; set; } = 10;

        /// <summary>
        /// 啟用並發上傳
        /// </summary>
        public bool EnableConcurrentUpload { get; set; } = true;

        /// <summary>
        /// 最大並發上傳數量
        /// </summary>
        public int MaxConcurrentUploads { get; set; } = 3;

        /// <summary>
        /// 驗證設定是否有效
        /// </summary>
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (MaxRetryCount < 0)
                errors.Add("MaxRetryCount 必須大於或等於 0");

            if (RetryDelayMs < 0)
                errors.Add("RetryDelayMs 必須大於或等於 0");

            if (ImageQuality < 1 || ImageQuality > 100)
                errors.Add("ImageQuality 必須在 1-100 之間");

            if (UploadTimeoutSeconds < 1)
                errors.Add("UploadTimeoutSeconds 必須大於 0");

            if (MaxFilesPerUpload < 1)
                errors.Add("MaxFilesPerUpload 必須大於 0");

            if (MaxConcurrentUploads < 1)
                errors.Add("MaxConcurrentUploads 必須大於 0");

            if (!AllowedMimeTypes.Any())
                errors.Add("AllowedMimeTypes 不能為空");

            if (!AllowedExtensions.Any())
                errors.Add("AllowedExtensions 不能為空");

            // 驗證檔案大小限制
            foreach (var (category, size) in MaxFileSizes)
            {
                if (size <= 0)
                    errors.Add($"分類 {category} 的檔案大小限制必須大於 0");
            }

            // 驗證尺寸限制
            foreach (var (category, limit) in DimensionLimits)
            {
                if (limit.MinWidth <= 0 || limit.MinHeight <= 0 || limit.MaxWidth <= 0 || limit.MaxHeight <= 0)
                    errors.Add($"分類 {category} 的尺寸限制必須大於 0");

                if (limit.MinWidth > limit.MaxWidth || limit.MinHeight > limit.MaxHeight)
                    errors.Add($"分類 {category} 的最小尺寸不能大於最大尺寸");
            }

            return !errors.Any();
        }
    }

    /// <summary>
    /// 圖片尺寸限制
    /// </summary>
    public class ImageDimensionLimit
    {
        /// <summary>
        /// 最小寬度
        /// </summary>
        public int MinWidth { get; set; }

        /// <summary>
        /// 最小高度
        /// </summary>
        public int MinHeight { get; set; }

        /// <summary>
        /// 最大寬度
        /// </summary>
        public int MaxWidth { get; set; }

        /// <summary>
        /// 最大高度
        /// </summary>
        public int MaxHeight { get; set; }

        public ImageDimensionLimit() { }

        public ImageDimensionLimit(int minWidth, int minHeight, int maxWidth, int maxHeight)
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
        }
    }

    /// <summary>
    /// 圖片尺寸
    /// </summary>
    public class ImageSize
    {
        /// <summary>
        /// 寬度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }

        public ImageSize() { }

        public ImageSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}