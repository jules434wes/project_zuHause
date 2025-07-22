using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using zuHause.Models;

namespace zuHause.ViewComponents
{
    public class ViewRentalApplicationsModalViewComponent : ViewComponent
    {
        public readonly ZuHauseContext _context;

        public ViewRentalApplicationsModalViewComponent(ZuHauseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int propertyId, int applicationId)
        {
            var application =  await _context.RentalApplications.Where(r => r.PropertyId == propertyId && r.ApplicationId == applicationId).Include(p=>p.Property).Include(m=>m.Member).FirstOrDefaultAsync();
            if(application == null)
            {
                return Content("查無資料");
            }

            var images = await _context.UserUploads.Where(u => u.ModuleCode == "ApplyRental" && u.UploadTypeCode == "TENANT_APPLY" && u.SourceEntityId == application!.ApplicationId).FirstOrDefaultAsync();
            return View();

            //驗證失敗要回傳甚麼？不回傳
        }
    }
}
