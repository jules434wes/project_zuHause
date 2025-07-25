using zuHause.Enums;

namespace zuHause.Models
{
    /// <summary>
    /// 多尺寸圖片處理結果
    /// </summary>
    public class MultiSizeProcessingResult
    {
        /// <summary>
        /// 圖片 GUID
        /// </summary>
        public Guid ImageGuid { get; set; }

        /// <summary>
        /// 各尺寸的處理串流
        /// </summary>
        public Dictionary<ImageSize, Stream> ProcessedStreams { get; set; } = new();

        /// <summary>
        /// 原始檔案格式
        /// </summary>
        public string OriginalFormat { get; set; } = string.Empty;

        /// <summary>
        /// 原始檔案名稱
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// 原始檔案大小（位元組）
        /// </summary>
        public long OriginalFileSizeBytes { get; set; }

        /// <summary>
        /// 處理時間
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 處理是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 錯誤訊息（如有）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 取得成功處理的尺寸數量
        /// </summary>
        public int SuccessCount => ProcessedStreams.Count;

        /// <summary>
        /// 總檔案大小（所有尺寸）
        /// </summary>
        public long TotalProcessedSizeBytes => ProcessedStreams.Values.Sum(s => s.Length);

        /// <summary>
        /// 釋放所有串流資源
        /// </summary>
        public void Dispose()
        {
            foreach (var stream in ProcessedStreams.Values)
            {
                stream?.Dispose();
            }
            ProcessedStreams.Clear();
        }

        /// <summary>
        /// 建立成功結果
        /// </summary>
        /// <param name="imageGuid">圖片 GUID</param>
        /// <param name="processedStreams">處理後的串流</param>
        /// <param name="originalFileName">原始檔案名稱</param>
        /// <param name="originalFormat">原始格式</param>
        /// <param name="originalFileSize">原始檔案大小</param>
        /// <returns>成功結果</returns>
        public static MultiSizeProcessingResult CreateSuccess(
            Guid imageGuid,
            Dictionary<ImageSize, Stream> processedStreams,
            string originalFileName,
            string originalFormat,
            long originalFileSize)
        {
            return new MultiSizeProcessingResult
            {
                ImageGuid = imageGuid,
                ProcessedStreams = processedStreams,
                OriginalFileName = originalFileName,
                OriginalFormat = originalFormat,
                OriginalFileSizeBytes = originalFileSize,
                Success = true
            };
        }

        /// <summary>
        /// 建立失敗結果
        /// </summary>
        /// <param name="imageGuid">圖片 GUID</param>
        /// <param name="originalFileName">原始檔案名稱</param>
        /// <param name="errorMessage">錯誤訊息</param>
        /// <returns>失敗結果</returns>
        public static MultiSizeProcessingResult CreateFailure(
            Guid imageGuid,
            string originalFileName,
            string errorMessage)
        {
            return new MultiSizeProcessingResult
            {
                ImageGuid = imageGuid,
                OriginalFileName = originalFileName,
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>
    /// 圖片處理異常
    /// </summary>
    public class ImageProcessingException : Exception
    {
        public ImageProcessingException(string message) : base(message) { }
        public ImageProcessingException(string message, Exception innerException) : base(message, innerException) { }
    }
}