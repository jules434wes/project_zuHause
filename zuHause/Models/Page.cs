using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 共用頁面代碼表
/// </summary>
public partial class Page
{
    /// <summary>
    /// 頁面識別碼
    /// </summary>
    public string PageCode { get; set; } = null!;

    /// <summary>
    /// 頁面名稱
    /// </summary>
    public string PageName { get; set; } = null!;

    /// <summary>
    /// 路徑
    /// </summary>
    public string RoutePath { get; set; } = null!;

    /// <summary>
    /// 模組
    /// </summary>
    public string ModuleScope { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 顯示順序
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

    public virtual ICollection<CarouselImage> CarouselImages { get; set; } = new List<CarouselImage>();
}
