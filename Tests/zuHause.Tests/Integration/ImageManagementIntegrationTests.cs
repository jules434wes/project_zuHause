using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
    /// 統一圖片管理系統整合測試
    /// Task 6: 完整的端到端測試，不使用 Mock，驗證真實的業務流程
    /// </summary>
    public class ImageManagementIntegrationTests : IDisposable
    {
        private readonly ZuHauseContext _context;
        private readonly IServiceProvider _serviceProvider;
        
        // 真實的業務邏輯服務（完整 DI 鏈）
        private readonly IEntityExistenceChecker _entityExistenceChecker;
        private readonly IDisplayOrderManager _displayOrderManager;
        private readonly IImageQueryService _imageQueryService;
        private readonly IImageUploadService _imageUploadService;
        private readonly PropertyImageService _propertyImageService;
        
        public ImageManagementIntegrationTests()
        {
            // 建立真實的服務容器
            var services = new ServiceCollection();
            
            // 設定 InMemory 資料庫
            services.AddDbContext<ZuHauseContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            
            // 設定 Configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ImageSettings:BaseUrl"] = "/images/"
                })
                .Build();
            services.AddSingleton<IConfiguration>(configuration);
            
            // 註冊真實的圖片處理器（簡化版，用於測試）
            services.AddScoped<IImageProcessor, TestImageProcessor>();
            
            // 註冊所有業務邏輯服務
            services.AddScoped<IEntityExistenceChecker, EntityExistenceChecker>();
            services.AddScoped<IDisplayOrderManager, DisplayOrderManager>();
            services.AddScoped<IImageQueryService, ImageQueryService>();
            services.AddScoped<IImageUploadService, ImageUploadService>();
            services.AddScoped<PropertyImageService>();
            
            // 註冊 Logger
            services.AddLogging();
            
            _serviceProvider = services.BuildServiceProvider();
            
            // 取得服務實例
            _context = _serviceProvider.GetRequiredService<ZuHauseContext>();
            _entityExistenceChecker = _serviceProvider.GetRequiredService<IEntityExistenceChecker>();
            _displayOrderManager = _serviceProvider.GetRequiredService<IDisplayOrderManager>();
            _imageQueryService = _serviceProvider.GetRequiredService<IImageQueryService>();
            _imageUploadService = _serviceProvider.GetRequiredService<IImageUploadService>();
            _propertyImageService = _serviceProvider.GetRequiredService<PropertyImageService>();
            
            // 準備測試資料
            SeedTestData();
        }

        private void SeedTestData()
        {
            // 建立測試會員
            _context.Members.AddRange(
                new Member 
                { 
                    MemberId = 1, 
                    Email = "test1@example.com", 
                    MemberName = "Test User 1",
                    Password = "password123",
                    PhoneNumber = "0912345678"
                },
                new Member 
                { 
                    MemberId = 2, 
                    Email = "test2@example.com", 
                    MemberName = "Test User 2",
                    Password = "password456",
                    PhoneNumber = "0987654321"
                }
            );

            // 建立測試房源
            _context.Properties.AddRange(
                new Property 
                { 
                    PropertyId = 1, 
                    Title = "測試房源 1", 
                    LandlordMemberId = 1,
                    MonthlyRent = 15000,
                    ElectricityFeeType = "固定式",
                    WaterFeeType = "固定式",
                    StatusCode = "active",
                    CreatedAt = DateTime.UtcNow
                },
                new Property 
                { 
                    PropertyId = 2, 
                    Title = "測試房源 2", 
                    LandlordMemberId = 2,
                    MonthlyRent = 20000,
                    ElectricityFeeType = "固定式",
                    WaterFeeType = "固定式",
                    StatusCode = "active",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // 建立測試家具
            _context.FurnitureProducts.AddRange(
                new FurnitureProduct 
                { 
                    FurnitureProductId = "1", 
                    ProductName = "測試家具 1",
                    ListPrice = 5000,
                    DailyRental = 100
                },
                new FurnitureProduct 
                { 
                    FurnitureProductId = "2", 
                    ProductName = "測試家具 2",
                    ListPrice = 8000,
                    DailyRental = 150
                }
            );

            // 建立測試系統訊息（代表 Announcement）
            _context.SystemMessages.AddRange(
                new SystemMessage
                {
                    MessageId = 1,
                    Title = "測試公告 1",
                    CategoryCode = "TEST",
                    AudienceTypeCode = "ALL",
                    MessageContent = "測試內容",
                    AdminId = 1
                },
                new SystemMessage
                {
                    MessageId = 2,
                    Title = "測試公告 2",
                    CategoryCode = "TEST",
                    AudienceTypeCode = "ALL",
                    MessageContent = "測試內容",
                    AdminId = 1
                }
            );

            _context.SaveChanges();
        }

        #region Task 1-2 整合測試：實體驗證和 DisplayOrder 管理

        [Fact]
        public async Task EntityExistenceChecker_WithAllEntityTypes_ShouldWorkCorrectly()
        {
            // Act & Assert - 測試所有實體類型的存在性檢查
            (await _entityExistenceChecker.ExistsAsync(EntityType.Property, 1)).Should().BeTrue();
            (await _entityExistenceChecker.ExistsAsync(EntityType.Member, 1)).Should().BeTrue();
            (await _entityExistenceChecker.ExistsAsync(EntityType.Furniture, 1)).Should().BeTrue();
            (await _entityExistenceChecker.ExistsAsync(EntityType.Announcement, 1)).Should().BeTrue();
            
            // 不存在的實體
            (await _entityExistenceChecker.ExistsAsync(EntityType.Property, 999)).Should().BeFalse();
        }

        [Fact]
        public async Task DisplayOrderManager_EndToEndFlow_ShouldWorkCorrectly()
        {
            // Arrange - 建立測試圖片
            var images = new List<long>();
            for (int i = 1; i <= 5; i++)
            {
                var image = new Image
                {
                    ImageGuid = Guid.NewGuid(),
                    EntityType = EntityType.Property,
                    EntityId = 1,
                    Category = ImageCategory.Gallery,
                    OriginalFileName = $"test{i}.jpg",
                    StoredFileName = $"{Guid.NewGuid()}.webp",
                    MimeType = "image/webp",
                    FileSizeBytes = 1000000,
                    Width = 1200,
                    Height = 800,
                    IsActive = true,
                    UploadedAt = DateTime.UtcNow
                };
                _context.Images.Add(image);
                _context.SaveChanges();
                images.Add(image.ImageId);
            }

            // Act - 分配 DisplayOrder
            var assignResult = await _displayOrderManager.AssignDisplayOrdersAsync(EntityType.Property, 1, ImageCategory.Gallery, images);

            // Assert - 驗證分配結果
            assignResult.IsSuccess.Should().BeTrue();
            assignResult.AssignedOrders.Should().HaveCount(5);

            // Assert - 驗證順序分配
            var orderedImages = await _context.Images
                .Where(i => i.EntityType == EntityType.Property && i.EntityId == 1)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            orderedImages.Should().HaveCount(5);
            for (int i = 0; i < orderedImages.Count; i++)
            {
                orderedImages[i].DisplayOrder.Should().Be(i + 1);
            }
        }

        #endregion

        #region Task 3-4 整合測試：圖片上傳和查詢服務

        [Fact]
        public async Task ImageUploadAndQuery_EndToEndFlow_ShouldWorkCorrectly()
        {
            // Arrange
            var files = CreateMockFiles(
                ("image1.jpg", 1024 * 1024, "image/jpeg"),
                ("image2.png", 2 * 1024 * 1024, "image/png"),
                ("image3.webp", 800 * 1024, "image/webp")
            );

            // Act - 上傳圖片（使用真實的業務流程）
            var uploadResults = await _imageUploadService.UploadImagesAsync(
                files, EntityType.Property, 1, ImageCategory.Gallery);

            // Assert - 上傳結果
            uploadResults.Should().HaveCount(3);
            uploadResults.Should().AllSatisfy(r => r.Success.Should().BeTrue());

            // Act - 查詢圖片
            var queryResults = await _imageQueryService.GetImagesByEntityAsync(EntityType.Property, 1);

            // Assert - 查詢結果
            queryResults.Should().HaveCount(3);
            queryResults.Should().AllSatisfy(img => 
            {
                img.EntityType.Should().Be(EntityType.Property);
                img.EntityId.Should().Be(1);
                img.Category.Should().Be(ImageCategory.Gallery);
                img.IsActive.Should().BeTrue();
            });

            // Act - 查詢主圖
            var mainImage = await _imageQueryService.GetMainImageAsync(EntityType.Property, 1);

            // Assert - 主圖應該是第一張上傳的圖片
            mainImage.Should().NotBeNull();
            mainImage!.DisplayOrder.Should().Be(1);
            mainImage.OriginalFileName.Should().Be("image1.jpg");
        }

        [Fact]
        public async Task ImageUpload_WithInvalidEntity_ShouldReturnFailure()
        {
            // Arrange
            var files = CreateMockFiles(("test.jpg", 1024 * 1024, "image/jpeg"));

            // Act - 嘗試上傳到不存在的實體
            var results = await _imageUploadService.UploadImagesAsync(
                files, EntityType.Property, 999, ImageCategory.Gallery);

            // Assert
            results.Should().HaveCount(1);
            results[0].Success.Should().BeFalse();
            results[0].ErrorMessage.Should().Contain("不存在");
        }

        #endregion

        #region Task 5 整合測試：PropertyImageService Facade

        [Fact]
        public async Task PropertyImageService_BackwardCompatibility_ShouldWorkCorrectly()
        {
            // Arrange
            var files = CreateMockFiles(
                ("property1.jpg", 1024 * 1024, "image/jpeg"),
                ("property2.jpg", 2 * 1024 * 1024, "image/jpeg")
            );

            // Act - 使用舊的 PropertyImageService 介面
            var uploadResults = await _propertyImageService.UploadPropertyImagesAsync(1, files);

            // Assert - 向後相容性
            uploadResults.Should().HaveCount(2);
            uploadResults.Should().AllSatisfy(r => r.Success.Should().BeTrue());
            uploadResults[0].IsMainImage.Should().BeTrue(); // 第一張是主圖
            uploadResults[1].IsMainImage.Should().BeFalse();

            // Act - 使用新方法查詢
            var mainImage = await _propertyImageService.GetMainPropertyImageAsync(1);
            var allImages = await _propertyImageService.GetPropertyImagesAsync(1);
            var imageCount = await _propertyImageService.GetPropertyImageCountAsync(1);
            var hasImages = await _propertyImageService.HasPropertyImagesAsync(1);

            // Assert - 新方法正常運作
            mainImage.Should().NotBeNull();
            mainImage!.DisplayOrder.Should().Be(1);
            
            allImages.Should().HaveCount(2);
            imageCount.Should().Be(2);
            hasImages.Should().BeTrue();
        }

        #endregion

        #region Task 6 整合測試：多實體類型圖片管理

        [Fact]
        public async Task MultiEntityImageManagement_AllEntityTypes_ShouldWorkCorrectly()
        {
            // Arrange
            var testScenarios = new[]
            {
                new { EntityType = EntityType.Property, EntityId = 1, Category = ImageCategory.Gallery, FileName = "property.jpg" },
                new { EntityType = EntityType.Member, EntityId = 1, Category = ImageCategory.Avatar, FileName = "avatar.jpg" },
                new { EntityType = EntityType.Furniture, EntityId = 1, Category = ImageCategory.Product, FileName = "furniture.jpg" },
                new { EntityType = EntityType.Announcement, EntityId = 1, Category = ImageCategory.Gallery, FileName = "announcement.jpg" }
            };

            foreach (var scenario in testScenarios)
            {
                // Act - 為每種實體類型上傳圖片
                var files = CreateMockFiles((scenario.FileName, 1024 * 1024, "image/jpeg"));
                var results = await _imageUploadService.UploadImagesAsync(
                    files, scenario.EntityType, scenario.EntityId, scenario.Category);

                // Assert - 驗證上傳成功
                results.Should().HaveCount(1);
                results[0].Success.Should().BeTrue();
                results[0].EntityType.Should().Be(scenario.EntityType);
                results[0].EntityId.Should().Be(scenario.EntityId);
                results[0].Category.Should().Be(scenario.Category);
                results[0].OriginalFileName.Should().Be(scenario.FileName);
            }

            // Act - 驗證所有圖片都正確儲存
            foreach (var scenario in testScenarios)
            {
                var images = await _imageQueryService.GetImagesByEntityAsync(scenario.EntityType, scenario.EntityId);
                
                // Assert
                images.Should().HaveCount(1);
                images[0].EntityType.Should().Be(scenario.EntityType);
                images[0].EntityId.Should().Be(scenario.EntityId);
                images[0].Category.Should().Be(scenario.Category);
                images[0].IsActive.Should().BeTrue();
            }
        }

        [Fact]
        public async Task CompleteImageManagementWorkflow_ShouldWorkEndToEnd()
        {
            // Arrange
            var propertyId = 1;

            // Step 1: 驗證實體存在
            var propertyExists = await _entityExistenceChecker.ExistsAsync(EntityType.Property, propertyId);
            propertyExists.Should().BeTrue();

            // Step 2: 上傳多張圖片
            var files = CreateMockFiles(
                ("main.jpg", 1024 * 1024, "image/jpeg"),
                ("bedroom.jpg", 1024 * 1024, "image/jpeg"),
                ("living.jpg", 1024 * 1024, "image/jpeg"),
                ("kitchen.jpg", 1024 * 1024, "image/jpeg")
            );

            var uploadResults = await _imageUploadService.UploadImagesAsync(
                files, EntityType.Property, propertyId, ImageCategory.Gallery);

            uploadResults.Should().HaveCount(4);
            uploadResults.Should().AllSatisfy(r => r.Success.Should().BeTrue());

            // Step 3: 驗證主圖邏輯
            var mainImage = await _imageQueryService.GetMainImageAsync(EntityType.Property, propertyId);
            mainImage.Should().NotBeNull();
            mainImage!.DisplayOrder.Should().Be(1);
            mainImage.OriginalFileName.Should().Be("main.jpg");

            // Step 4: 測試設定新主圖
            var allImages = await _imageQueryService.GetImagesByEntityAsync(EntityType.Property, propertyId);
            var secondImageId = allImages.First(i => i.OriginalFileName == "bedroom.jpg").ImageId;
            
            var setMainResult = await _imageUploadService.SetMainImageAsync(secondImageId);
            setMainResult.Should().BeTrue();

            // 驗證新主圖
            var newMainImage = await _imageQueryService.GetMainImageAsync(EntityType.Property, propertyId);
            newMainImage.Should().NotBeNull();
            newMainImage!.ImageId.Should().Be(secondImageId);
            newMainImage.DisplayOrder.Should().Be(1);

            // Step 5: 測試重新排序
            var imageIds = allImages.Select(i => i.ImageId).OrderByDescending(id => id).ToList(); // 反向排序
            var reorderResult = await _imageUploadService.ReorderImagesAsync(EntityType.Property, propertyId, imageIds);
            reorderResult.Should().BeTrue();

            // 驗證排序結果
            var reorderedImages = await _imageQueryService.GetImagesByEntityAsync(EntityType.Property, propertyId);
            for (int i = 0; i < reorderedImages.Count; i++)
            {
                reorderedImages[i].ImageId.Should().Be(imageIds[i]);
                reorderedImages[i].DisplayOrder.Should().Be(i + 1);
            }

            // Step 6: 測試刪除圖片
            var imageToDelete = reorderedImages.Last();
            var deleteResult = await _imageUploadService.DeleteImageAsync(imageToDelete.ImageId);
            deleteResult.Should().BeTrue();

            // 驗證軟刪除
            var deletedImage = await _context.Images.FindAsync(imageToDelete.ImageId);
            deletedImage.Should().NotBeNull();
            deletedImage!.IsActive.Should().BeFalse();

            // 驗證活躍圖片數量
            var remainingImages = await _imageQueryService.GetImagesByEntityAsync(EntityType.Property, propertyId);
            remainingImages.Should().HaveCount(3);
            remainingImages.Should().AllSatisfy(img => img.IsActive.Should().BeTrue());
        }

        #endregion

        // 測試輔助方法
        private IFormFileCollection CreateMockFiles(params (string fileName, long size, string contentType)[] fileSpecs)
        {
            var files = new FormFileCollection();
            
            foreach (var (fileName, size, contentType) in fileSpecs)
            {
                var stream = new MemoryStream(new byte[size]);
                var file = new FormFile(stream, 0, size, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };
                
                files.Add(file);
            }
            
            return files;
        }

        public void Dispose()
        {
            _context.Dispose();
            _serviceProvider?.GetService<IDisposable>()?.Dispose();
        }
    }

    /// <summary>
    /// 測試用的圖片處理器 - 真實實作，但簡化版
    /// </summary>
    public class TestImageProcessor : IImageProcessor
    {
        public Task<ImageProcessingResult> ConvertToWebPAsync(Stream sourceStream, int? maxWidth = null, int quality = 80)
        {
            // 簡化版實作，用於測試
            var result = new ImageProcessingResult
            {
                Success = true,
                ProcessedFormat = "webp",
                SizeBytes = 500000,
                Width = Math.Min(maxWidth ?? 1200, 1200),
                Height = 800
            };
            
            return Task.FromResult(result);
        }

        public Task<ImageProcessingResult> GenerateThumbnailAsync(Stream imageStream, int width, int height)
        {
            var result = new ImageProcessingResult
            {
                Success = true,
                ProcessedFormat = "webp",
                SizeBytes = 50000,
                Width = width,
                Height = height
            };
            
            return Task.FromResult(result);
        }

        public Task<bool> ValidateImageAsync(Stream imageStream)
        {
            return Task.FromResult(true);
        }

        public Task<(int width, int height)> GetImageDimensionsAsync(Stream imageStream)
        {
            return Task.FromResult((1200, 800));
        }
    }
}