namespace zuHause.Models
{
    /// <summary>
    /// 清理操作結果模型
    /// </summary>
    public class CleanupResult
    {
        /// <summary>
        /// 清理是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息（如有）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 清理的 Blob 檔案數量
        /// </summary>
        public int BlobFilesDeleted { get; set; }

        /// <summary>
        /// 清理的快取項目數量
        /// </summary>
        public int CacheEntriesDeleted { get; set; }

        /// <summary>
        /// 清理開始時間
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 清理完成時間
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 清理耗時
        /// </summary>
        public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

        /// <summary>
        /// 清理的總項目數量
        /// </summary>
        public int TotalItemsDeleted => BlobFilesDeleted + CacheEntriesDeleted;

        /// <summary>
        /// 建立成功的清理結果
        /// </summary>
        public static CleanupResult CreateSuccess(int blobFilesDeleted, int cacheEntriesDeleted)
        {
            return new CleanupResult
            {
                IsSuccess = true,
                BlobFilesDeleted = blobFilesDeleted,
                CacheEntriesDeleted = cacheEntriesDeleted,
                CompletedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 建立失敗的清理結果
        /// </summary>
        public static CleanupResult CreateFailure(string errorMessage, int blobFilesDeleted = 0, int cacheEntriesDeleted = 0)
        {
            return new CleanupResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                BlobFilesDeleted = blobFilesDeleted,
                CacheEntriesDeleted = cacheEntriesDeleted,
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// 清理統計資訊模型
    /// </summary>
    public class CleanupStatistics
    {
        /// <summary>
        /// 最後清理時間
        /// </summary>
        public DateTime? LastCleanupTime { get; set; }

        /// <summary>
        /// 下次清理時間
        /// </summary>
        public DateTime? NextCleanupTime { get; set; }

        /// <summary>
        /// 今日清理次數
        /// </summary>
        public int TodayCleanupCount { get; set; }

        /// <summary>
        /// 今日清理的檔案總數
        /// </summary>
        public int TodayFilesDeleted { get; set; }

        /// <summary>
        /// 今日清理的快取項目總數
        /// </summary>
        public int TodayCacheEntriesDeleted { get; set; }

        /// <summary>
        /// 清理服務是否運行中
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// 清理間隔（小時）
        /// </summary>
        public int CleanupIntervalHours { get; set; } = 1;

        /// <summary>
        /// 檔案過期時間（小時）
        /// </summary>
        public int FileExpirationHours { get; set; } = 6;

        /// <summary>
        /// 最後錯誤訊息（如有）
        /// </summary>
        public string? LastErrorMessage { get; set; }

        /// <summary>
        /// 最後錯誤時間（如有）
        /// </summary>
        public DateTime? LastErrorTime { get; set; }
    }
}