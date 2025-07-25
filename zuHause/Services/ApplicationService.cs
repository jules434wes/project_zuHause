using zuHause.Models;
using Microsoft.EntityFrameworkCore;

namespace zuHause.Services
{
    public class ApplicationService
    {
        public readonly ZuHauseContext _context;
        public ApplicationService(ZuHauseContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> UpdateApplicationStatusAsync(int applicationId, string status)
        {
            var application = await _context.RentalApplications
                .Include(a => a.ApplicationStatusLogs)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
                return (false, "找不到對應申請");

            if (application.ApplicationStatusLogs.OrderByDescending(x => x.StatusLogId).FirstOrDefault()?.StatusCode == status)
                return (false, "狀態重複");

            var now = DateTime.Now;

            var log = new ApplicationStatusLog
            {
                ApplicationId = applicationId,
                StatusCode = status,
                ChangedAt = now,
                UpdatedAt = now
            };

            application.CurrentStatus = status;
            application.UpdatedAt = now;

            await _context.ApplicationStatusLogs.AddAsync(log);
            await _context.SaveChangesAsync();

            return (true, "更新成功");
        }
    }

}
