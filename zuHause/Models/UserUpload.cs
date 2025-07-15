using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 會員上傳資料紀錄表
/// </summary>
public partial class UserUpload
{
    /// <summary>
    /// 上傳ID
    /// </summary>
    public int UploadId { get; set; }

    /// <summary>
    /// 會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 來源模組代碼
    /// </summary>
    public string ModuleCode { get; set; } = null!;

    /// <summary>
    /// 來源資料ID
    /// </summary>
    public int? SourceEntityId { get; set; }

    /// <summary>
    /// 上傳種類代碼
    /// </summary>
    public string UploadTypeCode { get; set; } = null!;

    /// <summary>
    /// 原始檔名
    /// </summary>
    public string OriginalFileName { get; set; } = null!;

    /// <summary>
    /// 實體檔名
    /// </summary>
    public string StoredFileName { get; set; } = null!;

    /// <summary>
    /// 副檔名
    /// </summary>
    public string FileExt { get; set; } = null!;

    /// <summary>
    /// MIME 類型
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// 檔案路徑
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// 檔案大小(Byte)
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 檔案雜湊
    /// </summary>
    public string? Checksum { get; set; }

    /// <summary>
    /// 審核ID
    /// </summary>
    public int? ApprovalId { get; set; }

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 上傳時間
    /// </summary>
    public DateTime UploadedAt { get; set; }

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

    public virtual ICollection<FileApproval> FileApprovals { get; set; } = new List<FileApproval>();

    public virtual Member Member { get; set; } = null!;
}
