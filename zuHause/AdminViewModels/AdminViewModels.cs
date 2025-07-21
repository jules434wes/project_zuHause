using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using zuHause.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace zuHause.AdminViewModels
{
    // 使用者管理相關 ViewModels
    public class AdminUserListViewModel : BaseListViewModel<MemberData>
    {
        public AdminUserListViewModel()
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

        public AdminUserListViewModel(ZuHauseContext context)
        {
            PageTitle = "會員管理";
            Items = LoadUsersFromDatabase(context);
            TotalCount = Items.Count;
            PendingVerificationUsers = LoadPendingVerificationUsers(context);
            PendingLandlordUsers = LoadPendingLandlordUsers(context);
            
            // 批量操作設定
            BulkConfig = new BulkActionConfig
            {
                SelectAllId = "selectAllUsers",
                CheckboxClass = "user-checkbox",
                BulkButtonId = "bulkMessageBtn",
                BulkButtonText = "批量發送訊息"
            };
        }

        public AdminUserListViewModel(ZuHauseContext context, bool landlordsOnly)
        {
            PageTitle = landlordsOnly ? "房東管理" : "會員管理";
            Items = LoadUsersFromDatabase(context, landlordsOnly);
            TotalCount = Items.Count;
            PendingVerificationUsers = LoadPendingVerificationUsers(context, landlordsOnly);
            PendingLandlordUsers = LoadPendingLandlordUsers(context, landlordsOnly);
            
            // 批量操作設定
            BulkConfig = new BulkActionConfig
            {
                SelectAllId = "selectAllUsers",
                CheckboxClass = "user-checkbox",
                BulkButtonId = "bulkMessageBtn",
                BulkButtonText = "批量發送訊息"
            };
        }

        public List<MemberData> PendingVerificationUsers { get; set; } = new List<MemberData>();
        public List<MemberData> PendingLandlordUsers { get; set; } = new List<MemberData>();

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            
            return text.Substring(0, maxLength) + "...";
        }

        private List<MemberData> LoadUsersFromDatabase(ZuHauseContext context)
        {
            return LoadUsersFromDatabase(context, false);
        }

        private List<MemberData> LoadUsersFromDatabase(ZuHauseContext context, bool landlordsOnly)
        {
            var query = context.Members.AsQueryable();
            
            if (landlordsOnly)
            {
                query = query.Where(m => m.IsLandlord);
            }
            
            var members = query
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new 
                {
                    Member = m,
                    HasPendingApproval = context.Approvals.Any(a => a.ApplicantMemberId == m.MemberId && 
                                                                   a.ModuleCode == "IDENTITY" && 
                                                                   a.StatusCode == "PENDING"),
                    ResidenceCity = context.Cities.FirstOrDefault(c => c.CityId == m.ResidenceCityId),
                    PrimaryRentalCity = context.Cities.FirstOrDefault(c => c.CityId == m.PrimaryRentalCityId)
                })
                .Select(x => new MemberData
                {
                    MemberID = x.Member.MemberId.ToString(),
                    MemberName = x.Member.MemberName,
                    Email = x.Member.Email,
                    NationalNo = x.Member.NationalIdNo ?? "",
                    PhoneNumber = x.Member.PhoneNumber,
                    AccountStatus = x.Member.IsActive ? "active" : "inactive",
                    VerificationStatus = x.Member.IdentityVerifiedAt.HasValue ? "verified" : 
                                       (x.HasPendingApproval ? "pending" : "unverified"),
                    IsLandlord = x.Member.IsLandlord,
                    RegistrationDate = x.Member.CreatedAt.ToString("yyyy-MM-dd"),
                    LastLoginTime = x.Member.LastLoginAt.HasValue ? x.Member.LastLoginAt.Value.ToString("yyyy-MM-dd") : "--",
                    
                    // 進階篩選屬性
                    Gender = (int)x.Member.Gender,
                    ResidenceCityID = x.Member.ResidenceCityId ?? 0,
                    PrimaryRentalCityID = x.Member.PrimaryRentalCityId ?? 0,
                    ResidenceCityName = x.ResidenceCity != null ? x.ResidenceCity.CityName : "",
                    PrimaryRentalCityName = x.PrimaryRentalCity != null ? x.PrimaryRentalCity.CityName : ""
                })
                .ToList();

            return members;
        }

        private List<MemberData> LoadPendingVerificationUsers(ZuHauseContext context)
        {
            return LoadPendingVerificationUsers(context, false);
        }

        private List<MemberData> LoadPendingVerificationUsers(ZuHauseContext context, bool landlordsOnly)
        {
            var query = context.Members
                .Join(context.Approvals,
                      m => m.MemberId,
                      a => a.ApplicantMemberId,
                      (m, a) => new { Member = m, Approval = a })
                .Where(x => x.Member.IsActive && 
                           x.Approval.ModuleCode == "IDENTITY" && 
                           x.Approval.StatusCode == "PENDING");

            if (landlordsOnly)
            {
                query = query.Where(x => x.Member.IsLandlord);
            }

            var pendingUsers = query
                .OrderBy(x => x.Approval.CreatedAt)
                .Select(x => new MemberData
                {
                    MemberID = x.Member.MemberId.ToString(),
                    MemberName = x.Member.MemberName,
                    Email = x.Member.Email,
                    NationalNo = x.Member.NationalIdNo ?? "",
                    PhoneNumber = x.Member.PhoneNumber,
                    AccountStatus = x.Member.IsActive ? "active" : "inactive",
                    VerificationStatus = "pending",
                    IsLandlord = x.Member.IsLandlord,
                    RegistrationDate = x.Member.CreatedAt.ToString("yyyy-MM-dd"),
                    LastLoginTime = x.Member.LastLoginAt.HasValue ? x.Member.LastLoginAt.Value.ToString("yyyy-MM-dd") : "--",
                    ApplyTime = x.Approval.CreatedAt.ToString("yyyy-MM-dd"),
                    HasIdentityUploads = context.UserUploads.Any(u => u.MemberId == x.Member.MemberId && 
                                                                     u.ModuleCode == "MemberInfo" && 
                                                                     u.IsActive &&
                                                                     (u.UploadTypeCode == "USER_ID_FRONT" || u.UploadTypeCode == "USER_ID_BACK"))
                })
                .ToList();

            return pendingUsers;
        }

        private List<MemberData> LoadPendingLandlordUsers(ZuHauseContext context)
        {
            return LoadPendingLandlordUsers(context, false);
        }

        private List<MemberData> LoadPendingLandlordUsers(ZuHauseContext context, bool landlordsOnly)
        {
            var query = context.Members
                .Join(context.Approvals,
                      m => m.MemberId,
                      a => a.ApplicantMemberId,
                      (m, a) => new { Member = m, Approval = a })
                .Where(x => x.Member.IsActive && 
                           x.Approval.ModuleCode == "LANDLORD" && 
                           x.Approval.StatusCode == "PENDING");

            if (landlordsOnly)
            {
                query = query.Where(x => x.Member.IsLandlord);
            }

            var pendingLandlords = query
                .OrderBy(x => x.Approval.CreatedAt)
                .Select(x => new MemberData
                {
                    MemberID = x.Member.MemberId.ToString(),
                    MemberName = x.Member.MemberName,
                    Email = x.Member.Email,
                    NationalNo = x.Member.NationalIdNo ?? "",
                    PhoneNumber = x.Member.PhoneNumber,
                    AccountStatus = x.Member.IsActive ? "active" : "inactive",
                    VerificationStatus = "pending",
                    IsLandlord = x.Member.IsLandlord,
                    RegistrationDate = x.Member.CreatedAt.ToString("yyyy-MM-dd"),
                    LastLoginTime = x.Member.LastLoginAt.HasValue ? x.Member.LastLoginAt.Value.ToString("yyyy-MM-dd") : "--",
                    ApplyTime = x.Approval.CreatedAt.ToString("yyyy-MM-dd"),
                    HasIdentityUploads = context.UserUploads.Any(u => u.MemberId == x.Member.MemberId && 
                                                                     u.ModuleCode == "MemberInfo" && 
                                                                     u.IsActive &&
                                                                     (u.UploadTypeCode == "USER_ID_FRONT" || u.UploadTypeCode == "USER_ID_BACK"))
                })
                .ToList();

            return pendingLandlords;
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

    public class AdminUserDetailsViewModel : BaseDetailsViewModel<MemberData>
    {
        public AdminUserDetailsViewModel(ZuHauseContext context, int memberId)
        {
            PageTitle = "會員詳情";
            Data = LoadMemberFromDatabase(context, memberId);
            
            Tabs = new List<TabConfig>
            {
                new TabConfig { TabId = "basicInfo", Title = "基本資料", IsDefault = true },
                new TabConfig { TabId = "renterActivity", Title = "租客活動", BadgeCount = GetRenterActivityCount(context, memberId) },
                new TabConfig { TabId = "landlordActivity", Title = "房東活動", BadgeCount = Data.IsLandlord ? GetLandlordActivityCount(context, memberId) : null },
                new TabConfig { TabId = "orderHistory", Title = "訂單記錄", BadgeCount = GetOrderHistoryCount(context, memberId) },
                new TabConfig { TabId = "supportHistory", Title = "溝通與支援", BadgeCount = GetSupportHistoryCount(context, memberId) }
            };
        }

        private MemberData LoadMemberFromDatabase(ZuHauseContext context, int memberId)
        {
            var member = context.Members
                .Include(m => m.ResidenceCity)
                .Include(m => m.PrimaryRentalCity)
                .FirstOrDefault(m => m.MemberId == memberId);
            
            if (member == null)
            {
                throw new ArgumentException($"找不到會員ID: {memberId}");
            }

            var hasPendingApproval = context.Approvals.Any(a => a.ApplicantMemberId == memberId && 
                                                               a.ModuleCode == "IDENTITY" && 
                                                               a.StatusCode == "PENDING");

            var verificationStatus = member.IdentityVerifiedAt.HasValue ? "verified" : 
                                   (hasPendingApproval ? "pending" : "unverified");

            var memberData = new MemberData 
            { 
                MemberID = member.MemberId.ToString(), 
                MemberName = member.MemberName, 
                Email = member.Email, 
                NationalNo = member.NationalIdNo ?? "", 
                PhoneNumber = member.PhoneNumber, 
                AccountStatus = member.IsActive ? "active" : "inactive", 
                VerificationStatus = verificationStatus, 
                IsLandlord = member.IsLandlord, 
                RegistrationDate = member.CreatedAt.ToString("yyyy-MM-dd"),
                LastLoginTime = member.LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? "--",
                
                // 詳細資料
                BirthDate = member.BirthDate != default(DateOnly) ? member.BirthDate.ToDateTime(TimeOnly.MinValue) : null,
                Gender = (int)member.Gender,
                ResidenceCityID = member.ResidenceCityId ?? 0,
                ResidenceCityName = member.ResidenceCity?.CityName ?? "",
                PrimaryRentalCityID = member.PrimaryRentalCityId ?? 0,
                PrimaryRentalCityName = member.PrimaryRentalCity?.CityName ?? "",
                DetailedAddress = member.AddressLine ?? "",
                PreferredRentalAreas = GetPreferredRentalAreas(context, memberId),
                
                // 驗證時間
                PhoneVerifiedAt = member.PhoneVerifiedAt,
                EmailVerifiedAt = member.EmailVerifiedAt,
                IdentityVerifiedAt = member.IdentityVerifiedAt,
                
                HasIdentityUploads = context.UserUploads.Any(u => u.MemberId == memberId && 
                                                                  u.ModuleCode == "MemberInfo" && 
                                                                  u.IsActive &&
                                                                  (u.UploadTypeCode == "USER_ID_FRONT" || u.UploadTypeCode == "USER_ID_BACK"))
            };

            // 載入活動相關資料
            memberData.RentalContracts = LoadRentalContracts(context, memberId);
            memberData.OwnedProperties = LoadOwnedProperties(context, memberId);
            memberData.ReceivedComplaints = LoadReceivedComplaints(context, memberId);
            memberData.FurnitureOrders = LoadFurnitureOrders(context, memberId);
            memberData.CustomerServiceTickets = LoadCustomerServiceTickets(context, memberId);
            memberData.SubmittedComplaints = LoadSubmittedComplaints(context, memberId);

            return memberData;
        }

        private int GetRenterActivityCount(ZuHauseContext context, int memberId)
        {
            return context.RentalApplications.Count(ra => ra.MemberId == memberId);
        }

        private int GetLandlordActivityCount(ZuHauseContext context, int memberId)
        {
            return context.Properties.Count(p => p.LandlordMemberId == memberId);
        }

        private int GetOrderHistoryCount(ZuHauseContext context, int memberId)
        {
            return context.FurnitureOrders.Count(fo => fo.MemberId == memberId);
        }

        private int GetSupportHistoryCount(ZuHauseContext context, int memberId)
        {
            return context.CustomerServiceTickets.Count(cst => cst.MemberId == memberId);
        }

        private string GetPreferredRentalAreas(ZuHauseContext context, int memberId)
        {
            var areas = context.RenterPosts
                .Where(rp => rp.MemberId == memberId)
                .Join(context.Districts, rp => rp.DistrictId, d => d.DistrictId, (rp, d) => d.DistrictName)
                .Distinct()
                .ToList();
            
            return string.Join("、", areas);
        }

        private List<RentalContractData> LoadRentalContracts(ZuHauseContext context, int memberId)
        {
            return context.Contracts
                .Include(c => c.RentalApplication)
                .ThenInclude(ra => ra.Property)
                .ThenInclude(p => p.LandlordMember)
                .Where(c => c.RentalApplication.MemberId == memberId)
                .Select(c => new RentalContractData
                {
                    ContractId = c.ContractId.ToString(),
                    PropertyTitle = c.RentalApplication.Property.Title,
                    LandlordName = c.RentalApplication.Property.LandlordMember.MemberName,
                    RentPeriod = c.StartDate.ToString("yyyy-MM-dd") + " ~ " + (c.EndDate.HasValue ? c.EndDate.Value.ToString("yyyy-MM-dd") : "--"),
                    Status = c.Status == "ACTIVE" ? "生效中" : "已結束",
                    PropertyUrl = $"/Property/Details/{c.RentalApplication.Property.PropertyId}",
                    LandlordUrl = $"/Admin/admin_userDetails/{c.RentalApplication.Property.LandlordMember.MemberId}"
                })
                .OrderByDescending(c => c.ContractId)
                .ToList();
        }

        private List<PropertyData> LoadOwnedProperties(ZuHauseContext context, int memberId)
        {
            return context.Properties
                .Where(p => p.LandlordMemberId == memberId)
                .Select(p => new PropertyData
                {
                    PropertyID = p.PropertyId.ToString(),
                    PropertyTitle = p.Title,
                    Address = p.AddressLine ?? "",
                    Status = p.StatusCode == "ACTIVE" ? "已上架" : "未上架",
                    ExpiryDate = p.ExpireAt.HasValue ? p.ExpireAt.Value.ToString("yyyy-MM-dd") : "-"
                })
                .OrderByDescending(p => p.PropertyID)
                .ToList();
        }

        private List<ComplaintData> LoadReceivedComplaints(ZuHauseContext context, int memberId)
        {
            return context.PropertyComplaints
                .Include(pc => pc.Property)
                .Where(pc => pc.Property.LandlordMemberId == memberId)
                .Select(pc => new ComplaintData
                {
                    ComplaintId = pc.ComplaintId.ToString(),
                    Subject = "房源投訴",
                    Summary = pc.ComplaintContent.Length > 20 ? pc.ComplaintContent.Substring(0, 20) + "..." : pc.ComplaintContent,
                    TargetProperty = pc.Property.Title,
                    ComplaintDate = pc.CreatedAt,
                    Status = pc.StatusCode == "RESOLVED" ? "已回覆" : "處理中",
                    ComplaintUrl = $"/Admin/ComplaintDetails/{pc.ComplaintId}",
                    PropertyUrl = $"/Property/Details/{pc.PropertyId}"
                })
                .OrderByDescending(c => c.ComplaintDate)
                .ToList();
        }

        private List<OrderData> LoadFurnitureOrders(ZuHauseContext context, int memberId)
        {
            return context.FurnitureOrders
                .Where(fo => fo.MemberId == memberId)
                .Select(fo => new OrderData
                {
                    OrderId = fo.FurnitureOrderId,
                    TotalAmount = fo.TotalAmount,
                    OrderStatus = fo.Status == "PREPARING" ? "準備中" :
                                 fo.Status == "COMPLETED" ? "已完成" :
                                 fo.Status == "CANCELLED" ? "已取消" : "未知",
                    PaymentStatus = fo.PaymentStatus == "PAID" ? "已付款" : "未付款",
                    OrderDate = fo.CreatedAt,
                    OrderUrl = $"/Furniture/OrderDetails/{fo.FurnitureOrderId}"
                })
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        private List<CustomerServiceData> LoadCustomerServiceTickets(ZuHauseContext context, int memberId)
        {
            return context.CustomerServiceTickets
                .Where(cst => cst.MemberId == memberId)
                .Select(cst => new CustomerServiceData
                {
                    TicketId = cst.TicketId.ToString(),
                    Subject = cst.Subject,
                    RelatedItem = cst.ContractId.HasValue ? $"合約 C{cst.ContractId:000}" : "一般諮詢",
                    Status = cst.StatusCode == "OPEN" ? "處理中" : "已關閉",
                    LastReplyTime = cst.ReplyAt.HasValue ? cst.ReplyAt.Value : cst.CreatedAt,
                    TicketUrl = $"/Admin/CustomerServiceDetails/{cst.TicketId}",
                    RelatedItemUrl = cst.ContractId.HasValue ? $"/Contract/Details/{cst.ContractId}" : "#"
                })
                .OrderByDescending(c => c.LastReplyTime)
                .ToList();
        }

        private List<ComplaintData> LoadSubmittedComplaints(ZuHauseContext context, int memberId)
        {
            return context.PropertyComplaints
                .Include(pc => pc.Property)
                .Where(pc => pc.ComplainantId == memberId)
                .Select(pc => new ComplaintData
                {
                    ComplaintId = pc.ComplaintId.ToString(),
                    Subject = "房源投訴",
                    Summary = pc.ComplaintContent.Length > 20 ? pc.ComplaintContent.Substring(0, 20) + "..." : pc.ComplaintContent,
                    TargetProperty = pc.Property.Title,
                    ComplaintDate = pc.CreatedAt,
                    Status = pc.StatusCode == "RESOLVED" ? "已處理" : "處理中",
                    ComplaintUrl = $"/Admin/ComplaintDetails/{pc.ComplaintId}",
                    PropertyUrl = $"/Property/Details/{pc.PropertyId}"
                })
                .OrderByDescending(c => c.ComplaintDate)
                .ToList();
        }
    }

    // 房源管理相關 ViewModels
    public class AdminPropertyListViewModel : BaseListViewModel<PropertyData>
    {
        public AdminPropertyListViewModel()
        {
            PageTitle = "房源管理";
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
    public class AdminCustomerServiceDetailsViewModel : BaseDetailsViewModel<CustomerServiceCase>
    {
        public AdminCustomerServiceDetailsViewModel(string caseId = "CS001")
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
    public class AdminSystemMessageListViewModel : BaseListViewModel<SystemMessageData>
    {
        public AdminSystemMessageListViewModel()
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

    public class AdminSystemMessageNewViewModel : BaseAdminViewModel
    {
        public AdminSystemMessageNewViewModel()
        {
            PageTitle = "新增系統訊息";
            AvailableUsers = new List<UserSelectData>();
        }

        public AdminSystemMessageNewViewModel(ZuHauseContext context)
        {
            PageTitle = "新增系統訊息";
            AvailableUsers = LoadAvailableUsers(context);
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
        
        public List<UserSelectData> AvailableUsers { get; set; }

        private List<UserSelectData> LoadAvailableUsers(ZuHauseContext context)
        {
            return context.Members
                .Where(m => m.IsActive)
                .OrderBy(m => m.MemberName)
                .Select(m => new UserSelectData
                {
                    MemberId = m.MemberId.ToString(),
                    MemberName = m.MemberName,
                    Email = m.Email,
                    NationalNo = m.NationalIdNo ?? ""
                })
                .ToList();
        }
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
        public string LastLoginTime { get; set; } = string.Empty;
        public string ApplyTime { get; set; } = string.Empty;
        public bool HasIdentityUploads { get; set; } = false;
        
        // 進階篩選屬性
        public int Gender { get; set; }
        public int ResidenceCityID { get; set; }
        public int PrimaryRentalCityID { get; set; }
        public string ResidenceCityName { get; set; } = string.Empty;
        public string PrimaryRentalCityName { get; set; } = string.Empty;
        
        // 詳情頁面專用屬性
        public DateTime? BirthDate { get; set; }
        public string DetailedAddress { get; set; } = string.Empty;
        public string PreferredRentalAreas { get; set; } = string.Empty;
        public DateTime? PhoneVerifiedAt { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? IdentityVerifiedAt { get; set; }
        public string GenderDisplay => Gender switch 
        {
            1 => "男",
            2 => "女", 
            _ => "其他"
        };
        
        // 活動相關資料
        public List<RentalContractData> RentalContracts { get; set; } = new();
        public List<PropertyData> OwnedProperties { get; set; } = new();
        public List<ComplaintData> ReceivedComplaints { get; set; } = new();
        public List<OrderData> FurnitureOrders { get; set; } = new();
        public List<CustomerServiceData> CustomerServiceTickets { get; set; } = new();
        public List<ComplaintData> SubmittedComplaints { get; set; } = new();
        
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
        public string Address { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
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

    public class UserSelectData
    {
        public string MemberId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NationalNo { get; set; } = string.Empty;
    }

    // 會員詳情頁面專用資料模型
    public class RentalContractData
    {
        public string ContractId { get; set; } = string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;
        public string LandlordName { get; set; } = string.Empty;
        public string RentPeriod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PropertyUrl { get; set; } = string.Empty;
        public string LandlordUrl { get; set; } = string.Empty;
    }

    public class ComplaintData
    {
        public string ComplaintId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string TargetProperty { get; set; } = string.Empty;
        public DateTime ComplaintDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ComplaintUrl { get; set; } = string.Empty;
        public string PropertyUrl { get; set; } = string.Empty;
    }

    public class OrderData
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string OrderUrl { get; set; } = string.Empty;
    }

    public class CustomerServiceData
    {
        public string TicketId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string RelatedItem { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime LastReplyTime { get; set; }
        public string TicketUrl { get; set; } = string.Empty;
        public string RelatedItemUrl { get; set; } = string.Empty;
    }
}