using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels;

namespace zuHause.Components
{
    public class PropertyCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PropertySummaryViewModel property)
        {
            return View(property);
        }
    }
}