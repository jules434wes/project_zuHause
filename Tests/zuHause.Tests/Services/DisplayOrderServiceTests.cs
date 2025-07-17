using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Services;
using zuHause.Services.Interfaces;

namespace zuHause.Tests.Services
{
    public class DisplayOrderServiceTests : IDisposable
    {
        private readonly ZuHauseContext _context;
        private readonly IDisplayOrderService _service;

        public DisplayOrderServiceTests()
        {
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ZuHauseContext(options);
            _service = new DisplayOrderService(_context);

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

            // 新增測試圖片
            _context.Images.AddRange(
                new Image 
                { 
                    ImageId = 1,
                    ImageGuid = Guid.NewGuid(),
                    EntityType = EntityType.Property,
                    EntityId = 1,
                    Category = ImageCategory.Gallery,
                    MimeType = "image/jpeg",
                    OriginalFileName = "image1.jpg",
                    FileSizeBytes = 1024,
                    Width = 800,
                    Height = 600,
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Image 
                { 
                    ImageId = 2,
                    ImageGuid = Guid.NewGuid(),
                    EntityType = EntityType.Property,
                    EntityId = 1,
                    Category = ImageCategory.Gallery,
                    MimeType = "image/jpeg",
                    OriginalFileName = "image2.jpg",
                    FileSizeBytes = 1024,
                    Width = 800,
                    Height = 600,
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Image 
                { 
                    ImageId = 3,
                    ImageGuid = Guid.NewGuid(),
                    EntityType = EntityType.Property,
                    EntityId = 1,
                    Category = ImageCategory.Gallery,
                    MimeType = "image/jpeg",
                    OriginalFileName = "image3.jpg",
                    FileSizeBytes = 1024,
                    Width = 800,
                    Height = 600,
                    DisplayOrder = 3,
                    IsActive = true
                }
            );

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetNextDisplayOrderAsync_WithExistingImages_ShouldReturnCorrectOrder()
        {
            // Act
            var nextOrder = await _service.GetNextDisplayOrderAsync(EntityType.Property, 1, ImageCategory.Gallery);

            // Assert
            Assert.Equal(4, nextOrder);
        }

        [Fact]
        public async Task GetNextDisplayOrderAsync_WithNoExistingImages_ShouldReturnOne()
        {
            // Act
            var nextOrder = await _service.GetNextDisplayOrderAsync(EntityType.Property, 1, ImageCategory.BedRoom);

            // Assert
            Assert.Equal(1, nextOrder);
        }

        [Fact]
        public async Task ReorderImagesAsync_WithValidImageIds_ShouldReturnSuccess()
        {
            // Arrange
            var newOrder = new List<long> { 3, 1, 2 };

            // Act
            var result = await _service.ReorderImagesAsync(EntityType.Property, 1, ImageCategory.Gallery, newOrder);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.AffectedCount);

            // 驗證順序是否正確更新
            var images = await _context.Images
                .Where(i => i.EntityType == EntityType.Property && i.EntityId == 1 && i.Category == ImageCategory.Gallery)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            Assert.Equal(3, images[0].ImageId);
            Assert.Equal(1, images[1].ImageId);
            Assert.Equal(2, images[2].ImageId);
        }

        [Fact]
        public async Task ReorderImagesAsync_WithInvalidImageIds_ShouldReturnFailure()
        {
            // Arrange
            var invalidOrder = new List<long> { 1, 2, 999 };

            // Act
            var result = await _service.ReorderImagesAsync(EntityType.Property, 1, ImageCategory.Gallery, invalidOrder);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("找不到圖片ID", result.ErrorMessage);
        }

        [Fact]
        public async Task ReorderImagesAsync_WithEmptyList_ShouldReturnFailure()
        {
            // Arrange
            var emptyOrder = new List<long>();

            // Act
            var result = await _service.ReorderImagesAsync(EntityType.Property, 1, ImageCategory.Gallery, emptyOrder);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("圖片ID列表不能為空", result.ErrorMessage);
        }

        [Fact]
        public async Task SetAsPrimaryImageAsync_WithValidImageId_ShouldReturnSuccess()
        {
            // Act - 將第3張圖片設為主圖
            var result = await _service.SetAsPrimaryImageAsync(3);

            // Assert
            Assert.True(result.IsSuccess);

            // 驗證第3張圖片變成第1張
            var primaryImage = await _context.Images.FirstAsync(i => i.ImageId == 3);
            Assert.Equal(1, primaryImage.DisplayOrder);

            // 驗證其他圖片順序正確調整
            var images = await _context.Images
                .Where(i => i.EntityType == EntityType.Property && i.EntityId == 1 && i.Category == ImageCategory.Gallery)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            Assert.Equal(3, images[0].ImageId); // 原本第3張變成第1張
            Assert.Equal(1, images[1].ImageId); // 原本第1張變成第2張
            Assert.Equal(2, images[2].ImageId); // 原本第2張變成第3張
        }

        [Fact]
        public async Task SetAsPrimaryImageAsync_WithInvalidImageId_ShouldReturnFailure()
        {
            // Act
            var result = await _service.SetAsPrimaryImageAsync(999);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("找不到ID為 999 的圖片", result.ErrorMessage);
        }

        [Fact]
        public async Task BatchUpdateDisplayOrderAsync_WithValidUpdates_ShouldReturnSuccess()
        {
            // Arrange
            var updates = new List<DisplayOrderUpdate>
            {
                new DisplayOrderUpdate { ImageId = 1, NewDisplayOrder = 3 },
                new DisplayOrderUpdate { ImageId = 2, NewDisplayOrder = 1 },
                new DisplayOrderUpdate { ImageId = 3, NewDisplayOrder = 2 }
            };

            // Act
            var result = await _service.BatchUpdateDisplayOrderAsync(updates);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.AffectedCount);

            // 驗證順序是否正確更新
            var image1 = await _context.Images.FirstAsync(i => i.ImageId == 1);
            var image2 = await _context.Images.FirstAsync(i => i.ImageId == 2);
            var image3 = await _context.Images.FirstAsync(i => i.ImageId == 3);

            Assert.Equal(3, image1.DisplayOrder);
            Assert.Equal(1, image2.DisplayOrder);
            Assert.Equal(2, image3.DisplayOrder);
        }

        [Fact]
        public async Task BatchUpdateDisplayOrderAsync_WithInvalidOrder_ShouldReturnFailure()
        {
            // Arrange
            var updates = new List<DisplayOrderUpdate>
            {
                new DisplayOrderUpdate { ImageId = 1, NewDisplayOrder = 0 } // 無效順序
            };

            // Act
            var result = await _service.BatchUpdateDisplayOrderAsync(updates);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("無效的顯示順序", result.ErrorMessage);
        }

        [Fact]
        public async Task BatchUpdateDisplayOrderAsync_WithEmptyList_ShouldReturnFailure()
        {
            // Arrange
            var emptyUpdates = new List<DisplayOrderUpdate>();

            // Act
            var result = await _service.BatchUpdateDisplayOrderAsync(emptyUpdates);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("更新列表不能為空", result.ErrorMessage);
        }

        [Fact]
        public async Task CompactDisplayOrderAsync_ShouldAdjustSubsequentOrders()
        {
            // Arrange - 模擬移除第2張圖片（DisplayOrder = 2）
            var removedDisplayOrder = 2;

            // Act
            var result = await _service.CompactDisplayOrderAsync(EntityType.Property, 1, ImageCategory.Gallery, removedDisplayOrder);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.AffectedCount); // 只有第3張圖片被調整

            // 驗證第3張圖片的順序從3變成2
            var image3 = await _context.Images.FirstAsync(i => i.ImageId == 3);
            Assert.Equal(2, image3.DisplayOrder);

            // 驗證第1張圖片順序不變
            var image1 = await _context.Images.FirstAsync(i => i.ImageId == 1);
            Assert.Equal(1, image1.DisplayOrder);
        }

        [Fact]
        public async Task CompactDisplayOrderAsync_WithNoSubsequentImages_ShouldReturnSuccess()
        {
            // Arrange - 模擬移除最後一張圖片（DisplayOrder = 3）
            var removedDisplayOrder = 3;

            // Act
            var result = await _service.CompactDisplayOrderAsync(EntityType.Property, 1, ImageCategory.Gallery, removedDisplayOrder);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.AffectedCount); // 沒有圖片需要調整
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}