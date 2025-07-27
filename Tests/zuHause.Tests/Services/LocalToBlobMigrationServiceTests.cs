using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using zuHause.Data;
using zuHause.Enums;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Services;
using zuHause.Tests.Fixtures;
using System.Transactions;

namespace zuHause.Tests.Services
{
    /// <summary>
    /// LocalToBlobMigrationService 單元測試
    /// 遵循 anti-mocking 原則：只 mock 外部依賴，不 mock 業務邏輯
    /// 使用真實 SQL Server 資料庫和事務隔離確保測試可靠性
    /// </summary>
    public class LocalToBlobMigrationServiceTests : IClassFixture<TestDatabaseFixture>, IDisposable
    {
        private readonly TestDatabaseFixture _databaseFixture;
        private readonly ZuHauseContext _context;
        private readonly Mock<IBlobStorageService> _mockBlobStorageService;
        private readonly Mock<IBlobUrlGenerator> _mockUrlGenerator;
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<LocalToBlobMigrationService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly LocalToBlobMigrationService _service;
        private readonly TransactionScope _transactionScope;
        private readonly List<Guid> _testImageGuids;

        public LocalToBlobMigrationServiceTests(TestDatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            
            // 使用事務範圍確保測試隔離
            _transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            
            // 使用共享的測試資料庫
            _context = _databaseFixture.CreateDbContext();
            
            // 初始化測試資料 GUID 列表
            _testImageGuids = new List<Guid>();

            // Mock 外部依賴
            _mockBlobStorageService = new Mock<IBlobStorageService>();
            _mockUrlGenerator = new Mock<IBlobUrlGenerator>();
            _mockImageProcessor = new Mock<IImageProcessor>();
            _mockLogger = new Mock<ILogger<LocalToBlobMigrationService>>();
            _mockConfiguration = new Mock<IConfiguration>();

            // 使用真實的 Memory Cache
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            // 設定 Configuration mock
            _mockConfiguration.Setup(x => x["ImageStorage:LocalPath"])
                .Returns("wwwroot/images");

            _service = new LocalToBlobMigrationService(
                _context,
                _mockBlobStorageService.Object,
                _mockUrlGenerator.Object,
                _mockImageProcessor.Object,
                _memoryCache,
                _mockLogger.Object,
                _mockConfiguration.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // 使用唯一的 GUID 作為測試資料識別符
            var testImageGuid1 = Guid.NewGuid();
            var testImageGuid2 = Guid.NewGuid();
            _testImageGuids.Add(testImageGuid1);
            _testImageGuids.Add(testImageGuid2);
            
            var testImages = new List<Image>
            {
                new Image
                {
                    // 移除 ImageId，讓資料庫自動產生 IDENTITY 值
                    ImageGuid = testImageGuid1,
                    EntityType = EntityType.Property,
                    EntityId = 100,
                    Category = ImageCategory.Gallery,
                    StoredFileName = "test_image_1.webp",
                    OriginalFileName = "original_1.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Image
                {
                    // 移除 ImageId，讓資料庫自動產生 IDENTITY 值
                    ImageGuid = testImageGuid2,
                    EntityType = EntityType.Property,
                    EntityId = 101,
                    Category = ImageCategory.Gallery,
                    StoredFileName = "test_image_2.webp",
                    OriginalFileName = "original_2.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            _context.Images.AddRange(testImages);
            _context.SaveChanges();
        }

        [Fact]
        public async Task ScanLocalImagesAsync_ShouldReturnCorrectResults()
        {
            // Arrange
            var scanOptions = new LocalImageScanOptions
            {
                ValidateFileIntegrity = false // 跳過檔案完整性檢查以專注於邏輯測試
            };

            // Act
            var result = await _service.ScanLocalImagesAsync(scanOptions);

            // Assert
            Assert.NotNull(result);
            
            // 只驗證測試創建的資料，不假設資料庫為空
            var testImageCount = _context.Images.Count(i => _testImageGuids.Contains(i.ImageGuid));
            Assert.True(result.TotalImages >= testImageCount, $"期望至少找到 {testImageCount} 筆測試資料，實際找到 {result.TotalImages} 筆");
            
            Assert.True(result.ScanTime <= DateTime.UtcNow);
            Assert.NotNull(result.ReadyToMigrate);
            Assert.NotNull(result.ProblematicImages);
        }

        [Fact]
        public async Task ScanLocalImagesAsync_WithDateFilter_ShouldFilterCorrectly()
        {
            // Arrange
            var scanOptions = new LocalImageScanOptions
            {
                ModifiedAfter = DateTime.UtcNow.AddHours(-25), // 只取 1 天內的
                ValidateFileIntegrity = false
            };

            // Act
            var result = await _service.ScanLocalImagesAsync(scanOptions);

            // Assert
            Assert.NotNull(result);
            
            // 驗證測試資料中符合時間範圍的數量
            var expectedTestImages = _context.Images
                .Where(i => _testImageGuids.Contains(i.ImageGuid) && i.UploadedAt >= scanOptions.ModifiedAfter)
                .Count();
            Assert.True(result.TotalImages >= expectedTestImages, $"期望至少找到 {expectedTestImages} 筆符合時間範圍的測試資料");
        }

        [Fact]
        public async Task StartMigrationAsync_ShouldCreateValidSession()
        {
            // Arrange
            var config = new MigrationConfiguration
            {
                Name = "測試遷移",
                BatchSize = 10,
                MaxConcurrency = 2,
                DeleteLocalFilesAfterMigration = false
            };

            // Act
            var session = await _service.StartMigrationAsync(config);

            // Assert
            Assert.NotNull(session);
            Assert.Equal("測試遷移", session.Name);
            Assert.Equal(MigrationStatus.Created, session.Status);
            Assert.Equal(10, session.BatchSize);
            Assert.False(string.IsNullOrEmpty(session.MigrationId));
            Assert.True(session.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public async Task PauseMigrationAsync_WithValidRunningMigration_ShouldReturnTrue()
        {
            // Arrange
            var config = new MigrationConfiguration { Name = "測試遷移" };
            var session = await _service.StartMigrationAsync(config);
            
            // 模擬遷移開始運行
            session.Status = MigrationStatus.Running;
            _memoryCache.Set($"migration_session_{session.MigrationId}", session);

            // Act
            var result = await _service.PauseMigrationAsync(session.MigrationId);

            // Assert
            Assert.True(result);
            
            var updatedSession = _memoryCache.Get<MigrationSession>($"migration_session_{session.MigrationId}");
            Assert.NotNull(updatedSession);
            Assert.Equal(MigrationStatus.Paused, updatedSession.Status);
            Assert.NotNull(updatedSession.PausedAt);
        }

        [Fact]
        public async Task PauseMigrationAsync_WithInvalidMigration_ShouldReturnFalse()
        {
            // Arrange
            var invalidMigrationId = "non-existent-id";

            // Act
            var result = await _service.PauseMigrationAsync(invalidMigrationId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ResumeMigrationAsync_WithValidPausedMigration_ShouldReturnTrue()
        {
            // Arrange
            var config = new MigrationConfiguration { Name = "測試遷移" };
            var session = await _service.StartMigrationAsync(config);
            
            // 模擬遷移暫停狀態
            session.Status = MigrationStatus.Paused;
            session.PausedAt = DateTime.UtcNow;
            _memoryCache.Set($"migration_session_{session.MigrationId}", session);

            // Act
            var result = await _service.ResumeMigrationAsync(session.MigrationId);

            // Assert
            Assert.True(result);
            
            var updatedSession = _memoryCache.Get<MigrationSession>($"migration_session_{session.MigrationId}");
            Assert.NotNull(updatedSession);
            Assert.Equal(MigrationStatus.Running, updatedSession.Status);
            Assert.Null(updatedSession.PausedAt);
        }

        [Fact]
        public async Task CancelMigrationAsync_WithValidMigration_ShouldReturnTrue()
        {
            // Arrange
            var config = new MigrationConfiguration { Name = "測試遷移" };
            var session = await _service.StartMigrationAsync(config);

            // Act
            var result = await _service.CancelMigrationAsync(session.MigrationId);

            // Assert
            Assert.True(result);
            
            var updatedSession = _memoryCache.Get<MigrationSession>($"migration_session_{session.MigrationId}");
            Assert.NotNull(updatedSession);
            Assert.Equal(MigrationStatus.Cancelled, updatedSession.Status);
            Assert.NotNull(updatedSession.CancelledAt);
        }

        [Fact]
        public async Task GetMigrationProgressAsync_WithValidMigration_ShouldReturnProgress()
        {
            // Arrange
            var config = new MigrationConfiguration { Name = "測試遷移" };
            var session = await _service.StartMigrationAsync(config);
            
            // 模擬進度
            session.TotalImages = 100;
            session.ProcessedImages = 50;
            session.SuccessCount = 45;
            session.FailureCount = 5;
            session.StartedAt = DateTime.UtcNow.AddMinutes(-10);
            _memoryCache.Set($"migration_session_{session.MigrationId}", session);

            // Act
            var progress = await _service.GetMigrationProgressAsync(session.MigrationId);

            // Assert
            Assert.NotNull(progress);
            Assert.Equal(session.MigrationId, progress.MigrationId);
            Assert.Equal(100, progress.TotalImages);
            Assert.Equal(50, progress.ProcessedImages);
            Assert.Equal(45, progress.SuccessCount);
            Assert.Equal(5, progress.FailureCount);
            Assert.Equal(50.0, progress.ProgressPercentage);
            Assert.NotNull(progress.ElapsedTime);
        }

        [Fact]
        public async Task GetMigrationProgressAsync_WithInvalidMigration_ShouldThrowException()
        {
            // Arrange
            var invalidMigrationId = "non-existent-id";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetMigrationProgressAsync(invalidMigrationId));
            
            Assert.Contains("找不到遷移會話", exception.Message);
        }

        [Fact]
        public async Task GetMigrationSessionsAsync_ShouldReturnAllSessions()
        {
            // Arrange
            var config1 = new MigrationConfiguration { Name = "遷移1" };
            var config2 = new MigrationConfiguration { Name = "遷移2" };
            
            var session1 = await _service.StartMigrationAsync(config1);
            var session2 = await _service.StartMigrationAsync(config2);

            // Act
            var sessions = await _service.GetMigrationSessionsAsync();

            // Assert
            Assert.NotNull(sessions);
            Assert.Equal(2, sessions.Count);
            Assert.Contains(sessions, s => s.Name == "遷移1");
            Assert.Contains(sessions, s => s.Name == "遷移2");
            
            // 驗證按創建時間降序排列
            Assert.True(sessions[0].CreatedAt >= sessions[1].CreatedAt);
        }

        [Fact]
        public async Task ValidateMigrationAsync_WithValidMigration_ShouldReturnValidationResult()
        {
            // Arrange
            var config = new MigrationConfiguration { Name = "測試遷移" };
            var session = await _service.StartMigrationAsync(config);
            
            session.TotalImages = 10;
            session.SuccessCount = 8;
            _memoryCache.Set($"migration_session_{session.MigrationId}", session);

            // Act
            var result = await _service.ValidateMigrationAsync(session.MigrationId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Equal(10, result.TotalImages);
            Assert.Equal(8, result.ValidatedImages);
        }

        [Fact]
        public async Task CleanupLocalFilesAsync_WithValidMigration_ShouldReturnCleanupResult()
        {
            // Arrange
            var config = new MigrationConfiguration { Name = "測試遷移" };
            var session = await _service.StartMigrationAsync(config);
            
            session.MigratedFiles.Add("path1");
            session.MigratedFiles.Add("path2");
            _memoryCache.Set($"migration_session_{session.MigrationId}", session);

            // Act
            var result = await _service.CleanupLocalFilesAsync(session.MigrationId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.TotalFiles);
            Assert.Equal(2, result.DeletedFiles);
        }

        [Fact]
        public async Task RollbackMigrationAsync_WithValidMigration_ShouldReturnTrue()
        {
            // Arrange
            var config = new MigrationConfiguration { Name = "測試遷移" };
            var session = await _service.StartMigrationAsync(config);
            
            var migratedFiles = new List<string> { "blob1", "blob2" };
            session.MigratedFiles.AddRange(migratedFiles);
            _memoryCache.Set($"migration_session_{session.MigrationId}", session);

            // 設定 BlobStorageService mock 回傳成功
            _mockBlobStorageService.Setup(x => x.DeleteMultipleAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, bool> 
                { 
                    { "blob1", true }, 
                    { "blob2", true } 
                });

            // Act
            var result = await _service.RollbackMigrationAsync(session.MigrationId);

            // Assert
            Assert.True(result);
            
            // 驗證 DeleteMultipleAsync 被調用
            _mockBlobStorageService.Verify(
                x => x.DeleteMultipleAsync(It.Is<IEnumerable<string>>(files => 
                    files.Count() == 2 && files.Contains("blob1") && files.Contains("blob2"))), 
                Times.Once);
        }

        [Fact]
        public async Task RollbackMigrationAsync_WithBlobStorageError_ShouldReturnFalse()
        {
            // Arrange
            var config = new MigrationConfiguration { Name = "測試遷移" };
            var session = await _service.StartMigrationAsync(config);
            
            session.MigratedFiles.Add("blob1");
            _memoryCache.Set($"migration_session_{session.MigrationId}", session);

            // 設定 BlobStorageService mock 拋出異常
            _mockBlobStorageService.Setup(x => x.DeleteMultipleAsync(It.IsAny<IEnumerable<string>>()))
                .ThrowsAsync(new Exception("Blob Storage 錯誤"));

            // Act
            var result = await _service.RollbackMigrationAsync(session.MigrationId);

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            try
            {
                // 使用事務回滾代替資料庫刪除，確保測試隔離
                _transactionScope?.Dispose();
                
                Console.WriteLine("✓ 測試事務已回滾，測試資料已清理");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 測試清理時發生錯誤：{ex.Message}");
            }
            finally
            {
                _context?.Dispose();
                _memoryCache?.Dispose();
            }
        }
    }
}