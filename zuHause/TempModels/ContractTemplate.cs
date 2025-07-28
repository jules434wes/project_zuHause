using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 合約範本表
/// </summary>
[Table("contractTemplates")]
public partial class ContractTemplate
{
    /// <summary>
    /// 範本ID
    /// </summary>
    [Key]
    [Column("contractTemplateId")]
    public int ContractTemplateId { get; set; }

    /// <summary>
    /// 範本名稱
    /// </summary>
    [Column("templateName")]
    [StringLength(30)]
    public string TemplateName { get; set; } = null!;

    /// <summary>
    /// 範本內容
    /// </summary>
    [Column("templateContent")]
    public string TemplateContent { get; set; } = null!;

    /// <summary>
    /// 範本上傳時間
    /// </summary>
    [Column("uploadedAt")]
    [Precision(0)]
    public DateTime UploadedAt { get; set; }

    [InverseProperty("Template")]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
