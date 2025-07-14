using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace zuHause.Models;

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
    public string FurnitureCategoriesId { get; set; } = null!; //主鍵

    /// <summary>
    /// 上層分類ID
    /// </summary>
     [Column("parentId")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 分類名稱
    /// </summary>
    [Required(ErrorMessage = "分類名稱為必填")]
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 階層層級
    /// </summary>

    /// <summary>
    /// 顯示排序
    /// </summary>
    [Column("displayOrder")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<FurnitureProduct> FurnitureProducts { get; set; } = new List<FurnitureProduct>();

    public virtual ICollection<FurnitureCategory> InverseParent { get; set; } = new List<FurnitureCategory>();

    public virtual FurnitureCategory? Parent { get; set; }
}
