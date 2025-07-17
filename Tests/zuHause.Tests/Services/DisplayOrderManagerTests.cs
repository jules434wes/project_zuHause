using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Services;
using zuHause.Interfaces;

namespace zuHause.Tests.Services
{
    public class DisplayOrderManagerTests : IDisposable
    {
        private readonly ZuHauseContext _context;
        private readonly IDisplayOrderManager _service;

        public DisplayOrderManagerTests()
        {
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ZuHauseContext(options);
            _service = new DisplayOrderManager(_context);

            // 設定測試資料
            SetupTestData();
        }

        private void SetupTestData()
        {
            // 新增測試會員和房源
            _context.Members.Add(new Member { MemberId = 1, Email = "test@example.com", MemberName = "Test User" });
            _context.Properties.Add(new Property { PropertyId = 1, Title = "Test Property", LandlordMemberId = 1 });

            // 新增測試圖片
            _context.Images.AddRange(
                new Image 
                { 
                    ImageId = 1, ImageGuid = Guid.NewGuid(), EntityType = EntityType.Property, EntityId = 1,
                    Category = ImageCategory.Gallery, MimeType = "image/jpeg", OriginalFileName = "image1.jpg",
                    FileSizeBytes = 1024, Width = 800, Height = 600, DisplayOrder = 1, IsActive = true
                },
                new Image 
                { 
                    ImageId = 2, ImageGuid = Guid.NewGuid(), EntityType = EntityType.Property, EntityId = 1,
                    Category = ImageCategory.Gallery, MimeType = "image/jpeg", OriginalFileName = "image2.jpg",
                    FileSizeBytes = 1024, Width = 800, Height = 600, DisplayOrder = 2, IsActive = true
                },
                new Image 
                { 
                    ImageId = 3, ImageGuid = Guid.NewGuid(), EntityType = EntityType.Property, EntityId = 1,
                    Category = ImageCategory.Gallery, MimeType = "image/jpeg", OriginalFileName = "image3.jpg",
                    FileSizeBytes = 1024, Width = 800, Height = 600, DisplayOrder = 3, IsActive = true
                }
            );

            _context.SaveChanges();
        }

        [Fact]
        public async Task AssignDisplayOrdersAsync_WithValidImageIds_ShouldReturnSuccess()
        {
            // Arrange
            var newImageIds = new List<long> { 4, 5, 6 };
            
            // 先新增圖片但不設定 DisplayOrder
            _context.Images.AddRange(
                new Image { ImageId = 4, ImageGuid = Guid.NewGuid(), EntityType = EntityType.Property, EntityId = 1, Category = ImageCategory.Gallery, MimeType = "image/jpeg", OriginalFileName = "image4.jpg", FileSizeBytes = 1024, Width = 800, Height = 600, IsActive = true },
                new Image { ImageId = 5, ImageGuid = Guid.NewGuid(), EntityType = EntityType.Property, EntityId = 1, Category = ImageCategory.Gallery, MimeType = "image/jpeg", OriginalFileName = "image5.jpg", FileSizeBytes = 1024, Width = 800, Height = 600, IsActive = true },
                new Image { ImageId = 6, ImageGuid = Guid.NewGuid(), EntityType = EntityType.Property, EntityId = 1, Category = ImageCategory.Gallery, MimeType = "image/jpeg", OriginalFileName = "image6.jpg", FileSizeBytes = 1024, Width = 800, Height = 600, IsActive = true }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.AssignDisplayOrdersAsync(EntityType.Property, 1, ImageCategory.Gallery, newImageIds);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.AffectedCount);
            Assert.Equal(3, result.AssignedOrders.Count);
            
            // 驗證分配的順序是連續的
            var assignedValues = result.AssignedOrders.Values.OrderBy(v => v).ToList();
            Assert.Equal(4, assignedValues[0]); // 應該從 4 開始（前面已有 1,2,3）
            Assert.Equal(5, assignedValues[1]);
            Assert.Equal(6, assignedValues[2]);
        }

        [Fact]
        public async Task AssignDisplayOrdersAsync_WithNonExistentImageIds_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentImageIds = new List<long> { 999, 1000 };

            // Act
            var result = await _service.AssignDisplayOrdersAsync(EntityType.Property, 1, ImageCategory.Gallery, nonExistentImageIds);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("找不到圖片ID", result.ErrorMessage);
        }

        [Fact]
        public async Task AssignDisplayOrdersAsync_WithEmptyImageIds_ShouldReturnFailure()
        {
            // Arrange
            var emptyImageIds = new List<long>();

            // Act
            var result = await _service.AssignDisplayOrdersAsync(EntityType.Property, 1, ImageCategory.Gallery, emptyImageIds);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("圖片ID列表不能為空", result.ErrorMessage);
        }

        [Fact]
        public async Task GetNextDisplayOrderAsync_WithExistingImages_ShouldReturnCorrectOrder()
        {
            // Act
            var nextOrder = await _service.GetNextDisplayOrderAsync(EntityType.Property, 1, ImageCategory.Gallery);

            // Assert
            Assert.Equal(4, nextOrder); // 目前最大 DisplayOrder 是 3，所以下一個應該是 4
        }

