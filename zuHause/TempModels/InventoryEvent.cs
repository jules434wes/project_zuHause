using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 庫存事件表
/// </summary>
[Table("inventoryEvents")]
public partial class InventoryEvent
{
    /// <summary>
    /// 事件ID
    /// </summary>
    [Key]
    [Column("furnitureInventoryId")]
    public Guid FurnitureInventoryId { get; set; }

    /// <summary>
    /// 商品ID
    /// </summary>
    [Column("productId")]
    [StringLength(50)]
    [Unicode(false)]
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 事件類型
    /// </summary>
    [Column("eventType")]
    [StringLength(20)]
    [Unicode(false)]
    public string EventType { get; set; } = null!;

    /// <summary>
    /// 異動數量
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 來源類型
    /// </summary>
    [Column("sourceType")]
    [StringLength(20)]
    [Unicode(false)]
    public string? SourceType { get; set; }

    /// <summary>
    /// 來源編號
    /// </summary>
    [Column("sourceId")]
    [StringLength(50)]
    [Unicode(false)]
    public string? SourceId { get; set; }

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

    [ForeignKey("ProductId")]
    [InverseProperty("InventoryEvents")]
    public virtual FurnitureProduct Product { get; set; } = null!;
}
