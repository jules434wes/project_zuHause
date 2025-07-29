using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 房源設備關聯資料表
/// </summary>
[Table("propertyEquipmentRelations")]
public partial class PropertyEquipmentRelation
{
    /// <summary>
    /// 關聯ID
    /// </summary>
    [Key]
    [Column("relationID")]
    public int RelationId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    [Column("propertyID")]
    public int PropertyId { get; set; }

    /// <summary>
    /// 設備分類ID
    /// </summary>
    [Column("categoryID")]
    public int CategoryId { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("PropertyEquipmentRelations")]
    public virtual PropertyEquipmentCategory Category { get; set; } = null!;

    [ForeignKey("PropertyId")]
    [InverseProperty("PropertyEquipmentRelations")]
    public virtual Property Property { get; set; } = null!;
}
