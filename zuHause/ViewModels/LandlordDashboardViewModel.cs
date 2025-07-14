namespace zuHause.ViewModels
{
    public class LandlordDashboardViewModel
    {
        public int MemberId { get; set; }
        public string LandlordName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        
        // 統計資訊
        public int TotalProperties { get; set; }
        public int ActiveProperties { get; set; }
        public int PendingApplications { get; set; }
        public int TotalFavorites { get; set; }
        public decimal TotalRevenue { get; set; }
        
        // 房源列表
        public List<LandlordPropertyViewModel> Properties { get; set; } = new List<LandlordPropertyViewModel>();
        
        // 最新申請
        public List<RentalApplicationSummaryViewModel> RecentApplications { get; set; } = new List<RentalApplicationSummaryViewModel>();
        
        // 訊息通知
        public List<NotificationViewModel> Notifications { get; set; } = new List<NotificationViewModel>();
    }
    
    public class LandlordPropertyViewModel
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ViewCount { get; set; }
        public int FavoriteCount { get; set; }
        public int ApplicationCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MainImagePath { get; set; } = string.Empty;
    }
    
    public class RentalApplicationSummaryViewModel
    {
        public int RentalApplicationId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
    
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }
}