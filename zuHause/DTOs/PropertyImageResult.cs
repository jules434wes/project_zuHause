namespace zuHause.DTOs
{
    /// <summary>
    /// 房源圖片處理結果 DTO - 避免貧血模型，包含自主邏輯
    /// </summary>
    public class PropertyImageResult
    {
        /// <summary>
        /// 處理是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 錯誤訊息 (處理失敗時)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 原始檔案名稱
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// 圖片資料庫 ID (成功時)
        /// </summary>
        public int? PropertyImageId { get; set; }

        /// <summary>
        /// 原圖相對路徑
        /// </summary>
        public string? OriginalImagePath { get; set; }

        /// <summary>
        /// 中圖相對路徑 (800px寬)
        /// </summary>
        public string? MediumImagePath { get; set; }

        /// <summary>
        /// 縮圖相對路徑 (300x200px)
        /// </summary>
        public string? ThumbnailImagePath { get; set; }

        /// <summary>
        /// 是否為主圖
        /// </summary>
        public bool IsMainImage { get; set; }

        /// <summary>
        /// 原始檔案大小 (位元組)
        /// </summary>
        public long OriginalSizeBytes { get; set; }

        /// <summary>
        /// 處理後總檔案大小 (位元組)
        /// </summary>
        public long ProcessedSizeBytes { get; set; }

        /// <summary>
        /// 圖片寬度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 圖片高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 處理時間
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        // === 實體自主邏輯 - 避免貧血模型 ===

        /// <summary>
        /// 檢查是否處理成功且有有效路徑
        /// </summary>
        /// <returns>true 如果處理成功且所有路徑都存在</returns>
        public bool IsValidResult()
        {
            return Success && 
                   PropertyImageId.HasValue &&
                   !string.IsNullOrEmpty(OriginalImagePath) &&
                   !string.IsNullOrEmpty(MediumImagePath) &&
                   !string.IsNullOrEmpty(ThumbnailImagePath);
        }

        /// <summary>
        /// 取得圖片尺寸描述
        /// </summary>
        /// <returns>格式化的尺寸字串 (例如: "1200x800")</returns>
        public string GetDimensionDescription()
        {
            return $"{Width}x{Height}";
        }

        /// <summary>
        /// 取得檔案大小節省描述
        /// </summary>
        /// <returns>壓縮率描述</returns>
        public string GetCompressionDescription()
        {
            if (OriginalSizeBytes == 0) return "無法計算";
            
            var compressionRatio = (double)(OriginalSizeBytes - ProcessedSizeBytes) / OriginalSizeBytes * 100;
            return compressionRatio > 0 
                ? $"節省 {compressionRatio:F1}% 空間" 
                : "檔案大小未改變";
        }

        /// <summary>
        /// 取得處理摘要資訊
        /// </summary>
        /// <returns>包含檔案名稱、尺寸、壓縮率的摘要字串</returns>
        public string GetProcessingSummary()
        {
            if (!Success)
                return $"處理失敗: {ErrorMessage}";

            var mainImageIndicator = IsMainImage ? " (主圖)" : "";
            return $"{OriginalFileName}{mainImageIndicator} - {GetDimensionDescription()} - {GetCompressionDescription()}";
        }

        /// <summary>
        /// 取得所有圖片路徑清單
        /// </summary>
        /// <returns>包含原圖、中圖、縮圖的路徑列表</returns>
        public List<string> GetAllImagePaths()
        {
            var paths = new List<string>();
            
            if (!string.IsNullOrEmpty(OriginalImagePath))
                paths.Add(OriginalImagePath);
            
            if (!string.IsNullOrEmpty(MediumImagePath))
                paths.Add(MediumImagePath);
                
            if (!string.IsNullOrEmpty(ThumbnailImagePath))
                paths.Add(ThumbnailImagePath);
                
            return paths;
        }

        /// <summary>
        /// 建立成功的處理結果
        /// </summary>
        public static PropertyImageResult CreateSuccess(
            string originalFileName,
            int propertyImageId,
            string originalImagePath,
            string mediumImagePath,
            string thumbnailImagePath,
            bool isMainImage,
            long originalSizeBytes,
            long processedSizeBytes,
            int width,
            int height)
        {
            return new PropertyImageResult
            {
                Success = true,
                OriginalFileName = originalFileName,
                PropertyImageId = propertyImageId,
                OriginalImagePath = originalImagePath,
                MediumImagePath = mediumImagePath,
                ThumbnailImagePath = thumbnailImagePath,
                IsMainImage = isMainImage,
                OriginalSizeBytes = originalSizeBytes,
                ProcessedSizeBytes = processedSizeBytes,
                Width = width,
                Height = height
            };
        }

        /// <summary>
        /// 建立失敗的處理結果
        /// </summary>
        public static PropertyImageResult CreateFailure(string originalFileName, string errorMessage)
        {
            return new PropertyImageResult
            {
                Success = false,
                OriginalFileName = originalFileName,
                ErrorMessage = errorMessage
            };
        }
    }
}