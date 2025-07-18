using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using zuHause.DTOs;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Services;
using zuHause.Enums;

namespace zuHause.Tests.Services
{
    /// <summary>
    /// PropertyImageService 單元測試 - 重構為 Facade 模式測試
    /// 遵循反過度模擬原則：使用真實的業務邏輯服務，僅 Mock 外部依賴
    /// </summary>
    public class PropertyImageServiceTests : IDisposable
    {
        private readonly ZuHauseContext _context;
        private readonly PropertyImageService _service;
        
        // 只 Mock 外部依賴
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<PropertyImageService>> _mockLogger;
        private readonly Mock<ILogger<ImageUploadService>> _mockImageUploadLogger;
        
        // 使用真實的業務邏輯服務
        private readonly ImageUploadService _imageUploadService;
        private readonly ImageQueryService _imageQueryService;
        private readonly EntityExistenceChecker _entityExistenceChecker;
        private readonly DisplayOrderManager _displayOrderManager;

        public PropertyImageServiceTests()
        {
            // 使用 InMemory 資料庫進行測試
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ZuHauseContext(options);

            // 只 Mock 外部依賴
            _mockImageProcessor = new Mock<IImageProcessor>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<PropertyImageService>>();
            _mockImageUploadLogger = new Mock<ILogger<ImageUploadService>>();

            // 設定 Configuration Mock
            _mockConfiguration.Setup(c => c["ImageSettings:BaseUrl"]).Returns("/images/");

            // 建立真實的業務邏輯服務
            _entityExistenceChecker = new EntityExistenceChecker(_context);
            _displayOrderManager = new DisplayOrderManager(_context);
            _imageUploadService = new ImageUploadService(
                _context,
                _mockImageProcessor.Object,
                _entityExistenceChecker,
                _displayOrderManager,
                _mockImageUploadLogger.Object);
            _imageQueryService = new ImageQueryService(_context, _mockConfiguration.Object);

            // 建立 PropertyImageService facade
            _service = new PropertyImageService(
                _imageUploadService,
                _imageQueryService,
                _mockLogger.Object);

            // 設定外部依賴的預設行為
            SetupImageProcessor();

            // 準備測試資料
            SeedTestData();
        }

        private void SetupImageProcessor()
        {
            // 設定 ImageProcessor 的預設行為（外部依賴）
            _mockImageProcessor.Setup(ip => ip.ConvertToWebPAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ImageProcessingResult
                {
                    Success = true,
                    ProcessedFormat = "webp",
                    SizeBytes = 500000,
                    Width = 1200,
                    Height = 800
                });
        }

