using System.ComponentModel.DataAnnotations;

namespace zuHause.AdminDTOs
{
    /// <summary>
    /// 更新客服案件回覆 DTO
    /// </summary>
    public class UpdateCustomerServiceReplyDto
    {
        /// <summary>
        /// 客服案件ID
        /// </summary>
        [Required(ErrorMessage = "案件ID為必填項目")]
        public int TicketId { get; set; }

        /// <summary>
        /// 回覆內容
        /// </summary>
        [Required(ErrorMessage = "回覆內容為必填項目")]
        [StringLength(2000, ErrorMessage = "回覆內容長度不能超過2000字")]
        public string ReplyContent { get; set; } = null!;

        /// <summary>
        /// 處理狀態代碼 (PENDING|PROGRESS|RESOLVED)
        /// </summary>
        [Required(ErrorMessage = "狀態代碼為必填項目")]
        public string StatusCode { get; set; } = null!;

        /// <summary>
        /// 內部註記 (選填)
        /// </summary>
        [StringLength(500, ErrorMessage = "內部註記長度不能超過500字")]
        public string? InternalNote { get; set; }
    }
}