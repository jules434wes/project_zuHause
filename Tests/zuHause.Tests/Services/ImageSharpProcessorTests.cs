using FluentAssertions;
using zuHause.Services;
using zuHause.DTOs;

namespace zuHause.Tests.Services
{
    /// <summary>
    /// ImageSharpProcessor 單元測試
    /// 遵循反過度模擬原則：使用真實的 ImageSharp 進行狀態驗證
    /// </summary>
    public class ImageSharpProcessorTests : IDisposable
    {
        private readonly ImageSharpProcessor _processor;
        private readonly string _testDataPath;

        public ImageSharpProcessorTests()
        {
            _processor = new ImageSharpProcessor();
            _testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        }

        [Fact]
        public async Task ConvertToWebPAsync_WithValidPngImage_ShouldReturnSuccessResult()
        {
            // Arrange
            var testImagePath = Path.Combine(_testDataPath, "test-image.png");
            using var sourceStream = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);

            // Act
            var result = await _processor.ConvertToWebPAsync(sourceStream);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
            result.ProcessedStream.Should().NotBeNull();
            result.ProcessedFormat.Should().Be("webp");
            result.Width.Should().BeGreaterThan(0);
            result.Height.Should().BeGreaterThan(0);
            result.SizeBytes.Should().BeGreaterThan(0);
            result.IsValidForDisplay().Should().BeTrue();
        }

        [Fact]
        public async Task ConvertToWebPAsync_WithMaxWidth_ShouldResizeImage()
        {
            // Arrange
            var testImagePath = Path.Combine(_testDataPath, "test-image.png");
            using var sourceStream = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);
            var maxWidth = 100;

            // Act
            var result = await _processor.ConvertToWebPAsync(sourceStream, maxWidth, 80);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Width.Should().BeLessThanOrEqualTo(maxWidth);
            result.ProcessedFormat.Should().Be("webp");
            result.IsValidForDisplay().Should().BeTrue();
        }

        [Fact]
        public async Task ConvertToWebPAsync_WithInvalidStream_ShouldReturnFailureResult()
        {
            // Arrange
            using var invalidStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

            // Act
            var result = await _processor.ConvertToWebPAsync(invalidStream);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
            result.ProcessedStream.Should().BeNull();
            result.IsValidForDisplay().Should().BeFalse();
        }

        [Fact]
        public async Task GenerateThumbnailAsync_WithValidImage_ShouldReturnFixedSizeThumbnail()
        {
            // Arrange
            var testImagePath = Path.Combine(_testDataPath, "test-image.png");
            using var sourceStream = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);
            var targetWidth = 150;
            var targetHeight = 150;

            // Act
            var result = await _processor.GenerateThumbnailAsync(sourceStream, targetWidth, targetHeight);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Width.Should().Be(targetWidth);
            result.Height.Should().Be(targetHeight);
            result.ProcessedFormat.Should().Be("webp");
            result.ProcessedStream.Should().NotBeNull();
            result.SizeBytes.Should().BeGreaterThan(0);
            result.IsValidForDisplay().Should().BeTrue();
        }

        [Fact]
        public async Task GenerateThumbnailAsync_WithInvalidDimensions_ShouldReturnFailureResult()
        {
            // Arrange
            var testImagePath = Path.Combine(_testDataPath, "test-image.png");
            using var sourceStream = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);

            // Act
            var result = await _processor.GenerateThumbnailAsync(sourceStream, 0, 100);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("縮圖尺寸必須大於 0");
            result.ProcessedStream.Should().BeNull();
            result.IsValidForDisplay().Should().BeFalse();
        }

        [Fact]
        public async Task ConvertToWebPAsync_WithEmptyStream_ShouldReturnFailureResult()
        {
            // Arrange
            using var emptyStream = new MemoryStream();

            // Act
            var result = await _processor.ConvertToWebPAsync(emptyStream);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
            result.ProcessedStream.Should().BeNull();
            result.IsValidForDisplay().Should().BeFalse();
        }

        [Theory]
        [InlineData(50)]
        [InlineData(80)]
        [InlineData(95)]
        public async Task ConvertToWebPAsync_WithDifferentQualityLevels_ShouldProcessSuccessfully(int quality)
        {
            // Arrange
            var testImagePath = Path.Combine(_testDataPath, "test-image.png");
            using var sourceStream = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);

            // Act
            var result = await _processor.ConvertToWebPAsync(sourceStream, null, quality);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ProcessedFormat.Should().Be("webp");
            result.IsValidForDisplay().Should().BeTrue();
        }

        [Fact]
        public void ImageProcessingResult_CreateSuccess_ShouldCreateValidResult()
        {
            // Arrange
            using var testStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            var width = 200;
            var height = 150;
            var originalFormat = "png";

            // Act
            var result = ImageProcessingResult.CreateSuccess(testStream, width, height, originalFormat);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Width.Should().Be(width);
            result.Height.Should().Be(height);
            result.OriginalFormat.Should().Be(originalFormat);
            result.ProcessedFormat.Should().Be("webp");
            result.GetDimensionDescription().Should().Be($"{width}x{height}");
            result.IsValidForDisplay().Should().BeTrue();
            result.IsWebOptimized().Should().BeTrue();
        }

        [Fact]
        public void ImageProcessingResult_CreateFailure_ShouldCreateFailureResult()
        {
            // Arrange
            var errorMessage = "測試錯誤訊息";

            // Act
            var result = ImageProcessingResult.CreateFailure(errorMessage);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be(errorMessage);
            result.ProcessedStream.Should().BeNull();
            result.IsValidForDisplay().Should().BeFalse();
            result.GetProcessingSummary().Should().Contain(errorMessage);
        }

        public void Dispose()
        {
            // ImageSharpProcessor 不需要 Dispose，此處保留介面相容性
        }
    }
}