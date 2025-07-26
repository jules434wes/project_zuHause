using zuHause.Enums;

namespace zuHause.Models
{
    /// <summary>
    /// 圖片尺寸規格定義
    /// 定義各種圖片尺寸的寬度和高度規格
    /// </summary>
    public static class ImageSizeSpecs
    {
        /// <summary>
        /// 各尺寸的寬高定義
        /// </summary>
        public static readonly Dictionary<ImageSize, (int Width, int Height)> Dimensions = new()
        {
            [ImageSize.Original] = (1920, 1080),    // 原始尺寸 (Full HD)
            [ImageSize.Large] = (1200, 800),        // 大尺寸
            [ImageSize.Medium] = (800, 600),         // 中等尺寸
            [ImageSize.Thumbnail] = (300, 200)      // 縮圖尺寸
        };

        /// <summary>
        /// 取得指定尺寸的寬度
        /// </summary>
        /// <param name="size">圖片尺寸</param>
        /// <returns>寬度像素</returns>
        public static int GetWidth(ImageSize size)
        {
            return Dimensions.TryGetValue(size, out var dimension) ? dimension.Width : 0;
        }

        /// <summary>
        /// 取得指定尺寸的高度
        /// </summary>
        /// <param name="size">圖片尺寸</param>
        /// <returns>高度像素</returns>
        public static int GetHeight(ImageSize size)
        {
            return Dimensions.TryGetValue(size, out var dimension) ? dimension.Height : 0;
        }

        /// <summary>
        /// 取得所有支援的尺寸
        /// </summary>
        /// <returns>支援的尺寸列表</returns>
        public static IEnumerable<ImageSize> GetAllSizes()
        {
            return Dimensions.Keys;
        }

        /// <summary>
        /// 檢查指定尺寸是否有效
        /// </summary>
        /// <param name="size">要檢查的尺寸</param>
        /// <returns>是否為有效尺寸</returns>
        public static bool IsValidSize(ImageSize size)
        {
            return Dimensions.ContainsKey(size);
        }
    }
}