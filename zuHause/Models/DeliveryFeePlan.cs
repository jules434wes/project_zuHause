using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 家具配送費方案表
/// </summary>
public partial class DeliveryFeePlan
{
    /// <summary>
    /// 配送方案ID
    /// </summary>
    public int PlanId { get; set; }

    /// <summary>
    /// 方案名稱
    /// </summary>
    public string PlanName { get; set; } = null!;

    /// <summary>
    /// 基本費用
    /// </summary>
    public decimal BaseFee { get; set; }

    /// <summary>
    /// 偏遠加收
    /// </summary>
    public decimal RemoteAreaSurcharge { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    public string CurrencyCode { get; set; } = null!;

    /// <summary>
    /// 生效時間
    /// </summary>
    public DateTime StartAt { get; set; }

    /// <summary>
    /// 結束時間
    /// </summary>
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 重量上限KG
    /// </summary>
    public decimal? MaxWeightKg { get; set; }

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
}
