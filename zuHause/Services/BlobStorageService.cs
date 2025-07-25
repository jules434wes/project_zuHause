using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Options;
using zuHause.Enums;

namespace zuHause.Services
{
    /// <summary>
    /// Blob Storage 操作服務實作
    /// 提供併發控制和自動重試機制的 Blob 操作功能
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobStorageOptions _options;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly SemaphoreSlim _uploadSemaphore;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(
            IOptions<BlobStorageOptions> options,
            ILogger<BlobStorageService> logger)
        {
            _options = options.Value;
            _logger = logger;
            _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            
            // 初始化併發控制信號量（最多3個併發上傳）
            _uploadSemaphore = new SemaphoreSlim(_options.MaxConcurrentUploads, _options.MaxConcurrentUploads);
        }

        /// <summary>
        /// 單一檔案上傳（包含重試機制）
        /// </summary>
        public async Task<BlobUploadResult> UploadWithRetryAsync(
            Stream stream, 
            string blobPath, 
            string contentType = "image/webp", 
            bool overwrite = true)
        {
            // 驗證輸入參數
            if (stream == null || stream.Length == 0)
            {
                return BlobUploadResult.CreateFailure("檔案串流不能為空");
            }

            if (string.IsNullOrEmpty(blobPath))
            {
                return BlobUploadResult.CreateFailure("Blob 路徑不能為空");
            }

            if (stream.Length > _options.MaxFileSizeBytes)
            {
                return BlobUploadResult.CreateFailure($"檔案大小超過限制 {_options.MaxFileSizeBytes / 1024 / 1024}MB");
            }

            // 併發控制
            await _uploadSemaphore.WaitAsync();
            
            try
            {
                return await UploadWithRetryInternalAsync(stream, blobPath, contentType, overwrite);
            }
            finally
            {
                _uploadSemaphore.Release();
            }
        }

        /// <summary>
        /// 內部重試上傳實作
        /// </summary>
        private async Task<BlobUploadResult> UploadWithRetryInternalAsync(
            Stream stream, 
            string blobPath, 
            string contentType, 
            bool overwrite)
        {
            Exception? lastException = null;
            var originalPosition = stream.Position;
            var fileSizeBytes = stream.Length;

            for (int attempt = 0; attempt <= _options.MaxRetryAttempts; attempt++)
            {
                try
                {
                    _logger.LogDebug("開始上傳 Blob: {BlobPath}, 嘗試: {Attempt}/{MaxAttempts}", 
                        blobPath, attempt + 1, _options.MaxRetryAttempts + 1);

                    var blobClient = _containerClient.GetBlobClient(blobPath);
                    
                    var uploadOptions = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                        Conditions = overwrite ? null : new BlobRequestConditions { IfNoneMatch = ETag.All }
                    };

                    await blobClient.UploadAsync(stream, uploadOptions);

                    var blobUrl = blobClient.Uri.ToString();
                    _logger.LogInformation("Blob 上傳成功: {BlobPath}, URL: {BlobUrl}, 嘗試次數: {Attempt}", 
                        blobPath, blobUrl, attempt + 1);

                    return BlobUploadResult.CreateSuccess(blobUrl, blobPath, fileSizeBytes, attempt);
                }
                catch (RequestFailedException ex) when (IsRetriableError(ex) && attempt < _options.MaxRetryAttempts)
                {
                    lastException = ex;
                    _logger.LogWarning("Blob 上傳失敗（可重試）: {BlobPath}, 錯誤: {Error}, 嘗試: {Attempt}/{MaxAttempts}", 
                        blobPath, ex.Message, attempt + 1, _options.MaxRetryAttempts + 1);

                    // 等待重試間隔
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    
                    // 重置串流位置
                    stream.Position = originalPosition;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Blob 上傳發生不可重試錯誤: {BlobPath}", blobPath);
                    return BlobUploadResult.CreateFailure($"上傳失敗: {ex.Message}", attempt);
                }
            }

            _logger.LogError("Blob 上傳重試 {MaxAttempts} 次後仍失敗: {BlobPath}, 最後錯誤: {Error}", 
                _options.MaxRetryAttempts, blobPath, lastException?.Message);
            
