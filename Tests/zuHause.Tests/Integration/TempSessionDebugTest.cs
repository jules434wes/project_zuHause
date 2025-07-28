using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using zuHause.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace zuHause.Tests.Integration
{
    /// <summary>
    /// 專門調試臨時會話問題的測試
    /// </summary>
    public class TempSessionDebugTest : IClassFixture<AzureTestWebApplicationFactory<Program>>
    {
        private readonly AzureTestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TempSessionDebugTest(AzureTestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task DebugTempSession_ShouldPreserveTempImages()
        {
            Console.WriteLine("🔍 開始調試臨時會話問題");
            
            // === 第一步：執行第一階段臨時上傳 ===
            var imageFiles = RealFileBuilder.CreateRealFormFileCollection(2, "debug_test");
            
            var tempUploadContent = new MultipartFormDataContent();
            tempUploadContent.Add(new StringContent("Gallery"), "category");
            
            foreach (var file in imageFiles)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                tempUploadContent.Add(streamContent, "files", file.FileName);
            }

            var tempUploadResponse = await _client.PostAsync("/api/images/temp-upload", tempUploadContent);
            tempUploadResponse.IsSuccessStatusCode.Should().BeTrue("第一階段應該成功");
            
            var tempUploadResponseContent = await tempUploadResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 第一階段回應: {tempUploadResponseContent}");
            
            var tempUploadData = JsonSerializer.Deserialize<JsonElement>(tempUploadResponseContent);
            var tempSessionId = tempUploadData.GetProperty("tempSessionId").GetString();
            
            Console.WriteLine($"✅ 第一階段完成 - TempSessionId: {tempSessionId}");
            Console.WriteLine($"✅ 上傳圖片數量: {tempUploadData.GetProperty("images").GetArrayLength()}");
            
            // === 第二步：直接檢查臨時會話服務 ===
            using var scope = _factory.Services.CreateScope();
            var tempSessionService = scope.ServiceProvider.GetRequiredService<ITempSessionService>();
            
            Console.WriteLine($"🔍 檢查臨時會話服務中的圖片...");
            var tempImages = await tempSessionService.GetTempImagesAsync(tempSessionId!);
            
            Console.WriteLine($"🔍 臨時會話中找到 {tempImages.Count} 張圖片");
            foreach (var tempImg in tempImages)
            {
                Console.WriteLine($"  - 圖片: {tempImg.ImageGuid}, 分類: {tempImg.Category}, 檔名: {tempImg.OriginalFileName}");
            }
            
            // 驗證
            tempImages.Should().HaveCount(2, "應該有 2 張臨時圖片");
            tempImages.All(img => img.Category == ImageCategory.Gallery).Should().BeTrue("所有圖片應該是 Gallery 分類");
            
            Console.WriteLine($"✅ 臨時會話調試測試完成！");
        }
    }
}