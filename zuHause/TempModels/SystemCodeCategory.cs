using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 代碼類別表
/// </summary>
[Table("systemCodeCategories")]
public partial class SystemCodeCategory
{
    /// <summary>
    /// 代碼類別
    /// </summary>
    [Key]
    [Column("codeCategory")]
    [StringLength(20)]
    public string CodeCategory { get; set; } = null!;

    /// <summary>
    /// 類別名稱
    /// </summary>
    [Column("categoryName")]
    [StringLength(50)]
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    [Column("description")]
    [StringLength(100)]
    public string? Description { get; set; }

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

    [InverseProperty("CodeCategoryNavigation")]
    public virtual ICollection<SystemCode> SystemCodes { get; set; } = new List<SystemCode>();
}
