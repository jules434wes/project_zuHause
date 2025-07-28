using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 聊天室
/// </summary>
[Table("chatrooms")]
[Index("LastMessageAt", Name = "IX_chatrooms_lastmsg")]
[Index("InitiatorMemberId", "ParticipantMemberId", Name = "UQ_chatrooms_members", IsUnique = true)]
public partial class Chatroom
{
    /// <summary>
    /// 聊天室ID
    /// </summary>
    [Key]
    [Column("chatroomID")]
    public int ChatroomId { get; set; }

    /// <summary>
    /// 發起人會員ID
    /// </summary>
    [Column("initiatorMemberID")]
    public int InitiatorMemberId { get; set; }

    /// <summary>
    /// 參與者會員ID
    /// </summary>
    [Column("participantMemberID")]
    public int ParticipantMemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    [Column("propertyID")]
    public int? PropertyId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後訊息時間
    /// </summary>
    [Column("lastMessageAt")]
    [Precision(0)]
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("Chatroom")]
    public virtual ICollection<ChatroomMessage> ChatroomMessages { get; set; } = new List<ChatroomMessage>();

    [ForeignKey("InitiatorMemberId")]
    [InverseProperty("ChatroomInitiatorMembers")]
    public virtual Member InitiatorMember { get; set; } = null!;

    [ForeignKey("ParticipantMemberId")]
    [InverseProperty("ChatroomParticipantMembers")]
    public virtual Member ParticipantMember { get; set; } = null!;

    [ForeignKey("PropertyId")]
    [InverseProperty("Chatrooms")]
    public virtual Property? Property { get; set; }
}
