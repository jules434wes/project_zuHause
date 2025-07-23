using zuHause.DTOs;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 房源地圖資料快取服務介面
    /// </summary>
    public interface IPropertyMapCacheService
    {
        /// <summary>
        /// 從快取中取得房源地圖資料
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>快取的地圖資料，如果不存在或已過期則回傳 null</returns>
        Task<PropertyMapDto?> GetFromCacheAsync(int propertyId);

        /// <summary>
        /// 將房源地圖資料儲存到快取
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="data">要快取的地圖資料</param>
        /// <returns>Task</returns>
        Task SetCacheAsync(int propertyId, PropertyMapDto data);

        /// <summary>
        /// 清除指定房源的快取資料
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>Task</returns>
        Task ClearCacheAsync(int propertyId);

        /// <summary>
        /// 檢查房源座標是否有變化，如有變化則清除快取
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="currentLatitude">目前緯度</param>
        /// <param name="currentLongitude">目前經度</param>
        /// <returns>是否有座標變化</returns>
        Task<bool> CheckAndClearIfLocationChangedAsync(int propertyId, decimal currentLatitude, decimal currentLongitude);
    }
}