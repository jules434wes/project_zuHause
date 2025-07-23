namespace zuHause.DTOs
{
    /// <summary>
    /// 房源地圖資料傳輸物件
    /// </summary>
    public class PropertyMapDto
    {
        /// <summary>
        /// 房源ID
        /// </summary>
        public int PropertyId { get; set; }

        /// <summary>
        /// 緯度
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// 經度
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// 附近設施列表
        /// </summary>
        public List<NearbyPlaceDto> NearbyPlaces { get; set; } = new List<NearbyPlaceDto>();

        /// <summary>
        /// 是否已達API使用限制
        /// </summary>
        public bool IsLimited { get; set; }

        /// <summary>
        /// 限制訊息
        /// </summary>
        public string? LimitMessage { get; set; }

        /// <summary>
        /// 快取建立時間
        /// </summary>
        public DateTime? CachedAt { get; set; }

        /// <summary>
        /// 快取過期時間
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 建立API限制回應
        /// </summary>
        public static PropertyMapDto CreateLimitedResponse(int propertyId, string message)
        {
            return new PropertyMapDto
            {
                PropertyId = propertyId,
                IsLimited = true,
                LimitMessage = message,
                NearbyPlaces = new List<NearbyPlaceDto>()
            };
        }

        /// <summary>
        /// 建立錯誤回應
        /// </summary>
        public static PropertyMapDto CreateErrorResponse(int propertyId, string errorMessage)
        {
            return new PropertyMapDto
            {
                PropertyId = propertyId,
                IsLimited = false,
                LimitMessage = errorMessage,
                NearbyPlaces = new List<NearbyPlaceDto>()
            };
        }
    }
}