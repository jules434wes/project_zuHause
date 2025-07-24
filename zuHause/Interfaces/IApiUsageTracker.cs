using System;
using System.Threading.Tasks;

namespace zuHause.Interfaces
{
    /// <summary>
    /// Google Maps API 使用量追蹤介面
    /// </summary>
    public interface IApiUsageTracker
    {
        /// <summary>
        /// 檢查指定 API 是否可以使用（未超過每日限制）
        /// </summary>
        /// <param name="apiType">API 類型（Geocoding, Places, DistanceMatrix）</param>
        /// <returns>true 如果可以使用，false 如果已達限制</returns>
        Task<bool> CanUseApiAsync(string apiType);

        /// <summary>
        /// 記錄 API 使用量
        /// </summary>
        /// <param name="apiType">API 類型</param>
        /// <param name="cost">預估成本，預設為 0</param>
        Task RecordApiUsageAsync(string apiType, decimal cost = 0);

        /// <summary>
        /// 獲取指定日期的 API 使用次數
        /// </summary>
        /// <param name="apiType">API 類型</param>
        /// <param name="date">查詢日期，null 則為今日</param>
        /// <returns>使用次數</returns>
        Task<int> GetDailyUsageAsync(string apiType, DateTime? date = null);

        /// <summary>
        /// 檢查指定 API 是否已達每日限制
        /// </summary>
        /// <param name="apiType">API 類型</param>
        /// <param name="date">查詢日期，null 則為今日</param>
        /// <returns>true 如果已達限制</returns>
        Task<bool> IsLimitReachedAsync(string apiType, DateTime? date = null);
    }
}