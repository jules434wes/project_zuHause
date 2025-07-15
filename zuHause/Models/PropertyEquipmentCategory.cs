using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 房源設備分類資料表
/// </summary>
public partial class PropertyEquipmentCategory
{
    /// <summary>
    /// 上層分類ID
    /// </summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>
    /// 設備名稱
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public int CategoryId { get; set; }

    public virtual ICollection<PropertyEquipmentCategory> InverseParentCategory { get; set; } = new List<PropertyEquipmentCategory>();

    public virtual PropertyEquipmentCategory? ParentCategory { get; set; }

    public virtual ICollection<PropertyEquipmentRelation> PropertyEquipmentRelations { get; set; } = new List<PropertyEquipmentRelation>();
}