        [Fact]
        public async Task GetNextDisplayOrderAsync_WithNoExistingImages_ShouldReturnOne()
        {
            // Act
            var nextOrder = await _service.GetNextDisplayOrderAsync(EntityType.Property, 1, ImageCategory.BedRoom);

            // Assert
            Assert.Equal(1, nextOrder); // 沒有圖片，應該返回 1
        }

        [Fact]
        public async Task ReorderDisplayOrdersAsync_WithExistingImages_ShouldReturnSuccess()
        {
            // Arrange - 人為製造順序不連續的情況
            var images = await _context.Images.Where(i => i.EntityType == EntityType.Property && i.EntityId == 1).ToListAsync();
            images[0].DisplayOrder = 1;
            images[1].DisplayOrder = 5; // 跳過 2,3,4
            images[2].DisplayOrder = 8; // 跳過 6,7
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ReorderDisplayOrdersAsync(EntityType.Property, 1, ImageCategory.Gallery);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.BeforeCount);
            Assert.Equal(3, result.AfterCount);
            Assert.Equal(2, result.UpdatedImageIds.Count); // 只有 2 個圖片需要更新順序

            // 驗證重新排序後的順序
            var reorderedImages = await _context.Images
                .Where(i => i.EntityType == EntityType.Property && i.EntityId == 1 && i.Category == ImageCategory.Gallery)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            Assert.Equal(1, reorderedImages[0].DisplayOrder);
            Assert.Equal(2, reorderedImages[1].DisplayOrder);
            Assert.Equal(3, reorderedImages[2].DisplayOrder);
        }

        [Fact]
        public async Task MoveImageToPositionAsync_WithValidImage_ShouldReturnSuccess()
        {
            // Arrange - 將第3張圖片移到第1位
            var imageId = 3;
            var newPosition = 1;

            // Act
            var result = await _service.MoveImageToPositionAsync(imageId, newPosition);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.OldPosition);
            Assert.Equal(1, result.NewPosition);
            Assert.Equal(3, result.AffectedCount); // 3張圖片都會受影響

            // 驗證移動後的順序
            var images = await _context.Images
                .Where(i => i.EntityType == EntityType.Property && i.EntityId == 1 && i.Category == ImageCategory.Gallery)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            Assert.Equal(3, images[0].ImageId); // 原本第3張變成第1張
            Assert.Equal(1, images[1].ImageId); // 原本第1張變成第2張
            Assert.Equal(2, images[2].ImageId); // 原本第2張變成第3張
        }

        [Fact]
        public async Task MoveImageToPositionAsync_WithNonExistentImage_ShouldReturnFailure()
        {
            // Act
            var result = await _service.MoveImageToPositionAsync(999, 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("找不到ID為 999 的圖片", result.ErrorMessage);
        }

        [Fact]
        public async Task RemoveImageAndAdjustOrdersAsync_WithValidImage_ShouldReturnSuccess()
        {
            // Arrange - 移除第2張圖片
            var imageId = 2;

            // Act
            var result = await _service.RemoveImageAndAdjustOrdersAsync(imageId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.RemovedPosition);
            Assert.Equal(1, result.AdjustedCount); // 只有第3張圖片需要調整

            // 驗證移除後的狀態
            var removedImage = await _context.Images.FirstOrDefaultAsync(i => i.ImageId == imageId);
            Assert.False(removedImage!.IsActive); // 應該被標記為非活動

            // 驗證後續圖片的順序調整
            var remainingImages = await _context.Images
                .Where(i => i.EntityType == EntityType.Property && i.EntityId == 1 && i.Category == ImageCategory.Gallery && i.IsActive)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            Assert.Equal(2, remainingImages.Count);
            Assert.Equal(1, remainingImages[0].DisplayOrder);
            Assert.Equal(2, remainingImages[1].DisplayOrder); // 原本第3張變成第2張
        }

        [Fact]
        public async Task RemoveImageAndAdjustOrdersAsync_WithNonExistentImage_ShouldReturnFailure()
        {
            // Act
            var result = await _service.RemoveImageAndAdjustOrdersAsync(999);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("找不到ID為 999 的圖片", result.ErrorMessage);
        }

        [Theory]
        [InlineData(ConcurrencyControlStrategy.OptimisticLock)]
        [InlineData(ConcurrencyControlStrategy.PessimisticLock)]
        [InlineData(ConcurrencyControlStrategy.NoLock)]
        public async Task AssignDisplayOrdersAsync_WithDifferentStrategies_ShouldWork(ConcurrencyControlStrategy strategy)
        {
            // Arrange
            var newImageIds = new List<long> { 4 };
            _context.Images.Add(new Image 
            { 
                ImageId = 4, ImageGuid = Guid.NewGuid(), EntityType = EntityType.Property, EntityId = 1, 
                Category = ImageCategory.Gallery, MimeType = "image/jpeg", OriginalFileName = "image4.jpg", 
                FileSizeBytes = 1024, Width = 800, Height = 600, IsActive = true 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.AssignDisplayOrdersAsync(EntityType.Property, 1, ImageCategory.Gallery, newImageIds, strategy);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.AffectedCount);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}