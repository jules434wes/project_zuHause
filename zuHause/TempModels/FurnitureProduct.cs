using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具商品資料表
/// </summary>
[Table("furnitureProducts")]
[Index("Status", "CategoryId", Name = "IX_furnProducts_status_cat")]
public partial class FurnitureProduct
{
    /// <summary>
    /// 商品ID
    /// </summary>
    [Key]
    [Column("furnitureProductId")]
    [StringLength(50)]
    [Unicode(false)]
    public string FurnitureProductId { get; set; } = null!;

    /// <summary>
    /// 分類ID
    /// </summary>
    [Column("categoryId")]
    [StringLength(50)]
    [Unicode(false)]
    public string? CategoryId { get; set; }

    /// <summary>
    /// 商品名稱
    /// </summary>
    [Column("productName")]
    [StringLength(100)]
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// 商品描述
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 原價
    /// </summary>
    [Column("listPrice", TypeName = "decimal(10, 0)")]
    public decimal ListPrice { get; set; }

    /// <summary>
    /// 每日租金
    /// </summary>
    [Column("dailyRental", TypeName = "decimal(10, 0)")]
    public decimal DailyRental { get; set; }

    /// <summary>
    /// 商品圖片URL
    /// </summary>
    [Column("imageUrl")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 上下架狀態
    /// </summary>
    [Column("status")]
    public bool Status { get; set; }

    /// <summary>
    /// 上架時間
    /// </summary>
    [Column("listedAt")]
    public DateOnly? ListedAt { get; set; }

    /// <summary>
    /// 下架時間
    /// </summary>
    [Column("delistedAt")]
    public DateOnly? DelistedAt { get; set; }

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

    [ForeignKey("CategoryId")]
    [InverseProperty("FurnitureProducts")]
    public virtual FurnitureCategory? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<FurnitureCartItem> FurnitureCartItems { get; set; } = new List<FurnitureCartItem>();

    [InverseProperty("Product")]
    public virtual ICollection<FurnitureInventory> FurnitureInventories { get; set; } = new List<FurnitureInventory>();

    [InverseProperty("Product")]
    public virtual ICollection<FurnitureOrderHistory> FurnitureOrderHistories { get; set; } = new List<FurnitureOrderHistory>();

    [InverseProperty("Product")]
    public virtual ICollection<FurnitureOrderItem> FurnitureOrderItems { get; set; } = new List<FurnitureOrderItem>();

    [InverseProperty("Product")]
    public virtual ICollection<InventoryEvent> InventoryEvents { get; set; } = new List<InventoryEvent>();
}
