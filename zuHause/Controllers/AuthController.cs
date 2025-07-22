using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using zuHause.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace zuHause.Controllers
{
    /// <summary>
    /// 管理員身份驗證控制器
    /// 
    /// 功能說明：
    /// - 提供管理員登入/登出功能
    /// - 使用 Cookie Authentication 機制
    /// - 登入成功後自動儲存管理員完整身份資訊至 Claims
    /// - 支援與 AdminController 和 DashboardController 共享登入狀態
    /// 
    /// 使用方式：
    /// 1. GET /Auth/Login - 顯示登入頁面
    /// 2. POST /Auth/Login - 處理登入請求 (需提供 account, password)
    /// 3. GET /Auth/Logout - 登出並重導向至登入頁面
    /// 
    /// 登入後可取得的身份資訊：
    /// - ClaimTypes.NameIdentifier: 管理員ID
    /// - ClaimTypes.Name: 管理員姓名  
    /// - "Account": 管理員帳號
    /// - "RoleCode": 角色代碼
    /// - "RoleName": 角色名稱
    /// - "PermissionsJSON": 角色權限JSON字串
    /// 
    /// 在其他 Controller 中取得身份資訊範例：
    /// var adminId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    /// var adminName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
    /// var permissions = HttpContext.User.FindFirst("PermissionsJSON")?.Value;
    /// 
    /// 注意事項：
    /// - 目前密碼驗證使用明文比對，建議改用 bcrypt 進行雜湊驗證
    /// - 登入成功後會重導向至 Dashboard/Index
    /// </summary>
    public class AuthController : Controller
    {
        private readonly ZuHauseContext _context;

        public AuthController(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 顯示登入頁面
        /// </summary>
        /// <returns>登入視圖</returns>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 處理管理員登入請求
        /// </summary>
        /// <param name="account">管理員帳號</param>
        /// <param name="password">密碼</param>
        /// <returns>
        /// 登入成功：重導向至 Dashboard/Index
        /// 登入失敗：返回登入頁面並顯示錯誤訊息
        /// </returns>
        /// <remarks>
        /// 登入流程：
        /// 1. 驗證帳號是否存在且啟用
        /// 2. 驗證密碼是否正確 (目前為明文比對)
        /// 3. 取得管理員角色資訊
        /// 4. 建立 Claims 並設定 Cookie Authentication
        /// 5. 重導向至後台首頁
        /// 
        /// 建立的 Claims 包含：
        /// - 管理員ID、姓名、帳號
        /// - 角色代碼、角色名稱
        /// - 權限JSON字串
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> Login(string account, string password)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Account == account && a.DeletedAt == null);
            if (admin == null || !admin.IsActive)
            {
                ViewBag.Error = "帳號不存在或已停用";
                return View();
            }

            if (!VerifyPasswordWithBase64(password, admin.PasswordSalt ?? "", admin.PasswordHash))
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
            // ✅ ✅ ✅ 更新最後登入時間
            admin.LastLoginAt = DateTime.Now;
            _context.SaveChanges();
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
        private bool VerifyPasswordWithBase64(string inputPassword, string salt, string storedBase64Hash)
        {

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var combined = inputPassword + salt;
            var bytes = System.Text.Encoding.UTF8.GetBytes(combined);
            var hashBytes = sha256.ComputeHash(bytes);
            var computedBase64 = Convert.ToBase64String(hashBytes);
           
            return computedBase64 == storedBase64Hash;
        }


        /// <summary>
        /// 管理員登出
        /// </summary>
        /// <returns>重導向至登入頁面</returns>
        /// <remarks>
        /// 登出流程：
        /// 1. 清除 AdminCookies 認證資訊
        /// 2. 重導向至登入頁面
        /// 
        /// 調用方式：
        /// - 直接訪問 /Auth/Logout
        /// - 或在其他頁面中建立登出連結
        /// </remarks>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookies");
            return RedirectToAction("Login");
        }
    }
}
