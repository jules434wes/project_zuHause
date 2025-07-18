using Microsoft.AspNetCore.Mvc;

namespace zuHause.ViewComponents
{
    public class MemberContractCardViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke()
        {

            return View();
        }

    }
}
