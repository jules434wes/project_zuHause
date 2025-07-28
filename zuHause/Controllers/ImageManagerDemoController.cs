using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Enums;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Controllers
{
    /// <summary>
    /// 圖片管理器演示控制器 - 提供真實資料庫連動的測試頁面
    /// </summary>
    public class ImageManagerDemoController : Controller
    {
        private readonly IImageQueryService _imageQueryService;
        private readonly ZuHauseContext _context;
        private readonly ILogger<ImageManagerDemoController> _logger;

        public ImageManagerDemoController(
            IImageQueryService imageQueryService,
            ZuHauseContext context,
            ILogger<ImageManagerDemoController> logger)
        {
            _imageQueryService = imageQueryService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 純 API 方式演示頁面 - 通用圖片管理測試
        /// </summary>
        public async Task<IActionResult> PureAPI()
        {
            try
            {
                // 提供可用的實體類型清單
                var entityTypes = Enum.GetValues<EntityType>()
                    .Select(e => new { Value = (int)e, Text = e.ToString() })
                    .ToList();

                // 提供預設的演示設定，但允許用戶自行選擇
                ViewBag.Title = "純 API 方式演示";
                ViewBag.Description = "完全使用 JavaScript + REST API，支援自選實體類型和 ID 進行測試";
                ViewBag.EntityTypes = entityTypes;
                ViewBag.DefaultEntityType = (int)EntityType.Property;
                ViewBag.DefaultEntityId = 1; // 使用更常見的預設 ID
                ViewBag.DefaultCategory = (int)ImageCategory.Gallery;

                _logger.LogInformation("PureAPI 演示頁面載入完成，提供通用實體選擇功能");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入 PureAPI 演示頁面時發生錯誤");
                
                // 錯誤時提供基本功能
                ViewBag.Title = "純 API 方式演示";
                ViewBag.Description = "完全使用 JavaScript + REST API，支援自選實體類型和 ID 進行測試";
                ViewBag.EntityTypes = new[] { new { Value = 1, Text = "Property" } };
                ViewBag.DefaultEntityType = (int)EntityType.Property;
                ViewBag.DefaultEntityId = 1;
                ViewBag.DefaultCategory = (int)ImageCategory.Gallery;

                return View();
            }
        }

        /// <summary>
        /// Tag Helper 方式演示頁面 - 簡化的統一圖片管理器
        /// </summary>
        public IActionResult PureAPISimple()
        {
            try
            {
                ViewBag.Title = "Tag Helper 圖片管理器演示";
                ViewBag.Description = "使用 Tag Helper 的統一圖片管理組件，支援拖拽排序、主圖設定等完整功能";
                
                _logger.LogInformation("PureAPISimple 演示頁面載入完成");
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入 PureAPISimple 演示頁面時發生錯誤");
                
                ViewBag.Title = "Tag Helper 圖片管理器演示";
                ViewBag.Description = "載入時發生錯誤，但基本功能仍可使用";
                
                return View();
            }
        }

        /// <summary>
        /// 演示首頁 - 提供功能選擇
        /// </summary>
        public IActionResult Index()
        {
            ViewBag.Title = "圖片管理器演示系統";
            return View();
        }
    }
}