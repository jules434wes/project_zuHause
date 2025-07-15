using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 管理員資料
/// </summary>
public partial class Admin
{
    /// <summary>
    /// 管理員ID
    /// </summary>
    public int AdminId { get; set; }

    /// <summary>
    /// 帳號
    /// </summary>
    public string Account { get; set; } = null!;

    /// <summary>
    /// 密碼雜湊
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// 密碼 Salt
    /// </summary>
    public string? PasswordSalt { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 角色代碼
    /// </summary>
    public string RoleCode { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 密碼更新時間
    /// </summary>
    public DateTime? PasswordUpdatedAt { get; set; }

    /// <summary>
    /// 建立時間
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

    public virtual AdminRole RoleCodeNavigation { get; set; } = null!;

    public virtual ICollection<SystemMessage> SystemMessages { get; set; } = new List<SystemMessage>();
}
