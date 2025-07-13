using Microsoft.AspNetCore.Mvc;

namespace zuHause.Controllers
{
    public class MemberContractsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ContractProduction()
        {
            return View();
        }
    }
}
