using zuHause.Enums;

namespace zuHause.Models
{
    /// <summary>
    /// Blob 上傳結果模型
    /// </summary>
    public class BlobUploadResult
    {
        /// <summary>
        /// 上傳是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 結果訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 上傳後的 Blob URL
        /// </summary>
        public string? BlobUrl { get; set; }

        /// <summary>
        /// Blob 路徑
        /// </summary>
        public string? BlobPath { get; set; }

        /// <summary>
        /// 上傳時間
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 檔案大小（位元組）
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// 重試次數
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 建立成功結果
        /// </summary>
        /// <param name="blobUrl">Blob URL</param>
        /// <param name="blobPath">Blob 路徑</param>
        /// <param name="fileSizeBytes">檔案大小</param>
        /// <param name="retryCount">重試次數</param>
        /// <returns>成功結果</returns>
        public static BlobUploadResult CreateSuccess(string blobUrl, string blobPath, long fileSizeBytes, int retryCount = 0)
        {
            return new BlobUploadResult
            {
                Success = true,
                Message = "上傳成功",
                BlobUrl = blobUrl,
                BlobPath = blobPath,
                FileSizeBytes = fileSizeBytes,
                RetryCount = retryCount
            };
        }

        /// <summary>
        /// 建立失敗結果
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        /// <param name="retryCount">重試次數</param>
        /// <returns>失敗結果</returns>
        public static BlobUploadResult CreateFailure(string message, int retryCount = 0)
        {
            return new BlobUploadResult
            {
                Success = false,
                Message = message,
                RetryCount = retryCount
            };
        }
    }

    /// <summary>
    /// 原子性多尺寸上傳結果模型
    /// </summary>
    public class AtomicUploadResult
    {
        /// <summary>
        /// 上傳是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 結果訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 各尺寸上傳結果
        /// </summary>
        public Dictionary<ImageSize, BlobUploadResult> Results { get; set; } = new();

        /// <summary>
        /// 成功上傳的數量
        /// </summary>
        public int SuccessCount => Results.Values.Count(r => r.Success);

        /// <summary>
        /// 總上傳數量
        /// </summary>
        public int TotalCount => Results.Count;

        /// <summary>
        /// 上傳時間
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 是否完全成功（所有尺寸都上傳成功）
        /// </summary>
        public bool IsFullySuccessful => Success && SuccessCount == TotalCount;

        /// <summary>
        /// 建立成功結果
        /// </summary>
        /// <param name="results">各尺寸上傳結果</param>
        /// <returns>成功結果</returns>
        public static AtomicUploadResult CreateSuccess(Dictionary<ImageSize, BlobUploadResult> results)
        {
            var allSuccess = results.Values.All(r => r.Success);
            var successCount = results.Values.Count(r => r.Success);
            var totalCount = results.Count;

            return new AtomicUploadResult
            {
                Success = allSuccess,
                Message = allSuccess ? "所有尺寸上傳成功" : $"部分成功：{successCount}/{totalCount}",
                Results = results
            };
        }

        /// <summary>
        /// 建立失敗結果
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        /// <param name="results">部分結果（如有）</param>
        /// <returns>失敗結果</returns>
        public static AtomicUploadResult CreateFailure(string message, Dictionary<ImageSize, BlobUploadResult>? results = null)
        {
            return new AtomicUploadResult
            {
                Success = false,
                Message = message,
                Results = results ?? new Dictionary<ImageSize, BlobUploadResult>()
            };
        }

        /// <summary>
        /// 取得成功上傳的 URL 列表
        /// </summary>
        /// <returns>成功的 URL 清單</returns>
        public Dictionary<ImageSize, string> GetSuccessfulUrls()
        {
            return Results
                .Where(kvp => kvp.Value.Success && !string.IsNullOrEmpty(kvp.Value.BlobUrl))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.BlobUrl!);
        }

        /// <summary>
        /// 取得失敗的錯誤訊息
        /// </summary>
        /// <returns>失敗訊息清單</returns>
        public Dictionary<ImageSize, string> GetFailureMessages()
        {
            return Results
                .Where(kvp => !kvp.Value.Success)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Message);
        }
    }
}