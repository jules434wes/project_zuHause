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
    /// 專門調試第二階段房源創建問題的測試類
    /// 重點：找出為什麼房源創建失敗或事務回滾
    /// </summary>
    public class PropertyCreateDebugTests : IClassFixture<AzureTestWebApplicationFactory<Program>>, IDisposable
    {
        private readonly AzureTestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PropertyCreateDebugTests(AzureTestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// 測試第二階段：僅使用固定的 tempSessionId 測試房源創建
        /// 目標：專注診斷為什麼房源創建失敗
        /// </summary>
        [Fact]
        public async Task Debug_PropertyCreate_WithFixedTempSessionId()
        {
            Console.WriteLine("🔍 開始調試第二階段房源創建問題");

            // 使用獨立的 HttpClient
            using var debugClient = _factory.CreateClient();

            // 先進行第一階段以獲取真實的 tempSessionId
            Console.WriteLine("📋 第一階段：創建真實的 tempSessionId");
            var tempSessionId = await CreateRealTempSessionAsync(debugClient);
            Console.WriteLine($"✅ 取得 tempSessionId: {tempSessionId}");

            // 驗證臨時會話中的圖片
            await VerifyTempSessionImagesAsync(tempSessionId);

            // 設置身份驗證並取得 AntiForgery Token
            Console.WriteLine("🔑 設置身份驗證...");
            var antiForgeryToken = await SetupAuthenticationAndGetTokenAsync(debugClient, 51);
            Console.WriteLine($"🔑 AntiForgery Token: {(!string.IsNullOrEmpty(antiForgeryToken) ? "獲取成功" : "獲取失敗")}");

            // 準備房源創建請求
            Console.WriteLine("📝 準備房源創建請求...");
            var propertyCreateContent = BuildPropertyCreateContent(tempSessionId, antiForgeryToken);

            // 記錄請求內容
            LogRequestContent(propertyCreateContent);

            // 在創建前檢查資料庫狀態
            await CheckDatabaseStateBeforeCreate();

            // 執行房源創建
            Console.WriteLine("🏠 執行房源創建...");
            var propertyCreateResponse = await debugClient.PostAsync("/Property/Create", propertyCreateContent);

            // 詳細分析回應
            await AnalyzePropertyCreateResponse(propertyCreateResponse);

            // 在創建後檢查資料庫狀態
            await CheckDatabaseStateAfterCreate();
        }

        /// <summary>
        /// 創建真實的臨時會話
        /// </summary>
        private async Task<string> CreateRealTempSessionAsync(HttpClient client)
        {
            // 創建 2 張測試圖片
            var imageFiles = RealFileBuilder.CreateRealFormFileCollection(2, "debug_test");

            var tempUploadContent = new MultipartFormDataContent();
            tempUploadContent.Add(new StringContent("Gallery"), "category");

            foreach (var file in imageFiles)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                tempUploadContent.Add(streamContent, "files", file.FileName);
            }

            var tempUploadResponse = await client.PostAsync("/api/images/temp-upload", tempUploadContent);

            if (!tempUploadResponse.IsSuccessStatusCode)
            {
                var errorContent = await tempUploadResponse.Content.ReadAsStringAsync();
                throw new Exception($"臨時上傳失敗: {tempUploadResponse.StatusCode} - {errorContent}");
            }

            var tempUploadResponseContent = await tempUploadResponse.Content.ReadAsStringAsync();
            var tempUploadData = JsonSerializer.Deserialize<JsonElement>(tempUploadResponseContent);
            return tempUploadData.GetProperty("tempSessionId").GetString()!;
        }

        /// <summary>
        /// 驗證臨時會話中的圖片
        /// </summary>
        private async Task VerifyTempSessionImagesAsync(string tempSessionId)
        {
            using var scope = _factory.Services.CreateScope();
            var tempSessionService = scope.ServiceProvider.GetRequiredService<ITempSessionService>();

            var isValidSession = await tempSessionService.IsValidTempSessionAsync(tempSessionId);
            Console.WriteLine($"🔍 臨時會話有效性: {isValidSession}");

            if (isValidSession)
            {
                var tempImages = await tempSessionService.GetTempImagesAsync(tempSessionId);
                Console.WriteLine($"📊 臨時圖片數量: {tempImages.Count}");

                foreach (var img in tempImages)
                {
                    Console.WriteLine($"  - 圖片 {img.ImageGuid}: {img.Category}, {img.OriginalFileName}");
                }
            }
        }

        /// <summary>
        /// 設置身份驗證並取得 AntiForgery Token
        /// </summary>
        private async Task<string> SetupAuthenticationAndGetTokenAsync(HttpClient client, int userId)
        {
            client.DefaultRequestHeaders.Add("Cookie", $"UserId={userId}");

            var getResponse = await client.GetAsync("/Property/Create");
            var content = await getResponse.Content.ReadAsStringAsync();

            var tokenMatch = System.Text.RegularExpressions.Regex.Match(content,
                @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]*)"" />|""__RequestVerificationToken"":""([^""]*)""");

            if (tokenMatch.Success)
            {
                return tokenMatch.Groups[1].Value ?? tokenMatch.Groups[2].Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// 建構房源創建請求內容
        /// </summary>
        private MultipartFormDataContent BuildPropertyCreateContent(string tempSessionId, string antiForgeryToken)
        {
            var content = new MultipartFormDataContent();

            // AntiForgery Token
            if (!string.IsNullOrEmpty(antiForgeryToken))
            {
                content.Add(new StringContent(antiForgeryToken), "__RequestVerificationToken");
            }

            // TempSessionId（關鍵！）
            content.Add(new StringContent(tempSessionId), "TempSessionId");

            // 房源基本資料
            content.Add(new StringContent("調試測試房源"), "Title");
            content.Add(new StringContent("25000"), "MonthlyRent");
            content.Add(new StringContent("2"), "CityId");              // 修正：臺北市 (CityId=2)
            content.Add(new StringContent("1"), "DistrictId");          // 大安區 (DistrictId=1, 屬於臺北市)
            content.Add(new StringContent("調試測試地址"), "AddressLine");
            content.Add(new StringContent("2"), "RoomCount");
            content.Add(new StringContent("1"), "LivingRoomCount");
            content.Add(new StringContent("1"), "BathroomCount");
            content.Add(new StringContent("25"), "Area");
            content.Add(new StringContent("3"), "CurrentFloor");
            content.Add(new StringContent("10"), "TotalFloors");
            content.Add(new StringContent("2"), "DepositMonths");
            content.Add(new StringContent("12"), "MinimumRentalMonths");
            content.Add(new StringContent("true"), "ManagementFeeIncluded");
            content.Add(new StringContent("800"), "ManagementFeeAmount");
            content.Add(new StringContent("false"), "ParkingAvailable");
            content.Add(new StringContent("調試測試描述"), "Description");
            content.Add(new StringContent("2"), "ListingPlanId");       // 修正：3天方案 (PlanId=2, 已啟用)

            // 🔴 根據 DDL 加入所有缺失的必要欄位（NOT NULL且無預設值）
            content.Add(new StringContent("50000"), "DepositAmount");           // 押金金額 - NOT NULL
            content.Add(new StringContent("台水"), "WaterFeeType");             // 水費計算方式 - NOT NULL (使用正確值)
            content.Add(new StringContent("台電"), "ElectricityFeeType");       // 電費計算方式 - NOT NULL (使用正確值) 
            content.Add(new StringContent("PENDING"), "StatusCode");            // 房源狀態代碼 - NOT NULL (使用 PropertyStatusConstants.PENDING)

            // 其他可能需要的欄位（根據 DDL 結構補齊）
            content.Add(new StringContent("false"), "ParkingFeeRequired");      // 停車費需額外收費
            content.Add(new StringContent("false"), "CleaningFeeRequired");     // 清潔費需額外收費

            // 必需的設備選擇（避免驗證失敗）
            content.Add(new StringContent("1"), "SelectedEquipmentIds");

            return content;
        }

        /// <summary>
        /// 記錄請求內容
        /// </summary>
        private void LogRequestContent(MultipartFormDataContent content)
        {
            Console.WriteLine("📋 房源創建請求內容:");
            
            // 注意：這裡我們不能直接讀取 MultipartFormDataContent 的內容
            // 但我們可以記錄我們知道已添加的欄位
            Console.WriteLine("  - TempSessionId: 已設置");
            Console.WriteLine("  - Title: 調試測試房源");
            Console.WriteLine("  - MonthlyRent: 25000");
            Console.WriteLine("  - __RequestVerificationToken: 已設置");
            Console.WriteLine("  - SelectedEquipmentIds: 1");
        }

        /// <summary>
        /// 檢查創建前的資料庫狀態
        /// </summary>
        private async Task CheckDatabaseStateBeforeCreate()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();

            var existingPropertiesCount = await context.Properties
                .Where(p => p.Title.Contains("調試測試房源"))
                .CountAsync();

            Console.WriteLine($"🔍 創建前資料庫中相同標題的房源數量: {existingPropertiesCount}");
        }

        /// <summary>
        /// 分析房源創建回應
        /// </summary>
        private async Task AnalyzePropertyCreateResponse(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"🔍 房源創建回應分析:");
            Console.WriteLine($"  - 狀態碼: {response.StatusCode}");
            Console.WriteLine($"  - 原因短語: {response.ReasonPhrase}");
            Console.WriteLine($"  - 是否成功: {response.IsSuccessStatusCode}");

            // 檢查回應標頭
            if (response.Headers.Location != null)
            {
                Console.WriteLine($"  - 重導向位置: {response.Headers.Location}");
            }

            // 記錄回應內容（截斷以避免過長）
            if (responseContent.Length > 500)
            {
                Console.WriteLine($"  - 回應內容（前500字符）: {responseContent.Substring(0, 500)}...");
            }
            else
            {
                Console.WriteLine($"  - 回應內容: {responseContent}");
            }

            // 如果失敗，拋出異常以中止測試
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"房源創建失敗 - 狀態碼: {response.StatusCode}, 內容: {responseContent}");
            }
        }

        /// <summary>
        /// 檢查創建後的資料庫狀態
        /// </summary>
        private async Task CheckDatabaseStateAfterCreate()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();

            // 查詢新創建的房源
            var newProperties = await context.Properties
                .Where(p => p.Title.Contains("調試測試房源"))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"🔍 創建後資料庫狀態:");
            Console.WriteLine($"  - 找到的測試房源數量: {newProperties.Count}");

            if (newProperties.Any())
            {
                var latestProperty = newProperties.First();
                Console.WriteLine($"  - 最新房源 ID: {latestProperty.PropertyId}");
                Console.WriteLine($"  - 房源標題: {latestProperty.Title}");
                Console.WriteLine($"  - 房東 ID: {latestProperty.LandlordMemberId}");
                Console.WriteLine($"  - 創建時間: {latestProperty.CreatedAt}");
                Console.WriteLine($"  - 狀態: {latestProperty.StatusCode}");

                // 檢查相關的圖片記錄
                var relatedImages = await context.Images
                    .Where(img => img.EntityType == EntityType.Property 
                               && img.EntityId == latestProperty.PropertyId)
                    .ToListAsync();

                Console.WriteLine($"  - 關聯的圖片數量: {relatedImages.Count}");
                foreach (var img in relatedImages)
                {
                    Console.WriteLine($"    - 圖片: {img.StoredFileName}, 分類: {img.Category}, 啟用: {img.IsActive}");
                }
            }
            else
            {
                Console.WriteLine("  ❌ 沒有找到新創建的房源！");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}