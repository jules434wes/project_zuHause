using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 尋租文章需求條件清單
/// </summary>
public partial class RenterRequirementList
{
    /// <summary>
    /// 條件ID
    /// </summary>
    public int RequirementId { get; set; }

    /// <summary>
    /// 條件名稱
    /// </summary>
    public string RequirementName { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    public virtual ICollection<RenterRequirementRelation> RenterRequirementRelations { get; set; } = new List<RenterRequirementRelation>();
}
