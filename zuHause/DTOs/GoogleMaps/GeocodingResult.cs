namespace zuHause.DTOs.GoogleMaps
{
    /// <summary>
    /// Google Geocoding API 回應結果
    /// </summary>
    public class GeocodingResult
    {
        /// <summary>
        /// 是否成功取得座標
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 緯度
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// 經度
        /// </summary>
        public decimal? Longitude { get; set; }

        /// <summary>
        /// 格式化後的地址
        /// </summary>
        public string FormattedAddress { get; set; } = string.Empty;

        /// <summary>
        /// 錯誤訊息（若失敗）
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// API 回應狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}