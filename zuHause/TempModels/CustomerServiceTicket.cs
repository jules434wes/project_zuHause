using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 客服聯繫表
/// </summary>
[Table("customerServiceTickets")]
[Index("CategoryCode", Name = "IX_customerServiceTickets_categoryCode")]
[Index("ContractId", Name = "IX_customerServiceTickets_contractId")]
[Index("CreatedAt", Name = "IX_customerServiceTickets_createdAt")]
[Index("IsResolved", Name = "IX_customerServiceTickets_isResolved")]
[Index("MemberId", Name = "IX_customerServiceTickets_memberId")]
[Index("PropertyId", Name = "IX_customerServiceTickets_propertyId")]
[Index("StatusCode", Name = "IX_customerServiceTickets_statusCode")]
public partial class CustomerServiceTicket
{
    /// <summary>
    /// 客服單ID (自動遞增，從201開始)
    /// </summary>
    [Key]
    [Column("ticketId")]
    public int TicketId { get; set; }

    /// <summary>
    /// 使用者ID
    /// </summary>
    [Column("memberId")]
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    [Column("propertyId")]
    public int? PropertyId { get; set; }

    /// <summary>
    /// 租約ID
    /// </summary>
    [Column("contractId")]
    public int? ContractId { get; set; }

    /// <summary>
    /// 家具訂單ID
    /// </summary>
    [Column("furnitureOrderId")]
    [StringLength(50)]
    [Unicode(false)]
    public string? FurnitureOrderId { get; set; }

    /// <summary>
    /// 主旨
    /// </summary>
    [Column("subject")]
    [StringLength(100)]
    public string Subject { get; set; } = null!;

    /// <summary>
    /// 主分類代碼
    /// </summary>
    [Column("categoryCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string CategoryCode { get; set; } = null!;

    /// <summary>
    /// 需求內容
    /// </summary>
    [Column("ticketContent")]
    public string TicketContent { get; set; } = null!;

    /// <summary>
    /// 客服最後回覆
    /// </summary>
    [Column("replyContent")]
    public string? ReplyContent { get; set; }

    /// <summary>
    /// 狀態代碼
    /// </summary>
    [Column("statusCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後回覆時間
    /// </summary>
    [Column("replyAt")]
    [Precision(0)]
    public DateTime? ReplyAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 客服人員ID
    /// </summary>
    [Column("handledBy")]
    public int? HandledBy { get; set; }

    /// <summary>
    /// 是否結案
    /// </summary>
    [Column("isResolved")]
    public bool IsResolved { get; set; }

    [ForeignKey("ContractId")]
    [InverseProperty("CustomerServiceTickets")]
    public virtual Contract? Contract { get; set; }

    [ForeignKey("FurnitureOrderId")]
    [InverseProperty("CustomerServiceTickets")]
    public virtual FurnitureOrder? FurnitureOrder { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("CustomerServiceTickets")]
    public virtual Member Member { get; set; } = null!;

    [ForeignKey("PropertyId")]
    [InverseProperty("CustomerServiceTickets")]
    public virtual Property? Property { get; set; }
}
