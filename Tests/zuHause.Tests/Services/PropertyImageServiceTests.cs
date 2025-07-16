using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using zuHause.DTOs;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Services;

namespace zuHause.Tests.Services
{
    /// <summary>
    /// PropertyImageService 單元測試
    /// 遵循反過度模擬原則：測試真實業務邏輯，最小化 Mock 使用
    /// </summary>
    public class PropertyImageServiceTests : IDisposable
    {
        private readonly ZuHauseContext _context;
        private readonly PropertyImageService _service;
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<ILogger<PropertyImageService>> _mockLogger;
        private readonly string _testWebRoot;

        public PropertyImageServiceTests()
        {
            // 使用 InMemory 資料庫進行測試
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ZuHauseContext(options);

            // 設定測試環境
            _testWebRoot = Path.Combine(Path.GetTempPath(), "PropertyImageServiceTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testWebRoot);

            _mockImageProcessor = new Mock<IImageProcessor>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<PropertyImageService>>();

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testWebRoot);

            _service = new PropertyImageService(
                _context,
                _mockImageProcessor.Object,
                _mockEnvironment.Object,
                _mockLogger.Object);

            // 準備測試資料
            SeedTestData();
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

            SetupMockImageProcessor();

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(1);
            if (!results[0].Success)
            {
                // 調試輸出錯誤訊息
                Console.WriteLine($"Error: {results[0].ErrorMessage}");
            }
            results[0].Success.Should().BeTrue();
            results[0].IsMainImage.Should().BeTrue(); // 第一張應該是主圖
            results[0].OriginalFileName.Should().Be("test1.jpg");
            results[0].PropertyImageId.Should().BeGreaterThan(0);
            results[0].IsValidResult().Should().BeTrue();

            // 驗證資料庫記錄
            var dbImage = await _context.PropertyImages.FirstOrDefaultAsync(pi => pi.PropertyId == propertyId);
            dbImage.Should().NotBeNull();
            dbImage!.DisplayOrder.Should().Be(1);
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WithMultipleImages_ShouldSetFirstAsMainImage()
        {
            // Arrange
            var propertyId = 1;
            var files = CreateMockFiles(
                ("test1.jpg", 1024 * 1024, "image/jpeg"),
                ("test2.png", 512 * 1024, "image/png"),
                ("test3.jpg", 2 * 1024 * 1024, "image/jpeg") // 2MB
            );

            SetupMockImageProcessor();

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(3);
            results.All(r => r.Success).Should().BeTrue();
            
            results[0].IsMainImage.Should().BeTrue(); // 只有第一張是主圖
            results[1].IsMainImage.Should().BeFalse();
            results[2].IsMainImage.Should().BeFalse();

            // 驗證顯示順序
            var dbImages = await _context.PropertyImages
                .Where(pi => pi.PropertyId == propertyId)
                .OrderBy(pi => pi.DisplayOrder)
                .ToListAsync();

            dbImages.Should().HaveCount(3);
            dbImages[0].DisplayOrder.Should().Be(1);
            dbImages[1].DisplayOrder.Should().Be(2);
            dbImages[2].DisplayOrder.Should().Be(3);
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WithFileExceedingSize_ShouldReturnFailure()
        {
            // Arrange
            var propertyId = 1;
            var files = CreateMockFiles(("large.jpg", 3 * 1024 * 1024, "image/jpeg")); // 3MB 超過限制

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(1);
            results[0].Success.Should().BeFalse();
            results[0].ErrorMessage.Should().Contain("檔案大小超過");
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WithUnsupportedFormat_ShouldReturnFailure()
        {
            // Arrange
            var propertyId = 1;
            var files = CreateMockFiles(("test.gif", 1024, "image/gif")); // 不支援的格式

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(1);
            results[0].Success.Should().BeFalse();
            results[0].ErrorMessage.Should().Contain("不支援的檔案格式");
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WithTooManyFiles_ShouldReturnFailure()
        {
            // Arrange
            var propertyId = 1;
            var fileSpecs = Enumerable.Range(1, 16) // 16 張超過限制
                .Select(i => ($"test{i}.jpg", (long)1024, "image/jpeg"))
                .ToArray();
            var files = CreateMockFilesFromArray(fileSpecs);

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(1);
            results[0].Success.Should().BeFalse();
            results[0].ErrorMessage.Should().Contain("最多只能上傳 15 張圖片");
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WithExistingImages_ShouldRespectTotalLimit()
        {
            // Arrange
            var propertyId = 1;
            
            // 先新增 14 張圖片到資料庫
            for (int i = 1; i <= 14; i++)
            {
                _context.PropertyImages.Add(new PropertyImage
                {
                    PropertyId = propertyId,
                    ImagePath = $"/uploads/properties/{propertyId}/existing_{i}.webp",
                    DisplayOrder = i,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            // 嘗試再上傳 2 張（總共會超過 15 張限制）
            var files = CreateMockFiles(
                ("new1.jpg", 1024, "image/jpeg"),
                ("new2.jpg", 1024, "image/jpeg")
            );

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(1);
            results[0].Success.Should().BeFalse();
            results[0].ErrorMessage.Should().Contain("超過圖片數量限制");
            results[0].ErrorMessage.Should().Contain("目前已有 14 張");
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WithNonExistentProperty_ShouldReturnFailure()
        {
            // Arrange
            var propertyId = 999; // 不存在的房源ID
            var files = CreateMockFiles(("test.jpg", 1024, "image/jpeg"));

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(1);
            results[0].Success.Should().BeFalse();
            results[0].ErrorMessage.Should().Contain("房源 ID 999 不存在");
        }

        [Fact]
        public async Task UploadPropertyImagesAsync_WhenImageProcessorFails_ShouldReturnFailure()
        {
            // Arrange
            var propertyId = 1;
            var files = CreateMockFiles(("test.jpg", 1024, "image/jpeg"));

            // 設定 ImageProcessor 失敗
            _mockImageProcessor.Setup(ip => ip.ConvertToWebPAsync(It.IsAny<Stream>(), It.IsAny<int?>(), It.IsAny<int>()))
                .ReturnsAsync(ImageProcessingResult.CreateFailure("圖片處理引擎錯誤"));

            // Act
            var results = await _service.UploadPropertyImagesAsync(propertyId, files);

            // Assert
            results.Should().HaveCount(1);
            results[0].Success.Should().BeFalse();
            results[0].ErrorMessage.Should().Contain("原圖處理失敗");
        }

        [Fact]
        public void PropertyImageResult_CreateSuccess_ShouldCreateValidResult()
        {
            // Arrange & Act
            var result = PropertyImageResult.CreateSuccess(
                "test.jpg", 1, "/original.webp", "/medium.webp", "/thumb.webp", 
                true, 1024, 800, 1200, 800);

            // Assert
            result.Success.Should().BeTrue();
            result.IsValidResult().Should().BeTrue();
            result.GetDimensionDescription().Should().Be("1200x800");
            result.GetCompressionDescription().Should().Contain("節省");
            result.GetAllImagePaths().Should().HaveCount(3);
        }

        [Fact]
        public void PropertyImageResult_CreateFailure_ShouldCreateFailureResult()
        {
            // Arrange & Act
            var result = PropertyImageResult.CreateFailure("test.jpg", "測試錯誤");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("測試錯誤");
            result.IsValidResult().Should().BeFalse();
            result.GetProcessingSummary().Should().Contain("處理失敗");
        }

        private IFormFileCollection CreateMockFiles(params (string fileName, long size, string contentType)[] fileSpecs)
        {
            var files = new FormFileCollection();
            
            foreach (var (fileName, size, contentType) in fileSpecs)
            {
                var content = new byte[size];
                // 填入一些假的圖片數據 (為了測試檔案大小驗證)
                for (int i = 0; i < Math.Min(size, 1000); i++)
                {
                    content[i] = (byte)(i % 256);
                }

                var stream = new MemoryStream(content);
                var formFile = new FormFile(stream, 0, size, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };
                
                files.Add(formFile);
            }

            return files;
        }

        private IFormFileCollection CreateMockFilesFromArray((string fileName, long size, string contentType)[] fileSpecs)
        {
            return CreateMockFiles(fileSpecs);
        }

        private void SetupMockImageProcessor()
        {
            // 設定成功的圖片處理回應
            var successStream = new MemoryStream(new byte[1000]);
            
            _mockImageProcessor.Setup(ip => ip.ConvertToWebPAsync(It.IsAny<Stream>(), It.IsAny<int?>(), It.IsAny<int>()))
                .ReturnsAsync(() => ImageProcessingResult.CreateSuccess(successStream, 800, 600, "jpeg"));

            _mockImageProcessor.Setup(ip => ip.GenerateThumbnailAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(() => ImageProcessingResult.CreateSuccess(successStream, 300, 200, "jpeg"));
        }

        public void Dispose()
        {
            _context.Dispose();
            
            // 清理測試檔案
            if (Directory.Exists(_testWebRoot))
            {
                Directory.Delete(_testWebRoot, true);
            }
        }
    }
}