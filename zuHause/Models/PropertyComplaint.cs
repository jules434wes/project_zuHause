using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 房源投訴表
/// </summary>
public partial class PropertyComplaint
{
    /// <summary>
    /// 投訴ID (自動遞增，從301開始)
    /// </summary>
    public int ComplaintId { get; set; }

    /// <summary>
    /// 投訴人ID
    /// </summary>
    public int ComplainantId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    public int PropertyId { get; set; }

    /// <summary>
    /// 被投訴房東ID
    /// </summary>
    public int LandlordId { get; set; }

    /// <summary>
    /// 投訴內容
    /// </summary>
    public string ComplaintContent { get; set; } = null!;

    /// <summary>
    /// 處理狀態代碼
    /// </summary>
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 結案時間
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// 內部註記
    /// </summary>
    public string? InternalNote { get; set; }

    /// <summary>
    /// 處理人員ID
    /// </summary>
    public int? HandledBy { get; set; }

    public virtual Member Complainant { get; set; } = null!;

    public virtual Member Landlord { get; set; } = null!;

    public virtual Property Property { get; set; } = null!;
}
