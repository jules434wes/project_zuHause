using zuHause.Enums;

namespace zuHause.DTOs
{
    /// <summary>
    /// 統一圖片上傳結果 DTO - 支援所有實體類型的圖片上傳
    /// </summary>
    public class ImageUploadResult
    {
        /// <summary>
        /// 上傳是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 錯誤訊息 (上傳失敗時)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 錯誤代碼 (用於程式化處理)
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 圖片資料庫 ID
        /// </summary>
        public long? ImageId { get; set; }

        /// <summary>
        /// 圖片唯一識別碼
        /// </summary>
        public Guid? ImageGuid { get; set; }

        /// <summary>
        /// 儲存檔名
        /// </summary>
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>
        /// 圖片存取 URL (用於前端顯示)
        /// </summary>
        public string? ImageUrl => !string.IsNullOrEmpty(StoredFileName) ? $"/uploads/{StoredFileName}" : null;

        /// <summary>
        /// 原始檔案名稱
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// 檔案大小 (位元組)
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// 圖片寬度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 圖片高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// MIME 類型
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// 上傳時間
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 實體類型
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// 實體ID
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// 圖片分類
        /// </summary>
        public ImageCategory Category { get; set; }

        /// <summary>
        /// 顯示順序
        /// </summary>
        public int? DisplayOrder { get; set; }

        /// <summary>
        /// 是否為主圖
        /// </summary>
        public bool IsMainImage { get; set; }

        /// <summary>
        /// 靜態工廠方法 - 建立成功結果
        /// </summary>
        public static ImageUploadResult CreateSuccess(
            long imageId,
            Guid imageGuid,
            string originalFileName,
            string storedFileName,
            EntityType entityType,
            int entityId,
            ImageCategory category,
            long fileSizeBytes,
            int width,
            int height,
            string mimeType,
            int? displayOrder = null,
            bool isMainImage = false)
        {
            return new ImageUploadResult
            {
                Success = true,
                ImageId = imageId,
                ImageGuid = imageGuid,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                EntityType = entityType,
                EntityId = entityId,
                Category = category,
                FileSizeBytes = fileSizeBytes,
                Width = width,
                Height = height,
                MimeType = mimeType,
                DisplayOrder = displayOrder,
                IsMainImage = isMainImage,
                UploadedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 靜態工廠方法 - 建立失敗結果
        /// </summary>
        public static ImageUploadResult CreateFailure(
            string originalFileName, 
            string errorMessage, 
            string? errorCode = null)
        {
            return new ImageUploadResult
            {
                Success = false,
                OriginalFileName = originalFileName,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// 建立驗證失敗的結果
        /// </summary>
        public static ImageUploadResult ValidationFailure(
            string originalFileName, 
            string validationError)
        {
            return new ImageUploadResult
            {
                Success = false,
                OriginalFileName = originalFileName,
                ErrorMessage = validationError,
                ErrorCode = "VALIDATION_FAILED"
            };
        }

        /// <summary>
        /// 建立實體不存在的錯誤結果
        /// </summary>
        public static ImageUploadResult EntityNotFound(
            string originalFileName, 
            EntityType entityType, 
            int entityId)
        {
            return new ImageUploadResult
            {
                Success = false,
                OriginalFileName = originalFileName,
                ErrorMessage = $"{entityType} ID {entityId} 不存在",
                ErrorCode = "ENTITY_NOT_FOUND",
                EntityType = entityType,
                EntityId = entityId
            };
        }
    }
}