namespace zuHause.DTOs.GoogleMaps
{
    /// <summary>
    /// Google Places API 搜尋結果
    /// </summary>
    public class PlaceSearchResult
    {
        /// <summary>
        /// 是否成功搜尋
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 搜尋到的地點清單
        /// </summary>
        public List<PlaceInfo> Places { get; set; } = new List<PlaceInfo>();

        /// <summary>
        /// 錯誤訊息（若失敗）
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// API 回應狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 地點資訊
    /// </summary>
    public class PlaceInfo
    {
        /// <summary>
        /// 地點 ID
        /// </summary>
        public string PlaceId { get; set; } = string.Empty;

        /// <summary>
        /// 地點名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 地點類型（如 park, gym, police, post_office）
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 緯度
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// 經度
        /// </summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 評分（1-5）
        /// </summary>
        public decimal? Rating { get; set; }

        /// <summary>
        /// 是否營業中
        /// </summary>
        public bool? IsOpen { get; set; }
    }
}