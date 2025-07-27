using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Terminal;
using System.Threading;
using zuHause.Models;
using zuHause.Services;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.Controllers
{
    public class MemberInboxController : Controller
    {
        public readonly ZuHauseContext _context;
        public readonly NotificationService _notificationService;
        public MemberInboxController(ZuHauseContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
        public async Task<IActionResult> Index()
        {
            //所有人都有會員訊息，有房東身分的就會收到房東訊息
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Member");
            }

            var member = await _context.Members.FindAsync(userId);

            bool isLandlord = member!.IsLandlord;

            var AllMsgs = await _context.SystemMessages
                .Where(x =>
                (x.AudienceTypeCode == "ALL_MEMBERS") ||
                (x.AudienceTypeCode == "INDIVIDUAL" && x.ReceiverId == userId) ||
                (x.AudienceTypeCode == "ALL_LANDLORDS" && member.IsLandlord)
                ).ToListAsync();

            List<SystemMessageViewModel> messages = AllMsgs.Select(msg => new SystemMessageViewModel
            {
                SourceType = "SystemMessage", // 系統訊息表
                NotificationId = msg.MessageId,
                AudienceTypeCode = msg.AudienceTypeCode == "INDIVIDUAL" ? "User" : "System", //只分為給個人或系統，個人會顯示通知，系統會顯示公告
                TypeCode = msg.CategoryCode, // 分類型，前端沒想到要用來做甚麼
                Title = msg.Title,
                NotificationContent = msg.MessageContent,
                LinkUrl = msg.AttachmentUrl,
                ModuleCode = "System",
                SourceEntityId = null,
                IsRead = false,
                ReadAt = null,
                SentAt = msg.SentAt,
                DeletedAt = null,
            }).ToList();



            var notification = await _context.UserNotifications
                .Where(u => u.ReceiverId == userId && u.DeletedAt == null)
                .ToListAsync();
            List<SystemMessageViewModel> notificationMsg = notification.Select(msg => new SystemMessageViewModel
            {
                SourceType = "Notification", // 通知訊息表
                NotificationId = msg.NotificationId,
                AudienceTypeCode = "User",
                TypeCode = msg.TypeCode, // 分類型，前端沒想到要用來做甚麼
                Title = msg.Title,
                NotificationContent = msg.NotificationContent,
                LinkUrl = msg.LinkUrl,
                ModuleCode = msg.ModuleCode,
                SourceEntityId = msg.SourceEntityId,
                IsRead = msg.IsRead,
                ReadAt = msg.ReadAt,
                SentAt = msg.SentAt,
                DeletedAt = msg.DeletedAt
            }).ToList();



            var applyRentalIds = notificationMsg
    .Where(n => n.ModuleCode == "ApplyRental" && n.SourceEntityId != null)
    .Select(n => n.SourceEntityId!.Value)
    .Distinct()
    .ToList();


            System.Diagnostics.Debug.WriteLine($"==========={string.Join(',', applyRentalIds)}=================");

            Dictionary<int, List<string>> uploadDict = new();
            if (applyRentalIds.Any())
            {


                uploadDict = await _context.UserUploads
                    .Where(u =>
                        u.ModuleCode == "ApplyRental" &&
                        u.IsActive &&
                        applyRentalIds.Contains(u.SourceEntityId!.Value))
                    .GroupBy(u => u.SourceEntityId!.Value)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(u => Path.Combine("/uploads/rentalApply", u.StoredFileName)).ToList()
                    );
            }

            var rentalIds = notificationMsg
    .Where(x => x.ModuleCode == "ApplyRental" && x.SourceEntityId != null)
    .Select(x => x.SourceEntityId!.Value)
    .ToList();

            var rentalDict = _context.RentalApplications
                .Where(r => rentalIds.Contains(r.ApplicationId))
                .ToDictionary(r => r.ApplicationId, r => r.CurrentStatus);

            foreach (var msg in notificationMsg)
            {
                if (msg.ModuleCode == "ApplyRental" && msg.SourceEntityId != null)
                {
                    if (rentalDict.TryGetValue(msg.SourceEntityId.Value, out var status))
                    {
                        msg.ApplicationStatus = status;
                    }

                    if (uploadDict.TryGetValue(msg.SourceEntityId.Value, out var imgList))
                    {
                        msg.ImageUrls = imgList;
                    }
                    else
                    {
                        msg.ImageUrls = new List<string>();
                    }
                }
            }

            List<SystemMessageViewModel> totalMessages = new List<SystemMessageViewModel>();
            totalMessages.AddRange(messages);
            totalMessages.AddRange(notificationMsg);
            totalMessages = totalMessages.OrderByDescending(x => x.SentAt).ToList();
            return View(totalMessages);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsReadApi([FromBody] int notificationId)
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId);
            if (result.Success)
                return Ok(new { message = "標記為已讀成功" });
            return BadRequest(new { message = result.Message });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var result = await _notificationService.SoftDeleteAsync(notificationId);
            if (result.Success)
            {
                TempData["SuccessMessageTitle"] = "通知";
                TempData["SuccessMessageContent"] = "刪除成功";
            }
            return RedirectToAction("Index");
        }


    }
}
