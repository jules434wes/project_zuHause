using zuHause.Enums;

namespace zuHause.Models
{
    /// <summary>
    /// 本地圖片掃描結果
    /// </summary>
    public class LocalImageScanResult
    {
        public int TotalImages { get; set; }
        public int ReadyCount { get; set; }
        public int ProblematicCount { get; set; }
        public TimeSpan EstimatedMigrationTime { get; set; }
        public DateTime ScanTime { get; set; } = DateTime.UtcNow;
        
        public List<LocalImageInfo> ReadyToMigrate { get; set; } = new();
        public List<LocalImageInfo> ProblematicImages { get; set; } = new();
        
        public long TotalFileSizeBytes => ReadyToMigrate.Sum(img => img.FileSize);
        public string TotalFileSizeFormatted => FormatFileSize(TotalFileSizeBytes);
        
        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            double size = bytes;
            int suffixIndex = 0;
            
            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }
            
            return $"{size:F2} {suffixes[suffixIndex]}";
        }
    }

    /// <summary>
    /// 本地圖片資訊
    /// </summary>
    public class LocalImageInfo
    {
        public long ImageId { get; set; }
        public Guid ImageGuid { get; set; }
        public EntityType EntityType { get; set; }
        public int EntityId { get; set; }
        public ImageCategory Category { get; set; }
        public string StoredFileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string LocalPath { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public LocalImageStatus Status { get; set; }
        public DateTime LastModified { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 本地圖片狀態
    /// </summary>
    public enum LocalImageStatus
    {
        Unknown = 0,
        Ready = 1,
        FileNotFound = 2,
        CorruptFile = 3,
        UnsupportedFormat = 4,
        AccessDenied = 5
    }

    /// <summary>
    /// 掃描選項
    /// </summary>
    public class LocalImageScanOptions
    {
        public bool IncludeCorruptFiles { get; set; } = false;
        public bool ValidateFileIntegrity { get; set; } = true;
        public int MaxConcurrency { get; set; } = 10;
        public List<string> ExcludePaths { get; set; } = new();
        public DateTime? ModifiedAfter { get; set; }
        public DateTime? ModifiedBefore { get; set; }
    }
}