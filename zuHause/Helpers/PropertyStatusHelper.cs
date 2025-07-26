using zuHause.Constants;

namespace zuHause.Helpers
{
    /// <summary>
    /// 房源狀態管理輔助類
    /// 提供房源狀態的中文顯示、樣式對應和業務邏輯判斷
    /// 支援 12 個房源狀態，移除 DRAFT，新增 BANNED
    /// </summary>
    public static class PropertyStatusHelper
    {
        /// <summary>
        /// 房源狀態代碼對應中文顯示名稱
        /// </summary>
        private static readonly Dictionary<string, string> StatusDisplayNames = new()
        {
            { PropertyStatusConstants.PENDING, "審核中" },
            { PropertyStatusConstants.PENDING_PAYMENT, "待付款" },
            { PropertyStatusConstants.REJECT_REVISE, "審核未通過(待補件)" },
            { PropertyStatusConstants.REJECTED, "審核未通過" },
            { PropertyStatusConstants.BANNED, "已封鎖" },
            { PropertyStatusConstants.LISTED, "上架中" },
            { PropertyStatusConstants.CONTRACT_ISSUED, "已發出合約" },
            { PropertyStatusConstants.PENDING_RENEWAL, "待續約" },
            { PropertyStatusConstants.LEASE_RENEWING, "續約(房客申請中)" },
            { PropertyStatusConstants.IDLE, "閒置中" },
            { PropertyStatusConstants.ALREADY_RENTED, "出租中" },
            { PropertyStatusConstants.INVALID, "房源已下架" }
        };

        /// <summary>
        /// 房源狀態對應 Bootstrap 樣式類別
        /// </summary>
        private static readonly Dictionary<string, string> StatusStyles = new()
        {
            { PropertyStatusConstants.PENDING, "warning" },
            { PropertyStatusConstants.PENDING_PAYMENT, "info" },
            { PropertyStatusConstants.REJECT_REVISE, "danger" },
            { PropertyStatusConstants.REJECTED, "danger" },
            { PropertyStatusConstants.BANNED, "dark" },
            { PropertyStatusConstants.LISTED, "success" },
            { PropertyStatusConstants.CONTRACT_ISSUED, "primary" },
            { PropertyStatusConstants.PENDING_RENEWAL, "warning" },
            { PropertyStatusConstants.LEASE_RENEWING, "info" },
            { PropertyStatusConstants.IDLE, "secondary" },
            { PropertyStatusConstants.ALREADY_RENTED, "dark" },
            { PropertyStatusConstants.INVALID, "secondary" }
        };

        /// <summary>
        /// 可編輯的房源狀態清單 (v1.2 更新：LISTED 也可編輯)
        /// </summary>
        private static readonly HashSet<string> EditableStatuses = new()
        {
            PropertyStatusConstants.IDLE, PropertyStatusConstants.LISTED
        };

        /// <summary>
        /// 需要用戶行動的房源狀態清單
        /// </summary>
        private static readonly HashSet<string> ActionRequiredStatuses = new()
        {
            PropertyStatusConstants.REJECT_REVISE, PropertyStatusConstants.PENDING_PAYMENT, PropertyStatusConstants.BANNED
        };

        /// <summary>
        /// 可下架的房源狀態清單
        /// </summary>
        private static readonly HashSet<string> TakeDownableStatuses = new()
        {
            PropertyStatusConstants.LISTED
        };

        /// <summary>
        /// 三分組：可用房源狀態
        /// </summary>
        private static readonly HashSet<string> AvailableStatuses = new()
        {
            PropertyStatusConstants.CONTRACT_ISSUED, PropertyStatusConstants.LISTED
        };

        /// <summary>
        /// 三分組：等待刊登的房源狀態
        /// </summary>
        private static readonly HashSet<string> PendingStatuses = new()
        {
            PropertyStatusConstants.PENDING, PropertyStatusConstants.PENDING_PAYMENT, PropertyStatusConstants.REJECT_REVISE, PropertyStatusConstants.IDLE
        };

        /// <summary>
        /// 三分組：目前不可用的房源狀態
        /// </summary>
        private static readonly HashSet<string> UnavailableStatuses = new()
        {
            PropertyStatusConstants.ALREADY_RENTED, PropertyStatusConstants.INVALID, PropertyStatusConstants.REJECTED, PropertyStatusConstants.BANNED, PropertyStatusConstants.PENDING_RENEWAL, PropertyStatusConstants.LEASE_RENEWING
        };

        /// <summary>
        /// 取得房源狀態的中文顯示名稱
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>中文顯示名稱，未知狀態返回原代碼</returns>
        public static string GetDisplayName(string statusCode)
        {
            if (string.IsNullOrWhiteSpace(statusCode))
                return "未知狀態";

            return StatusDisplayNames.TryGetValue(statusCode.ToUpper(), out var displayName) 
                ? displayName 
                : statusCode;
        }

        /// <summary>
        /// 取得房源狀態對應的 Bootstrap 樣式類別
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>Bootstrap 樣式類別名稱</returns>
        public static string GetStatusStyle(string statusCode)
        {
            if (string.IsNullOrWhiteSpace(statusCode))
                return "secondary";

            return StatusStyles.TryGetValue(statusCode.ToUpper(), out var style) 
                ? style 
                : "secondary";
        }

        /// <summary>
        /// 檢查房源是否可編輯
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>是否可編輯</returns>
        public static bool IsEditable(string statusCode)
        {
            if (string.IsNullOrWhiteSpace(statusCode))
                return false;

            return EditableStatuses.Contains(statusCode.ToUpper());
        }

