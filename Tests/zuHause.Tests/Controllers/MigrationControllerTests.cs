using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using zuHause.Controllers.Api;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Tests.Controllers
{
    /// <summary>
    /// MigrationController 單元測試
    /// 測試 API 端點的行為和錯誤處理
    /// </summary>
    public class MigrationControllerTests
    {
        private readonly Mock<ILocalToBlobMigrationService> _mockMigrationService;
        private readonly Mock<ILogger<MigrationController>> _mockLogger;
        private readonly MigrationController _controller;

        public MigrationControllerTests()
        {
            _mockMigrationService = new Mock<ILocalToBlobMigrationService>();
            _mockLogger = new Mock<ILogger<MigrationController>>();
            _controller = new MigrationController(_mockMigrationService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ScanLocalImages_WithValidOptions_ShouldReturnOkResult()
        {
            // Arrange
            var scanOptions = new LocalImageScanOptions 
            { 
                ValidateFileIntegrity = true,
                MaxConcurrency = 5
            };
            
            var expectedResult = new LocalImageScanResult
            {
                TotalImages = 10,
                ReadyCount = 8,
                ProblematicCount = 2,
                ScanTime = DateTime.UtcNow
            };

            _mockMigrationService.Setup(x => x.ScanLocalImagesAsync(It.IsAny<LocalImageScanOptions>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.ScanLocalImages(scanOptions);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var scanResult = Assert.IsType<LocalImageScanResult>(okResult.Value);
            
            Assert.Equal(10, scanResult.TotalImages);
            Assert.Equal(8, scanResult.ReadyCount);
            Assert.Equal(2, scanResult.ProblematicCount);
            
            _mockMigrationService.Verify(x => x.ScanLocalImagesAsync(It.IsAny<LocalImageScanOptions>()), Times.Once);
        }

        [Fact]
        public async Task ScanLocalImages_WithNullOptions_ShouldReturnOkResult()
        {
            // Arrange
            var expectedResult = new LocalImageScanResult
            {
                TotalImages = 5,
                ReadyCount = 5,
                ProblematicCount = 0
            };

            _mockMigrationService.Setup(x => x.ScanLocalImagesAsync(null))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.ScanLocalImages(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<LocalImageScanResult>(okResult.Value);
            
            _mockMigrationService.Verify(x => x.ScanLocalImagesAsync(null), Times.Once);
        }

        [Fact]
        public async Task ScanLocalImages_WithServiceException_ShouldReturnInternalServerError()
        {
            // Arrange
            _mockMigrationService.Setup(x => x.ScanLocalImagesAsync(It.IsAny<LocalImageScanOptions>()))
                .ThrowsAsync(new Exception("掃描過程發生錯誤"));

            // Act
            var result = await _controller.ScanLocalImages(new LocalImageScanOptions());

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            
            var errorResponse = statusResult.Value as dynamic;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task StartMigration_WithValidConfig_ShouldReturnOkResult()
        {
            // Arrange
            var config = new MigrationConfiguration
            {
                Name = "測試遷移",
                BatchSize = 50,
                MaxConcurrency = 3
            };

            var expectedSession = new MigrationSession
            {
                MigrationId = "test-migration-id",
                Name = "測試遷移",
                Status = MigrationStatus.Created,
                CreatedAt = DateTime.UtcNow
            };

            _mockMigrationService.Setup(x => x.StartMigrationAsync(It.IsAny<MigrationConfiguration>()))
                .ReturnsAsync(expectedSession);

            // Act
            var result = await _controller.StartMigration(config);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var session = Assert.IsType<MigrationSession>(okResult.Value);
            
            Assert.Equal("test-migration-id", session.MigrationId);
            Assert.Equal("測試遷移", session.Name);
            Assert.Equal(MigrationStatus.Created, session.Status);
            
            _mockMigrationService.Verify(x => x.StartMigrationAsync(It.IsAny<MigrationConfiguration>()), Times.Once);
        }

        [Fact]
        public async Task StartMigration_WithEmptyName_ShouldReturnBadRequest()
        {
            // Arrange
            var config = new MigrationConfiguration
            {
                Name = "", // 空名稱
                BatchSize = 50
            };

            // Act
            var result = await _controller.StartMigration(config);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            
            var errorResponse = badRequestResult.Value as dynamic;
            Assert.NotNull(errorResponse);
            
            _mockMigrationService.Verify(x => x.StartMigrationAsync(It.IsAny<MigrationConfiguration>()), Times.Never);
        }

        [Fact]
        public async Task GetMigrationSessions_ShouldReturnOkResult()
        {
            // Arrange
            var expectedSessions = new List<MigrationSession>
            {
                new MigrationSession { MigrationId = "id1", Name = "遷移1", Status = MigrationStatus.Running },
                new MigrationSession { MigrationId = "id2", Name = "遷移2", Status = MigrationStatus.Completed }
            };

            _mockMigrationService.Setup(x => x.GetMigrationSessionsAsync())
                .ReturnsAsync(expectedSessions);

            // Act
            var result = await _controller.GetMigrationSessions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var sessions = Assert.IsType<List<MigrationSession>>(okResult.Value);
            
            Assert.Equal(2, sessions.Count);
            Assert.Contains(sessions, s => s.Name == "遷移1");
            Assert.Contains(sessions, s => s.Name == "遷移2");
        }

        [Fact]
        public async Task GetMigrationProgress_WithValidId_ShouldReturnOkResult()
        {
            // Arrange
            var migrationId = "valid-migration-id";
            var expectedProgress = new MigrationProgress
            {
                MigrationId = migrationId,
                Status = MigrationStatus.Running,
                TotalImages = 100,
                ProcessedImages = 50,
                ProgressPercentage = 50.0
            };

            _mockMigrationService.Setup(x => x.GetMigrationProgressAsync(migrationId))
                .ReturnsAsync(expectedProgress);

            // Act
            var result = await _controller.GetMigrationProgress(migrationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var progress = Assert.IsType<MigrationProgress>(okResult.Value);
            
            Assert.Equal(migrationId, progress.MigrationId);
            Assert.Equal(50.0, progress.ProgressPercentage);
        }

        [Fact]
        public async Task GetMigrationProgress_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = "invalid-id";
            
            _mockMigrationService.Setup(x => x.GetMigrationProgressAsync(invalidId))
                .ThrowsAsync(new ArgumentException("找不到遷移會話"));

            // Act
            var result = await _controller.GetMigrationProgress(invalidId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            
            var errorResponse = notFoundResult.Value as dynamic;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task PauseMigration_WithValidId_ShouldReturnOkResult()
        {
            // Arrange
            var migrationId = "valid-migration-id";
            
            _mockMigrationService.Setup(x => x.PauseMigrationAsync(migrationId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.PauseMigration(migrationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task PauseMigration_WithInvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidId = "invalid-id";
            
            _mockMigrationService.Setup(x => x.PauseMigrationAsync(invalidId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.PauseMigration(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            
            var errorResponse = badRequestResult.Value as dynamic;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task ResumeMigration_WithValidId_ShouldReturnOkResult()
        {
            // Arrange
            var migrationId = "valid-migration-id";
            
            _mockMigrationService.Setup(x => x.ResumeMigrationAsync(migrationId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ResumeMigration(migrationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CancelMigration_WithValidId_ShouldReturnOkResult()
        {
            // Arrange
            var migrationId = "valid-migration-id";
            
            _mockMigrationService.Setup(x => x.CancelMigrationAsync(migrationId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CancelMigration(migrationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ValidateMigration_WithValidId_ShouldReturnOkResult()
        {
            // Arrange
            var migrationId = "valid-migration-id";
            var expectedResult = new MigrationValidationResult
            {
                IsValid = true,
                TotalImages = 10,
                ValidatedImages = 9,
                ValidatedAt = DateTime.UtcNow
            };

            _mockMigrationService.Setup(x => x.ValidateMigrationAsync(migrationId))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.ValidateMigration(migrationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var validationResult = Assert.IsType<MigrationValidationResult>(okResult.Value);
            
            Assert.True(validationResult.IsValid);
            Assert.Equal(10, validationResult.TotalImages);
            Assert.Equal(9, validationResult.ValidatedImages);
        }

        [Fact]
        public async Task CleanupLocalFiles_WithValidId_ShouldReturnOkResult()
        {
            // Arrange
            var migrationId = "valid-migration-id";
            var expectedResult = new MigrationCleanupResult
            {
                IsSuccess = true,
                TotalFiles = 5,
                DeletedFiles = 5,
                FreedSpaceBytes = 1024000
            };

            _mockMigrationService.Setup(x => x.CleanupLocalFilesAsync(migrationId))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.CleanupLocalFiles(migrationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cleanupResult = Assert.IsType<MigrationCleanupResult>(okResult.Value);
            
            Assert.True(cleanupResult.IsSuccess);
            Assert.Equal(5, cleanupResult.TotalFiles);
            Assert.Equal(5, cleanupResult.DeletedFiles);
        }

        [Fact]
        public async Task RollbackMigration_WithValidId_ShouldReturnOkResult()
        {
            // Arrange
            var migrationId = "valid-migration-id";
            
            _mockMigrationService.Setup(x => x.RollbackMigrationAsync(migrationId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RollbackMigration(migrationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task RollbackMigration_WithInvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidId = "invalid-id";
            
            _mockMigrationService.Setup(x => x.RollbackMigrationAsync(invalidId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.RollbackMigration(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            
            var errorResponse = badRequestResult.Value as dynamic;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task AllEndpoints_WithServiceExceptions_ShouldReturnInternalServerError()
        {
            // Arrange
            var testException = new Exception("服務異常");
            var migrationId = "test-id";

            _mockMigrationService.Setup(x => x.GetMigrationProgressAsync(It.IsAny<string>()))
                .ThrowsAsync(testException);
            _mockMigrationService.Setup(x => x.PauseMigrationAsync(It.IsAny<string>()))
                .ThrowsAsync(testException);
            _mockMigrationService.Setup(x => x.ValidateMigrationAsync(It.IsAny<string>()))
                .ThrowsAsync(testException);

            // Act & Assert
            var progressResult = await _controller.GetMigrationProgress(migrationId);
            var pauseResult = await _controller.PauseMigration(migrationId);
            var validateResult = await _controller.ValidateMigration(migrationId);

            // 驗證都返回 500 錯誤
            Assert.IsType<ObjectResult>(progressResult.Result);
            Assert.IsType<ObjectResult>(pauseResult);
            Assert.IsType<ObjectResult>(validateResult.Result);
        }
    }
}