using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具購物車明細表
/// </summary>
[Table("furnitureCartItems")]
public partial class FurnitureCartItem
{
    /// <summary>
    /// 明細ID
    /// </summary>
    [Key]
    [Column("cartItemId")]
    [StringLength(50)]
    [Unicode(false)]
    public string CartItemId { get; set; } = null!;

    /// <summary>
    /// 購物車ID
    /// </summary>
    [Column("cartId")]
    [StringLength(50)]
    [Unicode(false)]
    public string CartId { get; set; } = null!;

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
    /// 租期(天)
    /// </summary>
    [Column("rentalDays")]
    public int RentalDays { get; set; }

    /// <summary>
    /// 單價快照
    /// </summary>
    [Column("unitPriceSnapshot", TypeName = "decimal(10, 0)")]
    public decimal UnitPriceSnapshot { get; set; }

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

    [ForeignKey("CartId")]
    [InverseProperty("FurnitureCartItems")]
    public virtual FurnitureCart Cart { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("FurnitureCartItems")]
    public virtual FurnitureProduct Product { get; set; } = null!;
}
