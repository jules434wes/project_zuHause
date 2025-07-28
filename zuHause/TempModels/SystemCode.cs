using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 代碼總表
/// </summary>
[PrimaryKey("CodeCategory", "Code")]
[Table("systemCodes")]
public partial class SystemCode
{
    /// <summary>
    /// 代碼類別
    /// </summary>
    [Key]
    [Column("codeCategory")]
    [StringLength(20)]
    public string CodeCategory { get; set; } = null!;

    /// <summary>
    /// 代碼
    /// </summary>
    [Key]
    [Column("code")]
    [StringLength(20)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 代碼名稱
    /// </summary>
    [Column("codeName")]
    [StringLength(50)]
    public string CodeName { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    [Column("displayOrder")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

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

    [InverseProperty("SystemCode")]
    public virtual ICollection<ApprovalItem> ApprovalItems { get; set; } = new List<ApprovalItem>();

    [InverseProperty("SystemCode")]
    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    [ForeignKey("CodeCategory")]
    [InverseProperty("SystemCodes")]
    public virtual SystemCodeCategory CodeCategoryNavigation { get; set; } = null!;
}
