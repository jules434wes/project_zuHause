using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 訂單事件表
/// </summary>
[Table("orderEvents")]
public partial class OrderEvent
{
    /// <summary>
    /// 事件ID
    /// </summary>
    [Key]
    [Column("orderEventsId")]
    public Guid OrderEventsId { get; set; }

    /// <summary>
    /// 訂單ID
    /// </summary>
    [Column("orderId")]
    [StringLength(50)]
    [Unicode(false)]
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 事件類型
    /// </summary>
    [Column("eventType")]
    [StringLength(30)]
    [Unicode(false)]
    public string EventType { get; set; } = null!;

    /// <summary>
    /// 事件內容
    /// </summary>
    [Column("payload")]
    public string? Payload { get; set; }

    /// <summary>
    /// 發生時間
    /// </summary>
    [Column("occurredAt")]
    [Precision(0)]
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// 寫入時間
    /// </summary>
    [Column("recordedAt")]
    [Precision(0)]
    public DateTime RecordedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderEvents")]
    public virtual FurnitureOrder Order { get; set; } = null!;
}
