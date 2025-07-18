using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Services;
using zuHause.Interfaces;

namespace zuHause.Tests.Services
{
    public class EntityExistenceCheckerTests : IDisposable
    {
        private readonly ZuHauseContext _context;
        private readonly IEntityExistenceChecker _service;

        public EntityExistenceCheckerTests()
        {
            var options = new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ZuHauseContext(options);
            _service = new EntityExistenceChecker(_context);

            // 設定測試資料
            SetupTestData();
        }

        private void SetupTestData()
        {
            // 新增測試會員
            _context.Members.AddRange(
                new Member { MemberId = 1, Email = "test1@example.com", MemberName = "Test User 1", Password = "password123", PhoneNumber = "0912345678" },
                new Member { MemberId = 2, Email = "test2@example.com", MemberName = "Test User 2", Password = "password456", PhoneNumber = "0987654321" }
            );

            // 新增測試房源
            _context.Properties.AddRange(
                new Property { PropertyId = 1, Title = "Test Property 1", LandlordMemberId = 1, MonthlyRent = 15000, ElectricityFeeType = "固定式", StatusCode = "active", WaterFeeType = "固定式", CreatedAt = DateTime.UtcNow },
                new Property { PropertyId = 2, Title = "Test Property 2", LandlordMemberId = 2, MonthlyRent = 20000, ElectricityFeeType = "固定式", StatusCode = "active", WaterFeeType = "固定式", CreatedAt = DateTime.UtcNow }
            );

            // 新增測試家具
            _context.FurnitureProducts.AddRange(
                new FurnitureProduct { FurnitureProductId = "1", ProductName = "Test Furniture 1" },
                new FurnitureProduct { FurnitureProductId = "2", ProductName = "Test Furniture 2" }
            );

            // 新增測試公告
            _context.SystemMessages.AddRange(
                new SystemMessage { MessageId = 1, Title = "Test Message 1", CategoryCode = "TEST", AudienceTypeCode = "ALL", MessageContent = "Test Content", AdminId = 1 },
                new SystemMessage { MessageId = 2, Title = "Test Message 2", CategoryCode = "TEST", AudienceTypeCode = "ALL", MessageContent = "Test Content", AdminId = 1 }
            );

            _context.SaveChanges();
        }

        [Theory]
        [InlineData(EntityType.Member, 1, true)]
        [InlineData(EntityType.Member, 999, false)]
        [InlineData(EntityType.Property, 1, true)]
        [InlineData(EntityType.Property, 999, false)]
        [InlineData(EntityType.Furniture, 1, true)]
        [InlineData(EntityType.Furniture, 999, false)]
        [InlineData(EntityType.Announcement, 1, true)]
        [InlineData(EntityType.Announcement, 999, false)]
        public async Task ExistsAsync_ShouldReturnExpectedResult(EntityType entityType, int entityId, bool expected)
        {
            // Act
            var result = await _service.ExistsAsync(entityType, entityId);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(EntityType.Member, 0, false)]
        [InlineData(EntityType.Member, -1, false)]
        [InlineData(EntityType.Property, 0, false)]
        [InlineData(EntityType.Property, -1, false)]
        public async Task ExistsAsync_WithInvalidEntityId_ShouldReturnFalse(EntityType entityType, int entityId, bool expected)
        {
            // Act
            var result = await _service.ExistsAsync(entityType, entityId);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(EntityType.Member, 1, "Test User 1")]
        [InlineData(EntityType.Member, 2, "Test User 2")]
        [InlineData(EntityType.Property, 1, "Test Property 1")]
        [InlineData(EntityType.Property, 2, "Test Property 2")]
        [InlineData(EntityType.Furniture, 1, "Test Furniture 1")]
        [InlineData(EntityType.Furniture, 2, "Test Furniture 2")]
        [InlineData(EntityType.Announcement, 1, "Test Message 1")]
        [InlineData(EntityType.Announcement, 2, "Test Message 2")]
        public async Task GetEntityNameAsync_WithValidEntity_ShouldReturnCorrectName(EntityType entityType, int entityId, string expectedName)
        {
            // Act
            var result = await _service.GetEntityNameAsync(entityType, entityId);

            // Assert
            Assert.Equal(expectedName, result);
        }

        [Theory]
        [InlineData(EntityType.Member, 999)]
        [InlineData(EntityType.Property, 999)]
        [InlineData(EntityType.Furniture, 999)]
        [InlineData(EntityType.Announcement, 999)]
        public async Task GetEntityNameAsync_WithNonExistentEntity_ShouldReturnNull(EntityType entityType, int entityId)
        {
            // Act
            var result = await _service.GetEntityNameAsync(entityType, entityId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetExistingEntityIdsAsync_WithValidIds_ShouldReturnExistingIds()
        {
            // Arrange
            var entityIds = new List<int> { 1, 2, 999 };

            // Act
            var result = await _service.GetExistingEntityIdsAsync(EntityType.Member, entityIds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result);
            Assert.Contains(2, result);
            Assert.DoesNotContain(999, result);
        }

        [Fact]
        public async Task GetExistingEntityIdsAsync_WithEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var entityIds = new List<int>();

            // Act
            var result = await _service.GetExistingEntityIdsAsync(EntityType.Member, entityIds);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExistingEntityIdsAsync_WithNullList_ShouldReturnEmptyList()
        {
            // Act
            var result = await _service.GetExistingEntityIdsAsync(EntityType.Member, null!);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(EntityType.Member, 1, 1, true)]
        [InlineData(EntityType.Member, 1, 2, false)]
        [InlineData(EntityType.Property, 1, 1, true)]
        [InlineData(EntityType.Property, 1, 2, false)]
        [InlineData(EntityType.Furniture, 1, 1, true)]
        [InlineData(EntityType.Furniture, 1, 2, true)] // Furniture 目前不檢查 memberId
        [InlineData(EntityType.Announcement, 1, 1, true)]
        [InlineData(EntityType.Announcement, 1, 2, true)] // Announcement 目前不檢查 memberId
        public async Task IsOwnedByMemberAsync_ShouldReturnExpectedResult(EntityType entityType, int entityId, int memberId, bool expected)
        {
            // Act
            var result = await _service.IsOwnedByMemberAsync(entityType, entityId, memberId);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(EntityType.Member, 0, 1, false)]
        [InlineData(EntityType.Member, 1, 0, false)]
        [InlineData(EntityType.Member, -1, 1, false)]
        [InlineData(EntityType.Member, 1, -1, false)]
        public async Task IsOwnedByMemberAsync_WithInvalidParameters_ShouldReturnFalse(EntityType entityType, int entityId, int memberId, bool expected)
        {
            // Act
            var result = await _service.IsOwnedByMemberAsync(entityType, entityId, memberId);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task IsOwnedByMemberAsync_WithNonExistentEntity_ShouldReturnFalse()
        {
            // Act
            var result = await _service.IsOwnedByMemberAsync(EntityType.Property, 999, 1);

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}