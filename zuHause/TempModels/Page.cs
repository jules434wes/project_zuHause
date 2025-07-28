using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 共用頁面代碼表
/// </summary>
[Table("pages")]
public partial class Page
{
    /// <summary>
    /// 頁面識別碼
    /// </summary>
    [Key]
    [Column("pageCode")]
    [StringLength(50)]
    [Unicode(false)]
    public string PageCode { get; set; } = null!;

    /// <summary>
    /// 頁面名稱
    /// </summary>
    [Column("pageName")]
    [StringLength(50)]
    public string PageName { get; set; } = null!;

    /// <summary>
    /// 路徑
    /// </summary>
    [Column("routePath")]
    [StringLength(100)]
    [Unicode(false)]
    public string RoutePath { get; set; } = null!;

    /// <summary>
    /// 模組
    /// </summary>
    [Column("moduleScope")]
    [StringLength(20)]
    [Unicode(false)]
    public string ModuleScope { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 顯示順序
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

    [InverseProperty("PageCodeNavigation")]
    public virtual ICollection<CarouselImage> CarouselImages { get; set; } = new List<CarouselImage>();
}
