using Microsoft.AspNetCore.Mvc;

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
            return View();
        }
        public IActionResult admin_propertiesList()
        {
            return View();
        }
        
        public IActionResult admin_userDetails()
        {
            return View();
        }

        public IActionResult admin_propertyDetails()
        {
            return View();
        }

        public IActionResult admin_customerServiceList()
        {
            return View();
        }

        public IActionResult admin_customerServiceDetails()
        {
            return View();
        }

        public IActionResult admin_systemMessageList()
        {
            return View();
        }

    }
}
