using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 尋租文章資料表
/// </summary>
[Table("renterPosts")]
[Index("CityId", "DistrictId", "HouseType", Name = "IX_renterPosts_location")]
public partial class RenterPost
{
    /// <summary>
    /// 文章ID
    /// </summary>
    [Key]
    [Column("postID")]
    public int PostId { get; set; }

    /// <summary>
    /// 租客會員ID
    /// </summary>
    [Column("memberID")]
    public int MemberId { get; set; }

    /// <summary>
    /// 希望縣市ID
    /// </summary>
    [Column("cityID")]
    public int CityId { get; set; }

    /// <summary>
    /// 希望區域ID
    /// </summary>
    [Column("districtID")]
    public int DistrictId { get; set; }

    /// <summary>
    /// 房屋類型
    /// </summary>
    [Column("houseType")]
    [StringLength(20)]
    public string HouseType { get; set; } = null!;

    /// <summary>
    /// 預算下限
    /// </summary>
    [Column("budgetMin", TypeName = "decimal(10, 2)")]
    public decimal BudgetMin { get; set; }

    /// <summary>
    /// 預算上限
    /// </summary>
    [Column("budgetMax", TypeName = "decimal(10, 2)")]
    public decimal BudgetMax { get; set; }

    /// <summary>
    /// 詳細需求
    /// </summary>
    [Column("postContent")]
    public string? PostContent { get; set; }

    /// <summary>
    /// 瀏覽數
    /// </summary>
    [Column("viewCount")]
    public int ViewCount { get; set; }

    /// <summary>
    /// 回覆數
    /// </summary>
    [Column("replyCount")]
    public int ReplyCount { get; set; }

    /// <summary>
    /// 發布時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後編輯時間
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

    /// <summary>
    /// 是否有效
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("RenterPosts")]
    public virtual Member Member { get; set; } = null!;

    [InverseProperty("Post")]
    public virtual ICollection<RenterPostReply> RenterPostReplies { get; set; } = new List<RenterPostReply>();

    [InverseProperty("Post")]
    public virtual ICollection<RenterRequirementRelation> RenterRequirementRelations { get; set; } = new List<RenterRequirementRelation>();
}
