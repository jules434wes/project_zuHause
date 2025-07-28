using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;

namespace zuHause.Controllers
{
    public class AnnouncementController : Controller
    {
        public IActionResult Announcement()
        {
            return View();
        }

        // 將 DbContext 類型更正為 ZuHauseContext
        private readonly ZuHauseContext _context; // 依賴注入您的 ZuHauseContext

        // Controller 的建構子，用於接收 ZuHauseContext
        public AnnouncementController(ZuHauseContext context)
        {
            _context = context;
        }

        // --- 新增的 API 端點 ---
        //Announcements
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
            //var now = DateTime.Now;
            //query = query.Where(sm => sm.StartAt <= now && (sm.EndAt == null || sm.EndAt >= now));


            // 總筆數用於計算總頁數
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var rawAnnouncements = await query
     .OrderByDescending(sm => sm.UpdatedAt)
     .Skip((pageNumber - 1) * pageSize)
     .Take(pageSize)
     // 注意：這裡只選擇原始的 moduleScope，不進行轉換
     .Select(sm => new
     {
         sm.SiteMessagesId,
         sm.Title,
         sm.ModuleScope, // 原始的 ModuleScope
         sm.SiteMessageContent,
         sm.UpdatedAt,
         sm.AttachmentUrl
     })
     .ToListAsync(); // <-- 在這裡將資料庫查詢結果載入到記憶體

            // 然後在記憶體中對載入的資料進行處理和轉換
            var announcements = rawAnnouncements.Select(sm => new
            {
                id = sm.SiteMessagesId,
                title = sm.Title,
                // 現在可以在記憶體中使用 switch 表達式進行轉換
                moduleScope = sm.ModuleScope switch
                {
                    "TENANT" => "[租客]",
                    "LANDLORD" => "[房東]",
                    "FURNITURE" => "[家具]",
                    "COMMON" => "[營運]",
                    _ => "" // 如果都不符合，為空字串
                },
                contentPreview = sm.SiteMessageContent.Length > 100 ? sm.SiteMessageContent.Substring(0, 100) + "..." : sm.SiteMessageContent,
                updatedAt = sm.UpdatedAt.ToString("yyyy-MM-dd HH:mm"),
                attachmentUrl = sm.AttachmentUrl
            }).ToList(); // 轉換為最終的列表

            return Ok(new
            {
                announcements = announcements,
                currentPage = pageNumber,
                totalPages = totalPages
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
                attachmentUrl = announcement.AttachmentUrl,
                content = announcement.SiteMessageContent,
                updatedAt = announcement.UpdatedAt.ToString("yyyy-MM-dd HH:mm"),

            });
        }
    }
}
