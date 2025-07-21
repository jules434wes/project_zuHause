using Microsoft.AspNetCore.Mvc;

namespace zuHause.ViewComponents
{
    public class SignContractModal : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
