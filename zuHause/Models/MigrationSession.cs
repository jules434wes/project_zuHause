using zuHause.Enums;

namespace zuHause.Models
{
    /// <summary>
    /// 遷移會話
    /// </summary>
    public class MigrationSession
    {
        public string MigrationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public MigrationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? PausedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        public int TotalImages { get; set; }
        public int ProcessedImages { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int CurrentBatch { get; set; }
        public int BatchSize { get; set; } = 50;
        
        public List<string> MigratedFiles { get; set; } = new();
        public List<FailedMigrationItem> FailedImages { get; set; } = new();
        public List<MigrationWarning> Warnings { get; set; } = new();
        
        public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue 
            ? CompletedAt.Value - StartedAt.Value 
            : null;
            
        public double ProgressPercentage => TotalImages > 0 
            ? (double)ProcessedImages / TotalImages * 100 
            : 0;
    }

    /// <summary>
    /// 遷移警告
    /// </summary>
    public class MigrationWarning
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 遷移狀態
    /// </summary>
    public enum MigrationStatus
    {
        Created = 0,
        Running = 1,
        Paused = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5
    }

    /// <summary>
    /// 遷移配置
    /// </summary>
    public class MigrationConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public int BatchSize { get; set; } = 50;
        public int MaxConcurrency { get; set; } = 3;
        public bool DeleteLocalFilesAfterMigration { get; set; } = false;
        public bool ValidateAfterUpload { get; set; } = true;
        public List<long> IncludeImageIds { get; set; } = new();
        public List<long> ExcludeImageIds { get; set; } = new();
        public List<EntityType> EntityTypes { get; set; } = new();
        public List<ImageCategory> Categories { get; set; } = new();
    }

    /// <summary>
    /// 失敗的遷移項目
    /// </summary>
    public class FailedMigrationItem
    {
        public long ImageId { get; set; }
        public Guid ImageGuid { get; set; }
        public string LocalPath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; } = DateTime.UtcNow;
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// 遷移進度
    /// </summary>
    public class MigrationProgress
    {
        public string MigrationId { get; set; } = string.Empty;
        public MigrationStatus Status { get; set; }
        public int TotalImages { get; set; }
        public int ProcessedImages { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double ProgressPercentage { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
        public string CurrentBatchInfo { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}