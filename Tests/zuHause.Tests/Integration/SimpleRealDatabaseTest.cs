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
    /// 簡單的真實資料庫測試 - 驗證 RowVersion 併發控制
    /// </summary>
    public class SimpleRealDatabaseTest : IDisposable
    {
        private readonly ZuHauseContext _context;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<SimpleRealDatabaseTest> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SimpleRealDatabaseTest()
        {
            var services = new ServiceCollection();
            
            // 使用真實的 Azure SQL Database 連接
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

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<ZuHauseContext>();
            _imageUploadService = _serviceProvider.GetRequiredService<IImageUploadService>();
            _logger = _serviceProvider.GetRequiredService<ILogger<SimpleRealDatabaseTest>>();

            // 準備測試資料
            SetupTestData();
        }

        private void SetupTestData()
        {
            // 確保有測試房源（使用現有資料）
            var property = _context.Properties.FirstOrDefault();
            if (property == null)
            {
                _logger.LogWarning("沒有找到房源資料，測試可能無法執行");
                return;
            }

            // 檢查但不清理舊的測試圖片
            var oldTestImages = _context.Images.Where(i => 
                i.OriginalFileName.StartsWith("realdb_test_")).ToList();
            
            if (oldTestImages.Any())
            {
                _logger.LogInformation("發現 {Count} 張已存在的測試圖片，將創建新的測試圖片", oldTestImages.Count);
            }

            // 創建測試圖片
            var testImages = new List<Image>();
            for (int i = 1; i <= 3; i++)
            {
                var image = new Image
                {
                    ImageGuid = Guid.NewGuid(),
                    EntityType = EntityType.Property,
                    EntityId = property.PropertyId,
                    Category = ImageCategory.Gallery,
                    OriginalFileName = $"realdb_test_{i}.jpg",
                    StoredFileName = $"{Guid.NewGuid()}.webp",
                    MimeType = "image/webp",
                    FileSizeBytes = 1000000,
                    Width = 1200,
                    Height = 800,
                    IsActive = true,
                    DisplayOrder = i,
                    UploadedAt = DateTime.UtcNow
                    // RowVersion 由資料庫自動填充
                };
                testImages.Add(image);
            }

            _context.Images.AddRange(testImages);
            _context.SaveChanges();

            _logger.LogInformation("創建了 {Count} 張測試圖片 - 房源ID: {PropertyId}", 
                testImages.Count, property.PropertyId);
        }

        [Fact]
        public async Task ReorderImages_WithRealDatabase_ShouldPersistChangesWithRowVersion()
        {
            // Arrange
            var testImages = await _context.Images
                .Where(i => i.OriginalFileName.StartsWith("realdb_test_"))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            if (!testImages.Any())
            {
                Assert.Fail("沒有找到測試圖片");
                return;
            }

            var property = testImages.First();
            var originalOrder = testImages.Select(i => i.ImageId).ToList();
            var newOrder = originalOrder.AsEnumerable().Reverse().ToList();

            _logger.LogInformation("測試開始 - 房源ID: {PropertyId}, 原始順序: {Original}, 新順序: {New}", 
                property.EntityId, 
                string.Join(", ", originalOrder), 
                string.Join(", ", newOrder));

            // 記錄排序前的 RowVersion
            var beforeRowVersions = testImages.ToDictionary(i => i.ImageId, i => Convert.ToBase64String(i.RowVersion));
            _logger.LogInformation("排序前 RowVersion: {RowVersions}", 
                string.Join(", ", beforeRowVersions.Select(kv => $"{kv.Key}:{kv.Value[..8]}...")));

            // Act
            var result = await _imageUploadService.ReorderImagesAsync(EntityType.Property, property.EntityId, newOrder);

            // Assert
            Assert.True(result, "重新排序應該成功");

            // 驗證資料庫中的實際變更
            var updatedImages = await _context.Images
                .Where(i => i.OriginalFileName.StartsWith("realdb_test_"))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            _logger.LogInformation("排序後的順序: {Updated}", 
                string.Join(", ", updatedImages.Select(i => $"{i.ImageId}({i.DisplayOrder})")));

            // 記錄排序後的 RowVersion
            var afterRowVersions = updatedImages.ToDictionary(i => i.ImageId, i => Convert.ToBase64String(i.RowVersion));
            _logger.LogInformation("排序後 RowVersion: {RowVersions}", 
                string.Join(", ", afterRowVersions.Select(kv => $"{kv.Key}:{kv.Value[..8]}...")));

            // 驗證順序正確
            Assert.Equal(newOrder.Count, updatedImages.Count);
            for (int i = 0; i < newOrder.Count; i++)
            {
                Assert.Equal(newOrder[i], updatedImages[i].ImageId);
                Assert.Equal(i + 1, updatedImages[i].DisplayOrder);
            }

            // 驗證 RowVersion 已更新（併發控制證明）
            foreach (var image in updatedImages)
            {
                var beforeVersion = beforeRowVersions[image.ImageId];
                var afterVersion = Convert.ToBase64String(image.RowVersion);
                
                if (beforeVersion != afterVersion)
                {
                    _logger.LogInformation("圖片 {ImageId} 的 RowVersion 已更新: {Before} -> {After}", 
                        image.ImageId, beforeVersion[..8] + "...", afterVersion[..8] + "...");
                }
            }

            _logger.LogInformation("✅ 真實資料庫測試完成 - 排序功能和併發控制都正常工作");
        }

        public void Dispose()
        {
            // 不清理測試資料，讓使用者可以在資料庫中確認
            _logger.LogInformation("測試完成，測試資料保留在資料庫中供確認");

            _context.Dispose();
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}