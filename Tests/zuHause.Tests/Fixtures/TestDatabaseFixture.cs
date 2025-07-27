using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using zuHause.Data;
using zuHause.Models;

namespace zuHause.Tests.Fixtures
{
    /// <summary>
    /// 測試資料庫固定裝置，用於共享測試資料庫連接
    /// 遵循使用者要求：所有測試使用真實 SQL Server 資料庫
    /// </summary>
    public class TestDatabaseFixture : IDisposable
    {
        public string ConnectionString { get; private set; }
        public string DatabaseName { get; private set; }
        
        public TestDatabaseFixture()
        {
            // 使用固定的測試資料庫名稱，避免每次創建新資料庫
            DatabaseName = "zuHauseTestDatabase";
            ConnectionString = $"Server=(localdb)\\mssqllocaldb;Database={DatabaseName};Trusted_Connection=true;MultipleActiveResultSets=true;Connection Timeout=60";
            
            InitializeDatabase();
        }
        
        private void InitializeDatabase()
        {
            try
            {
                using var context = CreateDbContext();
                
                // 確保資料庫存在
                context.Database.EnsureCreated();
                
                Console.WriteLine($"✓ 測試資料庫初始化完成：{DatabaseName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 測試資料庫初始化失敗：{ex.Message}");
                throw;
            }
        }
        
        public ZuHauseContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseSqlServer(ConnectionString)
                .EnableSensitiveDataLogging() // 用於調試
                .Options;
                
            return new ZuHauseContext(options);
        }
        
        public DbContextOptions<ZuHauseContext> GetDbContextOptions()
        {
            return new DbContextOptionsBuilder<ZuHauseContext>()
                .UseSqlServer(ConnectionString)
                .EnableSensitiveDataLogging()
                .Options;
        }
        
        public void Dispose()
        {
            try
            {
                // 注意：不刪除測試資料庫，因為可能被其他測試使用
                // 在需要時可以手動清理
                Console.WriteLine($"✓ 測試資料庫固定裝置已釋放：{DatabaseName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 清理測試資料庫時發生錯誤：{ex.Message}");
            }
        }
    }
}