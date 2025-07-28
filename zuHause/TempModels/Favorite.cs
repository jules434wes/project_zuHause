using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 收藏表
/// </summary>
[PrimaryKey("MemberId", "PropertyId")]
[Table("favorites")]
[Index("PropertyId", Name = "IX_favorites_property")]
public partial class Favorite
{
    /// <summary>
    /// 會員ID
    /// </summary>
    [Key]
    [Column("memberID")]
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    [Key]
    [Column("propertyID")]
    public int PropertyId { get; set; }

    /// <summary>
    /// 是否有效
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 收藏時間
    /// </summary>
    [Column("favoritedAt")]
    [Precision(0)]
    public DateTime FavoritedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("Favorites")]
    public virtual Member Member { get; set; } = null!;

    [ForeignKey("PropertyId")]
    [InverseProperty("Favorites")]
    public virtual Property Property { get; set; } = null!;
}
