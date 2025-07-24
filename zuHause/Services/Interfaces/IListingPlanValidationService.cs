using zuHause.DTOs;
using zuHause.Models;

namespace zuHause.Services.Interfaces
{
    /// <summary>
    /// 刊登方案驗證服務介面
    /// 提供刊登方案驗證、時間計算和費用計算功能
    /// </summary>
    public interface IListingPlanValidationService
    {
        /// <summary>
        /// 驗證刊登方案是否有效
        /// 檢查方案是否啟用、是否在有效期間內
        /// </summary>
        /// <param name="planId">方案ID</param>
        /// <returns>驗證結果</returns>
        Task<PropertyValidationResult> ValidateListingPlanAsync(int planId);

        /// <summary>
        /// 計算刊登到期日
        /// 規則：開始日期 + 方案天數 + 1天的 00:00
        /// </summary>
        /// <param name="planId">方案ID</param>
        /// <param name="startDate">開始日期</param>
        /// <returns>到期日期</returns>
        Task<DateTime?> CalculateExpireDateAsync(int planId, DateTime startDate);

        /// <summary>
        /// 計算刊登總費用
        /// 規則：每日費用 × 最小刊登天數
        /// </summary>
        /// <param name="planId">方案ID</param>
        /// <returns>總費用</returns>
        Task<decimal?> CalculateTotalFeeAsync(int planId);

        /// <summary>
        /// 獲取目前可用的刊登方案
        /// 條件：IsActive=true 且在有效期間內
        /// </summary>
        /// <returns>可用方案列表</returns>
        Task<List<ListingPlan>> GetActiveListingPlansAsync();

        /// <summary>
        /// 驗證刊登時間期間是否有效
        /// 檢查請求日期是否在方案有效期間內
        /// </summary>
        /// <param name="planId">方案ID</param>
        /// <param name="requestDate">請求日期</param>
        /// <returns>驗證結果</returns>
        Task<PropertyValidationResult> ValidateListingPeriodAsync(int planId, DateTime requestDate);

        /// <summary>
        /// 根據方案ID獲取方案詳細資訊
        /// </summary>
        /// <param name="planId">方案ID</param>
        /// <returns>方案資訊</returns>
        Task<ListingPlan?> GetListingPlanByIdAsync(int planId);
    }
}