using System.ComponentModel.DataAnnotations;
using zuHause.Helpers;

namespace zuHause.DTOs
{
    /// <summary>
    /// 房源管理資料傳輸物件
    /// 支援房源管理頁面的完整功能需求
    /// </summary>
    public class PropertyManagementDto
    {
        // === 基本房源資訊 ===
        
        /// <summary>
        /// 房源ID
        /// </summary>
        public int PropertyId { get; set; }

        /// <summary>
        /// 房源標題
        /// </summary>
        [Required(ErrorMessage = "房源標題為必填項目")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "房源標題長度須介於5-100字元")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 月租金
        /// </summary>
        [Required(ErrorMessage = "月租金為必填項目")]
        [Range(1, 500000, ErrorMessage = "月租金須介於1-500,000元")]
        public decimal MonthlyRent { get; set; }

        /// <summary>
        /// 完整地址
        /// </summary>
        [Required(ErrorMessage = "房源地址為必填項目")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "地址長度須介於10-200字元")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 房源狀態代碼（英文）
        /// </summary>
        public string StatusCode { get; set; } = string.Empty;

        // === 房源詳細資訊 ===

        /// <summary>
        /// 房數
        /// </summary>
        [Range(1, 50, ErrorMessage = "房數須介於1-50之間")]
        public int RoomCount { get; set; }

        /// <summary>
        /// 廳數
        /// </summary>
        [Range(0, 20, ErrorMessage = "廳數須介於0-20之間")]
        public int LivingRoomCount { get; set; }

        /// <summary>
        /// 衛數
        /// </summary>
        [Range(1, 20, ErrorMessage = "衛數須介於1-20之間")]
        public int BathroomCount { get; set; }

        /// <summary>
        /// 坪數
        /// </summary>
        [Range(1, 1000, ErrorMessage = "坪數須介於1-1000坪")]
        public decimal? Area { get; set; }

        /// <summary>
        /// 所在樓層
        /// </summary>
        [Range(1, 100, ErrorMessage = "所在樓層須介於1-100樓")]
        public int CurrentFloor { get; set; }

        /// <summary>
        /// 總樓層數
        /// </summary>
        [Range(1, 100, ErrorMessage = "總樓層數須介於1-100樓")]
        public int TotalFloors { get; set; }

        // === 圖片與媒體 ===

        /// <summary>
        /// 預覽圖片URL
        /// </summary>
        public string ThumbnailUrl { get; set; } = string.Empty;

        /// <summary>
        /// 房源圖片URL清單
        /// </summary>
        public List<string> ImageUrls { get; set; } = new();

        // === 時間資訊 ===

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 上架時間
        /// </summary>
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// 到期時間
        /// </summary>
        public DateTime? ExpireAt { get; set; }

        /// <summary>
        /// 付款狀態
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// 是否有停車位
        /// </summary>
        public bool ParkingAvailable { get; set; }

        /// <summary>
        /// 管理費是否包含在租金中
        /// </summary>
        public bool ManagementFeeIncluded { get; set; }

        /// <summary>
        /// 管理費金額
        /// </summary>
        public decimal? ManagementFeeAmount { get; set; }

        /// <summary>
        /// 押金金額
        /// </summary>
        public decimal DepositAmount { get; set; }

        /// <summary>
        /// 押金月數
        /// </summary>
        public int DepositMonths { get; set; }

        /// <summary>
        /// 最短租期(月)
        /// </summary>
        public int MinimumRentalMonths { get; set; }

        // === 統計資訊 ===

        /// <summary>
        /// 瀏覽次數
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// 收藏次數
        /// </summary>
        public int FavoriteCount { get; set; }

        /// <summary>
        /// 申請次數
        /// </summary>
        public int ApplicationCount { get; set; }

        // === 計算屬性 - 整合 PropertyStatusHelper ===

        /// <summary>
        /// 正體中文狀態顯示
        /// </summary>
        public string StatusDisplayName => PropertyStatusHelper.GetDisplayName(StatusCode);

        /// <summary>
        /// Bootstrap 樣式類別
        /// </summary>
        public string StatusStyle => PropertyStatusHelper.GetStatusStyle(StatusCode);

        /// <summary>
        /// 是否可編輯
        /// </summary>
        public bool IsEditable => PropertyStatusHelper.IsEditable(StatusCode);

        /// <summary>
        /// 是否可下架
        /// </summary>
        public bool IsTakeDownable => PropertyStatusHelper.IsTakeDownable(StatusCode);

        /// <summary>
        /// 是否需要用戶行動
        /// </summary>
        public bool RequiresAction => PropertyStatusHelper.RequiresAction(StatusCode);

        /// <summary>
        /// 是否需要補充資料
        /// </summary>
        public bool RequiresDataSupplement => PropertyStatusHelper.RequiresDataSupplement(StatusCode);

        /// <summary>
        /// 是否需要聯絡客服
        /// </summary>
        public bool RequiresCustomerService => PropertyStatusHelper.RequiresCustomerService(StatusCode);

        /// <summary>
        /// 房型顯示（如：2房1廳1衛）
        /// </summary>
        public string LayoutDisplay => $"{RoomCount}房{LivingRoomCount}廳{BathroomCount}衛";

        /// <summary>
        /// 樓層顯示（如：3F/5F）
        /// </summary>
        public string FloorInfo => $"{CurrentFloor}F/{TotalFloors}F";

        /// <summary>
        /// 租金顯示格式
        /// </summary>
        public string PriceDisplay => $"NT$ {MonthlyRent:N0}";

        /// <summary>
        /// 坪數顯示
        /// </summary>
        public string AreaDisplay => Area.HasValue ? $"{Area:N1} 坪" : "未提供";

