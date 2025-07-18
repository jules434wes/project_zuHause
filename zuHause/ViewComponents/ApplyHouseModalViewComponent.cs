using Microsoft.AspNetCore.Mvc;

namespace zuHause.ViewComponents
{
    public class ApplyHouseModalViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke()
        {
            return View();
        }

    }
}
