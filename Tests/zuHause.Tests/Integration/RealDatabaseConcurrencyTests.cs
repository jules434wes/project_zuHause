using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using zuHause.DTOs;
using zuHause.Enums;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Services;

namespace zuHause.Tests.Integration
{
    /// <summary>
    /// 真實資料庫併發控制測試
    /// 測試 ReorderImagesAsync 的樂觀鎖機制
    /// 使用 IAsyncLifetime 確保每個測試方法的隔離性
    /// </summary>
    public class RealDatabaseConcurrencyTests : IAsyncLifetime
    {
        private IServiceProvider _rootServiceProvider;
        private string _testDatabaseName;

        public RealDatabaseConcurrencyTests()
        {
            // 使用現有的資料庫，避免創建新資料庫的權限問題
            _testDatabaseName = "zuHause"; // 使用現有資料庫
            
            var services = new ServiceCollection();
            
            // 使用現有的資料庫
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=tcp:zuhause.database.windows.net,1433;Initial Catalog=zuHause;Persist Security Info=False;User ID=zuhause;Password=DB$MSIT67;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            services.AddDbContext<ZuHauseContext>(options =>
                options.UseSqlServer(connectionString));

            // 註冊所有必要的服務
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IImageProcessor, TestImageProcessor>();
            services.AddScoped<IEntityExistenceChecker, EntityExistenceChecker>();
            services.AddScoped<IDisplayOrderManager, DisplayOrderManager>();
            services.AddScoped<IImageQueryService, ImageQueryService>();
            services.AddScoped<IImageUploadService, ImageUploadService>();
            services.AddLogging(builder => builder.AddConsole());

            _rootServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// 每個測試方法執行前的初始化
        /// 使用現有資料庫和測試資料
        /// </summary>
        public async Task InitializeAsync()
        {
            using var scope = _rootServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RealDatabaseConcurrencyTests>>();

            // 不創建新資料庫，直接使用現有資料庫
            logger.LogInformation("使用現有資料庫進行併發測試: {DatabaseName}", _testDatabaseName);

            // 清理並準備測試資料
            await CleanupAndSetupTestDataAsync(context, logger);
        }

        /// <summary>
        /// 清理並準備測試資料（使用現有的上下文）
        /// </summary>
        private async Task CleanupAndSetupTestDataAsync(ZuHauseContext context, ILogger logger)
        {
            // 清理舊的測試資料
            var oldTestImages = context.Images.Where(i => 
                i.OriginalFileName.StartsWith("concurrency_test_")).ToList();
            
            if (oldTestImages.Any())
            {
                context.Images.RemoveRange(oldTestImages);
                await context.SaveChangesAsync();
                logger.LogInformation("清理了 {Count} 張舊併發測試圖片", oldTestImages.Count);
            }

            // 使用現有的房源（避免創建重複資料）
            var property = await context.Properties.FirstOrDefaultAsync();
            if (property == null)
            {
                logger.LogWarning("沒有找到房源資料，測試可能無法執行");
                return;
            }

            // 創建測試圖片（讓資料庫自動處理 RowVersion）
            var testImages = new List<Image>();
            for (int i = 1; i <= 5; i++)
            {
                var image = new Image
                {
                    ImageGuid = Guid.NewGuid(),
                    EntityType = EntityType.Property,
                    EntityId = property.PropertyId,
                    Category = ImageCategory.Gallery,
                    OriginalFileName = $"concurrency_test_{i}.jpg",
                    StoredFileName = $"{Guid.NewGuid()}.webp",
                    MimeType = "image/webp",
                    FileSizeBytes = 1000000,
                    Width = 1200,
                    Height = 800,
                    IsActive = true,
                    DisplayOrder = i,
                    UploadedAt = DateTime.UtcNow
                    // 不手動設定 RowVersion，讓資料庫自動處理
                };
                testImages.Add(image);
            }

            context.Images.AddRange(testImages);
            await context.SaveChangesAsync();

            logger.LogInformation("測試資料準備完成 - 房源ID: {PropertyId}, 圖片數量: {ImageCount}", 
                property.PropertyId, testImages.Count);
        }

        [Fact]
        public async Task ReorderImagesAsync_ConcurrencyControl_ShouldHandleOptimisticLocking()
        {
            // Arrange - 為此測試創建獨立的服務作用域
            using var scope = _rootServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RealDatabaseConcurrencyTests>>();

            var property = await context.Properties.FirstAsync();
            var images = await context.Images
                .Where(i => i.EntityType == EntityType.Property && 
                           i.EntityId == property.PropertyId && 
                           i.IsActive &&
                           i.OriginalFileName.StartsWith("concurrency_test_"))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            logger.LogInformation("開始併發測試 - 房源ID: {PropertyId}, 圖片數量: {ImageCount}", 
                property.PropertyId, images.Count);

            var imageIds = images.Select(i => i.ImageId).ToList();
            var reverseImageIds = imageIds.AsEnumerable().Reverse().ToList();

            // Act & Assert - 模擬併發重新排序
            var task1 = Task.Run(async () =>
            {
                using var taskScope = _rootServiceProvider.CreateScope();
                var service = taskScope.ServiceProvider.GetRequiredService<IImageUploadService>();
                var taskLogger = taskScope.ServiceProvider.GetRequiredService<ILogger<RealDatabaseConcurrencyTests>>();
                
                taskLogger.LogInformation("任務1 開始 - 順序: {Order}", string.Join(", ", imageIds));
                var result = await service.ReorderImagesAsync(EntityType.Property, property.PropertyId, imageIds);
                taskLogger.LogInformation("任務1 完成 - 結果: {Result}", result);
                return result;
            });

            var task2 = Task.Run(async () =>
            {
                using var taskScope = _rootServiceProvider.CreateScope();
                var service = taskScope.ServiceProvider.GetRequiredService<IImageUploadService>();
                var taskLogger = taskScope.ServiceProvider.GetRequiredService<ILogger<RealDatabaseConcurrencyTests>>();
                
                // 略微延遲以模擬真實併發場景
                await Task.Delay(50);
                taskLogger.LogInformation("任務2 開始 - 順序: {Order}", string.Join(", ", reverseImageIds));
                var result = await service.ReorderImagesAsync(EntityType.Property, property.PropertyId, reverseImageIds);
                taskLogger.LogInformation("任務2 完成 - 結果: {Result}", result);
                return result;
            });

            var results = await Task.WhenAll(task1, task2);

            // 驗證至少有一個操作成功
            var successCount = results.Count(r => r);
            logger.LogInformation("併發測試結果 - 成功: {SuccessCount}/2", successCount);

            // 檢查最終的資料庫狀態（使用新的上下文以避免快取問題）
            using var finalScope = _rootServiceProvider.CreateScope();
            var finalContext = finalScope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            
            var finalImages = await finalContext.Images
                .Where(i => i.EntityType == EntityType.Property && 
                           i.EntityId == property.PropertyId && 
                           i.IsActive &&
                           i.OriginalFileName.StartsWith("concurrency_test_"))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            logger.LogInformation("最終排序狀態: {FinalOrder}", 
                string.Join(", ", finalImages.Select(i => $"{i.ImageId}({i.DisplayOrder})")));

            // 驗證資料一致性
            Assert.True(successCount >= 1, "至少應該有一個併發操作成功");
            Assert.Equal(images.Count, finalImages.Count);
            Assert.True(finalImages.All(i => i.DisplayOrder.HasValue && i.DisplayOrder > 0));
            
            // 驗證 DisplayOrder 的連續性
            var orders = finalImages.Select(i => i.DisplayOrder.Value).OrderBy(o => o).ToList();
            for (int i = 0; i < orders.Count; i++)
            {
                Assert.Equal(i + 1, orders[i]);
            }
        }

        [Fact]
        public async Task ReorderImagesAsync_RealDatabase_ShouldPersistChanges()
        {
            // Arrange - 為此測試創建獨立的服務作用域
            using var scope = _rootServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            var imageUploadService = scope.ServiceProvider.GetRequiredService<IImageUploadService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RealDatabaseConcurrencyTests>>();

            var property = await context.Properties.FirstAsync();
            var images = await context.Images
                .Where(i => i.EntityType == EntityType.Property && 
                           i.EntityId == property.PropertyId && 
                           i.IsActive &&
                           i.OriginalFileName.StartsWith("concurrency_test_"))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var originalOrder = images.Select(i => i.ImageId).ToList();
            var newOrder = originalOrder.AsEnumerable().Reverse().ToList();

            logger.LogInformation("真實資料庫測試 - 原始順序: {Original}, 新順序: {New}", 
                string.Join(", ", originalOrder), string.Join(", ", newOrder));

            // Act
            var result = await imageUploadService.ReorderImagesAsync(EntityType.Property, property.PropertyId, newOrder);

            // Assert
            Assert.True(result, "重新排序應該成功");

            // 驗證資料庫中的實際變更（使用新的上下文以避免快取問題）
            using var verifyScope = _rootServiceProvider.CreateScope();
            var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            
            var updatedImages = await verifyContext.Images
                .Where(i => i.EntityType == EntityType.Property && 
                           i.EntityId == property.PropertyId && 
                           i.IsActive &&
                           i.OriginalFileName.StartsWith("concurrency_test_"))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            logger.LogInformation("更新後順序: {Updated}", 
                string.Join(", ", updatedImages.Select(i => $"{i.ImageId}({i.DisplayOrder})")));

            // 驗證新順序與預期一致
            Assert.Equal(newOrder.Count, updatedImages.Count);
            for (int i = 0; i < newOrder.Count; i++)
            {
                Assert.Equal(newOrder[i], updatedImages[i].ImageId);
                Assert.Equal(i + 1, updatedImages[i].DisplayOrder);
            }
        }

        /// <summary>
        /// 每個測試方法執行後的清理
        /// 清理測試資料但保留現有資料庫
        /// </summary>
        public async Task DisposeAsync()
        {
            using var scope = _rootServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RealDatabaseConcurrencyTests>>();

            // 清理測試產生的圖片資料
            try
            {
                var testImages = context.Images.Where(i => i.OriginalFileName.StartsWith("concurrency_test_")).ToList();
                if (testImages.Any())
                {
                    context.Images.RemoveRange(testImages);
                    await context.SaveChangesAsync();
                    logger.LogInformation("清理了 {Count} 張併發測試圖片", testImages.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "清理併發測試資料時發生錯誤");
            }

            context.Dispose();
            (_rootServiceProvider as IDisposable)?.Dispose();
        }
    }
}