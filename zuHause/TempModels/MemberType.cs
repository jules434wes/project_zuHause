using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 會員身分表
/// </summary>
[Table("memberTypes")]
public partial class MemberType
{
    /// <summary>
    /// 身份ID
    /// </summary>
    [Key]
    [Column("memberTypeID")]
    public int MemberTypeId { get; set; }

    /// <summary>
    /// 身分名稱
    /// </summary>
    [Column("typeName")]
    [StringLength(30)]
    public string TypeName { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    [Column("description")]
    [StringLength(100)]
    public string? Description { get; set; }

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

    [InverseProperty("MemberType")]
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
