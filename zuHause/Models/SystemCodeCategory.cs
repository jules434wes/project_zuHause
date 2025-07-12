using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 代碼類別表
/// </summary>
public partial class SystemCodeCategory
{
    /// <summary>
    /// 代碼類別
    /// </summary>
    public string CodeCategory { get; set; } = null!;

    /// <summary>
    /// 類別名稱
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

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

    public virtual ICollection<SystemCode> SystemCodes { get; set; } = new List<SystemCode>();
}
