using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 房源設備分類資料表
/// </summary>
[Table("propertyEquipmentCategories")]
public partial class PropertyEquipmentCategory
{
    /// <summary>
    /// 上層分類ID
    /// </summary>
    [Column("parentCategoryID")]
    public int? ParentCategoryId { get; set; }

    /// <summary>
    /// 設備名稱
    /// </summary>
    [Column("categoryName")]
    [StringLength(50)]
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

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

    [Key]
    [Column("categoryID")]
    public int CategoryId { get; set; }

    [InverseProperty("ParentCategory")]
    public virtual ICollection<PropertyEquipmentCategory> InverseParentCategory { get; set; } = new List<PropertyEquipmentCategory>();

    [ForeignKey("ParentCategoryId")]
    [InverseProperty("InverseParentCategory")]
    public virtual PropertyEquipmentCategory? ParentCategory { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<PropertyEquipmentRelation> PropertyEquipmentRelations { get; set; } = new List<PropertyEquipmentRelation>();
}
