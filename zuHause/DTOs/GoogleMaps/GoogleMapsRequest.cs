namespace zuHause.DTOs.GoogleMaps
{
    /// <summary>
    /// Google Maps Geocoding 請求參數
    /// </summary>
    public class GeocodingRequest
    {
        /// <summary>
        /// 要進行地理編碼的地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 語言偏好（預設為繁體中文）
        /// </summary>
        public string Language { get; set; } = "zh-TW";

        /// <summary>
        /// 地區偏好（預設為台灣）
        /// </summary>
        public string Region { get; set; } = "TW";
    }

    /// <summary>
    /// Google Maps Places 搜尋請求參數
    /// </summary>
    public class PlaceSearchRequest
    {
        /// <summary>
        /// 搜尋中心點緯度
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// 搜尋中心點經度
        /// </summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// 搜尋半徑（公尺，最大 50000）
        /// </summary>
        public int Radius { get; set; } = 5000;

        /// <summary>
        /// 地點類型（park, gym, police, post_office 等）
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 關鍵字搜尋
        /// </summary>
        public string Keyword { get; set; } = string.Empty;

        /// <summary>
        /// 語言偏好（預設為繁體中文）
        /// </summary>
        public string Language { get; set; } = "zh-TW";

        /// <summary>
        /// 最小評分篩選（1-5）
        /// </summary>
        public decimal? MinRating { get; set; }
    }

    /// <summary>
    /// Google Maps Distance Matrix 請求參數
    /// </summary>
    public class DistanceMatrixRequest
    {
        /// <summary>
        /// 起點座標
        /// </summary>
        public LocationPoint Origin { get; set; } = new LocationPoint();

        /// <summary>
        /// 目的地座標清單
        /// </summary>
        public List<LocationPoint> Destinations { get; set; } = new List<LocationPoint>();

        /// <summary>
        /// 交通模式（driving, walking, bicycling, transit）
        /// </summary>
        public string TravelMode { get; set; } = "driving";

        /// <summary>
        /// 單位系統（metric 公制, imperial 英制）
        /// </summary>
        public string Units { get; set; } = "metric";

        /// <summary>
        /// 語言偏好（預設為繁體中文）
        /// </summary>
        public string Language { get; set; } = "zh-TW";
    }

    /// <summary>
    /// 座標點
    /// </summary>
    public class LocationPoint
    {
        /// <summary>
        /// 緯度
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// 經度
        /// </summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// 轉換為 Google Maps API 格式字串
        /// </summary>
        public override string ToString()
        {
            return $"{Latitude},{Longitude}";
        }
    }
}