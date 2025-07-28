using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具訂單查詢明細表
/// </summary>
[Table("furnitureOrderItems")]
public partial class FurnitureOrderItem
{
    /// <summary>
    /// 明細ID
    /// </summary>
    [Key]
    [Column("furnitureOrderItemId")]
    [StringLength(50)]
    [Unicode(false)]
    public string FurnitureOrderItemId { get; set; } = null!;

    /// <summary>
    /// 訂單ID
    /// </summary>
    [Column("orderId")]
    [StringLength(50)]
    [Unicode(false)]
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 商品ID
    /// </summary>
    [Column("productId")]
    [StringLength(50)]
    [Unicode(false)]
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 數量
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 單日租金快照
    /// </summary>
    [Column("dailyRentalSnapshot", TypeName = "decimal(10, 0)")]
    public decimal DailyRentalSnapshot { get; set; }

    /// <summary>
    /// 租期(天)
    /// </summary>
    [Column("rentalDays")]
    public int RentalDays { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    [Column("subTotal", TypeName = "decimal(12, 0)")]
    public decimal SubTotal { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("FurnitureOrderItems")]
    public virtual FurnitureOrder Order { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("FurnitureOrderItems")]
    public virtual FurnitureProduct Product { get; set; } = null!;
}
