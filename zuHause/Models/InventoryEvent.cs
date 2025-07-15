using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 庫存事件表
/// </summary>
public partial class InventoryEvent
{
    /// <summary>
    /// 事件ID
    /// </summary>
    public Guid FurnitureInventoryId { get; set; }

    /// <summary>
    /// 商品ID
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 事件類型
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// 異動數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 來源類型
    /// </summary>
    public string? SourceType { get; set; }

    /// <summary>
    /// 來源編號
    /// </summary>
    public string? SourceId { get; set; }

    /// <summary>
    /// 發生時間
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// 寫入時間
    /// </summary>
    public DateTime RecordedAt { get; set; }

    public virtual FurnitureProduct Product { get; set; } = null!;
}
