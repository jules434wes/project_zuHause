using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 會員身分表
/// </summary>
public partial class MemberType
{
    /// <summary>
    /// 身份ID
    /// </summary>
    public int MemberTypeId { get; set; }

    /// <summary>
    /// 身分名稱
    /// </summary>
    public string TypeName { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

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

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
