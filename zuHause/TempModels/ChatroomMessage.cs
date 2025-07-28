using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 聊天室訊息
/// </summary>
[Table("chatroomMessages")]
[Index("ChatroomId", "SentAt", Name = "IX_chatroomMessages_chat_time")]
public partial class ChatroomMessage
{
    /// <summary>
    /// 訊息ID
    /// </summary>
    [Key]
    [Column("messageID")]
    public int MessageId { get; set; }

    /// <summary>
    /// 聊天室ID
    /// </summary>
    [Column("chatroomID")]
    public int ChatroomId { get; set; }

    /// <summary>
    /// 發送者會員ID
    /// </summary>
    [Column("senderMemberID")]
    public int SenderMemberId { get; set; }

    /// <summary>
    /// 內容
    /// </summary>
    [Column("messageContent")]
    [StringLength(1000)]
    public string MessageContent { get; set; } = null!;

    /// <summary>
    /// 是否已讀
    /// </summary>
    [Column("isRead")]
    public bool IsRead { get; set; }

    /// <summary>
    /// 傳送時間
    /// </summary>
    [Column("sentAt")]
    [Precision(0)]
    public DateTime SentAt { get; set; }

    /// <summary>
    /// 已讀時間
    /// </summary>
    [Column("readAt")]
    [Precision(0)]
    public DateTime? ReadAt { get; set; }

    [ForeignKey("ChatroomId")]
    [InverseProperty("ChatroomMessages")]
    public virtual Chatroom Chatroom { get; set; } = null!;

    [ForeignKey("SenderMemberId")]
    [InverseProperty("ChatroomMessages")]
    public virtual Member SenderMember { get; set; } = null!;
}
