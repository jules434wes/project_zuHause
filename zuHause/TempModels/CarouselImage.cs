using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 輪播圖片表
/// </summary>
[Table("carouselImages")]
[Index("StartAt", "EndAt", Name = "IX_carouselImages_active_time")]
public partial class CarouselImage
{
    /// <summary>
    /// 圖片ID
    /// </summary>
    [Key]
    [Column("carouselImageId")]
    public int CarouselImageId { get; set; }

    /// <summary>
    /// 名稱
    /// </summary>
    [Column("imagesName")]
    [StringLength(50)]
    public string ImagesName { get; set; } = null!;

    /// <summary>
    /// 分類
    /// </summary>
    [Column("category")]
    [StringLength(20)]
    [Unicode(false)]
    public string Category { get; set; } = null!;

    /// <summary>
    /// 圖片URL
    /// </summary>
    [Column("imageUrl")]
    [StringLength(255)]
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// 頁面識別碼
    /// </summary>
    [Column("pageCode")]
    [StringLength(50)]
    [Unicode(false)]
    public string? PageCode { get; set; }

    /// <summary>
    /// 顯示順序
    /// </summary>
    [Column("displayOrder")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 開始時間
    /// </summary>
    [Column("startAt")]
    [Precision(0)]
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// 結束時間
    /// </summary>
    [Column("endAt")]
    [Precision(0)]
    public DateTime? EndAt { get; set; }

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

    /// <summary>
    /// 刪除時間
    /// </summary>
    [Column("deletedAt")]
    [Precision(0)]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 圖片超連結
    /// </summary>
    [Column("webUrl")]
    [StringLength(255)]
    public string? WebUrl { get; set; }

    [ForeignKey("PageCode")]
    [InverseProperty("CarouselImages")]
    public virtual Page? PageCodeNavigation { get; set; }
}
