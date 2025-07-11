using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 聊天室訊息
/// </summary>
public partial class ChatroomMessage
{
    /// <summary>
    /// 訊息ID
    /// </summary>
    public int MessageId { get; set; }

    /// <summary>
    /// 聊天室ID
    /// </summary>
    public int ChatroomId { get; set; }

    /// <summary>
    /// 發送者會員ID
    /// </summary>
    public int SenderMemberId { get; set; }

    /// <summary>
    /// 內容
    /// </summary>
    public string MessageContent { get; set; } = null!;

    /// <summary>
    /// 是否已讀
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// 傳送時間
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// 已讀時間
    /// </summary>
    public DateTime? ReadAt { get; set; }

    public virtual Chatroom Chatroom { get; set; } = null!;

    public virtual Member SenderMember { get; set; } = null!;
}
