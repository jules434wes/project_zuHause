using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using zuHause.Models;

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

        public IViewComponentResult Invoke()
        {

            var userId = HttpContext.User.FindFirst("UserId")?.Value;
            string avatar = "~/images/user-image.jpg";

            if (!string.IsNullOrEmpty(userId))
            {


                var cached = _cache.Get<string>($"Avatar_{userId}");
                if (!string.IsNullOrEmpty(cached))
                {
                    avatar = cached;
                }
                else
                {
                    var photoPath = _context.Members.Join(_context.UserUploads,
                    m => new { m.MemberId },
                    u => new { u.MemberId },
                    (m, u) => new
                    {
                        MemberId = m.MemberId,
                        UploadId = u.UploadId,
                        PhotoPath = u.FilePath,
                        UploadTypeCode = u.UploadTypeCode,
                    }
                    ).Where((x) => x.MemberId == Convert.ToInt32(userId) && x.UploadTypeCode == "USER_IMG")
                    .OrderByDescending(x => x.UploadId)
                    .FirstOrDefault()?.PhotoPath;
                    if(photoPath != null)
                    {
                        avatar = $"~{photoPath}";
                        _cache.Set($"Avatar_{userId}", avatar);
                    }
                }
            }
            return View("Default", avatar);
        }
    }
}
