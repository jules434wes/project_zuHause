using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace zuHause.Models;

/// <summary>
/// 合約範本表
/// </summary>
public partial class ContractTemplate
{
    /// <summary>
    /// 範本ID
    /// </summary>
    [Key]
    public int ContractTemplateId { get; set; }

    /// <summary>
    /// 範本名稱
    /// </summary>
    public string TemplateName { get; set; } = null!;

    /// <summary>
    /// 範本內容
    /// </summary>
    public string TemplateContent { get; set; } = null!;

    /// <summary>
    /// 範本上傳時間
    /// </summary>
    public DateTime UploadedAt { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
