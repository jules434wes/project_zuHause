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
    /// 快速兩階段測試 - 專注於調試第二階段問題
    /// </summary>
    public class QuickTwoPhaseTest : IClassFixture<AzureTestWebApplicationFactory<Program>>
    {
        private readonly AzureTestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public QuickTwoPhaseTest(AzureTestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task QuickTwoPhase_ShouldWork()
        {
            Console.WriteLine("🚀 開始快速兩階段測試");
            
            using var isolatedClient = _factory.CreateClient();
            isolatedClient.DefaultRequestHeaders.Add("Cookie", $"UserId=51");
            
            // === 第一階段：臨時上傳 ===
            var imageFiles = RealFileBuilder.CreateRealFormFileCollection(2, "quick_test");
            
            var tempUploadContent = new MultipartFormDataContent();
            tempUploadContent.Add(new StringContent("Gallery"), "category");
            
            foreach (var file in imageFiles)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                tempUploadContent.Add(streamContent, "files", file.FileName);
            }

            var tempUploadResponse = await isolatedClient.PostAsync("/api/images/temp-upload", tempUploadContent);
            tempUploadResponse.IsSuccessStatusCode.Should().BeTrue("第一階段應該成功");
            
            var tempUploadResponseContent = await tempUploadResponse.Content.ReadAsStringAsync();
            var tempUploadData = JsonSerializer.Deserialize<JsonElement>(tempUploadResponseContent);
            var tempSessionId = tempUploadData.GetProperty("tempSessionId").GetString();
            
            Console.WriteLine($"✅ 第一階段完成 - TempSessionId: {tempSessionId}");
            
            // === 驗證臨時圖片 ===
            using var scope1 = _factory.Services.CreateScope();
            var tempSessionService = scope1.ServiceProvider.GetRequiredService<ITempSessionService>();
            var tempImages = await tempSessionService.GetTempImagesAsync(tempSessionId!);
            Console.WriteLine($"🔍 臨時會話中有 {tempImages.Count} 張圖片");
            
            // === 第二階段：房源創建 ===
            var antiForgeryToken = await GetAntiForgeryTokenAsync(isolatedClient);
            
            var propertyCreateContent = new MultipartFormDataContent();
            if (!string.IsNullOrEmpty(antiForgeryToken))
            {
                propertyCreateContent.Add(new StringContent(antiForgeryToken), "__RequestVerificationToken");
            }
            
            // 關鍵：傳遞 TempSessionId
            propertyCreateContent.Add(new StringContent(tempSessionId!), "TempSessionId");
            
            // 基本房源資料
            propertyCreateContent.Add(new StringContent("快速測試房源"), "Title");
            propertyCreateContent.Add(new StringContent("25000"), "MonthlyRent");
            propertyCreateContent.Add(new StringContent("2"), "CityId");
            propertyCreateContent.Add(new StringContent("1"), "DistrictId");
            propertyCreateContent.Add(new StringContent("測試地址"), "AddressLine");
            propertyCreateContent.Add(new StringContent("2"), "RoomCount");
            propertyCreateContent.Add(new StringContent("1"), "LivingRoomCount");
            propertyCreateContent.Add(new StringContent("1"), "BathroomCount");
            propertyCreateContent.Add(new StringContent("25"), "Area");
            propertyCreateContent.Add(new StringContent("5"), "CurrentFloor");
            propertyCreateContent.Add(new StringContent("10"), "TotalFloors");
            propertyCreateContent.Add(new StringContent("2"), "DepositMonths");
            propertyCreateContent.Add(new StringContent("12"), "MinimumRentalMonths");
            propertyCreateContent.Add(new StringContent("true"), "ManagementFeeIncluded");
            propertyCreateContent.Add(new StringContent("1000"), "ManagementFeeAmount");
            propertyCreateContent.Add(new StringContent("false"), "ParkingAvailable");
            propertyCreateContent.Add(new StringContent("快速測試描述"), "Description");
            propertyCreateContent.Add(new StringContent("台水"), "WaterFeeType");
            propertyCreateContent.Add(new StringContent("台電"), "ElectricityFeeType");
            propertyCreateContent.Add(new StringContent("2"), "ListingPlanId");
            propertyCreateContent.Add(new StringContent("1"), "SelectedEquipmentIds");

            Console.WriteLine($"🏠 開始第二階段房源創建...");
            var propertyCreateResponse = await isolatedClient.PostAsync("/property/create", propertyCreateContent);
            
            var propertyCreateResponseContent = await propertyCreateResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 第二階段狀態: {propertyCreateResponse.StatusCode}");
            Console.WriteLine($"🔍 第二階段回應內容前500字元: {propertyCreateResponseContent.Substring(0, Math.Min(500, propertyCreateResponseContent.Length))}");
            
            propertyCreateResponse.IsSuccessStatusCode.Should().BeTrue("第二階段應該成功");
            
            // === 檢查資料庫結果 ===
            using var scope2 = _factory.Services.CreateScope();
            var context = scope2.ServiceProvider.GetRequiredService<ZuHauseContext>();
            
            var createdProperty = await context.Properties
                .Where(p => p.Title == "快速測試房源")
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
                
            createdProperty.Should().NotBeNull("房源應該被創建");
            Console.WriteLine($"✅ 房源已創建 - ID: {createdProperty!.PropertyId}");
            
            var migratedImages = await context.Images
                .Where(img => img.EntityType == EntityType.Property 
                           && img.EntityId == createdProperty.PropertyId 
                           && img.IsActive)
                .ToListAsync();
                
            Console.WriteLine($"🔍 找到 {migratedImages.Count} 張遷移的圖片");
            foreach (var img in migratedImages)
            {
                Console.WriteLine($"  - {img.StoredFileName} (Category: {img.Category}, DisplayOrder: {img.DisplayOrder})");
            }
            
            migratedImages.Should().HaveCount(2, "應該有 2 張圖片被遷移");
            
            Console.WriteLine($"✅ 快速兩階段測試完成！");
        }
        
        private async Task<string> GetAntiForgeryTokenAsync(HttpClient client)
        {
            var response = await client.GetAsync("/property/create");
            var content = await response.Content.ReadAsStringAsync();
            
            var tokenStart = content.IndexOf("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"");
            if (tokenStart == -1) return string.Empty;
            
            tokenStart += "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"".Length;
            var tokenEnd = content.IndexOf("\"", tokenStart);
            
            return content.Substring(tokenStart, tokenEnd - tokenStart);
        }
    }
}