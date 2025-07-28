using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具購物車表
/// </summary>
[Table("furnitureCarts")]
public partial class FurnitureCart
{
    /// <summary>
    /// 購物車ID
    /// </summary>
    [Key]
    [Column("furnitureCartId")]
    [StringLength(50)]
    [Unicode(false)]
    public string FurnitureCartId { get; set; } = null!;

    /// <summary>
    /// 會員ID
    /// </summary>
    [Column("memberId")]
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    [Column("propertyId")]
    public int? PropertyId { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    [Column("status")]
    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

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

    /// <summary>
    /// 刪除時間
    /// </summary>
    [Column("deletedAt")]
    [Precision(0)]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("Cart")]
    public virtual ICollection<FurnitureCartItem> FurnitureCartItems { get; set; } = new List<FurnitureCartItem>();

    [ForeignKey("MemberId")]
    [InverseProperty("FurnitureCarts")]
    public virtual Member Member { get; set; } = null!;

    [ForeignKey("PropertyId")]
    [InverseProperty("FurnitureCarts")]
    public virtual Property? Property { get; set; }
}
