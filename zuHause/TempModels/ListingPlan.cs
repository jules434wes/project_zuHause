using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 刊登費方案表
/// </summary>
[Table("listingPlans")]
public partial class ListingPlan
{
    /// <summary>
    /// 方案名稱
    /// </summary>
    [Column("planName")]
    [StringLength(50)]
    public string PlanName { get; set; } = null!;

    /// <summary>
    /// 每日刊登費
    /// </summary>
    [Column("pricePerDay", TypeName = "decimal(10, 2)")]
    public decimal PricePerDay { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    [Column("currencyCode")]
    [StringLength(3)]
    [Unicode(false)]
    public string CurrencyCode { get; set; } = null!;

    /// <summary>
    /// 最小上架天數
    /// </summary>
    [Column("minListingDays")]
    public int MinListingDays { get; set; }

    /// <summary>
    /// 生效時間
    /// </summary>
    [Column("startAt")]
    [Precision(0)]
    public DateTime StartAt { get; set; }

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
    /// 刊登費方案ID
    /// </summary>
    [Key]
    [Column("planId")]
    public int PlanId { get; set; }

    [InverseProperty("ListingPlan")]
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
