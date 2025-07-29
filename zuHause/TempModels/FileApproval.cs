using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 檔案審核
/// </summary>
[Table("fileApprovals")]
public partial class FileApproval
{
    /// <summary>
    /// 審核ID
    /// </summary>
    [Key]
    [Column("approvalID")]
    public int ApprovalId { get; set; }

    /// <summary>
    /// 上傳ID
    /// </summary>
    [Column("uploadID")]
    public int UploadId { get; set; }

    /// <summary>
    /// 會員ID
    /// </summary>
    [Column("memberID")]
    public int MemberId { get; set; }

    /// <summary>
    /// 審核狀態代碼
    /// </summary>
    [Column("statusCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 審核說明
    /// </summary>
    [Column("resultDescription")]
    [StringLength(500)]
    public string? ResultDescription { get; set; }

    /// <summary>
    /// 申請時間
    /// </summary>
    [Column("appliedAt")]
    [Precision(0)]
    public DateTime AppliedAt { get; set; }

    /// <summary>
    /// 審核時間
    /// </summary>
    [Column("reviewedAt")]
    [Precision(0)]
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// 審核人員ID
    /// </summary>
    [Column("reviewerAdminID")]
    public int? ReviewerAdminId { get; set; }

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

    [ForeignKey("UploadId")]
    [InverseProperty("FileApprovals")]
    public virtual UserUpload Upload { get; set; } = null!;
}
