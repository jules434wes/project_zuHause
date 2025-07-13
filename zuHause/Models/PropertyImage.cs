using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 房源圖片表
/// </summary>
public partial class PropertyImage
{
    /// <summary>
    /// 圖片ID
    /// </summary>
    public int ImageId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    public int PropertyId { get; set; }

    /// <summary>
    /// 圖片路徑
    /// </summary>
    public string ImagePath { get; set; } = null!;

    /// <summary>
    /// 顯示順序
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual Property Property { get; set; } = null!;
}
