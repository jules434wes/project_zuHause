using Microsoft.AspNetCore.Mvc;
using zuHause.DTOs;
using zuHause.Interfaces;
using zuHause.Enums;

namespace zuHause.Components
{
    /// <summary>
    /// 房源管理卡片 ViewComponent
    /// 支援多種顯示模式，可重用於不同頁面
    /// </summary>
    public class PropertyManagementCardViewComponent : ViewComponent
    {
        private readonly IImageQueryService _imageQueryService;
        
        public PropertyManagementCardViewComponent(IImageQueryService imageQueryService)
        {
            _imageQueryService = imageQueryService;
        }

        /// <summary>
        /// 房源管理卡片組件主要方法
        /// </summary>
        /// <param name="property">房源資料</param>
        /// <param name="displayMode">顯示模式</param>
        /// <param name="showActions">是否顯示操作按鈕</param>
        /// <param name="showStats">是否顯示統計資訊</param>
        /// <returns>卡片視圖</returns>
        public async Task<IViewComponentResult> InvokeAsync(
            PropertyManagementDto property, 
            PropertyCardDisplayMode displayMode = PropertyCardDisplayMode.Management,
            bool showActions = true,
            bool showStats = true)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            // ThumbnailUrl 現在直接從資料庫的 PreviewImageUrl 欄位讀取，不需要動態更新

            // 確保 ImageUrls 有資料
            if (!property.ImageUrls.Any())
            {
                try
                {
                    var images = await _imageQueryService.GetImagesByEntityAsync(EntityType.Property, property.PropertyId);
                    property.ImageUrls = images
                        .Select(img => _imageQueryService.GenerateImageUrl(img, ImageSize.Medium))
                        .ToList();
                }
                catch (Exception)
                {
                    // 圖片服務失效時保持空清單
                    property.ImageUrls = new List<string>();
                }
            }

            var model = new PropertyCardDisplayDto
            {
                Property = property,
                DisplayMode = displayMode,
                ShowActions = showActions && displayMode == PropertyCardDisplayMode.Management,
                ShowStats = showStats && displayMode != PropertyCardDisplayMode.Profile,
                ShowStatusBadge = true,
                CustomCssClass = GetCustomCssClass(displayMode, property)
            };

            // 根據顯示模式選擇不同的視圖模板
            var viewName = GetViewName(displayMode);

            return View(viewName, model);
        }

        /// <summary>
        /// 根據顯示模式取得視圖名稱
        /// </summary>
        /// <param name="displayMode">顯示模式</param>
        /// <returns>視圖名稱</returns>
        private static string GetViewName(PropertyCardDisplayMode displayMode)
        {
            return displayMode switch
            {
                PropertyCardDisplayMode.Management => "Management",
                PropertyCardDisplayMode.Profile => "Profile",
                PropertyCardDisplayMode.Stats => "Stats",
                PropertyCardDisplayMode.Compact => "Compact",
                _ => "Management"
            };
        }

        /// <summary>
        /// 根據顯示模式和房源狀態取得自訂CSS類別
        /// </summary>
        /// <param name="displayMode">顯示模式</param>
        /// <param name="property">房源資料</param>
        /// <returns>CSS類別字串</returns>
        private static string GetCustomCssClass(PropertyCardDisplayMode displayMode, PropertyManagementDto property)
        {
            var classes = new List<string>();

            // 基於顯示模式的CSS類別
            classes.Add($"property-card-{displayMode.ToString().ToLower()}");

            // 基於狀態分組的CSS類別
            classes.Add($"property-group-{property.StatusGroup.ToString().ToLower()}");

            // 基於狀態的CSS類別
            classes.Add($"property-status-{property.StatusCode.ToLower()}");

            // 特殊狀態標記
            if (property.RequiresAction)
            {
                classes.Add("property-requires-action");
            }

            if (property.IsLegacyStatus)
            {
                classes.Add("property-legacy-status");
            }

            return string.Join(" ", classes);
        }
    }
}