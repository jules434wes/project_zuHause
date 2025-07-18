using zuHause.Enums;

namespace zuHause.Models
{
    /// <summary>
    /// 統一圖片管理實體類別
    /// </summary>
    public class Image
    {
        /// <summary>
        /// 圖片ID（主鍵）
        /// </summary>
        public long ImageId { get; set; }

        /// <summary>
        /// 圖片GUID（唯一識別碼）
        /// </summary>
        public Guid ImageGuid { get; set; }

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
        /// MIME類型
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// 原始檔名
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// 儲存檔名（計算欄位）
        /// </summary>
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>
        /// 檔案大小（位元組）
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
        /// 顯示順序
        /// </summary>
        public int? DisplayOrder { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 上傳者會員ID
        /// </summary>
        public int? UploadedByMemberId { get; set; }

        /// <summary>
        /// 上傳時間
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