        /// <summary>
        /// 檢查房源是否可下架
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>是否可下架</returns>
        public static bool IsTakeDownable(string statusCode)
        {
            if (string.IsNullOrWhiteSpace(statusCode))
                return false;

            return TakeDownableStatuses.Contains(statusCode.ToUpper());
        }

        /// <summary>
        /// 檢查房源是否需要用戶行動
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>是否需要用戶行動</returns>
        public static bool RequiresAction(string statusCode)
        {
            if (string.IsNullOrWhiteSpace(statusCode))
                return false;

            return ActionRequiredStatuses.Contains(statusCode.ToUpper());
        }

        /// <summary>
        /// 檢查是否為 REJECT_REVISE 狀態（需要補充資料）
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>是否需要補充資料</returns>
        public static bool RequiresDataSupplement(string statusCode)
        {
            return string.Equals(statusCode, "REJECT_REVISE", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 檢查是否為 BANNED 狀態（需要聯絡客服）
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>是否需要聯絡客服</returns>
        public static bool RequiresCustomerService(string statusCode)
        {
            return string.Equals(statusCode, "BANNED", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 取得房源狀態分組
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>房源狀態分組</returns>
        public static PropertyStatusGroup GetStatusGroup(string statusCode)
        {
            if (string.IsNullOrWhiteSpace(statusCode))
                return PropertyStatusGroup.Unavailable;

            var upperCode = statusCode.ToUpper();

            if (AvailableStatuses.Contains(upperCode))
                return PropertyStatusGroup.Available;
            else if (PendingStatuses.Contains(upperCode))
                return PropertyStatusGroup.Pending;
            else
                return PropertyStatusGroup.Unavailable;
        }

        /// <summary>
        /// 取得分組內的所有狀態代碼
        /// </summary>
        /// <param name="group">狀態分組</param>
        /// <returns>狀態代碼清單</returns>
        public static HashSet<string> GetStatusesByGroup(PropertyStatusGroup group)
        {
            return group switch
            {
                PropertyStatusGroup.Available => new HashSet<string>(AvailableStatuses),
                PropertyStatusGroup.Pending => new HashSet<string>(PendingStatuses),
                PropertyStatusGroup.Unavailable => new HashSet<string>(UnavailableStatuses),
                _ => new HashSet<string>()
            };
        }

        /// <summary>
        /// 取得分組的中文顯示名稱
        /// </summary>
        /// <param name="group">狀態分組</param>
        /// <returns>分組中文名稱</returns>
        public static string GetGroupDisplayName(PropertyStatusGroup group)
        {
            return group switch
            {
                PropertyStatusGroup.Available => "可用房源",
                PropertyStatusGroup.Pending => "等待刊登的房源",
                PropertyStatusGroup.Unavailable => "目前不可用的房源",
                _ => "未知分組"
            };
        }

        /// <summary>
        /// 取得分組的圖示
        /// </summary>
        /// <param name="group">狀態分組</param>
        /// <returns>Font Awesome 圖示類別</returns>
        public static string GetGroupIcon(PropertyStatusGroup group)
        {
            return group switch
            {
                PropertyStatusGroup.Available => "fas fa-check-circle text-success",
                PropertyStatusGroup.Pending => "fas fa-clock text-warning", 
                PropertyStatusGroup.Unavailable => "fas fa-ban text-secondary",
                _ => "fas fa-question-circle text-muted"
            };
        }

        /// <summary>
        /// 取得檢視房源的模式
        /// </summary>
        /// <param name="statusCode">房源狀態代碼</param>
        /// <returns>檢視房源模式</returns>
        public static PropertyViewMode GetViewMode(string statusCode)
        {
            var group = GetStatusGroup(statusCode);
            
            return group switch
            {
                PropertyStatusGroup.Available => PropertyViewMode.Full,
                PropertyStatusGroup.Pending => PropertyViewMode.Preview,
                PropertyStatusGroup.Unavailable => PropertyViewMode.Snapshot,
                _ => PropertyViewMode.Snapshot
            };
        }

        /// <summary>
        /// 取得所有可用的房源狀態清單（用於下拉選單等）
        /// </summary>
        /// <returns>狀態代碼與顯示名稱的字典</returns>
        public static Dictionary<string, string> GetAllStatuses()
        {
            return new Dictionary<string, string>(StatusDisplayNames);
        }

    }

    /// <summary>
    /// 房源狀態分組枚舉
    /// </summary>
    public enum PropertyStatusGroup
    {
        /// <summary>
        /// 可用房源：CONTRACT_ISSUED, LISTED
        /// </summary>
        Available,

        /// <summary>
        /// 等待刊登的房源：PENDING, PENDING_PAYMENT, REJECT_REVISE, IDLE
        /// </summary>
        Pending,

        /// <summary>
        /// 目前不可用的房源：ALREADY_RENTED, INVALID, REJECTED, BANNED
        /// </summary>
        Unavailable
    }

    /// <summary>
    /// 房源檢視模式枚舉
    /// </summary>
    public enum PropertyViewMode
    {
        /// <summary>
        /// 完整功能模式：可用房源的完整功能頁面
        /// </summary>
        Full,

        /// <summary>
        /// 預覽模式：等待刊登房源的預覽，功能按鈕失效
        /// </summary>
        Preview,

        /// <summary>
        /// 快照模式：不可用房源的唯讀快照
        /// </summary>
        Snapshot
    }
}