        private void SeedTestData()
        {
            var property = new Property
            {
                PropertyId = 1,
                Title = "測試房源",
                MonthlyRent = 15000,
                CreatedAt = DateTime.UtcNow,
                ElectricityFeeType = "固定式",
                StatusCode = "active",
                WaterFeeType = "固定式"
            };

            _context.Properties.Add(property);
            _context.SaveChanges();
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WithValidSingleImage_ShouldReturnSuccessResult()
        {
            // Arrange
            var propertyId = 1;
            var files = CreateMockFiles(("test1.jpg", 1024 * 1024, "image/jpeg")); // 1MB

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(1);
            
            // 調試輸出
            if (!results[0].Success)
            {
                Console.WriteLine($"上傳失敗: {results[0].ErrorMessage}");
            }
            
            results[0].Success.Should().BeTrue();
            results[0].IsMainImage.Should().BeTrue(); // 第一張應該是主圖
            results[0].OriginalFileName.Should().Be("test1.jpg");
            results[0].PropertyImageId.Should().BeGreaterThan(0);
            results[0].IsValidResult().Should().BeTrue();

            // 驗證資料庫記錄 - 現在儲存到統一的 Images 表
            var dbImage = await _context.Images.FirstOrDefaultAsync(i => 
                i.EntityType == EntityType.Property && i.EntityId == propertyId);
            dbImage.Should().NotBeNull();
            dbImage!.IsActive.Should().BeTrue();
            dbImage.Category.Should().Be(ImageCategory.Gallery);
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WithMultipleImages_ShouldSetFirstAsMainImage()
        {
            // Arrange
            var propertyId = 1;
            var files = CreateMockFiles(
                ("image1.jpg", 1024 * 1024, "image/jpeg"),
                ("image2.png", 2 * 1024 * 1024, "image/png"),
                ("image3.webp", 800 * 1024, "image/webp")
            );

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(3);
            results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
            
            // 第一張應該是主圖
            results[0].IsMainImage.Should().BeTrue();
            results[1].IsMainImage.Should().BeFalse();
            results[2].IsMainImage.Should().BeFalse();

            // 驗證資料庫記錄
            var dbImages = await _context.Images
                .Where(i => i.EntityType == EntityType.Property && i.EntityId == propertyId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
            
            dbImages.Should().HaveCount(3);
            dbImages[0].DisplayOrder.Should().Be(1); // 主圖
            dbImages[1].DisplayOrder.Should().Be(2);
            dbImages[2].DisplayOrder.Should().Be(3);
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WithNonExistentProperty_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentPropertyId = 999;
            var files = CreateMockFiles(("test.jpg", 1024 * 1024, "image/jpeg"));

            // Act
            var results = await _service.UploadPropertyImagesAsync(nonExistentPropertyId, files);

            // Assert
            results.Should().HaveCount(1);
            results[0].Success.Should().BeFalse();
            results[0].ErrorMessage.Should().Contain("不存在");
        }

        [Fact]
        public async Task GetMainPropertyImageAsync_WithExistingImages_ShouldReturnMainImage()
        {
            // Arrange
            var propertyId = 1;
            
            // 先上傳一些圖片
            var files = CreateMockFiles(
                ("main.jpg", 1024 * 1024, "image/jpeg"),
                ("secondary.jpg", 1024 * 1024, "image/jpeg")
            );
            await _service.UploadPropertyImagesAsync(propertyId, files);

            // Act
            var mainImage = await _service.GetMainPropertyImageAsync(propertyId);

            // Assert
            mainImage.Should().NotBeNull();
            mainImage!.DisplayOrder.Should().Be(1);
            mainImage.EntityType.Should().Be(EntityType.Property);
            mainImage.EntityId.Should().Be(propertyId);
        }

        [Fact]
        public async Task GetPropertyImagesAsync_WithExistingImages_ShouldReturnAllImages()
        {
            // Arrange
            var propertyId = 1;
            
            // 先上傳一些圖片
            var files = CreateMockFiles(
                ("image1.jpg", 1024 * 1024, "image/jpeg"),
                ("image2.jpg", 1024 * 1024, "image/jpeg"),
                ("image3.jpg", 1024 * 1024, "image/jpeg")
            );
            await _service.UploadPropertyImagesAsync(propertyId, files);

            // Act
            var images = await _service.GetPropertyImagesAsync(propertyId);

            // Assert
            images.Should().HaveCount(3);
            images.Should().AllSatisfy(img => 
            {
                img.EntityType.Should().Be(EntityType.Property);
                img.EntityId.Should().Be(propertyId);
                img.IsActive.Should().BeTrue();
            });
        }

        [Fact]
        public async Task DeletePropertyImageAsync_WithExistingImage_ShouldReturnTrue()
        {
            // Arrange
            var propertyId = 1;
            var files = CreateMockFiles(("test.jpg", 1024 * 1024, "image/jpeg"));
            var uploadResults = await _service.UploadPropertyImagesAsync(propertyId, files);
            var imageId = uploadResults[0].PropertyImageId!.Value;

            // Act
            var result = await _service.DeletePropertyImageAsync(imageId);

            // Assert
            result.Should().BeTrue();
            
            // 驗證圖片被軟刪除
            var deletedImage = await _context.Images.FindAsync((long)imageId);
            deletedImage.Should().NotBeNull();
            deletedImage!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task SetMainPropertyImageAsync_WithValidImageId_ShouldReturnTrue()
        {
            // Arrange
            var propertyId = 1;
            var files = CreateMockFiles(
                ("image1.jpg", 1024 * 1024, "image/jpeg"),
                ("image2.jpg", 1024 * 1024, "image/jpeg")
            );
            var uploadResults = await _service.UploadPropertyImagesAsync(propertyId, files);
            var secondImageId = uploadResults[1].PropertyImageId!.Value;

            // Act - 將第二張圖片設為主圖
            var result = await _service.SetMainPropertyImageAsync(secondImageId);

            // Assert
            result.Should().BeTrue();
            
            // 驗證第二張圖片現在是主圖（DisplayOrder = 1）
            var newMainImage = await _context.Images.FindAsync((long)secondImageId);
            newMainImage.Should().NotBeNull();
            newMainImage!.DisplayOrder.Should().Be(1);
        }

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
        }
    }
}