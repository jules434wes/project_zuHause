using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 管理員角色資料
/// </summary>
[Table("adminRoles")]
public partial class AdminRole
{
    /// <summary>
    /// 角色代碼
    /// </summary>
    [Key]
    [Column("roleCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string RoleCode { get; set; } = null!;

    /// <summary>
    /// 角色名稱
    /// </summary>
    [Column("roleName")]
    [StringLength(50)]
    public string RoleName { get; set; } = null!;

    /// <summary>
    /// 權限JSON
    /// </summary>
    [Column("permissionsJSON")]
    public string PermissionsJson { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

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

    [InverseProperty("RoleCodeNavigation")]
    public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();
}
