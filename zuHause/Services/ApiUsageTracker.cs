using Microsoft.EntityFrameworkCore;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Services
{
    /// <summary>
    /// Google Maps API 使用量追蹤服務
    /// </summary>
    public class ApiUsageTracker : IApiUsageTracker
    {
        private readonly ZuHauseContext _context;
        private readonly ILogger<ApiUsageTracker> _logger;
        
        // Google Maps API 每日限制配置
        private static readonly Dictionary<string, int> DailyLimits = new()
        {
            { "Geocoding", 300 },
            { "Places", 300 },
            { "DistanceMatrix", 300 }
        };

        public ApiUsageTracker(ZuHauseContext context, ILogger<ApiUsageTracker> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 檢查指定 API 是否可以使用（未超過每日限制）
        /// </summary>
        public async Task<bool> CanUseApiAsync(string apiType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiType))
                {
                    _logger.LogWarning("API類型為空或null");
                    return false;
                }

                if (!DailyLimits.ContainsKey(apiType))
                {
                    _logger.LogWarning("未知的API類型: {ApiType}", apiType);
                    return false;
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var today = DateTime.Today;
                
                var usage = await _context.GoogleMapsApiUsages
                    .FirstOrDefaultAsync(u => u.ApiType == apiType && u.RequestDate == today, cts.Token);

                if (usage == null)
                {
                    // 沒有今日記錄，可以使用
                    return true;
                }

                var limit = DailyLimits[apiType];
                var canUse = usage.RequestCount < limit && !usage.IsLimitReached;
                
                _logger.LogDebug("API使用量檢查 - {ApiType}: {Current}/{Limit}, 可用: {CanUse}", 
                    apiType, usage.RequestCount, limit, canUse);
                    
                return canUse;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("API使用量檢查超時 - {ApiType}", apiType);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查API使用量時發生錯誤 - {ApiType}", apiType);
                return false;
            }
        }

        /// <summary>
        /// 記錄 API 使用量
        /// </summary>
        public async Task RecordApiUsageAsync(string apiType, decimal cost = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiType))
                {
                    _logger.LogWarning("嘗試記錄空的API類型");
                    return;
                }

                if (!DailyLimits.ContainsKey(apiType))
                {
                    _logger.LogWarning("嘗試記錄未知的API類型: {ApiType}", apiType);
                    return;
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var today = DateTime.Today;
                
                var usage = await _context.GoogleMapsApiUsages
                    .FirstOrDefaultAsync(u => u.ApiType == apiType && u.RequestDate == today, cts.Token);

                if (usage == null)
                {
                    // 建立新的記錄
                    usage = new GoogleMapsApiUsage
                    {
                        ApiType = apiType,
                        RequestDate = today,
                        RequestCount = 1,
                        EstimatedCost = cost,
                        IsLimitReached = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.GoogleMapsApiUsages.Add(usage);
                }
                else
                {
                    // 更新現有記錄
                    usage.RequestCount++;
                    usage.EstimatedCost += cost;
                    
                    // 檢查是否達到限制
                    var limit = DailyLimits[apiType];
                    if (usage.RequestCount >= limit)
                    {
                        usage.IsLimitReached = true;
                        _logger.LogWarning("API每日限制已達到 - {ApiType}: {Count}/{Limit}", 
                            apiType, usage.RequestCount, limit);
                    }
                }

                await _context.SaveChangesAsync(cts.Token);
                
                _logger.LogDebug("API使用量記錄更新 - {ApiType}: {Count}, 成本: {Cost}", 
                    apiType, usage.RequestCount, cost);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("記錄API使用量超時 - {ApiType}", apiType);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄API使用量時發生錯誤 - {ApiType}, 成本: {Cost}", apiType, cost);
                throw;
            }
        }

        /// <summary>
        /// 獲取指定日期的 API 使用次數
        /// </summary>
        public async Task<int> GetDailyUsageAsync(string apiType, DateTime? date = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiType))
                {
                    _logger.LogWarning("查詢API使用量時，API類型為空");
                    return 0;
                }

                var queryDate = date?.Date ?? DateTime.Today;
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                var usage = await _context.GoogleMapsApiUsages
                    .FirstOrDefaultAsync(u => u.ApiType == apiType && u.RequestDate == queryDate, cts.Token);

                var count = usage?.RequestCount ?? 0;
                
                _logger.LogDebug("API使用量查詢 - {ApiType} ({Date}): {Count}", 
                    apiType, queryDate.ToString("yyyy-MM-dd"), count);
                    
                return count;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("查詢API使用量超時 - {ApiType}", apiType);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢API使用量時發生錯誤 - {ApiType}, 日期: {Date}", apiType, date);
                return 0;
            }
        }

        /// <summary>
        /// 檢查指定 API 是否已達每日限制
        /// </summary>
        public async Task<bool> IsLimitReachedAsync(string apiType, DateTime? date = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiType))
                {
                    return true; // 安全考量，未知API類型視為已達限制
                }

                if (!DailyLimits.ContainsKey(apiType))
                {
                    _logger.LogWarning("檢查限制時遇到未知API類型: {ApiType}", apiType);
                    return true; // 安全考量
                }

                var queryDate = date?.Date ?? DateTime.Today;
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                var usage = await _context.GoogleMapsApiUsages
                    .FirstOrDefaultAsync(u => u.ApiType == apiType && u.RequestDate == queryDate, cts.Token);

                if (usage == null)
                {
                    return false; // 沒有記錄，未達限制
                }

                var limit = DailyLimits[apiType];
                var isReached = usage.IsLimitReached || usage.RequestCount >= limit;
                
                _logger.LogDebug("API限制檢查 - {ApiType} ({Date}): {Current}/{Limit}, 已達限制: {IsReached}", 
                    apiType, queryDate.ToString("yyyy-MM-dd"), usage.RequestCount, limit, isReached);
                    
                return isReached;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("檢查API限制超時 - {ApiType}", apiType);
                return true; // 安全考量，超時視為已達限制
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查API限制時發生錯誤 - {ApiType}, 日期: {Date}", apiType, date);
                return true; // 安全考量
            }
        }
    }
}