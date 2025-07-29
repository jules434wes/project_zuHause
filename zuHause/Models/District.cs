using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 鄉鎮區代碼表
/// </summary>
public partial class District
{
    /// <summary>
    /// 區代碼
    /// </summary>
    public string DistrictCode { get; set; } = null!;

    /// <summary>
    /// 區名稱
    /// </summary>
    public string DistrictName { get; set; } = null!;

    /// <summary>
    /// 縣市代碼
    /// </summary>
    public string CityCode { get; set; } = null!;

    /// <summary>
    /// 郵遞區號
    /// </summary>
    public string ZipCode { get; set; } = null!;

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
    /// 排序
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 鄉鎮區ID
    /// </summary>
    public int DistrictId { get; set; }

    /// <summary>
    /// 縣市ID
    /// </summary>
    public int CityId { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<Member> MemberPrimaryRentalDistricts { get; set; } = new List<Member>();

    public virtual ICollection<Member> MemberResidenceDistricts { get; set; } = new List<Member>();

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
