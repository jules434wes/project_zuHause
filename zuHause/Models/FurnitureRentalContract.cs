using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 家具租賃合約表
/// </summary>
public partial class FurnitureRentalContract
{
    /// <summary>
    /// 合約ID
    /// </summary>
    public string FurnitureRentalContractsId { get; set; } = null!;

    /// <summary>
    /// 訂單ID
    /// </summary>
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 合約 JSON
    /// </summary>
    public string? ContractJson { get; set; }

    /// <summary>
    /// 簽章連結
    /// </summary>
    public string? ContractLink { get; set; }

    /// <summary>
    /// 配送日期
    /// </summary>
    public DateOnly? DeliveryDate { get; set; }

    /// <summary>
    /// 退租政策
    /// </summary>
    public string? TerminationPolicy { get; set; }

    /// <summary>
    /// 簽署狀態
    /// </summary>
    public string? SignStatus { get; set; }

    /// <summary>
    /// 簽署完成時間
    /// </summary>
    public DateTime? SignedAt { get; set; }

    /// <summary>
    /// 電子簽章值
    /// </summary>
    public string? ESignatureValue { get; set; }

    /// <summary>
    /// 簽約日期
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual FurnitureOrder Order { get; set; } = null!;
}
