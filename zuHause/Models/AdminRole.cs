using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 管理員角色資料
/// </summary>
public partial class AdminRole
{
    /// <summary>
    /// 角色代碼
    /// </summary>
    public string RoleCode { get; set; } = null!;

    /// <summary>
    /// 角色名稱
    /// </summary>
    public string RoleName { get; set; } = null!;

    /// <summary>
    /// 權限JSON
    /// </summary>
    public string PermissionsJson { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();
}
