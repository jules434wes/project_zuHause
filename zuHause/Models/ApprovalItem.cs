using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 審核明細
/// </summary>
public partial class ApprovalItem
{
    /// <summary>
    /// 審核明細ID
    /// </summary>
    public int ApprovalItemId { get; set; }

    /// <summary>
    /// 審核ID
    /// </summary>
    public int ApprovalId { get; set; }

    /// <summary>
    /// 操作者ID
    /// </summary>
    public int ActionBy { get; set; }

    /// <summary>
    /// 操作類型
    /// </summary>
    public string ActionType { get; set; } = null!;

    /// <summary>
    /// 操作備註
    /// </summary>
    public string? ActionNote { get; set; }

    /// <summary>
    /// 審核快照JSON
    /// </summary>
    public string? SnapshotJson { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual Approval Approval { get; set; } = null!;
}
