using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 收藏表
/// </summary>
public partial class Favorite
{
    /// <summary>
    /// 會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    public int PropertyId { get; set; }

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 收藏時間
    /// </summary>
    public DateTime FavoritedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual Property Property { get; set; } = null!;
}
