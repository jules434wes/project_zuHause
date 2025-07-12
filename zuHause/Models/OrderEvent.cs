using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 訂單事件表
/// </summary>
public partial class OrderEvent
{
    /// <summary>
    /// 事件ID
    /// </summary>
    public Guid OrderEventsId { get; set; }

    /// <summary>
    /// 訂單ID
    /// </summary>
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 事件類型
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// 事件內容
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// 發生時間
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// 寫入時間
    /// </summary>
    public DateTime RecordedAt { get; set; }

    public virtual FurnitureOrder Order { get; set; } = null!;
}
