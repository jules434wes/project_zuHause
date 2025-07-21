using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using zuHause.Models;
using zuHause.Services;
using zuHause.ViewModels.MemberViewModel;



namespace zuHause.ViewComponents
{
    public class ApplyRentalModalViewComponent : ViewComponent
    {

        public readonly ZuHauseContext _context;
        public ApplyRentalModalViewComponent(ZuHauseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int propertyId, int userId)
        {
            var cities = await _context.Cities.Select(c => new SelectListItem
            {
                Value = c.CityId.ToString(),
                Text = c.CityName
            }).ToListAsync();

            Property? property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
            {
                return View();
            }

            Member? member = await _context.Members.FindAsync(userId);



            //要防呆，要身分認證後才可以申請，否則跳出提示
            if(member == null)
            {
                return View();
            }

            RentalApplicationViewModel model = new RentalApplicationViewModel
            {
                PropertyId = propertyId,
                CityOptions = cities,
                AddressLine = property!.AddressLine,
                MonthlyRent = property.MonthlyRent,
                MemberName = member.MemberName,
                BirthDate = member.BirthDate,
                NationalIdNo = member.NationalIdNo,

            };


            return View(model);
        }
    }
}
