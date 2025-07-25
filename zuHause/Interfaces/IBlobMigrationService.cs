using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Interfaces
{
    /// <summary>
    /// Blob 檔案遷移服務介面
    /// 負責處理臨時區域到正式區域的檔案移動邏輯
    /// </summary>
    public interface IBlobMigrationService
    {
        /// <summary>
        /// 將臨時區域的檔案移動到正式區域
        /// </summary>
        /// <param name="tempSessionId">臨時會話ID</param>
        /// <param name="imageGuids">要遷移的圖片GUID列表</param>
        /// <param name="category">目標類別</param>
        /// <param name="entityId">目標實體ID</param>
        /// <returns>遷移結果</returns>
        Task<MigrationResult> MoveTempToPermanentAsync(
            string tempSessionId, 
            IEnumerable<Guid> imageGuids, 
            ImageCategory category, 
            int entityId);

        /// <summary>
        /// 驗證臨時檔案是否存在且有效
        /// </summary>
        /// <param name="tempSessionId">臨時會話ID</param>
        /// <param name="imageGuids">圖片GUID列表</param>
        /// <returns>驗證結果</returns>
        Task<BlobValidationResult> ValidateTempFilesAsync(
            string tempSessionId, 
            IEnumerable<Guid> imageGuids);

        /// <summary>
        /// 回滾已移動的檔案（發生錯誤時使用）
        /// </summary>
        /// <param name="migrationResult">要回滾的遷移結果</param>
        /// <returns>回滾結果</returns>
        Task<bool> RollbackMigrationAsync(MigrationResult migrationResult);
    }
}