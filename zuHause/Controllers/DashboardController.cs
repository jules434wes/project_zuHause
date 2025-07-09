using Microsoft.AspNetCore.Mvc;

namespace zuHause.Controllers
{
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Role = "超級管理員";
            ViewBag.EmployeeID = "9527";
            ViewBag.RoleAccess = new Dictionary<string, List<string>> {
                { "超級管理員", new List<string>{ "overview", "monitor", "behavior", "orders", "system", "roles", "Backend_user_list", "contract_template", "platform_fee", "imgup", "furniture_fee", "Marquee_edit", "furniture_management" } },
                { "管理員", new List<string>{ "overview", "behavior", "orders" } },
                { "房源審核員", new List<string>{ "monitor" } },
                { "客服", new List<string>{ "behavior", "orders" } }
                    };
            return View();


        }
        [HttpGet("{id}")]
        public IActionResult LoadTab(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var viewPath = $"~/Views/Dashboard/Partial/{id}.cshtml";
            if (!System.IO.File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Views", "Dashboard", "Partial", $"{id}.cshtml")))
            {
                return Content($"⚠️ 找不到對應的分頁檔案：{id}");
            }

            return PartialView(viewPath);

        }

        public IActionResult Backend_user_list()
        {
            return PartialView("Partial/_Backend_user_list");
        }










    }


}
