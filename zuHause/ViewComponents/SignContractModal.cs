using Microsoft.AspNetCore.Mvc;
using zuHause.Models;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.ViewComponents
{
    public class SignContractModal : ViewComponent
    {
        public IViewComponentResult Invoke(int applicationId , int contractId)
        {

            TenantSignViewModel model = new TenantSignViewModel
            {
                RentalApplicationId = applicationId,
                ContractId = contractId,
            };



            return View(model);
        }
    }
}
