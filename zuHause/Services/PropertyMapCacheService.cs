using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using zuHause.DTOs;
using zuHause.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// 房源地圖資料快取服務實作
    /// 整合 IMemoryCache + Azure Blob Storage 雙層快取機制
    /// </summary>
    public class PropertyMapCacheService : IPropertyMapCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<PropertyMapCacheService> _logger;
        private readonly BlobContainerClient _blobContainerClient;
        
        // 快取設定
        private static readonly TimeSpan MemoryCacheExpiry = TimeSpan.FromHours(2);
        private static readonly TimeSpan BlobCacheExpiry = TimeSpan.FromDays(30);
        
        public PropertyMapCacheService(
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<PropertyMapCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // 初始化 Blob Storage 用戶端
            var connectionString = configuration.GetValue<string>("AzureStorage:BlobConnectionString");
            var containerName = configuration.GetValue<string>("AzureStorage:MapCacheContainer");
            
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
            {
                throw new InvalidOperationException("Azure Storage 配置不完整，請檢查 appsettings.json");
            }
            
            _blobContainerClient = new BlobContainerClient(connectionString, containerName);
        }

        /// <summary>
        /// 從快取中取得房源地圖資料（雙層快取：記憶體 -> Blob Storage）
        /// </summary>
        public async Task<PropertyMapDto?> GetFromCacheAsync(int propertyId)
        {
            var memoryCacheKey = GetMemoryCacheKey(propertyId);

            try
            {
                // 1. 先檢查記憶體快取
                if (_memoryCache.TryGetValue(memoryCacheKey, out PropertyMapDto? memoryData))
                {
                    _logger.LogInformation("房源 {PropertyId} 記憶體快取命中", propertyId);
                    return memoryData;
                }

                // 2. 檢查 Blob Storage 快取 (待階段2實作)
                var blobData = await LoadFromBlobStorageAsync(propertyId);
                if (blobData != null && !IsExpired(blobData))
                {
                    _logger.LogInformation("房源 {PropertyId} Blob 快取命中，載入至記憶體", propertyId);
                    
                    // 載入到記憶體快取
                    _memoryCache.Set(memoryCacheKey, blobData, MemoryCacheExpiry);
                    return blobData;
                }

                _logger.LogInformation("房源 {PropertyId} 快取未命中", propertyId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "讀取房源 {PropertyId} 快取時發生錯誤", propertyId);
                return null;
            }
        }

        /// <summary>
        /// 將房源地圖資料儲存到快取（雙層儲存：記憶體 + Blob Storage）
        /// </summary>
        public async Task SetCacheAsync(int propertyId, PropertyMapDto data)
        {
            if (data == null) return;

            var memoryCacheKey = GetMemoryCacheKey(propertyId);

            try
            {
                // 設定快取時間戳記
                data.CachedAt = DateTime.UtcNow;
                data.ExpiresAt = DateTime.UtcNow.Add(BlobCacheExpiry);

                // 1. 儲存到記憶體快取
                _memoryCache.Set(memoryCacheKey, data, MemoryCacheExpiry);
                _logger.LogInformation("房源 {PropertyId} 已儲存至記憶體快取", propertyId);

                // 2. 儲存到 Blob Storage (待階段2實作)
                await SaveToBlobStorageAsync(propertyId, data);
                _logger.LogInformation("房源 {PropertyId} 已儲存至 Blob 快取", propertyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "儲存房源 {PropertyId} 快取時發生錯誤", propertyId);
            }
        }

        /// <summary>
        /// 清除指定房源的快取資料
        /// </summary>
        public async Task ClearCacheAsync(int propertyId)
        {
            var memoryCacheKey = GetMemoryCacheKey(propertyId);

            try
            {
                // 1. 清除記憶體快取
                _memoryCache.Remove(memoryCacheKey);
                _logger.LogInformation("房源 {PropertyId} 記憶體快取已清除", propertyId);

                // 2. 清除 Blob Storage 快取 (待階段2實作)
                await DeleteFromBlobStorageAsync(propertyId);
                _logger.LogInformation("房源 {PropertyId} Blob 快取已清除", propertyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除房源 {PropertyId} 快取時發生錯誤", propertyId);
            }
        }

        /// <summary>
        /// 檢查房源座標是否有變化，如有變化則清除快取
        /// </summary>
        public async Task<bool> CheckAndClearIfLocationChangedAsync(int propertyId, decimal currentLatitude, decimal currentLongitude)
        {
            try
            {
                var cachedData = await GetFromCacheAsync(propertyId);
                if (cachedData == null) return false;

                // 檢查座標是否有變化（允許微小的浮點數差異）
                const double tolerance = 0.000001; // 約1公尺的精度
                var latDiff = Math.Abs(cachedData.Latitude.GetValueOrDefault() - (double)currentLatitude);
                var lngDiff = Math.Abs(cachedData.Longitude.GetValueOrDefault() - (double)currentLongitude);

                if (latDiff > tolerance || lngDiff > tolerance)
                {
                    _logger.LogWarning("房源 {PropertyId} 座標已變化，清除快取。舊座標: ({OldLat}, {OldLng})，新座標: ({NewLat}, {NewLng})",
                        propertyId, cachedData.Latitude, cachedData.Longitude, currentLatitude, currentLongitude);
                    
                    await ClearCacheAsync(propertyId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查房源 {PropertyId} 座標變化時發生錯誤", propertyId);
                return false;
            }
        }

        #region Private Methods

        /// <summary>
        /// 產生記憶體快取鍵值
        /// </summary>
        private static string GetMemoryCacheKey(int propertyId)
        {
            return $"property:{propertyId}:map";
        }

        /// <summary>
        /// 檢查快取資料是否已過期
        /// </summary>
        private static bool IsExpired(PropertyMapDto data)
        {
            return data.ExpiresAt.HasValue && data.ExpiresAt.Value < DateTime.UtcNow;
        }

        /// <summary>
        /// 從 Blob Storage 載入快取資料
        /// </summary>
        private async Task<PropertyMapDto?> LoadFromBlobStorageAsync(int propertyId)
        {
            try
            {
                var blobName = GetBlobName(propertyId);
                var blobClient = _blobContainerClient.GetBlobClient(blobName);

                // 檢查 Blob 是否存在
                if (!await blobClient.ExistsAsync())
                {
                    return null;
                }

                // 下載 Blob 內容
                var response = await blobClient.DownloadContentAsync();
                var jsonContent = response.Value.Content.ToString();

                // 反序列化 JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<PropertyMapDto>(jsonContent, options);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從 Blob Storage 載入房源 {PropertyId} 快取時發生錯誤", propertyId);
                return null;
            }
        }

        /// <summary>
        /// 儲存資料到 Blob Storage
        /// </summary>
        private async Task SaveToBlobStorageAsync(int propertyId, PropertyMapDto data)
        {
            try
            {
                var blobName = GetBlobName(propertyId);
                var blobClient = _blobContainerClient.GetBlobClient(blobName);

                // 序列化為 JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var jsonContent = JsonSerializer.Serialize(data, options);

                // 上傳到 Blob Storage
                await blobClient.UploadAsync(
                    BinaryData.FromString(jsonContent), 
                    overwrite: true);

                _logger.LogInformation("房源 {PropertyId} 快取已儲存至 Blob: {BlobName}", propertyId, blobName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "儲存房源 {PropertyId} 快取至 Blob Storage 時發生錯誤", propertyId);
            }
        }

        /// <summary>
        /// 從 Blob Storage 刪除快取資料
        /// </summary>
        private async Task DeleteFromBlobStorageAsync(int propertyId)
        {
            try
            {
                var blobName = GetBlobName(propertyId);
                var blobClient = _blobContainerClient.GetBlobClient(blobName);

                // 刪除 Blob (如果存在)
                await blobClient.DeleteIfExistsAsync();
                
                _logger.LogInformation("房源 {PropertyId} Blob 快取已刪除: {BlobName}", propertyId, blobName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除房源 {PropertyId} Blob 快取時發生錯誤", propertyId);
            }
        }

        /// <summary>
        /// 產生 Blob 名稱
        /// </summary>
        private static string GetBlobName(int propertyId)
        {
            return $"property-{propertyId}-map.json";
        }

        #endregion
    }
}