        /// <summary>
        /// 建立時間顯示
        /// </summary>
        public string CreatedAtDisplay => CreatedAt.ToString("yyyy/MM/dd");

        /// <summary>
        /// 更新時間顯示
        /// </summary>
        public string UpdatedAtDisplay => UpdatedAt.ToString("yyyy/MM/dd HH:mm");

        /// <summary>
        /// 到期時間顯示
        /// </summary>
        public string ExpireAtDisplay => ExpireAt?.ToString("yyyy/MM/dd") ?? "";

        /// <summary>
        /// 三分組分類
        /// </summary>
        public PropertyStatusGroup StatusGroup => PropertyStatusHelper.GetStatusGroup(StatusCode);

        /// <summary>
        /// 檢視房源模式
        /// </summary>
        public PropertyViewMode ViewMode => PropertyStatusHelper.GetViewMode(StatusCode);

        /// <summary>
        /// 是否為過時狀態（如DRAFT） - 當前版本中移除DRAFT狀態，保留此屬性用於向後相容
        /// </summary>
        public bool IsLegacyStatus => false; // 移除DRAFT狀態後，目前無過時狀態

        /// <summary>
        /// 停車位顯示
        /// </summary>
        public string ParkingDisplay => ParkingAvailable ? "有停車位" : "無停車位";

        /// <summary>
        /// 押金顯示
        /// </summary>
        public string DepositDisplay => $"{DepositMonths}個月押金 (NT$ {DepositAmount:N0})";

        /// <summary>
        /// 管理費顯示
        /// </summary>
        public string ManagementFeeDisplay => ManagementFeeIncluded 
            ? "管理費含在租金內" 
            : ManagementFeeAmount.HasValue 
                ? $"管理費 NT$ {ManagementFeeAmount:N0}" 
                : "無管理費";

        /// <summary>
        /// 最短租期顯示
        /// </summary>
        public string MinimumRentalDisplay => $"最短 {MinimumRentalMonths} 個月";
    }

    /// <summary>
    /// 房源管理列表回應 DTO
    /// </summary>
    public class PropertyManagementListResponseDto
    {
        /// <summary>
        /// 房源清單
        /// </summary>
        public List<PropertyManagementDto> Properties { get; set; } = new();

        /// <summary>
        /// 總房源數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 統計資料
        /// </summary>
        public PropertyManagementStatsDto Stats { get; set; } = new();

        /// <summary>
        /// 狀態摘要統計
        /// </summary>
        public Dictionary<string, int> StatusSummary { get; set; } = new();

        // === 三分組數據 ===

        /// <summary>
        /// 可用房源清單
        /// </summary>
        public List<PropertyManagementDto> AvailableProperties => 
            Properties.Where(p => p.StatusGroup == PropertyStatusGroup.Available).ToList();

        /// <summary>
        /// 等待刊登的房源清單
        /// </summary>
        public List<PropertyManagementDto> PendingProperties => 
            Properties.Where(p => p.StatusGroup == PropertyStatusGroup.Pending).ToList();

        /// <summary>
        /// 目前不可用的房源清單
        /// </summary>
        public List<PropertyManagementDto> UnavailableProperties => 
            Properties.Where(p => p.StatusGroup == PropertyStatusGroup.Unavailable).ToList();

        /// <summary>
        /// 可用房源數量
        /// </summary>
        public int AvailableCount => AvailableProperties.Count;

        /// <summary>
        /// 等待刊登房源數量
        /// </summary>
        public int PendingCount => PendingProperties.Count;

        /// <summary>
        /// 不可用房源數量
        /// </summary>
        public int UnavailableCount => UnavailableProperties.Count;

        /// <summary>
        /// 需要行動的房源數量
        /// </summary>
        public int ActionRequiredCount => Properties.Count(p => p.RequiresAction);
    }

    /// <summary>
    /// 房源管理統計 DTO（簡化版）
    /// </summary>
    public class PropertyManagementStatsDto
    {
        /// <summary>
        /// 總房源數
        /// </summary>
        public int TotalProperties { get; set; }
    }

    /// <summary>
    /// ViewComponent 參數 DTO（支援重用性）
    /// </summary>
    public class PropertyCardDisplayDto
    {
        /// <summary>
        /// 房源資料
        /// </summary>
        public PropertyManagementDto Property { get; set; } = new();

        /// <summary>
        /// 顯示模式
        /// </summary>
        public PropertyCardDisplayMode DisplayMode { get; set; } = PropertyCardDisplayMode.Management;

        /// <summary>
        /// 是否顯示操作按鈕
        /// </summary>
        public bool ShowActions { get; set; } = true;

        /// <summary>
        /// 是否顯示統計資訊
        /// </summary>
        public bool ShowStats { get; set; } = true;

        /// <summary>
        /// 是否顯示狀態標籤
        /// </summary>
        public bool ShowStatusBadge { get; set; } = true;

        /// <summary>
        /// 自訂CSS類別
        /// </summary>
        public string CustomCssClass { get; set; } = string.Empty;
    }

    /// <summary>
    /// 房源卡片顯示模式枚舉
    /// </summary>
    public enum PropertyCardDisplayMode
    {
        /// <summary>
        /// 房源管理頁面（完整操作功能）
        /// </summary>
        Management,

        /// <summary>
        /// 房東個人資料頁面（簡化顯示）
        /// </summary>
        Profile,

        /// <summary>
        /// 統計頁面（數據重點顯示）
        /// </summary>
        Stats,

        /// <summary>
        /// 縮略模式（最小化顯示）
        /// </summary>
        Compact
    }
}