using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 管理員資料
/// </summary>
[Table("admins")]
public partial class Admin
{
    /// <summary>
    /// 管理員ID
    /// </summary>
    [Key]
    [Column("adminID")]
    public int AdminId { get; set; }

    /// <summary>
    /// 帳號
    /// </summary>
    [Column("account")]
    [StringLength(50)]
    [Unicode(false)]
    public string Account { get; set; } = null!;

    /// <summary>
    /// 密碼雜湊
    /// </summary>
    [Column("passwordHash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// 密碼 Salt
    /// </summary>
    [Column("passwordSalt")]
    [StringLength(64)]
    public string? PasswordSalt { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 角色代碼
    /// </summary>
    [Column("roleCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string RoleCode { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    [Column("lastLoginAt")]
    [Precision(0)]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 密碼更新時間
    /// </summary>
    [Column("passwordUpdatedAt")]
    [Precision(0)]
    public DateTime? PasswordUpdatedAt { get; set; }

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
    /// 刪除時間
    /// </summary>
    [Column("deletedAt")]
    [Precision(0)]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("RoleCode")]
    [InverseProperty("Admins")]
    public virtual AdminRole RoleCodeNavigation { get; set; } = null!;

    [InverseProperty("Admin")]
    public virtual ICollection<SystemMessage> SystemMessages { get; set; } = new List<SystemMessage>();
}
