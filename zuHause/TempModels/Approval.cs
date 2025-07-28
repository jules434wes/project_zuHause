using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 審核主檔
/// </summary>
[Table("approvals")]
[Index("ApplicantMemberId", Name = "IX_approvals_applicantMemberID")]
[Index("CreatedAt", Name = "IX_approvals_createdAt")]
[Index("CurrentApproverId", Name = "IX_approvals_currentApproverID")]
[Index("ModuleCode", Name = "IX_approvals_moduleCode")]
[Index("StatusCode", Name = "IX_approvals_statusCode")]
[Index("StatusCategory", "StatusCode", Name = "IX_approvals_status_category")]
[Index("ModuleCode", "ApplicantMemberId", "SourcePropertyId", Name = "UQ_approvals_member_module", IsUnique = true)]
public partial class Approval
{
    /// <summary>
    /// 審核ID (自動遞增，從701開始)
    /// </summary>
    [Key]
    [Column("approvalID")]
    public int ApprovalId { get; set; }

    /// <summary>
    /// 模組代碼
    /// </summary>
    [Column("moduleCode")]
    [StringLength(20)]
    public string ModuleCode { get; set; } = null!;

    /// <summary>
    /// 審核房源ID
    /// </summary>
    [Column("sourcePropertyID")]
    public int? SourcePropertyId { get; set; }

    /// <summary>
    /// 申請會員ID
    /// </summary>
    [Column("applicantMemberID")]
    public int ApplicantMemberId { get; set; }

    /// <summary>
    /// 審核狀態碼
    /// </summary>
    [Column("statusCode")]
    [StringLength(20)]
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 審核人員ID
    /// </summary>
    [Column("currentApproverID")]
    public int? CurrentApproverId { get; set; }

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

    /// <summary>
    /// 狀態類別 (計算欄位)
    /// </summary>
    [Column("statusCategory")]
    [StringLength(20)]
    public string? StatusCategory { get; set; }

    [ForeignKey("ApplicantMemberId")]
    [InverseProperty("Approvals")]
    public virtual Member ApplicantMember { get; set; } = null!;

    [InverseProperty("Approval")]
    public virtual ICollection<ApprovalItem> ApprovalItems { get; set; } = new List<ApprovalItem>();

    [ForeignKey("SourcePropertyId")]
    [InverseProperty("Approvals")]
    public virtual Property? SourceProperty { get; set; }

    [ForeignKey("StatusCategory, StatusCode")]
    [InverseProperty("Approvals")]
    public virtual SystemCode? SystemCode { get; set; }
}
