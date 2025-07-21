using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.ViewComponents
{
    public class MemberApplicationCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {

            return View();
        }
    }
}
