using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Options;

namespace zuHause.Services
{
    /// <summary>
    /// Azure Blob Storage 連線測試實作
    /// </summary>
    public class BlobStorageConnectionTest : IBlobStorageConnectionTest
    {
        private readonly BlobStorageOptions _options;
        private readonly ILogger<BlobStorageConnectionTest> _logger;

        public BlobStorageConnectionTest(
            IOptions<BlobStorageOptions> options,
            ILogger<BlobStorageConnectionTest> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// 測試 Azure Blob Storage 連線
        /// </summary>
        public async Task<ConnectionTestResult> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("開始 Azure Blob Storage 連線測試");

                if (string.IsNullOrEmpty(_options.ConnectionString))
                {
                    return ConnectionTestResult.Failure("連線字串不能為空");
                }

                // 建立 Blob Service Client
                var blobServiceClient = new BlobServiceClient(_options.ConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);

                // 1. 測試 Container 是否存在
                var exists = await containerClient.ExistsAsync();
                if (!exists.Value)
                {
                    return ConnectionTestResult.Failure($"Container '{_options.ContainerName}' 不存在");
                }

                // 2. 測試上傳權限 - 建立測試檔案
                var testBlobName = $"connection-test-{Guid.NewGuid()}.txt";
                var testContent = "Azure Blob Storage 連線測試";
                var testBlob = containerClient.GetBlobClient(testBlobName);

                await testBlob.UploadAsync(new BinaryData(testContent), overwrite: true);
                _logger.LogInformation("測試檔案上傳成功: {BlobName}", testBlobName);

                // 3. 測試讀取權限
                var downloadResult = await testBlob.DownloadContentAsync();
                var downloadedContent = downloadResult.Value.Content.ToString();

                if (downloadedContent != testContent)
                {
                    return ConnectionTestResult.Failure("讀寫測試失敗：內容不一致");
                }

                // 4. 清理測試檔案
                await testBlob.DeleteIfExistsAsync();
                _logger.LogInformation("測試檔案清理完成: {BlobName}", testBlobName);

                _logger.LogInformation("Azure Blob Storage 連線測試成功");
                return ConnectionTestResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Blob Storage 連線測試失敗");
                return ConnectionTestResult.Failure($"連線失敗: {ex.Message}");
            }
        }
    }
}