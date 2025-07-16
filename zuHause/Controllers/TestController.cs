using Microsoft.AspNetCore.Mvc;

namespace zuHause.Controllers
{
    public class TestController : Controller
    {
        public IActionResult ApiTest()
        {
            return View();
        }
    }
}