namespace zuHause.DTOs
{
    /// <summary>
    /// 圖片處理結果 DTO - 避免貧血模型，包含自主邏輯
    /// </summary>
    public class ImageProcessingResult : IDisposable
    {
        /// <summary>
        /// 處理是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 處理後的圖片串流
        /// </summary>
        public Stream? ProcessedStream { get; set; }

        /// <summary>
        /// 錯誤訊息 (處理失敗時)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 處理後圖片寬度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 處理後圖片高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 處理後檔案大小 (位元組)
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// 原始檔案格式
        /// </summary>
        public string? OriginalFormat { get; set; }

        /// <summary>
        /// 處理後檔案格式
        /// </summary>
        public string ProcessedFormat { get; set; } = "webp";

        /// <summary>
        /// 處理時間
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        // === 實體自主邏輯 - 避免貧血模型 ===

        /// <summary>
        /// 檢查結果是否適合顯示
        /// </summary>
        /// <returns>true 如果處理成功且有有效的圖片串流</returns>
        public bool IsValidForDisplay()
        {
            return Success && 
                   ProcessedStream != null && 
                   SizeBytes > 0 && 
                   Width > 0 && 
                   Height > 0;
        }

        /// <summary>
        /// 取得圖片尺寸描述
        /// </summary>
        /// <returns>格式化的尺寸字串 (例如: "800x600")</returns>
        public string GetDimensionDescription()
        {
            return $"{Width}x{Height}";
        }

        /// <summary>
        /// 取得檔案大小的友善顯示格式
        /// </summary>
        /// <returns>格式化的檔案大小 (例如: "1.2 MB")</returns>
        public string GetFileSizeDescription()
        {
            if (SizeBytes < 1024)
                return $"{SizeBytes} B";
            
            if (SizeBytes < 1024 * 1024)
                return $"{SizeBytes / 1024.0:F1} KB";
            
            return $"{SizeBytes / (1024.0 * 1024.0):F1} MB";
        }

        /// <summary>
        /// 檢查是否為有效的網頁圖片格式
        /// </summary>
        /// <returns>true 如果適合網頁使用</returns>
        public bool IsWebOptimized()
        {
            return Success && 
                   ProcessedFormat.ToLower() == "webp" && 
                   SizeBytes < 5 * 1024 * 1024; // 小於 5MB
        }

        /// <summary>
        /// 取得處理摘要資訊
        /// </summary>
        /// <returns>包含格式、尺寸、大小的摘要字串</returns>
        public string GetProcessingSummary()
        {
            if (!Success)
                return $"處理失敗: {ErrorMessage}";

            return $"{ProcessedFormat.ToUpper()} {GetDimensionDescription()} ({GetFileSizeDescription()})";
        }

        /// <summary>
        /// 建立成功的處理結果
        /// </summary>
        public static ImageProcessingResult CreateSuccess(
            Stream processedStream,
            int width,
            int height,
            string? originalFormat = null)
        {
            return new ImageProcessingResult
            {
                Success = true,
                ProcessedStream = processedStream,
                Width = width,
                Height = height,
                SizeBytes = processedStream?.Length ?? 0,
                OriginalFormat = originalFormat,
                ProcessedFormat = "webp"
            };
        }

        /// <summary>
        /// 建立失敗的處理結果
        /// </summary>
        public static ImageProcessingResult CreateFailure(string errorMessage)
        {
            return new ImageProcessingResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                ProcessedStream = null
            };
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            ProcessedStream?.Dispose();
        }
    }
}