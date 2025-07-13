using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 尋租條件關聯
/// </summary>
public partial class RenterRequirementRelation
{
    /// <summary>
    /// 關聯ID
    /// </summary>
    public int RelationId { get; set; }

    /// <summary>
    /// 文章ID
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// 條件ID
    /// </summary>
    public int RequirementId { get; set; }

    public virtual RenterPost Post { get; set; } = null!;

    public virtual RenterRequirementList Requirement { get; set; } = null!;
}
