using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 檔案審核
/// </summary>
public partial class FileApproval
{
    /// <summary>
    /// 審核ID
    /// </summary>
    public int ApprovalId { get; set; }

    /// <summary>
    /// 上傳ID
    /// </summary>
    public int UploadId { get; set; }

    /// <summary>
    /// 會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 審核狀態代碼
    /// </summary>
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 審核說明
    /// </summary>
    public string? ResultDescription { get; set; }

    /// <summary>
    /// 申請時間
    /// </summary>
    public DateTime AppliedAt { get; set; }

    /// <summary>
    /// 審核時間
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// 審核人員ID
    /// </summary>
    public int? ReviewerAdminId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual UserUpload Upload { get; set; } = null!;
}
