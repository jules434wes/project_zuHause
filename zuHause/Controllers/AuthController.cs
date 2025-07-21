using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using zuHause.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace zuHause.Controllers
{
    public class AuthController : Controller
    {
        private readonly ZuHauseContext _context;

        public AuthController(ZuHauseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string account, string password)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Account == account && a.DeletedAt == null);
            if (admin == null || !admin.IsActive)
            {
                ViewBag.Error = "帳號不存在或已停用";
                return View();
            }

            if (admin.PasswordHash != password) // ⛔️建議用 bcrypt 驗證
            {
                ViewBag.Error = "密碼錯誤";
                return View();
            }

            var role = _context.AdminRoles.FirstOrDefault(r => r.RoleCode == admin.RoleCode && r.IsActive);
            if (role == null)
            {
                ViewBag.Error = "角色資料錯誤";
                return View();
            }

            // 設定 Cookie Authentication Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
                new Claim(ClaimTypes.Name, admin.Name),
                new Claim("Account", admin.Account),
                new Claim("RoleCode", admin.RoleCode),
                new Claim("RoleName", role.RoleName),
                new Claim("PermissionsJSON", role.PermissionsJson ?? "{}")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "AdminCookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("AdminCookies", claimsPrincipal);

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookies");
            return RedirectToAction("Login");
        }
    }
}
