using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 訊息位置
/// </summary>
public partial class MessagePlacement
{
    /// <summary>
    /// 頁面識別碼
    /// </summary>
    public string PageCode { get; set; } = null!;

    /// <summary>
    /// 區段代碼
    /// </summary>
    public string SectionCode { get; set; } = null!;

    /// <summary>
    /// 訊息ID
    /// </summary>
    public int MessageId { get; set; }

    /// <summary>
    /// 小標題
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// 顯示順序
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual SiteMessage Message { get; set; } = null!;
}
