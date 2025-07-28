using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具歷史訂單清單
/// </summary>
[Table("furnitureOrderHistory")]
public partial class FurnitureOrderHistory
{
    /// <summary>
    /// 流水號
    /// </summary>
    [Key]
    [Column("furnitureOrderHistoryId")]
    [StringLength(50)]
    [Unicode(false)]
    public string FurnitureOrderHistoryId { get; set; } = null!;

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
    /// 商品名稱快照
    /// </summary>
    [Column("productNameSnapshot")]
    [StringLength(100)]
    public string ProductNameSnapshot { get; set; } = null!;

    /// <summary>
    /// 商品描述快照
    /// </summary>
    [Column("descriptionSnapshot")]
    public string? DescriptionSnapshot { get; set; }

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
    /// 租借開始日
    /// </summary>
    [Column("rentalStart")]
    public DateOnly RentalStart { get; set; }

    /// <summary>
    /// 租借結束日
    /// </summary>
    [Column("rentalEnd")]
    public DateOnly RentalEnd { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    [Column("subTotal", TypeName = "decimal(12, 0)")]
    public decimal SubTotal { get; set; }

    /// <summary>
    /// 明細狀態
    /// </summary>
    [Column("itemStatus")]
    [StringLength(50)]
    public string ItemStatus { get; set; } = null!;

    /// <summary>
    /// 實際歸還日期
    /// </summary>
    [Column("returnedAt")]
    public DateOnly? ReturnedAt { get; set; }

    /// <summary>
    /// 損壞說明
    /// </summary>
    [Column("damageNote")]
    [StringLength(255)]
    public string? DamageNote { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("FurnitureOrderHistories")]
    public virtual FurnitureOrder Order { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("FurnitureOrderHistories")]
    public virtual FurnitureProduct Product { get; set; } = null!;
}
