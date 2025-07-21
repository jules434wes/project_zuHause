using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using zuHause.Models;

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
        public IActionResult Login(string account, string password)
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

            // 設定 Session
            HttpContext.Session.SetString("adminID", admin.AdminId.ToString()); // ✅ 正確屬性名稱
            HttpContext.Session.SetString("roleCode", admin.RoleCode);
            HttpContext.Session.SetString("roleName", role.RoleName);
            HttpContext.Session.SetString("permissionsJSON", role.PermissionsJson ?? "{}"); // ✅ 注意 Json 結尾 J 要小寫


            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
