using Microsoft.AspNetCore.Mvc;

namespace zuHause.ViewComponents
{
    public class ApplyRentalModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
