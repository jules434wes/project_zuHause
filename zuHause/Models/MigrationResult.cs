using zuHause.Enums;

namespace zuHause.Models
{
    /// <summary>
    /// 檔案遷移結果模型
    /// </summary>
    public class MigrationResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 總檔案數量
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// 成功遷移的檔案數量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失敗的檔案數量
        /// </summary>
        public int FailedCount => TotalFiles - SuccessCount;

        /// <summary>
        /// 遷移詳細結果（圖片GUID -> 各尺寸遷移結果）
        /// </summary>
        public Dictionary<Guid, Dictionary<ImageSize, BlobUploadResult>> Details { get; set; } = new();

        /// <summary>
        /// 已成功移動的檔案路徑（用於回滾）
        /// </summary>
        public List<string> MovedFilePaths { get; set; } = new();

        /// <summary>
        /// 遷移開始時間
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 遷移完成時間
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 遷移耗時
        /// </summary>
        public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

        /// <summary>
        /// 建立成功的遷移結果
        /// </summary>
        public static MigrationResult CreateSuccess(
            Dictionary<Guid, Dictionary<ImageSize, BlobUploadResult>> details,
            List<string> movedFilePaths)
        {
            return new MigrationResult
            {
                IsSuccess = true,
                Details = details,
                MovedFilePaths = movedFilePaths,
                TotalFiles = details.SelectMany(d => d.Value).Count(),
                SuccessCount = details.SelectMany(d => d.Value).Count(r => r.Value.Success),
                CompletedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 建立失敗的遷移結果
        /// </summary>
        public static MigrationResult CreateFailure(
            string errorMessage,
            Dictionary<Guid, Dictionary<ImageSize, BlobUploadResult>>? details = null,
            List<string>? movedFilePaths = null)
        {
            var result = new MigrationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                Details = details ?? new(),
                MovedFilePaths = movedFilePaths ?? new(),
                CompletedAt = DateTime.UtcNow
            };

            result.TotalFiles = result.Details.SelectMany(d => d.Value).Count();
            result.SuccessCount = result.Details.SelectMany(d => d.Value).Count(r => r.Value.Success);

            return result;
        }
    }

    /// <summary>
    /// Blob 檔案驗證結果模型
    /// </summary>
    public class BlobValidationResult
    {
        /// <summary>
        /// 是否全部有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 找到的有效檔案（圖片GUID -> 檔案路徑清單）
        /// </summary>
        public Dictionary<Guid, List<string>> ValidFiles { get; set; } = new();

        /// <summary>
        /// 遺失的檔案（圖片GUID -> 預期的檔案路徑清單）
        /// </summary>
        public Dictionary<Guid, List<string>> MissingFiles { get; set; } = new();

        /// <summary>
        /// 驗證的總檔案數量
        /// </summary>
        public int TotalExpectedFiles => ValidFiles.SelectMany(v => v.Value).Count() + MissingFiles.SelectMany(m => m.Value).Count();

        /// <summary>
        /// 有效檔案數量
        /// </summary>
        public int ValidFileCount => ValidFiles.SelectMany(v => v.Value).Count();

        /// <summary>
        /// 建立成功的驗證結果
        /// </summary>
        public static BlobValidationResult CreateSuccess(Dictionary<Guid, List<string>> validFiles)
        {
            return new BlobValidationResult
            {
                IsValid = true,
                ValidFiles = validFiles
            };
        }

        /// <summary>
        /// 建立失敗的驗證結果
        /// </summary>
        public static BlobValidationResult CreateFailure(
            string errorMessage,
            Dictionary<Guid, List<string>>? validFiles = null,
            Dictionary<Guid, List<string>>? missingFiles = null)
        {
            return new BlobValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                ValidFiles = validFiles ?? new(),
                MissingFiles = missingFiles ?? new()
            };
        }
    }
}