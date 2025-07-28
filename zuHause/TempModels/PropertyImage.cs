using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 房源圖片表
/// </summary>
[Table("propertyImages")]
[Index("PropertyId", "DisplayOrder", Name = "UQ_propertyImages_property_order", IsUnique = true)]
public partial class PropertyImage
{
    /// <summary>
    /// 圖片ID
    /// </summary>
    [Key]
    [Column("imageID")]
    public int ImageId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    [Column("propertyID")]
    public int PropertyId { get; set; }

    /// <summary>
    /// 圖片路徑
    /// </summary>
    [Column("imagePath")]
    [StringLength(500)]
    public string ImagePath { get; set; } = null!;

    /// <summary>
    /// 顯示順序
    /// </summary>
    [Column("displayOrder")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("PropertyId")]
    [InverseProperty("PropertyImages")]
    public virtual Property Property { get; set; } = null!;
}
