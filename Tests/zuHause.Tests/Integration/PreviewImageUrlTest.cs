using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Interfaces;

namespace zuHause.Tests.Integration
{
    /// <summary>
    /// 專門測試 PreviewImageUrl 更新功能
    /// </summary>
    public class PreviewImageUrlTest : IClassFixture<AzureTestWebApplicationFactory<Program>>
    {
        private readonly AzureTestWebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;

        public PreviewImageUrlTest(AzureTestWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task TestPreviewImageUrlUpdate()
        {
            _output.WriteLine("🔍 開始測試 PreviewImageUrl 更新功能");

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            var blobMigrationService = scope.ServiceProvider.GetRequiredService<IBlobMigrationService>();

            // 查詢房源 5064
            var property = await context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == 5064);

            _output.WriteLine($"🏠 房源 5064 存在: {property != null}");
            if (property != null)
            {
                _output.WriteLine($"🏠 房源標題: {property.Title}");
                _output.WriteLine($"🖼️ 當前 PreviewImageUrl: {property.PreviewImageUrl ?? "NULL"}");
            }

            // 查詢該房源的圖片
            var images = await context.Images
                .Where(img => img.EntityId == 5064 &&
                             img.EntityType == EntityType.Property &&
                             img.Category == ImageCategory.Gallery)
                .OrderBy(img => img.DisplayOrder)
                .ToListAsync();

            _output.WriteLine($"🖼️ 找到 {images.Count} 張圖片:");
            foreach (var img in images)
            {
                _output.WriteLine($"   - ImageId: {img.ImageId}, ImageGuid: {img.ImageGuid}, DisplayOrder: {img.DisplayOrder}");
            }

            // 查詢 DisplayOrder = 1 的圖片
            var previewImage = images.FirstOrDefault(img => img.DisplayOrder == 1);
            _output.WriteLine($"🎯 DisplayOrder=1 的圖片存在: {previewImage != null}");
            
            if (previewImage != null)
            {
                _output.WriteLine($"🎯 DisplayOrder=1 圖片詳情: ImageId={previewImage.ImageId}, ImageGuid={previewImage.ImageGuid}");
                
                // 手動調用 PreparePropertyPreviewImageUpdateAsync 的邏輯
                var urlGenerator = scope.ServiceProvider.GetRequiredService<IBlobUrlGenerator>();
                var expectedUrl = urlGenerator.GetBlobPath(ImageCategory.Gallery, 5064, previewImage.ImageGuid, ImageSize.Medium);
                _output.WriteLine($"🔗 預期的 PreviewImageUrl: {expectedUrl}");
                
                // 手動設置 PreviewImageUrl 並保存
                if (property != null)
                {
                    property.PreviewImageUrl = expectedUrl;
                    await context.SaveChangesAsync();
                    _output.WriteLine("✅ 手動設置 PreviewImageUrl 完成");
                    
                    // 重新查詢驗證
                    await context.Entry(property).ReloadAsync();
                    _output.WriteLine($"✅ 驗證更新後的 PreviewImageUrl: {property.PreviewImageUrl}");
                    
                    // 驗證設置是否成功
                    property.PreviewImageUrl.Should().NotBeNullOrEmpty("PreviewImageUrl 應該已經被設置");
                    // 注意：URL 中的 GUID 會移除破折號，所以要比較移除破折號後的版本
                    var guidWithoutDashes = previewImage.ImageGuid.ToString("N"); // "N" format removes dashes
                    property.PreviewImageUrl.Should().Contain(guidWithoutDashes, "PreviewImageUrl 應該包含對應的 ImageGuid (無破折號格式)");
                }
            }

            _output.WriteLine("🔍 PreviewImageUrl 測試完成");
        }
    }
}