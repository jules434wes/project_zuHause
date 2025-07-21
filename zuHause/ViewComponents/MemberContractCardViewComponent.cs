using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.ViewComponents
{
    public class MemberContractCardViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke(ContractsViewModel model)
        {

            return View(model);
        }

    }
}
