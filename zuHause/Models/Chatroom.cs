using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 聊天室
/// </summary>
public partial class Chatroom
{
    /// <summary>
    /// 聊天室ID
    /// </summary>
    public int ChatroomId { get; set; }

    /// <summary>
    /// 發起人會員ID
    /// </summary>
    public int InitiatorMemberId { get; set; }

    /// <summary>
    /// 參與者會員ID
    /// </summary>
    public int ParticipantMemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    public int? PropertyId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後訊息時間
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ChatroomMessage> ChatroomMessages { get; set; } = new List<ChatroomMessage>();

    public virtual Member InitiatorMember { get; set; } = null!;

    public virtual Member ParticipantMember { get; set; } = null!;

    public virtual Property? Property { get; set; }
}
