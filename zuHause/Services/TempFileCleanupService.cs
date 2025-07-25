using Microsoft.Extensions.Caching.Memory;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Services
{
    /// <summary>
    /// 臨時檔案清理服務實作
    /// 提供定時清理過期臨時檔案和快取的背景服務
    /// </summary>
    public class TempFileCleanupService : BackgroundService, ITempFileCleanupService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<TempFileCleanupService> _logger;

        // 統計資訊
        private CleanupStatistics _statistics = new CleanupStatistics();
        private readonly object _statsLock = new object();

        public TempFileCleanupService(
            IServiceScopeFactory serviceScopeFactory,
            IMemoryCache memoryCache,
            ILogger<TempFileCleanupService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _memoryCache = memoryCache;
            _logger = logger;

            // 初始化統計資訊
            lock (_statsLock)
            {
                _statistics.IsRunning = true;
                _statistics.NextCleanupTime = DateTime.UtcNow.AddHours(1);
            }
        }

        /// <summary>
        /// 背景服務主要執行邏輯
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("臨時檔案清理背景服務已啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 執行清理
                    var result = await ExecuteCleanupAsync();

                    // 更新統計資訊
                    UpdateStatistics(result);

                    // 記錄清理結果
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("定時清理完成 - Blob檔案: {BlobCount}, 快取項目: {CacheCount}, 耗時: {Duration}ms",
                            result.BlobFilesDeleted, result.CacheEntriesDeleted, result.Duration?.TotalMilliseconds);
                    }
                    else
                    {
                        _logger.LogError("定時清理失敗: {Error}", result.ErrorMessage);
                    }

                    // 等待1小時
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("臨時檔案清理背景服務收到停止信號");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "臨時檔案清理背景服務發生未預期錯誤");

                    // 更新錯誤統計
                    lock (_statsLock)
                    {
                        _statistics.LastErrorMessage = ex.Message;
                        _statistics.LastErrorTime = DateTime.UtcNow;
                    }

                    // 等待較短時間後重試
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }

            // 更新統計資訊
            lock (_statsLock)
            {
                _statistics.IsRunning = false;
                _statistics.NextCleanupTime = null;
            }

            _logger.LogInformation("臨時檔案清理背景服務已停止");
        }

        /// <summary>
        /// 手動執行完整清理
        /// </summary>
        public async Task<CleanupResult> ExecuteCleanupAsync(int olderThanHours = 6)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogDebug("開始執行臨時檔案清理 - 清理 {Hours} 小時前的檔案", olderThanHours);

            try
            {
                // 1. 清理過期的 Blob 檔案
                var blobFilesDeleted = await CleanupExpiredBlobFilesAsync(olderThanHours);

                // 2. 清理過期的快取項目
                var cacheEntriesDeleted = await CleanupExpiredCacheEntriesAsync();

                var result = CleanupResult.CreateSuccess(blobFilesDeleted, cacheEntriesDeleted);
                result.StartedAt = startTime;

                _logger.LogInformation("清理操作完成 - Blob檔案: {BlobCount}, 快取項目: {CacheCount}, 總耗時: {Duration}ms",
                    blobFilesDeleted, cacheEntriesDeleted, result.Duration?.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理操作失敗");
                
                var result = CleanupResult.CreateFailure($"清理操作失敗: {ex.Message}");
                result.StartedAt = startTime;
                return result;
            }
        }

        /// <summary>
        /// 清理過期的 Blob 檔案
        /// </summary>
        public async Task<int> CleanupExpiredBlobFilesAsync(int olderThanHours = 6)
        {
            try
            {
                _logger.LogDebug("開始清理過期的 Blob 檔案");

                using var scope = _serviceScopeFactory.CreateScope();
                var blobStorageService = scope.ServiceProvider.GetRequiredService<IBlobStorageService>();
                
                var deletedCount = await blobStorageService.CleanupExpiredTempFilesAsync(olderThanHours);

                _logger.LogDebug("Blob 檔案清理完成 - 清理了 {Count} 個檔案", deletedCount);
                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理 Blob 檔案時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 清理過期的快取項目
        /// </summary>
        public async Task<int> CleanupExpiredCacheEntriesAsync()
        {
            try
            {
                _logger.LogDebug("開始清理過期的快取項目");

                using var scope = _serviceScopeFactory.CreateScope();
                var tempSessionService = scope.ServiceProvider.GetRequiredService<ITempSessionService>();
                
                var deletedCount = await tempSessionService.CleanupExpiredSessionsAsync();

                _logger.LogDebug("快取項目清理完成 - 清理了 {Count} 個項目", deletedCount);
                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理快取項目時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 取得清理統計資訊
        /// </summary>
        public async Task<CleanupStatistics> GetCleanupStatisticsAsync()
        {
            lock (_statsLock)
            {
                // 返回統計資訊的副本
                return new CleanupStatistics
                {
                    LastCleanupTime = _statistics.LastCleanupTime,
                    NextCleanupTime = _statistics.NextCleanupTime,
                    TodayCleanupCount = _statistics.TodayCleanupCount,
                    TodayFilesDeleted = _statistics.TodayFilesDeleted,
                    TodayCacheEntriesDeleted = _statistics.TodayCacheEntriesDeleted,
                    IsRunning = _statistics.IsRunning,
                    CleanupIntervalHours = _statistics.CleanupIntervalHours,
                    FileExpirationHours = _statistics.FileExpirationHours,
                    LastErrorMessage = _statistics.LastErrorMessage,
                    LastErrorTime = _statistics.LastErrorTime
                };
            }
        }

        /// <summary>
        /// 更新統計資訊
        /// </summary>
        private void UpdateStatistics(CleanupResult result)
        {
            lock (_statsLock)
            {
                var now = DateTime.UtcNow;
                var today = now.Date;

                // 重置當日統計（如果是新的一天）
                if (_statistics.LastCleanupTime?.Date != today)
                {
                    _statistics.TodayCleanupCount = 0;
                    _statistics.TodayFilesDeleted = 0;
                    _statistics.TodayCacheEntriesDeleted = 0;
                }

                // 更新統計
                _statistics.LastCleanupTime = now;
                _statistics.NextCleanupTime = now.AddHours(1);
                _statistics.TodayCleanupCount++;
                _statistics.TodayFilesDeleted += result.BlobFilesDeleted;
                _statistics.TodayCacheEntriesDeleted += result.CacheEntriesDeleted;

                // 更新錯誤資訊
                if (!result.IsSuccess)
                {
                    _statistics.LastErrorMessage = result.ErrorMessage;
                    _statistics.LastErrorTime = now;
                }
            }
        }

        /// <summary>
        /// 停止服務時的清理工作
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("正在停止臨時檔案清理背景服務...");

            await base.StopAsync(cancellationToken);

            lock (_statsLock)
            {
                _statistics.IsRunning = false;
            }

            _logger.LogInformation("臨時檔案清理背景服務已停止");
        }
    }
}