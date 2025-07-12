using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 家具商品分類表
/// </summary>
public partial class FurnitureCategory
{
    /// <summary>
    /// 分類ID
    /// </summary>
    public string FurnitureCategoriesId { get; set; } = null!;

    /// <summary>
    /// 上層分類ID
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 階層層級
    /// </summary>
    public byte Depth { get; set; }

    /// <summary>
    /// 顯示排序
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<FurnitureProduct> FurnitureProducts { get; set; } = new List<FurnitureProduct>();

    public virtual ICollection<FurnitureCategory> InverseParent { get; set; } = new List<FurnitureCategory>();

    public virtual FurnitureCategory? Parent { get; set; }
}
