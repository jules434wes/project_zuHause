using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 家具租賃合約表
/// </summary>
[Table("furnitureRentalContracts")]
public partial class FurnitureRentalContract
{
    /// <summary>
    /// 合約ID
    /// </summary>
    [Key]
    [Column("furnitureRentalContractsId")]
    [StringLength(50)]
    [Unicode(false)]
    public string FurnitureRentalContractsId { get; set; } = null!;

    /// <summary>
    /// 訂單ID
    /// </summary>
    [Column("orderId")]
    [StringLength(50)]
    [Unicode(false)]
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 合約 JSON
    /// </summary>
    [Column("contractJson")]
    public string? ContractJson { get; set; }

    /// <summary>
    /// 簽章連結
    /// </summary>
    [Column("contractLink")]
    [StringLength(255)]
    public string? ContractLink { get; set; }

    /// <summary>
    /// 配送日期
    /// </summary>
    [Column("deliveryDate")]
    public DateOnly? DeliveryDate { get; set; }

    /// <summary>
    /// 退租政策
    /// </summary>
    [Column("terminationPolicy")]
    public string? TerminationPolicy { get; set; }

    /// <summary>
    /// 簽署狀態
    /// </summary>
    [Column("signStatus")]
    [StringLength(20)]
    [Unicode(false)]
    public string? SignStatus { get; set; }

    /// <summary>
    /// 簽署完成時間
    /// </summary>
    [Column("signedAt")]
    [Precision(0)]
    public DateTime? SignedAt { get; set; }

    /// <summary>
    /// 電子簽章值
    /// </summary>
    [Column("eSignatureValue")]
    [StringLength(255)]
    public string? ESignatureValue { get; set; }

    /// <summary>
    /// 簽約日期
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

    [ForeignKey("OrderId")]
    [InverseProperty("FurnitureRentalContracts")]
    public virtual FurnitureOrder Order { get; set; } = null!;
}
