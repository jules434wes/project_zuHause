using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using zuHause.Models;

namespace zuHause.Tests.Integration
{
    /// <summary>
    /// 專門檢查資料庫連接配置的測試
    /// </summary>
    public class DatabaseConnectionTest : IClassFixture<AzureTestWebApplicationFactory<Program>>
    {
        private readonly AzureTestWebApplicationFactory<Program> _factory;

        public DatabaseConnectionTest(AzureTestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void CheckDatabaseConnection_ShouldConnectToAzureSQL()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            
            var connectionString = context.Database.GetDbConnection().ConnectionString;
            Console.WriteLine($"🔍 連接字串: {connectionString}");
            
            // 檢查是否連接到 Azure SQL Database
            Assert.Contains("database.windows.net", connectionString, StringComparison.OrdinalIgnoreCase);
            
            // 測試實際連接
            var canConnect = context.Database.CanConnect();
            Console.WriteLine($"🔍 資料庫連接狀態: {canConnect}");
            Assert.True(canConnect, "無法連接到資料庫");
            
            // 查詢房源總數
            var propertyCount = context.Properties.Count();
            Console.WriteLine($"🔍 資料庫中房源總數: {propertyCount}");
        }
    }
}