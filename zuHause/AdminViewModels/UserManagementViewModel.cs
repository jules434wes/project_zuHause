using System;
using System.Collections.Generic;

namespace zuHause.AdminViewModels
{
    /// <summary>
    /// 用戶管理 ViewModel
    /// </summary>
    public class UserManagementViewModel : BaseListViewModel<UserInfo>
    {
        /// <summary>
        /// 篩選條件
        /// </summary>
        public UserFilterCriteria FilterCriteria { get; set; } = new();

        /// <summary>
        /// 分頁配置
        /// </summary>
        public List<TabConfig> TabConfigs { get; set; } = new()
        {
            new TabConfig { TabId = "all", Title = "全部會員", IsDefault = true },
            new TabConfig { TabId = "pending", Title = "等待身分證驗證", BadgeStyle = "bg-warning" },
            new TabConfig { TabId = "landlord", Title = "申請成為房東", BadgeStyle = "bg-warning" }
        };

        /// <summary>
        /// 篩選選項
        /// </summary>
        public UserFilterOptions FilterOptions { get; set; } = new();
    }

    /// <summary>
    /// 用戶資訊
    /// </summary>
    public class UserInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string NationalNo { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public bool IsLandlord { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? ApplicationDate { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string PrimaryRentalCityId { get; set; } = string.Empty;
        public string ResidenceCityId { get; set; } = string.Empty;

        /// <summary>
        /// 取得帳號狀態顯示文字
        /// </summary>
        public string AccountStatusText => AdminConstants.AccountStatusOptions.GetValueOrDefault(AccountStatus, AccountStatus);

        /// <summary>
        /// 取得帳號狀態 Badge 樣式
        /// </summary>
        public string AccountStatusBadgeStyle => AdminConstants.StatusBadgeStyles.GetValueOrDefault(AccountStatus, "bg-secondary");

        /// <summary>
        /// 取得驗證狀態顯示文字
        /// </summary>
        public string VerificationStatusText => AdminConstants.VerificationStatusOptions.GetValueOrDefault(VerificationStatus, VerificationStatus);

        /// <summary>
        /// 取得驗證狀態 Badge 樣式
        /// </summary>
        public string VerificationStatusBadgeStyle => AdminConstants.StatusBadgeStyles.GetValueOrDefault(VerificationStatus, "bg-secondary");

        /// <summary>
        /// 是否房東顯示文字
        /// </summary>
        public string IsLandlordText => IsLandlord ? "是" : "否";

        /// <summary>
        /// 是否房東 Badge 樣式
        /// </summary>
        public string IsLandlordBadgeStyle => IsLandlord ? "bg-info" : "bg-secondary";
    }

    /// <summary>
    /// 用戶篩選條件
    /// </summary>
    public class UserFilterCriteria : BaseFilterCriteria
    {
        /// <summary>
        /// 帳號狀態
        /// </summary>
        public string AccountStatus { get; set; } = string.Empty;

        /// <summary>
        /// 驗證狀態
        /// </summary>
        public string VerificationStatus { get; set; } = string.Empty;

        /// <summary>
        /// 身分證上傳狀態
        /// </summary>
        public string IdUploadStatus { get; set; } = string.Empty;

        /// <summary>
        /// 是否為房東
        /// </summary>
        public bool? IsLandlord { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 主要租賃城市ID
        /// </summary>
        public string PrimaryRentalCityId { get; set; } = string.Empty;

        /// <summary>
        /// 居住城市ID
        /// </summary>
        public string ResidenceCityId { get; set; } = string.Empty;

        /// <summary>
        /// 註冊日期開始
        /// </summary>
        public DateTime? RegisterDateStart { get; set; }

        /// <summary>
        /// 註冊日期結束
        /// </summary>
        public DateTime? RegisterDateEnd { get; set; }

        /// <summary>
        /// 申請日期開始
        /// </summary>
        public DateTime? ApplicationDateStart { get; set; }

        /// <summary>
        /// 申請日期結束
        /// </summary>
        public DateTime? ApplicationDateEnd { get; set; }

        /// <summary>
        /// 最後登入日期開始
        /// </summary>
        public DateTime? LastLoginDateStart { get; set; }

        /// <summary>
        /// 最後登入日期結束
        /// </summary>
        public DateTime? LastLoginDateEnd { get; set; }
    }

    /// <summary>
    /// 用戶篩選選項
    /// </summary>
    public class UserFilterOptions
    {
        /// <summary>
        /// 搜尋欄位選項
        /// </summary>
        public Dictionary<string, string> SearchFields { get; set; } = AdminConstants.UserSearchFields;

        /// <summary>
        /// 帳號狀態選項
        /// </summary>
        public Dictionary<string, string> AccountStatusOptions { get; set; } = AdminConstants.AccountStatusOptions;

        /// <summary>
        /// 驗證狀態選項
        /// </summary>
        public Dictionary<string, string> VerificationStatusOptions { get; set; } = AdminConstants.VerificationStatusOptions;

        /// <summary>
        /// 身分證上傳狀態選項
        /// </summary>
        public Dictionary<string, string> IdUploadStatusOptions { get; set; } = AdminConstants.IdUploadStatusOptions;

        /// <summary>
        /// 城市選項
        /// </summary>
        public Dictionary<string, string> CityOptions { get; set; } = AdminConstants.CityOptions;

        /// <summary>
        /// 性別選項
        /// </summary>
        public Dictionary<string, string> GenderOptions { get; set; } = new()
        {
            { "male", "男性" },
            { "female", "女性" },
            { "other", "其他" }
        };

        /// <summary>
        /// 是否房東選項
        /// </summary>
        public Dictionary<string, string> LandlordOptions { get; set; } = new()
        {
            { "true", "是" },
            { "false", "否" }
        };
    }
}