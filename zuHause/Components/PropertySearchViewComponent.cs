using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.ViewModels;

namespace zuHause.Components
{
    public class PropertySearchViewComponent : ViewComponent
    {
        private readonly ZuHauseContext _context;

        public PropertySearchViewComponent(ZuHauseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(PropertySearchViewModel? searchModel = null)
        {
            var viewModel = new PropertySearchFormViewModel
            {
                SearchCriteria = searchModel ?? new PropertySearchViewModel(),
                Cities = await _context.Cities
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CityName)
                    .ToListAsync(),
                Districts = searchModel?.CityId.HasValue == true
                    ? await _context.Districts
                        .Where(d => d.CityId == searchModel.CityId && d.IsActive)
                        .OrderBy(d => d.DistrictName)
                        .ToListAsync()
                    : new List<zuHause.Models.District>()
            };

            return View(viewModel);
        }
    }

    public class PropertySearchFormViewModel
    {
        public PropertySearchViewModel SearchCriteria { get; set; } = new PropertySearchViewModel();
        public List<zuHause.Models.City> Cities { get; set; } = new List<zuHause.Models.City>();
        public List<zuHause.Models.District> Districts { get; set; } = new List<zuHause.Models.District>();
    }
}