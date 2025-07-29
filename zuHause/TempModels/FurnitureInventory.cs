using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具庫存表
/// </summary>
[Table("furnitureInventory")]
[Index("ProductId", "AvailableQuantity", Name = "IX_furnInventory_available")]
public partial class FurnitureInventory
{
    /// <summary>
    /// 庫存ID
    /// </summary>
    [Key]
    [Column("furnitureInventoryId")]
    [StringLength(50)]
    [Unicode(false)]
    public string FurnitureInventoryId { get; set; } = null!;

    /// <summary>
    /// 商品ID
    /// </summary>
    [Column("productId")]
    [StringLength(50)]
    [Unicode(false)]
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 總庫存數量
    /// </summary>
    [Column("totalQuantity")]
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 已出租數量
    /// </summary>
    [Column("rentedQuantity")]
    public int RentedQuantity { get; set; }

    /// <summary>
    /// 可用庫存
    /// </summary>
    [Column("availableQuantity")]
    public int AvailableQuantity { get; set; }

    /// <summary>
    /// 安全庫存
    /// </summary>
    [Column("safetyStock")]
    public int SafetyStock { get; set; }

    /// <summary>
    /// 最後更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("FurnitureInventories")]
    public virtual FurnitureProduct Product { get; set; } = null!;
}
