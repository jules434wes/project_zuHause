using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 代碼總表
/// </summary>
public partial class SystemCode
{
    /// <summary>
    /// 代碼類別
    /// </summary>
    public string CodeCategory { get; set; } = null!;

    /// <summary>
    /// 代碼
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 代碼名稱
    /// </summary>
    public string CodeName { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int DisplayOrder { get; set; }

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

    public virtual ICollection<ApprovalItem> ApprovalItems { get; set; } = new List<ApprovalItem>();

    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    public virtual SystemCodeCategory CodeCategoryNavigation { get; set; } = null!;
}
