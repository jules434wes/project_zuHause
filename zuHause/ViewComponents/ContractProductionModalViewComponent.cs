using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.ViewComponents
{
    public class ContractProductionModalViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke()
        {

            return View();
        }
    }
}
