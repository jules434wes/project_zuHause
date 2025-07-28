namespace zuHause.Constants
{
    /// <summary>
    /// 房源狀態常數定義
    /// 統一管理所有房源狀態代碼，避免魔術字串
    /// </summary>
    public static class PropertyStatusConstants
    {
        // === 審核階段狀態 ===
        
        /// <summary>
        /// 審核中
        /// </summary>
        public const string PENDING = "PENDING";
        
        /// <summary>
        /// 待付款
        /// </summary>
        public const string PENDING_PAYMENT = "PENDING_PAYMENT";
        
        /// <summary>
        /// 審核未通過(待補件)
        /// </summary>
        public const string REJECT_REVISE = "REJECT_REVISE";
        
        /// <summary>
        /// 審核未通過
        /// </summary>
        public const string REJECTED = "REJECTED";
        
        /// <summary>
        /// 已封鎖
        /// </summary>
        public const string BANNED = "BANNED";
        
        // === 上架階段狀態 ===
        
        /// <summary>
        /// 上架中
        /// </summary>
        public const string LISTED = "LISTED";
        
        /// <summary>
        /// 已發出合約
        /// </summary>
        public const string CONTRACT_ISSUED = "CONTRACT_ISSUED";
        
        /// <summary>
        /// 待續約
        /// </summary>
        public const string PENDING_RENEWAL = "PENDING_RENEWAL";
        
        /// <summary>
        /// 續約(房客申請中)
        /// </summary>
        public const string LEASE_RENEWING = "LEASE_RENEWING";
        
        // === 其他狀態 ===
        
        /// <summary>
        /// 閒置中
        /// </summary>
        public const string IDLE = "IDLE";
        
        /// <summary>
        /// 出租中
        /// </summary>
        public const string ALREADY_RENTED = "ALREADY_RENTED";
        
        /// <summary>
        /// 房源已下架
        /// </summary>
        public const string INVALID = "INVALID";
        
        // === 狀態集合定義 ===
        
        /// <summary>
        /// 所有有效狀態清單
        /// </summary>
        public static readonly string[] ALL_STATUSES = {
            PENDING, PENDING_PAYMENT, REJECT_REVISE, REJECTED, BANNED,
            LISTED, CONTRACT_ISSUED, PENDING_RENEWAL, LEASE_RENEWING,
            IDLE, ALREADY_RENTED, INVALID
        };
        
        /// <summary>
        /// 狀態分組定義
        /// </summary>
        public static class Groups
        {
            /// <summary>
            /// 等待刊登的狀態
            /// </summary>
            public static readonly string[] PENDING_STATES = {
                PENDING, PENDING_PAYMENT, REJECT_REVISE
            };
            
            /// <summary>
            /// 可用房源狀態
            /// </summary>
            public static readonly string[] AVAILABLE_STATES = {
                LISTED, CONTRACT_ISSUED, PENDING_RENEWAL, LEASE_RENEWING
            };
            
            /// <summary>
            /// 不可用房源狀態
            /// </summary>
            public static readonly string[] UNAVAILABLE_STATES = {
                REJECTED, BANNED, IDLE, ALREADY_RENTED, INVALID
            };
        }
        
        // === 輔助方法 ===
        
        /// <summary>
        /// 驗證狀態是否有效
        /// </summary>
        /// <param name="status">狀態代碼</param>
        /// <returns>是否為有效狀態</returns>
        public static bool IsValidStatus(string status)
        {
            return ALL_STATUSES.Contains(status);
        }
        
        /// <summary>
        /// 判斷是否為等待刊登狀態
        /// </summary>
        /// <param name="status">狀態代碼</param>
        /// <returns>是否為等待刊登狀態</returns>
        public static bool IsPendingStatus(string status)
        {
            return Groups.PENDING_STATES.Contains(status);
        }
        
        /// <summary>
        /// 判斷是否為可用狀態
        /// </summary>
        /// <param name="status">狀態代碼</param>
        /// <returns>是否為可用狀態</returns>
        public static bool IsAvailableStatus(string status)
        {
            return Groups.AVAILABLE_STATES.Contains(status);
        }
        
        /// <summary>
        /// 判斷是否為不可用狀態
        /// </summary>
        /// <param name="status">狀態代碼</param>
        /// <returns>是否為不可用狀態</returns>
        public static bool IsUnavailableStatus(string status)
        {
            return Groups.UNAVAILABLE_STATES.Contains(status);
        }
    }
}