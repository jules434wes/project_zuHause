using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace zuHause.Models;

/// <summary>
/// 家具購物車表
/// </summary>
public partial class FurnitureCart
{
    /// <summary>
    /// 購物車ID
    /// </summary>
    public string FurnitureCartId { get; set; } = null!;

    /// <summary>
    /// 會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    public int? PropertyId { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    //總金額
    [NotMapped]
    public decimal TotalAmount { get; set; }

    public virtual ICollection<FurnitureCartItem> FurnitureCartItems { get; set; } = new List<FurnitureCartItem>();

    public virtual Member Member { get; set; } = null!;

    public virtual Property? Property { get; set; }
}
