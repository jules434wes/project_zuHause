using Microsoft.AspNetCore.Mvc;

namespace zuHause.ViewModels.MemberViewModel
{
    public class MemberProfileViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
