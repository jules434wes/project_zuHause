using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels;

namespace zuHause.Components
{
    public class PropertyImageGalleryViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<PropertyImageViewModel> images)
        {
            return View(images);
        }
    }
}