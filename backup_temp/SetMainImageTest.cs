using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Services;
using zuHause.Interfaces;

namespace zuHause.Tests.Unit
{
    /// <summary>
    /// 測試 ImageUploadService 中的 SetMainImageAsync 和 Property.PreviewImageUrl 自動同步功能
    /// </summary>
    public class SetMainImageTest
    {
        private readonly ITestOutputHelper _output;

        public SetMainImageTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task SetMainImageAsync_ShouldUpdatePropertyPreviewImageUrl_WhenImageIsPropertyGallery()
        {
            // Arrange - 建立測試用的 In-Memory 資料庫
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ZuHauseContext(options);

            // 建立測試資料
            var property = new Property
            {
                PropertyId = 1,
                Title = "測試房源",
                LandlordMemberId = 1,
                MonthlyRent = 10000,
                CityId = 1,
                DistrictId = 1,
                RoomCount = 2,
                LivingRoomCount = 1,
                BathroomCount = 1,
                CurrentFloor = 3,
                TotalFloors = 5,
                Area = 30,
                MinimumRentalMonths = 12,
                WaterFeeType = "包租",
                ElectricityFeeType = "包租",
                ManagementFeeIncluded = true,
                ParkingAvailable = false,
                ParkingFeeRequired = false,
                CleaningFeeRequired = false,
                IsPaid = false,
                StatusCode = "DRAFT",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PreviewImageUrl = null // 初始沒有預覽圖
            };

            var image1 = new Image
            {
                ImageId = 1,
                ImageGuid = Guid.NewGuid(),
                EntityType = EntityType.Property,
                EntityId = 1,
                Category = ImageCategory.Gallery,
                MimeType = "image/webp",
                OriginalFileName = "test1.jpg",
                StoredFileName = "gallery/1/abc123.webp",
                FileSizeBytes = 1000,
                Width = 800,
                Height = 600,
                DisplayOrder = 2, // 不是主圖
                IsActive = true,
                UploadedAt = DateTime.UtcNow
            };

            var image2 = new Image
            {
                ImageId = 2,
                ImageGuid = Guid.NewGuid(),
                EntityType = EntityType.Property,
                EntityId = 1,
                Category = ImageCategory.Gallery,
                MimeType = "image/webp",
                OriginalFileName = "test2.jpg",
                StoredFileName = "gallery/1/def456.webp",
                FileSizeBytes = 1200,
                Width = 800,
                Height = 600,
                DisplayOrder = 3, // 不是主圖
                IsActive = true,
                UploadedAt = DateTime.UtcNow
            };

            context.Properties.Add(property);
            context.Images.AddRange(image1, image2);
            await context.SaveChangesAsync();

            // 建立 Mock 服務
            var mockDisplayOrderManager = new Mock<IDisplayOrderManager>();
            var mockImageQueryService = new Mock<IImageQueryService>();
            var mockLogger = new Mock<ILogger<ImageUploadService>>();

            // Mock DisplayOrderManager.MoveImageToPositionAsync 回傳成功結果
            mockDisplayOrderManager
                .Setup(x => x.MoveImageToPositionAsync(It.IsAny<long>(), It.Is<int>(pos => pos == 1)))
                .ReturnsAsync(new MigrationResult 
                { 
                    IsSuccess = true, 
                    MovedFilePaths = new List<string>() 
                });

            // Mock IImageQueryService.GenerateImageUrl 回傳測試 URL
            mockImageQueryService
                .Setup(x => x.GenerateImageUrl(It.IsAny<string>(), It.Is<ImageSize>(size => size == ImageSize.Medium)))
                .Returns((string storedFileName, ImageSize size) => 
                    $"https://test.blob.core.windows.net/images/{storedFileName}?size={size}");

            // 建立其他必要的 Mock
            var mockImageProcessor = new Mock<IImageProcessor>();
            var mockEntityExistenceChecker = new Mock<IEntityExistenceChecker>();
            var mockBlobStorageService = new Mock<IBlobStorageService>();
            var mockTempSessionService = new Mock<ITempSessionService>();
            var mockHttpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

            // 建立 ImageUploadService
            var service = new ImageUploadService(
                context,
                mockImageProcessor.Object,
                mockEntityExistenceChecker.Object,
                mockDisplayOrderManager.Object,
                mockLogger.Object,
                mockBlobStorageService.Object,
                mockTempSessionService.Object,
                mockHttpContextAccessor.Object,
                mockImageQueryService.Object
            );

            _output.WriteLine($"🔍 測試開始 - 設定圖片 {image2.ImageId} 為主圖");
            _output.WriteLine($"📋 初始狀態 - Property.PreviewImageUrl: {property.PreviewImageUrl ?? "NULL"}");

            // Act - 呼叫 SetMainImageAsync，將 image2 設為主圖
            var result = await service.SetMainImageAsync(image2.ImageId);

            // Assert - 驗證結果
            result.Should().BeTrue("SetMainImageAsync 應該成功");
            
            // 重新載入房源資料以檢查 PreviewImageUrl 是否已更新
            await context.Entry(property).ReloadAsync();
            
            _output.WriteLine($"✅ 測試結果 - Property.PreviewImageUrl: {property.PreviewImageUrl ?? "NULL"}");
            
            property.PreviewImageUrl.Should().NotBeNullOrEmpty("Property.PreviewImageUrl 應該已被自動更新");
            property.PreviewImageUrl.Should().Contain(image2.StoredFileName, "PreviewImageUrl 應該包含新主圖的檔案名稱");

            _output.WriteLine("🎯 測試完成 - SetMainImageAsync 正確自動更新了 Property.PreviewImageUrl");
        }

