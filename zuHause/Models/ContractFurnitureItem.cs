using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 合約內容家具表
/// </summary>
public partial class ContractFurnitureItem
{
    /// <summary>
    /// 家具清單ID
    /// </summary>
    public int ContractFurnitureItemId { get; set; }

    /// <summary>
    /// 合約ID
    /// </summary>
    public int ContractId { get; set; }

    /// <summary>
    /// 家具名稱
    /// </summary>
    public string FurnitureName { get; set; } = null!;

    /// <summary>
    /// 家具狀況
    /// </summary>
    public string? FurnitureCondition { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 修繕費負責人
    /// </summary>
    public string? RepairChargeOwner { get; set; }

    /// <summary>
    /// 維修權責
    /// </summary>
    public string? RepairResponsibility { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public int? UnitPrice { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    public int? Amount { get; set; }

    public virtual Contract Contract { get; set; } = null!;
}
