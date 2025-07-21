using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 合約欄位儲存表
/// </summary>
public partial class Contract
{
    /// <summary>
    /// 合約ID
    /// </summary>
    public int ContractId { get; set; }

    /// <summary>
    /// 申請編號ID
    /// </summary>
    public int? RentalApplicationId { get; set; }

    /// <summary>
    /// 合約範本編號
    /// </summary>
    public int? TemplateId { get; set; }

    /// <summary>
    /// 合約起始日
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// 合約結束日
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// 合約狀態
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 管轄法院
    /// </summary>
    public string? CourtJurisdiction { get; set; }

    /// <summary>
    /// 是否可轉租
    /// </summary>
    public bool IsSublettable { get; set; }

    /// <summary>
    /// 使用目的
    /// </summary>
    public string? UsagePurpose { get; set; }

    /// <summary>
    /// 押金金額
    /// </summary>
    public int? DepositAmount { get; set; }

    /// <summary>
    /// 清潔費
    /// </summary>
    public int? CleaningFee { get; set; }

    /// <summary>
    /// 管理費
    /// </summary>
    public int? ManagementFee { get; set; }

    /// <summary>
    /// 停車費
    /// </summary>
    public int? ParkingFee { get; set; }

    /// <summary>
    /// 違約金金額
    /// </summary>
    public int? PenaltyAmount { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 合約自訂名稱
    /// </summary>
    public string? CustomName { get; set; }

    public string LandlordHouseholdAddress { get; set; } = null!;

    public virtual ICollection<ContractComment> ContractComments { get; set; } = new List<ContractComment>();

    public virtual ICollection<ContractCustomField> ContractCustomFields { get; set; } = new List<ContractCustomField>();

    public virtual ICollection<ContractFurnitureItem> ContractFurnitureItems { get; set; } = new List<ContractFurnitureItem>();

    public virtual ICollection<ContractSignature> ContractSignatures { get; set; } = new List<ContractSignature>();

    public virtual ICollection<CustomerServiceTicket> CustomerServiceTickets { get; set; } = new List<CustomerServiceTicket>();

    public virtual RentalApplication? RentalApplication { get; set; }

    public virtual ContractTemplate? Template { get; set; }
}
