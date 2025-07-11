using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore; // 確保您有此 using 語句，用於 DbContext

// 確保您引用了包含 SiteMessage 和 ZuHauseContext 的命名空間
// 假設它們在 zuHause.Models 或 zuhause.Data 命名空間下
using zuHause.Models; // 您的模型 SiteMessage 所在的命名空間
// using zuhause.Data; // 如果您的 ZuHauseContext 在這裡，請取消註釋或修改為實際路徑

namespace zuhause.Controllers
{
    public class TenantController : Controller
    {
        // 將 DbContext 類型更正為 ZuHauseContext
        private readonly ZuHauseContext _context; // 依賴注入您的 ZuHauseContext

        // Controller 的建構子，用於接收 ZuHauseContext
        // 確保您的 Startup.cs (或 Program.cs) 已註冊 ZuHauseContext
        public TenantController(ZuHauseContext context)
        {
            _context = context;
        }

        // --- 現有的 View Action 方法 ---
        public IActionResult FrontPage()
        {
            return View();
        }

        public IActionResult Announcement()
        {
            return View();
        }

        public IActionResult Search()
        {
            return View();
        }

        public IActionResult CollectionAndComparison()
        {
            return View();
        }

        // --- 新增的 API 端點 ---

        // GET: /api/Tenant/Announcements/list?pageNumber=1&pageSize=6
        // 這個 Action 用於獲取公告列表，包含分頁資訊
        [HttpGet("api/Tenant/Announcements/list")]
        // 在 TenantController.cs 中的 GetAnnouncementsList 方法
        public async Task<IActionResult> GetAnnouncementsList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 6)
        {
            var query = _context.SiteMessages.AsQueryable();

            // 篩選條件：只顯示類別為 'ANNOUNCEMENT'、啟用中且未被軟刪除的公告
            query = query.Where(sm => sm.Category == "ANNOUNCEMENT" && sm.IsActive == true && sm.DeletedAt == null);

            // 您可能需要根據 StartAt 和 EndAt 篩選，確保只顯示當前有效的公告
            var now = DateTime.Now;
            query = query.Where(sm => sm.StartAt <= now && (sm.EndAt == null || sm.EndAt >= now));


            // 總筆數用於計算總頁數
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var announcements = await query
                .OrderByDescending(sm => sm.UpdatedAt) // 依更新日期降序排列
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(sm => new
                {
                    id = sm.SiteMessagesId,
                    title = sm.Title,
                    // 關鍵：這裡必須使用 sm.SiteMessageContent 來匹配模型和資料庫欄位
                    contentPreview = sm.SiteMessageContent.Length > 100 ? sm.SiteMessageContent.Substring(0, 100) + "..." : sm.SiteMessageContent,
                    updatedAt = sm.UpdatedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Ok(new
            {
                announcements = announcements,
                currentPage = pageNumber,
                totalPages = totalPages,
                totalCount = totalCount
            });
        }

        // GET: /api/Tenant/Announcements/5
        // 這個 Action 用於獲取單條公告的完整內容
        [HttpGet("api/Tenant/Announcements/{id}")]
        // 在 TenantController.cs 中的 GetAnnouncement 方法 (獲取單條公告詳情)
        public async Task<IActionResult> GetAnnouncement(int id)
        {
            var announcement = await _context.SiteMessages.FirstOrDefaultAsync(sm => sm.SiteMessagesId == id);
            if (announcement == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                id = announcement.SiteMessagesId,
                title = announcement.Title,
                // 關鍵：這裡也必須使用 announcement.SiteMessageContent
                content = announcement.SiteMessageContent,
                updatedAt = announcement.UpdatedAt.ToString("yyyy-MM-dd HH:mm")
            });
        }

        // GET: /api/Tenant/StaticContent/terms (或 /disclaimer, /privacy)
        // 這個 Action 用於獲取靜態頁面的內容
        [HttpGet("api/Tenant/StaticContent/{code}")]
        public async Task<IActionResult> GetStaticContent(string code)
        {
            // 假設靜態內容的識別碼 (code) 存儲在 Category 欄位中
            // 並且這些靜態內容也是啟用的
            var staticContent = await _context.SiteMessages
                                            .FirstOrDefaultAsync(sm => sm.Category == code && sm.IsActive && sm.DeletedAt == null);

            if (staticContent == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                Code = staticContent.Category, // 返回 Category 作為 Code
                staticContent.Title,
                Content = staticContent.SiteMessageContent, // 返回 SiteMessageContent 作為 Content
                UpdatedAt = staticContent.UpdatedAt.ToString("yyyy-MM-dd HH:mm")
            });
        }
        // 在 TenantController.cs 中，貼到現有方法 (如 CollectionAndComparison()) 之後
        // ...

        // 這個 API 端點用於測試資料庫連線
        [HttpGet("api/TestDbConnection")]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                // 嘗試從 SiteMessages 集合中讀取資料
                // 這會強制 Entity Framework Core 建立與資料庫的連線
                var messageCount = await _context.SiteMessages.CountAsync();

                // 如果執行到這裡，表示連線成功且讀取到資料
                return Ok($"資料庫連接成功！總共有 {messageCount} 筆網站訊息。");
            }
            catch (Exception ex)
            {
                // 如果發生任何錯誤，捕獲異常並回傳錯誤訊息
                // 這將幫助您診斷問題，例如連接字串錯誤或權限問題
                return StatusCode(500, $"資料庫連接失敗！錯誤訊息：{ex.Message}");
            }
        }

    }
}