using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using zuHause.Models;
using zuHause.ViewModels;
using System.Security.Claims;

namespace zuHause.Controllers
{
    public class LandlordController : Controller
    {
        private readonly ILogger<LandlordController> _logger;
        private readonly ZuHauseContext _context;
        
        public LandlordController(ILogger<LandlordController> logger, ZuHauseContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /landlord/become
        [HttpGet("landlord/become")]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> Become()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Member");
            }
            
            var member = await _context.Members.FindAsync(int.Parse(userId));
            if (member == null)
            {
                return RedirectToAction("Login", "Member");
            }
            
            var viewModel = new LandlordApplicationViewModel
            {
                MemberName = member.MemberName,
                IsAlreadyLandlord = member.IsLandlord,
                CanApply = CheckEligibility(member, out string errorMessage),
                ErrorMessage = errorMessage
            };
            
            return View(viewModel);
        }
        
        // POST: /landlord/agreetotems
        [HttpPost("landlord/agreetotems")]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> AgreeToTerms()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Member");
            }
            
            var member = await _context.Members.FindAsync(int.Parse(userId));
            if (member == null)
            {
                return RedirectToAction("Login", "Member");
            }
            
            // 再次檢查資格（防止前端繞過）
            if (!CheckEligibility(member, out string errorMessage))
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("Become");
            }
            
            if (!member.IsLandlord)
            {
                member.IsLandlord = true;
                member.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "恭喜您！已成功申請成為房東。";
            }
            
            return RedirectToAction("Become");
        }
        
        private bool CheckEligibility(Member member, out string errorMessage)
        {
            if (!member.IsActive)
            {
                errorMessage = "很抱歉，您不符合申請資格。";
                return false;
            }
            
            if (member.IdentityVerifiedAt == null || 
                member.PhoneVerifiedAt == null || 
                string.IsNullOrEmpty(member.NationalIdNo))
            {
                errorMessage = "請先完善會員資料後再進行申請。";
                return false;
            }
            
            errorMessage = string.Empty;
            return true;
        }
    }
} 