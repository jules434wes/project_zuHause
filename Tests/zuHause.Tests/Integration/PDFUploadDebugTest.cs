using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using System.Text.Json;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using zuHause.Tests.TestHelpers;

namespace zuHause.Tests.Integration
{
    public class PDFUploadDebugTest : IClassFixture<AzureTestWebApplicationFactory<Program>>
    {
        private readonly AzureTestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public PDFUploadDebugTest(AzureTestWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task SinglePDF_Upload_ShouldSucceed()
        {
            // 設置身份驗證
            _output.WriteLine("🔑 設置身份驗證 (UserId: 51)...");
            _client.DefaultRequestHeaders.Add("Cookie", $"UserId=51");

            // 創建 PDF 文件
            var pdfFile = RealFileBuilder.CreateRealPdfDocument("測試 PDF 調試內容", "test-debug.pdf");

            // 準備上傳內容
            var tempUploadContent = new MultipartFormDataContent();
            tempUploadContent.Add(new StringContent("Document"), "category");
            
            var pdfStreamContent = new StreamContent(pdfFile.OpenReadStream());
            pdfStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(pdfFile.ContentType);
            tempUploadContent.Add(pdfStreamContent, "files", pdfFile.FileName);

            _output.WriteLine($"📤 開始上傳 PDF: {pdfFile.FileName}");
            _output.WriteLine($"   File Size: {pdfFile.Length} bytes");
            _output.WriteLine($"   Content Type: {pdfFile.ContentType}");

            // 執行上傳
            var response = await _client.PostAsync("/api/images/temp-upload", tempUploadContent);

            // 讀取響應
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _output.WriteLine($"📥 上傳響應:");
            _output.WriteLine($"   Status Code: {response.StatusCode}");
            _output.WriteLine($"   Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
            _output.WriteLine($"   Content: {responseContent}");

            // 驗證響應
            response.IsSuccessStatusCode.Should().BeTrue($"PDF 上傳應該成功。響應: {responseContent}");

            // 解析 JSON 響應
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            jsonResponse.GetProperty("success").GetBoolean().Should().BeTrue();
            
            var tempSessionId = jsonResponse.GetProperty("tempSessionId").GetString();
            tempSessionId.Should().NotBeNullOrEmpty();

            _output.WriteLine($"✅ PDF 上傳成功，TempSessionId: {tempSessionId}");
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}