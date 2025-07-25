using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using zuHause.Models;
using zuHause.Services;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.Controllers
{
    [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
    //[Authorize(Roles = "1", AuthenticationSchemes = "MemberCookieAuth")]
    public class MemberContractsController : Controller
    {
        public readonly ZuHauseContext _context;
        private readonly IConverter _converter;
        public readonly ApplicationService _applicationService;
        public MemberContractsController(ZuHauseContext context, IConverter converter, ApplicationService applicationService)
        {
            _context = context;
            _converter = converter;
            _applicationService = applicationService;

        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Member");
            }

            int userId = Convert.ToInt32(User.FindFirst("UserId")!.Value);
            System.Diagnostics.Debug.WriteLine($"===={userId}===");
            //查詢申請租賃紀錄
            IQueryable<RentalApplication> rentalLog = _context.RentalApplications.Where(r => r.ApplicationType == "RENTAL" && r.MemberId == userId);

            var contractsLog = await rentalLog.Join(_context.Contracts,
                r => r.ApplicationId,
                c => c.RentalApplicationId,
                (r, c) => new { r, c }).Join(_context.FurnitureOrders,
                rc => rc.r.PropertyId,
                f => f.PropertyId,
                (rc, f) => new { rc, f }).Join(_context.Properties,
                rcf => rcf.rc.r.PropertyId,
                p => p.PropertyId,
                (rcf, p) => new { rcf, p }).Join(_context.Members,
                rcfp => rcfp.rcf.rc.r.MemberId,
                m => m.MemberId,
                (rcfp, m) => new ContractsViewModel
                {
                    ApplicationId = rcfp.rcf.rc.r.ApplicationId, // 申請ID
                    ApplicationType = rcfp.rcf.rc.r.ApplicationType, // 申請類型(租賃/看房)
                    MemberId = rcfp.rcf.rc.r.MemberId, // 會員ID
                    PropertyId = rcfp.rcf.rc.r.PropertyId, // 房源編號
                    CurrentStatus = rcfp.rcf.rc.r.CurrentStatus, // 申請狀態
                    ContractId = rcfp.rcf.rc.c.ContractId, // 合約ID
                    StartDate = rcfp.rcf.rc.c.StartDate, // 起始時間
                    EndDate = rcfp.rcf.rc.c.EndDate, // 結束時間
                    Status = rcfp.rcf.rc.c.Status, // 合約狀態
                    CustomName = rcfp.rcf.rc.c.CustomName, // 合約備註
                    HaveFurniture = rcfp.rcf.f.FurnitureOrderId, // 如果有代表有家具
                    LandlordMemberId = rcfp.p.LandlordMemberId, // 房東ID
                    PublishedAt = rcfp.p.PublishedAt, // 房東ID
                    Title = rcfp.p.Title, // 
                    AddressLine = rcfp.p.AddressLine,
                    MonthlyRent = rcfp.p.MonthlyRent,
                    PreviewImageUrl = rcfp.p.PreviewImageUrl,
                    StatusDisplayName = GetStatusDisplayName(rcfp.rcf.rc.c.Status),
                    ApplicantName = m.MemberName,
                    ApplicantBath = m.BirthDate,
                })
                .ToListAsync();


            foreach (var log in contractsLog)
            {

                System.Diagnostics.Debug.WriteLine($"===={log.StatusDisplayName}===");

            }

            List<SystemCode> contractStatus = await _context.SystemCodes.Where(s => s.CodeCategory == "CONTRACT_STATUS").ToListAsync();

            foreach (SystemCode item in contractStatus)
            {
                item.CodeName = GetStatusDisplayName(item.Code);
            }
            ViewBag.codeCategory = contractStatus;





            return View(contractsLog);
        }

        public static string GetStatusDisplayName(string? code)
        {
            return code switch
            {
                "ACTIVE" => "進行中",
                "EXPIRED" => "已到期",
                "RENEWABLE" => "可續約",
                "TERMINATED" => "已終止",
                "RELISTABLE" => "已下架",
                "BESIGNED" => "待簽約",
                "SIGNED" => "已簽約",
                _ => "狀態名稱"
            };
        }


        public async Task<IActionResult> ContractProduction(int applicationId = 20)
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

            var templates = await _context.ContractTemplates
                .Select(t => new SelectListItem
                {
                    Value = t.ContractTemplateId.ToString(),
                    Text = t.TemplateName
                }).ToListAsync();



            if (application == null)
                return NotFound();

            var vm = new ContractFormViewModel
            {
                TemplateOptions = templates,
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

                model.TemplateOptions = await _context.ContractTemplates
                    .Select(t => new SelectListItem { Value = t.ContractTemplateId.ToString(), Text = t.TemplateName })
                    .ToListAsync();


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
                Status = "BESIGNED",
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
            if (model.SignatureFile != null)
            {
                var file = model.SignatureFile;
                string storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                string filePath = Path.Combine("wwwroot/uploads/signatures", storedFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }



                var upload = new UserUpload
                {
                    MemberId = memberId,
                    ModuleCode = "CONTRACT",
                    SourceEntityId = contractId,
                    UploadTypeCode = "LANDLORD_SIGNATURE",
                    OriginalFileName = file.FileName,
                    StoredFileName = storedFileName,
                    FileExt = Path.GetExtension(file.FileName),
                    MimeType = file.ContentType,
                    FilePath = $"/uploads/signatures/{storedFileName}",
                    FileSize = file.Length,
                    UploadedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsActive = true
                };

                _context.UserUploads.Add(upload);
                await _context.SaveChangesAsync();

                // 新增對應的 ContractSignature 資料
                var contractSignature = new ContractSignature
                {
                    ContractId = contractId,
                    SignerId = memberId,
                    SignerRole = "LANDLORD", // 或 "TENANT"，你可以根據實際使用者判斷
                    SignMethod = "UPLOAD",
                    SignatureFileUrl = upload.FilePath,
                    UploadId = upload.UploadId,
                    SignedAt = now
                };

                _context.ContractSignatures.Add(contractSignature);

            }



            await _context.SaveChangesAsync();

            //還沒決定好會去哪一頁    
            return RedirectToAction("ContractList");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTenantSign(TenantSignViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Content("上傳失敗，請檢查檔案或申請");
            }

            var memberId = int.Parse(User.FindFirst("UserId")!.Value);
            var now = DateTime.Now;

            // 儲存簽名檔案（UserUpload）
            if (model.SignatureFile != null)
            {
                var file = model.SignatureFile;
                string storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                string filePath = Path.Combine("wwwroot/uploads/signatures", storedFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }



                var upload = new UserUpload
                {
                    MemberId = memberId,
                    ModuleCode = "CONTRACT",
                    SourceEntityId = model.ContractId,
                    UploadTypeCode = "TENANT_SIGNATURE",
                    OriginalFileName = file.FileName,
                    StoredFileName = storedFileName,
                    FileExt = Path.GetExtension(file.FileName),
                    MimeType = file.ContentType,
                    FilePath = $"/uploads/signatures/{storedFileName}",
                    FileSize = file.Length,
                    UploadedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsActive = true
                };

                _context.UserUploads.Add(upload);
                await _context.SaveChangesAsync();

                // 新增對應的 ContractSignature 資料
                var contractSignature = new ContractSignature
                {
                    ContractId = model.ContractId,
                    SignerId = memberId,
                    SignerRole = "TENANT", // 或 "TENANT"，你可以根據實際使用者判斷
                    SignMethod = "UPLOAD",
                    SignatureFileUrl = upload.FilePath,
                    UploadId = upload.UploadId,
                    SignedAt = now
                };

                _context.ContractSignatures.Add(contractSignature);

            }



            await _context.SaveChangesAsync();
            await _applicationService.UpdateApplicationStatusAsync(model.RentalApplicationId!.Value, "SIGNING");

            return RedirectToAction("Index", "MemberApplications");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateContractName([FromBody] ContractNameDto data)
        {
            int contractId = data.ContractId;
            string contractName = data.ContractName!;
            System.Diagnostics.Debug.WriteLine($"===={contractId}===");
            System.Diagnostics.Debug.WriteLine($"===={contractName}===");
            Contract? result = await _context.Contracts.Where(c => c.ContractId == contractId).FirstOrDefaultAsync();
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
            var html = await GenerateContractHtml(contractId);
            ViewBag.ContractHtml = html;
            return View(contractId);
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
                     .ThenInclude(a => a.Member) // ✅ 加這行
                 .Include(c => c.RentalApplication)
                     .ThenInclude(a => a.Property)
                         .ThenInclude(p => p.LandlordMember)
                 .Include(c => c.Template)
                 .FirstOrDefaultAsync(c => c.ContractId == contractId);

            // 備註條件區塊處理
            string commentBlock = "";
            if (contract.ContractComments.Any())
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
            return File(file, "application/pdf", $"Contract_{contractId}.pdf");
        }




    }
    public class ContractNameDto
    {
        public int ContractId { get; set; }
        public string? ContractName { get; set; }
    }




}
