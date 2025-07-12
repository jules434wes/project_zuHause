using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 合約備註表
/// </summary>
public partial class ContractComment
{
    /// <summary>
    /// 合約備註ID
    /// </summary>
    public int ContractCommentId { get; set; }

    /// <summary>
    /// 合約ID
    /// </summary>
    public int ContractId { get; set; }

    /// <summary>
    /// 備註類型
    /// </summary>
    public string CommentType { get; set; } = null!;

    /// <summary>
    /// 內容
    /// </summary>
    public string CommentText { get; set; } = null!;

    /// <summary>
    /// 建立者
    /// </summary>
    public int CreatedById { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual Contract Contract { get; set; } = null!;
}
