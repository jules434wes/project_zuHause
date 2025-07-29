using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using zuHause.DTOs;
using zuHause.Models;
using zuHause.Services;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.Controllers
{
    [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
    public class MemberContractsController : Controller
    {
        public readonly ZuHauseContext _context;
        private readonly IConverter _converter;
        public readonly ApplicationService _applicationService;
        public readonly ImageResolverService _imageResolverService;
        public readonly NotificationService _notificationService;
        public MemberContractsController(ZuHauseContext context, IConverter converter, ApplicationService applicationService, ImageResolverService imageResolverService, NotificationService notificationService)
        {
            _context = context;
            _converter = converter;
            _applicationService = applicationService;
            _imageResolverService = imageResolverService;
            _notificationService = notificationService;
        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Member");
            }

            int userId = Convert.ToInt32(User.FindFirst("UserId")!.Value);
            string? role = User.FindFirst(ClaimTypes.Role)?.Value;

            List<ContractsViewModel> contractsLog;

            var codeDict = await _context.SystemCodes
                .Where(s => s.CodeCategory == "CONTRACT_STATUS")
                .ToDictionaryAsync(s => s.Code, s => s.CodeName);







            if (role == "1") // 房客
            {


                System.Diagnostics.Debug.WriteLine($"===========房客=================");
                contractsLog = await _context.RentalApplications
                    .Where(r => r.ApplicationType == "RENTAL" && r.MemberId == userId)
                    .Join(_context.Contracts,
                        r => r.ApplicationId,
                        c => c.RentalApplicationId,
                        (r, c) => new { r, c })
                    .Join(_context.Properties,
                        rc => rc.r.PropertyId,
                        p => p.PropertyId,
                        (rc, p) => new { rc, p })
                    .Join(_context.Members,
                        rcp => rcp.rc.r.MemberId,
                        m => m.MemberId,
                        (rcp, m) => new ContractsViewModel
                        {
                            ApplicationId = rcp.rc.r.ApplicationId,
                            ApplicationType = rcp.rc.r.ApplicationType,
                            MemberId = rcp.rc.r.MemberId,
                            PropertyId = rcp.rc.r.PropertyId,
                            CurrentStatus = rcp.rc.r.CurrentStatus,
                            ContractId = rcp.rc.c.ContractId,
                            StartDate = rcp.rc.c.StartDate,
                            EndDate = rcp.rc.c.EndDate,
                            Status = rcp.rc.c.Status,
                            CustomName = rcp.rc.c.CustomName,
                            HaveFurniture = _context.FurnitureOrders
                                .Any(f => f.PropertyId == rcp.rc.r.PropertyId) ? "是" : null,

                            LandlordMemberId = rcp.p.LandlordMemberId,
                            PublishedAt = rcp.p.PublishedAt,
                            Title = rcp.p.Title,
                            AddressLine = rcp.p.AddressLine,
                            MonthlyRent = rcp.p.MonthlyRent,
                            PreviewImageUrl = rcp.p.PreviewImageUrl,
                            StatusDisplayName = GetStatusDisplayName(rcp.rc.c.Status, codeDict),
                            ApplicantName = m.MemberName,
                            ApplicantBath = m.BirthDate,
                        })
                    .OrderByDescending(x => x.ContractId)
                    .ToListAsync();


            }
            else if (role == "2") // 房東
            {
                System.Diagnostics.Debug.WriteLine($"===========房東=================");
                contractsLog = await _context.Contracts
                    .Where(c => c.RentalApplication != null &&
                                c.RentalApplication.Property.LandlordMemberId == userId)
                    .Select(c => new ContractsViewModel
                    {
                        ApplicationId = c.RentalApplication!.ApplicationId,
                        ApplicationType = c.RentalApplication.ApplicationType,
                        MemberId = c.RentalApplication.MemberId,
                        PropertyId = c.RentalApplication.PropertyId,
                        CurrentStatus = c.RentalApplication.CurrentStatus,
                        ContractId = c.ContractId,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status,
                        CustomName = c.CustomName,
                        HaveFurniture = _context.FurnitureOrders
                            .Where(f => f.PropertyId == c.RentalApplication.PropertyId)
                            .Select(f => f.FurnitureOrderId)
                            .FirstOrDefault(), // 若沒有就為0
                        LandlordMemberId = c.RentalApplication.Property.LandlordMemberId,
                        PublishedAt = c.RentalApplication.Property.PublishedAt,
                        Title = c.RentalApplication.Property.Title,
                        AddressLine = c.RentalApplication.Property.AddressLine,
                        MonthlyRent = c.RentalApplication.Property.MonthlyRent,
                        PreviewImageUrl = c.RentalApplication.Property.PreviewImageUrl,
                        StatusDisplayName = GetStatusDisplayName(c.Status,codeDict),
                        ApplicantName = c.RentalApplication.Member.MemberName,
                        ApplicantBath = c.RentalApplication.Member.BirthDate,
                    }).OrderByDescending(x => x.ContractId)
                    .ToListAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"===========未知身分=================");
                contractsLog = new List<ContractsViewModel>(); // 若身分未知則給空
            }

            // 顯示代碼轉換
            List<SystemCode> contractStatus = await _context.SystemCodes
                .Where(s => s.CodeCategory == "CONTRACT_STATUS")
                .ToListAsync();

            foreach (SystemCode item in contractStatus)
            {
                item.CodeName = GetStatusDisplayName(item.Code, codeDict);
            }

            ViewBag.codeCategory = contractStatus;



            string? filterStatus = Request.Query["statusFilter"];

            if (!string.IsNullOrEmpty(filterStatus))
            {
                contractsLog = contractsLog
                    .Where(c => c.Status == filterStatus)
                    .ToList();
            }

            foreach(ContractsViewModel item in contractsLog)
            {
                string ImgUrl = await _imageResolverService.GetImageUrl(item.PropertyId, "Property", "Gallery", "medium");
                item.imgPath = ImgUrl;
            }


            return View(contractsLog);
        }

        public static string GetStatusDisplayName(string? code, Dictionary<string, string> codeDict)
        {
            if (string.IsNullOrEmpty(code)) return "狀態不明";
            return codeDict.TryGetValue(code, out var name) ? name : "未知狀態";
        }

        [HttpPost]
        public async Task<IActionResult> InboxAgreeApply(string type,int applicationId)
        {
            if(type== "ApplyRental")
            {
                await _applicationService.UpdateApplicationStatusAsync(applicationId, "WAITING_CONTRACT");

                return RedirectToAction("ContractProduction", new { applicationId = applicationId });
            }
            else
            {
                await _applicationService.UpdateApplicationStatusAsync(applicationId, "APPROVED");
                return RedirectToAction("Index", "MemberInbox");
            }
        }
        [HttpPost]
        public async Task<IActionResult> InboxRejectedApply(string type,int applicationId)
        {
            await _applicationService.UpdateApplicationStatusAsync(applicationId, "REJECTED");
            return RedirectToAction("Index", "MemberInbox");
        }



        [HttpPost]
        public async Task<IActionResult> UpdateStatusApi([FromBody] ApplicationStatusDto dto)
        {
            await _applicationService.UpdateApplicationStatusAsync(dto.ApplicationId, dto.Status);
            return Ok(new { message = "狀態更新成功" });
        }



        public async Task<IActionResult> ContractProduction(int applicationId)
        {
            var application = await _context.RentalApplications
                .Include(a => a.Member) // 租客
                .Include(a => a.Property) // 房源
                .ThenInclude(p => p.LandlordMember) // 房東
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            var cities = await _context.Cities.Select(c => new SelectListItem
            {
                Value = c.CityId.ToString(),
                Text = c.CityName
            }).ToListAsync();

            var latestTemplate = await _context.ContractTemplates
                .OrderByDescending(t => t.ContractTemplateId)
                .FirstOrDefaultAsync();



            if (application == null)
                return NotFound();

            string ImgUrl = await _imageResolverService.GetImageUrl(application.PropertyId, "Property", "Gallery", "medium");

            var vm = new ContractFormViewModel
            {
                SelectedTemplateId = latestTemplate!.ContractTemplateId,
                RentalApplicationId = application.ApplicationId,
                PropertyId = application.Property.PropertyId,
                Title = application.Property.Title,
                LandlordName = application.Property.LandlordMember.MemberName,
                LandlordMemberId = application.Property.LandlordMember.MemberId,
                LandlordNationalIdNo = application.Property.LandlordMember.NationalIdNo,
                LandlordBirthDate = application.Property.LandlordMember.BirthDate,
                MonthlyRent = application.Property.MonthlyRent,
                AddressLine = application.Property.AddressLine,
                RentalStartDate = application.RentalStartDate,
                RentalEndDate = application.RentalEndDate,
                CityOptions = cities,
                imgPath = ImgUrl,
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContractProduction(ContractFormViewModel model)
        {




            if (!ModelState.IsValid)
            {
                // 若驗證失敗，回傳城市選單重新載入 View
                model.CityOptions = await _context.Cities
                    .Select(c => new SelectListItem { Value = c.CityId.ToString(), Text = c.CityName })
                    .ToListAsync();

                model.CityOptions = await _context.Cities
                    .Select(c => new SelectListItem { Value = c.CityId.ToString(), Text = c.CityName })
                    .ToListAsync();


                foreach (var entry in ModelState)
                {
                    if (entry.Value?.Errors.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"欄位 {entry.Key} 有錯誤：");
                        foreach (var error in entry.Value.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"- {error.ErrorMessage}");
                        }
                    }
                }

                return View(model);
            }

            var memberId = int.Parse(User.FindFirst("UserId")!.Value);
            var now = DateTime.Now;

            var cityName = await _context.Cities
            .Where(c => c.CityId == model.SelectedCityId)
            .Select(c => c.CityName)
            .FirstOrDefaultAsync();

            var districtName = await _context.Districts
                .Where(d => d.DistrictId == model.SelectedDistrictId)
                .Select(d => d.DistrictName)
                .FirstOrDefaultAsync();

            string fullAddress = $"{cityName}{districtName}{model.AddressDetail}";



            // 儲存主合約資料
            var contract = new Contract
            {
                RentalApplicationId = model.RentalApplicationId!.Value,
                LandlordHouseholdAddress = fullAddress,
                Status = "PENDING",
                CourtJurisdiction = model.CourtJurisdiction,
                IsSublettable = model.IsSublettable,
                UsagePurpose = model.UsagePurpose,
                DepositAmount = model.DepositAmount,
                CleaningFee = model.CleaningFee,
                ManagementFee = model.ManagementFee,
                ParkingFee = model.ParkingFee,
                PenaltyAmount = model.PenaltyAmount,
                StartDate = model.RentalStartDate!.Value,
                EndDate = model.RentalEndDate,
                CreatedAt = now,
                UpdatedAt = now,
                TemplateId = model.SelectedTemplateId,
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync(); // 儲存以取得 ContractId

            int contractId = contract.ContractId;

            // 儲存備註（ContractComment）
            foreach (var c in model.Comments)
            {
                _context.ContractComments.Add(new ContractComment
                {
                    ContractId = contractId,
                    CommentType = c.CommentType,
                    CommentText = c.CommentText,
                    CreatedById = model.LandlordMemberId,
                    CreatedAt = now,
                });
            }

            // 儲存家具清單（ContractFurnitureItem）
            foreach (var f in model.FurnitureItems)
            {
                _context.ContractFurnitureItems.Add(new ContractFurnitureItem
                {
                    ContractId = contractId,
                    FurnitureName = f.FurnitureName,
                    FurnitureCondition = f.FurnitureCondition,
                    Quantity = f.Quantity,
                    RepairChargeOwner = f.RepairChargeOwner,
                    RepairResponsibility = f.RepairResponsibility
                });
            }

            // 儲存上傳檔案（UserUpload）
            if (model.UploadFiles != null)
            {
                foreach (var file in model.UploadFiles)
                {
                    if (file.Length > 0)
                    {
                        string storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        string filePath = Path.Combine("wwwroot/uploads/contracts", storedFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _context.UserUploads.Add(new UserUpload
                        {
                            MemberId = memberId,
                            ModuleCode = "CONTRACT",
                            SourceEntityId = contractId,
                            UploadTypeCode = "LANDLORD_APPLY",
                            OriginalFileName = file.FileName,
                            StoredFileName = storedFileName,
                            FileExt = Path.GetExtension(file.FileName),
                            MimeType = file.ContentType,
                            FilePath = $"/uploads/contracts/{storedFileName}",
                            FileSize = file.Length,
                            UploadedAt = now,
                            CreatedAt = now,
                            UpdatedAt = now,
                            IsActive = true
                        });
                    }
                }
            }

            // 儲存簽名檔案（UserUpload）

            if (!string.IsNullOrEmpty(model.SignatureDataUrl))
            {
                var base64Data = Regex.Match(model.SignatureDataUrl, @"data:image/(?<type>.+?);base64,(?<data>.+)").Groups["data"].Value;
                var imageBytes = Convert.FromBase64String(base64Data);

                string storedFileName = $"{Guid.NewGuid()}.png";
                string filePath = Path.Combine("wwwroot/uploads/signatures", storedFileName);
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                var upload = new UserUpload
                {
                    MemberId = memberId,
                    ModuleCode = "CONTRACT",
                    SourceEntityId = contractId,
                    UploadTypeCode = "LANDLORD_SIGNATURE",
                    OriginalFileName = "signature.png",
                    StoredFileName = storedFileName,
                    FileExt = ".png",
                    MimeType = "image/png",
                    FilePath = $"/uploads/signatures/{storedFileName}",
                    FileSize = imageBytes.Length,
                    UploadedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsActive = true
                };

                _context.UserUploads.Add(upload);
                await _context.SaveChangesAsync();

                var contractSignature = new ContractSignature
                {
                    ContractId = contractId,
                    SignerId = memberId,
                    SignerRole = "LANDLORD",
                    SignMethod = "CANVAS",
                    SignatureFileUrl = upload.FilePath,
                    UploadId = upload.UploadId,
                    SignedAt = now
                };

                _context.ContractSignatures.Add(contractSignature);
            }



            await _context.SaveChangesAsync();

            return RedirectToAction("Preview", new { contractId = contractId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTenantSign(TenantSignViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.SignatureDataUrl))
            {
                return Content("簽名失敗，請重新確認是否有簽名！");
            }

            var memberId = int.Parse(User.FindFirst("UserId")!.Value);
            var now = DateTime.Now;

            // 將 base64 圖片儲存
            var base64Data = Regex.Match(model.SignatureDataUrl, @"data:image/(?<type>.+?);base64,(?<data>.+)").Groups["data"].Value;
            var imageBytes = Convert.FromBase64String(base64Data);

            string storedFileName = $"{Guid.NewGuid()}.png";
            string filePath = Path.Combine("wwwroot/uploads/signatures", storedFileName);
            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            var upload = new UserUpload
            {
                MemberId = memberId,
                ModuleCode = "CONTRACT",
                SourceEntityId = model.ContractId,
                UploadTypeCode = "TENANT_SIGNATURE",
                OriginalFileName = "signature.png",
                StoredFileName = storedFileName,
                FileExt = ".png",
                MimeType = "image/png",
                FilePath = $"/uploads/signatures/{storedFileName}",
                FileSize = imageBytes.Length,
                UploadedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
                IsActive = true
            };

            _context.UserUploads.Add(upload);
            await _context.SaveChangesAsync();

            var contractSignature = new ContractSignature
            {
                ContractId = model.ContractId,
                SignerId = memberId,
                SignerRole = "TENANT",
                SignMethod = "CANVAS",
                SignatureFileUrl = upload.FilePath,
                UploadId = upload.UploadId,
                SignedAt = now
            };

            _context.ContractSignatures.Add(contractSignature);
            await _context.SaveChangesAsync();

            await _applicationService.UpdateApplicationStatusAsync(model.RentalApplicationId!.Value, "WAIT_TENANT_AGREE");

            return RedirectToAction("Preview", "MemberContracts", new { contractId = model.ContractId });
        }



        [HttpPost]
        public async Task<IActionResult> UpdateContractName([FromBody] ContractNameDto data)
        {
            int contractId = data.ContractId;
            string contractName = data.ContractName!;
            System.Diagnostics.Debug.WriteLine($"===={contractId}===");
            System.Diagnostics.Debug.WriteLine($"===={contractName}===");
            Contract result = await _context.Contracts.Where(c => c.ContractId == contractId).FirstOrDefaultAsync();
            if (result == null) return BadRequest("請確認合約編號");
            result.CustomName = contractName;
            _context.Update(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                contractName = contractName
            });
        }
        public async Task<IActionResult> Preview(int contractId)
        {
            var contract = await _context.Contracts
           .Include(c => c.RentalApplication)
           .ThenInclude(a => a!.Property)
           .ThenInclude(p => p.LandlordMember)
           .Include(c => c.RentalApplication!.Member)
           .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            string userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            string? actionType = "PIKA";


            System.Diagnostics.Debug.WriteLine($"➡️ User身分: {userRole}");
            System.Diagnostics.Debug.WriteLine($"➡️ UserId: {userId}");
            System.Diagnostics.Debug.WriteLine($"➡️ 合約申請者ID: {contract.RentalApplication!.MemberId}");
            System.Diagnostics.Debug.WriteLine($"➡️ 合約申請狀態: {contract.RentalApplication.CurrentStatus}");
            //擬定合約
            if (userRole == "2" && contract.RentalApplication!.Property.LandlordMemberId == userId &&
                contract.RentalApplication.CurrentStatus == "WAITING_CONTRACT")
            {
                actionType = "SEND_CONTRACT";
            }

            if (userRole == "1" && contract.RentalApplication!.MemberId == userId &&
            contract.RentalApplication.CurrentStatus == "WAIT_TENANT_AGREE")
            {
                actionType = "TENANT_NEED_AGREE";
            }

            if (userRole == "2" && contract.RentalApplication!.Property.LandlordMemberId == userId &&
                contract.RentalApplication.CurrentStatus == "WAIT_LANDLORD_AGREE")
            {
                actionType = "LANDLORD_NEED_AGREE";
            }

            var html = await GenerateContractHtml(contractId);

            ViewBag.ContractHtml = html;
            ViewBag.type = actionType;

            return View(contractId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendContract(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.RentalApplication)
                .ThenInclude(a => a.Property)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null || !contract.RentalApplicationId.HasValue)
                return BadRequest("找不到對應的合約申請紀錄");

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            if (contract.RentalApplication!.Property.LandlordMemberId != userId)
                return Forbid();

            int applicationId = contract.RentalApplicationId.Value;
            var (success, message) = await _applicationService.UpdateApplicationStatusAsync(applicationId, "SIGNING");

            if (!success)
                return BadRequest(new { msg = message });


            contract.Status = "BESIGNED";
            contract.UpdatedAt = DateTime.Now;
            _context.Update(contract);
            await _context.SaveChangesAsync();



            return RedirectToAction("Index", "MemberContracts", new { msg = message });
        }


        private int CalculateMonthDifference(DateOnly start, DateOnly? end)
        {
            if (end == null) return 0;

            var startDate = new DateTime(start.Year, start.Month, start.Day);
            var endDate = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day);

            int months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
            if (endDate.Day >= startDate.Day) months++;
            return months;
        }

        private async Task<string> GenerateContractHtml(int contractId, bool isDownload = false)
        {
            var contract = await _context.Contracts
                 .Include(c => c.ContractFurnitureItems)
                 .Include(c => c.ContractComments)
                 .Include(c => c.ContractSignatures)
                 .Include(c => c.RentalApplication)
                     .ThenInclude(a => a!.Member) // ✅ 加這行
                 .Include(c => c.RentalApplication)
                     .ThenInclude(a => a!.Property)
                         .ThenInclude(p => p.LandlordMember)
                 .Include(c => c.Template)
                 .FirstOrDefaultAsync(c => c.ContractId == contractId);

            // 備註條件區塊處理
            string commentBlock = "";
            if (contract!.ContractComments.Any())
            {
                var sb = new System.Text.StringBuilder("<ul>");
                foreach (var comment in contract.ContractComments)
                {
                    sb.Append($"<li>{comment.CommentText}</li>");
                }
                sb.Append("</ul>");
                commentBlock = sb.ToString();
            }
            int rentalMonths = CalculateMonthDifference(contract.StartDate, contract.EndDate);

            if (contract == null || contract.Template == null)
                return "<p>合約不存在或無範本</p>";

            // ✅ 使用資料庫中 TemplateContent
            string templateHtml = contract.Template.TemplateContent;

            var landlord = contract.RentalApplication?.Property?.LandlordMember;

            var tenant = contract.RentalApplication?.Member;
            var applicationId = contract.RentalApplicationId;

            var uploads = await _context.UserUploads
                .Where(u => u.IsActive &&
                           (
                               (u.ModuleCode == "ApplyRental" && u.SourceEntityId == applicationId && u.UploadTypeCode == "TENANT_APPLY") ||
                               (u.ModuleCode == "CONTRACT" && u.SourceEntityId == contractId &&
                                u.UploadTypeCode == "LANDLORD_APPLY")
                           ))
                .ToListAsync();



            var landlordId = contract.RentalApplication?.Property?.LandlordMemberId ?? -1;
            var tenantId = contract.RentalApplication?.MemberId ?? -1;
            System.Diagnostics.Debug.WriteLine($"🧾 ContractId: {contractId}");
            System.Diagnostics.Debug.WriteLine($"➡️ LandlordId: {landlordId}");
            System.Diagnostics.Debug.WriteLine($"➡️ TenantId: {tenantId}");

            // 租客上傳：申請時期的附件
            var tenantExtraFiles = uploads
                .Where(u => u.ModuleCode == "ApplyRental" && u.UploadTypeCode == "TENANT_APPLY")
                .Select(u => u.FilePath)
                .ToList();

            // 房東上傳：合約建立時的附件（不含簽名）
            var landlordExtraFiles = uploads
                .Where(u => u.ModuleCode == "CONTRACT" && u.UploadTypeCode == "LANDLORD_APPLY")
                .Select(u => u.FilePath)
                .ToList();

            var request = HttpContext.Request;
            string baseUrl = $"{request.Scheme}://{request.Host}";

            string landlordImagesHtml = string.Join("", landlordExtraFiles.Select(p => $"<img src='{baseUrl}{p}' />"));
            string tenantImagesHtml = string.Join("", tenantExtraFiles.Select(p => $"<img src='{baseUrl}{p}' />"));

            //string testAllUploadsHtml = string.Join("<br>", uploads.Select(u => $"{u.UploadTypeCode} - {u.FilePath} - memberId: {u.MemberId}"));
            string landlordSignature = "";
            string tenantSignature = "";

            if (isDownload)
            {
                landlordImagesHtml = string.Join("",
                landlordExtraFiles.Select(p =>
                {
                    var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", p.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    var fileUri = new Uri(localPath).AbsoluteUri;

                    return System.IO.File.Exists(localPath)
                        ? $"<img src='{fileUri}' />"
                        : $"<p style='color:red'>圖片不存在：{fileUri}</p>";
                }));

                tenantImagesHtml = string.Join("",
                    tenantExtraFiles.Select(p =>
                    {
                        var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", p.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                        var fileUri = new Uri(localPath).AbsoluteUri;

                        return System.IO.File.Exists(localPath)
                            ? $"<img src='{fileUri}' />"
                            : $"<p style='color:red'>圖片不存在：{fileUri}</p>";
                    }));




                var landlordSignaturePath = contract.ContractSignatures
                    .FirstOrDefault(s => s.SignerRole == "LANDLORD")?.SignatureFileUrl;
                if (!string.IsNullOrWhiteSpace(landlordSignaturePath))
                {
                    var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", landlordSignaturePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    var fileUri = new Uri(localPath).AbsoluteUri;

                    landlordSignature = System.IO.File.Exists(localPath)
                        ? fileUri
                        : $"<p style='color:red'>簽名檔不存在：{fileUri}</p>";
                }

            


                var tenantSignaturePath = contract.ContractSignatures
                    .FirstOrDefault(s => s.SignerRole == "TENANT")?.SignatureFileUrl;
                if (!string.IsNullOrWhiteSpace(tenantSignaturePath))
                {
                    var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tenantSignaturePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    var fileUri = new Uri(localPath).AbsoluteUri;

                    tenantSignature = System.IO.File.Exists(localPath)
                        ? fileUri
                        : $"<p style='color:red'>簽名檔不存在：{fileUri}</p>";
                }
            }

            var fields = new Dictionary<string, string>
            {
                // 甲方
                { "{{甲方姓名}}", landlord?.MemberName ?? "" },
                { "{{甲方地址}}", contract.LandlordHouseholdAddress ?? "" },
                { "{{甲方身分證}}", landlord?.NationalIdNo ?? "" },
                { "{{甲方生日}}", landlord?.BirthDate.ToString("yyyy/MM/dd") ?? "" },

                // 乙方
                { "{{乙方姓名}}", tenant?.MemberName ?? "" },
                { "{{乙方身分證}}", tenant?.NationalIdNo ?? "" },
                { "{{乙方地址}}", tenant?.AddressLine ?? "" },
                { "{{乙方生日}}", tenant?.BirthDate.ToString("yyyy/MM/dd") ?? "" },

                // 合約內容
                { "{{租賃起日}}", contract.StartDate.ToString("yyyy/MM/dd") },
                { "{{租賃迄日}}", contract.EndDate?.ToString("yyyy/MM/dd") ?? "" },
                { "{{月租金}}", contract.RentalApplication?.Property?.MonthlyRent.ToString("N0") ?? "" },
                { "{{押金}}", contract.DepositAmount?.ToString("N0") ?? "" },
                { "{{使用目的}}", contract.UsagePurpose ?? "" },
                { "{{糾紛法院}}", contract.CourtJurisdiction ?? "" },
                { "{{違約金}}", contract.PenaltyAmount?.ToString("N0") ?? "" },
                { "{{租賃月數}}", rentalMonths.ToString() },
                { "{{租賃地址}}", contract.RentalApplication?.Property?.AddressLine ?? "" },
                { "{{房源編號}}", contract.RentalApplication?.PropertyId.ToString() ?? "" },
                { "{{備註條件}}", commentBlock ?? "" },
                { "{{家具清單}}", GenerateFurnitureHtml(contract.ContractFurnitureItems.ToList()) },
            };
            // 簽名圖片
            if (isDownload)
            {
                fields.Add("{{甲方簽名圖}}", landlordSignature);
                fields.Add("{{乙方簽名圖}}", tenantSignature);
            }
            else
            {
                fields.Add("{{甲方簽名圖}}", contract.ContractSignatures.FirstOrDefault(s => s.SignerRole == "LANDLORD")?.SignatureFileUrl ?? "");
                fields.Add("{{乙方簽名圖}}", contract.ContractSignatures.FirstOrDefault(s => s.SignerRole == "TENANT")?.SignatureFileUrl ?? "");
            }
            // HTML 頁面顯示圖片的欄位
            fields.Add("{{甲方附件圖片}}", landlordImagesHtml);
            fields.Add("{{乙方附件圖片}}", tenantImagesHtml);


                // 除錯專用測試文字（可選）
                //fields.Add("{{測試列印}}", testAllUploadsHtml);



                foreach (var kv in fields)
                {
                    templateHtml = templateHtml.Replace(kv.Key, kv.Value);
                }

            return templateHtml;
        }


        private string GenerateFurnitureHtml(List<ContractFurnitureItem> items)
        {
            if (items == null || items.Count == 0) return "<p>無提供家具</p>";
            var sb = new System.Text.StringBuilder("<ul>");
            foreach (var item in items)
            {
                sb.Append($"<li>{item.FurnitureName}（數量：{item.Quantity}，狀況：{item.FurnitureCondition}，責任：{item.RepairResponsibility}）</li>");
            }
            sb.Append("</ul>");
            return sb.ToString();
        }





        public async Task<IActionResult> DownloadPdf(int contractId)
        {
            var html = await GenerateContractHtml(contractId,true);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait
        },
                Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            var file = _converter.Convert(doc);
            return File(file, "application/pdf", $"Contract_{contractId}_{DateTime.Today.ToString("yyyy-MM-dd")}.pdf");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgreeContract(int contractId, string type)
        {
            var contract = await _context.Contracts
                .Include(c => c.RentalApplication)
                .ThenInclude(a => a.Property)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            var userId = int.Parse(User.FindFirst("UserId")!.Value);

            System.Diagnostics.Debug.WriteLine($"目前使用者ID：{userId}");
            System.Diagnostics.Debug.WriteLine($"租客應為ID：{contract!.RentalApplication!.MemberId}");
            System.Diagnostics.Debug.WriteLine($"TYPE：{type}");


            if (type == "TENANT_NEED_AGREE" && contract!.RentalApplication!.MemberId == userId)
            {
                await _applicationService.UpdateApplicationStatusAsync(contract!.RentalApplicationId!.Value, "WAIT_LANDLORD_AGREE");
                return RedirectToAction("Index", "MemberApplications");
            }
            else if (type == "LANDLORD_NEED_AGREE" && contract!.RentalApplication!.Property.LandlordMemberId == userId)
            {
                await _applicationService.UpdateApplicationStatusAsync(contract!.RentalApplicationId!.Value, "CONTRACTED");

                contract.Status = "SIGNED";
                contract.UpdatedAt = DateTime.Now;
                _context.Update(contract);
                await _context.SaveChangesAsync();

                var (success, message) = await _notificationService.CreateUserNotificationAsync
                    (
                    receiverId: contract!.RentalApplication!.MemberId,
                    typeCode: "CONTRACT_CONTRACTED",
                    title: $"您申請的【{contract!.RentalApplication!.Property.Title}】租賃合約完成！",
                    content:
                        $"親愛的會員您好，\n您申請的【{contract!.RentalApplication!.Property.Title}】租賃合約完成！可至合約管理下載合約！",
                    ModuleCode: "Contract",
                    sourceEntityId: contract.ContractId // 合約編號
                    );


                return RedirectToAction("Index", "MemberContracts");
            }
            else
            {
                return Forbid();
            }

        }

        public async Task<string> GetPropertyImageUrl(int propertyId)
        {
           string ImgUrl =  await _imageResolverService.GetImageUrl(propertyId, "Property", "Gallery", "medium");

            return ImgUrl;
        }

    }
    public class ContractNameDto
    {
        public int ContractId { get; set; }
        public string? ContractName { get; set; }
    }




}
