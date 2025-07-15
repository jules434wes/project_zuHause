using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 客服聯繫表
/// </summary>
public partial class CustomerServiceTicket
{
    /// <summary>
    /// 客服單ID (自動遞增，從201開始)
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 使用者ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    public int? PropertyId { get; set; }

    /// <summary>
    /// 租約ID
    /// </summary>
    public int? ContractId { get; set; }

    /// <summary>
    /// 家具訂單ID
    /// </summary>
    public string? FurnitureOrderId { get; set; }

    /// <summary>
    /// 主旨
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// 主分類代碼
    /// </summary>
    public string CategoryCode { get; set; } = null!;

    /// <summary>
    /// 需求內容
    /// </summary>
    public string TicketContent { get; set; } = null!;

    /// <summary>
    /// 客服最後回覆
    /// </summary>
    public string? ReplyContent { get; set; }

    /// <summary>
    /// 狀態代碼
    /// </summary>
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後回覆時間
    /// </summary>
    public DateTime? ReplyAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 客服人員ID
    /// </summary>
    public int? HandledBy { get; set; }

    /// <summary>
    /// 是否結案
    /// </summary>
    public bool IsResolved { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual FurnitureOrder? FurnitureOrder { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual Property? Property { get; set; }
}
