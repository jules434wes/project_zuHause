using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Services
{
    /// <summary>
    /// Blob 檔案遷移服務實作
    /// 負責處理臨時區域到正式區域的檔案移動邏輯
    /// </summary>
    public class BlobMigrationService : IBlobMigrationService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IBlobUrlGenerator _urlGenerator;
        private readonly ITempSessionService _tempSessionService;
        private readonly ILogger<BlobMigrationService> _logger;

        public BlobMigrationService(
            IBlobStorageService blobStorageService,
            IBlobUrlGenerator urlGenerator,
            ITempSessionService tempSessionService,
            ILogger<BlobMigrationService> logger)
        {
            _blobStorageService = blobStorageService;
            _urlGenerator = urlGenerator;
            _tempSessionService = tempSessionService;
            _logger = logger;
        }

        /// <summary>
        /// 將臨時區域的檔案移動到正式區域
        /// </summary>
        public async Task<MigrationResult> MoveTempToPermanentAsync(
            string tempSessionId, 
            IEnumerable<Guid> imageGuids, 
            ImageCategory category, 
            int entityId)
        {
            _logger.LogInformation("開始檔案遷移: TempSession={TempSessionId}, Category={Category}, EntityId={EntityId}, ImageCount={ImageCount}",
                tempSessionId, category, entityId, imageGuids.Count());

            try
            {
                // 1. 驗證臨時檔案
                var validationResult = await ValidateTempFilesAsync(tempSessionId, imageGuids);
                if (!validationResult.IsValid)
                {
                    return MigrationResult.CreateFailure($"臨時檔案驗證失敗: {validationResult.ErrorMessage}");
                }

                // 2. 準備移動操作
                var moveOperations = new Dictionary<string, string>();
                var migrationDetails = new Dictionary<Guid, Dictionary<ImageSize, BlobUploadResult>>();

                foreach (var imageGuid in imageGuids)
                {
                    migrationDetails[imageGuid] = new Dictionary<ImageSize, BlobUploadResult>();

                    // 為每個尺寸準備移動操作
                    foreach (ImageSize size in Enum.GetValues<ImageSize>())
                    {
                        var tempPath = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, size);
                        var permanentPath = _urlGenerator.GetBlobPath(category, entityId, imageGuid, size);
                        
                        moveOperations[tempPath] = permanentPath;
                    }
                }

                _logger.LogDebug("準備移動 {OperationCount} 個檔案", moveOperations.Count);

                // 3. 執行批次移動
                var moveResults = await _blobStorageService.MoveBatchAsync(moveOperations, deleteSource: true);

                // 4. 處理結果
                var movedFilePaths = new List<string>();
                var allSuccess = true;

                foreach (var imageGuid in imageGuids)
                {
                    foreach (ImageSize size in Enum.GetValues<ImageSize>())
                    {
                        var tempPath = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, size);
                        
                        if (moveResults.TryGetValue(tempPath, out var result))
                        {
                            migrationDetails[imageGuid][size] = result;
                            
                            if (result.Success)
                            {
                                movedFilePaths.Add(result.BlobPath ?? "");
                            }
                            else
                            {
                                allSuccess = false;
                                _logger.LogError("檔案移動失敗: {TempPath} -> {Error}", tempPath, result.Message);
                            }
                        }
                        else
                        {
                            // 如果沒有在結果中找到，表示移動失敗
                            var failureResult = BlobUploadResult.CreateFailure($"移動操作未執行: {tempPath}");
                            migrationDetails[imageGuid][size] = failureResult;
                            allSuccess = false;
                        }
                    }
                }

                // 5. 更新臨時會話（移除已成功遷移的圖片）
                if (allSuccess)
                {
                    foreach (var imageGuid in imageGuids)
                    {
                        await _tempSessionService.RemoveTempImageAsync(tempSessionId, imageGuid);
                    }
                }

                // 6. 建立結果
                if (allSuccess)
                {
                    _logger.LogInformation("檔案遷移成功完成: {FileCount} 個檔案", movedFilePaths.Count);
                    return MigrationResult.CreateSuccess(migrationDetails, movedFilePaths);
                }
                else
                {
                    var errorMessage = "部分檔案遷移失敗，請檢查日誌";
                    _logger.LogWarning(errorMessage);
                    return MigrationResult.CreateFailure(errorMessage, migrationDetails, movedFilePaths);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檔案遷移過程中發生異常");
                return MigrationResult.CreateFailure($"遷移過程中發生異常: {ex.Message}");
            }
        }

        /// <summary>
        /// 驗證臨時檔案是否存在且有效
        /// </summary>
        public async Task<BlobValidationResult> ValidateTempFilesAsync(
            string tempSessionId, 
            IEnumerable<Guid> imageGuids)
        {
            _logger.LogDebug("開始驗證臨時檔案: TempSession={TempSessionId}, ImageCount={ImageCount}",
                tempSessionId, imageGuids.Count());

            try
            {
                // 1. 驗證會話有效性
                if (!await _tempSessionService.IsValidTempSessionAsync(tempSessionId))
                {
                    return BlobValidationResult.CreateFailure("無效的臨時會話ID");
                }

                // 2. 驗證會話中的圖片資訊
                var tempImages = await _tempSessionService.GetTempImagesAsync(tempSessionId);
                var tempImageGuids = tempImages.Select(img => img.ImageGuid).ToHashSet();

                var missingInSession = imageGuids.Where(guid => !tempImageGuids.Contains(guid)).ToList();
                if (missingInSession.Any())
                {
                    return BlobValidationResult.CreateFailure(
                        $"以下圖片不存在於臨時會話中: {string.Join(", ", missingInSession)}");
                }

                // 3. 驗證實際檔案存在性
                var validFiles = new Dictionary<Guid, List<string>>();
                var missingFiles = new Dictionary<Guid, List<string>>();

                foreach (var imageGuid in imageGuids)
                {
                    validFiles[imageGuid] = new List<string>();
                    missingFiles[imageGuid] = new List<string>();

                    foreach (ImageSize size in Enum.GetValues<ImageSize>())
                    {
                        var tempPath = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, size);
                        var exists = await _blobStorageService.ExistsAsync(tempPath);

                        if (exists)
                        {
                            validFiles[imageGuid].Add(tempPath);
                        }
                        else
                        {
                            missingFiles[imageGuid].Add(tempPath);
                            _logger.LogWarning("臨時檔案不存在: {TempPath}", tempPath);
                        }
                    }
                }

                // 4. 檢查是否有遺失的檔案
                var totalMissingFiles = missingFiles.SelectMany(m => m.Value).Count();
                if (totalMissingFiles > 0)
                {
                    return BlobValidationResult.CreateFailure(
                        $"發現 {totalMissingFiles} 個遺失的臨時檔案",
                        validFiles,
                        missingFiles);
                }

                _logger.LogDebug("臨時檔案驗證通過: {ValidFileCount} 個檔案", 
                    validFiles.SelectMany(v => v.Value).Count());

                return BlobValidationResult.CreateSuccess(validFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證臨時檔案時發生異常");
                return BlobValidationResult.CreateFailure($"驗證過程中發生異常: {ex.Message}");
            }
        }

        /// <summary>
        /// 回滾已移動的檔案（發生錯誤時使用）
        /// </summary>
        public async Task<bool> RollbackMigrationAsync(MigrationResult migrationResult)
        {
            if (migrationResult.MovedFilePaths.Count == 0)
            {
                _logger.LogInformation("沒有需要回滾的檔案");
                return true;
            }

            _logger.LogWarning("開始回滾檔案遷移: {FileCount} 個檔案", migrationResult.MovedFilePaths.Count);

            try
            {
                var deleteResults = await _blobStorageService.DeleteMultipleAsync(migrationResult.MovedFilePaths);
                var successCount = deleteResults.Count(r => r.Value);
                var failureCount = deleteResults.Count(r => !r.Value);

                if (failureCount > 0)
                {
                    _logger.LogError("回滾過程中 {FailureCount} 個檔案刪除失敗", failureCount);
                    return false;
                }

                _logger.LogInformation("檔案回滾成功完成: {SuccessCount} 個檔案已刪除", successCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回滾檔案時發生異常");
                return false;
            }
        }
    }
}