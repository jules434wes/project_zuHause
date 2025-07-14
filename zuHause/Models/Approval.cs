using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 審核主檔
/// </summary>
public partial class Approval
{
    /// <summary>
    /// 審核ID
    /// </summary>
    public int ApprovalId { get; set; }

    /// <summary>
    /// 模組代碼
    /// </summary>
    public string ModuleCode { get; set; } = null!;

    /// <summary>
    /// 來源ID
    /// </summary>
    public int SourceId { get; set; }

    /// <summary>
    /// 申請會員ID
    /// </summary>
    public int ApplicantMemberId { get; set; }

    /// <summary>
    /// 審核狀態碼
    /// </summary>
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 審核人員ID
    /// </summary>
    public int? CurrentApproverId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 審核狀態分類
    /// </summary>
    public string? StatusCategory { get; set; }

    public virtual Member ApplicantMember { get; set; } = null!;

    public virtual ICollection<ApprovalItem> ApprovalItems { get; set; } = new List<ApprovalItem>();

    public virtual Property Source { get; set; } = null!;

    public virtual SystemCode? SystemCode { get; set; }
}
