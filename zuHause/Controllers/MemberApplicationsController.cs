using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using zuHause.Configs;
using zuHause.Models;
using zuHause.Services;
using zuHause.ViewModels.MemberViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace zuHause.Controllers
{
    public class MemberApplicationsController : Controller
    {
        public readonly ZuHauseContext _context;
        public readonly ApplicationService _applicationService;
        public readonly NotificationService _notificationService;
        public readonly ImageResolverService _imageResolverService;
        public MemberApplicationsController(ZuHauseContext context, ApplicationService applicationService, NotificationService notificationService, ImageResolverService imageResolverService)
        {
            _context = context;
            _applicationService = applicationService;
            _notificationService = notificationService;
            _imageResolverService = imageResolverService;
        }


        public async Task<IActionResult> Index(string type = "ALL", string? statusFilter = null)
        {
            ViewBag.type = type;
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login", "Member");

            int memberId = int.Parse(User.FindFirst("UserId")!.Value);


            var viewModel = await _context.RentalApplications
                .Where(app => app.MemberId == memberId && app.IsActive && app.DeletedAt == null)
                .Where(app => (type == "ALL" || type == "") && (app.ApplicationType == "HOUSE_VIEWING" || app.ApplicationType == "RENTAL") ||
                                (type == "HOUSE_VIEWING" && app.ApplicationType == "HOUSE_VIEWING") ||
                                (type == "RENTAL" && app.ApplicationType == "RENTAL"))
                .Where(app => string.IsNullOrEmpty(statusFilter) || app.CurrentStatus == statusFilter)
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
                }).OrderByDescending(x => x.ApplicationId)
                .ToListAsync();


            foreach(ApplicationRecordViewModel item in viewModel)
            {
            string ImgUrl = await _imageResolverService.GetImageUrl(item.PropertyId, "Property", "Gallery", "medium");
                item.imgPath = ImgUrl;
            }



            var applicationStatusCodes = await _context.SystemCodes
            .Where(s => s.CodeCategory == "USER_APPLY_STATUS" && s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToDictionaryAsync(s => s.Code, s => s.CodeName);
            //變成字典例如："APPLIED","已申請"

            List<string>? allowedStatusCodes = null;
            if (type == "HOUSE_VIEWING" || type == "RENTAL" || type == "REJECTED_FLOW")
            {
                allowedStatusCodes = ApplicationFlowConfig.ApplicationStepsMap.GetValueOrDefault(type);
                // 取出對應清單
            }

            Dictionary<string, string> filteredStatusDict;
            if (allowedStatusCodes is not null)
            {
                filteredStatusDict = applicationStatusCodes
                    .Where(kv => allowedStatusCodes.Contains(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
            }
            else
            {
                filteredStatusDict = applicationStatusCodes; // 如果有狀態就篩選符合List的列表出來，也存成字典
            }
            ViewBag.ApplicationStatusOptions = filteredStatusDict;


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

            if (!ModelState.IsValid)
            {
                //確認房源會拿到的網址後跳轉
                return Redirect(model.ReturnUrl ?? "/MemberApplications/Index");
            }

            var userId = int.Parse(User.FindFirst("UserId")!.Value);



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

            var newLog = new ApplicationStatusLog
            {
                ApplicationId = application.ApplicationId,
                StatusCode = "APPLIED",
                ChangedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            _context.ApplicationStatusLogs.Add(newLog);
            await _context.SaveChangesAsync();

            int propertyId = model.PropertyId;

            var property = await _context.Properties.FindAsync(propertyId);
            var member = await _context.Members.FindAsync(userId);


            var (success, message) = await _notificationService.CreateUserNotificationAsync
                (
                receiverId: property!.LandlordMemberId,
                typeCode: "APPLY_VIEW",
                title: $"【{property!.Title}】收到新的看房申請！申請人：【{member!.MemberName}】",
                content:
                    $"親愛的房東您好，\n會員【{member!.MemberName}】想預約看房您的【{property!.Title}】物件\n預約日期：【{application.ScheduleTime!.Value.ToString("yyyy-MM-dd HH:mm")}】\n申請留言：{application.Message}",
                ModuleCode: "ApplyHouse",
                sourceEntityId: application.ApplicationId // 申請編號
                );


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

            var newLog = new ApplicationStatusLog
            {
                ApplicationId = rentalApp.ApplicationId,
                StatusCode = "APPLIED",
                ChangedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            _context.ApplicationStatusLogs.Add(newLog);
            await _context.SaveChangesAsync();


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

            int propertyId = model.PropertyId;

            var property = await _context.Properties.FindAsync(propertyId);
            var member = await _context.Members.FindAsync(userId);

            var (success, message) = await _notificationService.CreateUserNotificationAsync
            (
            receiverId: property!.LandlordMemberId,
            typeCode: "APPLY_VIEW",
            title: $"【{property!.Title}】收到新的租賃申請！申請人：【{member!.MemberName}】",
            content:
                $"親愛的房東您好，\n會員【{member!.MemberName}】想申請租賃您的【{property!.Title}】物件\n租賃期間：【{rentalApp.RentalStartDate} 至 {rentalApp.RentalEndDate}】\n申請留言：{rentalApp.Message}",
            ModuleCode: "ApplyRental",
            sourceEntityId: rentalApp.ApplicationId // 申請編號
            );




            return RedirectToAction("Index", "MemberApplications");
        }





        //創建一個路由，用來更改某筆合約的狀態，傳兩個參數，第一個是哪一筆申請，第二個是改成甚麼狀態，要寫到log

        [HttpPost]
        public async Task<IActionResult> UpdateApplicationLog([FromBody] UpdateApplicationViewModel model)
        {

            var (success, message) = await _applicationService.UpdateApplicationStatusAsync(
            Convert.ToInt32(model.ApplicationId), model.Status!
            );

            if (!success)
                return BadRequest(new { msg = message });

            return Ok(new { msg = message });
        }


    }
}


