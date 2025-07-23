namespace zuHause.DTOs.GoogleMaps
{
    /// <summary>
    /// Google Distance Matrix API 回應結果
    /// </summary>
    public class DistanceMatrixResult
    {
        /// <summary>
        /// 是否成功計算距離
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 距離資訊清單
        /// </summary>
        public List<DistanceInfo> Distances { get; set; } = new List<DistanceInfo>();

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
    /// 距離資訊
    /// </summary>
    public class DistanceInfo
    {
        /// <summary>
        /// 目的地地址
        /// </summary>
        public string DestinationAddress { get; set; } = string.Empty;

        /// <summary>
        /// 距離（公尺）
        /// </summary>
        public int DistanceInMeters { get; set; }

        /// <summary>
        /// 距離文字描述（如 "1.2 公里"）
        /// </summary>
        public string DistanceText { get; set; } = string.Empty;

        /// <summary>
        /// 行車時間（秒）
        /// </summary>
        public int DurationInSeconds { get; set; }

        /// <summary>
        /// 行車時間文字描述（如 "5 分鐘"）
        /// </summary>
        public string DurationText { get; set; } = string.Empty;

        /// <summary>
        /// 計算狀態（OK, NOT_FOUND, ZERO_RESULTS 等）
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}