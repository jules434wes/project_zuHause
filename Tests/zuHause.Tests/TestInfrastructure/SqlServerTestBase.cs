using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using zuHause.Models;
using Xunit;

namespace zuHause.Tests.TestInfrastructure
{
    /// <summary>
    /// SQL Server 整合測試基類
    /// 用於需要真實資料庫行為的測試（交易、並發、約束等）
    /// </summary>
    /// <remarks>
    /// 使用方式：
    /// 1. 繼承此基類
    /// 2. 在測試中使用 Context 屬性
    /// 3. 每個測試方法都會有獨立的資料庫實例
    /// 
    /// 注意：
    /// - 需要 Docker 環境支援 SQL Server 容器
    /// - 比 InMemory 測試慢，但提供真實的資料庫行為
    /// - 適用於整合測試、並發測試、交易測試
    /// </remarks>
    public abstract class SqlServerTestBase : IAsyncLifetime
    {
        private string _connectionString = string.Empty;
        private string _databaseName = string.Empty;
        
        protected ZuHauseContext Context { get; private set; } = null!;
        protected IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// 測試初始化 - 建立資料庫容器和上下文
        /// </summary>
        public async Task InitializeAsync()
        {
            // 生成唯一的資料庫名稱
            _databaseName = $"TestDb_{Guid.NewGuid():N}";
            
            // TODO: Task 3 時實作 Testcontainers 邏輯
            // 暫時使用 LocalDB 或現有連線字串
            _connectionString = GetTestConnectionString();

            // 建立服務容器
            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            Context = ServiceProvider.GetRequiredService<ZuHauseContext>();

            // 建立資料庫架構
            await Context.Database.EnsureCreatedAsync();
            
            // 執行測試資料設定
            await SeedTestDataAsync();
        }

        /// <summary>
        /// 測試清理 - 刪除資料庫
        /// </summary>
        public async Task DisposeAsync()
        {
            if (Context != null)
            {
                await Context.Database.EnsureDeletedAsync();
                await Context.DisposeAsync();
            }

            if (ServiceProvider is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// 配置測試服務
        /// </summary>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ZuHauseContext>(options =>
                options.UseSqlServer(_connectionString));
        }

        /// <summary>
        /// 設定測試資料
        /// </summary>
        protected virtual async Task SeedTestDataAsync()
        {
            // 子類可以覆寫此方法來設定特定的測試資料
            await Task.CompletedTask;
        }

        /// <summary>
        /// 獲取測試資料庫連線字串
        /// </summary>
        private string GetTestConnectionString()
        {
            // 優先使用環境變數中的測試資料庫
            var testConnectionString = Environment.GetEnvironmentVariable("TEST_DATABASE_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(testConnectionString))
            {
                return testConnectionString.Replace("{DatabaseName}", _databaseName);
            }

            // 後備選項：使用 LocalDB
            return $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Integrated Security=true;MultipleActiveResultSets=true;";
        }

        /// <summary>
        /// 建立新的交易作用域
        /// </summary>
        protected async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 測試並發場景的輔助方法
        /// </summary>
        protected async Task<T[]> ExecuteConcurrentlyAsync<T>(params Func<Task<T>>[] operations)
        {
            var tasks = operations.Select(op => Task.Run(op)).ToArray();
            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 重設資料庫狀態（保留架構，清除資料）
        /// </summary>
        protected async Task ResetDatabaseAsync()
        {
            // 刪除所有資料但保留架構
            await Context.Database.ExecuteSqlRawAsync("DELETE FROM images");
            await Context.Database.ExecuteSqlRawAsync("DELETE FROM properties");
            await Context.Database.ExecuteSqlRawAsync("DELETE FROM members");
            // 根據需要添加其他資料表
        }
    }
}

// 使用範例：
/*
public class ImageUploadIntegrationTests : SqlServerTestBase
{
    private IImageUploadService _service = null!;

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        
        // 註冊額外的服務
        services.AddScoped<IImageUploadService, ImageUploadService>();
        services.AddScoped<IImageProcessor, ImageProcessor>();
    }

    protected override async Task SeedTestDataAsync()
    {
        // 設定測試資料
        Context.Members.Add(new Member { MemberId = 1, Email = "test@example.com" });
        Context.Properties.Add(new Property { PropertyId = 1, LandlordMemberId = 1 });
        await Context.SaveChangesAsync();
    }

    [Fact]
    public async Task UploadImages_WithTransaction_ShouldCommitOrRollback()
    {
        _service = ServiceProvider.GetRequiredService<IImageUploadService>();
        
        // 測試交易行為
        var result = await ExecuteInTransactionAsync(async () =>
        {
            return await _service.UploadAsync(propertyId: 1, imageFiles);
        });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ConcurrentDisplayOrderUpdate_ShouldHandleCorrectly()
    {
        // 測試並發場景
        var results = await ExecuteConcurrentlyAsync(
            () => _service.SetPrimaryImageAsync(1),
            () => _service.SetPrimaryImageAsync(2),
            () => _service.SetPrimaryImageAsync(3)
        );

        // 驗證只有一個操作成功，其他失敗或等待
        Assert.Single(results.Where(r => r.Success));
    }
}
*/