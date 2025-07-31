using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using zuHause.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using zuHause.Interfaces;

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
            var member = context.Members
                .Include(m => m.PrimaryRentalCity)
                .Include(m => m.PrimaryRentalDistrict)
                .FirstOrDefault(m => m.MemberId == memberId);
            
            if (member?.PrimaryRentalCity != null && member?.PrimaryRentalDistrict != null)
            {
                return $"{member.PrimaryRentalCity.CityName} {member.PrimaryRentalDistrict.DistrictName}";
            }
            else if (member?.PrimaryRentalCity != null)
            {
                return member.PrimaryRentalCity.CityName;
            }
            
            return string.Empty;
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
            var properties = context.Properties
                .Include(p => p.City)
                .Include(p => p.District)
                .Where(p => p.LandlordMemberId == memberId)
                .Select(p => new
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    CityName = p.City != null ? p.City.CityName : "",
                    DistrictName = p.District != null ? p.District.DistrictName : "",
                    AddressLine = p.AddressLine ?? "",
                    StatusCode = p.StatusCode,
                    ExpireAt = p.ExpireAt
                })
                .OrderByDescending(p => p.PropertyId)
                .ToList();

            return properties.Select(p => new PropertyData
            {
                PropertyID = p.PropertyId.ToString(),
                PropertyTitle = p.Title,
                Address = $"{p.CityName}{p.DistrictName}{p.AddressLine}".Trim(),
                Status = GetPropertyStatusDisplayText(p.StatusCode),
                ExpiryDate = p.ExpireAt.HasValue ? p.ExpireAt.Value.ToString("yyyy-MM-dd") : "-"
            }).ToList();
        }

        /// <summary>
        /// 根據房源狀態代碼取得顯示文字
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>狀態顯示文字</returns>
        private string GetPropertyStatusDisplayText(string statusCode)
        {
            return statusCode switch
            {
                "PENDING" => "審核中",
                "PENDING_PAYMENT" => "待付款", 
                "REJECT_REVISE" => "審核未通過(待補件)",
                "REJECTED" => "審核未通過",
                "LISTED" => "上架中",
                "CONTRACT_ISSUED" => "已發出合約",
                "PENDING_RENEWAL" => "待續約",
                "LEASE_EXPIRED_RENEWING" => "續約(房客申請中)",
                "IDLE" => "閒置中",
                "ALREADY_RENTED" => "出租中",
                "INVALID" => "房源已下架",
                "BANNED" => "房源違規遭強制下架，請重新申請上架",
                _ => "未知狀態"
            };
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
                    Status = pc.StatusCode == "RESOLVED" ? "已處理" : "處理中",
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

        public AdminPropertyListViewModel(ZuHauseContext context)
        {
            PageTitle = "房源管理";
            Items = LoadPropertiesFromDatabase(context);
            TotalCount = Items.Count;
            PendingProperties = LoadPendingProperties(context);
            Cities = LoadCitiesFromDatabase(context);
            
            BulkConfig = new BulkActionConfig
            {
                SelectAllId = "selectAllProperties",
                CheckboxClass = "property-checkbox",
                BulkButtonId = "bulkVerifyBtn",
                BulkButtonText = "批量審核"
            };
        }

        public List<PropertyData> PendingProperties { get; set; } = new List<PropertyData>();
        public List<City> Cities { get; set; } = new List<City>();

        private List<PropertyData> LoadPropertiesFromDatabase(ZuHauseContext context)
        {
            var properties = context.Properties
                .Include(p => p.LandlordMember)
                .Include(p => p.City)
                .Include(p => p.District)
                .Include(p => p.Approvals.Where(a => a.ModuleCode == "PROPERTY"))
                .OrderByDescending(p => p.UpdatedAt)
                .Select(p => new
                {
                    PropertyID = p.PropertyId.ToString(),
                    PropertyTitle = p.Title,
                    LandlordName = p.LandlordMember.MemberName,
                    LandlordId = p.LandlordMemberId.ToString(),
                    CityName = p.City != null ? p.City.CityName : "",
                    DistrictName = p.District != null ? p.District.DistrictName : "",
                    Address = p.AddressLine ?? "",
                    RentPrice = (int)p.MonthlyRent,
                    StatusCode = p.StatusCode,
                    IsPaid = p.IsPaid,
                    ExpireAt = p.ExpireAt,
                    SubmissionDate = p.PublishedAt.HasValue ? p.PublishedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedDate = p.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    ExpiryDate = p.ExpireAt.HasValue ? p.ExpireAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "-"
                })
                .ToList()
                .Select(p => new PropertyData
                {
                    PropertyID = p.PropertyID,
                    PropertyTitle = p.PropertyTitle,
                    LandlordName = p.LandlordName,
                    LandlordId = p.LandlordId,
                    Address = $"{p.CityName}{p.DistrictName}{p.Address}".Trim(),
                    RentPrice = p.RentPrice,
                    Status = GetPropertyStatusDisplay(p.StatusCode, p.IsPaid, p.ExpireAt),
                    PaymentStatus = GetPaymentStatusDisplay(p.IsPaid, p.ExpireAt),
                    SubmissionDate = p.SubmissionDate,
                    UpdatedDate = p.UpdatedDate,
                    ExpiryDate = p.ExpiryDate
                })
                .ToList();

            return properties;
        }

        private List<PropertyData> LoadPendingProperties(ZuHauseContext context)
        {
            var pendingProperties = context.Properties
                .Include(p => p.LandlordMember)
                .Include(p => p.City)
                .Include(p => p.District)
                .Include(p => p.Approvals.Where(a => a.ModuleCode == "PROPERTY"))
                .Where(p => p.StatusCode == "PENDING" || p.StatusCode == "REJECTED")
                .OrderBy(p => p.CreatedAt)
                .Select(p => new
                {
                    PropertyID = p.PropertyId.ToString(),
                    PropertyTitle = p.Title,
                    LandlordName = p.LandlordMember.MemberName,
                    LandlordId = p.LandlordMemberId.ToString(),
                    CityName = p.City != null ? p.City.CityName : "",
                    DistrictName = p.District != null ? p.District.DistrictName : "",
                    Address = p.AddressLine ?? "",
                    RentPrice = (int)p.MonthlyRent,
                    IsPaid = p.IsPaid,
                    ExpireAt = p.ExpireAt,
                    SubmissionDate = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedDate = p.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    ExpiryDate = p.ExpireAt.HasValue ? p.ExpireAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "-",
                    HasDocumentUpload = !string.IsNullOrEmpty(p.PropertyProofUrl)
                })
                .ToList()
                .Select(p => new PropertyData
                {
                    PropertyID = p.PropertyID,
                    PropertyTitle = p.PropertyTitle,
                    LandlordName = p.LandlordName,
                    LandlordId = p.LandlordId,
                    Address = $"{p.CityName}{p.DistrictName}{p.Address}".Trim(),
                    RentPrice = p.RentPrice,
                    Status = "審核中",
                    PaymentStatus = GetPaymentStatusDisplay(p.IsPaid, p.ExpireAt),
                    SubmissionDate = p.SubmissionDate,
                    UpdatedDate = p.UpdatedDate,
                    ExpiryDate = p.ExpiryDate,
                    HasDocumentUpload = p.HasDocumentUpload
                })
                .ToList();

            return pendingProperties;
        }

        private List<City> LoadCitiesFromDatabase(ZuHauseContext context)
        {
            return context.Cities
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CityName)
                .ToList();
        }

        private string GetPropertyStatusDisplay(string statusCode, bool isPaid, DateTime? expireAt)
        {
            return statusCode switch
            {
                "PENDING" => "審核中",
                "PENDING_PAYMENT" => "待付款",
                "LISTED" => "上架中",
                "CONTRACT_ISSUED" => "已發出合約",
                "PENDING_RENEWAL" => "待續約",
                "LEASE_EXPIRED_RENEWING" => "續約申請中",
                "IDLE" => "閒置中",
                "ALREADY_RENTED" => "出租中",
                "REJECT_REVISE" => "審核未通過(待補件)",
                "REJECTED" => "審核未通過",
                "INVALID" => "房源已下架",
                "BANNED" => "違規下架",
                "ACTIVE" => "審核完成(舊狀態)", // 向後相容，但不應該出現在新系統中
                _ => "未知狀態"
            };
        }

        private string GetPaymentStatusDisplay(bool isPaid, DateTime? expireAt)
        {
            if (!isPaid)
                return "未付費";
            
            if (!expireAt.HasValue)
                return "已付費";
                
            return expireAt.Value > DateTime.Now ? "已付費" : "已過期";
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

    // 房源詳情相關 ViewModels
    public class AdminPropertyDetailsViewModel : BaseAdminViewModel
    {
        public AdminPropertyDetailsViewModel()
        {
            PageTitle = "房源詳情";
        }

        public AdminPropertyDetailsViewModel(ZuHauseContext context, IImageQueryService imageQueryService, int propertyId)
        {
            PageTitle = "房源詳情";
            Data = LoadPropertyDetailsFromDatabase(context, propertyId);
            RentalHistory = LoadRentalHistory(context, propertyId);
            Complaints = LoadComplaints(context, propertyId);
            Equipment = LoadEquipment(context, propertyId);
            Images = LoadImages(context, imageQueryService, propertyId);
            ApprovalHistory = LoadApprovalHistory(context, propertyId);
        }

        public PropertyDetailsData? Data { get; set; }
        public List<RentalHistoryData> RentalHistory { get; set; } = new();
        public List<ComplaintDetailsData> Complaints { get; set; } = new();
        public List<EquipmentData> Equipment { get; set; } = new();
        public List<PropertyImageData> Images { get; set; } = new();
        public List<ApprovalHistoryData> ApprovalHistory { get; set; } = new();

        private PropertyDetailsData? LoadPropertyDetailsFromDatabase(ZuHauseContext context, int propertyId)
        {
            var property = context.Properties
                .Include(p => p.LandlordMember)
                .Include(p => p.City)
                .Include(p => p.District)
                .Include(p => p.ListingPlan)
                .Include(p => p.Approvals.Where(a => a.ModuleCode == "PROPERTY"))
                .FirstOrDefault(p => p.PropertyId == propertyId);

            if (property == null)
                return null;

            var approval = property.Approvals.FirstOrDefault();

            return new PropertyDetailsData
            {
                PropertyId = property.PropertyId.ToString(),
                Title = property.Title,
                LandlordName = property.LandlordMember.MemberName,
                LandlordId = property.LandlordMemberId.ToString(),
                Address = $"{property.City?.CityName ?? ""}{property.District?.DistrictName ?? ""}{property.AddressLine ?? ""}".Trim(),
                MonthlyRent = property.MonthlyRent,
                DepositAmount = property.DepositAmount,
                DepositMonths = property.DepositMonths,
                RoomCount = property.RoomCount,
                LivingRoomCount = property.LivingRoomCount,
                BathroomCount = property.BathroomCount,
                CurrentFloor = property.CurrentFloor,
                TotalFloors = property.TotalFloors,
                Area = property.Area,
                MinimumRentalMonths = property.MinimumRentalMonths,
                Description = property.Description ?? "",
                SpecialRules = property.SpecialRules ?? "",
                WaterFeeType = property.WaterFeeType,
                CustomWaterFee = property.CustomWaterFee,
                ElectricityFeeType = property.ElectricityFeeType,
                CustomElectricityFee = property.CustomElectricityFee,
                ManagementFeeIncluded = property.ManagementFeeIncluded,
                ManagementFeeAmount = property.ManagementFeeAmount,
                ParkingAvailable = property.ParkingAvailable,
                ParkingFeeRequired = property.ParkingFeeRequired,
                ParkingFeeAmount = property.ParkingFeeAmount,
                CleaningFeeRequired = property.CleaningFeeRequired,
                CleaningFeeAmount = property.CleaningFeeAmount,
                PropertyProofUrl = property.PropertyProofUrl,
                PreviewImageUrl = property.PreviewImageUrl,
                ApprovalStatus = GetApprovalStatusDisplay(property.StatusCode),
                StatusCode = property.StatusCode, // 添加原始狀態碼
                PaymentStatus = GetPaymentStatusDisplay(property.IsPaid, property.ExpireAt),
                PublishedAt = property.PublishedAt,
                ExpireAt = property.ExpireAt,
                UpdatedAt = property.UpdatedAt,
                CreatedAt = property.CreatedAt,
                ListingPlan = property.ListingPlan != null ? new ListingPlanData
                {
                    PlanName = property.ListingPlan.PlanName,
                    PricePerDay = property.ListingPlan.PricePerDay,
                    MinListingDays = property.ListingPlan.MinListingDays,
                    CalculatedFee = property.ListingDays.HasValue 
                        ? property.ListingPlan.PricePerDay * property.ListingDays.Value 
                        : property.ListingFeeAmount ?? 0,
                    ListingDays = property.ListingDays ?? property.ListingPlan.MinListingDays
                } : null
            };
        }

        private List<RentalHistoryData> LoadRentalHistory(ZuHauseContext context, int propertyId)
        {
            return context.RentalApplications
                .Include(ra => ra.Member)
                .Include(ra => ra.Contracts)
                .Where(ra => ra.PropertyId == propertyId && ra.Contracts.Any())
                .SelectMany(ra => ra.Contracts.Select(c => new RentalHistoryData
                {
                    ContractId = c.ContractId,
                    TenantName = ra.Member.MemberName,
                    TenantId = ra.MemberId,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    MonthlyRent = ra.Property.MonthlyRent,
                    ApplicationDate = ra.CreatedAt
                }))
                .OrderByDescending(rh => rh.StartDate)
                .ToList();
        }

        private List<ComplaintDetailsData> LoadComplaints(ZuHauseContext context, int propertyId)
        {
            return context.PropertyComplaints
                .Include(pc => pc.Complainant)
                .Where(pc => pc.PropertyId == propertyId)
                .Select(pc => new ComplaintDetailsData
                {
                    ComplaintId = pc.ComplaintId,
                    ComplainantName = pc.Complainant.MemberName,
                    ComplainantId = pc.ComplainantId,
                    ComplaintContent = pc.ComplaintContent,
                    Status = pc.StatusCode,
                    CreatedAt = pc.CreatedAt,
                    ResolvedAt = pc.ResolvedAt,
                    InternalNote = pc.InternalNote
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        private List<EquipmentData> LoadEquipment(ZuHauseContext context, int propertyId)
        {
            return context.PropertyEquipmentRelations
                .Include(per => per.Category)
                .Where(per => per.PropertyId == propertyId)
                .Select(per => new EquipmentData
                {
                    CategoryName = per.Category.CategoryName,
                    Quantity = per.Quantity,
                    CreatedAt = per.CreatedAt
                })
                .OrderBy(e => e.CategoryName)
                .ToList();
        }

        private List<PropertyImageData> LoadImages(ZuHauseContext context, IImageQueryService imageQueryService, int propertyId)
        {
            // 先從資料庫取得原始圖片資料
            var images = context.Images
                .Where(img => img.EntityType == zuHause.Enums.EntityType.Property && 
                             img.EntityId == propertyId && 
                             img.IsActive)
                .OrderBy(i => i.DisplayOrder ?? int.MaxValue)
                .ThenBy(i => i.UploadedAt)
                .ToList();

            // 使用 ImageQueryService 生成正確的圖片 URL
            return images.Select(img => new PropertyImageData
            {
                ImageId = (int)img.ImageId,
                ImageUrl = imageQueryService.GenerateImageUrl(img.StoredFileName, zuHause.Enums.ImageSize.Original),
                ImageSize = $"{img.Width}x{img.Height}",
                ImageCategory = GetImageCategoryDisplay(img.Category),
                DisplayOrder = img.DisplayOrder ?? 0,
                UploadedAt = img.UploadedAt
            }).ToList();
        }

        private List<ApprovalHistoryData> LoadApprovalHistory(ZuHauseContext context, int propertyId)
        {
            var approvalHistory = new List<ApprovalHistoryData>();

            // 取得房源相關的審核記錄
            var approvals = context.Approvals
                .Include(a => a.ApprovalItems)
                .Where(a => a.SourcePropertyId == propertyId && a.ModuleCode == "PROPERTY")
                .ToList();

            foreach (var approval in approvals)
            {
                foreach (var item in approval.ApprovalItems.OrderBy(ai => ai.CreatedAt))
                {
                    var adminName = item.ActionBy.HasValue 
                        ? context.Admins.FirstOrDefault(admin => admin.AdminId == item.ActionBy.Value)?.Name ?? "系統自動"
                        : "系統自動";

                    approvalHistory.Add(new ApprovalHistoryData
                    {
                        OperationTime = item.CreatedAt,
                        ActionType = item.ActionType,
                        ActionNote = item.ActionNote ?? "",
                        AdminName = adminName,
                        AdminId = item.ActionBy
                    });
                }
            }

            return approvalHistory.OrderByDescending(ah => ah.OperationTime).ToList();
        }

        private static string GetImageCategoryDisplay(zuHause.Enums.ImageCategory category)
        {
            switch (category)
            {
                case zuHause.Enums.ImageCategory.BedRoom:
                    return "臥室";
                case zuHause.Enums.ImageCategory.Living:
                    return "客廳";
                case zuHause.Enums.ImageCategory.Kitchen:
                    return "廚房";
                case zuHause.Enums.ImageCategory.Balcony:
                    return "陽台";
                case zuHause.Enums.ImageCategory.Gallery:
                    return "圖片集";
                default:
                    return "其他";
            }
        }

        private string GetApprovalStatusDisplay(string statusCode)
        {
            switch (statusCode)
            {
                case "PENDING":
                    return "審核中";
                case "PENDING_PAYMENT":
                    return "待付款";
                case "LISTED":
                    return "上架中";
                case "CONTRACT_ISSUED":
                    return "已發出合約";
                case "PENDING_RENEWAL":
                    return "待續約";
                case "LEASE_EXPIRED_RENEWING":
                    return "續約申請中";
                case "IDLE":
                    return "閒置中";
                case "ALREADY_RENTED":
                    return "出租中";
                case "REJECT_REVISE":
                    return "審核未通過(待補件)";
                case "REJECTED":
                    return "審核未通過";
                case "INVALID":
                    return "房源已下架";
                case "BANNED":
                    return "違規下架";
                case "ACTIVE": // 向後相容，但不應該出現在新系統中
                    return "審核完成(舊狀態)";
                default:
                    return "未知狀態";
            }
        }

        private string GetPaymentStatusDisplay(bool isPaid, DateTime? expireAt)
        {
            if (!isPaid)
                return "未付費";
            
            if (!expireAt.HasValue)
                return "已付費";
                
            return expireAt.Value > DateTime.Now ? "已付費" : "已過期";
        }
    }

    

    // 系統訊息相關 ViewModels
    public class AdminSystemMessageListViewModel : BaseListViewModel<SystemMessageData>
    {
        public AdminSystemMessageListViewModel()
        {
            PageTitle = "系統訊息管理";
            Items = new List<SystemMessageData>();
            TotalCount = 0;
            HasBulkActions = false;
        }

        public AdminSystemMessageListViewModel(ZuHauseContext context)
        {
            PageTitle = "系統訊息管理";
            Items = LoadSystemMessagesFromDatabase(context);
            TotalCount = Items.Count;
            HasBulkActions = false;
        }

        private List<SystemMessageData> LoadSystemMessagesFromDatabase(ZuHauseContext context)
        {
            return context.SystemMessages
                .Include(sm => sm.Admin)
                .Include(sm => sm.Receiver)
                .Where(sm => sm.IsActive)
                .OrderByDescending(sm => sm.CreatedAt)
                .Select(sm => new SystemMessageData
                {
                    MessageID = sm.MessageId.ToString(),
                    MessageTitle = sm.Title,
                    MessageContent = sm.MessageContent,
                    Category = sm.CategoryCode,
                    CategoryDisplay = GetCategoryDisplay(sm.CategoryCode),
                    AudienceType = sm.AudienceTypeCode,
                    AudienceDisplay = GetAudienceDisplay(sm.AudienceTypeCode),
                    // 根據發送對象類型顯示不同的接收者資訊
                    ReceiverName = sm.AudienceTypeCode == "INDIVIDUAL" && sm.Receiver != null 
                        ? sm.Receiver.MemberName 
                        : GetAudienceDisplay(sm.AudienceTypeCode),
                    ReceiverId = sm.ReceiverId.HasValue ? sm.ReceiverId.Value.ToString() : "",
                    AdminName = sm.Admin.Name,
                    SentAt = sm.SentAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    CreatedAt = sm.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                })
                .ToList();
        }

        private static string GetCategoryDisplay(string categoryCode)
        {
            return categoryCode switch
            {
                "ANNOUNCEMENT" => "一般公告",
                "UPDATE" => "功能更新",
                "SECURITY" => "安全提醒",
                "MAINTENANCE" => "系統維護",
                "POLICY" => "政策更新",
                "ORDER" => "訂單通知",
                "PROPERTY" => "房源通知",
                "ACCOUNT" => "帳戶通知",
                _ => "其他"
            };
        }

        private static string GetAudienceDisplay(string audienceTypeCode)
        {
            return audienceTypeCode switch
            {
                "INDIVIDUAL" => "個別用戶",
                "ALL_MEMBERS" => "全體會員",
                "ALL_LANDLORDS" => "全體房東",
                _ => "未知"
            };
        }
    }

    public class AdminSystemMessageNewViewModel : BaseAdminViewModel
    {
        public AdminSystemMessageNewViewModel()
        {
            PageTitle = "新增系統訊息";
            AvailableUsers = new List<UserSelectData>();
            AvailableTemplates = new List<SystemMessageTemplateData>();
            MessageCategories = new List<CategorySelectData>();
        }

        public AdminSystemMessageNewViewModel(ZuHauseContext context)
        {
            PageTitle = "新增系統訊息";
            AvailableUsers = LoadAvailableUsers(context);
            AvailableTemplates = LoadAvailableTemplates(context);
            MessageCategories = LoadMessageCategories();
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
        public string? AttachmentUrl { get; set; }
        
        public List<UserSelectData> AvailableUsers { get; set; }
        public List<SystemMessageTemplateData> AvailableTemplates { get; set; }
        public List<CategorySelectData> MessageCategories { get; set; }

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

        private List<SystemMessageTemplateData> LoadAvailableTemplates(ZuHauseContext context)
        {
            return context.AdminMessageTemplates
                .Where(t => t.CategoryCode == "SYSTEM_MESSAGE" && t.IsActive)
                .OrderBy(t => t.Title)
                .Select(t => new SystemMessageTemplateData
                {
                    TemplateID = t.TemplateId,
                    Title = t.Title,
                    TemplateContent = t.TemplateContent,
                    ContentPreview = t.TemplateContent.Length > 50 ? t.TemplateContent.Substring(0, 50) + "..." : t.TemplateContent,
                    CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd")
                })
                .ToList();
        }

        private List<CategorySelectData> LoadMessageCategories()
        {
            return new List<CategorySelectData>
            {
                new CategorySelectData { Value = "ANNOUNCEMENT", Text = "一般公告" },
                new CategorySelectData { Value = "UPDATE", Text = "功能更新" },
                new CategorySelectData { Value = "SECURITY", Text = "安全提醒" },
                new CategorySelectData { Value = "MAINTENANCE", Text = "系統維護" },
                new CategorySelectData { Value = "POLICY", Text = "政策更新" },
                new CategorySelectData { Value = "ORDER", Text = "訂單通知" },
                new CategorySelectData { Value = "PROPERTY", Text = "房源通知" },
                new CategorySelectData { Value = "ACCOUNT", Text = "帳戶通知" }
            };
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
        public bool HasPendingIdentityApplication => VerificationStatus == "pending";
    }

    public class PropertyData
    {
        public string PropertyID { get; set; } = string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;
        public string LandlordName { get; set; } = string.Empty;
        public string LandlordId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public int RentPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string SubmissionDate { get; set; } = string.Empty;
        public string UpdatedDate { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public bool HasDocumentUpload { get; set; } = false;
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
        public string MessageContent { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryDisplay { get; set; } = string.Empty;
        public string AudienceType { get; set; } = string.Empty;
        public string AudienceDisplay { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public string SentAt { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        // 格式化屬性
        public string ContentPreview => MessageContent.Length > 50 ? MessageContent.Substring(0, 50) + "..." : MessageContent;
    }

    public class SystemMessageTemplateData
    {
        public int TemplateID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TemplateContent { get; set; } = string.Empty;
        public string ContentPreview { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class CategorySelectData
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
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

    // 房源詳情專用資料模型
    public class PropertyDetailsData
    {
        public string PropertyId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string LandlordName { get; set; } = string.Empty;
        public string LandlordId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal MonthlyRent { get; set; }
        public decimal DepositAmount { get; set; }
        public int DepositMonths { get; set; }
        public int RoomCount { get; set; }
        public int LivingRoomCount { get; set; }
        public int BathroomCount { get; set; }
        public int CurrentFloor { get; set; }
        public int TotalFloors { get; set; }
        public decimal Area { get; set; }
        public int MinimumRentalMonths { get; set; }
        public string Description { get; set; } = string.Empty;
        public string SpecialRules { get; set; } = string.Empty;
        public string WaterFeeType { get; set; } = string.Empty;
        public decimal? CustomWaterFee { get; set; }
        public string ElectricityFeeType { get; set; } = string.Empty;
        public decimal? CustomElectricityFee { get; set; }
        public bool ManagementFeeIncluded { get; set; }
        public decimal? ManagementFeeAmount { get; set; }
        public bool ParkingAvailable { get; set; }
        public bool ParkingFeeRequired { get; set; }
        public decimal? ParkingFeeAmount { get; set; }
        public bool CleaningFeeRequired { get; set; }
        public decimal? CleaningFeeAmount { get; set; }
        public string? PropertyProofUrl { get; set; }
        public string? PreviewImageUrl { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty; // 原始狀態碼
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpireAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public ListingPlanData? ListingPlan { get; set; }

        // 格式化屬性
        public string Layout => $"{RoomCount}房{LivingRoomCount}廳{BathroomCount}衛";
        public string FloorDisplay => $"{CurrentFloor}樓";
        public string TotalFloorsDisplay => $"{TotalFloors}樓";
        public bool IsActive => ApprovalStatus == "上架中" || ApprovalStatus == "已發出合約" || ApprovalStatus == "出租中";
        public bool HasPropertyProof => !string.IsNullOrEmpty(PropertyProofUrl);
    }

    public class ListingPlanData
    {
        public string PlanName { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public int MinListingDays { get; set; }
        public decimal CalculatedFee { get; set; }
        public int ListingDays { get; set; }
    }

    public class RentalHistoryData
    {
        public int ContractId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? MonthlyRent { get; set; }
        public DateTime ApplicationDate { get; set; }

        public string StatusDisplay => Status switch
        {
            "ACTIVE" => "生效中",
            "COMPLETED" => "已結束",
            "TERMINATED" => "提前終止",
            _ => "未知"
        };

        public string RentalPeriod => EndDate.HasValue 
            ? $"{StartDate:yyyy-MM-dd} ~ {EndDate.Value:yyyy-MM-dd}"
            : $"{StartDate:yyyy-MM-dd} ~ 進行中";
    }

    public class ComplaintDetailsData
    {
        public int ComplaintId { get; set; }
        public string ComplainantName { get; set; } = string.Empty;
        public int ComplainantId { get; set; }
        public string ComplaintContent { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? InternalNote { get; set; }

        public string StatusDisplay => Status switch
        {
            "PENDING" => "待處理",
            "PROGRESS" => "處理中",
            "RESOLVED" => "已處理",
            _ => "未知"
        };

        public string Summary => ComplaintContent.Length > 50 
            ? ComplaintContent.Substring(0, 50) + "..."
            : ComplaintContent;
    }

    public class EquipmentData
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PropertyImageData
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageSize { get; set; } = string.Empty;
        public string ImageCategory { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public DateTime UploadedAt { get; set; }

        public string SizeDisplay => ImageSize switch
        {
            "THUMBNAIL" => "縮圖",
            "SMALL" => "小圖",
            "MEDIUM" => "中圖", 
            "LARGE" => "大圖",
            "ORIGINAL" => "原圖",
            _ => "未知"
        };

        public string CategoryDisplay => ImageCategory switch
        {
            "ROOM" => "房間",
            "BATHROOM" => "浴室",
            "KITCHEN" => "廚房",
            "LIVING_ROOM" => "客廳",
            "BALCONY" => "陽台",
            "EXTERIOR" => "外觀",
            "OTHER" => "其他",
            _ => "未分類"
        };
    }

    public class ApprovalHistoryData
    {
        public DateTime OperationTime { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string ActionNote { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public int? AdminId { get; set; }

        public string ActionTypeDisplay => ActionType switch
        {
            "SUBMIT" => "提交申請",
            "APPROVE" => "審核通過",
            "REJECT" => "審核駁回",
            "REVISE" => "要求修正",
            "MARK_PAID" => "標記已付款",
            "PUBLISH" => "發布上架",
            "SUSPEND" => "暫停",
            "REACTIVATE" => "重新啟用",
            _ => ActionType
        };

        public string BadgeClass => ActionType switch
        {
            "SUBMIT" => "bg-info",
            "APPROVE" => "bg-success", 
            "REJECT" => "bg-danger",
            "REVISE" => "bg-warning",
            "MARK_PAID" => "bg-primary",
            "PUBLISH" => "bg-success",
            "SUSPEND" => "bg-secondary",
            "REACTIVATE" => "bg-info",
            _ => "bg-secondary"
        };
    }

    // 房源投訴管理相關 ViewModels
    public class AdminPropertyComplaintsViewModel : BaseListViewModel<PropertyComplaintData>
    {
        public AdminPropertyComplaintsViewModel()
        {
            PageTitle = "房源投訴管理";
            Items = new List<PropertyComplaintData>();
            TotalCount = 0;
            HasBulkActions = false;
        }

        public AdminPropertyComplaintsViewModel(ZuHauseContext context)
        {
            PageTitle = "房源投訴管理";
            Items = LoadComplaintsFromDatabase(context);
            TotalCount = Items.Count;
            HasBulkActions = false;
        }

        private List<PropertyComplaintData> LoadComplaintsFromDatabase(ZuHauseContext context)
        {
            // 正確的查詢方式：在Select中直接導航，不需要Include
            var complaints = context.PropertyComplaints
                .OrderByDescending(pc => pc.CreatedAt)
                .Select(pc => new PropertyComplaintData
                {
                    ComplaintId = pc.ComplaintId,
                    ComplainantName = pc.Complainant != null ? pc.Complainant.MemberName : "未知用戶",
                    ComplainantId = pc.ComplainantId,
                    PropertyTitle = pc.Property != null ? pc.Property.Title : "未知房源",
                    PropertyId = pc.PropertyId,
                    LandlordName = pc.Landlord != null ? pc.Landlord.MemberName : "未知房東",
                    LandlordId = pc.LandlordId,
                    ComplaintContent = pc.ComplaintContent ?? "",
                    Status = pc.StatusCode ?? "UNKNOWN",
                    CreatedAt = pc.CreatedAt,
                    UpdatedAt = pc.UpdatedAt,
                    ResolvedAt = pc.ResolvedAt,
                    InternalNote = pc.InternalNote,
                    HandledBy = pc.HandledBy
                })
                .ToList();

            return complaints;
        }
    }

    // 房源投訴資料模型
    public class PropertyComplaintData
    {
        public int ComplaintId { get; set; }
        public string ComplainantName { get; set; } = string.Empty;
        public int ComplainantId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public string LandlordName { get; set; } = string.Empty;
        public int LandlordId { get; set; }
        public string ComplaintContent { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? InternalNote { get; set; }
        public int? HandledBy { get; set; }

        // 格式化屬性
        public string ComplaintIdDisplay => $"CMPL-{ComplaintId:0000}";
        public string StatusDisplay => Status switch
        {
            "PENDING" => "待處理",
            "PROGRESS" => "處理中",
            "RESOLVED" => "已處理",
            _ => "未知"
        };
        public string Summary => ComplaintContent.Length > 50 
            ? ComplaintContent.Substring(0, 50) + "..."
            : ComplaintContent;
        public string ComplaintUrl => $"/Admin/admin_propertyComplaints/{ComplaintId}";
        public string PropertyUrl => $"/Admin/admin_propertyDetails/{PropertyId}";
        public string ComplainantUrl => $"/Admin/admin_userDetails/{ComplainantId}";
        public string LandlordUrl => $"/Admin/admin_userDetails/{LandlordId}";
    }
}