using Microsoft.AspNetCore.Mvc;

namespace zuhause.Controllers
{
    public class TenantController : Controller
    {
        public IActionResult FrontPage()
        {
            return View();
        }

        public IActionResult Announcement()
        {
            return View();
        }

        public IActionResult Search()
        {
            return View();
        }
        public IActionResult CollectionAndComparison()
        {
            return View();
        }

    }
}
