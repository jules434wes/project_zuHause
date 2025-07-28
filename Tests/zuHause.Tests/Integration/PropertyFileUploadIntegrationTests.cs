using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using FluentAssertions;
using Xunit;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace zuHause.Tests.Integration
{
    /// <summary>
    /// 房源檔案上傳整合測試 - 使用真實 Azure Blob Storage 和資料庫
    /// 確保房源圖片和 PDF 文件能正確上傳並記錄到資料庫
    /// </summary>
    public class PropertyFileUploadIntegrationTests : IClassFixture<AzureTestWebApplicationFactory<Program>>
    {
        private readonly AzureTestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PropertyFileUploadIntegrationTests(AzureTestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// 測試房源圖片上傳到 Azure 並記錄到資料庫
        /// 驗證: 1. 檔案上傳到 Azure Blob Storage 2. 資料庫記錄正確 3. 可查詢到測試資料
        /// </summary>
        [Fact]
        public async Task PropertyImageUpload_ToAzureAndDatabase_ShouldCreateTestRecords()
        {
            // Arrange - 準備測試資料
            var testPropertyId = await GetExistingPropertyAsync();
            Console.WriteLine($"🏠 使用房源 ID: {testPropertyId}");
            
            // 設置身份驗證並取得 AntiForgery Token
            var antiForgeryToken = await SetupAuthenticationAndGetTokenAsync(51); // 使用房東 ID 51
            Console.WriteLine($"🔐 取得 AntiForgery Token: {(string.IsNullOrEmpty(antiForgeryToken) ? "未找到" : "已取得")}");
            
            var imageFiles = RealFileBuilder.CreateRealFormFileCollection(3, "test_property_gallery");

            // Act - 執行房源圖片上傳
            var content = new MultipartFormDataContent();
            
            // 加入 AntiForgery Token (如果有)
            if (!string.IsNullOrEmpty(antiForgeryToken))
            {
                content.Add(new StringContent(antiForgeryToken), "__RequestVerificationToken");
            }
            
            // 注意：不要加入 PropertyId，因為 Create 方法是用來創建新房源，不是更新現有房源
            // content.Add(new StringContent(testPropertyId.ToString()), "PropertyId");
            content.Add(new StringContent("測試房源標題"), "Title");
            content.Add(new StringContent("25000"), "MonthlyRent");
            content.Add(new StringContent("1"), "CityId");
            content.Add(new StringContent("1"), "DistrictId");
            content.Add(new StringContent("測試地址"), "AddressLine");
            content.Add(new StringContent("3"), "RoomCount");
            content.Add(new StringContent("1"), "LivingRoomCount");
            content.Add(new StringContent("2"), "BathroomCount");
            content.Add(new StringContent("30"), "Area");
            content.Add(new StringContent("5"), "CurrentFloor");
            content.Add(new StringContent("10"), "TotalFloors");
            content.Add(new StringContent("2"), "DepositMonths");
            content.Add(new StringContent("12"), "MinimumRentalMonths");
            content.Add(new StringContent("true"), "ManagementFeeIncluded");
            content.Add(new StringContent("1000"), "ManagementFeeAmount");
            content.Add(new StringContent("false"), "ParkingAvailable");
            content.Add(new StringContent("測試房源描述"), "Description");
            content.Add(new StringContent("1"), "ListingPlanId");

            // 加入圖片檔案
            foreach (var file in imageFiles)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "PropertyImages", file.FileName);
            }

            // 執行 POST 請求
            var response = await _client.PostAsync("/Property/Create", content);

            // 檢查詳細錯誤信息
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // 強制失敗測試以顯示錯誤詳情
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"❌ 上傳失敗 - 狀態碼: {response.StatusCode}\n錯誤內容: {responseContent}");
            }

            // Assert - 驗證結果
            response.IsSuccessStatusCode.Should().BeTrue($"上傳請求應該成功，但收到狀態碼: {response.StatusCode}");
            
            // 🔍 DEBUG: 檢查是否重定向到成功頁面，這可能包含新的房源 ID
            Console.WriteLine($"🔍 回應內容預覽: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
            Console.WriteLine($"🔍 回應 Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
            
            // 嘗試從重定向位置取得新的房源 ID
            int? actualPropertyId = null;
            if (response.Headers.Location != null)
            {
                var locationPath = response.Headers.Location.ToString();
                Console.WriteLine($"🔍 重定向到: {locationPath}");
                
                // 嘗試從 URL 解析房源 ID (例如: /Property/CreationSuccess/123)
                var match = System.Text.RegularExpressions.Regex.Match(locationPath, @"/(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var newPropertyId))
                {
                    actualPropertyId = newPropertyId;
                    Console.WriteLine($"🏠 實際創建的房源 ID: {actualPropertyId}");
                }
            }

            // 驗證資料庫記錄 - 使用實際創建的房源 ID
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();

            // 使用實際創建的房源 ID，如果沒有則回到原本的 testPropertyId
            var searchPropertyId = actualPropertyId ?? testPropertyId;
            Console.WriteLine($"🔍 搜尋圖片的房源 ID: {searchPropertyId}");

            var uploadedImages = await context.Images
                .Where(img => img.EntityType == EntityType.Property 
                           && img.EntityId == searchPropertyId 
                           && img.Category == ImageCategory.Gallery
                           && img.StoredFileName.Contains("TEST_"))
                .ToListAsync();
                
            Console.WriteLine($"🔍 找到 {uploadedImages.Count} 張測試圖片");
            foreach (var img in uploadedImages)
            {
                Console.WriteLine($"  - {img.StoredFileName} (房源ID: {img.EntityId})");
            }
            
            // 🔍 DEBUG: 查看資料庫中所有的測試圖片記錄 (不限房源ID)
            var allTestImages = await context.Images
                .Where(img => img.StoredFileName.Contains("TEST_"))
                .ToListAsync();
                
            Console.WriteLine($"🔍 資料庫中總共有 {allTestImages.Count} 張 TEST_ 圖片:");
            foreach (var img in allTestImages)
            {
                Console.WriteLine($"  - {img.StoredFileName} (房源ID: {img.EntityId}, EntityType: {img.EntityType}, Category: {img.Category})");
            }
            
            // 如果沒有找到任何測試圖片，拋出詳細錯誤
            if (uploadedImages.Count == 0 && allTestImages.Count == 0)
            {
                throw new Exception($"🚨 沒有找到任何測試圖片！請檢查圖片上傳邏輯。\n搜尋房源ID: {searchPropertyId}\n原始房源ID: {testPropertyId}\n實際房源ID: {actualPropertyId}");
            }
            else if (uploadedImages.Count == 0)
            {
                throw new Exception($"🚨 找到 {allTestImages.Count} 張測試圖片，但沒有與房源 {searchPropertyId} 關聯的圖片");
            }

            uploadedImages.Should().HaveCount(3, "應該上傳3張測試圖片");
            
            foreach (var image in uploadedImages)
            {
                image.StoredFileName.Should().StartWith("TEST_", "所有測試檔案都應該有 TEST_ 前綴");
                image.IsActive.Should().BeTrue("上傳的圖片應該是啟用狀態");
                image.EntityType.Should().Be(EntityType.Property);
                image.EntityId.Should().Be(searchPropertyId);
                image.Category.Should().Be(ImageCategory.Gallery);
                image.MimeType.Should().StartWith("image/", "應該是圖片類型");
            }

            // 驗證 DisplayOrder 已正確分配
            var orderedImages = uploadedImages.OrderBy(img => img.DisplayOrder).ToList();
            for (int i = 0; i < orderedImages.Count; i++)
            {
                orderedImages[i].DisplayOrder.Should().Be(i + 1, $"第 {i + 1} 張圖片的 DisplayOrder 應該是 {i + 1}");
            }

            // 驗證主圖設定
            orderedImages.First().DisplayOrder.Should().Be(1, "第一張圖片應該是主圖");
        }

        /// <summary>
        /// 測試 PDF 文件上傳到 Azure 並記錄到資料庫
        /// 驗證: 1. PDF 檔案上傳到 Azure 2. 資料庫記錄分類為 Document 3. 可查詢到測試資料
        /// </summary>
        [Fact]
        public async Task PropertyPdfUpload_ToAzureAndDatabase_ShouldCreateDocumentRecord()
        {
            // Arrange - 準備測試資料
            var testPropertyId = await GetExistingPropertyAsync();
            var pdfFile = RealFileBuilder.CreateRealPdfDocument("測試房產證明文件內容", "test_property_proof.pdf");

            // Act - 執行 PDF 檔案上傳
            var content = new MultipartFormDataContent();
            
            // 加入房源基本資料 (最簡化版本)
            content.Add(new StringContent(testPropertyId.ToString()), "PropertyId");
            content.Add(new StringContent("測試 PDF 上傳房源"), "Title");
            content.Add(new StringContent("20000"), "MonthlyRent");
            content.Add(new StringContent("1"), "CityId");
            content.Add(new StringContent("1"), "DistrictId");
            content.Add(new StringContent("測試地址"), "AddressLine");
            content.Add(new StringContent("2"), "RoomCount");
            content.Add(new StringContent("1"), "LivingRoomCount");
            content.Add(new StringContent("1"), "BathroomCount");
            content.Add(new StringContent("25"), "Area");
            content.Add(new StringContent("3"), "CurrentFloor");
            content.Add(new StringContent("10"), "TotalFloors");
            content.Add(new StringContent("2"), "DepositMonths");
            content.Add(new StringContent("12"), "MinimumRentalMonths");
            content.Add(new StringContent("false"), "ManagementFeeIncluded");
            content.Add(new StringContent("false"), "ParkingAvailable");
            content.Add(new StringContent("測試 PDF 上傳功能"), "Description");
            content.Add(new StringContent("1"), "ListingPlanId");

            // 加入 PDF 檔案
            var streamContent = new StreamContent(pdfFile.OpenReadStream());
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(pdfFile.ContentType);
            content.Add(streamContent, "PropertyProofDocument", pdfFile.FileName);

            // 執行 POST 請求
            var response = await _client.PostAsync("/Property/Create", content);

            // Assert - 驗證結果
            response.IsSuccessStatusCode.Should().BeTrue($"PDF 上傳請求應該成功，但收到狀態碼: {response.StatusCode}");

            // 驗證資料庫記錄
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();

            var uploadedPdf = await context.Images
                .Where(img => img.EntityType == EntityType.Property 
                           && img.EntityId == testPropertyId 
                           && img.Category == ImageCategory.Document
                           && img.StoredFileName.Contains("TEST_"))
                .FirstOrDefaultAsync();

            uploadedPdf.Should().NotBeNull("應該找到上傳的 PDF 記錄");
            uploadedPdf!.StoredFileName.Should().StartWith("TEST_", "PDF 檔案應該有 TEST_ 前綴");
            uploadedPdf.Category.Should().Be(ImageCategory.Document, "PDF 應該歸類為 Document");
            uploadedPdf.IsActive.Should().BeTrue("上傳的 PDF 應該是啟用狀態");
            uploadedPdf.OriginalFileName.Should().Be("test_property_proof.pdf");
            uploadedPdf.EntityType.Should().Be(EntityType.Property);
            uploadedPdf.EntityId.Should().Be(testPropertyId);
        }

        /// <summary>
        /// 測試混合檔案上傳 (圖片 + PDF)
        /// 驗證: 1. 同時上傳多張圖片和一個 PDF 2. 正確分類儲存 3. 都有 TEST_ 前綴
        /// </summary>
        [Fact]
        public async Task MixedFileUpload_ImagesAndPdf_ShouldCreateBothCategoryRecords()
        {
            // Arrange - 準備測試資料
            var testPropertyId = await GetExistingPropertyAsync();
            var imageFiles = RealFileBuilder.CreateRealFormFileCollection(2, "test_mixed_gallery");
            var pdfFile = RealFileBuilder.CreateRealPdfDocument("測試混合檔案房產證明", "test_mixed_proof.pdf");

            // Act - 執行混合檔案上傳
            var content = new MultipartFormDataContent();
            
            // 基本房源資料
            content.Add(new StringContent(testPropertyId.ToString()), "PropertyId");
            content.Add(new StringContent("混合檔案測試房源"), "Title");
            content.Add(new StringContent("30000"), "MonthlyRent");
            content.Add(new StringContent("1"), "CityId");
            content.Add(new StringContent("1"), "DistrictId");
            content.Add(new StringContent("測試地址"), "AddressLine");
            content.Add(new StringContent("4"), "RoomCount");
            content.Add(new StringContent("2"), "LivingRoomCount");
            content.Add(new StringContent("2"), "BathroomCount");
            content.Add(new StringContent("40"), "Area");
            content.Add(new StringContent("7"), "CurrentFloor");
            content.Add(new StringContent("15"), "TotalFloors");
            content.Add(new StringContent("2"), "DepositMonths");
            content.Add(new StringContent("12"), "MinimumRentalMonths");
            content.Add(new StringContent("true"), "ManagementFeeIncluded");
            content.Add(new StringContent("1500"), "ManagementFeeAmount");
            content.Add(new StringContent("true"), "ParkingAvailable");
            content.Add(new StringContent("true"), "ParkingFeeRequired");
            content.Add(new StringContent("2000"), "ParkingFeeAmount");
            content.Add(new StringContent("測試混合檔案上傳功能"), "Description");
            content.Add(new StringContent("1"), "ListingPlanId");

            // 加入圖片檔案
            foreach (var file in imageFiles)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "PropertyImages", file.FileName);
            }

            // 加入 PDF 檔案
            var pdfStreamContent = new StreamContent(pdfFile.OpenReadStream());
            pdfStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(pdfFile.ContentType);
            content.Add(pdfStreamContent, "PropertyProofDocument", pdfFile.FileName);

            // 執行 POST 請求
            var response = await _client.PostAsync("/Property/Create", content);

            // Assert - 驗證結果
            response.IsSuccessStatusCode.Should().BeTrue($"混合檔案上傳請求應該成功，但收到狀態碼: {response.StatusCode}");

            // 驗證資料庫記錄
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();

            // 驗證圖片記錄
            var uploadedImages = await context.Images
                .Where(img => img.EntityType == EntityType.Property 
                           && img.EntityId == testPropertyId 
                           && img.Category == ImageCategory.Gallery
                           && img.StoredFileName.Contains("TEST_"))
                .ToListAsync();

            uploadedImages.Should().HaveCount(2, "應該上傳2張測試圖片");

            // 驗證 PDF 記錄
            var uploadedPdf = await context.Images
                .Where(img => img.EntityType == EntityType.Property 
                           && img.EntityId == testPropertyId 
                           && img.Category == ImageCategory.Document
                           && img.StoredFileName.Contains("TEST_"))
                .FirstOrDefaultAsync();

            uploadedPdf.Should().NotBeNull("應該找到上傳的 PDF 記錄");

            // 驗證總計記錄數
            var totalUploaded = await context.Images
                .CountAsync(img => img.EntityType == EntityType.Property 
                                && img.EntityId == testPropertyId 
                                && img.StoredFileName.Contains("TEST_"));

            totalUploaded.Should().Be(3, "應該總共有3個檔案記錄 (2張圖片 + 1個PDF)");
        }

        /// <summary>
        /// 設置測試身份驗證並取得 AntiForgery Token
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        private async Task<string> SetupAuthenticationAndGetTokenAsync(int userId)
        {
            // 第一步：取得 Create 頁面以獲取 AntiForgery Token
            _client.DefaultRequestHeaders.Add("Cookie", $"UserId={userId}");
            
            var getResponse = await _client.GetAsync("/Property/Create");
            var content = await getResponse.Content.ReadAsStringAsync();
            
            // 從回應中解析 AntiForgery Token
            var tokenMatch = System.Text.RegularExpressions.Regex.Match(content, 
                @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]*)"" />|""__RequestVerificationToken"":""([^""]*)""");
            
            if (tokenMatch.Success)
            {
                return tokenMatch.Groups[1].Value ?? tokenMatch.Groups[2].Value;
            }
            
            // 如果找不到 token，返回空字串（測試環境可能會跳過驗證）
            return string.Empty;
        }

        /// <summary>
        /// 取得現有房源 ID，避免建立新房源的資料庫約束問題
        /// 專注於測試檔案上傳功能，而非房源建立功能
        /// </summary>
        private async Task<int> GetExistingPropertyAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();

            // 優先查詢房東 ID 51 的房源
            var landlordProperty = await context.Properties
                .Where(p => p.LandlordMemberId == 51)
                .FirstOrDefaultAsync();
                
            if (landlordProperty != null)
            {
                return landlordProperty.PropertyId;
            }

            // 如果沒有房東 51 的房源，查詢任何現有房源
            var anyProperty = await context.Properties
                .FirstOrDefaultAsync();
                
            if (anyProperty != null)
            {
                return anyProperty.PropertyId;
            }

            // 如果資料庫中完全沒有房源，拋出例外
            throw new InvalidOperationException("資料庫中沒有任何房源記錄，無法進行檔案上傳測試");
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}