using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using zuHause.Data;
using zuHause.Models;

namespace zuHause.Tests.Integration
{
    /// <summary>
    /// 測試專用的 WebApplicationFactory，使用獨立的測試資料庫
    /// 確保整合測試不會影響真實的 Azure SQL Database
    /// </summary>
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
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
                
                // 註冊測試專用的資料庫連接
                var testDatabaseName = $"zuHauseIntegrationTest_{Guid.NewGuid():N}";
                var testConnectionString = $"Server=(localdb)\\mssqllocaldb;Database={testDatabaseName};Trusted_Connection=true;MultipleActiveResultSets=true;Connection Timeout=60";
                
                services.AddDbContext<ZuHauseContext>(options =>
                {
                    options.UseSqlServer(testConnectionString);
                    options.EnableSensitiveDataLogging();
                });
                
                // 設定環境變數跳過 PDF 庫載入
                Environment.SetEnvironmentVariable("SKIP_PDF_LIBRARY", "true");
            });
            
            builder.UseEnvironment("Testing");
        }
        
        public ZuHauseContext CreateDbContext()
        {
            var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            
            // 確保測試資料庫存在
            context.Database.EnsureCreated();
            
            return context;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 清理測試資料庫
                try
                {
                    using var scope = Services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
                    context.Database.EnsureDeleted();
                    Console.WriteLine("✓ 整合測試資料庫已清理");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ 清理整合測試資料庫時發生錯誤：{ex.Message}");
                }
            }
            
            base.Dispose(disposing);
        }
    }
}