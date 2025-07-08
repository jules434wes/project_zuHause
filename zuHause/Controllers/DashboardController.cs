using Microsoft.AspNetCore.Mvc;

namespace zuHause.Controllers
{
    public class DashboardController : Controller
    {
       
        public IActionResult Index(string tab = "overview")
        {
            ViewBag.Tab = tab;
            ViewBag.CurrentTabName = GetTabName(tab);

            // 模擬登入者資訊
            ViewBag.UserName = "使用者名稱";
            ViewBag.UserId = "使用者編號";
            ViewBag.UserRole = "超級管理員";
            return View();
        }
        private string GetTabName(string tab)
        {
            var tabNames = new Dictionary<string, string>
            {
                { "overview", "📊 平台整體概況" },
                { "monitor", "📦 商品/房源監控" },
                { "behavior", "👤 用戶行為監控" },
                { "orders", "🛒 訂單金流" },
                { "system", "🛠️ 系統狀態" },
                { "roles", "👑 角色管理" },
                { "Backend_user_list", "👨‍💻 後臺用戶" },
                { "contract_template", "📄 合約範本" },
                { "platform_fee", "💰 費用設定" },
                { "imgup", "🖼️ 輪播圖片" },
                { "furniture_fee", "🚚 配送費設定" },
                { "Marquee_edit", "🎠 跑馬燈管理" },
                { "furniture_management", "📦 家具管理" }
            };

            return tabNames.ContainsKey(tab) ? tabNames[tab] : "未知";
        }
    }

}
