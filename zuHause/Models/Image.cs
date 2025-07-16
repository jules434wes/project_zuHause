using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 圖片表
/// </summary>
public partial class Image
{
    public long ImageId { get; set; }

    public Guid ImageGuid { get; set; }

    public string EntityType { get; set; } = null!;

    public int EntityId { get; set; }

    public string Category { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string StoredFileName { get; set; } = null!;

    public long FileSizeBytes { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public Guid? UploadedByUserId { get; set; }

    public DateTime UploadedAt { get; set; }
}
