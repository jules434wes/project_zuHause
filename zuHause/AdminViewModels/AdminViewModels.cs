using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace zuHause.AdminViewModels
{
    // 使用者管理相關 ViewModels
    public class UserListViewModel : BaseListViewModel<MemberData>
    {
        public UserListViewModel()
        {
            PageTitle = "使用者管理";
            Items = GenerateMockUsers();
            TotalCount = Items.Count;
            
            // 批量操作設定
            BulkConfig = new BulkActionConfig
            {
                SelectAllId = "selectAllUsers",
                CheckboxClass = "user-checkbox",
                BulkButtonId = "bulkMessageBtn",
                BulkButtonText = "批量發送訊息"
            };
        }

        private List<MemberData> GenerateMockUsers()
        {
            return new List<MemberData>
            {
                new MemberData { MemberID = "M001", MemberName = "王小明", Email = "wang@example.com", NationalNo = "A123456789", PhoneNumber = "0912345678", AccountStatus = "active", VerificationStatus = "verified", IsLandlord = true, RegistrationDate = "2024-01-15" },
                new MemberData { MemberID = "M002", MemberName = "李小華", Email = "lee@example.com", NationalNo = "B234567890", PhoneNumber = "0923456789", AccountStatus = "active", VerificationStatus = "pending", IsLandlord = false, RegistrationDate = "2024-02-20" },
                new MemberData { MemberID = "M003", MemberName = "張小美", Email = "zhang@example.com", NationalNo = "C345678901", PhoneNumber = "0934567890", AccountStatus = "suspended", VerificationStatus = "verified", IsLandlord = true, RegistrationDate = "2024-03-10" }
            };
        }
    }

    public class UserDetailsViewModel : BaseDetailsViewModel<MemberData>
    {
        public UserDetailsViewModel(string memberId = "M001")
        {
            PageTitle = "會員詳情";
            Data = GenerateMockUser(memberId);
            
            Tabs = new List<TabConfig>
            {
                new TabConfig { TabId = "basicInfo", Title = "基本資料", IsDefault = true },
                new TabConfig { TabId = "renterActivity", Title = "租客活動", BadgeCount = 5 },
                new TabConfig { TabId = "landlordActivity", Title = "房東活動", BadgeCount = Data.IsLandlord ? 3 : null },
                new TabConfig { TabId = "orderHistory", Title = "訂單記錄", BadgeCount = 8 },
                new TabConfig { TabId = "supportHistory", Title = "溝通與支援", BadgeCount = 2 }
            };
        }

        private MemberData GenerateMockUser(string memberId)
        {
            return new MemberData 
            { 
                MemberID = memberId, 
                MemberName = "王小明", 
                Email = "wang@example.com", 
                NationalNo = "A123456789", 
                PhoneNumber = "0912345678", 
                AccountStatus = "active", 
                VerificationStatus = "verified", 
                IsLandlord = true, 
                RegistrationDate = "2024-01-15" 
            };
        }
    }

    // 物業管理相關 ViewModels
    public class PropertyListViewModel : BaseListViewModel<PropertyData>
    {
        public PropertyListViewModel()
        {
            PageTitle = "物業管理";
            Items = GenerateMockProperties();
            TotalCount = Items.Count;
            
            BulkConfig = new BulkActionConfig
            {
                SelectAllId = "selectAllProperties",
                CheckboxClass = "property-checkbox",
                BulkButtonId = "bulkVerifyBtn",
                BulkButtonText = "批量審核"
            };
        }

        private List<PropertyData> GenerateMockProperties()
        {
            return new List<PropertyData>
            {
                new PropertyData { PropertyID = "P001", PropertyTitle = "台北市中心雅房", LandlordName = "王小明", PropertyType = "雅房", RentPrice = 15000, Status = "verified", SubmissionDate = "2024-01-15" },
                new PropertyData { PropertyID = "P002", PropertyTitle = "新北市套房出租", LandlordName = "李小華", PropertyType = "套房", RentPrice = 20000, Status = "pending", SubmissionDate = "2024-02-20" },
                new PropertyData { PropertyID = "P003", PropertyTitle = "桃園市整層公寓", LandlordName = "張小美", PropertyType = "整層住家", RentPrice = 35000, Status = "rejected", SubmissionDate = "2024-03-10" }
            };
        }
    }

    // 客服管理相關 ViewModels
    public class CustomerServiceDetailsViewModel : BaseDetailsViewModel<CustomerServiceCase>
    {
        public CustomerServiceDetailsViewModel(string caseId = "CS001")
        {
            PageTitle = "客服案件詳情";
            Data = GenerateMockCase(caseId);
            IsEditMode = true;
        }

        private CustomerServiceCase GenerateMockCase(string caseId)
        {
            return new CustomerServiceCase
            {
                CaseID = caseId,
                CaseTitle = "租屋糾紛處理",
                MemberName = "王小明",
                Category = "rental_dispute",
                Priority = "high",
                Status = "processing",
                CreatedDate = "2024-07-10",
                AssignedAdmin = "Admin001"
            };
        }
    }

    // 系統訊息相關 ViewModels
    public class SystemMessageListViewModel : BaseListViewModel<SystemMessageData>
    {
        public SystemMessageListViewModel()
        {
            PageTitle = "系統訊息管理";
            Items = GenerateMockMessages();
            TotalCount = Items.Count;
            HasBulkActions = false;
        }

        private List<SystemMessageData> GenerateMockMessages()
        {
            return new List<SystemMessageData>
            {
                new SystemMessageData { MessageID = "SM001", MessageTitle = "系統維護通知", Category = "maintenance", Audience = "all_members", AdminName = "系統管理員", SendDate = "2024-07-10", Status = "sent" },
                new SystemMessageData { MessageID = "SM002", MessageTitle = "優惠活動公告", Category = "promotion", Audience = "all_landlords", AdminName = "行銷部", SendDate = "2024-07-08", Status = "draft" },
                new SystemMessageData { MessageID = "SM003", MessageTitle = "個別用戶通知", Category = "announcement", Audience = "individual", AdminName = "客服部", SendDate = "2024-07-05", Status = "sent" }
            };
        }
    }

    public class SystemMessageNewViewModel : BaseAdminViewModel
    {
        public SystemMessageNewViewModel()
        {
            PageTitle = "新增系統訊息";
        }

        [Required(ErrorMessage = "訊息標題為必填")]
        public string MessageTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "訊息內容為必填")]
        public string MessageContent { get; set; } = string.Empty;

        [Required(ErrorMessage = "發送對象為必填")]
        public string Audience { get; set; } = string.Empty;

        [Required(ErrorMessage = "訊息分類為必填")]
        public string Category { get; set; } = string.Empty;

        public string? SelectedUserId { get; set; }
        public bool IsScheduled { get; set; } = false;
        public string? ScheduledDate { get; set; }
    }

    // 資料模型類別
    public class MemberData
    {
        public string MemberID { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NationalNo { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public bool IsLandlord { get; set; }
        public string RegistrationDate { get; set; } = string.Empty;
        
        // 額外屬性用於UI顯示
        public bool IsActive => AccountStatus == "active";
        public bool IsIdentityVerified => VerificationStatus == "verified";
    }

    public class PropertyData
    {
        public string PropertyID { get; set; } = string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;
        public string LandlordName { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public int RentPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SubmissionDate { get; set; } = string.Empty;
    }

    public class CustomerServiceCase
    {
        public string CaseID { get; set; } = string.Empty;
        public string CaseTitle { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
        public string AssignedAdmin { get; set; } = string.Empty;
    }

    public class SystemMessageData
    {
        public string MessageID { get; set; } = string.Empty;
        public string MessageTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public string SendDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}