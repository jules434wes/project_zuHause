using zuHause.Models;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 臨時檔案清理服務介面
    /// 提供定時清理過期臨時檔案和快取的功能
    /// </summary>
    public interface ITempFileCleanupService
    {
        /// <summary>
        /// 手動執行完整清理
        /// </summary>
        /// <param name="olderThanHours">清理幾小時前的檔案（預設為 6 小時）</param>
        /// <returns>清理結果</returns>
        Task<CleanupResult> ExecuteCleanupAsync(int olderThanHours = 6);

        /// <summary>
        /// 清理過期的 Blob 檔案
        /// </summary>
        /// <param name="olderThanHours">清理幾小時前的檔案</param>
        /// <returns>清理的檔案數量</returns>
        Task<int> CleanupExpiredBlobFilesAsync(int olderThanHours = 6);

        /// <summary>
        /// 清理過期的快取項目
        /// </summary>
        /// <returns>清理的快取項目數量</returns>
        Task<int> CleanupExpiredCacheEntriesAsync();

        /// <summary>
        /// 取得清理統計資訊
        /// </summary>
        /// <returns>清理統計</returns>
        Task<CleanupStatistics> GetCleanupStatisticsAsync();
    }
}