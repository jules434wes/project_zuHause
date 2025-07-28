using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 訊息位置
/// </summary>
[PrimaryKey("PageCode", "SectionCode")]
[Table("messagePlacements")]
public partial class MessagePlacement
{
    /// <summary>
    /// 頁面識別碼
    /// </summary>
    [Key]
    [Column("pageCode")]
    [StringLength(50)]
    [Unicode(false)]
    public string PageCode { get; set; } = null!;

    /// <summary>
    /// 區段代碼
    /// </summary>
    [Key]
    [Column("sectionCode")]
    [StringLength(50)]
    [Unicode(false)]
    public string SectionCode { get; set; } = null!;

    /// <summary>
    /// 訊息ID
    /// </summary>
    [Column("messageID")]
    public int MessageId { get; set; }

    /// <summary>
    /// 小標題
    /// </summary>
    [Column("subtitle")]
    [StringLength(100)]
    public string? Subtitle { get; set; }

    /// <summary>
    /// 顯示順序
    /// </summary>
    [Column("displayOrder")]
    public int DisplayOrder { get; set; }

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

    [ForeignKey("MessageId")]
    [InverseProperty("MessagePlacements")]
    public virtual SiteMessage Message { get; set; } = null!;
}
