using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using zuHause.Models;
using zuHause.ViewModels;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.ViewComponents
{
    public class NavbarMemberViewComponent : ViewComponent
    {
        public readonly ZuHauseContext _context;
        private readonly IMemoryCache _cache;
        public NavbarMemberViewComponent(ZuHauseContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new NavbarMemberViewModel();
            
            // 檢查用戶是否已登入
            viewModel.IsAuthenticated = HttpContext.User.Identity?.IsAuthenticated == true;
            
            if (!viewModel.IsAuthenticated)
            {
                return View("Default", viewModel);
            }
            // 取得用戶基本資訊
            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;

            int userId;
            Member? member = null;
            if (int.TryParse(userIdClaim, out userId))
            {
                member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == userId);
                viewModel.UserId = userId;
                
                // 取得用戶詳細資訊 (包含角色狀態)
                    
                if (member != null)
                {
                    viewModel.IsLandlord = member.IsLandlord;
                    viewModel.MemberTypeId = member.MemberTypeId;
                    viewModel.IsIdentityVerified = member.IdentityVerifiedAt.HasValue;
                }
            }
            
            // 處理頭像邏輯 (保持原有邏輯，優化查詢)
            var cached = _cache.Get<string>($"Avatar_{userIdClaim}");
            if (!string.IsNullOrEmpty(cached))
            {
                viewModel.Avatar = cached;
            }
            else
            {
                var photoPath = await _context.Members
                    .Join(_context.UserUploads,
                        m => new { m.MemberId },
                        u => new { u.MemberId },
                        (m, u) => new
                        {
                            MemberId = m.MemberId,
                            UploadId = u.UploadId,
                            PhotoPath = u.FilePath,
                            UploadTypeCode = u.UploadTypeCode,
                        })
                    .Where(x => x.MemberId == userId && x.UploadTypeCode == "USER_IMG")
                    .OrderByDescending(x => x.UploadId)
                    .Select(x => x.PhotoPath)
                    .FirstOrDefaultAsync();
                    
                if (photoPath != null)
                {
                    viewModel.Avatar = $"~{photoPath}";
                    _cache.Set($"Avatar_{userIdClaim}", viewModel.Avatar, TimeSpan.FromMinutes(15));
                }
            }
            // 取得兩種訊息來源
            var systemMsgs = await _context.SystemMessages
                .Where(x =>
                    (x.AudienceTypeCode == "ALL_MEMBERS" ||
                    x.AudienceTypeCode == "INDIVIDUAL" && x.ReceiverId == userId ||
                    x.AudienceTypeCode == "ALL_LANDLORDS" && member!.IsLandlord))
                .Select(x => new SystemMessageViewModel
                {
                    SourceType = "SystemMessage",
                    NotificationId = x.MessageId,
                    AudienceTypeCode = x.AudienceTypeCode == "INDIVIDUAL" ? "User" : "System",
                    TypeCode = x.CategoryCode,
                    Title = x.Title,
                    NotificationContent = x.MessageContent,
                    LinkUrl = x.AttachmentUrl,
                    ModuleCode = "System",
                    SourceEntityId = null,
                    IsRead = false,
                    ReadAt = null,
                    SentAt = x.SentAt,
                    DeletedAt = null,
                })
                .ToListAsync();

            var userNotis = await _context.UserNotifications
                .Where(x => x.ReceiverId == userId && x.DeletedAt == null)
                .Select(x => new SystemMessageViewModel
                {
                    SourceType = "Notification",
                    NotificationId = x.NotificationId,
                    AudienceTypeCode = "User",
                    TypeCode = x.TypeCode,
                    Title = x.Title,
                    NotificationContent = x.NotificationContent,
                    LinkUrl = x.LinkUrl,
                    ModuleCode = x.ModuleCode,
                    SourceEntityId = x.SourceEntityId,
                    IsRead = x.IsRead,
                    ReadAt = x.ReadAt,
                    SentAt = x.SentAt,
                    DeletedAt = x.DeletedAt,
                })
                .ToListAsync();

            int count = userNotis.Where(x=>x.IsRead == false && x.AudienceTypeCode != "System").Count();

            // 合併後排序取前 3 筆
            var merged = systemMsgs
                .Concat(userNotis)
                .OrderByDescending(x => x.SentAt)
                .Take(5)
                .ToList();
            viewModel.NeverReadCount = count;
            // 放入 ViewModel
            viewModel.LatestNotifications = merged;



            return View("Default", viewModel);
        }
    }
}