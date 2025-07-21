using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using zuHause.Models;
using zuHause.Services;
using zuHause.ViewModels.MemberViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace zuHause.Controllers
{
    public class MemberApplicationsController : Controller
    {
        public readonly ZuHauseContext _context;
        public MemberApplicationsController(ZuHauseContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login", "Member");

            int memberId = int.Parse(User.FindFirst("UserId")!.Value);


            var viewModel = await _context.RentalApplications
                .Where(app => app.MemberId == memberId && app.IsActive && app.DeletedAt == null)
                .Include(app => app.ApplicationStatusLogs)
                .Include(app => app.Contracts)
                .Include(app => app.Property)
                .AsSplitQuery()
                .Select(app => new ApplicationRecordViewModel
                {
                    ApplicationId = app.ApplicationId,
                    ApplicationType = app.ApplicationType,
                    CreatedAt = app.CreatedAt,
                    ScheduleTime = app.ScheduleTime,
                    RentalStartDate = app.RentalStartDate,
                    RentalEndDate = app.RentalEndDate,

                    // 房源資訊
                    PropertyId = app.Property.PropertyId,
                    LandlordMemberId = app.Property.LandlordMemberId,
                    Title = app.Property.Title,
                    AddressLine = app.Property.AddressLine,
                    MonthlyRent = app.Property.MonthlyRent,

                    // 只抓第一筆合約（若有）
                    ContractId = app.Contracts
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => (int?)c.ContractId)
                    .FirstOrDefault(),

                    // 狀態歷程，依照 ChangedAt 排序
                    StatusLogs = app.ApplicationStatusLogs
                        .OrderBy(log => log.ChangedAt)
                        .Select(log => new ApplicationStatusLogViewModel
                        {
                            StatusLogId = log.StatusLogId,
                            StatusCode = log.StatusCode,
                            ChangedAt = log.ChangedAt
                        }).ToList()
                })
                .ToListAsync();

            // 查詢申請狀態代碼表
            var applicationStatusCodes = await _context.SystemCodes
                .Where(s => s.CodeCategory == "USER_APPLY_STATUS" && s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            // 使用 ViewBag 傳到 View
            ViewBag.ApplicationStatusOptions = applicationStatusCodes;




            return View(viewModel);
        }

        public IActionResult Test()
        {
            return View();
        }

        // 申請看房
        [HttpPost]
        public async Task<IActionResult> CreateApplyHouse(ApplicationViewModel model)
        {


            System.Diagnostics.Debug.WriteLine($"===={model.ScheduleDate}===");
            System.Diagnostics.Debug.WriteLine($"===={model.ScheduleHouse}");
            System.Diagnostics.Debug.WriteLine($"===={model.ScheduleMinute}");
            System.Diagnostics.Debug.WriteLine($"===={model.ScheduleTime}");
            System.Diagnostics.Debug.WriteLine($"===={Convert.ToInt32(model.PropertyId)}");
            System.Diagnostics.Debug.WriteLine($"===={"11111111111"}===");
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"===={"2222222"}===");

                //確認房源會拿到的網址後跳轉
                return Redirect(model.ReturnUrl ?? "/MemberApplications/Index");
            }

            var userId = int.Parse(User.FindFirst("UserId")!.Value);

            System.Diagnostics.Debug.WriteLine($"===={"3333333"}===");


            var scheduledDateTime = model.ScheduleDate;

            var application = new RentalApplication
            {
                ApplicationType = "HOUSE_VIEWING",
                MemberId = userId,
                PropertyId = Convert.ToInt32(model.PropertyId),
                Message = model.Message,
                ScheduleTime = model.ScheduleTime,
                RentalStartDate = model.RentalStartDate,
                RentalEndDate = model.RentalEndDate,
                CurrentStatus = "APPLIED",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = true,
            };
            _context.RentalApplications.Add(application);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // 申請租賃
        [HttpPost]
        public async Task<IActionResult> ApplyRental(RentalApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["FormError"] = "請確認所有欄位皆正確填寫";

                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {

                        System.Diagnostics.Debug.WriteLine($"欄位 {key} 發生錯誤：{error.ErrorMessage}");
                    }
                }

                return RedirectToAction("Index", "MemberApplications");
            }

            int userId = int.Parse(User.FindFirst("UserId")!.Value);


            var cityName = _context.Cities.FirstOrDefault(c => c.CityId == model.SelectedCityId)?.CityName;
            var districtName = _context.Districts.FirstOrDefault(d => d.DistrictId == model.SelectedDistrictId)?.DistrictName;

            var fullAddress = $"{cityName}{districtName}{model.AddressDetail}";



            var rentalApp = new RentalApplication
            {
                MemberId = userId,
                PropertyId = model.PropertyId,
                Message = model.Message,
                RentalStartDate = model.RentalStartDate,
                RentalEndDate = model.RentalEndDate,
                HouseholdAddress = fullAddress,
                ScheduleTime = null,
                ApplicationType = "RENTAL",
                CurrentStatus = "APPLIED",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = true
            };

            _context.RentalApplications.Add(rentalApp);
            await _context.SaveChangesAsync(); // 先存入，取得 ApplicationId 當作 SourceEntityId


            string folder = "rentalApply";
            string uploadPath = Path.Combine("wwwroot", "uploads", folder);
            Directory.CreateDirectory(uploadPath);

            foreach (var file in model.UploadFiles)
            {

                if (file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName);
                var storeFileName = Guid.NewGuid() + ext;
                var savePath = Path.Combine(uploadPath, storeFileName);
                var relativePath = $"/uploads/{folder}/{storeFileName}";

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var upload = new UserUpload
                {
                    MemberId = userId,
                    ModuleCode = "ApplyRental",
                    SourceEntityId = rentalApp.ApplicationId,
                    UploadTypeCode = "TENANT_APPLY",
                    OriginalFileName = file.FileName,
                    StoredFileName = storeFileName,
                    FileExt = ext,
                    MimeType = file.ContentType,
                    FilePath = relativePath,
                    FileSize = file.Length,
                    IsActive = true,
                    UploadedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.UserUploads.Add(upload);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "MemberApplications");
        }




    }
}
