using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Data.Configurations
{
    /// <summary>
    /// Image 實體的 EF Core 配置
    /// </summary>
    public class ImageConfiguration : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            // 資料表名稱和檢查約束
            builder.ToTable("images", t =>
            {
                t.HasCheckConstraint("ck_images_entityType", 
                    "[entityType] IN ('Member','Property','Furniture','Announcement')");
                t.HasCheckConstraint("ck_images_category", 
                    "[category] IN ('BedRoom','Kitchen','Living','Balcony','Avatar','Gallery','Product')");
            });

            // 主鍵設定
            builder.HasKey(i => i.ImageId);
            builder.Property(i => i.ImageId)
                .HasColumnName("imageId")
                .ValueGeneratedOnAdd();

            // ImageGuid 設定
            builder.Property(i => i.ImageGuid)
                .HasColumnName("imageGuid")
                .IsRequired()
                .HasDefaultValueSql("newsequentialid()");

            builder.HasIndex(i => i.ImageGuid)
                .IsUnique()
                .HasDatabaseName("uq_images_imageGuid");

            // EntityType 枚舉轉字串
            builder.Property(i => i.EntityType)
                .HasColumnName("entityType")
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString(),
                    v => (EntityType)Enum.Parse(typeof(EntityType), v)
                );

            // EntityId 設定
            builder.Property(i => i.EntityId)
                .HasColumnName("entityId")
                .IsRequired();

            // Category 枚舉轉字串
            builder.Property(i => i.Category)
                .HasColumnName("category")
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString(),
                    v => (ImageCategory)Enum.Parse(typeof(ImageCategory), v)
                );

            // MimeType 設定
            builder.Property(i => i.MimeType)
                .HasColumnName("mimeType")
                .IsRequired()
                .HasMaxLength(50);

            // OriginalFileName 設定
            builder.Property(i => i.OriginalFileName)
                .HasColumnName("originalFileName")
                .IsRequired()
                .HasMaxLength(255);

            // StoredFileName 計算欄位
            builder.Property(i => i.StoredFileName)
                .HasColumnName("storedFileName")
                .HasComputedColumnSql("lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end", stored: true);

            // FileSizeBytes 設定
            builder.Property(i => i.FileSizeBytes)
                .HasColumnName("fileSizeBytes")
                .IsRequired();

            // 尺寸設定
            builder.Property(i => i.Width)
                .HasColumnName("width")
                .IsRequired();

            builder.Property(i => i.Height)
                .HasColumnName("height")
                .IsRequired();

            // DisplayOrder 設定
            builder.Property(i => i.DisplayOrder)
                .HasColumnName("displayOrder")
                .IsRequired(false);

            // IsActive 設定
            builder.Property(i => i.IsActive)
                .HasColumnName("isActive")
                .IsRequired()
                .HasDefaultValue(true);

            // UploadedByMemberId 設定
            builder.Property(i => i.UploadedByMemberId)
                .HasColumnName("uploadedByMemberId")
                .IsRequired(false);

            // UploadedAt 設定
            builder.Property(i => i.UploadedAt)
                .HasColumnName("uploadedAt")
                .IsRequired()
                .HasDefaultValueSql("sysutcdatetime()");

            // 暫時註解掉 RowVersion 配置，避免 EF Core timestamp 插入問題
            // TODO: 需要研究 EF Core 9.0.7 對 SQL Server timestamp/rowversion 的正確配置方法
            // 可能的解決方向：
            // 1. 升級 EF Core 版本檢查已修復的 Bug
            // 2. 檢查資料庫 Schema 與 Code-First 模型的完全同步性
            // 3. 在獨立測試專案中重現此問題以找出根因
            // 
            // builder.Property(i => i.RowVersion)
            //     .HasColumnName("rowVersion")
            //     .HasColumnType("timestamp")
            //     .ValueGeneratedOnAddOrUpdate()
            //     .IsConcurrencyToken()
            //     .Metadata.SetBeforeSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            // 主要查詢覆蓋索引
            builder.HasIndex(i => new { i.EntityType, i.EntityId, i.Category, i.DisplayOrder, i.IsActive })
                .HasDatabaseName("ix_images_entity")
                .IncludeProperties(i => new { i.ImageGuid, i.StoredFileName, i.FileSizeBytes, i.Width, i.Height, i.UploadedAt, i.MimeType, i.OriginalFileName });

            // 外鍵設定
            builder.HasOne<Member>()
                .WithMany()
                .HasForeignKey(i => i.UploadedByMemberId)
                .HasConstraintName("fk_images_members")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}