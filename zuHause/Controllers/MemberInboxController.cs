using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;

namespace zuHause.Controllers
{
    public class MemberInboxController : Controller
    {
        public readonly ZuHauseContext _context;
        public MemberInboxController (ZuHauseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            //所有人都有會員訊息，有房東身分的就會收到房東訊息
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login","Member");
            }

            var member = await _context.Members.FindAsync(userId);

            bool isLandlord = member!.IsLandlord;

            var AllMsgs = await _context.SystemMessages
                .Where(x => 
                x.AudienceTypeCode == "ALL_MEMBERS" || 
                x.AudienceTypeCode == "INDIVIDUAL" && x.ReceiverId == userId || 
                x.AudienceTypeCode == "ALL_LANDLORDS" && member.IsLandlord
                ).ToListAsync();




            return View(AllMsgs);
        }
    }
}
