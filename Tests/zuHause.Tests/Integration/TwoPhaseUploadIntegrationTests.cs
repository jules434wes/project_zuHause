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
    /// 兩階段圖片上傳整合測試 - 使用真實 Azure Blob Storage 和資料庫
    /// 第一階段：臨時上傳 (POST /api/images/temp-upload)
    /// 第二階段：房源創建與遷移 (POST /Property/Create with tempSessionId)
    /// </summary>
    public class TwoPhaseUploadIntegrationTests : IClassFixture<AzureTestWebApplicationFactory<Program>>
    {
        private readonly AzureTestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly ILogger<TwoPhaseUploadIntegrationTests>? _logger;

        public TwoPhaseUploadIntegrationTests(AzureTestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            
            // 嘗試獲取 logger 實例
            try
            {
                using var scope = _factory.Services.CreateScope();
                _logger = scope.ServiceProvider.GetService<ILogger<TwoPhaseUploadIntegrationTests>>();
            }
            catch
            {
                // Logger 初始化失敗不影響測試運行
                _logger = null;
            }
        }

        /// <summary>
        /// 測試第一階段：圖片臨時上傳到 Azure
        /// 驗證: 1. 臨時上傳成功 2. 取得 tempSessionId 3. 圖片存在於臨時區域
        /// </summary>
        [Fact]
        public async Task Phase1_TempImageUpload_ShouldSucceedAndReturnTempSessionId()
        {
            // Arrange - 準備測試圖片
            var imageFiles = RealFileBuilder.CreateRealFormFileCollection(2, "temp_upload_test");
            
            // Act - 執行第一階段臨時上傳
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("Gallery"), "category"); // 設定分類為 Gallery（房源相簿圖片）
            
            foreach (var file in imageFiles)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "files", file.FileName);
            }

            var response = await _client.PostAsync("/api/images/temp-upload", content);

            // Assert - 驗證第一階段結果
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ 臨時上傳失敗 - 狀態碼: {response.StatusCode}");
                Console.WriteLine($"❌ 錯誤內容: {errorContent}");
                throw new Exception($"臨時上傳請求失敗 - 狀態碼: {response.StatusCode}, 錯誤內容: {errorContent}");
            }
            
            response.IsSuccessStatusCode.Should().BeTrue($"臨時上傳請求應該成功，但收到狀態碼: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 臨時上傳回應: {responseContent}");
            
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            // 驗證回應格式
            responseData.GetProperty("success").GetBoolean().Should().BeTrue("臨時上傳應該成功");
            responseData.GetProperty("tempSessionId").GetString().Should().NotBeNullOrEmpty("應該取得 TempSessionId");
            
            var tempSessionId = responseData.GetProperty("tempSessionId").GetString();
            tempSessionId!.Length.Should().Be(32, "TempSessionId 應該是 32 字元的 GUID");
            
            var imagesArray = responseData.GetProperty("images");
            imagesArray.GetArrayLength().Should().Be(2, "應該上傳 2 張圖片");
            
            Console.WriteLine($"✅ 第一階段成功 - TempSessionId: {tempSessionId}");
            Console.WriteLine($"✅ 上傳圖片數量: {imagesArray.GetArrayLength()}");
        }

        /// <summary>
        /// 測試完整的兩階段上傳流程
        /// 第一階段：臨時上傳 → 第二階段：房源創建與遷移 → 驗證資料庫記錄
        /// </summary>
        [Fact]
        public async Task CompleteTwoPhaseUpload_ShouldCreatePropertyWithMigratedImages()
        {
            try
            {
                Console.WriteLine($"🚀 開始完整兩階段上傳測試");
                
                // === 確保獨立的臨時會話 ===
                // 為這個測試創建一個新的 HttpClient 以避免共享 Cookie
                using var isolatedClient = _factory.CreateClient();
                Console.WriteLine($"✅ HttpClient 創建成功");
            
            // 手動清理可能存在的臨時會話衝突
            await ClearAnyExistingTempSessionAsync(isolatedClient);
            
            // 🔑 設置身份驗證（只需要設置一次）
            Console.WriteLine($"🔑 設置身份驗證 (UserId: 51)...");
            isolatedClient.DefaultRequestHeaders.Add("Cookie", $"UserId=51");
            
            // === 第一階段：臨時上傳 ===
            var imageFiles = RealFileBuilder.CreateRealFormFileCollection(3, "two_phase_test");
            
            var tempUploadContent = new MultipartFormDataContent();
            tempUploadContent.Add(new StringContent("Gallery"), "category");
            
            foreach (var file in imageFiles)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                tempUploadContent.Add(streamContent, "files", file.FileName);
            }

            var tempUploadResponse = await isolatedClient.PostAsync("/api/images/temp-upload", tempUploadContent);
            
            // 捕獲詳細錯誤信息
            if (!tempUploadResponse.IsSuccessStatusCode)
            {
                var errorContent = await tempUploadResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ 第一階段臨時上傳失敗 - 狀態碼: {tempUploadResponse.StatusCode}");
                Console.WriteLine($"❌ 錯誤內容: {errorContent}");
                throw new Exception($"第一階段臨時上傳失敗 - 狀態碼: {tempUploadResponse.StatusCode}, 錯誤內容: {errorContent}");
            }
            
            tempUploadResponse.IsSuccessStatusCode.Should().BeTrue("第一階段臨時上傳應該成功");
            
            var tempUploadResponseContent = await tempUploadResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 第一階段 JSON 回應: {tempUploadResponseContent}");
            
            var tempUploadData = JsonSerializer.Deserialize<JsonElement>(tempUploadResponseContent);
            Console.WriteLine($"🔍 JSON 解析完成");
            
            var tempSessionId = tempUploadData.GetProperty("tempSessionId").GetString();
            Console.WriteLine($"🔍 tempSessionId 提取完成: {tempSessionId}");
            
            Console.WriteLine($"🔍 第一階段完成 - TempSessionId: {tempSessionId}");

            try
            {
                // === 第二階段：房源創建與遷移 ===
                Console.WriteLine($"📝 開始第二階段：房源創建與遷移");
            
            // 🔑 取得 AntiForgery Token (身份驗證已在第一階段設置)
            Console.WriteLine($"🔑 正在取得 AntiForgery Token...");
            var antiForgeryToken = await GetAntiForgeryTokenAsync(isolatedClient);
            Console.WriteLine($"🔑 AntiForgery Token 獲取完成: {(!string.IsNullOrEmpty(antiForgeryToken) ? "成功" : "失敗")}");
            
            var propertyCreateContent = new MultipartFormDataContent();
            
            // 加入 AntiForgery Token
            if (!string.IsNullOrEmpty(antiForgeryToken))
            {
                propertyCreateContent.Add(new StringContent(antiForgeryToken), "__RequestVerificationToken");
            }
            
            // 加入 TempSessionId（這是關鍵！）
            propertyCreateContent.Add(new StringContent(tempSessionId!), "TempSessionId");
            
            // 加入房源基本資料
            propertyCreateContent.Add(new StringContent("兩階段上傳測試房源"), "Title");
            propertyCreateContent.Add(new StringContent("35000"), "MonthlyRent");
            propertyCreateContent.Add(new StringContent("2"), "CityId"); // 修正：臺北市 ID=2
            propertyCreateContent.Add(new StringContent("1"), "DistrictId"); // 修正：大安區 ID=1
            propertyCreateContent.Add(new StringContent("測試地址 - 兩階段上傳"), "AddressLine");
            propertyCreateContent.Add(new StringContent("4"), "RoomCount");
            propertyCreateContent.Add(new StringContent("2"), "LivingRoomCount");
            propertyCreateContent.Add(new StringContent("2"), "BathroomCount");
            propertyCreateContent.Add(new StringContent("35"), "Area");
            propertyCreateContent.Add(new StringContent("8"), "CurrentFloor");
            propertyCreateContent.Add(new StringContent("15"), "TotalFloors");
            propertyCreateContent.Add(new StringContent("2"), "DepositMonths");
            propertyCreateContent.Add(new StringContent("12"), "MinimumRentalMonths");
            propertyCreateContent.Add(new StringContent("true"), "ManagementFeeIncluded");
            propertyCreateContent.Add(new StringContent("1200"), "ManagementFeeAmount");
            propertyCreateContent.Add(new StringContent("true"), "ParkingAvailable");
            propertyCreateContent.Add(new StringContent("兩階段上傳測試描述"), "Description");
            
            // 修正：加入缺失的必填欄位
            propertyCreateContent.Add(new StringContent("台水"), "WaterFeeType");
            propertyCreateContent.Add(new StringContent("台電"), "ElectricityFeeType");
            propertyCreateContent.Add(new StringContent("2"), "ListingPlanId"); // 修正：使用有效的刊登方案 ID
            propertyCreateContent.Add(new StringContent("1"), "SelectedEquipmentIds"); // 修正：至少選擇一項設備

            // 執行第二階段房源創建
            Console.WriteLine($"🏠 正在執行第二階段房源創建 POST /property/create...");
            var propertyCreateResponse = await isolatedClient.PostAsync("/property/create", propertyCreateContent);
            Console.WriteLine($"🏠 第二階段房源創建請求完成");
            
            // 檢查第二階段回應
            var propertyCreateResponseContent = await propertyCreateResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 第二階段回應狀態: {propertyCreateResponse.StatusCode}");
            Console.WriteLine($"🔍 第二階段完整回應內容:");
            Console.WriteLine($"================================");
            Console.WriteLine(propertyCreateResponseContent);
            Console.WriteLine($"================================");

            if (!propertyCreateResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ 第二階段房源創建失敗!");
                Console.WriteLine($"❌ 狀態碼: {propertyCreateResponse.StatusCode}");
                Console.WriteLine($"❌ 原因短語: {propertyCreateResponse.ReasonPhrase}");
                Console.WriteLine($"❌ 錯誤內容:");
                Console.WriteLine(propertyCreateResponseContent);
                throw new Exception($"❌ 第二階段房源創建失敗 - 狀態碼: {propertyCreateResponse.StatusCode}\\n錯誤內容: {propertyCreateResponseContent}");
            }

            // === 驗證資料庫中的遷移結果 ===
            
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();

            Console.WriteLine($"🔍 正在查詢資料庫中的房源...");
            
            // 先查看資料庫中所有包含"測試"的房源（調試用）
            var allTestProperties = await context.Properties
                .Where(p => p.Title.Contains("測試"))
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();
                
            Console.WriteLine($"🔍 資料庫中包含'測試'的房源數量: {allTestProperties.Count}");
            foreach (var prop in allTestProperties)
            {
                Console.WriteLine($"  - ID: {prop.PropertyId}, Title: {prop.Title}, Created: {prop.CreatedAt}");
            }

            // 查詢最新創建的房源（由我們的測試創建的）
            var latestProperty = await context.Properties
                .Where(p => p.Title.Contains("兩階段上傳測試房源"))
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            latestProperty.Should().NotBeNull("應該找到新創建的測試房源");
            Console.WriteLine($"🏠 找到測試房源 ID: {latestProperty!.PropertyId}");

            // 查詢遷移後的圖片記錄
            var migratedImages = await context.Images
                .Where(img => img.EntityType == EntityType.Property 
                           && img.EntityId == latestProperty.PropertyId 
                           && img.Category == ImageCategory.Gallery
                           && img.StoredFileName.Contains("TEST_"))
                .OrderBy(img => img.DisplayOrder)
                .ToListAsync();

            Console.WriteLine($"🔍 找到 {migratedImages.Count} 張遷移後的圖片");
            foreach (var img in migratedImages)
            {
                Console.WriteLine($"  - {img.StoredFileName} (DisplayOrder: {img.DisplayOrder}, Active: {img.IsActive})");
            }

            // 驗證遷移結果
            migratedImages.Should().HaveCount(3, "應該有 3 張圖片被成功遷移");
            
            for (int i = 0; i < migratedImages.Count; i++)
            {
                var image = migratedImages[i];
                image.StoredFileName.Should().StartWith("TEST_", "所有測試圖片都應該有 TEST_ 前綴");
                image.IsActive.Should().BeTrue("遷移後的圖片應該是啟用狀態");
                image.EntityType.Should().Be(EntityType.Property);
                image.EntityId.Should().Be(latestProperty.PropertyId);
                image.Category.Should().Be(ImageCategory.Gallery);
                image.DisplayOrder.Should().Be(i + 1, $"第 {i + 1} 張圖片的 DisplayOrder 應該是 {i + 1}");
                image.MimeType.Should().StartWith("image/", "應該是圖片類型");
            }

            Console.WriteLine($"✅ 兩階段上傳測試成功完成！");
            Console.WriteLine($"✅ 房源 ID: {latestProperty.PropertyId}");
            Console.WriteLine($"✅ 遷移圖片數量: {migratedImages.Count}");
            Console.WriteLine($"✅ TempSessionId: {tempSessionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 第二階段執行過程中發生異常:");
                Console.WriteLine($"❌ 異常類型: {ex.GetType().Name}");
                Console.WriteLine($"❌ 異常訊息: {ex.Message}");
                Console.WriteLine($"❌ 堆疊追蹤:");
                Console.WriteLine(ex.StackTrace);
                throw; // 重新拋出異常以確保測試失敗
            }
            }
            catch (Exception topLevelEx)
            {
                Console.WriteLine($"❌❌ 測試頂級異常捕獲 ❌❌");
                Console.WriteLine($"❌ 異常類型: {topLevelEx.GetType().Name}");
                Console.WriteLine($"❌ 異常訊息: {topLevelEx.Message}");
                Console.WriteLine($"❌ 詳細堆疊追蹤:");
                Console.WriteLine(topLevelEx.StackTrace);
                
                if (topLevelEx.InnerException != null)
                {
                    Console.WriteLine($"❌ 內部異常: {topLevelEx.InnerException.GetType().Name}");
                    Console.WriteLine($"❌ 內部異常訊息: {topLevelEx.InnerException.Message}");
                }
                
                throw; // 重新拋出異常以確保測試失敗顯示正確的錯誤
            }
        }

        /// <summary>
        /// 隔離測試：只測試房源創建，不涉及圖片遷移
        /// </summary>
        [Fact]
        public async Task SimplePropertyCreate_WithoutImages_ShouldSucceed()
        {
            using var isolatedClient = _factory.CreateClient();
            
            Console.WriteLine($"🔍 階段1測試：開始隔離房源創建測試（無圖片）");
            
            // 設置身份驗證
            isolatedClient.DefaultRequestHeaders.Add("Cookie", $"UserId=51");
            
            // 取得 AntiForgery Token
            var antiForgeryToken = await GetAntiForgeryTokenAsync(isolatedClient);
            
            var propertyCreateContent = new MultipartFormDataContent();
            
            if (!string.IsNullOrEmpty(antiForgeryToken))
            {
                propertyCreateContent.Add(new StringContent(antiForgeryToken), "__RequestVerificationToken");
            }
            
            // 注意：沒有 TempSessionId，不會觸發圖片遷移
            
            // 房源基本資料
            propertyCreateContent.Add(new StringContent("隔離測試房源"), "Title");
            propertyCreateContent.Add(new StringContent("25000"), "MonthlyRent");
            propertyCreateContent.Add(new StringContent("2"), "CityId"); // 臺北市
            propertyCreateContent.Add(new StringContent("1"), "DistrictId"); // 大安區
            propertyCreateContent.Add(new StringContent("隔離測試地址"), "AddressLine");
            propertyCreateContent.Add(new StringContent("3"), "RoomCount");
            propertyCreateContent.Add(new StringContent("1"), "LivingRoomCount");
            propertyCreateContent.Add(new StringContent("2"), "BathroomCount");
            propertyCreateContent.Add(new StringContent("30"), "Area");
            propertyCreateContent.Add(new StringContent("5"), "CurrentFloor");
            propertyCreateContent.Add(new StringContent("12"), "TotalFloors");
            propertyCreateContent.Add(new StringContent("2"), "DepositMonths");
            propertyCreateContent.Add(new StringContent("12"), "MinimumRentalMonths");
            propertyCreateContent.Add(new StringContent("false"), "ManagementFeeIncluded");
            propertyCreateContent.Add(new StringContent("800"), "ManagementFeeAmount"); // 須另計時必填
            propertyCreateContent.Add(new StringContent("false"), "ParkingAvailable");
            propertyCreateContent.Add(new StringContent("隔離測試描述"), "Description");
            propertyCreateContent.Add(new StringContent("台水"), "WaterFeeType");
            propertyCreateContent.Add(new StringContent("台電"), "ElectricityFeeType");
            propertyCreateContent.Add(new StringContent("2"), "ListingPlanId");
            propertyCreateContent.Add(new StringContent("1"), "SelectedEquipmentIds");

            Console.WriteLine($"🏠 執行房源創建 POST /property/create（無圖片）");
            var propertyCreateResponse = await isolatedClient.PostAsync("/property/create", propertyCreateContent);
            
            var responseContent = await propertyCreateResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 隔離測試回應狀態: {propertyCreateResponse.StatusCode}");
            
            if (!propertyCreateResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ 隔離測試失敗！");
                Console.WriteLine($"❌ 回應內容: {responseContent}");
                throw new Exception($"隔離房源創建失敗 - 狀態碼: {propertyCreateResponse.StatusCode}");
            }

            // 驗證資料庫中的房源
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();

            var createdProperty = await context.Properties
                .Where(p => p.Title.Contains("隔離測試房源"))
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            createdProperty.Should().NotBeNull("隔離測試房源應該被成功創建");
            Console.WriteLine($"✅ 隔離測試成功！房源 ID: {createdProperty!.PropertyId}");
            Console.WriteLine($"✅ 房源標題: {createdProperty.Title}");
            Console.WriteLine($"✅ 創建時間: {createdProperty.CreatedAt}");
        }

        /// <summary>
        /// 測試混合檔案的兩階段上傳（圖片 + PDF）
        /// </summary>
        [Fact]
        public async Task TwoPhaseUpload_MixedFiles_ShouldMigrateWithCorrectCategories()
        {
            // === 確保獨立的臨時會話 ===
            using var isolatedClient = _factory.CreateClient();
            
            // 🔑 設置身份驗證（只需要設置一次）
            Console.WriteLine($"🔑 設置身份驗證 (UserId: 51)...");
            isolatedClient.DefaultRequestHeaders.Add("Cookie", $"UserId=51");
            
            // === 第一階段：上傳圖片到臨時區域 ===
            var imageFiles = RealFileBuilder.CreateRealFormFileCollection(2, "mixed_gallery");
            
            var tempUploadContent1 = new MultipartFormDataContent();
            tempUploadContent1.Add(new StringContent("Gallery"), "category"); // 圖片分類
            
            foreach (var file in imageFiles)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                tempUploadContent1.Add(streamContent, "files", file.FileName);
            }

            var tempUploadResponse1 = await isolatedClient.PostAsync("/api/images/temp-upload", tempUploadContent1);
            tempUploadResponse1.IsSuccessStatusCode.Should().BeTrue("圖片臨時上傳應該成功");
            
            var tempUploadResponseContent1 = await tempUploadResponse1.Content.ReadAsStringAsync();
            var tempUploadData1 = JsonSerializer.Deserialize<JsonElement>(tempUploadResponseContent1);
            var tempSessionId = tempUploadData1.GetProperty("tempSessionId").GetString();

            // === 第一階段：上傳 PDF 到同一個臨時會話 ===
            var pdfFile = RealFileBuilder.CreateRealPdfDocument("混合檔案測試證明文件", "mixed_proof.pdf");
            
            var tempUploadContent2 = new MultipartFormDataContent();
            tempUploadContent2.Add(new StringContent("Document"), "category"); // PDF 分類
            
            var pdfStreamContent = new StreamContent(pdfFile.OpenReadStream());
            pdfStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(pdfFile.ContentType);
            tempUploadContent2.Add(pdfStreamContent, "files", pdfFile.FileName);

            // 使用相同的會話 cookie 以確保 PDF 加入同一個臨時會話
            var tempUploadResponse2 = await isolatedClient.PostAsync("/api/images/temp-upload", tempUploadContent2);
            
            // 先讀取響應內容（無論成功失敗都讀取）
            var response2Content = await tempUploadResponse2.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 PDF 上傳響應:");
            Console.WriteLine($"   Status Code: {tempUploadResponse2.StatusCode}");
            Console.WriteLine($"   Response Content: {response2Content}");
            Console.WriteLine($"   TempSessionId: {tempSessionId}");
            
            tempUploadResponse2.IsSuccessStatusCode.Should().BeTrue("PDF 臨時上傳應該成功");

            Console.WriteLine($"🔍 混合檔案第一階段完成 - TempSessionId: {tempSessionId}");

            // === 第二階段：房源創建與遷移 ===
            var antiForgeryToken = await GetAntiForgeryTokenAsync(isolatedClient);
            
            var propertyCreateContent = new MultipartFormDataContent();
            
            if (!string.IsNullOrEmpty(antiForgeryToken))
            {
                propertyCreateContent.Add(new StringContent(antiForgeryToken), "__RequestVerificationToken");
            }
            
            propertyCreateContent.Add(new StringContent(tempSessionId!), "TempSessionId");
            
            // 房源資料
            propertyCreateContent.Add(new StringContent("混合檔案測試房源"), "Title");
            propertyCreateContent.Add(new StringContent("28000"), "MonthlyRent");
            propertyCreateContent.Add(new StringContent("2"), "CityId"); // 修正：臺北市 ID=2
            propertyCreateContent.Add(new StringContent("1"), "DistrictId"); // 修正：大安區 ID=1
            propertyCreateContent.Add(new StringContent("混合檔案測試地址"), "AddressLine");
            propertyCreateContent.Add(new StringContent("3"), "RoomCount");
            propertyCreateContent.Add(new StringContent("1"), "LivingRoomCount");
            propertyCreateContent.Add(new StringContent("2"), "BathroomCount");
            propertyCreateContent.Add(new StringContent("30"), "Area");
            propertyCreateContent.Add(new StringContent("5"), "CurrentFloor");
            propertyCreateContent.Add(new StringContent("12"), "TotalFloors");
            propertyCreateContent.Add(new StringContent("2"), "DepositMonths");
            propertyCreateContent.Add(new StringContent("12"), "MinimumRentalMonths");
            propertyCreateContent.Add(new StringContent("false"), "ManagementFeeIncluded");
            propertyCreateContent.Add(new StringContent("600"), "ManagementFeeAmount"); // 須另計時必填
            propertyCreateContent.Add(new StringContent("false"), "ParkingAvailable");
            propertyCreateContent.Add(new StringContent("混合檔案上傳測試"), "Description");
            
            // 修正：加入缺失的必填欄位
            propertyCreateContent.Add(new StringContent("台水"), "WaterFeeType");
            propertyCreateContent.Add(new StringContent("台電"), "ElectricityFeeType");
            propertyCreateContent.Add(new StringContent("2"), "ListingPlanId"); // 修正：使用有效的刊登方案 ID
            propertyCreateContent.Add(new StringContent("1"), "SelectedEquipmentIds"); // 修正：至少選擇一項設備

            var propertyCreateResponse = await isolatedClient.PostAsync("/property/create", propertyCreateContent);
            
            var propertyCreateResponseContent = await propertyCreateResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 混合檔案測試第二階段回應狀態: {propertyCreateResponse.StatusCode}");
            Console.WriteLine($"🔍 混合檔案測試第二階段完整回應內容:");
            Console.WriteLine($"================================");
            Console.WriteLine(propertyCreateResponseContent);
            Console.WriteLine($"================================");
            
            if (!propertyCreateResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ 混合檔案測試第二階段房源創建失敗!");
                Console.WriteLine($"❌ 狀態碼: {propertyCreateResponse.StatusCode}");
                Console.WriteLine($"❌ 原因短語: {propertyCreateResponse.ReasonPhrase}");
                throw new Exception($"❌ 混合檔案房源創建失敗 - 狀態碼: {propertyCreateResponse.StatusCode}\\n錯誤內容: {propertyCreateResponseContent}");
            }

            // === 驗證分類遷移結果 ===
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();

            var latestProperty = await context.Properties
                .Where(p => p.Title.Contains("混合檔案測試房源"))
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            latestProperty.Should().NotBeNull("應該找到混合檔案測試房源");

            // 驗證圖片遷移
            var galleryImages = await context.Images
                .Where(img => img.EntityType == EntityType.Property 
                           && img.EntityId == latestProperty!.PropertyId 
                           && img.Category == ImageCategory.Gallery
                           && img.StoredFileName.Contains("TEST_"))
                .ToListAsync();

            // 驗證 PDF 文件：應該不存在於 images 表中（因為直接存到 properties.propertyProofURL）
            var documentImages = await context.Images
                .Where(img => img.EntityType == EntityType.Property 
                           && img.EntityId == latestProperty!.PropertyId 
                           && img.Category == ImageCategory.Document)
                .ToListAsync();

            Console.WriteLine($"🔍 Gallery 圖片: {galleryImages.Count} 張");
            Console.WriteLine($"🔍 Document 檔案（在 images 表中）: {documentImages.Count} 個");
            Console.WriteLine($"🔍 PropertyProofUrl: {latestProperty!.PropertyProofUrl}");

            galleryImages.Should().HaveCount(2, "應該有 2 張 Gallery 圖片");
            documentImages.Should().HaveCount(0, "PDF 文件不應存在於 images 表中，應直接設定到 propertyProofURL");

            // 驗證房源證明 URL 是否已設定
            latestProperty!.PropertyProofUrl.Should().NotBeNullOrEmpty("房源證明 URL 應該已設定");
            latestProperty.PropertyProofUrl.Should().Contain("Document", "證明文件 URL 應該包含 Document 路徑");

            Console.WriteLine($"✅ 混合檔案兩階段上傳測試成功！");
            Console.WriteLine($"✅ PropertyProofUrl: {latestProperty.PropertyProofUrl}");
        }

        /// <summary>
        /// 清理可能存在的臨時會話以確保測試隔離
        /// </summary>
        private async Task ClearAnyExistingTempSessionAsync(HttpClient client)
        {
            try
            {
                // 使用 TempSessionService 直接清理
                using var scope = _factory.Services.CreateScope();
                var tempSessionService = scope.ServiceProvider.GetRequiredService<ITempSessionService>();
                
                // 嘗試創建一個臨時會話並立即清理，這會觸發 Cookie 設置
                var tempSessionId = tempSessionService.GetOrCreateTempSessionId(new DefaultHttpContext());
                await tempSessionService.InvalidateTempSessionAsync(tempSessionId);
                
                _logger?.LogInformation("清理了臨時會話: {TempSessionId}", tempSessionId);
            }
            catch (Exception ex)
            {
                // 清理失敗不應影響測試，只記錄警告
                _logger?.LogWarning(ex, "清理臨時會話時發生錯誤，繼續執行測試");
            }
        }

        /// <summary>
        /// 設置測試身份驗證並取得 AntiForgery Token
        /// </summary>
        private async Task<string> SetupAuthenticationAndGetTokenAsync(HttpClient client, int userId)
        {
            client.DefaultRequestHeaders.Add("Cookie", $"UserId={userId}");
            
            var getResponse = await client.GetAsync("/property/create");
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
        /// 只獲取 AntiForgery Token，不設置身份驗證
        /// </summary>
        private async Task<string> GetAntiForgeryTokenAsync(HttpClient client)
        {
            var getResponse = await client.GetAsync("/property/create");
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