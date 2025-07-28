using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 家具歷史訂單清單
/// </summary>
public partial class FurnitureOrderHistory
{
    /// <summary>
    /// 流水號
    /// </summary>
    public string FurnitureOrderHistoryId { get; set; } = null!;

    /// <summary>
    /// 訂單ID
    /// </summary>
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 商品ID
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 商品名稱快照
    /// </summary>
    public string ProductNameSnapshot { get; set; } = null!;

    /// <summary>
    /// 商品描述快照
    /// </summary>
    public string? DescriptionSnapshot { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 單日租金快照
    /// </summary>
    public decimal DailyRentalSnapshot { get; set; }

    /// <summary>
    /// 租借開始日
    /// </summary>
    public DateOnly RentalStart { get; set; }

    /// <summary>
    /// 租借結束日
    /// </summary>
    public DateOnly RentalEnd { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// 明細狀態
    /// </summary>
    public string ItemStatus { get; set; } = null!;

    /// <summary>
    /// 實際歸還日期
    /// </summary>
    public DateOnly? ReturnedAt { get; set; }

    /// <summary>
    /// 損壞說明
    /// </summary>
    public string? DamageNote { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual FurnitureOrder Order { get; set; } = null!;

    public virtual FurnitureProduct Product { get; set; } = null!;
}
