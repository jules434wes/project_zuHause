using zuHause.Models;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 本地檔案到 Azure Blob Storage 遷移服務介面
    /// 負責處理既有本地圖片檔案批次遷移到雲端儲存
    /// </summary>
    public interface ILocalToBlobMigrationService
    {
        /// <summary>
        /// 掃描本地圖片檔案
        /// </summary>
        /// <param name="scanOptions">掃描選項</param>
        /// <returns>掃描結果</returns>
        Task<LocalImageScanResult> ScanLocalImagesAsync(LocalImageScanOptions? scanOptions = null);
        
        /// <summary>
        /// 開始批次遷移作業
        /// </summary>
        /// <param name="migrationConfig">遷移配置</param>
        /// <returns>遷移會話資訊</returns>
        Task<MigrationSession> StartMigrationAsync(MigrationConfiguration migrationConfig);
        
        /// <summary>
        /// 暫停遷移作業
        /// </summary>
        /// <param name="migrationId">遷移會話ID</param>
        /// <returns>操作結果</returns>
        Task<bool> PauseMigrationAsync(string migrationId);
        
        /// <summary>
        /// 恢復遷移作業
        /// </summary>
        /// <param name="migrationId">遷移會話ID</param>
        /// <returns>操作結果</returns>
        Task<bool> ResumeMigrationAsync(string migrationId);
        
        /// <summary>
        /// 取消遷移作業
        /// </summary>
        /// <param name="migrationId">遷移會話ID</param>
        /// <returns>操作結果</returns>
        Task<bool> CancelMigrationAsync(string migrationId);
        
        /// <summary>
        /// 獲取遷移進度
        /// </summary>
        /// <param name="migrationId">遷移會話ID</param>
        /// <returns>進度資訊</returns>
        Task<MigrationProgress> GetMigrationProgressAsync(string migrationId);
        
        /// <summary>
        /// 獲取所有遷移會話
        /// </summary>
        /// <returns>會話列表</returns>
        Task<List<MigrationSession>> GetMigrationSessionsAsync();
        
        /// <summary>
        /// 驗證遷移結果
        /// </summary>
        /// <param name="migrationId">遷移會話ID</param>
        /// <returns>驗證結果</returns>
        Task<MigrationValidationResult> ValidateMigrationAsync(string migrationId);
        
        /// <summary>
        /// 清理本地檔案
        /// </summary>
        /// <param name="migrationId">遷移會話ID</param>
        /// <returns>清理結果</returns>
        Task<MigrationCleanupResult> CleanupLocalFilesAsync(string migrationId);
        
        /// <summary>
        /// 回滾遷移（刪除已上傳的雲端檔案）
        /// </summary>
        /// <param name="migrationId">遷移會話ID</param>
        /// <returns>回滾結果</returns>
        Task<bool> RollbackMigrationAsync(string migrationId);
    }
}