using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 合約備註表
/// </summary>
[Table("contractComments")]
public partial class ContractComment
{
    /// <summary>
    /// 合約備註ID
    /// </summary>
    [Key]
    [Column("contractCommentId")]
    public int ContractCommentId { get; set; }

    /// <summary>
    /// 合約ID
    /// </summary>
    [Column("contractId")]
    public int ContractId { get; set; }

    /// <summary>
    /// 備註類型
    /// </summary>
    [Column("commentType")]
    [StringLength(20)]
    [Unicode(false)]
    public string CommentType { get; set; } = null!;

    /// <summary>
    /// 內容
    /// </summary>
    [Column("commentText")]
    [StringLength(100)]
    public string CommentText { get; set; } = null!;

    /// <summary>
    /// 建立者
    /// </summary>
    [Column("createdById")]
    public int CreatedById { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ContractId")]
    [InverseProperty("ContractComments")]
    public virtual Contract Contract { get; set; } = null!;
}
