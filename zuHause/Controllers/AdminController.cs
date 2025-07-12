using Microsoft.AspNetCore.Mvc;

namespace zuHause.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult admin_userslist()
        {
            return View();
        }


    }
}
