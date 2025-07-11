using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 家具購物車明細表
/// </summary>
public partial class FurnitureCartItem
{
    /// <summary>
    /// 明細ID
    /// </summary>
    public string CartItemId { get; set; } = null!;

    /// <summary>
    /// 購物車ID
    /// </summary>
    public string CartId { get; set; } = null!;

    /// <summary>
    /// 商品ID
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 租期(天)
    /// </summary>
    public int RentalDays { get; set; }

    /// <summary>
    /// 單價快照
    /// </summary>
    public decimal UnitPriceSnapshot { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual FurnitureCart Cart { get; set; } = null!;

    public virtual FurnitureProduct Product { get; set; } = null!;
}
