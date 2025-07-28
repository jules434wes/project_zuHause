using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 申請狀態歷程表
/// </summary>
[Table("applicationStatusLogs")]
public partial class ApplicationStatusLog
{
    /// <summary>
    /// 狀態歷程ID
    /// </summary>
    [Key]
    [Column("statusLogID")]
    public int StatusLogId { get; set; }

    /// <summary>
    /// 申請ID
    /// </summary>
    [Column("applicationID")]
    public int ApplicationId { get; set; }

    /// <summary>
    /// 狀態代碼
    /// </summary>
    [Column("statusCode")]
    [StringLength(20)]
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 進入狀態時間
    /// </summary>
    [Column("changedAt")]
    [Precision(0)]
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [Column("note")]
    public string? Note { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("ApplicationId")]
    [InverseProperty("ApplicationStatusLogs")]
    public virtual RentalApplication Application { get; set; } = null!;
}
