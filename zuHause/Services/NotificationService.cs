using Microsoft.AspNetCore.Http.HttpResults;
using zuHause.Models;

namespace zuHause.Services
{
    public class NotificationService
    {
        private readonly ZuHauseContext _context;

        public NotificationService(ZuHauseContext context)
        {
            _context = context;
        }

        // 新增一個通知
        public async Task<(bool Success, string Message)> CreateUserNotificationAsync(int receiverId, string typeCode, string title, string content, string ModuleCode, int sourceEntityId)
        {
            var notification = new UserNotification
            {
                ReceiverId = receiverId, // 接收者id
                TypeCode = typeCode, // 通知類型代碼
                Title = title,
                NotificationContent = content,
                ModuleCode = ModuleCode,
                SourceEntityId = sourceEntityId,
                StatusCode = "NEW",
                IsRead = false,
                SentAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.UserNotifications.Add(notification);
            await _context.SaveChangesAsync();
            return (true, "發送成功");
        }

        // 更改通知已讀
        public async Task<(bool Success, string Message)> MarkAsReadAsync(int notificationId)
        {
            var notification = _context.UserNotifications.Where(x=>x.NotificationId == notificationId).FirstOrDefault();

            if (notification == null)
            {
                return (false, "Notification not found.");
            }
            if(notification.IsRead == true)
            {
                return (false, "已經已讀");
            }
            notification.IsRead = true;
            notification.UpdatedAt = DateTime.Now;
            _context.UserNotifications.Update(notification);
            await _context.SaveChangesAsync();
            return (true, "標記成功");
        }

        // 軟刪除通知
        public async Task<(bool Success, string Message)> SoftDeleteAsync(int notificationId)
        {
            var notification = _context.UserNotifications.Where(x => x.NotificationId == notificationId).FirstOrDefault();

            if (notification == null)
            {
                return (false, "Notification not found.");
            }
            if (notification.DeletedAt != null)
            {
                return (false, "錯誤");
            }
            notification.DeletedAt = DateTime.Now;
            notification.UpdatedAt = DateTime.Now;
            _context.UserNotifications.Update(notification);
            await _context.SaveChangesAsync();

            return (true, "刪除成功");
        }


    }
}
