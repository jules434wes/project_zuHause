using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 家具訂單查詢明細表
/// </summary>
public partial class FurnitureOrderItem
{
    /// <summary>
    /// 明細ID
    /// </summary>
    public string FurnitureOrderItemId { get; set; } = null!;

    /// <summary>
    /// 訂單ID
    /// </summary>
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 商品ID
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 單日租金快照
    /// </summary>
    public decimal DailyRentalSnapshot { get; set; }

    /// <summary>
    /// 租期(天)
    /// </summary>
    public int RentalDays { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual FurnitureOrder Order { get; set; } = null!;

    public virtual FurnitureProduct Product { get; set; } = null!;
}
