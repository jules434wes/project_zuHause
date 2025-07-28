using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 系統訊息表
/// </summary>
[Table("systemMessages")]
[Index("AdminId", Name = "IX_systemMessages_adminID")]
[Index("AudienceTypeCode", Name = "IX_systemMessages_audienceTypeCode")]
[Index("DeletedAt", Name = "IX_systemMessages_deletedAt")]
[Index("ReceiverId", Name = "IX_systemMessages_receiverID")]
[Index("SentAt", Name = "IX_systemMessages_sentAt")]
public partial class SystemMessage
{
    /// <summary>
    /// 系統訊息ID (自動遞增，從401開始)
    /// </summary>
    [Key]
    [Column("messageID")]
    public int MessageId { get; set; }

    /// <summary>
    /// 訊息類別代碼
    /// </summary>
    [Column("categoryCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string CategoryCode { get; set; } = null!;

    /// <summary>
    /// 接受者類型代碼
    /// </summary>
    [Column("audienceTypeCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string AudienceTypeCode { get; set; } = null!;

    /// <summary>
    /// 個別接受者ID
    /// </summary>
    [Column("receiverID")]
    public int? ReceiverId { get; set; }

    /// <summary>
    /// 發送標題
    /// </summary>
    [Column("title")]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 發送內容
    /// </summary>
    [Column("messageContent")]
    public string MessageContent { get; set; } = null!;

    /// <summary>
    /// 附件URL
    /// </summary>
    [Column("attachmentUrl")]
    [StringLength(255)]
    [Unicode(false)]
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// 發送時間
    /// </summary>
    [Column("sentAt")]
    [Precision(0)]
    public DateTime SentAt { get; set; }

    /// <summary>
    /// 發送管理員ID
    /// </summary>
    [Column("adminID")]
    public int AdminId { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

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

    [ForeignKey("AdminId")]
    [InverseProperty("SystemMessages")]
    public virtual Admin Admin { get; set; } = null!;

    [ForeignKey("ReceiverId")]
    [InverseProperty("SystemMessages")]
    public virtual Member? Receiver { get; set; }
}
