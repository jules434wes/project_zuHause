using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 尋租文章資料表
/// </summary>
public partial class RenterPost
{
    /// <summary>
    /// 文章ID
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// 租客會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 希望縣市ID
    /// </summary>
    public int CityId { get; set; }

    /// <summary>
    /// 希望區域ID
    /// </summary>
    public int DistrictId { get; set; }

    /// <summary>
    /// 房屋類型
    /// </summary>
    public string HouseType { get; set; } = null!;

    /// <summary>
    /// 預算下限
    /// </summary>
    public decimal BudgetMin { get; set; }

    /// <summary>
    /// 預算上限
    /// </summary>
    public decimal BudgetMax { get; set; }

    /// <summary>
    /// 詳細需求
    /// </summary>
    public string? PostContent { get; set; }

    /// <summary>
    /// 瀏覽數
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// 回覆數
    /// </summary>
    public int ReplyCount { get; set; }

    /// <summary>
    /// 發布時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後編輯時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsActive { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual ICollection<RenterPostReply> RenterPostReplies { get; set; } = new List<RenterPostReply>();

    public virtual ICollection<RenterRequirementRelation> RenterRequirementRelations { get; set; } = new List<RenterRequirementRelation>();
}
