using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 租賃/看房申請資料表
/// </summary>
public partial class RentalApplication
{
    /// <summary>
    /// 申請ID
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// 申請類型
    /// </summary>
    public string ApplicationType { get; set; } = null!;

    /// <summary>
    /// 申請會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    public int PropertyId { get; set; }

    /// <summary>
    /// 申請留言
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 預約看房時間
    /// </summary>
    public DateTime? ScheduleTime { get; set; }

    /// <summary>
    /// 租期開始
    /// </summary>
    public DateOnly? RentalStartDate { get; set; }

    /// <summary>
    /// 租期結束
    /// </summary>
    public DateOnly? RentalEndDate { get; set; }

    /// <summary>
    /// 目前狀態
    /// </summary>
    public string CurrentStatus { get; set; } = null!;

    /// <summary>
    /// 申請時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 戶籍地址
    /// </summary>
    public string? HouseholdAddress { get; set; }

    public virtual ICollection<ApplicationStatusLog> ApplicationStatusLogs { get; set; } = new List<ApplicationStatusLog>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual Member Member { get; set; } = null!;

    public virtual Property Property { get; set; } = null!;
}
