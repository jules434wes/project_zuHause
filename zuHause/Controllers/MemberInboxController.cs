using Microsoft.AspNetCore.Mvc;

namespace zuHause.Controllers
{
    public class MemberInboxController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PrivateMessage()
        {
            return View();
        }
    }
}
