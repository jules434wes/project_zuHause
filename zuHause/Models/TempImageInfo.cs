using zuHause.Enums;

namespace zuHause.Models
{
    /// <summary>
    /// 臨時圖片資訊模型
    /// 用於 Memory Cache 儲存臨時會話中的圖片資訊
    /// </summary>
    public class TempImageInfo
    {
        /// <summary>
        /// 圖片唯一識別碼
        /// </summary>
        public Guid ImageGuid { get; set; }

        /// <summary>
        /// 原始檔案名稱
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// 臨時會話ID (32字元 GUID)
        /// </summary>
        public string TempSessionId { get; set; } = string.Empty;

        /// <summary>
        /// 上傳時間 (UTC)
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 檔案大小 (bytes)
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// MIME 類型
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// 圖片分類 (Gallery/Document)
        /// </summary>
        public ImageCategory Category { get; set; } = ImageCategory.Gallery;

        /// <summary>
        /// 是否已過期 (超過6小時)
        /// </summary>
        public bool IsExpired => DateTime.UtcNow.Subtract(UploadedAt).TotalHours > 6;

        /// <summary>
        /// 生成 WebP 檔案名稱 (與現有架構保持一致)
        /// </summary>
        public string StoredFileName => $"{ImageGuid:N}.webp";
    }
}