using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 鄉鎮區代碼表
/// </summary>
[Table("districts")]
[Index("CityId", "DistrictCode", Name = "UQ_districts_city_districtCode", IsUnique = true)]
public partial class District
{
    /// <summary>
    /// 區代碼
    /// </summary>
    [Column("districtCode")]
    [StringLength(10)]
    [Unicode(false)]
    public string DistrictCode { get; set; } = null!;

    /// <summary>
    /// 區名稱
    /// </summary>
    [Column("districtName")]
    [StringLength(50)]
    public string DistrictName { get; set; } = null!;

    /// <summary>
    /// 縣市代碼
    /// </summary>
    [Column("cityCode")]
    [StringLength(10)]
    [Unicode(false)]
    public string CityCode { get; set; } = null!;

    /// <summary>
    /// 郵遞區號
    /// </summary>
    [Column("zipCode")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZipCode { get; set; } = null!;

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
    /// 排序
    /// </summary>
    [Column("displayOrder")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 鄉鎮區ID
    /// </summary>
    [Key]
    [Column("districtId")]
    public int DistrictId { get; set; }

    /// <summary>
    /// 縣市ID
    /// </summary>
    [Column("cityId")]
    public int CityId { get; set; }

    [ForeignKey("CityId")]
    [InverseProperty("Districts")]
    public virtual City City { get; set; } = null!;

    [InverseProperty("PrimaryRentalDistrict")]
    public virtual ICollection<Member> MemberPrimaryRentalDistricts { get; set; } = new List<Member>();

    [InverseProperty("ResidenceDistrict")]
    public virtual ICollection<Member> MemberResidenceDistricts { get; set; } = new List<Member>();
}
