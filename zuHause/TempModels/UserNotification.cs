using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 使用者通知資料表
/// </summary>
[Table("userNotifications")]
[Index("ReceiverId", "IsRead", Name = "IX_userNotifications_receiver_isRead")]
public partial class UserNotification
{
    /// <summary>
    /// 通知ID
    /// </summary>
    [Key]
    [Column("notificationID")]
    public int NotificationId { get; set; }

    /// <summary>
    /// 接收者ID
    /// </summary>
    [Column("receiverID")]
    public int ReceiverId { get; set; }

    /// <summary>
    /// 發送者ID
    /// </summary>
    [Column("senderID")]
    public int? SenderId { get; set; }

    /// <summary>
    /// 通知類型代碼
    /// </summary>
    [Column("typeCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string TypeCode { get; set; } = null!;

    /// <summary>
    /// 訊息標題
    /// </summary>
    [Column("title")]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 訊息內容
    /// </summary>
    [Column("notificationContent")]
    public string NotificationContent { get; set; } = null!;

    /// <summary>
    /// 相關連結URL
    /// </summary>
    [Column("linkUrl")]
    [StringLength(255)]
    [Unicode(false)]
    public string? LinkUrl { get; set; }

    /// <summary>
    /// 來源模組代碼
    /// </summary>
    [Column("moduleCode")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ModuleCode { get; set; }

    /// <summary>
    /// 來源資料ID
    /// </summary>
    [Column("sourceEntityID")]
    public int? SourceEntityId { get; set; }

    /// <summary>
    /// 狀態代碼
    /// </summary>
    [Column("statusCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 是否已讀
    /// </summary>
    [Column("isRead")]
    public bool IsRead { get; set; }

    /// <summary>
    /// 閱讀時間
    /// </summary>
    [Column("readAt")]
    [Precision(0)]
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 發送時間
    /// </summary>
    [Column("sentAt")]
    [Precision(0)]
    public DateTime SentAt { get; set; }

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

    [ForeignKey("ReceiverId")]
    [InverseProperty("UserNotifications")]
    public virtual Member Receiver { get; set; } = null!;
}
