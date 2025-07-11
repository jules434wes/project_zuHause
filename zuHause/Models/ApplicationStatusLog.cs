using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 申請狀態歷程表
/// </summary>
public partial class ApplicationStatusLog
{
    /// <summary>
    /// 狀態歷程ID
    /// </summary>
    public int StatusLogId { get; set; }

    /// <summary>
    /// 申請ID
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// 狀態代碼
    /// </summary>
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 進入狀態時間
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual RentalApplication Application { get; set; } = null!;
}
