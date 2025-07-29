using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Security.Claims;
using zuHause.Models;
using zuHause.Services;
using zuHause.ViewModels.MemberViewModel;
using System.Text.Json;

namespace zuHause.Controllers
{
    public class MemberController : Controller
    {
        public readonly ZuHauseContext _context;
        private readonly IMemoryCache _cache;
        public readonly MemberService _memberService;
        public MemberController(ZuHauseContext context, IMemoryCache cache, MemberService memberService)
        {
            _context = context;
            _cache = cache;
            _memberService = memberService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string? ReturnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index");
            }

            var model = new LoginViewModel()
            {
                ReturnUrl = ReturnUrl
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? userPhoneNumber = model.PhoneNumber;
            string? userPassword = model.UserPassword;
            Member? member;

            try
            {
                member = await _context.Members.SingleOrDefaultAsync((x) => (x.PhoneNumber
             == userPhoneNumber));
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError("LoginStatus", "資料異常，請聯絡客服");
                return View(model);

            }
            catch (Exception)
            {
                ModelState.AddModelError("LoginStatus", "系統錯誤，請稍後再試");
                return View(model);
            }

            // 嘗試用try/catch
            if (member == null)
            {
                ModelState.AddModelError("LoginStatus", "帳號或密碼錯誤");

                return View(model);
            }

            bool isValid;
            try
            {
                isValid = _memberService.verifyPassword(member, model.UserPassword!);
            }
            catch (FormatException)
            {
                ModelState.AddModelError("LoginStatus", "帳號或密碼錯誤");
                return View(model);
            }

            if (!isValid)
            {

                ModelState.AddModelError("LoginStatus", "帳號或密碼錯誤");

                return View(model);
            }

            member.LastLoginAt = DateTime.Now;
            _context.Entry(member).Property(m => m.LastLoginAt).IsModified = true;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, member.MemberName),
                new Claim("Phone",member.PhoneNumber),
                new Claim("UserId",member.MemberId.ToString())

            };

