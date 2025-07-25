namespace zuHause.Options
{
    /// <summary>
    /// Azure Blob Storage 配置選項
    /// </summary>
    public class BlobStorageOptions
    {
        public const string SectionName = "AzureBlobStorage";

        /// <summary>
        /// Azure Blob Storage 連線字串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Container 名稱
        /// </summary>
        public string ContainerName { get; set; } = "zuhaus-images";

        /// <summary>
        /// 基礎 URL
        /// </summary>
        public string BaseUrl { get; set; } = "https://zuhauseimg.blob.core.windows.net";

        /// <summary>
        /// 臨時檔案保留時間（小時）
        /// </summary>
        public int TempFileRetentionHours { get; set; } = 6;

        /// <summary>
        /// 清理間隔時間（小時）
        /// </summary>
        public int CleanupIntervalHours { get; set; } = 1;

        /// <summary>
        /// 最大重試次數
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 1;

        /// <summary>
        /// 最大併發上傳數
        /// </summary>
        public int MaxConcurrentUploads { get; set; } = 3;

        /// <summary>
        /// 最大檔案大小（位元組）
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024; // 2MB
    }
}