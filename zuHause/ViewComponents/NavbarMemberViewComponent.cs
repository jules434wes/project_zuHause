using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using zuHause.Models;
using zuHause.ViewModels;
using System.Security.Claims;

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
            if (int.TryParse(userIdClaim, out var userId))
            {
                viewModel.UserId = userId;
                
                // 取得用戶詳細資訊 (包含角色狀態)
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.MemberId == userId);
                    
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
            
            return View("Default", viewModel);
        }
    }
}