using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 合約欄位儲存表
/// </summary>
[Table("contracts")]
[Index("Status", "StartDate", "EndDate", Name = "IX_contracts_status_dates")]
public partial class Contract
{
    /// <summary>
    /// 合約ID
    /// </summary>
    [Key]
    [Column("contractId")]
    public int ContractId { get; set; }

    /// <summary>
    /// 申請編號ID
    /// </summary>
    [Column("rentalApplicationId")]
    public int? RentalApplicationId { get; set; }

    /// <summary>
    /// 合約範本編號
    /// </summary>
    [Column("templateId")]
    public int? TemplateId { get; set; }

    /// <summary>
    /// 合約起始日
    /// </summary>
    [Column("startDate")]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// 合約結束日
    /// </summary>
    [Column("endDate")]
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// 合約狀態
    /// </summary>
    [Column("status")]
    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    /// <summary>
    /// 管轄法院
    /// </summary>
    [Column("courtJurisdiction")]
    [StringLength(50)]
    public string? CourtJurisdiction { get; set; }

    /// <summary>
    /// 是否可轉租
    /// </summary>
    [Column("isSublettable")]
    public bool IsSublettable { get; set; }

    /// <summary>
    /// 使用目的
    /// </summary>
    [Column("usagePurpose")]
    [StringLength(20)]
    public string? UsagePurpose { get; set; }

    /// <summary>
    /// 押金金額
    /// </summary>
    [Column("depositAmount")]
    public int? DepositAmount { get; set; }

    /// <summary>
    /// 清潔費
    /// </summary>
    [Column("cleaningFee")]
    public int? CleaningFee { get; set; }

    /// <summary>
    /// 管理費
    /// </summary>
    [Column("managementFee")]
    public int? ManagementFee { get; set; }

    /// <summary>
    /// 停車費
    /// </summary>
    [Column("parkingFee")]
    public int? ParkingFee { get; set; }

    /// <summary>
    /// 違約金金額
    /// </summary>
    [Column("penaltyAmount")]
    public int? PenaltyAmount { get; set; }

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
    /// 合約自訂名稱
    /// </summary>
    [Column("customName")]
    [StringLength(50)]
    public string? CustomName { get; set; }

    [StringLength(100)]
    public string LandlordHouseholdAddress { get; set; } = null!;

    [InverseProperty("Contract")]
    public virtual ICollection<ContractComment> ContractComments { get; set; } = new List<ContractComment>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractCustomField> ContractCustomFields { get; set; } = new List<ContractCustomField>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractFurnitureItem> ContractFurnitureItems { get; set; } = new List<ContractFurnitureItem>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractSignature> ContractSignatures { get; set; } = new List<ContractSignature>();

    [InverseProperty("Contract")]
    public virtual ICollection<CustomerServiceTicket> CustomerServiceTickets { get; set; } = new List<CustomerServiceTicket>();

    [ForeignKey("RentalApplicationId")]
    [InverseProperty("Contracts")]
    public virtual RentalApplication? RentalApplication { get; set; }

    [ForeignKey("TemplateId")]
    [InverseProperty("Contracts")]
    public virtual ContractTemplate? Template { get; set; }
}
