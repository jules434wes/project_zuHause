namespace zuHause.DTOs
{
    /// <summary>
    /// 附近設施資料傳輸物件
    /// </summary>
    public class NearbyPlaceDto
    {
        /// <summary>
        /// 設施名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 設施類型 (如：subway_station, supermarket 等)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 設施類型顯示名稱
        /// </summary>
        public string TypeDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 距離（公尺）
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// 步行時間（分鐘）
        /// </summary>
        public int WalkingTimeMinutes { get; set; }

        /// <summary>
        /// 設施緯度
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// 設施經度
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// 評分 (1-5 星)
        /// </summary>
        public double? Rating { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 根據設施類型取得顯示名稱
        /// </summary>
        public static string GetTypeDisplayName(string placeType)
        {
            return placeType switch
            {
                "subway_station" => "捷運站",
                "bus_station" => "公車站",
                "train_station" => "火車站",
                "supermarket" => "超市",
                "convenience_store" => "便利商店",
                "post_office" => "郵局",
                "police" => "警察局",
                "fire_station" => "消防局",
                "hospital" => "醫院",
                "park" => "公園",
                "gym" => "健身房",
                "library" => "圖書館",
                "school" => "學校",
                "university" => "大學",
                "restaurant" => "餐廳",
                "bank" => "銀行",
                "pharmacy" => "藥局",
                "shopping_mall" => "購物中心",
                _ => placeType
            };
        }

        /// <summary>
        /// 計算步行時間（基於距離估算）
        /// </summary>
        public static int CalculateWalkingTime(double distanceMeters)
        {
            // 假設步行速度為每分鐘80公尺
            const double walkingSpeedPerMinute = 80.0;
            return (int)Math.Ceiling(distanceMeters / walkingSpeedPerMinute);
        }
    }
}