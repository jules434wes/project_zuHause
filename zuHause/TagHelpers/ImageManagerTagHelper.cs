using Microsoft.AspNetCore.Razor.TagHelpers;
using zuHause.Enums;

namespace zuHause.TagHelpers
{
    /// <summary>
    /// 通用圖片管理器 TagHelper
    /// 用法：
    /// <zh-image-manager entity-type="Property" entity-id="@Model.PropertyId" category="Gallery" max-count="15" />
    /// 或者：
    /// <zh-image-manager entity-type="Property" entity-id="2006" category="Gallery" max-count="15" />
    /// </summary>
    [HtmlTargetElement("zh-image-manager")]
    public class ImageManagerTagHelper : TagHelper
    {
        /// <summary>
        /// 實體類型，可以是字串（如 "Property"）或直接設定 EntityType 列舉
        /// </summary>
        [HtmlAttributeName("entity-type")]
        public string? EntityTypeString { get; set; }
        
        /// <summary>
        /// 直接設定 EntityType 列舉（程式化使用）
        /// </summary>
        public EntityType? EntityType { get; set; }
        
        /// <summary>
        /// 實體 ID
        /// </summary>
        [HtmlAttributeName("entity-id")]
        public int EntityId { get; set; }
        
        /// <summary>
        /// 圖片分類，可以是字串（如 "Gallery"）或直接設定 ImageCategory 列舉
        /// </summary>
        [HtmlAttributeName("category")]
        public string? CategoryString { get; set; }
        
        /// <summary>
        /// 直接設定 ImageCategory 列舉（程式化使用）
        /// </summary>
        public ImageCategory? Category { get; set; }
        
        /// <summary>
        /// 最大圖片數量
        /// </summary>
        [HtmlAttributeName("max-count")]
        public int MaxCount { get; set; } = 15;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // 將自訂標籤轉成占位 div，供前端 JS 初始化
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            // 解析實體類型
            var entityType = EntityType ?? ParseEntityType(EntityTypeString);
            
            // 解析圖片分類
            var category = Category ?? ParseCategory(CategoryString);

            output.Attributes.SetAttribute("class", "zh-image-manager");
            output.Attributes.SetAttribute("data-entity-type", entityType.ToString());
            output.Attributes.SetAttribute("data-entity-id", EntityId.ToString());
            output.Attributes.SetAttribute("data-category", category.ToString());
            output.Attributes.SetAttribute("data-max-count", MaxCount.ToString());
        }
        
        /// <summary>
        /// 解析實體類型字串
        /// </summary>
        private static EntityType ParseEntityType(string? entityTypeString)
        {
            if (string.IsNullOrEmpty(entityTypeString))
                return Enums.EntityType.Property; // 預設值
                
            if (Enum.TryParse<EntityType>(entityTypeString, true, out var result))
                return result;
                
            return Enums.EntityType.Property; // 解析失敗時的預設值
        }
        
        /// <summary>
        /// 解析圖片分類字串
        /// </summary>
        private static ImageCategory ParseCategory(string? categoryString)
        {
            if (string.IsNullOrEmpty(categoryString))
                return ImageCategory.Gallery; // 預設值
                
            if (Enum.TryParse<ImageCategory>(categoryString, true, out var result))
                return result;
                
            return ImageCategory.Gallery; // 解析失敗時的預設值
        }
    }
} 