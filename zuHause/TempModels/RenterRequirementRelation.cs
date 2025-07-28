using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 尋租條件關聯
/// </summary>
[Table("renterRequirementRelations")]
[Index("PostId", "RequirementId", Name = "UQ_rentReqRel_post_req", IsUnique = true)]
public partial class RenterRequirementRelation
{
    /// <summary>
    /// 關聯ID
    /// </summary>
    [Key]
    [Column("relationID")]
    public int RelationId { get; set; }

    /// <summary>
    /// 文章ID
    /// </summary>
    [Column("postID")]
    public int PostId { get; set; }

    /// <summary>
    /// 條件ID
    /// </summary>
    [Column("requirementID")]
    public int RequirementId { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("RenterRequirementRelations")]
    public virtual RenterPost Post { get; set; } = null!;

    [ForeignKey("RequirementId")]
    [InverseProperty("RenterRequirementRelations")]
    public virtual RenterRequirementList Requirement { get; set; } = null!;
}
