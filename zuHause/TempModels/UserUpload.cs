using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 會員上傳資料紀錄表
/// </summary>
[Table("userUploads")]
public partial class UserUpload
{
    /// <summary>
    /// 上傳ID
    /// </summary>
    [Key]
    [Column("uploadID")]
    public int UploadId { get; set; }

    /// <summary>
    /// 會員ID
    /// </summary>
    [Column("memberID")]
    public int MemberId { get; set; }

    /// <summary>
    /// 來源模組代碼
    /// </summary>
    [Column("moduleCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string ModuleCode { get; set; } = null!;

    /// <summary>
    /// 來源資料ID
    /// </summary>
    [Column("sourceEntityID")]
    public int? SourceEntityId { get; set; }

    /// <summary>
    /// 上傳種類代碼
    /// </summary>
    [Column("uploadTypeCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string UploadTypeCode { get; set; } = null!;

    /// <summary>
    /// 原始檔名
    /// </summary>
    [Column("originalFileName")]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = null!;

    /// <summary>
    /// 實體檔名
    /// </summary>
    [Column("storedFileName")]
    [StringLength(255)]
    [Unicode(false)]
    public string StoredFileName { get; set; } = null!;

    /// <summary>
    /// 副檔名
    /// </summary>
    [Column("fileExt")]
    [StringLength(10)]
    [Unicode(false)]
    public string FileExt { get; set; } = null!;

    /// <summary>
    /// MIME 類型
    /// </summary>
    [Column("mimeType")]
    [StringLength(100)]
    [Unicode(false)]
    public string? MimeType { get; set; }

    /// <summary>
    /// 檔案路徑
    /// </summary>
    [Column("filePath")]
    [StringLength(255)]
    [Unicode(false)]
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// 檔案大小(Byte)
    /// </summary>
    [Column("fileSize")]
    public long FileSize { get; set; }

    /// <summary>
    /// 檔案雜湊
    /// </summary>
    [Column("checksum")]
    [StringLength(64)]
    [Unicode(false)]
    public string? Checksum { get; set; }

    /// <summary>
    /// 審核ID
    /// </summary>
    [Column("approvalID")]
    public int? ApprovalId { get; set; }

    /// <summary>
    /// 是否有效
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 上傳時間
    /// </summary>
    [Column("uploadedAt")]
    [Precision(0)]
    public DateTime UploadedAt { get; set; }

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

    /// <summary>
    /// 刪除時間
    /// </summary>
    [Column("deletedAt")]
    [Precision(0)]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("Upload")]
    public virtual ICollection<FileApproval> FileApprovals { get; set; } = new List<FileApproval>();

    [ForeignKey("MemberId")]
    [InverseProperty("UserUploads")]
    public virtual Member Member { get; set; } = null!;
}
