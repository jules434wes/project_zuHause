using System;

namespace zuHause.AdminViewModels
{
    /// <summary>
    /// 客服案件詳情頁面 ViewModel
    /// </summary>
    public class AdminCustomerServiceDetailsViewModel
    {
        /// <summary>
        /// 客服案件ID
        /// </summary>
        public int TicketId { get; set; }

        /// <summary>
        /// 客服案件顯示編號 (CS-0001 格式)
        /// </summary>
        public string TicketIdDisplay { get; set; } = null!;

        /// <summary>
        /// 會員ID
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// 會員姓名
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// 案件主旨
        /// </summary>
        public string Subject { get; set; } = null!;

        /// <summary>
        /// 類別代碼
        /// </summary>
        public string CategoryCode { get; set; } = null!;

        /// <summary>
        /// 類別顯示名稱
        /// </summary>
        public string CategoryDisplay { get; set; } = null!;

        /// <summary>
        /// 問題內容
        /// </summary>
        public string TicketContent { get; set; } = null!;

        /// <summary>
        /// 客服回覆內容
        /// </summary>
        public string? ReplyContent { get; set; }

        /// <summary>
        /// 狀態代碼
        /// </summary>
        public string StatusCode { get; set; } = null!;

        /// <summary>
        /// 狀態顯示名稱
        /// </summary>
        public string StatusDisplay { get; set; } = null!;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最後回覆時間
        /// </summary>
        public DateTime? ReplyAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 處理人員ID
        /// </summary>
        public int? HandledBy { get; set; }

        /// <summary>
        /// 處理人員姓名
        /// </summary>
        public string HandledByName { get; set; } = "---";

        /// <summary>
        /// 是否已結案
        /// </summary>
        public bool IsResolved { get; set; }

        /// <summary>
        /// 房源ID (僅當類別為房源時有值)
        /// </summary>
        public int? PropertyId { get; set; }

        /// <summary>
        /// 租約ID (僅當類別為租約時有值)
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// 傢俱訂單ID (僅當類別為傢俱時有值)
        /// </summary>
        public string? FurnitureOrderId { get; set; }

        /// <summary>
        /// 頁面標題
        /// </summary>
        public string PageTitle { get; set; } = "客服案件詳情";
    }
}