            if (member.MemberTypeId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.Role, member.MemberTypeId.Value.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "MemberCookieAuth");

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false // 瀏覽器關閉後登出，15分鐘閒置自動登出
                // 移除手動過期設定，使用 Program.cs 中的統一配置
            };

            var photoPath = _context.Members.Join(_context.UserUploads,
                m => new { m.MemberId },
                u => new { u.MemberId },
                (m, u) => new
                {
                    MemberId = m.MemberId,
                    UploadId = u.UploadId,
                    PhotoPath = u.FilePath,
                    UploadTypeCode = u.UploadTypeCode,
                }
                ).Where((x) => x.MemberId == member.MemberId && x.UploadTypeCode == "USER_IMG")
                .OrderByDescending(x => x.UploadId)
                .FirstOrDefault()?.PhotoPath;

            if (photoPath != null)
            {
                photoPath = $"~{photoPath}";
            }
            else
            {
                photoPath = "~/images/user-image.jpg";
            }

            _cache.Set($"Avatar_{member.MemberId.ToString()}", photoPath);

            await HttpContext.SignInAsync("MemberCookieAuth",
                new ClaimsPrincipal(claimsIdentity), authProperties);

            // 同時設置 Session 以支援 Stripe 返回認證
            HttpContext.Session.SetInt32("MemberId", member.MemberId);

            TempData["SuccessMessageTitle"] = "通知";
            TempData["SuccessMessageContent"] = "登入成功";

            // 智能重導向邏輯：直接基於 member 物件檢查，避免認證狀態延遲問題
            string redirectUrl;
            if (member.IsLandlord && member.MemberTypeId == 2 && member.IdentityVerifiedAt != null)
            {
                // 已驗證房東 → 房源管理頁面
                redirectUrl = Url.Action("PropertyManagement", "Landlord") ?? "/landlord/propertymanagement";
            }
            else
            {
                // 其他情況使用原有智能重導向邏輯
                redirectUrl = GetSmartRedirectUrl(model.ReturnUrl);
            }
            return Redirect(redirectUrl);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var memberId = User.FindFirst("UserId")?.Value;
            await HttpContext.SignOutAsync("MemberCookieAuth");
            _cache.Remove($"Avatar_{memberId}");


            TempData["SuccessMessageTitle"] = "通知";
            TempData["SuccessMessageContent"] = "您已登出";

            // 使用智能重導向邏輯（登出時沒有 ReturnUrl，純粹根據 Referer 判斷）
            var redirectUrl = GetSmartRedirectUrl(null);
            return Redirect(redirectUrl);
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> EnableLandlordRole()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var member = await _context.Members.FindAsync(userId);
            if (member == null) return Unauthorized();
            
            // 檢查是否已完成身份驗證
            if (!member.IdentityVerifiedAt.HasValue)
            {
                TempData["ErrorMessage"] = "請先完成身份驗證才能成為房東";
                return RedirectToAction("MemberProfile");
            }
            
            // 根據規格文件：同時更新 isLandlord 和 memberTypeID
            member.IsLandlord = true;
            member.MemberTypeId = 2; // 房東類型
            member.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return await SwitchRoleInternal(member, "landlord");
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> SwitchRole(string targetRole)
        {
            string[] validRoles = new String[] { "landlord", "tenant" };
            if (!validRoles.Contains(targetRole))
            {
                return BadRequest("無效的身分");
            }

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var member = await _context.Members.FindAsync(userId);
            if (member == null)
            {
                return View("Login");
            }

            return await SwitchRoleInternal(member, targetRole);

        }

        public async Task<IActionResult> SwitchRoleInternal(Member member, string targetRole)
        {


            string nowRole = "房客";
            string[] validRoles = new String[] { "landlord", "tenant" };
            if (!validRoles.Contains(targetRole))
            {
                return BadRequest("無效的身分");
            }

            if ((targetRole == "landlord" && member.MemberTypeId == 2) ||
                (targetRole == "tenant" && member.MemberTypeId == 1))
            {
                return RedirectToAction("MemberProfile");
            }

            if (targetRole == "landlord" && !member.IsLandlord)
            {
                return Forbid();
            }

            if (targetRole == "landlord")
            {
                member.MemberTypeId = 2;
                nowRole = "房東";

            }
            else if (targetRole == "tenant")
            {
                member.MemberTypeId = 1;
                nowRole = "房客";

            }
            member.UpdatedAt = DateTime.Now;
            _context.Update(member);
            await _context.SaveChangesAsync();



            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, member.MemberName),
                new Claim("Phone",member.PhoneNumber),
                new Claim("UserId",member.MemberId.ToString())
            };
            if (member.MemberTypeId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.Role, member.MemberTypeId.Value.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "MemberCookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false // 瀏覽器關閉後登出，15分鐘閒置自動登出
                // 移除手動過期設定，使用 Program.cs 中的統一配置
            };

            await HttpContext.SignOutAsync("MemberCookieAuth");
            await HttpContext.SignInAsync("MemberCookieAuth",
            new ClaimsPrincipal(claimsIdentity), authProperties);

            // 同時設置 Session 以支援 Stripe 返回認證
            HttpContext.Session.SetInt32("MemberId", member.MemberId);

            TempData["SuccessMessageTitle"] = "切換成功";
            TempData["SuccessMessageContent"] = $"您目前的身分為：{nowRole}";

            return RedirectToAction("MemberProfile");

        }


        public IActionResult ResetPasswordVerifyPhone()
        {
            return View();
        }
        public IActionResult ResetPasswordSendCode()
        {
            return View();
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public IActionResult ResetPasswordConfirmCode()
        {

            return View("ResetPasswordConfirmCode", new ForgotPasswordViewModel());
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> ResetPassword(ForgotPasswordViewModel model)
        {
            if (model.ReturnUrl == null)
            {
                model.ReturnUrl = Url.Action("Index", "Member");
            }

            if (!ModelState.IsValid)
            {
                return View("ResetPasswordConfirmCode", model);
            }

            var member = await _context.Members.FindAsync(int.Parse(User.FindFirst("UserId")!.Value));

            if (member == null) return View("ResetPasswordConfirmCode", model);
            bool result = _memberService.verifyPassword(member, model.OriginalPassword!);

            if (!result)
            {
                ModelState.AddModelError("OriginalPassword", "原密碼錯誤");
                return View("ResetPasswordConfirmCode", model);
            }


            _memberService.ResetPassword(member, model.UserPassword!);


            TempData["SuccessMessageTitle"] = "成功";
            TempData["SuccessMessageContent"] = "密碼修改完成";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index");
        }

        public IActionResult ResetPasswordChange()
        {
            return View();
        }
        [HttpGet]
        public IActionResult RegisterVerifyPhone()
        {

            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index");
            }

            TempData["activePage"] = "VerifyPhone";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterVerifyPhone(VerifyPhoneViewModel member)
        {
            TempData["activePage"] = "VerifyPhone";


            if (!ModelState.IsValid)
            {
                return View(member);
            }


            string phoneNumber = member.PhoneNumber!;
            var result = await _context.Members.Where(m => m.PhoneNumber == phoneNumber).ToListAsync();
            if (result.Count == 0)
            {
                return RedirectToAction("RegisteFillInfomation",new{ phoneNumber}); // 不走手機認證
            }
            else
            {
                ModelState.AddModelError("PhoneNumber", "此號碼已註冊過");
                return View(member);
            }
        }
        [HttpGet]
        public IActionResult RegisterSendCode()
        {

            TempData["activePage"] = "SendCode";

            if (!TempData.ContainsKey("phoneNumber"))
            {
                return RedirectToAction("RegisterVerifyPhone");
            }
            string? _phone = TempData["phoneNumber"]!.ToString();
            VerifyCodeViewModel model = new VerifyCodeViewModel
            {
                PhoneNumber = _phone,
                Verify = 555666,
            };
            return View(model);
        }
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> RegisteFillInfomation(string phoneNumber)
        {

            TempData["activePage"] = "FillInfomation";

            // 驗證手機驗證碼
            //if (!ModelState.IsValid)
            //{
            //    return View("RegisterSendCode", model);
            //}

            // 這裡應該要驗證手機驗證碼，目前使用固定值 555666
            //if (model.Verify != 555666)
            //{
            //    ModelState.AddModelError("Verify", "驗證碼錯誤");
            //    return View("RegisterSendCode", model);
            //}

            // 驗證成功，將手機號碼和驗證狀態存到 TempData
            //TempData["phoneNumber"] = model.PhoneNumber;
            TempData["phoneVerified"] = true; // 標記手機已驗證

            var cities = await _context.Cities.Select(c => new SelectListItem
            {
                Value = c.CityId.ToString(),
                Text = c.CityName
            }).ToListAsync();


            RegisterViewModel memberInfo = new RegisterViewModel
            {
                PhoneNumber = phoneNumber,
                CityOptions = cities,
            };
            return View(memberInfo);
        }

        [HttpPost]
        public async Task<IActionResult> Registe(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {

                TempData["activePage"] = "FillInfomation";

                model.CityOptions = await _context.Cities.Select(c => new SelectListItem
                {
                    Value = c.CityId.ToString(),
                    Text = c.CityName
                }).ToListAsync();

                if (model.ResidenceCity.HasValue)
                {
                    model.ResidenceDistrictOptions = await _context.Districts
                        .Where(d => d.CityId == model.ResidenceCity.Value)
                        .Select(d => new SelectListItem
                        {
                            Value = d.DistrictId.ToString(),
                            Text = d.DistrictName
                        }).ToListAsync();
                }

                if (model.PrimaryRentalCityID.HasValue)
                {
                    model.PrimaryRentalDistrictOptions = await _context.Districts
                        .Where(d => d.CityId == model.PrimaryRentalCityID.Value)
                        .Select(d => new SelectListItem
                        {
                            Value = d.DistrictId.ToString(),
                            Text = d.DistrictName
                        }).ToListAsync();
                }

                return View("RegisteFillInfomation", model);

            }

            DateOnly BirthDate = DateOnly.Parse(model.Birthday!);

            // 檢查手機是否已通過驗證
            bool phoneVerified = TempData["phoneVerified"] as bool? ?? false;
            DateTime? phoneVerifiedTime = phoneVerified ? DateTime.Now : null;

            Member member = new Member
            {
                MemberName = model.UserName!,
                Gender = model.Gender,
                BirthDate = BirthDate,
                PhoneNumber = model.PhoneNumber!,
                Email = model.Email!,
                PrimaryRentalCityId = model.PrimaryRentalCityID,
                PrimaryRentalDistrictId = model.PrimaryRentalDistrictID,
                ResidenceCityId = model.ResidenceCity,
                ResidenceDistrictId = model.ResidenceDistrictID,
                AddressLine = model.AddressLine,
                PhoneVerifiedAt = phoneVerifiedTime, // 設置手機驗證時間
                IsActive = true,
                IsLandlord = false,
                MemberTypeId = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };


            _memberService.Register(member, model.UserPassword!);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, member.MemberName),
                new Claim("Phone",member.PhoneNumber),
                new Claim("UserId",member.MemberId.ToString())

            };

            if (member.MemberTypeId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.Role, member.MemberTypeId.Value.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "MemberCookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false // 瀏覽器關閉後登出，15分鐘閒置自動登出
                // 移除手動過期設定，使用 Program.cs 中的統一配置
            };


            var photoPath = _context.Members.Join(_context.UserUploads,
                m => new { m.MemberId },
                u => new { u.MemberId },
                (m, u) => new
                {
                    MemberId = m.MemberId,
                    UploadId = u.UploadId,
                    PhotoPath = u.FilePath,
                    UploadTypeCode = u.UploadTypeCode,
                }
                ).Where((x) => x.MemberId == member.MemberId && x.UploadTypeCode == "USER_IMG")
                .OrderByDescending(x => x.UploadId)
                .FirstOrDefault()?.PhotoPath;

            if (photoPath != null)
            {
                photoPath = $"~{photoPath}";
            }
            else
            {
                photoPath = "~/images/user-image.jpg";
            }

            _cache.Set($"Avatar_{member.MemberId.ToString()}", photoPath);

            await HttpContext.SignInAsync("MemberCookieAuth",
                new ClaimsPrincipal(claimsIdentity), authProperties);

            // 同時設置 Session 以支援 Stripe 返回認證
            HttpContext.Session.SetInt32("MemberId", member.MemberId);

            TempData["SuccessMessageTitle"] = "註冊成功";
            TempData["SuccessMessageContent"] = "您可以開始體驗會員功能";
            return RedirectToAction("RegisteSuccess");


        }

        [HttpGet]
        public JsonResult GetDistrictsByCity(int cityId)
        {
            var districts = _context.Districts.Where(d => d.CityId == cityId).Select(d => new
            {
                value = d.DistrictId,
                text = d.DistrictName
            }).ToList();
            return Json(districts);
        }

        [Authorize(Roles = "1", AuthenticationSchemes = "MemberCookieAuth")]
        public IActionResult RegisteSuccess()
        {
            TempData["activePage"] = "RegisteSuccess";


            TempData["SuccessMessageTitle"] = "成功";
            TempData["SuccessMessageContent"] = "註冊成功";
            return View();
        }

        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        [HttpGet]
        public async Task<IActionResult> MemberProfile()
        {

            // 缺少審核身分證、電子信箱、手機

            string photoPath = _cache.Get<string>($"Avatar_{User.FindFirst("UserId")?.Value}") ?? "~/images/user-image.jpg";

            ViewBag.userPhoto = photoPath;

            int userId = Convert.ToInt32(User.FindFirst("UserId")!.Value);

            Member? member = await _context.Members.Where(m => m.MemberId == userId).FirstOrDefaultAsync();


            if (member == null)
            {
                return View("Login");
            }

            var cities = await _context.Cities.Select(c => new SelectListItem
            {
                Value = c.CityId.ToString(),
                Text = c.CityName
            }).ToListAsync();


            var residenceDistrict = await _context.Districts.Where(d => d.CityId == member.ResidenceCityId).Select(d => new SelectListItem
            {
                Value = d.DistrictId.ToString(),
                Text = d.DistrictName
            }).ToListAsync();
            var primaryRentalDistrict = await _context.Districts.Where(d => d.CityId == member.PrimaryRentalCityId).Select(d => new SelectListItem
            {
                Value = d.DistrictId.ToString(),
                Text = d.DistrictName
            }).ToListAsync();


            MemberProfileViewModel memberInfo = new MemberProfileViewModel
            {
                MemberName = member.MemberName,
                Gender = member.Gender,
                BirthDate = member.BirthDate,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                IsLandlord = member.IsLandlord,
                MemberTypeId = member.MemberTypeId,
                PrimaryRentalCityId = member.PrimaryRentalCityId,
                PrimaryRentalDistrictId = member.PrimaryRentalDistrictId,
                ResidenceCityId = member.ResidenceCityId,
                ResidenceDistrictId = member.ResidenceDistrictId,
                AddressLine = member.AddressLine,
                PhoneVerifiedAt = member.PhoneVerifiedAt,
                EmailVerifiedAt = member.EmailVerifiedAt,
                IdentityVerifiedAt = member.IdentityVerifiedAt,
                NationalIdNo = member.NationalIdNo,
                CreatedAt = member.CreatedAt,
                CityOptions = cities,
                ResidenceDistrictOptions = residenceDistrict,
                PrimaryRentalDistrictOptions = primaryRentalDistrict,
            };

            return View(memberInfo);
        }


        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        [HttpPost]
        public async Task<IActionResult> UpdatememberProfile(MemberProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CityOptions = await _context.Cities.Select(c => new SelectListItem
                {
                    Value = c.CityId.ToString(),
                    Text = c.CityName
                }).ToListAsync();


                model.ResidenceDistrictOptions = await _context.Districts.Where(d => d.CityId == model.ResidenceCityId).Select(d => new SelectListItem
                {
                    Value = d.DistrictId.ToString(),
                    Text = d.DistrictName
                }).ToListAsync();
                model.PrimaryRentalDistrictOptions = await _context.Districts.Where(d => d.CityId == model.PrimaryRentalCityId).Select(d => new SelectListItem
                {
                    Value = d.DistrictId.ToString(),
                    Text = d.DistrictName
                }).ToListAsync();


                string photoPath = _cache.Get<string>($"Avatar_{User.FindFirst("UserId")?.Value}") ?? "~/images/user-image.jpg";

                ViewBag.userPhoto = photoPath;

            }

            int userId = Convert.ToInt32(User.FindFirst("UserId")!.Value);

            Member? member = await _context.Members.FindAsync(userId);


            if (member == null)
            {
                return View("Login");
            }

            bool isVerified = model.IdentityVerifiedAt != null;

            if (!isVerified)
            {
                member.MemberName = model.MemberName!;
                member.BirthDate = model.BirthDate;
                member.Gender = model.Gender;
            }

            if (!string.IsNullOrEmpty(model.Email) && model.Email != member.Email) // 修改Email 移除認證時間
            {
                member.Email = model.Email!;
                member.EmailVerifiedAt = null;
            }
            member.UpdatedAt = DateTime.Now;
            member.PrimaryRentalCityId = model.PrimaryRentalCityId;
            member.PrimaryRentalDistrictId = model.PrimaryRentalDistrictId;
            member.ResidenceCityId = model.ResidenceCityId;
            member.ResidenceDistrictId = model.ResidenceDistrictId;
            member.AddressLine = model.AddressLine;

            await _context.SaveChangesAsync();
            TempData["SuccessMessageTitle"] = "成功";
            TempData["SuccessMessageContent"] = "會員資料已更新";
            return RedirectToAction("MemberProfile");

        }


        public String AccessDenied()
        {
            return "權限不足";
        }

        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> GetUploadStatus()
        {

            var member = await _context.Members.FindAsync(int.Parse(User.FindFirst("UserId")!.Value));
            if (member == null) return BadRequest(new
            {
                status = "error",
                message = "請先登入",
            });

            var frontLog = await _context.UserUploads.Where(u => u.MemberId == member.MemberId && u.UploadTypeCode == "USER_ID_FRONT").OrderByDescending(x => x.UploadId).FirstOrDefaultAsync();



            var backLog = await _context.UserUploads.Where(u => u.MemberId == member.MemberId && u.UploadTypeCode == "USER_ID_BACK").OrderByDescending(x => x.UploadId).FirstOrDefaultAsync();

            if (backLog == null && frontLog == null) return BadRequest(new
            {
                status = "error",
                message = "無上傳紀錄",
            });


            return Ok(new
            {
                status = "ok",
                hasFront = frontLog != null,
                hasBack = backLog != null,
                frontUploadedAt = frontLog == null ? "" : frontLog!.UploadedAt.ToString("yyyy-MM-dd"),
                backUploadedAt = backLog == null ? "" : backLog.UploadedAt.ToString("yyyy-MM-dd"),
                frontFilePath = frontLog == null ? "" : frontLog.FilePath,
                backFilePath = backLog == null ? "" : backLog.FilePath
            });
        }

        public async Task<IActionResult> Upload([FromForm] UserUploadViewModel model)
        {
            string uploadFolderName = "";

            if (User.FindFirst("UserId")?.Value == null)
            {
                return BadRequest("請先登入");
            }
            if (model.UploadFile == null || model.UploadFile.Length == 0)
            {
                return BadRequest("未上傳檔案");
            }
            string[] allowedTypes = ["image/jpeg", "image/png", "image/webp"];

            int maxSize = 5 * 1024 * 1024;

            if (!allowedTypes.Contains(model.UploadFile.ContentType))
            {
                return BadRequest("檔案格式錯誤，只允許 jpg/png/webp");
            }
            if (model.UploadFile.Length > maxSize)
            {
                return BadRequest("檔案過大，請勿超過 5MB");
            }

            if (model.ModuleCode == "MemberInfo")
            {
                uploadFolderName = "userPhoto";
            }

            var ext = Path.GetExtension(model.UploadFile.FileName);
            var storeFileName = Guid.NewGuid().ToString() + ext;
            var storePath = $"/uploads/{uploadFolderName}/{storeFileName}";
            var path = Path.Combine($"wwwroot/uploads/{uploadFolderName}", storeFileName);

            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.UploadFile.CopyToAsync(stream);
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "檔案儲存失敗，請稍後再試");
            }

            // 計算檔案檢查碼
            string checksum;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                using (var stream = model.UploadFile.OpenReadStream())
                {
                    var hash = sha256.ComputeHash(stream);
                    checksum = Convert.ToHexString(hash);
                }
            }

            // 對於身份證上傳，需要關聯到待審核的申請
            int? approvalId = null;
            if (model.ModuleCode == "MemberInfo" && 
                (model.UploadTypeCode == "USER_ID_FRONT" || model.UploadTypeCode == "USER_ID_BACK"))
            {
                var memberId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
                var pendingApproval = await _context.Approvals
                    .FirstOrDefaultAsync(a => 
                        a.ModuleCode == "IDENTITY" &&
                        a.ApplicantMemberId == memberId &&
                        a.StatusCode == "PENDING");
                
                if (pendingApproval != null)
                {
                    approvalId = pendingApproval.ApprovalId;
                    // 日誌：成功找到審核申請
                    System.Diagnostics.Debug.WriteLine($"身份證上傳：會員 {memberId} 關聯到申請 {approvalId}");
                }
                else
                {
                    // 日誌：未找到審核申請
                    System.Diagnostics.Debug.WriteLine($"身份證上傳：會員 {memberId} 未找到待審核申請");
                }
            }

            var uploadInfo = new UserUpload
            {
                MemberId = Convert.ToInt32(User.FindFirst("UserId")?.Value),
                ModuleCode = model.ModuleCode ?? "",
                SourceEntityId = Convert.ToInt32(User.FindFirst("UserId")?.Value),
                UploadTypeCode = model.UploadTypeCode ?? "",
                OriginalFileName = model.UploadFile.FileName,
                StoredFileName = storeFileName,
                FileExt = ext,
                MimeType = model.UploadFile.ContentType,
                FilePath = storePath,
                FileSize = model.UploadFile.Length,
                Checksum = checksum,
                ApprovalId = approvalId,
                IsActive = true,
                UploadedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _context.UserUploads.Add(uploadInfo);
            await _context.SaveChangesAsync();

            if (model.ModuleCode == "MemberInfo" && model.UploadTypeCode == "USER_IMG")
            {
                _cache.Set($"Avatar_{User.FindFirst("UserId")?.Value}", $"~{storePath}");
            }
            return Ok(new
            {
                originalFileName = model.UploadFile.FileName,
                storedFileName = storeFileName,
                previewUrl = $"/uploads/{uploadFolderName}/{storeFileName}",
            });
        }

        //會員認證

        [HttpPost]
        public async Task<IActionResult> SubmitIdentityApplication()
        {
            int memberId = int.Parse(User.FindFirst("UserId")!.Value);

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null)
                return NotFound("找不到會員");

            // 檢查是否已有待審核申請
            var existingApproval = await _context.Approvals
                .FirstOrDefaultAsync(a =>
                    a.ModuleCode == "IDENTITY" &&
                    a.ApplicantMemberId == memberId &&
                    a.StatusCode == "PENDING");

            if (existingApproval != null)
            {
                return Json(new { 
                    success = true, 
                    message = "身份驗證申請已存在",
                    approvalId = existingApproval.ApprovalId 
                });
            }




            var approval = new Approval
            {
                ModuleCode = "IDENTITY",
                ApplicantMemberId = memberId,
                StatusCode = "PENDING",
                CurrentApproverId = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Approvals.Add(approval);
            await _context.SaveChangesAsync();
            var snapshot = new
            {
                memberID = member.MemberId,
                memberName = member.MemberName,
                submitTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                verificationStatus = "pending"
            };
            string snapshotJson = JsonSerializer.Serialize(snapshot);

            var item = new ApprovalItem
            {
                ApprovalId = approval.ApprovalId,
                ActionBy = null,
                ActionType = "SUBMIT",
                ActionNote = "會員提交身分證驗證申請",
                SnapshotJson = snapshotJson,
                CreatedAt = DateTime.Now
            };
            _context.ApprovalItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "身份驗證申請已送出" });
        }




        /// <summary>
        /// 智能重導向輔助方法 - 結合Session模組追蹤和Referer判斷來決定轉導目標
        /// 優先順序：明確ReturnUrl > Session記錄 > Referer判斷 > 預設首頁
        /// </summary>
        /// <param name="returnUrl">明確指定的回傳URL</param>
        /// <returns>重導向URL</returns>
        private string GetSmartRedirectUrl(string? returnUrl)
        {
            try
            {
                // 第一優先：如果有明確的 ReturnUrl，優先使用
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return returnUrl;
                }

                // 第二優先：基於用戶類型的智能重導向
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst("UserId")?.Value;
                    if (int.TryParse(userIdClaim, out var userId))
                    {
                        // 查詢用戶身份類型
                        var member = _context.Members.FirstOrDefault(m => m.MemberId == userId);
                        if (member != null)
                        {
                            // 已驗證房東 → 房源管理頁面
                            if (member.IsLandlord && member.MemberTypeId == 2 && member.IdentityVerifiedAt != null)
                            {
                                return Url.Action("PropertyManagement", "Landlord") ?? "/landlord/propertymanagement";
                            }
                            // 房客 → 租屋首頁（保持原有邏輯）
                            // 未驗證房東 → 租屋首頁（保持原有邏輯）
                        }
                    }
                }

                // 第三優先：檢查Session中記錄的最後模組
                var lastModule = HttpContext.Session.GetString("LastModule");
                if (!string.IsNullOrEmpty(lastModule))
                {
                    return lastModule switch
                    {
                        "Furniture" => Url.Action("FurnitureHomePage", "Furniture") ?? "/Furniture/FurnitureHomePage",
                        "Rental" => Url.Action("FrontPage", "Tenant") ?? "/Tenant/FrontPage",
                        _ => Url.Action("FrontPage", "Tenant") ?? "/Tenant/FrontPage"
                    };
                }

                // 第四優先：回退到Referer判斷 (針對Session過期或新用戶)
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    // 防止無限重導向：跳過系統頁面
                    if (!referer.Contains("/Member/") && 
                        !referer.Contains("/Auth/") &&
                        !referer.Contains("/Admin/"))
                    {
                        // 使用Controller路徑自動判斷（移除硬編碼頁面清單）
                        if (referer.Contains("/Furniture/", StringComparison.OrdinalIgnoreCase))
                        {
                            return Url.Action("FurnitureHomePage", "Furniture") ?? "/Furniture/FurnitureHomePage";
                        }
                    }
                }

                // 預設：導向租屋首頁
                return Url.Action("FrontPage", "Tenant") ?? "/Tenant/FrontPage";
            }
            catch (Exception)
            {
                // 異常處理：回到安全的預設頁面
                return "/Tenant/FrontPage";
            }
        }
    }
}
