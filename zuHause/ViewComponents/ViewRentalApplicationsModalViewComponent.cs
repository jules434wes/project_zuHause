using Microsoft.AspNetCore.Mvc;

namespace zuHause.ViewComponents
{
    public class ViewRentalApplicationsModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
