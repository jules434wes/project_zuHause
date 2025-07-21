using Microsoft.AspNetCore.Mvc;
using zuHause.Services.Interfaces;

namespace zuHause.Controllers
{
    /// <summary>
    /// 錯誤處理控制器
    /// 處理權限不足、頁面不存在等錯誤狀況
    /// </summary>
    public class ErrorController : Controller
    {
        private readonly IPermissionService _permissionService;

        public ErrorController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// 403 權限不足錯誤頁面
        /// 當管理員嘗試存取沒有權限的功能時顯示
        /// </summary>
        /// <param name="requiredPermission">所需但缺少的權限</param>
        /// <param name="currentPath">使用者嘗試存取的路徑</param>
        /// <returns>403 錯誤頁面</returns>
        /// <remarks>
        /// 此頁面會顯示：
        /// 1. 權限不足的提示訊息
        /// 2. 當前使用者的角色和權限資訊
        /// 3. 所需權限的說明
        /// 4. 返回首頁或聯絡管理員的選項
        /// </remarks>
        public IActionResult AccessDenied(string? requiredPermission = null, string? currentPath = null)
        {
            // 設定 HTTP 狀態碼為 403
            Response.StatusCode = 403;

            // 取得當前使用者的權限資訊（用於顯示）
            ViewBag.RequiredPermission = requiredPermission;
            ViewBag.CurrentPath = currentPath;
            ViewBag.UserPermissions = _permissionService.GetUserPermissions(HttpContext.User);
            ViewBag.IsSuperAdmin = _permissionService.IsSuperAdmin(HttpContext.User);
            
            // 取得使用者角色資訊
            ViewBag.UserRole = HttpContext.User.FindFirst("RoleName")?.Value ?? "未知角色";
            ViewBag.UserName = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "未知使用者";

            // 權限名稱對應表（用於顯示友善的權限名稱）
            var permissionNames = new Dictionary<string, string>
            {
                { "member_list", "會員列表與驗證" },
                { "landlord_list", "房東列表" },
                { "property_list", "房源列表" },
                { "property_complaint_list", "房源投訴列表" },
                { "customer_service_list", "客服處理" },
                { "system_message_list", "系統訊息" },
                { "overview", "平台整體概況" },
                { "monitor", "商品與房源監控" },
                { "behavior", "用戶行為監控" },
                { "orders", "訂單與金流" },
                { "system", "系統通知與健康" },
                { "roles", "身分權限列表" },
                { "Backend_user_list", "後臺使用者" },
                { "contract_template", "合約範本管理" },
                { "platform_fee", "平台收費設定" },
                { "imgup", "輪播圖片管理" },
                { "furniture_fee", "家具配送費" },
                { "Marquee_edit", "跑馬燈管理" },
                { "furniture_management", "家具列表管理" },
                { "announcement_management", "公告管理" }
            };

            ViewBag.RequiredPermissionName = requiredPermission != null && permissionNames.ContainsKey(requiredPermission) 
                ? permissionNames[requiredPermission] 
                : requiredPermission ?? "未知功能";

            return View();
        }

        /// <summary>
        /// 404 頁面不存在錯誤
        /// </summary>
        /// <returns>404 錯誤頁面</returns>
        public IActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        /// <summary>
        /// 500 伺服器內部錯誤
        /// </summary>
        /// <returns>500 錯誤頁面</returns>
        public IActionResult InternalServerError()
        {
            Response.StatusCode = 500;
            return View();
        }
    }
}