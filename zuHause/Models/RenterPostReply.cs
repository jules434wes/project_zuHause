using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 尋租文章回覆資料表
/// </summary>
public partial class RenterPostReply
{
    /// <summary>
    /// 回覆ID
    /// </summary>
    public int ReplyId { get; set; }

    /// <summary>
    /// 文章ID
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// 房東會員ID
    /// </summary>
    public int LandlordMemberId { get; set; }

    /// <summary>
    /// 回覆內容
    /// </summary>
    public string? ReplyContent { get; set; }

    /// <summary>
    /// 推薦房源ID
    /// </summary>
    public int? SuggestPropertyId { get; set; }

    /// <summary>
    /// 符合預算
    /// </summary>
    public bool IsWithinBudget { get; set; }

    /// <summary>
    /// 符合條件標籤
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 回覆時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual Member LandlordMember { get; set; } = null!;

    public virtual RenterPost Post { get; set; } = null!;
}
