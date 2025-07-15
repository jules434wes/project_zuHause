using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 後台訊息模板表
/// </summary>
public partial class AdminMessageTemplate
{
    /// <summary>
    /// 模板ID(自動遞增)
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// 模板類別代碼
    /// </summary>
    public string CategoryCode { get; set; } = null!;

    /// <summary>
    /// 標題
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 內容
    /// </summary>
    public string TemplateContent { get; set; } = null!;

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
}
