using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

[Table("images")]
[Index("EntityType", "EntityId", "Category", "DisplayOrder", "IsActive", Name = "ix_images_entity")]
[Index("ImageGuid", Name = "uq_images_imageGuid", IsUnique = true)]
public partial class Image
{
    [Key]
    [Column("imageId")]
    public long ImageId { get; set; }

    [Column("imageGuid")]
    public Guid ImageGuid { get; set; }

    [Column("entityType")]
    [StringLength(50)]
    public string EntityType { get; set; } = null!;

    [Column("entityId")]
    public int EntityId { get; set; }

    [Column("category")]
    [StringLength(50)]
    public string Category { get; set; } = null!;

    [Column("mimeType")]
    [StringLength(50)]
    public string MimeType { get; set; } = null!;

    [Column("originalFileName")]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = null!;

    [Column("storedFileName")]
    [StringLength(41)]
    [Unicode(false)]
    public string StoredFileName { get; set; } = null!;

    [Column("fileSizeBytes")]
    public long FileSizeBytes { get; set; }

    [Column("width")]
    public int Width { get; set; }

    [Column("height")]
    public int Height { get; set; }

    [Column("displayOrder")]
    public int? DisplayOrder { get; set; }

    [Column("isActive")]
    public bool IsActive { get; set; }

    [Column("uploadedByMemberId")]
    public int? UploadedByMemberId { get; set; }

    [Column("uploadedAt")]
    public DateTime UploadedAt { get; set; }

    [Column("rowVersion")]
    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("UploadedByMemberId")]
    [InverseProperty("Images")]
    public virtual Member? UploadedByMember { get; set; }
}
