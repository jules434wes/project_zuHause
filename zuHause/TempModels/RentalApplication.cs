using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 租賃/看房申請資料表
/// </summary>
[Table("rentalApplications")]
public partial class RentalApplication
{
    /// <summary>
    /// 申請ID
    /// </summary>
    [Key]
    [Column("applicationID")]
    public int ApplicationId { get; set; }

    /// <summary>
    /// 申請類型
    /// </summary>
    [Column("applicationType")]
    [StringLength(20)]
    public string ApplicationType { get; set; } = null!;

    /// <summary>
    /// 申請會員ID
    /// </summary>
    [Column("memberID")]
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    [Column("propertyID")]
    public int PropertyId { get; set; }

    /// <summary>
    /// 申請留言
    /// </summary>
    [Column("message")]
    [StringLength(300)]
    public string? Message { get; set; }

    /// <summary>
    /// 預約看房時間
    /// </summary>
    [Column("scheduleTime")]
    [Precision(0)]
    public DateTime? ScheduleTime { get; set; }

    /// <summary>
    /// 租期開始
    /// </summary>
    [Column("rentalStartDate")]
    public DateOnly? RentalStartDate { get; set; }

    /// <summary>
    /// 租期結束
    /// </summary>
    [Column("rentalEndDate")]
    public DateOnly? RentalEndDate { get; set; }

    /// <summary>
    /// 目前狀態
    /// </summary>
    [Column("currentStatus")]
    [StringLength(20)]
    public string CurrentStatus { get; set; } = null!;

    /// <summary>
    /// 申請時間
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
    /// 刪除時間
    /// </summary>
    [Column("deletedAt")]
    [Precision(0)]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 是否有效
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 戶籍地址
    /// </summary>
    [Column("householdAddress")]
    [StringLength(100)]
    public string? HouseholdAddress { get; set; }

    [InverseProperty("Application")]
    public virtual ICollection<ApplicationStatusLog> ApplicationStatusLogs { get; set; } = new List<ApplicationStatusLog>();

    [InverseProperty("RentalApplication")]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [ForeignKey("MemberId")]
    [InverseProperty("RentalApplications")]
    public virtual Member Member { get; set; } = null!;

    [ForeignKey("PropertyId")]
    [InverseProperty("RentalApplications")]
    public virtual Property Property { get; set; } = null!;
}
