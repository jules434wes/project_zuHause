using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具訂單查詢表
/// </summary>
[Table("furnitureOrders")]
[Index("MemberId", "Status", Name = "IX_furnOrders_member_status")]
public partial class FurnitureOrder
{
    /// <summary>
    /// 訂單ID
    /// </summary>
    [Key]
    [Column("furnitureOrderId")]
    [StringLength(50)]
    [Unicode(false)]
    public string FurnitureOrderId { get; set; } = null!;

    /// <summary>
    /// 會員ID
    /// </summary>
    [Column("memberId")]
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    [Column("propertyId")]
    public int? PropertyId { get; set; }

    /// <summary>
    /// 成立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 總金額
    /// </summary>
    [Column("totalAmount", TypeName = "decimal(12, 0)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 訂單狀態
    /// </summary>
    [Column("status")]
    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    /// <summary>
    /// 付款狀態
    /// </summary>
    [Column("paymentStatus")]
    [StringLength(20)]
    [Unicode(false)]
    public string PaymentStatus { get; set; } = null!;

    /// <summary>
    /// 合約連結
    /// </summary>
    [Column("contractLink")]
    [StringLength(255)]
    public string? ContractLink { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    [Column("deletedAt")]
    [Precision(0)]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("FurnitureOrder")]
    public virtual ICollection<CustomerServiceTicket> CustomerServiceTickets { get; set; } = new List<CustomerServiceTicket>();

    [InverseProperty("Order")]
    public virtual ICollection<FurnitureOrderHistory> FurnitureOrderHistories { get; set; } = new List<FurnitureOrderHistory>();

    [InverseProperty("Order")]
    public virtual ICollection<FurnitureOrderItem> FurnitureOrderItems { get; set; } = new List<FurnitureOrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<FurnitureRentalContract> FurnitureRentalContracts { get; set; } = new List<FurnitureRentalContract>();

    [ForeignKey("MemberId")]
    [InverseProperty("FurnitureOrders")]
    public virtual Member Member { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<OrderEvent> OrderEvents { get; set; } = new List<OrderEvent>();

    [ForeignKey("PropertyId")]
    [InverseProperty("FurnitureOrders")]
    public virtual Property? Property { get; set; }
}