        [Fact]
        public async Task DeleteMainImage_ShouldUpdatePropertyPreviewImageUrl_ToNextMainImage()
        {
            // Arrange - 建立測試用的 In-Memory 資料庫
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ZuHauseContext(options);

            // 建立測試資料 - 有主圖的房源
            var property = new Property
            {
                PropertyId = 2,
                Title = "測試房源2",
                LandlordMemberId = 1,
                MonthlyRent = 15000,
                CityId = 1,
                DistrictId = 1,
                RoomCount = 3,
                LivingRoomCount = 1,
                BathroomCount = 2,
                CurrentFloor = 2,
                TotalFloors = 10,
                Area = 45,
                MinimumRentalMonths = 12,
                WaterFeeType = "包租",
                ElectricityFeeType = "包租",
                ManagementFeeIncluded = true,
                ParkingAvailable = false,
                ParkingFeeRequired = false,
                CleaningFeeRequired = false,
                IsPaid = false,
                StatusCode = "DRAFT",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PreviewImageUrl = "https://test.blob.core.windows.net/images/gallery/2/mainimage.webp" // 已有預覽圖
            };

            var mainImage = new Image
            {
                ImageId = 10,
                ImageGuid = Guid.NewGuid(),
                EntityType = EntityType.Property,
                EntityId = 2,
                Category = ImageCategory.Gallery,
                MimeType = "image/webp",
                OriginalFileName = "main.jpg",
                StoredFileName = "gallery/2/mainimage.webp",
                FileSizeBytes = 1500,
                Width = 800,
                Height = 600,
                DisplayOrder = 1, // 主圖
                IsActive = true,
                UploadedAt = DateTime.UtcNow
            };

            var secondImage = new Image
            {
                ImageId = 11,
                ImageGuid = Guid.NewGuid(),
                EntityType = EntityType.Property,
                EntityId = 2,
                Category = ImageCategory.Gallery,
                MimeType = "image/webp",
                OriginalFileName = "second.jpg",
                StoredFileName = "gallery/2/secondimage.webp",
                FileSizeBytes = 1300,
                Width = 800,
                Height = 600,
                DisplayOrder = 2, // 第二張圖
                IsActive = true,
                UploadedAt = DateTime.UtcNow
            };

            context.Properties.Add(property);
            context.Images.AddRange(mainImage, secondImage);
            await context.SaveChangesAsync();

            // 建立 Mock 服務
            var mockDisplayOrderManager = new Mock<IDisplayOrderManager>();
            var mockImageQueryService = new Mock<IImageQueryService>();
            var mockLogger = new Mock<ILogger<ImageUploadService>>();

            // Mock IImageQueryService.GenerateImageUrl
            mockImageQueryService
                .Setup(x => x.GenerateImageUrl(It.IsAny<string>(), It.Is<ImageSize>(size => size == ImageSize.Medium)))
                .Returns((string storedFileName, ImageSize size) => 
                    $"https://test.blob.core.windows.net/images/{storedFileName}?size={size}");

            // Mock DisplayOrderManager.RemoveImageAndAdjustOrdersAsync 成功，並且第二張圖會成為新的主圖
            mockDisplayOrderManager
                .Setup(x => x.RemoveImageAndAdjustOrdersAsync(It.IsAny<long>()))
                .Callback<long>(imageId =>
                {
                    // 模擬刪除主圖後，第二張圖的 DisplayOrder 變為 1
                    if (imageId == mainImage.ImageId)
                    {
                        secondImage.DisplayOrder = 1;
                    }
                })
                .ReturnsAsync(new MigrationResult 
                { 
                    IsSuccess = true, 
                    MovedFilePaths = new List<string>() 
                });

            // 建立其他必要的 Mock
            var mockImageProcessor = new Mock<IImageProcessor>();
            var mockEntityExistenceChecker = new Mock<IEntityExistenceChecker>();
            var mockBlobStorageService = new Mock<IBlobStorageService>();
            var mockTempSessionService = new Mock<ITempSessionService>();
            var mockHttpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

            // 建立 ImageUploadService
            var service = new ImageUploadService(
                context,
                mockImageProcessor.Object,
                mockEntityExistenceChecker.Object,
                mockDisplayOrderManager.Object,
                mockLogger.Object,
                mockBlobStorageService.Object,
                mockTempSessionService.Object,
                mockHttpContextAccessor.Object,
                mockImageQueryService.Object
            );

            _output.WriteLine($"🔍 測試開始 - 刪除主圖 {mainImage.ImageId}");
            _output.WriteLine($"📋 初始狀態 - Property.PreviewImageUrl: {property.PreviewImageUrl}");

            // Act - 刪除主圖
            var result = await service.DeleteImageAsync(mainImage.ImageId);

            // Assert - 驗證結果
            result.Should().BeTrue("DeleteImageAsync 應該成功");
            
            // 重新載入房源資料以檢查 PreviewImageUrl 是否已更新
            await context.Entry(property).ReloadAsync();
            
            _output.WriteLine($"✅ 測試結果 - Property.PreviewImageUrl: {property.PreviewImageUrl ?? "NULL"}");
            
            property.PreviewImageUrl.Should().NotBeNullOrEmpty("Property.PreviewImageUrl 應該已更新為新主圖");
            property.PreviewImageUrl.Should().Contain(secondImage.StoredFileName, "PreviewImageUrl 應該包含新主圖的檔案名稱");

            _output.WriteLine("🎯 測試完成 - DeleteImageAsync 正確自動更新了 Property.PreviewImageUrl");
        }
    }
}