using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 系統訊息表
/// </summary>
public partial class SystemMessage
{
    /// <summary>
    /// 系統訊息ID (自動遞增，從401開始)
    /// </summary>
    public int MessageId { get; set; }

    /// <summary>
    /// 訊息類別代碼
    /// </summary>
    public string CategoryCode { get; set; } = null!;

    /// <summary>
    /// 接受者類型代碼
    /// </summary>
    public string AudienceTypeCode { get; set; } = null!;

    /// <summary>
    /// 個別接受者ID
    /// </summary>
    public int? ReceiverId { get; set; }

    /// <summary>
    /// 發送標題
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 發送內容
    /// </summary>
    public string MessageContent { get; set; } = null!;

    /// <summary>
    /// 附件URL
    /// </summary>
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// 發送時間
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// 發送管理員ID
    /// </summary>
    public int AdminId { get; set; }

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

    /// <summary>
    /// 刪除時間
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual Admin Admin { get; set; } = null!;

    public virtual Member? Receiver { get; set; }
}
