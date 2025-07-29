using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 縣市代碼表
/// </summary>
public partial class City
{
    /// <summary>
    /// 縣市代碼
    /// </summary>
    public string CityCode { get; set; } = null!;

    /// <summary>
    /// 縣市名稱
    /// </summary>
    public string CityName { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int DisplayOrder { get; set; }

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

    public int CityId { get; set; }

    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    public virtual ICollection<Member> MemberPrimaryRentalCities { get; set; } = new List<Member>();

    public virtual ICollection<Member> MemberResidenceCities { get; set; } = new List<Member>();

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
