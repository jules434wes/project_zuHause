using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 尋租文章回覆資料表
/// </summary>
[Table("renterPostReplies")]
public partial class RenterPostReply
{
    /// <summary>
    /// 回覆ID
    /// </summary>
    [Key]
    [Column("replyID")]
    public int ReplyId { get; set; }

    /// <summary>
    /// 文章ID
    /// </summary>
    [Column("postID")]
    public int PostId { get; set; }

    /// <summary>
    /// 房東會員ID
    /// </summary>
    [Column("landlordMemberID")]
    public int LandlordMemberId { get; set; }

    /// <summary>
    /// 回覆內容
    /// </summary>
    [Column("replyContent")]
    public string? ReplyContent { get; set; }

    /// <summary>
    /// 推薦房源ID
    /// </summary>
    [Column("suggestPropertyID")]
    public int? SuggestPropertyId { get; set; }

    /// <summary>
    /// 符合預算
    /// </summary>
    [Column("isWithinBudget")]
    public bool IsWithinBudget { get; set; }

    /// <summary>
    /// 符合條件標籤
    /// </summary>
    [Column("tags")]
    [StringLength(100)]
    public string? Tags { get; set; }

    /// <summary>
    /// 回覆時間
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

    [ForeignKey("LandlordMemberId")]
    [InverseProperty("RenterPostReplies")]
    public virtual Member LandlordMember { get; set; } = null!;

    [ForeignKey("PostId")]
    [InverseProperty("RenterPostReplies")]
    public virtual RenterPost Post { get; set; } = null!;
}