            return BlobUploadResult.CreateFailure($"重試 {_options.MaxRetryAttempts} 次後仍失敗: {lastException?.Message}", _options.MaxRetryAttempts);
        }

        /// <summary>
        /// 原子性多尺寸上傳
        /// </summary>
        public async Task<AtomicUploadResult> UploadMultipleSizesAsync(
            Dictionary<ImageSize, Stream> streams, 
            string basePath, 
            string contentType = "image/webp")
        {
            if (streams == null || streams.Count == 0)
            {
                return AtomicUploadResult.CreateFailure("沒有提供要上傳的檔案");
            }

            _logger.LogInformation("開始原子性多尺寸上傳: {BasePath}, 尺寸數量: {Count}", basePath, streams.Count);

            var results = new Dictionary<ImageSize, BlobUploadResult>();
            var uploadedPaths = new List<string>();

            try
            {
                // 逐一上傳各尺寸
                foreach (var (size, stream) in streams)
                {
                    var sizeName = GetSizeName(size);
                    var blobPath = $"{basePath}/{sizeName}";
                    
                    var result = await UploadWithRetryAsync(stream, blobPath, contentType);
                    results[size] = result;

                    if (result.Success)
                    {
                        uploadedPaths.Add(result.BlobPath!);
                        _logger.LogDebug("尺寸 {Size} 上傳成功: {BlobPath}", size, blobPath);
                    }
                    else
                    {
                        _logger.LogError("尺寸 {Size} 上傳失敗: {BlobPath}, 錯誤: {Error}", 
                            size, blobPath, result.Message);
                        
                        // 發生錯誤，回滾已上傳的檔案
                        await RollbackUploadsAsync(uploadedPaths);
                        return AtomicUploadResult.CreateFailure($"尺寸 {size} 上傳失敗，已回滾: {result.Message}", results);
                    }
                }

                _logger.LogInformation("原子性多尺寸上傳完成: {BasePath}, 成功數量: {SuccessCount}", 
                    basePath, results.Count);

                return AtomicUploadResult.CreateSuccess(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "原子性多尺寸上傳發生異常: {BasePath}", basePath);
                
                // 發生異常，回滾已上傳的檔案
                await RollbackUploadsAsync(uploadedPaths);
                return AtomicUploadResult.CreateFailure($"上傳異常，已回滾: {ex.Message}", results);
            }
        }

        /// <summary>
        /// 回滾上傳的檔案
        /// </summary>
        private async Task RollbackUploadsAsync(List<string> uploadedPaths)
        {
            if (uploadedPaths.Count == 0) return;

            _logger.LogWarning("開始回滾已上傳檔案，數量: {Count}", uploadedPaths.Count);

            foreach (var path in uploadedPaths)
            {
                try
                {
                    await DeleteAsync(path);
                    _logger.LogDebug("回滾刪除成功: {BlobPath}", path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "回滾刪除失敗: {BlobPath}", path);
                }
            }
        }

        /// <summary>
        /// 刪除 Blob 檔案
        /// </summary>
        public async Task<bool> DeleteAsync(string blobPath)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobPath);
                var result = await blobClient.DeleteIfExistsAsync();
                
                _logger.LogDebug("刪除 Blob: {BlobPath}, 結果: {Deleted}", blobPath, result.Value);
                return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除 Blob 失敗: {BlobPath}", blobPath);
                return false;
            }
        }

        /// <summary>
        /// 刪除多個 Blob 檔案
        /// </summary>
        public async Task<Dictionary<string, bool>> DeleteMultipleAsync(IEnumerable<string> blobPaths)
        {
            var results = new Dictionary<string, bool>();
            var tasks = blobPaths.Select(async path =>
            {
                var success = await DeleteAsync(path);
                lock (results)
                {
                    results[path] = success;
                }
            });

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 檢查 Blob 是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string blobPath)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobPath);
                var result = await blobClient.ExistsAsync();
                return result.Value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得 Blob 的中繼資料
        /// </summary>
        public async Task<BlobMetadata?> GetBlobInfoAsync(string blobPath)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobPath);
                var properties = await blobClient.GetPropertiesAsync();

                return new BlobMetadata
                {
                    BlobPath = blobPath,
                    BlobUrl = blobClient.Uri.ToString(),
                    SizeBytes = properties.Value.ContentLength,
                    ContentType = properties.Value.ContentType,
                    LastModified = properties.Value.LastModified,
                    CreatedOn = properties.Value.CreatedOn,
                    ETag = properties.Value.ETag.ToString()
                };
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        /// <summary>
        /// 移動 Blob 檔案
        /// </summary>
        public async Task<BlobUploadResult> MoveAsync(string sourcePath, string destinationPath, bool deleteSource = true)
        {
            try
            {
                var sourceBlobClient = _containerClient.GetBlobClient(sourcePath);
                var destinationBlobClient = _containerClient.GetBlobClient(destinationPath);

                // 複製檔案
                await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

                // 等待複製完成
                var copyStatus = CopyStatus.Pending;
                while (copyStatus == CopyStatus.Pending)
                {
                    await Task.Delay(100);
                    var properties = await destinationBlobClient.GetPropertiesAsync();
                    copyStatus = properties.Value.CopyStatus;
                }

                if (copyStatus != CopyStatus.Success)
                {
                    return BlobUploadResult.CreateFailure($"移動失敗：複製狀態為 {copyStatus}");
                }

                // 刪除來源檔案
                if (deleteSource)
                {
                    await sourceBlobClient.DeleteIfExistsAsync();
                }

                _logger.LogInformation("Blob 移動成功: {SourcePath} -> {DestinationPath}", sourcePath, destinationPath);
                return BlobUploadResult.CreateSuccess(destinationBlobClient.Uri.ToString(), destinationPath, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blob 移動失敗: {SourcePath} -> {DestinationPath}", sourcePath, destinationPath);
                return BlobUploadResult.CreateFailure($"移動失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次移動 Blob 檔案
        /// </summary>
        public async Task<Dictionary<string, BlobUploadResult>> MoveBatchAsync(
            Dictionary<string, string> moveOperations, 
            bool deleteSource = true)
        {
            var results = new Dictionary<string, BlobUploadResult>();
            var tasks = moveOperations.Select(async kvp =>
            {
                var result = await MoveAsync(kvp.Key, kvp.Value, deleteSource);
                lock (results)
                {
                    results[kvp.Key] = result;
                }
            });

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 清理過期的臨時檔案
        /// </summary>
        public async Task<int> CleanupExpiredTempFilesAsync(int olderThanHours = 6)
        {
            var cleanupCount = 0;
            var cutoffTime = DateTimeOffset.UtcNow.AddHours(-olderThanHours);

            try
            {
                _logger.LogInformation("開始清理過期臨時檔案，時間早於: {CutoffTime}", cutoffTime);

                await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: "temp/"))
                {
                    if (blobItem.Properties.CreatedOn < cutoffTime)
                    {
                        try
                        {
                            var deleted = await DeleteAsync(blobItem.Name);
                            if (deleted)
                            {
                                cleanupCount++;
                                _logger.LogDebug("清理過期臨時檔案: {BlobName}", blobItem.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "清理過期臨時檔案失敗: {BlobName}", blobItem.Name);
                        }
                    }
                }

                _logger.LogInformation("清理過期臨時檔案完成，清理數量: {CleanupCount}", cleanupCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理過期臨時檔案發生異常");
            }

            return cleanupCount;
        }

        /// <summary>
        /// 判斷是否為可重試的錯誤
        /// </summary>
        private static bool IsRetriableError(RequestFailedException ex)
        {
            // 可重試的 HTTP 狀態碼
            return ex.Status is 408 or 429 or 500 or 502 or 503 or 504;
        }

        /// <summary>
        /// 取得圖片尺寸名稱
        /// </summary>
        private static string GetSizeName(ImageSize size)
        {
            return size switch
            {
                ImageSize.Original => "original",
                ImageSize.Large => "large",
                ImageSize.Medium => "medium", 
                ImageSize.Thumbnail => "thumbnail",
                _ => throw new ArgumentException($"不支援的圖片尺寸: {size}")
            };
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            _uploadSemaphore?.Dispose();
        }
    }
}