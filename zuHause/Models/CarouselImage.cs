using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 輪播圖片表
/// </summary>
public partial class CarouselImage
{
    /// <summary>
    /// 圖片ID
    /// </summary>
    public int CarouselImageId { get; set; }

    /// <summary>
    /// 名稱
    /// </summary>
    public string ImagesName { get; set; } = null!;

    /// <summary>
    /// 分類
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// 圖片URL
    /// </summary>
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// 頁面識別碼
    /// </summary>
    public string? PageCode { get; set; }

    /// <summary>
    /// 顯示順序
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 開始時間
    /// </summary>
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// 結束時間
    /// </summary>
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    /// <summary>
    /// 網頁連結
    /// </summary>
    public string? WebUrl { get; set; }

    public virtual Page? PageCodeNavigation { get; set; }
}
