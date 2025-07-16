namespace zuHause.DTOs
{
    /// <summary>
    /// 圖片處理選項 DTO
    /// </summary>
    public class ImageProcessingOptions
    {
        /// <summary>
        /// 最大寬度 (null 表示不限制)
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// 最大高度 (null 表示不限制)
        /// </summary>
        public int? MaxHeight { get; set; }

        /// <summary>
        /// 品質設定 (1-100，預設 80)
        /// </summary>
        public int Quality { get; set; } = 80;

        /// <summary>
        /// 是否保持寬高比
        /// </summary>
        public bool PreserveAspectRatio { get; set; } = true;

        /// <summary>
        /// 目標格式 (預設 WebP)
        /// </summary>
        public string TargetFormat { get; set; } = "webp";

        /// <summary>
        /// 建立預設的 WebP 轉換選項
        /// </summary>
        public static ImageProcessingOptions DefaultWebP(int? maxWidth = null, int quality = 80)
        {
            return new ImageProcessingOptions
            {
                MaxWidth = maxWidth,
                Quality = quality,
                TargetFormat = "webp",
                PreserveAspectRatio = true
            };
        }

        /// <summary>
        /// 建立縮圖選項
        /// </summary>
        public static ImageProcessingOptions ForThumbnail(int width, int height, int quality = 85)
        {
            return new ImageProcessingOptions
            {
                MaxWidth = width,
                MaxHeight = height,
                Quality = quality,
                TargetFormat = "webp",
                PreserveAspectRatio = false // 縮圖通常需要固定尺寸
            };
        }
    }
}