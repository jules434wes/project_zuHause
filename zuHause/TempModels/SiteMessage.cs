using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 網站訊息表
/// </summary>
[Table("siteMessages")]
public partial class SiteMessage
{
    [Key]
    [Column("siteMessagesId")]
    public int SiteMessagesId { get; set; }

    /// <summary>
    /// 標題
    /// </summary>
    [Column("title")]
    [StringLength(150)]
    public string? Title { get; set; }

    /// <summary>
    /// 內文
    /// </summary>
    [Column("siteMessageContent")]
    [StringLength(500)]
    public string SiteMessageContent { get; set; } = null!;

    /// <summary>
    /// 分類
    /// </summary>
    [Column("category")]
    [StringLength(12)]
    [Unicode(false)]
    public string Category { get; set; } = null!;

    /// <summary>
    /// 模組
    /// </summary>
    [Column("moduleScope")]
    [StringLength(12)]
    [Unicode(false)]
    public string ModuleScope { get; set; } = null!;

    /// <summary>
    /// 訊息類型
    /// </summary>
    [Column("messageType")]
    [StringLength(20)]
    [Unicode(false)]
    public string MessageType { get; set; } = null!;

    /// <summary>
    /// 顯示順序
    /// </summary>
    [Column("displayOrder")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 開始時間
    /// </summary>
    [Column("startAt")]
    [Precision(0)]
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// 結束時間
    /// </summary>
    [Column("endAt")]
    [Precision(0)]
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 圖片/附件URL
    /// </summary>
    [Column("attachmentUrl")]
    [StringLength(255)]
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    [Column("deletedAt")]
    [Precision(0)]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("Message")]
    public virtual ICollection<MessagePlacement> MessagePlacements { get; set; } = new List<MessagePlacement>();
}
