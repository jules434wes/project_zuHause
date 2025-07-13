using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 家具商品資料表
/// </summary>
public partial class FurnitureProduct
{
    /// <summary>
    /// 商品ID
    /// </summary>
    public string FurnitureProductId { get; set; } = null!;

    /// <summary>
    /// 分類ID
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// 商品描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 原價
    /// </summary>
    public decimal ListPrice { get; set; }

    /// <summary>
    /// 每日租金
    /// </summary>
    public decimal DailyRental { get; set; }

    /// <summary>
    /// 商品圖片URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 上下架狀態
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// 上架時間
    /// </summary>
    public DateOnly? ListedAt { get; set; }

    /// <summary>
    /// 下架時間
    /// </summary>
    public DateOnly? DelistedAt { get; set; }

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

    public virtual FurnitureCategory? Category { get; set; }

    public virtual ICollection<FurnitureCartItem> FurnitureCartItems { get; set; } = new List<FurnitureCartItem>();

    public virtual ICollection<FurnitureInventory> FurnitureInventories { get; set; } = new List<FurnitureInventory>();

    public virtual ICollection<FurnitureOrderHistory> FurnitureOrderHistories { get; set; } = new List<FurnitureOrderHistory>();

    public virtual ICollection<FurnitureOrderItem> FurnitureOrderItems { get; set; } = new List<FurnitureOrderItem>();

    public virtual ICollection<InventoryEvent> InventoryEvents { get; set; } = new List<InventoryEvent>();
}
