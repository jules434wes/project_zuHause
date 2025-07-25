using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace zuHause.Middleware
{
    /// <summary>
    /// 模組追蹤中間件 - 自動記錄用戶當前所在的業務模組
    /// 用於改善登入登出後的智能轉導體驗
    /// </summary>
    public class ModuleTrackingMiddleware
    {
        private readonly RequestDelegate _next;

        public ModuleTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // 追蹤用戶所在的業務模組
            if (!string.IsNullOrEmpty(path))
            {
                // 家具模組識別
                if (path.StartsWith("/Furniture/", StringComparison.OrdinalIgnoreCase))
                {
                    context.Session.SetString("LastModule", "Furniture");
                }
                // 租屋模組識別 (包含首頁和租客相關頁面)
                else if (path.StartsWith("/Tenant/", StringComparison.OrdinalIgnoreCase) ||
                         path.Equals("/", StringComparison.OrdinalIgnoreCase) ||
                         path.StartsWith("/Property/", StringComparison.OrdinalIgnoreCase))
                {
                    context.Session.SetString("LastModule", "Rental");
                }
                // 跳過管理員和會員相關頁面，避免影響模組判斷
                // 這些頁面不應該改變用戶的業務模組狀態
                else if (path.StartsWith("/Member/", StringComparison.OrdinalIgnoreCase) ||
                         path.StartsWith("/Auth/", StringComparison.OrdinalIgnoreCase) ||
                         path.StartsWith("/Admin/", StringComparison.OrdinalIgnoreCase))
                {
                    // 不記錄這些系統頁面的模組狀態
                }
            }

            await _next(context);
        }
    }
}