using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using zuHause.Services.Interfaces;

namespace zuHause.Attributes
{
    /// <summary>
    /// Admin 功能權限檢查屬性
    /// 
    /// 使用方式：
    /// [RequireAdminPermission("member_list")]
    /// public IActionResult admin_usersList() { ... }
    /// 
    /// 功能說明：
    /// - 在 Controller Action 執行前檢查管理員權限
    /// - 權限不足時返回 403 Forbidden 錯誤頁面
    /// - 配合 IPermissionService 進行權限驗證
    /// - 專門用於 AdminController 的功能保護
    /// 
    /// 權限檢查流程：
    /// 1. 從 HttpContext.User 取得管理員 Claims 資料
    /// 2. 透過 IPermissionService 檢查是否具有所需權限
    /// 3. 有權限：繼續執行 Action
    /// 4. 無權限：返回 403 錯誤頁面或重導向
    /// 
    /// 注意事項：
    /// - 此屬性假設使用者已通過登入驗證（配合 [Authorize] 使用）
    /// - 權限鍵值必須與 dashboard.js 中的功能鍵值一致
    /// - 無權限時的處理方式可透過建構函數參數自訂
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequireAdminPermissionAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 所需的權限鍵值
        /// </summary>
        private readonly string _requiredPermission;

        /// <summary>
        /// 權限不足時是否重導向到 403 頁面（預設 true）
        /// </summary>
        private readonly bool _redirectTo403;

        /// <summary>
        /// 初始化權限檢查屬性
        /// </summary>
        /// <param name="requiredPermission">所需的權限鍵值（對應 dashboard.js 中的功能鍵值）</param>
        /// <param name="redirectTo403">權限不足時是否重導向到 403 頁面</param>
        /// <remarks>
        /// 權限鍵值範例：
        /// - "member_list": 會員列表與驗證
        /// - "landlord_list": 房東列表
        /// - "property_list": 房源列表
        /// - "property_complaint_list": 房源投訴列表
        /// - "customer_service_list": 客服處理
        /// - "system_message_list": 系統訊息
        /// </remarks>
        public RequireAdminPermissionAttribute(string requiredPermission, bool redirectTo403 = true)
        {
            _requiredPermission = requiredPermission ?? throw new ArgumentNullException(nameof(requiredPermission));
            _redirectTo403 = redirectTo403;
        }

        /// <summary>
        /// Action 執行前的權限檢查
        /// </summary>
        /// <param name="context">Action 執行上下文</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // === 步驟1：取得權限檢查服務 ===
            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
            if (permissionService == null)
            {
                // 權限服務未註冊，為安全起見拒絕存取
                context.Result = new StatusCodeResult(500);
                return;
            }

            // === 步驟2：檢查使用者權限 ===
            var user = context.HttpContext.User;
            bool hasPermission = permissionService.HasPermission(user, _requiredPermission);

            // === 步驟3：處理權限檢查結果 ===
            if (!hasPermission)
            {
                // 權限不足的處理
                if (_redirectTo403)
                {
                    // 重導向到 403 錯誤頁面
                    context.Result = new RedirectToActionResult("AccessDenied", "Error", new { 
                        requiredPermission = _requiredPermission,
                        currentPath = context.HttpContext.Request.Path
                    });
                }
                else
                {
                    // 直接返回 403 狀態碼
                    context.Result = new StatusCodeResult(403);
                }
                return;
            }

            // === 步驟4：權限檢查通過，繼續執行 Action ===
            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// 權限檢查屬性的便利方法擴展
    /// 提供預定義的 Admin 功能權限檢查
    /// </summary>
    public static class AdminPermissions
    {
        /// <summary>
        /// 會員列表與驗證權限
        /// </summary>
        public const string MemberList = "member_list";

        /// <summary>
        /// 房東列表權限
        /// </summary>
        public const string LandlordList = "landlord_list";

        /// <summary>
        /// 房源列表權限
        /// </summary>
        public const string PropertyList = "property_list";

        /// <summary>
        /// 房源投訴列表權限
        /// </summary>
        public const string PropertyComplaintList = "property_complaint_list";

        /// <summary>
        /// 客服處理權限
        /// </summary>
        public const string CustomerServiceList = "customer_service_list";

        /// <summary>
        /// 系統訊息權限
        /// </summary>
        public const string SystemMessageList = "system_message_list";

        /// <summary>
        /// 房源詳情權限（通常與房源列表權限相同）
        /// </summary>
        public const string PropertyDetails = PropertyList;

        /// <summary>
        /// 會員詳情權限（通常與會員列表權限相同）
        /// </summary>
        public const string MemberDetails = MemberList;

        /// <summary>
        /// 客服詳情權限（通常與客服處理權限相同）
        /// </summary>
        public const string CustomerServiceDetails = CustomerServiceList;

        /// <summary>
        /// 系統訊息新增權限（通常與系統訊息權限相同）
        /// </summary>
        public const string SystemMessageNew = SystemMessageList;
    }
}