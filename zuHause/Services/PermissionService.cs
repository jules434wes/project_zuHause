using System.Security.Claims;
using System.Text.Json;
using zuHause.Services.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// 管理員權限檢查服務實作
    /// 
    /// 權限資料來源：
    /// - 權限資料儲存在管理員 Cookie 的 Claims 中
    /// - PermissionsJSON Claim 包含從資料庫 AdminRoles.PermissionsJson 讀取的權限資料
    /// - 資料格式：超級管理員 {"all": true}，一般管理員 {"overview": true, "monitor": false, ...}
    /// 
    /// 權限鍵值對應：
    /// - 對應 dashboard.js 中 allKeys 陣列的功能鍵值
    /// - Admin 功能：member_list, landlord_list, property_list, property_complaint_list, customer_service_list, system_message_list
    /// - Dashboard 功能：overview, monitor, behavior, orders, system, roles, Backend_user_list, etc.
    /// </summary>
    public class PermissionService : IPermissionService
    {
        /// <summary>
        /// 系統支援的所有權限鍵值
        /// 這個清單與 DashboardController.Index() 中的 allKeys 保持同步
        /// </summary>
        private static readonly List<string> AllAvailablePermissions = new()
        {
            // Dashboard 功能
            "overview", "monitor", "behavior", "orders", "system",
            "roles", "Backend_user_list", "contract_template",
            "platform_fee", "imgup", "furniture_fee", "Marquee_edit", 
            "furniture_management", "announcement_management",
            
            // Admin 功能（需要權限控制的）
            "member_list", "landlord_list", "property_list", 
            "property_complaint_list", "customer_service_list", "system_message_list"
        };

        /// <summary>
        /// 檢查管理員是否具有指定功能的權限
        /// </summary>
        /// <param name="user">當前用戶的 Claims Principal</param>
        /// <param name="requiredPermission">所需的權限鍵值</param>
        /// <returns>是否具有權限</returns>
        public bool HasPermission(ClaimsPrincipal user, string requiredPermission)
        {
            // === 步驟1：基本檢查 ===
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false; // 未登入
            }

            if (string.IsNullOrWhiteSpace(requiredPermission))
            {
                return false; // 無效的權限鍵值
            }

            // === 步驟2：取得權限資料 ===
            var permissionsJson = user.FindFirst("PermissionsJSON")?.Value;
            if (string.IsNullOrWhiteSpace(permissionsJson))
            {
                return false; // 沒有權限資料
            }

            try
            {
                // === 步驟3：解析權限JSON ===
                var permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(permissionsJson);
                if (permissions == null)
                {
                    return false; // JSON 解析失敗
                }

                // === 步驟4：檢查超級管理員權限 ===
                if (permissions.TryGetValue("all", out bool isAll) && isAll)
                {
                    return true; // 超級管理員擁有所有權限
                }

                // === 步驟5：檢查一般管理員的特定權限 ===
                return permissions.TryGetValue(requiredPermission, out bool hasPermission) && hasPermission;
            }
            catch (JsonException)
            {
                // JSON 格式錯誤，拒絕存取
                return false;
            }
        }

        /// <summary>
        /// 取得管理員的所有可用權限清單
        /// </summary>
        /// <param name="user">當前用戶的 Claims Principal</param>
        /// <returns>權限鍵值清單</returns>
        public List<string> GetUserPermissions(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return new List<string>(); // 未登入返回空清單
            }

            var permissionsJson = user.FindFirst("PermissionsJSON")?.Value;
            if (string.IsNullOrWhiteSpace(permissionsJson))
            {
                return new List<string>();
            }

            try
            {
                var permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(permissionsJson);
                if (permissions == null)
                {
                    return new List<string>();
                }

                // 超級管理員返回所有可用權限
                if (permissions.TryGetValue("all", out bool isAll) && isAll)
                {
                    return new List<string>(AllAvailablePermissions);
                }

                // 一般管理員返回被授權的權限（值為 true 的項目）
                return permissions
                    .Where(p => p.Value && AllAvailablePermissions.Contains(p.Key))
                    .Select(p => p.Key)
                    .ToList();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// 檢查管理員是否為超級管理員
        /// </summary>
        /// <param name="user">當前用戶的 Claims Principal</param>
        /// <returns>是否為超級管理員</returns>
        public bool IsSuperAdmin(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var permissionsJson = user.FindFirst("PermissionsJSON")?.Value;
            if (string.IsNullOrWhiteSpace(permissionsJson))
            {
                return false;
            }

            try
            {
                var permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(permissionsJson);
                return permissions?.TryGetValue("all", out bool isAll) == true && isAll;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}