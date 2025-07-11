using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 使用者通知資料表
/// </summary>
public partial class UserNotification
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public int NotificationId { get; set; }

    /// <summary>
    /// 接收者ID
    /// </summary>
    public int ReceiverId { get; set; }

    /// <summary>
    /// 發送者ID
    /// </summary>
    public int? SenderId { get; set; }

    /// <summary>
    /// 通知類型代碼
    /// </summary>
    public string TypeCode { get; set; } = null!;

    /// <summary>
    /// 訊息標題
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 訊息內容
    /// </summary>
    public string NotificationContent { get; set; } = null!;

    /// <summary>
    /// 相關連結URL
    /// </summary>
    public string? LinkUrl { get; set; }

    /// <summary>
    /// 來源模組代碼
    /// </summary>
    public string? ModuleCode { get; set; }

    /// <summary>
    /// 來源資料ID
    /// </summary>
    public int? SourceEntityId { get; set; }

    /// <summary>
    /// 狀態代碼
    /// </summary>
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 是否已讀
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// 閱讀時間
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 發送時間
    /// </summary>
    public DateTime SentAt { get; set; }

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

    public virtual Member Receiver { get; set; } = null!;
}
