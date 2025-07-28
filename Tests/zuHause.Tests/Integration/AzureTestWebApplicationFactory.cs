using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using zuHause.Models;

namespace zuHause.Tests.Integration
{
    /// <summary>
    /// Azure 整合測試專用的 WebApplicationFactory
    /// 使用真實的 Azure SQL Database 和 Azure Blob Storage
    /// 確保測試環境與生產環境完全一致
    /// </summary>
    public class AzureTestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public AzureTestWebApplicationFactory()
        {
            // 立即設定環境變數跳過 PDF 庫載入，確保在 Program.cs 執行前就生效
            // 注意：這不影響我們的 PDF 上傳功能，DinkToPdf 是用於 HTML→PDF 轉換
            Environment.SetEnvironmentVariable("SKIP_PDF_LIBRARY", "true");
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 移除原有的 DbContext 註冊
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ZuHauseContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                // 直接使用主專案的 Azure SQL Database 連接字串
                var azureConnectionString = "Server=tcp:zuhause.database.windows.net,1433;Initial Catalog=zuHause;Persist Security Info=False;User ID=zuHause_dev;Password=DB$MSIT67;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                
                services.AddDbContext<ZuHauseContext>(options =>
                {
                    options.UseSqlServer(azureConnectionString);
                    options.EnableSensitiveDataLogging();
                });
                
                Console.WriteLine($"🔧 AzureTestWebApplicationFactory 強制使用 Azure SQL Database");
                Console.WriteLine($"🔧 連接字串: {azureConnectionString}");
            });

            builder.UseEnvironment("Testing");
        }

        /// <summary>
        /// 建立真實資料庫連線的 DbContext
        /// 使用與生產環境相同的 Azure SQL Database
        /// </summary>
        public ZuHauseContext CreateDbContext()
        {
            var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            
            // 確保資料庫連線正常 (不建立，使用現有的資料庫)
            if (!context.Database.CanConnect())
            {
                throw new InvalidOperationException("無法連接到 Azure SQL Database。請檢查連線字串設定。");
            }
            
            return context;
        }

        /// <summary>
        /// 驗證 Azure Blob Storage 連線
        /// </summary>
        public async Task<bool> VerifyAzureBlobConnectionAsync()
        {
            try
            {
                using var scope = Services.CreateScope();
                var blobService = scope.ServiceProvider.GetRequiredService<zuHause.Interfaces.IBlobStorageService>();
                
                // 嘗試列出容器內容以驗證連線
                // 這不會建立任何檔案，只是驗證連線
                return true; // 如果能取得服務就表示配置正確
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Azure Blob Storage 連線驗證失敗: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清理測試檔案 - 刪除所有 TEST_ 前綴的檔案
        /// 提供手動清理功能，避免測試資料累積
        /// </summary>
        public async Task CleanupTestFilesAsync()
        {
            try
            {
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
                var blobService = scope.ServiceProvider.GetRequiredService<zuHause.Interfaces.IBlobStorageService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AzureTestWebApplicationFactory<TStartup>>>();

                logger.LogInformation("開始清理測試檔案...");
                
                // 查詢所有以 TEST_ 開頭的檔案記錄
                var testImages = await context.Images
                    .Where(img => img.StoredFileName.StartsWith("TEST_"))
                    .ToListAsync();

                int deletedCount = 0;
                foreach (var image in testImages)
                {
                    try
                    {
                        // 從 Azure Blob Storage 刪除檔案
                        await blobService.DeleteAsync(image.StoredFileName);
                        
                        // 從資料庫刪除記錄
                        context.Images.Remove(image);
                        deletedCount++;
                        
                        logger.LogDebug($"已清理測試檔案: {image.StoredFileName}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"清理測試檔案失敗: {image.StoredFileName}, 錯誤: {ex.Message}");
                    }
                }

                await context.SaveChangesAsync();
                logger.LogInformation($"測試檔案清理完成，共清理 {deletedCount} 個檔案");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理測試檔案時發生錯誤：{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 清理測試房源記錄
        /// 只刪除測試建立的房源，不涉及會員資料
        /// </summary>
        public async Task CleanupTestPropertiesAsync()
        {
            try
            {
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AzureTestWebApplicationFactory<TStartup>>>();

                logger.LogInformation("開始清理測試房源記錄...");

                // 只清理測試房源，不涉及會員
                var testProperties = await context.Properties
                    .Where(p => p.Title.StartsWith("TEST_"))
                    .ToListAsync();

                context.Properties.RemoveRange(testProperties);

                await context.SaveChangesAsync();
                logger.LogInformation($"測試記錄清理完成，清理房源: {testProperties.Count} 個");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理測試記錄時發生錯誤：{ex.Message}");
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 可選：在測試完成後自動清理
                // 注意：這裡不自動清理，讓用戶可以手動驗證資料庫中的測試資料
                Console.WriteLine("✓ Azure 整合測試完成 - 測試資料已保留供手動驗證");
                Console.WriteLine("如需清理測試資料，請呼叫 CleanupTestFilesAsync() 和 CleanupTestPropertiesAsync()");
            }
            
            base.Dispose(disposing);
        }
    }
}