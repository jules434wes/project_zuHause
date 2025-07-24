using zuHause.Enums;

namespace zuHause.Helpers
{
    /// <summary>
    /// 房源圖片分類雙語轉換輔助類
    /// 提供中文分類與英文分類之間的雙向轉換功能
    /// </summary>
    public static class PropertyImageCategoryHelper
    {
        /// <summary>
        /// 中文分類到英文分類的對應字典（儲存用）
        /// </summary>
        private static readonly Dictionary<string, ImageCategory> ChineseToEnglish = new()
        {
            { "總覽", ImageCategory.Gallery },
            { "客廳", ImageCategory.Living },
            { "臥室", ImageCategory.BedRoom },
            { "主臥", ImageCategory.BedRoom },
            { "陽台", ImageCategory.Balcony },
            { "飯廳", ImageCategory.Kitchen },
            { "書房", ImageCategory.Living },
            { "衛浴", ImageCategory.Balcony },
            { "衣帽間", ImageCategory.BedRoom },
            { "其他", ImageCategory.Gallery }
        };

        /// <summary>
        /// 英文分類到中文分類的對應字典（顯示用）
        /// 優先顯示最常用的中文分類
        /// </summary>
        private static readonly Dictionary<ImageCategory, string> EnglishToChinese = new()
        {
            { ImageCategory.Gallery, "總覽" },
            { ImageCategory.Living, "客廳" },
            { ImageCategory.BedRoom, "臥室" },
            { ImageCategory.Balcony, "陽台" },
            { ImageCategory.Kitchen, "飯廳" },
            { ImageCategory.Avatar, "其他" },
            { ImageCategory.Product, "其他" }
        };

        /// <summary>
        /// 房源專用的中文分類列表（前端顯示用）
        /// </summary>
        private static readonly List<string> PropertyChineseCategories = new()
        {
            "總覽", "客廳", "飯廳", "書房", "主臥", "臥室", "陽台", "衛浴", "衣帽間", "其他"
        };

        /// <summary>
        /// 將中文分類轉換為對應的英文分類（用於資料庫儲存）
        /// </summary>
        /// <param name="chineseCategory">中文分類名稱</param>
        /// <returns>對應的英文分類，無法對應時返回 Gallery</returns>
        public static ImageCategory GetEnglishCategory(string chineseCategory)
        {
            if (string.IsNullOrWhiteSpace(chineseCategory))
                return ImageCategory.Gallery;

            return ChineseToEnglish.TryGetValue(chineseCategory.Trim(), out var englishCategory) 
                ? englishCategory 
                : ImageCategory.Gallery;
        }

        /// <summary>
        /// 將英文分類轉換為對應的中文分類（用於前端顯示）
        /// </summary>
        /// <param name="englishCategory">英文分類</param>
        /// <returns>對應的中文分類名稱，無法對應時返回「其他」</returns>
        public static string GetChineseCategory(ImageCategory englishCategory)
        {
            return EnglishToChinese.TryGetValue(englishCategory, out var chineseCategory) 
                ? chineseCategory 
                : "其他";
        }

        /// <summary>
        /// 取得所有房源專用的中文分類列表（用於前端選項）
        /// </summary>
        /// <returns>中文分類名稱列表</returns>
        public static List<string> GetAllPropertyChineseCategories()
        {
            return new List<string>(PropertyChineseCategories);
        }

        /// <summary>
        /// 取得中文分類與英文分類的對應字典（用於前端 JavaScript）
        /// </summary>
        /// <returns>中文到英文的對應字典</returns>
        public static Dictionary<string, string> GetCategoryMappingForJs()
        {
            return ChineseToEnglish.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value.ToString()
            );
        }

        /// <summary>
        /// 驗證中文分類是否有效
        /// </summary>
        /// <param name="chineseCategory">要驗證的中文分類</param>
        /// <returns>是否為有效的房源分類</returns>
        public static bool IsValidPropertyCategory(string chineseCategory)
        {
            return !string.IsNullOrWhiteSpace(chineseCategory) && 
                   ChineseToEnglish.ContainsKey(chineseCategory.Trim());
        }
    }
}