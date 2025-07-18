using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Services;
using zuHause.Services.Interfaces;

namespace zuHause.Tests.Services
{
    public class ImageValidationServiceTests : IDisposable
    {
        private readonly ZuHauseContext _context;
        private readonly IImageValidationService _service;

        public ImageValidationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ZuHauseContext(options);
            _service = new ImageValidationService(_context);

            // 設定測試資料
            SetupTestData();
        }

        private void SetupTestData()
        {
            // 新增測試會員
            _context.Members.Add(new Member 
            { 
                MemberId = 1, 
                Email = "test@example.com",
                MemberName = "Test User"
            });

            // 新增測試房源
            _context.Properties.Add(new Property 
            { 
                PropertyId = 1, 
                Title = "Test Property",
                LandlordMemberId = 1
            });

            _context.SaveChanges();
        }

        [Theory]
        [InlineData("image/jpeg", "test.jpg", true)]
        [InlineData("image/png", "test.png", true)]
        [InlineData("image/webp", "test.webp", true)]
        [InlineData("image/gif", "test.gif", false)]
        [InlineData("image/jpeg", "test.png", false)]
        [InlineData("", "test.jpg", false)]
        [InlineData("image/jpeg", "", false)]
        public void ValidateFileFormat_ShouldReturnExpectedResult(string mimeType, string fileName, bool expectedValid)
        {
            // Act
            var result = _service.ValidateFileFormat(mimeType, fileName);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
            if (!expectedValid)
            {
                Assert.True(result.Errors.Any());
            }
        }

        [Theory]
        [InlineData(ImageCategory.Avatar, 1024 * 1024, true)] // 1MB
        [InlineData(ImageCategory.Avatar, 3 * 1024 * 1024, false)] // 3MB
        [InlineData(ImageCategory.Gallery, 5 * 1024 * 1024, true)] // 5MB
        [InlineData(ImageCategory.Gallery, 15 * 1024 * 1024, false)] // 15MB
        [InlineData(ImageCategory.Product, 3 * 1024 * 1024, true)] // 3MB
        [InlineData(ImageCategory.Product, 7 * 1024 * 1024, false)] // 7MB
        public void ValidateFileSize_ShouldReturnExpectedResult(ImageCategory category, long fileSizeBytes, bool expectedValid)
        {
            // Act
            var result = _service.ValidateFileSize(fileSizeBytes, category);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
            if (!expectedValid)
            {
                Assert.True(result.Errors.Any());
            }
        }

        [Theory]
        [InlineData(ImageCategory.Avatar, 500, 500, true)]
        [InlineData(ImageCategory.Avatar, 50, 50, false)] // 太小
        [InlineData(ImageCategory.Avatar, 2000, 2000, false)] // 太大
        [InlineData(ImageCategory.Gallery, 800, 600, true)]
        [InlineData(ImageCategory.Gallery, 200, 150, false)] // 太小
        [InlineData(ImageCategory.Product, 500, 500, true)]
        [InlineData(ImageCategory.Product, 100, 100, false)] // 太小
        public void ValidateImageDimensions_ShouldReturnExpectedResult(ImageCategory category, int width, int height, bool expectedValid)
        {
            // Act
            var result = _service.ValidateImageDimensions(width, height, category);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
            if (!expectedValid)
            {
                Assert.True(result.Errors.Any());
            }
        }

        [Theory]
        [InlineData(EntityType.Member, 1, true)]
        [InlineData(EntityType.Property, 1, true)]
        [InlineData(EntityType.Member, 999, false)]
        [InlineData(EntityType.Property, 999, false)]
        public async Task ValidateEntityRelationAsync_ShouldReturnExpectedResult(EntityType entityType, int entityId, bool expectedValid)
        {
            // Act
            var result = await _service.ValidateEntityRelationAsync(entityType, entityId);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
            if (!expectedValid)
            {
                Assert.True(result.Errors.Any());
            }
        }

        [Fact]
        public async Task ValidateImageAsync_WithValidImage_ShouldReturnSuccess()
        {
            // Arrange
            var image = new Image
            {
                EntityType = EntityType.Property,
                EntityId = 1,
                Category = ImageCategory.Gallery,
                MimeType = "image/jpeg",
                OriginalFileName = "test.jpg",
                FileSizeBytes = 2 * 1024 * 1024, // 2MB
                Width = 800,
                Height = 600,
                DisplayOrder = 1
            };

            // Act
            var result = await _service.ValidateImageAsync(image);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateImageAsync_WithInvalidImage_ShouldReturnFailure()
        {
            // Arrange
            var image = new Image
            {
                EntityType = EntityType.Property,
                EntityId = 999, // 不存在的實體
                Category = ImageCategory.Avatar,
                MimeType = "image/gif", // 不支援的格式
                OriginalFileName = "test.gif",
                FileSizeBytes = 5 * 1024 * 1024, // 超過大小限制
                Width = 50, // 太小
                Height = 50, // 太小
                DisplayOrder = -1 // 無效順序
            };

            // Act
            var result = await _service.ValidateImageAsync(image);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Count >= 5); // 應該有多個錯誤
        }

        [Fact]
        public async Task ValidateImageAsync_WithNullImage_ShouldReturnFailure()
        {
            // Act
            var result = await _service.ValidateImageAsync(null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("圖片實體不能為空", result.Errors);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidateFileSize_WithInvalidSize_ShouldReturnFailure(long invalidSize)
        {
            // Act
            var result = _service.ValidateFileSize(invalidSize, ImageCategory.Gallery);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("檔案大小必須大於 0", result.Errors);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(100, 0)]
        [InlineData(-1, 100)]
        [InlineData(100, -1)]
        public void ValidateImageDimensions_WithInvalidDimensions_ShouldReturnFailure(int width, int height)
        {
            // Act
            var result = _service.ValidateImageDimensions(width, height, ImageCategory.Gallery);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("圖片尺寸必須大於 0", result.Errors);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ValidateEntityRelationAsync_WithInvalidEntityId_ShouldReturnFailure(int invalidEntityId)
        {
            // Act
            var result = await _service.ValidateEntityRelationAsync(EntityType.Member, invalidEntityId);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("實體ID必須大於 0", result.Errors);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}