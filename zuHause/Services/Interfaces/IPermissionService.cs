using System.Security.Claims;

namespace zuHause.Services.Interfaces
{
    /// <summary>
    /// 管理員權限檢查服務介面
    /// 
    /// 功能說明：
    /// - 從管理員的 Claims 中檢查特定功能權限
    /// - 支援超級管理員（all: true）和一般管理員的權限檢查
    /// - 配合 RequireAdminPermissionAttribute 進行後端權限控制
    /// 
    /// 使用場景：
    /// - Controller Action 方法的權限檢查
    /// - 自訂 Authorization Attribute 的權限驗證
    /// - 動態權限檢查（如條件式顯示內容）
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// 檢查管理員是否具有指定功能的權限
        /// </summary>
        /// <param name="user">當前用戶的 Claims Principal（通常從 HttpContext.User 取得）</param>
        /// <param name="requiredPermission">所需的權限鍵值（對應 dashboard.js 中的功能鍵值）</param>
        /// <returns>
        /// true: 具有權限（超級管理員或一般管理員有此功能權限）
        /// false: 沒有權限或未登入
        /// </returns>
        /// <remarks>
        /// 權限檢查邏輯：
        /// 1. 檢查是否已登入（Claims 是否存在）
        /// 2. 從 PermissionsJSON Claim 解析權限資料
        /// 3. 如果是超級管理員（{"all": true}）→ 直接返回 true
        /// 4. 如果是一般管理員 → 檢查權限清單中是否包含所需權限
        /// 5. 其他情況 → 返回 false
        /// </remarks>
        bool HasPermission(ClaimsPrincipal user, string requiredPermission);

        /// <summary>
        /// 取得管理員的所有可用權限清單
        /// </summary>
        /// <param name="user">當前用戶的 Claims Principal</param>
        /// <returns>權限鍵值清單，超級管理員返回所有可用權限</returns>
        /// <remarks>
        /// 使用場景：
        /// - 動態生成選單
        /// - 權限檢查除錯
        /// - 管理介面顯示當前用戶權限
        /// </remarks>
        List<string> GetUserPermissions(ClaimsPrincipal user);

        /// <summary>
        /// 檢查管理員是否為超級管理員
        /// </summary>
        /// <param name="user">當前用戶的 Claims Principal</param>
        /// <returns>true: 超級管理員；false: 一般管理員或未登入</returns>
        bool IsSuperAdmin(ClaimsPrincipal user);
    }
}