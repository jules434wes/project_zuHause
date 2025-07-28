using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 後台訊息模板表
/// </summary>
[Table("adminMessageTemplates")]
public partial class AdminMessageTemplate
{
    /// <summary>
    /// 模板ID(自動遞增)
    /// </summary>
    [Key]
    [Column("templateID")]
    public int TemplateId { get; set; }

    /// <summary>
    /// 模板類別代碼
    /// </summary>
    [Column("categoryCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string CategoryCode { get; set; } = null!;

    /// <summary>
    /// 標題
    /// </summary>
    [Column("title")]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 內容
    /// </summary>
    [Column("templateContent")]
    public string TemplateContent { get; set; } = null!;

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
}
