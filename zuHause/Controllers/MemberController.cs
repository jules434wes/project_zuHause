using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Security.Claims;
using zuHause.Models;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.Controllers
{
    public class MemberController : Controller
    {



        public readonly ZuHauseContext _context;
        private readonly IMemoryCache _cache;
        public MemberController(ZuHauseContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string? ReturnUrl = null)
        {
            var model = new LoginViewModel()
            {
                ReturnUrl = ReturnUrl
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? userPhoneNumber = model.PhoneNumber;
            string? userPassword = model.UserPassword;
            var member = await _context.Members.SingleOrDefaultAsync((x) => (x.PhoneNumber
             == userPhoneNumber && x.Password == userPassword));
            if (member == null)
            {
                ModelState.AddModelError("LoginStatus", "帳號或密碼錯誤");

                return View(model);
            }

            //處理登入Cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, member.MemberName),
                new Claim("Phone",member.PhoneNumber),
                new Claim("UserId",member.MemberId.ToString()),
                new Claim(ClaimTypes.Role,member.MemberTypeId?.ToString() ?? ""),

            };

            if (member.MemberTypeId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.Role, member.MemberTypeId.Value.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "MemberCookieAuth");
            var authProperties = new AuthenticationProperties
            {
                // 待補
            };


            // 大頭照查詢
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
                photoPath = "~images/user-image.jpg";
            }

            _cache.Set($"Avatar_{member.MemberId.ToString()}", photoPath);

            await HttpContext.SignInAsync("MemberCookieAuth",
                new ClaimsPrincipal(claimsIdentity), authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var memberId = User.FindFirst("UserId")?.Value;
            await HttpContext.SignOutAsync("MemberCookieAuth");
            _cache.Remove($"Avatar_{memberId}");


            return RedirectToAction("Index");
        }
        public IActionResult ResetPasswordVerifyPhone()
        {
            return View();
        }
        public IActionResult ResetPasswordSendCode()
        {
            return View();
        }
        public IActionResult ResetPasswordConfirmCode()
        {
            return View();
        }
        public IActionResult ResetPasswordChange()
        {
            return View();
        }
        public IActionResult RegisterVerifyPhone()
        {
            return View();
        }
        public IActionResult RegisterSendCode()
        {
            return View();
        }
        public IActionResult RegisteFillInfomation()
        {
            return View();
        }
        [Authorize(Roles = "1", AuthenticationSchemes = "MemberCookieAuth")]
        public IActionResult RegisteSuccess()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public IActionResult MemberProfile()
        {
            string photoPath = _cache.Get<string>($"Avatar_{User.FindFirst("UserId")?.Value}") ?? "~/images/user-image.jpg";

            ViewBag.userPhoto = photoPath;
            return View();
        }
        public String AccessDenied()
        {
            return "權限不足";
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
            if(model.UploadFile.Length > maxSize)
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
            catch (Exception ex)
            {
                return StatusCode(500, "檔案儲存失敗，請稍後再試");
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
                IsActive = true,
                UploadedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _context.UserUploads.Add(uploadInfo);
            await _context.SaveChangesAsync();
            _cache.Set($"Avatar_{User.FindFirst("UserId")?.Value}", $"~{storePath}");
            return Ok(new
            {
                originalFileName = model.UploadFile.FileName, 
                storedFileName = storeFileName, 
                previewUrl = $"/uploads/{uploadFolderName}/{storeFileName}", 
            });
        }
    }
}
