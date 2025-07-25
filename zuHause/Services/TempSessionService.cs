using Microsoft.Extensions.Caching.Memory;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Services
{
    /// <summary>
    /// 臨時會話管理服務實作
    /// 提供 GUID + Cookie 穩定會話機制，支援表單合併提交的臨時圖片管理
    /// </summary>
    public class TempSessionService : ITempSessionService
    {
        private const string CookieName = "temp_session_id";
        private const string CacheKeyPrefix = "temp_images_";
        private const int SessionHours = 6;

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<TempSessionService> _logger;
        private readonly IWebHostEnvironment _environment;

        public TempSessionService(
            IMemoryCache memoryCache,
            ILogger<TempSessionService> logger,
            IWebHostEnvironment environment)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// 取得或建立臨時會話ID
        /// </summary>
        public string GetOrCreateTempSessionId(HttpContext context)
        {
            try
            {
                // 1. 嘗試從 Cookie 讀取現有 ID
                if (context.Request.Cookies.TryGetValue(CookieName, out var existingId) 
                    && !string.IsNullOrEmpty(existingId) 
                    && existingId.Length == 32)
                {
                    _logger.LogDebug("找到現有臨時會話ID: {TempSessionId}", existingId);
                    return existingId;
                }

                // 2. 產生新的 GUID（32字元，無破折號）
                var newTempId = Guid.NewGuid().ToString("N");
                _logger.LogInformation("建立新的臨時會話ID: {TempSessionId}", newTempId);

                // 3. 環境感知的 Cookie 安全設定
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,                                           // 防止 XSS 攻擊
                    Secure = !_environment.IsDevelopment(),                   // 開發環境允許 HTTP
                    SameSite = SameSiteMode.Lax,                              // CSRF 保護
                    Expires = DateTimeOffset.UtcNow.AddHours(SessionHours),   // 6小時過期
                    Path = "/"                                                // 全站有效
                };

                // 4. 設定 Cookie
                context.Response.Cookies.Append(CookieName, newTempId, cookieOptions);
                
                // 5. 初始化 Memory Cache 空列表
                var cacheKey = CacheKeyPrefix + newTempId;
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(SessionHours),
                    SlidingExpiration = TimeSpan.FromHours(1), // 1小時滑動過期
                    Priority = CacheItemPriority.Normal
                };

                _memoryCache.Set(cacheKey, new List<TempImageInfo>(), cacheOptions);
                _logger.LogDebug("初始化臨時會話快取: {CacheKey}", cacheKey);

                return newTempId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立臨時會話ID失敗");
                throw;
            }
        }

        /// <summary>
        /// 驗證臨時會話ID是否有效
        /// </summary>
        public async Task<bool> IsValidTempSessionAsync(string tempSessionId)
        {
            if (string.IsNullOrEmpty(tempSessionId) || tempSessionId.Length != 32)
            {
                return false;
            }

            var cacheKey = CacheKeyPrefix + tempSessionId;
            var exists = _memoryCache.TryGetValue(cacheKey, out _);
            
            _logger.LogDebug("臨時會話驗證: {TempSessionId}, 有效: {IsValid}", tempSessionId, exists);
            return await Task.FromResult(exists);
        }

        /// <summary>
        /// 失效臨時會話
        /// </summary>
        public async Task InvalidateTempSessionAsync(string tempSessionId)
        {
            if (string.IsNullOrEmpty(tempSessionId))
            {
                return;
            }

            var cacheKey = CacheKeyPrefix + tempSessionId;
            _memoryCache.Remove(cacheKey);
            
            _logger.LogInformation("臨時會話已失效: {TempSessionId}", tempSessionId);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 取得臨時會話中的圖片資訊列表
        /// </summary>
        public async Task<List<TempImageInfo>> GetTempImagesAsync(string tempSessionId)
        {
            if (string.IsNullOrEmpty(tempSessionId))
            {
                return new List<TempImageInfo>();
            }

            var cacheKey = CacheKeyPrefix + tempSessionId;
            var tempImages = _memoryCache.Get<List<TempImageInfo>>(cacheKey) ?? new List<TempImageInfo>();
            
            _logger.LogDebug("取得臨時圖片列表: {TempSessionId}, 數量: {Count}", tempSessionId, tempImages.Count);
            return await Task.FromResult(tempImages);
        }

        /// <summary>
        /// 新增圖片到臨時會話
        /// </summary>
        public async Task AddTempImageAsync(string tempSessionId, TempImageInfo tempImageInfo)
        {
            if (string.IsNullOrEmpty(tempSessionId) || tempImageInfo == null)
            {
                throw new ArgumentException("臨時會話ID和圖片資訊不能為空");
            }

            // 確保 TempSessionId 一致
            tempImageInfo.TempSessionId = tempSessionId;

            var cacheKey = CacheKeyPrefix + tempSessionId;
            var tempImages = _memoryCache.Get<List<TempImageInfo>>(cacheKey) ?? new List<TempImageInfo>();
            
            // 檢查是否已存在相同的圖片GUID
            if (tempImages.Any(img => img.ImageGuid == tempImageInfo.ImageGuid))
            {
                _logger.LogWarning("圖片已存在於臨時會話中: {ImageGuid}", tempImageInfo.ImageGuid);
                return;
            }

            tempImages.Add(tempImageInfo);
            
            // 更新快取，延長過期時間
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(SessionHours),
                SlidingExpiration = TimeSpan.FromHours(1),
                Priority = CacheItemPriority.Normal
            };

            _memoryCache.Set(cacheKey, tempImages, cacheOptions);
            
            _logger.LogInformation("新增臨時圖片: {TempSessionId}, {ImageGuid}, {FileName}", 
                tempSessionId, tempImageInfo.ImageGuid, tempImageInfo.OriginalFileName);
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// 從臨時會話中移除圖片
        /// </summary>
        public async Task<bool> RemoveTempImageAsync(string tempSessionId, Guid imageGuid)
        {
            if (string.IsNullOrEmpty(tempSessionId) || imageGuid == Guid.Empty)
            {
                return false;
            }

            var cacheKey = CacheKeyPrefix + tempSessionId;
            var tempImages = _memoryCache.Get<List<TempImageInfo>>(cacheKey) ?? new List<TempImageInfo>();
            
            var removedCount = tempImages.RemoveAll(img => img.ImageGuid == imageGuid);
            
            if (removedCount > 0)
            {
                // 更新快取
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(SessionHours),
                    SlidingExpiration = TimeSpan.FromHours(1),
                    Priority = CacheItemPriority.Normal
                };

                _memoryCache.Set(cacheKey, tempImages, cacheOptions);
                
                _logger.LogInformation("移除臨時圖片: {TempSessionId}, {ImageGuid}", tempSessionId, imageGuid);
                return await Task.FromResult(true);
            }

            _logger.LogWarning("找不到要移除的臨時圖片: {TempSessionId}, {ImageGuid}", tempSessionId, imageGuid);
            return await Task.FromResult(false);
        }

        /// <summary>
        /// 清理過期的臨時會話
        /// 注意: IMemoryCache 會自動清理過期項目，此方法主要用於手動觸發清理
        /// </summary>
        public async Task<int> CleanupExpiredSessionsAsync()
        {
            // IMemoryCache 會自動處理過期項目，這裡主要是記錄日誌
            _logger.LogInformation("執行臨時會話清理檢查");
            
            // 實際的清理由 IMemoryCache 自動處理
            // 這裡返回 0，因為我們無法精確統計自動清理的數量
            return await Task.FromResult(0);
        }
    }
}