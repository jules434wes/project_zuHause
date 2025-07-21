using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zuHause.Models;

/// <summary>
/// 網站訊息表
/// </summary>
public partial class SiteMessage
{
    /// <summary>
    /// 訊息ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SiteMessagesId { get; set; }

    /// <summary>
    /// 標題
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 內文
    /// </summary>
    public string SiteMessageContent { get; set; } = null!;

    /// <summary>
    /// 分類
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// 模組
    /// </summary>
    public string ModuleScope { get; set; } = null!;

    /// <summary>
    /// 訊息類型
    /// </summary>
    public string MessageType { get; set; } = null!;

    /// <summary>
    /// 顯示順序
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 開始時間
    /// </summary>
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// 結束時間
    /// </summary>
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 圖片/附件URL
    /// </summary>
    public string? AttachmentUrl { get; set; }

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

    public virtual ICollection<MessagePlacement> MessagePlacements { get; set; } = new List<MessagePlacement>();
}
