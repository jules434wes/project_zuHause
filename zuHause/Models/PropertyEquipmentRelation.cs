using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 房源設備關聯資料表
/// </summary>
public partial class PropertyEquipmentRelation
{
    /// <summary>
    /// 關聯ID
    /// </summary>
    public int RelationId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    public int PropertyId { get; set; }

    /// <summary>
    /// 設備分類ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual PropertyEquipmentCategory Category { get; set; } = null!;

    public virtual Property Property { get; set; } = null!;
}
