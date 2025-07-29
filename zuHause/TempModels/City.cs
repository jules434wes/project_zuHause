using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 縣市代碼表
/// </summary>
[Table("cities")]
[Index("CityCode", Name = "UQ_cities_cityCode", IsUnique = true)]
public partial class City
{
    /// <summary>
    /// 縣市代碼
    /// </summary>
    [Column("cityCode")]
    [StringLength(10)]
    [Unicode(false)]
    public string CityCode { get; set; } = null!;

    /// <summary>
    /// 縣市名稱
    /// </summary>
    [Column("cityName")]
    [StringLength(50)]
    public string CityName { get; set; } = null!;

    /// <summary>
    /// 排序
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

    [Key]
    [Column("cityId")]
    public int CityId { get; set; }

    [InverseProperty("City")]
    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    [InverseProperty("PrimaryRentalCity")]
    public virtual ICollection<Member> MemberPrimaryRentalCities { get; set; } = new List<Member>();

    [InverseProperty("ResidenceCity")]
    public virtual ICollection<Member> MemberResidenceCities { get; set; } = new List<Member>();
}
