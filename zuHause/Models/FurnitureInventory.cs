using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 家具庫存表
/// </summary>
public partial class FurnitureInventory
{
    /// <summary>
    /// 庫存ID
    /// </summary>
    public string FurnitureInventoryId { get; set; } = null!;

    /// <summary>
    /// 商品ID
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 總庫存數量
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 已出租數量
    /// </summary>
    public int RentedQuantity { get; set; }

    /// <summary>
    /// 可用庫存
    /// </summary>
    public int AvailableQuantity { get; set; }

    /// <summary>
    /// 安全庫存
    /// </summary>
    public int SafetyStock { get; set; }

    /// <summary>
    /// 最後更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual FurnitureProduct Product { get; set; } = null!;
}
