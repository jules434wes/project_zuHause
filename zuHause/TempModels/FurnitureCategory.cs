using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具商品分類表
/// </summary>
[Table("furnitureCategories")]
public partial class FurnitureCategory
{
    /// <summary>
    /// 分類ID
    /// </summary>
    [Key]
    [Column("furnitureCategoriesId")]
    [StringLength(50)]
    [Unicode(false)]
    public string FurnitureCategoriesId { get; set; } = null!;

    /// <summary>
    /// 上層分類ID
    /// </summary>
    [Column("parentId")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ParentId { get; set; }

    /// <summary>
    /// 分類名稱
    /// </summary>
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 階層層級
    /// </summary>
    [Column("depth")]
    public byte Depth { get; set; }

    /// <summary>
    /// 顯示排序
    /// </summary>
    [Column("displayOrder")]
    public int DisplayOrder { get; set; }

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

    [InverseProperty("Category")]
    public virtual ICollection<FurnitureProduct> FurnitureProducts { get; set; } = new List<FurnitureProduct>();

    [InverseProperty("Parent")]
    public virtual ICollection<FurnitureCategory> InverseParent { get; set; } = new List<FurnitureCategory>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual FurnitureCategory? Parent { get; set; }
}
