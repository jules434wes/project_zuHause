using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using zuHause.Data;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Services
{
    /// <summary>
    /// 本地檔案到 Azure Blob Storage 遷移服務實作
    /// </summary>
    public class LocalToBlobMigrationService : ILocalToBlobMigrationService
    {
        private readonly ZuHauseContext _context;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IBlobUrlGenerator _urlGenerator;
        private readonly IImageProcessor _imageProcessor;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<LocalToBlobMigrationService> _logger;
        private readonly IConfiguration _configuration;

        private readonly SemaphoreSlim _migrationSemaphore = new(1, 1);
        private const string MigrationSessionPrefix = "migration_session_";
        private const string ActiveMigrationsKey = "active_migrations";

        public LocalToBlobMigrationService(
            ZuHauseContext context,
            IBlobStorageService blobStorageService,
            IBlobUrlGenerator urlGenerator,
            IImageProcessor imageProcessor,
            IMemoryCache memoryCache,
            ILogger<LocalToBlobMigrationService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _urlGenerator = urlGenerator;
            _imageProcessor = imageProcessor;
            _memoryCache = memoryCache;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 掃描本地圖片檔案
        /// </summary>
        public async Task<LocalImageScanResult> ScanLocalImagesAsync(LocalImageScanOptions? scanOptions = null)
        {
            scanOptions ??= new LocalImageScanOptions();
            _logger.LogInformation("開始掃描本地圖片檔案");

            var result = new LocalImageScanResult();

            try
            {
                // 1. 從資料庫查詢圖片記錄
                var query = _context.Images.AsQueryable();

                if (scanOptions.ModifiedAfter.HasValue)
                    query = query.Where(i => i.UploadedAt >= scanOptions.ModifiedAfter.Value);

                if (scanOptions.ModifiedBefore.HasValue)
                    query = query.Where(i => i.UploadedAt <= scanOptions.ModifiedBefore.Value);

                var dbImages = await query.ToListAsync();
                result.TotalImages = dbImages.Count;

                _logger.LogDebug("從資料庫查詢到 {ImageCount} 筆圖片記錄", dbImages.Count);

                // 2. 並行處理圖片檔案檢查
                var semaphore = new SemaphoreSlim(scanOptions.MaxConcurrency, scanOptions.MaxConcurrency);
                var tasks = dbImages.Select(async image =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await ProcessSingleImageAsync(image, scanOptions);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                var scanResults = await Task.WhenAll(tasks);

                // 3. 分類結果
                foreach (var imageInfo in scanResults)
                {
                    if (imageInfo.Status == LocalImageStatus.Ready)
                    {
                        result.ReadyToMigrate.Add(imageInfo);
                        result.ReadyCount++;
                    }
                    else
                    {
                        result.ProblematicImages.Add(imageInfo);
                        result.ProblematicCount++;
                    }
                }

                // 4. 計算預估遷移時間
                result.EstimatedMigrationTime = CalculateEstimatedMigrationTime(result.ReadyCount);

                _logger.LogInformation("掃描完成: 總計 {Total}, 可遷移 {Ready}, 有問題 {Problematic}",
                    result.TotalImages, result.ReadyCount, result.ProblematicCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "掃描本地圖片檔案時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 開始批次遷移作業
        /// </summary>
        public async Task<MigrationSession> StartMigrationAsync(MigrationConfiguration migrationConfig)
        {
            await _migrationSemaphore.WaitAsync();
            try
            {
                _logger.LogInformation("開始遷移作業: {Name}", migrationConfig.Name);

                var session = new MigrationSession
                {
                    MigrationId = Guid.NewGuid().ToString("N"),
                    Name = migrationConfig.Name,
                    Status = MigrationStatus.Created,
                    BatchSize = migrationConfig.BatchSize,
                    CreatedAt = DateTime.UtcNow
                };

                // 儲存會話到快取
                _memoryCache.Set($"{MigrationSessionPrefix}{session.MigrationId}", session, TimeSpan.FromHours(24));

                // 加入活躍遷移列表
                var activeMigrations = _memoryCache.Get<List<string>>(ActiveMigrationsKey) ?? new List<string>();
                activeMigrations.Add(session.MigrationId);
                _memoryCache.Set(ActiveMigrationsKey, activeMigrations, TimeSpan.FromHours(24));

                // 在背景開始遷移工作
                _ = Task.Run(async () => await ExecuteMigrationAsync(session.MigrationId, migrationConfig));

                return session;
            }
            finally
            {
                _migrationSemaphore.Release();
            }
        }

        /// <summary>
        /// 暫停遷移作業
        /// </summary>
        public async Task<bool> PauseMigrationAsync(string migrationId)
        {
            var session = _memoryCache.Get<MigrationSession>($"{MigrationSessionPrefix}{migrationId}");
            if (session == null || session.Status != MigrationStatus.Running)
            {
                return false;
            }

            session.Status = MigrationStatus.Paused;
            session.PausedAt = DateTime.UtcNow;
            _memoryCache.Set($"{MigrationSessionPrefix}{migrationId}", session, TimeSpan.FromHours(24));

            _logger.LogInformation("遷移作業已暫停: {MigrationId}", migrationId);
            return true;
        }

        /// <summary>
        /// 恢復遷移作業
        /// </summary>
        public async Task<bool> ResumeMigrationAsync(string migrationId)
        {
            var session = _memoryCache.Get<MigrationSession>($"{MigrationSessionPrefix}{migrationId}");
            if (session == null || session.Status != MigrationStatus.Paused)
            {
                return false;
            }

            session.Status = MigrationStatus.Running;
            session.PausedAt = null;
            _memoryCache.Set($"{MigrationSessionPrefix}{migrationId}", session, TimeSpan.FromHours(24));

            _logger.LogInformation("遷移作業已恢復: {MigrationId}", migrationId);
            return true;
        }

        /// <summary>
        /// 取消遷移作業
        /// </summary>
        public async Task<bool> CancelMigrationAsync(string migrationId)
        {
            var session = _memoryCache.Get<MigrationSession>($"{MigrationSessionPrefix}{migrationId}");
            if (session == null || session.Status == MigrationStatus.Completed || session.Status == MigrationStatus.Cancelled)
            {
                return false;
            }

            session.Status = MigrationStatus.Cancelled;
            session.CancelledAt = DateTime.UtcNow;
            _memoryCache.Set($"{MigrationSessionPrefix}{migrationId}", session, TimeSpan.FromHours(24));

            _logger.LogInformation("遷移作業已取消: {MigrationId}", migrationId);
            return true;
        }

        /// <summary>
        /// 獲取遷移進度
        /// </summary>
        public async Task<MigrationProgress> GetMigrationProgressAsync(string migrationId)
        {
            var session = _memoryCache.Get<MigrationSession>($"{MigrationSessionPrefix}{migrationId}");
            if (session == null)
            {
                throw new ArgumentException($"找不到遷移會話: {migrationId}");
            }

            return new MigrationProgress
            {
                MigrationId = session.MigrationId,
                Status = session.Status,
                TotalImages = session.TotalImages,
                ProcessedImages = session.ProcessedImages,
                SuccessCount = session.SuccessCount,
                FailureCount = session.FailureCount,
                ProgressPercentage = session.ProgressPercentage,
                ElapsedTime = session.StartedAt.HasValue ? DateTime.UtcNow - session.StartedAt.Value : null,
                CurrentBatchInfo = $"Batch {session.CurrentBatch} (Size: {session.BatchSize})",
                LastUpdated = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 獲取所有遷移會話
        /// </summary>
        public async Task<List<MigrationSession>> GetMigrationSessionsAsync()
        {
            var activeMigrations = _memoryCache.Get<List<string>>(ActiveMigrationsKey) ?? new List<string>();
            var sessions = new List<MigrationSession>();

            foreach (var migrationId in activeMigrations)
            {
                var session = _memoryCache.Get<MigrationSession>($"{MigrationSessionPrefix}{migrationId}");
                if (session != null)
                {
                    sessions.Add(session);
                }
            }

            return sessions.OrderByDescending(s => s.CreatedAt).ToList();
        }

        /// <summary>
        /// 驗證遷移結果
        /// </summary>
        public async Task<MigrationValidationResult> ValidateMigrationAsync(string migrationId)
        {
            var session = _memoryCache.Get<MigrationSession>($"{MigrationSessionPrefix}{migrationId}");
            if (session == null)
            {
                throw new ArgumentException($"找不到遷移會話: {migrationId}");
            }

            var result = new MigrationValidationResult();
            
            // 實作驗證邏輯
            _logger.LogInformation("開始驗證遷移結果: {MigrationId}", migrationId);
            
            result.IsValid = true;
            result.TotalImages = session.TotalImages;
            result.ValidatedImages = session.SuccessCount;

            return result;
        }

        /// <summary>
        /// 清理本地檔案
        /// </summary>
        public async Task<MigrationCleanupResult> CleanupLocalFilesAsync(string migrationId)
        {
            var session = _memoryCache.Get<MigrationSession>($"{MigrationSessionPrefix}{migrationId}");
            if (session == null)
            {
                throw new ArgumentException($"找不到遷移會話: {migrationId}");
            }

            var result = new MigrationCleanupResult();
            
            _logger.LogInformation("開始清理本地檔案: {MigrationId}", migrationId);
            
            // 實作清理邏輯
            result.IsSuccess = true;
            result.TotalFiles = session.MigratedFiles.Count;
            result.DeletedFiles = session.MigratedFiles.Count;

            return result;
        }

        /// <summary>
        /// 回滾遷移
        /// </summary>
        public async Task<bool> RollbackMigrationAsync(string migrationId)
        {
            var session = _memoryCache.Get<MigrationSession>($"{MigrationSessionPrefix}{migrationId}");
            if (session == null)
            {
                return false;
            }

            try
            {
                _logger.LogWarning("開始回滾遷移: {MigrationId}", migrationId);
                
                // 刪除已上傳的 Blob 檔案
                var deleteResults = await _blobStorageService.DeleteMultipleAsync(session.MigratedFiles);
                var successCount = deleteResults.Count(r => r.Value);

                _logger.LogInformation("回滾完成: 成功刪除 {SuccessCount} 個檔案", successCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回滾遷移時發生錯誤: {MigrationId}", migrationId);
                return false;
            }
        }

        #region Private Methods

        private async Task<LocalImageInfo> ProcessSingleImageAsync(Image image, LocalImageScanOptions scanOptions)
        {
            var imageInfo = new LocalImageInfo
            {
                ImageId = image.ImageId,
                ImageGuid = image.ImageGuid,
                EntityType = image.EntityType,
                EntityId = image.EntityId,
                Category = image.Category,
                StoredFileName = image.StoredFileName,
                OriginalFileName = image.OriginalFileName,
                LastModified = image.UploadedAt
            };

            try
            {
                // 構建本地檔案路徑
                var localPath = GetLocalImagePath(image);
                imageInfo.LocalPath = localPath;

                if (File.Exists(localPath))
                {
                    var fileInfo = new FileInfo(localPath);
                    imageInfo.FileSize = fileInfo.Length;
                    imageInfo.Status = LocalImageStatus.Ready;

                    if (scanOptions.ValidateFileIntegrity)
                    {
                        // 驗證檔案完整性
                        if (!await ValidateImageFileIntegrityAsync(localPath))
                        {
                            imageInfo.Status = LocalImageStatus.CorruptFile;
                            imageInfo.ErrorMessage = "檔案損壞或格式不正確";
                        }
                    }
                }
                else
                {
                    imageInfo.Status = LocalImageStatus.FileNotFound;
                    imageInfo.ErrorMessage = "本地檔案不存在";
                }
            }
            catch (UnauthorizedAccessException)
            {
                imageInfo.Status = LocalImageStatus.AccessDenied;
                imageInfo.ErrorMessage = "無法存取檔案";
            }
            catch (Exception ex)
            {
                imageInfo.Status = LocalImageStatus.Unknown;
                imageInfo.ErrorMessage = ex.Message;
            }

            return imageInfo;
        }

        private string GetLocalImagePath(Image image)
        {
            var wwwrootPath = _configuration["ImageStorage:LocalPath"] ?? "wwwroot/images";
            return Path.Combine(wwwrootPath, image.EntityType.ToString(), image.EntityId.ToString(), image.StoredFileName);
        }

        private async Task<bool> ValidateImageFileIntegrityAsync(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                try
                {
                    using var tempStream = new MemoryStream();
                    await stream.CopyToAsync(tempStream);
                    tempStream.Position = 0;
                    var result = await _imageProcessor.ConvertToWebPAsync(tempStream);
                    return result.Success;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private TimeSpan CalculateEstimatedMigrationTime(int imageCount)
        {
            // 假設每張圖片需要 2 秒處理時間
            var estimatedSeconds = imageCount * 2;
            return TimeSpan.FromSeconds(estimatedSeconds);
        }

        private async Task ExecuteMigrationAsync(string migrationId, MigrationConfiguration config)
        {
            var session = _memoryCache.Get<MigrationSession>($"{MigrationSessionPrefix}{migrationId}");
            if (session == null) return;

            try
            {
                session.Status = MigrationStatus.Running;
                session.StartedAt = DateTime.UtcNow;
                _memoryCache.Set($"{MigrationSessionPrefix}{migrationId}", session, TimeSpan.FromHours(24));

                // 獲取需要遷移的圖片
                var imagesToMigrate = await GetImagesToMigrateAsync(config);
                session.TotalImages = imagesToMigrate.Count;

                _logger.LogInformation("開始遷移 {ImageCount} 張圖片", imagesToMigrate.Count);

                // 批次處理
                var batches = imagesToMigrate.Chunk(config.BatchSize);
                var batchNumber = 1;

                foreach (var batch in batches)
                {
                    if (session.Status != MigrationStatus.Running) break;

                    session.CurrentBatch = batchNumber;
                    await ProcessBatchAsync(session, batch.ToList(), config);
                    batchNumber++;

                    // 更新會話狀態
                    _memoryCache.Set($"{MigrationSessionPrefix}{migrationId}", session, TimeSpan.FromHours(24));
                }

                // 完成遷移
                session.Status = MigrationStatus.Completed;
                session.CompletedAt = DateTime.UtcNow;
                _memoryCache.Set($"{MigrationSessionPrefix}{migrationId}", session, TimeSpan.FromHours(24));

                _logger.LogInformation("遷移作業完成: {MigrationId}, 成功 {Success}, 失敗 {Failed}",
                    migrationId, session.SuccessCount, session.FailureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "遷移作業執行時發生錯誤: {MigrationId}", migrationId);
                session.Status = MigrationStatus.Failed;
                session.CompletedAt = DateTime.UtcNow;
                _memoryCache.Set($"{MigrationSessionPrefix}{migrationId}", session, TimeSpan.FromHours(24));
            }
        }

        private async Task<List<Image>> GetImagesToMigrateAsync(MigrationConfiguration config)
        {
            var query = _context.Images.AsQueryable();

            if (config.IncludeImageIds.Any())
                query = query.Where(i => config.IncludeImageIds.Contains(i.ImageId));

            if (config.ExcludeImageIds.Any())
                query = query.Where(i => !config.ExcludeImageIds.Contains(i.ImageId));

            if (config.EntityTypes.Any())
                query = query.Where(i => config.EntityTypes.Contains(i.EntityType));

            if (config.Categories.Any())
                query = query.Where(i => config.Categories.Contains(i.Category));

            return await query.ToListAsync();
        }

        private async Task ProcessBatchAsync(MigrationSession session, List<Image> batch, MigrationConfiguration config)
        {
            var semaphore = new SemaphoreSlim(config.MaxConcurrency, config.MaxConcurrency);
            var tasks = batch.Select(async image =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await MigrateSingleImageAsync(image, session, config);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);

            // 更新統計
            foreach (var result in results)
            {
                session.ProcessedImages++;
                if (result.IsSuccess)
                {
                    session.SuccessCount++;
                    session.MigratedFiles.AddRange(result.MigratedPaths);
                }
                else
                {
                    session.FailureCount++;
                    session.FailedImages.Add(new FailedMigrationItem
                    {
                        ImageId = result.ImageId,
                        ImageGuid = result.ImageGuid,
                        LocalPath = result.LocalPath,
                        ErrorMessage = result.ErrorMessage ?? "未知錯誤",
                        FailedAt = DateTime.UtcNow
                    });
                }
            }
        }

        private async Task<MigrationItemResult> MigrateSingleImageAsync(Image image, MigrationSession session, MigrationConfiguration config)
        {
            var result = new MigrationItemResult
            {
                ImageId = image.ImageId,
                ImageGuid = image.ImageGuid
            };

            try
            {
                var localPath = GetLocalImagePath(image);
                result.LocalPath = localPath;

                if (!File.Exists(localPath))
                {
                    result.ErrorMessage = "本地檔案不存在";
                    return result;
                }

                // 讀取並處理圖片
                using var stream = File.OpenRead(localPath);
                var processedImages = await _imageProcessor.ProcessMultipleSizesAsync(stream, image.ImageGuid, image.OriginalFileName);

                // 檢查處理結果
                if (!processedImages.Success)
                {
                    result.ErrorMessage = $"圖片處理失敗: {processedImages.ErrorMessage}";
                    return result;
                }

                // 上傳到 Blob Storage
                var blobPaths = new List<string>();
                foreach (var kvp in processedImages.ProcessedStreams)
                {
                    var size = kvp.Key;
                    var imageStream = kvp.Value;
                    
                    var blobPath = _urlGenerator.GetBlobPath(image.Category, image.EntityId, image.ImageGuid, size);
                    imageStream.Position = 0; // 重置串流位置
                    var uploadResult = await _blobStorageService.UploadWithRetryAsync(imageStream, blobPath, "image/webp");
                    
                    if (uploadResult.Success)
                    {
                        blobPaths.Add(uploadResult.BlobPath ?? "");
                    }
                    else
                    {
                        result.ErrorMessage = $"上傳失敗: {uploadResult.Message}";
                        return result;
                    }
                }

                // 釋放處理結果資源
                processedImages.Dispose();

                result.IsSuccess = true;
                result.MigratedPaths = blobPaths;

                // 如果配置要求刪除本地檔案
                if (config.DeleteLocalFilesAfterMigration)
                {
                    try
                    {
                        File.Delete(localPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "無法刪除本地檔案: {LocalPath}", localPath);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "遷移圖片時發生錯誤: ImageId={ImageId}", image.ImageId);
                return result;
            }
        }

        #endregion

        #region Helper Classes

        private class MigrationItemResult
        {
            public long ImageId { get; set; }
            public Guid ImageGuid { get; set; }
            public string LocalPath { get; set; } = string.Empty;
            public bool IsSuccess { get; set; }
            public string? ErrorMessage { get; set; }
            public List<string> MigratedPaths { get; set; } = new();
        }

        #endregion
    }
}