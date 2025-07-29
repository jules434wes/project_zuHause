using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 尋租文章需求條件清單
/// </summary>
[Table("renterRequirementList")]
public partial class RenterRequirementList
{
    /// <summary>
    /// 條件ID
    /// </summary>
    [Key]
    [Column("requirementID")]
    public int RequirementId { get; set; }

    /// <summary>
    /// 條件名稱
    /// </summary>
    [Column("requirementName")]
    [StringLength(100)]
    public string RequirementName { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

    [InverseProperty("Requirement")]
    public virtual ICollection<RenterRequirementRelation> RenterRequirementRelations { get; set; } = new List<RenterRequirementRelation>();
}
