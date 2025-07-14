using Microsoft.AspNetCore.Mvc;

namespace zuHause.Controllers
{
    public class MemberApplicationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Test()
        {
            return View();
        }
    }
}
