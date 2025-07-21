using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 家具訂單查詢表
/// </summary>
public partial class FurnitureOrder
{
    /// <summary>
    /// 訂單ID
    /// </summary>
    public string FurnitureOrderId { get; set; } = null!;

    /// <summary>
    /// 會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    public int? PropertyId { get; set; }

    /// <summary>
    /// 成立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 總金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 訂單狀態
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 付款狀態
    /// </summary>
    public string PaymentStatus { get; set; } = null!;

    /// <summary>
    /// 合約連結
    /// </summary>
    public string? ContractLink { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<CustomerServiceTicket> CustomerServiceTickets { get; set; } = new List<CustomerServiceTicket>();

    public virtual ICollection<FurnitureOrderHistory> FurnitureOrderHistories { get; set; } = new List<FurnitureOrderHistory>();

    public virtual ICollection<FurnitureOrderItem> FurnitureOrderItems { get; set; } = new List<FurnitureOrderItem>();

    public virtual ICollection<FurnitureRentalContract> FurnitureRentalContracts { get; set; } = new List<FurnitureRentalContract>();

    public virtual Member Member { get; set; } = null!;

    public virtual ICollection<OrderEvent> OrderEvents { get; set; } = new List<OrderEvent>();

    public virtual Property? Property { get; set; }

   
}
