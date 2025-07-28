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
    /// 簡化的房源創建測試，用於診斷第二階段失敗問題
    /// </summary>
    public class SimplePropertyCreateTest : IClassFixture<AzureTestWebApplicationFactory<Program>>
    {
        private readonly AzureTestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SimplePropertyCreateTest(AzureTestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task SimplePropertyCreate_ShouldSucceed()
        {
            // 檢查測試開始時的資料庫連接
            using var initialScope = _factory.Services.CreateScope();
            var initialContext = initialScope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            var initialConnectionString = initialContext.Database.GetConnectionString();
            Console.WriteLine($"🔍 測試開始時的連接字串: {initialConnectionString}");
            
            // 設置身份驗證
            _client.DefaultRequestHeaders.Add("Cookie", $"UserId=51");
            
            // 獲取 AntiForgery Token
            var antiForgeryToken = await GetAntiForgeryTokenAsync();
            Console.WriteLine($"🔍 獲取到的 AntiForgery Token: {(string.IsNullOrEmpty(antiForgeryToken) ? "空" : $"長度 {antiForgeryToken.Length} 字符")}");
            
            var propertyCreateContent = new MultipartFormDataContent();
            
            // 加入 AntiForgery Token
            if (!string.IsNullOrEmpty(antiForgeryToken))
            {
                propertyCreateContent.Add(new StringContent(antiForgeryToken), "__RequestVerificationToken");
                Console.WriteLine("✅ AntiForgery Token 已添加到請求中");
            }
            else
            {
                Console.WriteLine("⚠️ 警告：未能獲取 AntiForgery Token");
            }
            
            // 房源基本資料
            propertyCreateContent.Add(new StringContent("簡單測試房源"), "Title");
            propertyCreateContent.Add(new StringContent("25000"), "MonthlyRent");
            propertyCreateContent.Add(new StringContent("2"), "CityId"); // 使用臺北市 (CityId=2)
            propertyCreateContent.Add(new StringContent("1"), "DistrictId"); // 使用大安區 (DistrictId=1，屬於臺北市)
            propertyCreateContent.Add(new StringContent("簡單測試地址"), "AddressLine");
            propertyCreateContent.Add(new StringContent("2"), "RoomCount");
            propertyCreateContent.Add(new StringContent("1"), "LivingRoomCount");
            propertyCreateContent.Add(new StringContent("1"), "BathroomCount");
            propertyCreateContent.Add(new StringContent("25"), "Area");
            propertyCreateContent.Add(new StringContent("5"), "CurrentFloor");
            propertyCreateContent.Add(new StringContent("10"), "TotalFloors");
            propertyCreateContent.Add(new StringContent("2"), "DepositMonths");
            propertyCreateContent.Add(new StringContent("12"), "MinimumRentalMonths");
            propertyCreateContent.Add(new StringContent("false"), "ManagementFeeIncluded");
            propertyCreateContent.Add(new StringContent("1000"), "ManagementFeeAmount"); // 當 ManagementFeeIncluded=false 時必須提供
            propertyCreateContent.Add(new StringContent("false"), "ParkingAvailable");
            propertyCreateContent.Add(new StringContent("簡單測試房源描述"), "Description");
            
            // 必填欄位
            propertyCreateContent.Add(new StringContent("台水"), "WaterFeeType");
            propertyCreateContent.Add(new StringContent("台電"), "ElectricityFeeType");
            propertyCreateContent.Add(new StringContent("2"), "ListingPlanId");
            propertyCreateContent.Add(new StringContent("1"), "SelectedEquipmentIds");

            // 執行房源創建
            Console.WriteLine("🏠 開始執行房源創建...");
            Console.WriteLine($"🔍 請求 Content-Type: {propertyCreateContent.Headers.ContentType}");
            Console.WriteLine($"🔍 請求方法: POST");
            Console.WriteLine($"🔍 請求 URL: /property/create");
            
            var response = await _client.PostAsync("/property/create", propertyCreateContent);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 回應狀態: {response.StatusCode}");
            Console.WriteLine($"🔍 回應長度: {responseContent.Length} 字符");
            Console.WriteLine($"🔍 回應內容前500字符: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");
            
            // 檢查是否是重定向回應
            if (response.Headers.Location != null)
            {
                Console.WriteLine($"🔍 重定向位置: {response.Headers.Location}");
            }
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ 房源創建失敗!");
                Console.WriteLine($"❌ 狀態碼: {response.StatusCode}");
                Console.WriteLine($"❌ 原因: {response.ReasonPhrase}");
                throw new Exception($"房源創建失敗 - 狀態碼: {response.StatusCode}");
            }

            // 如果 PropertyController 中的驗證成功，這裡也應該能找到房源
            // 不需要延遲，因為 Controller 已經在同一個事務中驗證了
            
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            
            var newProperty = await context.Properties
                .AsNoTracking()
                .Where(p => p.Title.Contains("簡單測試房源"))
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            Console.WriteLine($"🔍 測試端驗證 - 查詢結果: 找到 {(newProperty != null ? 1 : 0)} 個匹配的房源");
            
            if (newProperty != null)
            {
                Console.WriteLine($"✅ 測試端驗證成功 - 房源ID: {newProperty.PropertyId}");
                Console.WriteLine($"✅ 房源詳情 - Title: {newProperty.Title}, Status: {newProperty.StatusCode}");
            }
            else
            {
                Console.WriteLine("❌ 測試端驗證失敗 - 在資料庫中找不到房源");
                
                // 輸出調試信息
                var recentProperties = await context.Properties
                    .AsNoTracking()
                    .Where(p => p.CreatedAt >= DateTime.Now.AddMinutes(-5))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync();
                    
                Console.WriteLine($"🔍 最近 5 分鐘內創建的所有房源數量: {recentProperties.Count}");
                foreach (var prop in recentProperties)
                {
                    Console.WriteLine($"  - ID: {prop.PropertyId}, Title: {prop.Title}, Created: {prop.CreatedAt}");
                }
                
                // 檢查資料庫連接是否相同
                var connectionString = context.Database.GetConnectionString();
                Console.WriteLine($"🔍 測試端使用的完整連接字串: {connectionString}");
            }

            newProperty.Should().NotBeNull("PropertyController 驗證成功但測試端找不到房源，表明可能存在不同的資料庫連接");
        }

        private async Task<string> GetAntiForgeryTokenAsync()
        {
            var getResponse = await _client.GetAsync("/property/create");
            var content = await getResponse.Content.ReadAsStringAsync();
            
            var tokenMatch = System.Text.RegularExpressions.Regex.Match(content, 
                @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]*)"" />|""__RequestVerificationToken"":""([^""]*)""");
            
            if (tokenMatch.Success)
            {
                return tokenMatch.Groups[1].Value ?? tokenMatch.Groups[2].Value;
            }
            
            return string.Empty;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}