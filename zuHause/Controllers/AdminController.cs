using Microsoft.AspNetCore.Mvc;
using zuHause.AdminViewModels;
using System.Linq;

namespace zuHause.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult admin_usersList()
        {
            var viewModel = new AdminUserListViewModel();
            return View(viewModel);
        }

        public IActionResult admin_propertiesList()
        {
            var viewModel = new AdminPropertyListViewModel();
            return View(viewModel);
        }
        
        public IActionResult admin_userDetails(string id = "M001")
        {
            var viewModel = new AdminUserDetailsViewModel(id);
            return View(viewModel);
        }

        public IActionResult admin_propertyDetails()
        {
            return View();
        }

        public IActionResult admin_customerServiceList()
        {
            return View();
        }

        public IActionResult admin_customerServiceDetails(string id = "CS001")
        {
            var viewModel = new AdminCustomerServiceDetailsViewModel(id);
            return View(viewModel);
        }

        public IActionResult admin_systemMessageList()
        {
            var viewModel = new AdminSystemMessageListViewModel();
            return View(viewModel);
        }

        public IActionResult admin_systemMessageNew()
        {
            var viewModel = new AdminSystemMessageNewViewModel();
            return View(viewModel);
        }

        // AJAX endpoints for dynamic data
        [HttpPost]
        public IActionResult SearchUsers(string keyword, string searchField)
        {
            // 模擬用戶搜尋結果
            var users = new[]
            {
                new { id = "M001", name = "王小明", email = "wang@example.com" },
                new { id = "M002", name = "李小華", email = "lee@example.com" },
                new { id = "M003", name = "張小美", email = "zhang@example.com" }
            };
            
            return Json(users.Where(u => 
                u.name.Contains(keyword) || 
                u.email.Contains(keyword) || 
                u.id.Contains(keyword)).Take(5));
        }

        [HttpPost]
        public IActionResult GetTemplates(string category = "")
        {
            // 模擬模板資料
            var templates = new[]
            {
                new { id = 1, title = "歡迎訊息", category = "welcome", content = "歡迎加入zuHause平台..." },
                new { id = 2, title = "驗證通知", category = "verification", content = "您的身分驗證已通過..." },
                new { id = 3, title = "租屋提醒", category = "reminder", content = "提醒您注意租屋相關事項..." }
            };

            if (!string.IsNullOrEmpty(category))
            {
                templates = templates.Where(t => t.category == category).ToArray();
            }

            return Json(templates);
        }
    }
}
