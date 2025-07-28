using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具配送費方案表
/// </summary>
[Table("deliveryFeePlans")]
public partial class DeliveryFeePlan
{
    /// <summary>
    /// 配送方案ID
    /// </summary>
    [Key]
    [Column("planId")]
    public int PlanId { get; set; }

    /// <summary>
    /// 方案名稱
    /// </summary>
    [Column("planName")]
    [StringLength(50)]
    public string PlanName { get; set; } = null!;

    /// <summary>
    /// 基本費用
    /// </summary>
    [Column("baseFee", TypeName = "decimal(10, 2)")]
    public decimal BaseFee { get; set; }

    /// <summary>
    /// 偏遠加收
    /// </summary>
    [Column("remoteAreaSurcharge", TypeName = "decimal(10, 2)")]
    public decimal RemoteAreaSurcharge { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    [Column("currencyCode")]
    [StringLength(3)]
    [Unicode(false)]
    public string CurrencyCode { get; set; } = null!;

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
    /// 重量上限KG
    /// </summary>
    [Column("maxWeightKG", TypeName = "decimal(6, 2)")]
    public decimal? MaxWeightKg { get; set; }

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
}
