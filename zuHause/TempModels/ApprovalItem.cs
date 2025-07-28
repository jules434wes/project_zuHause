using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 審核明細
/// </summary>
[Table("approvalItems")]
[Index("ActionBy", Name = "IX_approvalItems_actionBy")]
[Index("ActionType", Name = "IX_approvalItems_actionType")]
[Index("ActionCategory", "ActionType", Name = "IX_approvalItems_action_category")]
[Index("ApprovalId", Name = "IX_approvalItems_approvalID")]
[Index("CreatedAt", Name = "IX_approvalItems_createdAt")]
public partial class ApprovalItem
{
    /// <summary>
    /// 審核明細ID (自動遞增，從801開始)
    /// </summary>
    [Key]
    [Column("approvalItemID")]
    public int ApprovalItemId { get; set; }

    /// <summary>
    /// 審核ID
    /// </summary>
    [Column("approvalID")]
    public int ApprovalId { get; set; }

    /// <summary>
    /// 操作者ID
    /// </summary>
    [Column("actionBy")]
    public int? ActionBy { get; set; }

    /// <summary>
    /// 操作類型
    /// </summary>
    [Column("actionType")]
    [StringLength(20)]
    public string ActionType { get; set; } = null!;

    /// <summary>
    /// 內部操作備註
    /// </summary>
    [Column("actionNote")]
    public string? ActionNote { get; set; }

    /// <summary>
    /// 審核快照JSON
    /// </summary>
    [Column("snapshotJSON")]
    public string? SnapshotJson { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 操作類別 (計算欄位)
    /// </summary>
    [Column("actionCategory")]
    [StringLength(20)]
    public string? ActionCategory { get; set; }

    [ForeignKey("ApprovalId")]
    [InverseProperty("ApprovalItems")]
    public virtual Approval Approval { get; set; } = null!;

    [ForeignKey("ActionCategory, ActionType")]
    [InverseProperty("ApprovalItems")]
    public virtual SystemCode? SystemCode { get; set; }
}
