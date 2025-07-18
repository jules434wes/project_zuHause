using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 審核主檔
/// </summary>
public partial class Approval
{
    /// <summary>
    /// 審核ID (自動遞增，從701開始)
    /// </summary>
    public int ApprovalId { get; set; }

    /// <summary>
    /// 模組代碼
    /// </summary>
    public string ModuleCode { get; set; } = null!;

    /// <summary>
    /// 審核房源ID
    /// </summary>
    public int? SourcePropertyId { get; set; }

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
    /// 狀態類別 (計算欄位)
    /// </summary>
    public string? StatusCategory { get; set; }

    public virtual Member ApplicantMember { get; set; } = null!;

    public virtual ICollection<ApprovalItem> ApprovalItems { get; set; } = new List<ApprovalItem>();

    public virtual Property? SourceProperty { get; set; }

    public virtual SystemCode? SystemCode { get; set; }
}
