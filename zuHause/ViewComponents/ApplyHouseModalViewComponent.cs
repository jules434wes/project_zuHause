using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.ViewComponents
{
    public class ApplyHouseModalViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke(int propertyId, int userId)
        {

            ApplicationViewModel model = new ApplicationViewModel
            {
                PropertyId = propertyId,

            };
            return View(model);
        }



    }
}
