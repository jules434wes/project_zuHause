using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Xunit;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Services;
using zuHause.Interfaces;

namespace zuHause.Tests.Services
{
    public class ImageQueryServiceTests : IDisposable
    {
        private readonly ZuHauseContext _context;
        private readonly IImageQueryService _service;

        public ImageQueryServiceTests()
        {
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ZuHauseContext(options);

            // 設定 Configuration Mock
            var inMemorySettings = new Dictionary<string, string> {
                {"ImageSettings:BaseUrl", "/images/"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _service = new ImageQueryService(_context, configuration);

            // 設定測試資料
            SetupTestData();
        }

        private void SetupTestData()
        {
            // 新增測試圖片
            _context.Images.AddRange(
                // Property 1 的圖片
                new Image 
                { 
                    ImageId = 1, 
                    ImageGuid = new Guid("11111111-1111-1111-1111-111111111111"),
                    EntityType = EntityType.Property, 
                    EntityId = 1, 
                    Category = ImageCategory.Living,
                    StoredFileName = "property1_main.jpg",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Image 
                { 
                    ImageId = 2, 
                    ImageGuid = new Guid("22222222-2222-2222-2222-222222222222"),
                    EntityType = EntityType.Property, 
                    EntityId = 1, 
                    Category = ImageCategory.BedRoom,
                    StoredFileName = "property1_bedroom.jpg",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Image 
                { 
                    ImageId = 3, 
                    ImageGuid = new Guid("33333333-3333-3333-3333-333333333333"),
                    EntityType = EntityType.Property, 
                    EntityId = 1, 
                    Category = ImageCategory.Kitchen,
                    StoredFileName = "property1_kitchen.jpg",
                    DisplayOrder = null, // 測試 null DisplayOrder
                    IsActive = true
                },
                // Property 2 的圖片
                new Image 
                { 
                    ImageId = 4, 
                    ImageGuid = new Guid("44444444-4444-4444-4444-444444444444"),
                    EntityType = EntityType.Property, 
                    EntityId = 2, 
                    Category = ImageCategory.Living,
                    StoredFileName = "property2_main.jpg",
                    DisplayOrder = 1,
                    IsActive = true
                },
                // 非啟用圖片
                new Image 
                { 
                    ImageId = 5, 
                    ImageGuid = new Guid("55555555-5555-5555-5555-555555555555"),
                    EntityType = EntityType.Property, 
                    EntityId = 1, 
                    Category = ImageCategory.Living,
                    StoredFileName = "property1_inactive.jpg",
                    DisplayOrder = 0, // 最小 DisplayOrder 但非啟用
                    IsActive = false
                },
                // Member 圖片
                new Image 
                { 
                    ImageId = 6, 
                    ImageGuid = new Guid("66666666-6666-6666-6666-666666666666"),
                    EntityType = EntityType.Member, 
                    EntityId = 1, 
                    Category = ImageCategory.Avatar,
                    StoredFileName = "member1_avatar.jpg",
                    DisplayOrder = 1,
                    IsActive = true
                }
            );

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetImagesByEntityAsync_ValidPropertyId_ReturnsOrderedImages()
        {
            // Act
            var result = await _service.GetImagesByEntityAsync(EntityType.Property, 1);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].ImageId); // DisplayOrder = 1
            Assert.Equal(2, result[1].ImageId); // DisplayOrder = 2
            Assert.Equal(3, result[2].ImageId); // DisplayOrder = null，按 ImageId 排序
        }

        [Fact]
        public async Task GetImagesByEntityAsync_InvalidEntityId_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetImagesByEntityAsync(EntityType.Property, 0);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetImagesByEntityAsync_NonExistentEntity_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetImagesByEntityAsync(EntityType.Property, 999);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMainImageAsync_ValidEntity_ReturnsImageWithSmallestDisplayOrder()
        {
            // Act
            var result = await _service.GetMainImageAsync(EntityType.Property, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ImageId);
            Assert.Equal(1, result.DisplayOrder);
        }

        [Fact]
        public async Task GetMainImageAsync_OnlyInactiveImages_ReturnsNull()
        {
            // Arrange - 設定只有非啟用圖片的實體
            _context.Images.Add(new Image 
            { 
                ImageId = 7, 
                EntityType = EntityType.Property, 
                EntityId = 3, 
                StoredFileName = "inactive.jpg",
                IsActive = false
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMainImageAsync(EntityType.Property, 3);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetImagesByCategoryAsync_ValidCategory_ReturnsFilteredImages()
        {
            // Act
            var result = await _service.GetImagesByCategoryAsync(EntityType.Property, 1, ImageCategory.Living);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].ImageId);
            Assert.Equal(ImageCategory.Living, result[0].Category);
        }

        [Fact]
        public async Task GetImageByGuidAsync_ValidGuid_ReturnsImage()
        {
            // Arrange
            var testGuid = new Guid("11111111-1111-1111-1111-111111111111");

            // Act
            var result = await _service.GetImageByGuidAsync(testGuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ImageId);
            Assert.Equal(testGuid, result.ImageGuid);
        }

        [Fact]
        public async Task GetImageByGuidAsync_EmptyGuid_ReturnsNull()
        {
            // Act
            var result = await _service.GetImageByGuidAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetImageByIdAsync_ValidId_ReturnsImage()
        {
            // Act
            var result = await _service.GetImageByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ImageId);
        }

        [Fact]
        public async Task GetImageByIdAsync_InvalidId_ReturnsNull()
        {
            // Act
            var result = await _service.GetImageByIdAsync(0);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task HasImagesAsync_EntityWithImages_ReturnsTrue()
        {
            // Act
            var result = await _service.HasImagesAsync(EntityType.Property, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasImagesAsync_EntityWithoutImages_ReturnsFalse()
        {
            // Act
            var result = await _service.HasImagesAsync(EntityType.Property, 999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetImageCountAsync_EntityWithImages_ReturnsCorrectCount()
        {
            // Act
            var result = await _service.GetImageCountAsync(EntityType.Property, 1);

            // Assert
            Assert.Equal(3, result); // 只計算啟用的圖片
        }

        [Fact]
        public async Task GetImageCountAsync_EntityWithoutImages_ReturnsZero()
        {
            // Act
            var result = await _service.GetImageCountAsync(EntityType.Property, 999);

            // Assert
            Assert.Equal(0, result);
        }

        [Theory]
        [InlineData(ImageSize.Original, "/images/original/test.jpg")]
        [InlineData(ImageSize.Large, "/images/large/test.jpg")]
        [InlineData(ImageSize.Medium, "/images/medium/test.jpg")]
        [InlineData(ImageSize.Thumbnail, "/images/thumbnails/test.jpg")]
        public void GenerateImageUrl_ValidFileName_ReturnsCorrectUrl(ImageSize size, string expectedUrl)
        {
            // Act
            var result = _service.GenerateImageUrl("test.jpg", size);

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void GenerateImageUrl_EmptyFileName_ReturnsEmptyString()
        {
            // Act
            var result = _service.GenerateImageUrl("", ImageSize.Original);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GenerateImageUrls_ValidImageList_ReturnsDictionary()
        {
            // Arrange
            var images = new List<Image>
            {
                new Image { ImageId = 1, StoredFileName = "image1.jpg" },
                new Image { ImageId = 2, StoredFileName = "image2.jpg" }
            };

            // Act
            var result = _service.GenerateImageUrls(images, ImageSize.Thumbnail);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("/images/thumbnails/image1.jpg", result[1]);
            Assert.Equal("/images/thumbnails/image2.jpg", result[2]);
        }

        [Fact]
        public void GenerateImageUrls_EmptyList_ReturnsEmptyDictionary()
        {
            // Act
            var result = _service.GenerateImageUrls(new List<Image>(), ImageSize.Original);

            // Assert
            Assert.Empty(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}