namespace zuHause.Models
{
    /// <summary>
    /// 遷移驗證結果
    /// </summary>
    public class MigrationValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public int TotalImages { get; set; }
        public int ValidatedImages { get; set; }
        public int MissingLocalFiles { get; set; }
        public int MissingBlobFiles { get; set; }
        public int CorruptBlobFiles { get; set; }
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
        
        public List<ValidationIssue> Issues { get; set; } = new();
        
        public double ValidationPercentage => TotalImages > 0 
            ? (double)ValidatedImages / TotalImages * 100 
            : 0;
    }

    /// <summary>
    /// 驗證問題
    /// </summary>
    public class ValidationIssue
    {
        public long ImageId { get; set; }
        public Guid ImageGuid { get; set; }
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? LocalPath { get; set; }
        public string? BlobPath { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 遷移清理結果
    /// </summary>
    public class MigrationCleanupResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int TotalFiles { get; set; }
        public int DeletedFiles { get; set; }
        public int FailedDeletes { get; set; }
        public long FreedSpaceBytes { get; set; }
        public DateTime CleanedAt { get; set; } = DateTime.UtcNow;
        
        public List<CleanupIssue> Issues { get; set; } = new();
        
        public string FreedSpaceFormatted => FormatFileSize(FreedSpaceBytes);
        
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
    /// 清理問題
    /// </summary>
    public class CleanupIssue
    {
        public string FilePath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}