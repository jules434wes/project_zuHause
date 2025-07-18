using System.Collections.Generic;

namespace zuHause.AdminViewModels
{
    /// <summary>
    /// Admin 後台常數定義 - 集中管理設定值，避免硬編碼
    /// </summary>
    public static class AdminConstants
    {
        #region 用戶狀態定義
        
        /// <summary>
        /// 用戶帳號狀態選項
        /// </summary>
        public static readonly Dictionary<string, string> AccountStatusOptions = new()
        {
            { "active", "啟用中" },
            { "suspended", "暫時停權" },
            { "banned", "永久停用" }
        };

        /// <summary>
        /// 身分驗證狀態選項
        /// </summary>
        public static readonly Dictionary<string, string> VerificationStatusOptions = new()
        {
            { "verified", "已驗證" },
            { "pending", "等待驗證" },
            { "unverified", "尚未驗證" }
        };

        /// <summary>
        /// 身分證上傳狀態選項
        /// </summary>
        public static readonly Dictionary<string, string> IdUploadStatusOptions = new()
        {
            { "uploaded", "已上傳" },
            { "not_uploaded", "未上傳" },
            { "reviewing", "審核中" },
            { "approved", "審核通過" },
            { "rejected", "審核拒絕" }
        };

        #endregion

        #region 屋源管理定義

        /// <summary>
        /// 屋源狀態選項
        /// </summary>
        public static readonly Dictionary<string, string> PropertyStatusOptions = new()
        {
            { "active", "已上架" },
            { "pending", "待審核" },
            { "inactive", "未上架" },
            { "suspended", "暫停刊登" },
            { "expired", "已過期" }
        };

        /// <summary>
        /// 付款狀態選項
        /// </summary>
        public static readonly Dictionary<string, string> PaymentStatusOptions = new()
        {
            { "paid", "已付款" },
            { "unpaid", "未付款" },
            { "overdue", "逾期未付" },
            { "refunded", "已退款" }
        };

        #endregion

        #region 系統訊息定義

        /// <summary>
        /// 訊息分類選項
        /// </summary>
        public static readonly Dictionary<string, string> MessageCategoryOptions = new()
        {
            { "announcement", "系統公告" },
            { "promotion", "優惠活動" },
            { "maintenance", "維護通知" },
            { "warning", "警告通知" }
        };

        /// <summary>
        /// 發送對象選項
        /// </summary>
        public static readonly Dictionary<string, string> AudienceTypeOptions = new()
        {
            { "all_members", "全體會員" },
            { "all_landlords", "全體房東" },
            { "individual", "個別用戶" }
        };

        #endregion

        #region 客服案件定義

        /// <summary>
        /// 客服案件狀態選項
        /// </summary>
        public static readonly Dictionary<string, string> TicketStatusOptions = new()
        {
            { "open", "未結案" },
            { "in_progress", "處理中" },
            { "resolved", "已解決" },
            { "closed", "已關閉" }
        };

        /// <summary>
        /// 客服案件優先級選項
        /// </summary>
        public static readonly Dictionary<string, string> TicketPriorityOptions = new()
        {
            { "low", "低" },
            { "normal", "普通" },
            { "high", "高" },
            { "urgent", "緊急" }
        };

        #endregion

        #region 地區選項

        /// <summary>
        /// 城市選項
        /// </summary>
        public static readonly Dictionary<string, string> CityOptions = new()
        {
            { "1", "台北市" },
            { "2", "新北市" },
            { "3", "桃園市" },
            { "4", "台中市" },
            { "5", "台南市" },
            { "6", "高雄市" },
            { "7", "基隆市" },
            { "8", "新竹市" },
            { "9", "嘉義市" },
            { "10", "新竹縣" },
            { "11", "苗栗縣" },
            { "12", "彰化縣" },
            { "13", "南投縣" },
            { "14", "雲林縣" },
            { "15", "嘉義縣" },
            { "16", "屏東縣" },
            { "17", "宜蘭縣" },
            { "18", "花蓮縣" },
            { "19", "台東縣" },
            { "20", "澎湖縣" },
            { "21", "金門縣" },
            { "22", "連江縣" }
        };

        #endregion

        #region CSS 樣式對應

        /// <summary>
        /// 狀態對應的 Bootstrap Badge 樣式
        /// </summary>
        public static readonly Dictionary<string, string> StatusBadgeStyles = new()
        {
            // 帳號狀態
            { "active", "bg-success" },
            { "suspended", "bg-warning" },
            { "banned", "bg-danger" },
            
            // 驗證狀態
            { "verified", "bg-success" },
            { "pending", "bg-warning" },
            { "unverified", "bg-secondary" },
            
            // 屋源狀態
            { "inactive", "bg-secondary" },
            
            // 付款狀態
            { "paid", "bg-success" },
            { "unpaid", "bg-danger" },
            { "overdue", "bg-danger" },
            { "refunded", "bg-info" },
            
            // 客服案件狀態
            { "open", "bg-danger" },
            { "in_progress", "bg-warning" },
            { "resolved", "bg-success" },
            { "closed", "bg-secondary" }
        };

        #endregion

        #region 搜尋欄位定義

        /// <summary>
        /// 用戶搜尋欄位選項
        /// </summary>
        public static readonly Dictionary<string, string> UserSearchFields = new()
        {
            { "memberID", "會員ID" },
            { "memberName", "會員姓名" },
            { "email", "電子郵件" },
            { "phoneNumber", "手機號碼" },
            { "nationalNo", "身分證字號" },
            { "addressLine", "詳細地址" }
        };

        /// <summary>
        /// 系統訊息搜尋欄位選項
        /// </summary>
        public static readonly Dictionary<string, string> MessageSearchFields = new()
        {
            { "title", "訊息標題" },
            { "content", "訊息內容" },
            { "messageID", "訊息ID" },
            { "adminName", "發送者" }
        };

        #endregion

        #region 分頁設定

        /// <summary>
        /// 預設每頁顯示筆數
        /// </summary>
        public const int DefaultPageSize = 10;

        /// <summary>
        /// 每頁顯示筆數選項
        /// </summary>
        public static readonly int[] PageSizeOptions = { 10, 20, 50, 100 };

        #endregion
    }
}