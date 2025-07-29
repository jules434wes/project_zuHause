using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 合約內容家具表
/// </summary>
[Table("contractFurnitureItems")]
public partial class ContractFurnitureItem
{
    /// <summary>
    /// 家具清單ID
    /// </summary>
    [Key]
    [Column("contractFurnitureItemId")]
    public int ContractFurnitureItemId { get; set; }

    /// <summary>
    /// 合約ID
    /// </summary>
    [Column("contractId")]
    public int ContractId { get; set; }

    /// <summary>
    /// 家具名稱
    /// </summary>
    [Column("furnitureName")]
    [StringLength(50)]
    public string FurnitureName { get; set; } = null!;

    /// <summary>
    /// 家具狀況
    /// </summary>
    [Column("furnitureCondition")]
    [StringLength(30)]
    public string? FurnitureCondition { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 修繕費負責人
    /// </summary>
    [Column("repairChargeOwner")]
    [StringLength(20)]
    public string? RepairChargeOwner { get; set; }

    /// <summary>
    /// 維修權責
    /// </summary>
    [Column("repairResponsibility")]
    [StringLength(20)]
    public string? RepairResponsibility { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    [Column("unitPrice")]
    public int? UnitPrice { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    [Column("amount")]
    public int? Amount { get; set; }

    [ForeignKey("ContractId")]
    [InverseProperty("ContractFurnitureItems")]
    public virtual Contract Contract { get; set; } = null!;
}
