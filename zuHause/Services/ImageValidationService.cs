using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Services.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// 圖片驗證服務實作
    /// </summary>
    public class ImageValidationService : IImageValidationService
    {
        private readonly ZuHauseContext _context;

        private static readonly Dictionary<string, string[]> AllowedMimeTypes = new()
        {
            { "image/jpeg", new[] { ".jpg", ".jpeg" } },
            { "image/png", new[] { ".png" } },
            { "image/webp", new[] { ".webp" } },
            { "application/pdf", new[] { ".pdf" } }
        };

        private static readonly Dictionary<ImageCategory, long> MaxFileSizes = new()
        {
            { ImageCategory.Avatar, 2 * 1024 * 1024 }, // 2MB
            { ImageCategory.Gallery, 10 * 1024 * 1024 }, // 10MB
            { ImageCategory.BedRoom, 10 * 1024 * 1024 }, // 10MB
            { ImageCategory.Living, 10 * 1024 * 1024 }, // 10MB
            { ImageCategory.Kitchen, 10 * 1024 * 1024 }, // 10MB
            { ImageCategory.Balcony, 10 * 1024 * 1024 }, // 10MB
            { ImageCategory.Product, 5 * 1024 * 1024 }, // 5MB
            { ImageCategory.Document, 10 * 1024 * 1024 } // 10MB
        };

        private static readonly Dictionary<ImageCategory, (int MinWidth, int MinHeight, int MaxWidth, int MaxHeight)> DimensionLimits = new()
        {
            { ImageCategory.Avatar, (100, 100, 1000, 1000) },
            { ImageCategory.Gallery, (400, 300, 4000, 3000) },
            { ImageCategory.BedRoom, (400, 300, 4000, 3000) },
            { ImageCategory.Living, (400, 300, 4000, 3000) },
            { ImageCategory.Kitchen, (400, 300, 4000, 3000) },
            { ImageCategory.Balcony, (400, 300, 4000, 3000) },
            { ImageCategory.Product, (300, 300, 2000, 2000) }
        };

        public ImageValidationService(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 驗證檔案格式是否有效
        /// </summary>
        public ValidationResult ValidateFileFormat(string mimeType, string fileName)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
                return ValidationResult.Failure("MIME 類型不能為空");

            if (string.IsNullOrWhiteSpace(fileName))
                return ValidationResult.Failure("檔案名稱不能為空");

            // 檢查 MIME 類型是否支援
            if (!AllowedMimeTypes.ContainsKey(mimeType.ToLower()))
                return ValidationResult.Failure($"不支援的檔案格式: {mimeType}");

            // 檢查副檔名是否與 MIME 類型一致
            var fileExtension = Path.GetExtension(fileName).ToLower();
            var allowedExtensions = AllowedMimeTypes[mimeType.ToLower()];
            
            if (!allowedExtensions.Contains(fileExtension))
                return ValidationResult.Failure($"檔案副檔名 {fileExtension} 與 MIME 類型 {mimeType} 不一致");

            return ValidationResult.Success();
        }

        /// <summary>
        /// 驗證檔案大小是否在允許範圍內
        /// </summary>
        public ValidationResult ValidateFileSize(long fileSizeBytes, ImageCategory category)
        {
            if (fileSizeBytes <= 0)
                return ValidationResult.Failure("檔案大小必須大於 0");

            if (!MaxFileSizes.TryGetValue(category, out var maxSize))
                return ValidationResult.Failure($"未定義分類 {category} 的檔案大小限制");

            if (fileSizeBytes > maxSize)
            {
                var maxSizeMB = maxSize / (1024.0 * 1024.0);
                var currentSizeMB = fileSizeBytes / (1024.0 * 1024.0);
                return ValidationResult.Failure($"檔案大小 {currentSizeMB:F2}MB 超過限制 {maxSizeMB:F2}MB");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// 驗證圖片尺寸是否符合要求
        /// </summary>
        public ValidationResult ValidateImageDimensions(int width, int height, ImageCategory category)
        {
            // PDF 文件不需要尺寸驗證
            if (category == ImageCategory.Document)
                return ValidationResult.Success();
                
            if (width <= 0 || height <= 0)
                return ValidationResult.Failure("圖片尺寸必須大於 0");

            if (!DimensionLimits.TryGetValue(category, out var limits))
                return ValidationResult.Failure($"未定義分類 {category} 的尺寸限制");

            var errors = new List<string>();

            if (width < limits.MinWidth)
                errors.Add($"圖片寬度 {width}px 小於最小要求 {limits.MinWidth}px");

            if (height < limits.MinHeight)
                errors.Add($"圖片高度 {height}px 小於最小要求 {limits.MinHeight}px");

            if (width > limits.MaxWidth)
                errors.Add($"圖片寬度 {width}px 超過最大限制 {limits.MaxWidth}px");

            if (height > limits.MaxHeight)
                errors.Add($"圖片高度 {height}px 超過最大限制 {limits.MaxHeight}px");

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        /// <summary>
        /// 驗證實體關聯是否有效
        /// </summary>
        public async Task<ValidationResult> ValidateEntityRelationAsync(EntityType entityType, int entityId)
        {
            if (entityId <= 0)
                return ValidationResult.Failure("實體ID必須大於 0");

            bool entityExists = entityType switch
            {
                EntityType.Member => await _context.Members.AnyAsync(m => m.MemberId == entityId),
                EntityType.Property => await _context.Properties.AnyAsync(p => p.PropertyId == entityId),
                EntityType.Furniture => await _context.FurnitureProducts.AnyAsync(f => f.FurnitureProductId == entityId.ToString()),
                EntityType.Announcement => await _context.SystemMessages.AnyAsync(s => s.MessageId == entityId),
                _ => false
            };

            return entityExists 
                ? ValidationResult.Success() 
                : ValidationResult.Failure($"找不到類型為 {entityType} 且ID為 {entityId} 的實體");
        }

        /// <summary>
        /// 完整驗證圖片實體
        /// </summary>
        public async Task<ValidationResult> ValidateImageAsync(Image image)
        {
            if (image == null)
                return ValidationResult.Failure("圖片實體不能為空");

            var allErrors = new List<string>();

            // 檔案格式驗證
            var formatResult = ValidateFileFormat(image.MimeType, image.OriginalFileName);
            if (!formatResult.IsValid)
                allErrors.AddRange(formatResult.Errors);

            // 檔案大小驗證
            var sizeResult = ValidateFileSize(image.FileSizeBytes, image.Category);
            if (!sizeResult.IsValid)
                allErrors.AddRange(sizeResult.Errors);

            // 圖片尺寸驗證（Document 類別會自動跳過）
            var dimensionResult = ValidateImageDimensions(image.Width, image.Height, image.Category);
            if (!dimensionResult.IsValid)
                allErrors.AddRange(dimensionResult.Errors);

            // 實體關聯驗證
            var entityResult = await ValidateEntityRelationAsync(image.EntityType, image.EntityId);
            if (!entityResult.IsValid)
                allErrors.AddRange(entityResult.Errors);

            // 顯示順序驗證
            if (image.DisplayOrder.HasValue && image.DisplayOrder.Value <= 0)
                allErrors.Add("顯示順序必須大於 0");

            return allErrors.Any() 
                ? ValidationResult.Failure(allErrors) 
                : ValidationResult.Success();
        }
    }
}