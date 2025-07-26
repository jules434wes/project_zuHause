using System;

namespace zuHause.AdminDTOs
{
    /// <summary>
    /// 客服案件篩選參數 DTO
    /// </summary>
    public class CustomerServiceFilterDto
    {
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 搜尋欄位 (all|subject|memberName|content)
        /// </summary>
        public string? SearchField { get; set; } = "all";

        /// <summary>
        /// 狀態篩選 (PENDING|PROGRESS|RESOLVED)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 類別篩選 (PROPERTY|CONTRACT|FURNITURE)
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 開始日期
        /// </summary>
        public DateTime? DateStart { get; set; }

        /// <summary>
        /// 結束日期
        /// </summary>
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// 頁碼 (預設: 1)
        /// </summary>
        public int? Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數 (預設: 10)
        /// </summary>
        public int? PageSize { get; set; } = 10;
    }
}