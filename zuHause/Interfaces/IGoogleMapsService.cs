using zuHause.DTOs.GoogleMaps;

namespace zuHause.Interfaces
{
    /// <summary>
    /// Google Maps 服務介面
    /// </summary>
    public interface IGoogleMapsService
    {
        /// <summary>
        /// 地理編碼 - 將地址轉換為座標
        /// </summary>
        /// <param name="request">地理編碼請求參數</param>
        /// <returns>地理編碼結果</returns>
        Task<GeocodingResult> GeocodeAsync(GeocodingRequest request);

        /// <summary>
        /// 地點搜尋 - 搜尋指定類型的附近地點
        /// </summary>
        /// <param name="request">地點搜尋請求參數</param>
        /// <returns>地點搜尋結果</returns>
        Task<PlaceSearchResult> SearchPlacesAsync(PlaceSearchRequest request);

        /// <summary>
        /// 距離計算 - 計算起點到多個目的地的距離
        /// </summary>
        /// <param name="request">距離計算請求參數</param>
        /// <returns>距離計算結果</returns>
        Task<DistanceMatrixResult> CalculateDistancesAsync(DistanceMatrixRequest request);

        /// <summary>
        /// 批次地理編碼 - 處理多個地址
        /// </summary>
        /// <param name="addresses">地址清單</param>
        /// <returns>地理編碼結果清單</returns>
        Task<List<GeocodingResult>> BatchGeocodeAsync(List<string> addresses);
    }
}