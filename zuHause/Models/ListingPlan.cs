using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zuHause.Models;

/// <summary>
/// 刊登費方案表
/// </summary>
public partial class ListingPlan
{
    /// <summary>
    /// 方案名稱
    /// </summary>
    public string PlanName { get; set; } = null!;

    /// <summary>
    /// 每日刊登費
    /// </summary>
    public decimal PricePerDay { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    public string CurrencyCode { get; set; } = null!;

    /// <summary>
    /// 最小上架天數
    /// </summary>
    public int MinListingDays { get; set; }

    /// <summary>
    /// 生效時間
    /// </summary>
    public DateTime StartAt { get; set; }

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
    /// 刊登費方案ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PlanId { get; set; }

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
