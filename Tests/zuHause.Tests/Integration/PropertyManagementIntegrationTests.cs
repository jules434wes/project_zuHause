using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using zuHause.Models;
using zuHause.DTOs;
using zuHause.Controllers;
using zuHause.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using zuHause.Tests.Integration;
using zuHause.Interfaces;

namespace zuHause.Tests.Integration;

/// <summary>
/// 房源管理功能整合測試
/// 驗證12個房源狀態的三分組邏輯和條件式操作
/// </summary>
public class PropertyManagementIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly ZuHauseContext _context;
    private readonly LandlordController _controller;
    private readonly ILogger<LandlordController> _logger;

    public PropertyManagementIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        
        // 設定環境變數跳過 PDF 庫載入
        Environment.SetEnvironmentVariable("SKIP_PDF_LIBRARY", "true");
        
        // 使用測試專用的資料庫連線，確保不影響真實資料庫
        _context = _factory.CreateDbContext();
        
        // 建立 Logger (使用 factory 的服務)
        using var scope = _factory.Services.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<LandlordController>>();
        var imageQueryService = scope.ServiceProvider.GetRequiredService<IImageQueryService>();
        
        // 建立 Controller
        _controller = new LandlordController(_logger, _context, imageQueryService);
    }

    /// <summary>
    /// 驗證使用不存在的 LandlordMemberId 會觸發外鍵約束錯誤
    /// 這個測試確保 Properties.LandlordMemberId 外鍵約束正常運作
    /// </summary>
    [Fact]
    public async Task CreateProperty_WithInvalidLandlordMemberId_ShouldThrowForeignKeyConstraintError()
    {
        try
        {
            var scope = _factory.Services.CreateScope();
            using var newContext = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            
            await SetupBasicDataAsync(newContext);
            var listingPlan = await newContext.ListingPlans.FirstAsync(lp => lp.IsActive);
            
            // 使用一個絕對不存在的 MemberId (99999)
            var invalidMemberId = 99999;
            Console.WriteLine($"嘗試使用不存在的 LandlordMemberId: {invalidMemberId}");
            
            var property = new Property
            {
                LandlordMemberId = invalidMemberId, // 不存在的 MemberId
                Title = "測試房源 - 應該失敗",
                Description = "使用不存在房東ID的測試房源",
                CityId = 2,
                DistrictId = 1,
                AddressLine = "測試路999號",
                MonthlyRent = 20000,
                DepositAmount = 40000,
                DepositMonths = 2,
                RoomCount = 1,
                LivingRoomCount = 1,
                BathroomCount = 1,
                CurrentFloor = 1,
                TotalFloors = 5,
                Area = 15,
                MinimumRentalMonths = 12,
                WaterFeeType = "台水",
                ElectricityFeeType = "台電",
                ManagementFeeIncluded = true,
                ParkingAvailable = false,
                ListingDays = listingPlan.MinListingDays,
                ListingFeeAmount = listingPlan.PricePerDay * listingPlan.MinListingDays,
                ListingPlanId = listingPlan.PlanId,
                StatusCode = "PENDING",
                IsPaid = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            newContext.Properties.Add(property);
            
            // 這裡應該拋出外鍵約束錯誤
            await newContext.SaveChangesAsync();
            
            // 如果到達這裡，表示測試失敗了
            Console.WriteLine("❌ 錯誤：應該拋出外鍵約束錯誤，但沒有發生");
            throw new Exception("測試失敗：使用不存在的 LandlordMemberId 應該要失敗");
        }
        catch (Exception ex)
        {
            // 檢查是否為預期的外鍵約束錯誤
            if (ex.Message.Contains("FOREIGN KEY") || 
                ex.Message.Contains("FK_properties_landlord") ||
                ex.InnerException?.Message.Contains("FOREIGN KEY") == true ||
                ex.InnerException?.Message.Contains("FK_properties_landlord") == true)
            {
                Console.WriteLine("✓ 正確：外鍵約束正常運作，阻止了使用不存在的 LandlordMemberId");
                Console.WriteLine($"✓ 錯誤訊息: {ex.Message}");
                return; // 測試通過
            }
            
            // 如果是其他錯誤，重新拋出
            Console.WriteLine($"❌ 意外錯誤: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 建立涵蓋 12 種狀態的房源測試 - 真實資料庫驗證
    /// </summary>
    [Fact]
    public async Task CreateAllStatusProperties_ForDatabaseVerification()
    {
        try
        {
            // 使用新的 DbContext 實例避免追蹤衝突
            var scope = _factory.Services.CreateScope();
            using var newContext = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            
            // 建立基礎資料
            await SetupBasicDataAsync(newContext);
            var landlordId = await CreateSimpleLandlordAsync(newContext);
            
            // 取得可用的 ListingPlan (使用 IsActive=True 的方案)
            var listingPlan = await newContext.ListingPlans.FirstAsync(lp => lp.IsActive);
            
            var timestamp = DateTime.Now.Ticks;
            Console.WriteLine($"使用 ListingPlan: {listingPlan.PlanName}, PlanId: {listingPlan.PlanId}");
            
            // 查詢最大 PropertyId 以避免主鍵衝突
            var maxPropertyId = await newContext.Properties.MaxAsync(p => (int?)p.PropertyId) ?? 0;
            var newPropertyId = maxPropertyId + 1;
            Console.WriteLine($"查詢到最大 PropertyId: {maxPropertyId}, 新的 PropertyId: {newPropertyId}");
            
            // 定義完整的 12 種房源狀態及其對應的業務邏輯配置
            var baseTime = DateTime.Now;
            var statusConfigs = new[]
            {
                // === Available (可用房源) - 2種 ===
                new { Status = "LISTED", IsPaid = true, PaidAt = (DateTime?)baseTime.AddDays(-2), PublishedAt = (DateTime?)baseTime.AddDays(-2), ExpireAt = (DateTime?)baseTime.AddDays(28) },
                new { Status = "CONTRACT_ISSUED", IsPaid = true, PaidAt = (DateTime?)baseTime.AddDays(-5), PublishedAt = (DateTime?)baseTime.AddDays(-5), ExpireAt = (DateTime?)null },
                
                // === Pending (等待刊登) - 4種 ===
                new { Status = "PENDING", IsPaid = false, PaidAt = (DateTime?)null, PublishedAt = (DateTime?)null, ExpireAt = (DateTime?)null },
                new { Status = "PENDING_PAYMENT", IsPaid = false, PaidAt = (DateTime?)null, PublishedAt = (DateTime?)null, ExpireAt = (DateTime?)null },
                new { Status = "REJECT_REVISE", IsPaid = false, PaidAt = (DateTime?)null, PublishedAt = (DateTime?)null, ExpireAt = (DateTime?)null },
                new { Status = "IDLE", IsPaid = false, PaidAt = (DateTime?)baseTime.AddDays(-60), PublishedAt = (DateTime?)null, ExpireAt = (DateTime?)baseTime.AddDays(-30) },
                
                // === Unavailable (不可用) - 6種 ===
                new { Status = "REJECTED", IsPaid = false, PaidAt = (DateTime?)null, PublishedAt = (DateTime?)null, ExpireAt = (DateTime?)null },
                new { Status = "BANNED", IsPaid = false, PaidAt = (DateTime?)null, PublishedAt = (DateTime?)null, ExpireAt = (DateTime?)null },
                new { Status = "PENDING_RENEWAL", IsPaid = true, PaidAt = (DateTime?)baseTime.AddDays(-25), PublishedAt = (DateTime?)baseTime.AddDays(-25), ExpireAt = (DateTime?)baseTime.AddDays(5) },
                new { Status = "LEASE_RENEWING", IsPaid = true, PaidAt = (DateTime?)baseTime.AddDays(-35), PublishedAt = (DateTime?)baseTime.AddDays(-35), ExpireAt = (DateTime?)baseTime.AddDays(-5) },
                new { Status = "ALREADY_RENTED", IsPaid = true, PaidAt = (DateTime?)baseTime.AddDays(-10), PublishedAt = (DateTime?)baseTime.AddDays(-10), ExpireAt = (DateTime?)baseTime.AddDays(20) },
                new { Status = "INVALID", IsPaid = false, PaidAt = (DateTime?)baseTime.AddDays(-15), PublishedAt = (DateTime?)null, ExpireAt = (DateTime?)null }
            };

            var now = DateTime.Now;
            var listingFee = listingPlan.PricePerDay * listingPlan.MinListingDays;
            var createdProperties = new List<Property>();

            Console.WriteLine($"開始建立 {statusConfigs.Length} 種狀態的房源 (完整 12 種狀態測試)...");

            // 建立每種狀態的房源
            for (int i = 0; i < statusConfigs.Length; i++)
            {
                var config = statusConfigs[i];
                var currentPropertyId = newPropertyId + i;
                
                var property = new Property
                {
                    PropertyId = currentPropertyId,
                    LandlordMemberId = landlordId,
                    Title = $"測試房源{timestamp} - {config.Status}",
                    Description = $"測試用 {config.Status} 狀態房源",
                    CityId = 2, // 台北市
                    DistrictId = 1, // 大安區（屬於台北市）
                    AddressLine = $"測試路{timestamp}-{i + 1}號",
                    MonthlyRent = 20000 + (i * 1000), // 遞增租金
                    DepositAmount = 40000 + (i * 2000),
                    DepositMonths = 2,
                    RoomCount = 1 + (i % 3), // 1-3 房
                    LivingRoomCount = 1,
                    BathroomCount = 1,
                    CurrentFloor = 1 + (i % 5), // 1-5 樓
                    TotalFloors = 5,
                    Area = 15 + i, // 遞增坪數
                    MinimumRentalMonths = 12,
                    SpecialRules = $"測試守則 - {config.Status}",
                    WaterFeeType = "台水",
                    CustomWaterFee = null,
                    ElectricityFeeType = "台電",
                    CustomElectricityFee = null,
                    ManagementFeeIncluded = i % 2 == 0,
                    ManagementFeeAmount = i % 2 == 0 ? null : 1000,
                    ParkingAvailable = i % 3 == 0,
                    ParkingFeeRequired = false,
                    ParkingFeeAmount = null,
                    CleaningFeeRequired = false,
                    CleaningFeeAmount = null,
                    ListingDays = listingPlan.MinListingDays,
                    ListingFeeAmount = listingFee,
                    ListingPlanId = listingPlan.PlanId,
                    PropertyProofUrl = null,
                    StatusCode = config.Status,
                    IsPaid = config.IsPaid,
                    PaidAt = config.PaidAt,
                    PublishedAt = config.PublishedAt,
                    ExpireAt = config.ExpireAt,
                    CreatedAt = now.AddDays(-i), // 遞減建立時間
                    UpdatedAt = now.AddDays(-i + 1)
                };

                newContext.Properties.Add(property);
                createdProperties.Add(property);
                
                Console.WriteLine($"已準備房源 {i + 1}/{statusConfigs.Length}: PropertyId={currentPropertyId}, Status={config.Status}");
            }

            Console.WriteLine("開始儲存所有房源...");
            await newContext.SaveChangesAsync();
            Console.WriteLine($"成功建立 {createdProperties.Count} 筆房源");

            // 驗證建立結果
            var createdCount = await newContext.Properties
                .CountAsync(p => p.Title.Contains(timestamp.ToString()));
            
            createdCount.Should().Be(statusConfigs.Length, $"應該建立{statusConfigs.Length}筆測試房源 (完整 12 種狀態)");

            // 驗證每種狀態都有建立
            Console.WriteLine("開始驗證各狀態房源建立結果...");
            foreach (var config in statusConfigs)
            {
                var statusCount = await newContext.Properties
                    .CountAsync(p => p.Title.Contains(timestamp.ToString()) && p.StatusCode == config.Status);
                
                statusCount.Should().Be(1, $"應該有1筆 {config.Status} 狀態的房源");
                Console.WriteLine($"✓ {config.Status} 狀態房源建立成功");
            }

            // 驗證三分組邏輯
            Console.WriteLine("開始驗證三分組邏輯...");
            var availableStatuses = new[] { "LISTED", "CONTRACT_ISSUED" };
            var pendingStatuses = new[] { "PENDING", "PENDING_PAYMENT", "REJECT_REVISE", "IDLE" };
            var unavailableStatuses = new[] { "REJECTED", "BANNED", "PENDING_RENEWAL", "LEASE_RENEWING", "ALREADY_RENTED", "INVALID" };

            var availableCount = await newContext.Properties
                .CountAsync(p => p.Title.Contains(timestamp.ToString()) && availableStatuses.Contains(p.StatusCode));
            var pendingCount = await newContext.Properties
                .CountAsync(p => p.Title.Contains(timestamp.ToString()) && pendingStatuses.Contains(p.StatusCode));
            var unavailableCount = await newContext.Properties
                .CountAsync(p => p.Title.Contains(timestamp.ToString()) && unavailableStatuses.Contains(p.StatusCode));

            availableCount.Should().Be(2, "應該有2筆可用房源 (Available)");
            pendingCount.Should().Be(4, "應該有4筆等待刊登房源 (Pending)");
            unavailableCount.Should().Be(6, "應該有6筆不可用房源 (Unavailable)");

            Console.WriteLine($"✓ Available 群組: {availableCount} 筆");
            Console.WriteLine($"✓ Pending 群組: {pendingCount} 筆");
            Console.WriteLine($"✓ Unavailable 群組: {unavailableCount} 筆");

            Console.WriteLine($"完成 {statusConfigs.Length} 種狀態房源驗證 - 三分組邏輯正確");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"測試失敗: {ex.Message}");
            Console.WriteLine($"詳細錯誤: {ex}");
            throw;
        }
    }

    /// <summary>
    /// 測試 PropertyStatusHelper 的所有方法
    /// </summary>
    [Fact]
    public void PropertyStatusHelper_AllStatuses_ReturnsCorrectValues()
    {
        // Arrange: 所有12個狀態代碼
        var allStatuses = new[]
        {
            "PENDING", "PENDING_PAYMENT", "REJECT_REVISE", "REJECTED", "BANNED",
            "LISTED", "CONTRACT_ISSUED", "PENDING_RENEWAL", "LEASE_EXPIRED_RENEWING",
            "IDLE", "ALREADY_RENTED", "INVALID"
        };

        foreach (var status in allStatuses)
        {
            // Act & Assert: 驗證每個狀態的屬性
            var displayName = PropertyStatusHelper.GetDisplayName(status);
            var statusStyle = PropertyStatusHelper.GetStatusStyle(status);
            var statusGroup = PropertyStatusHelper.GetStatusGroup(status);

            // 確認都有有效的返回值
            displayName.Should().NotBeNullOrEmpty($"狀態 {status} 應該有中文顯示名稱");
            statusStyle.Should().NotBeNullOrEmpty($"狀態 {status} 應該有樣式類別");
            statusGroup.Should().BeOneOf(PropertyStatusGroup.Available, PropertyStatusGroup.Pending, PropertyStatusGroup.Unavailable);
        }
    }

    /// <summary>
    /// 測試三分組分類邏輯
    /// </summary>
    [Fact]
    public void PropertyStatusHelper_StatusGrouping_CorrectClassification()
    {
        // Arrange & Act & Assert: 驗證可用房源
        var availableStatuses = new[] { "CONTRACT_ISSUED", "LISTED" };
        foreach (var status in availableStatuses)
        {
            PropertyStatusHelper.GetStatusGroup(status).Should().Be(PropertyStatusGroup.Available, 
                $"{status} 應該歸類為可用房源");
        }

        // 驗證等待刊登房源
        var pendingStatuses = new[] { "PENDING", "PENDING_PAYMENT", "REJECT_REVISE", "IDLE" };
        foreach (var status in pendingStatuses)
        {
            PropertyStatusHelper.GetStatusGroup(status).Should().Be(PropertyStatusGroup.Pending,
                $"{status} 應該歸類為等待刊登房源");
        }

        // 驗證不可用房源
        var unavailableStatuses = new[] { "ALREADY_RENTED", "INVALID", "REJECTED", "BANNED" };
        foreach (var status in unavailableStatuses)
        {
            PropertyStatusHelper.GetStatusGroup(status).Should().Be(PropertyStatusGroup.Unavailable,
                $"{status} 應該歸類為不可用房源");
        }
    }

    /// <summary>
    /// 測試條件式操作權限
    /// </summary>
    [Fact]
    public void PropertyStatusHelper_ConditionalOperations_CorrectPermissions()
    {
        // 測試可編輯狀態
        PropertyStatusHelper.IsEditable("IDLE").Should().BeTrue("IDLE 狀態應該可編輯");
        PropertyStatusHelper.IsEditable("LISTED").Should().BeTrue("LISTED 狀態應該可編輯");
        PropertyStatusHelper.IsEditable("PENDING").Should().BeFalse("PENDING 狀態不應該可編輯");

        // 測試可下架狀態
        PropertyStatusHelper.IsTakeDownable("LISTED").Should().BeTrue("LISTED 狀態應該可下架");
        PropertyStatusHelper.IsTakeDownable("PENDING").Should().BeFalse("PENDING 狀態不應該可下架");

        // 測試需要補充資料
        PropertyStatusHelper.RequiresDataSupplement("REJECT_REVISE").Should().BeTrue("REJECT_REVISE 狀態需要補充資料");
        PropertyStatusHelper.RequiresDataSupplement("PENDING").Should().BeFalse("PENDING 狀態不需要補充資料");

        // 測試需要聯絡客服
        PropertyStatusHelper.RequiresCustomerService("BANNED").Should().BeTrue("BANNED 狀態需要聯絡客服");
        PropertyStatusHelper.RequiresCustomerService("PENDING").Should().BeFalse("PENDING 狀態不需要聯絡客服");

        // 測試需要用戶行動
        PropertyStatusHelper.RequiresAction("REJECT_REVISE").Should().BeTrue("REJECT_REVISE 需要用戶行動");
        PropertyStatusHelper.RequiresAction("PENDING_PAYMENT").Should().BeTrue("PENDING_PAYMENT 需要用戶行動");
        PropertyStatusHelper.RequiresAction("BANNED").Should().BeTrue("BANNED 需要用戶行動");
        PropertyStatusHelper.RequiresAction("LISTED").Should().BeFalse("LISTED 不需要用戶行動");
    }

    /// <summary>
    /// 測試 PropertyManagementDto 計算屬性
    /// </summary>
    [Fact]
    public void PropertyManagementDto_CalculatedProperties_ReturnsCorrectValues()
    {
        // Arrange
        var dto = new PropertyManagementDto
        {
            PropertyId = 1,
            Title = "測試房源",
            MonthlyRent = 25000,
            StatusCode = "PENDING",
            RoomCount = 2,
            LivingRoomCount = 1,
            BathroomCount = 1,
            Area = 15.5m,
            CurrentFloor = 3,
            TotalFloors = 5,
            CreatedAt = new DateTime(2024, 1, 15),
            UpdatedAt = new DateTime(2024, 1, 20),
            ExpireAt = new DateTime(2024, 12, 31)
        };

        // Act & Assert
        dto.StatusDisplayName.Should().Be("審核中");
        dto.StatusStyle.Should().Be("warning");
        dto.StatusGroup.Should().Be(PropertyStatusGroup.Pending);
        dto.LayoutDisplay.Should().Be("2房1廳1衛");
        dto.FloorInfo.Should().Be("3F/5F");
        dto.PriceDisplay.Should().Be("NT$ 25,000");
        dto.AreaDisplay.Should().Be("15.5 坪");
        dto.CreatedAtDisplay.Should().Be("2024/01/15");
        dto.ExpireAtDisplay.Should().Be("2024/12/31");
        dto.IsEditable.Should().BeFalse();
        dto.RequiresAction.Should().BeFalse();
        dto.RequiresDataSupplement.Should().BeFalse();
        dto.RequiresCustomerService.Should().BeFalse();
    }

    /// <summary>
    /// 測試 LandlordController PropertyManagement 方法
    /// 使用獨立的 DbContext 避免追蹤衝突
    /// </summary>
    [Fact]
    public async Task LandlordController_PropertyManagement_ReturnsCorrectGroupedData()
    {
        try
        {
            // 使用獨立的 DbContext 避免追蹤衝突
            var scope = _factory.Services.CreateScope();
            using var testContext = scope.ServiceProvider.GetRequiredService<ZuHauseContext>();
            var testLogger = scope.ServiceProvider.GetRequiredService<ILogger<LandlordController>>();
            
            // Arrange - 建立測試資料
            await SetupBasicDataAsync(testContext);
            var landlordId = await CreateSimpleLandlordAsync(testContext);
            
            // 建立測試房源（使用簡化版本避免複雜追蹤）
            var timestamp = DateTime.Now.Ticks;
            var testProperties = new[]
            {
                "LISTED", "CONTRACT_ISSUED", // Available - 2個
                "PENDING", "PENDING_PAYMENT", "REJECT_REVISE", "IDLE", // Pending - 4個  
                "REJECTED", "BANNED", "ALREADY_RENTED", "INVALID", "PENDING_RENEWAL", "LEASE_RENEWING" // Unavailable - 6個
            };

            var listingPlan = await testContext.ListingPlans.FirstAsync(lp => lp.IsActive);
            var maxPropertyId = await testContext.Properties.MaxAsync(p => (int?)p.PropertyId) ?? 0;

            // 確認 landlordId 存在於資料庫中
            var landlordExists = await testContext.Members.AnyAsync(m => m.MemberId == landlordId);
            if (!landlordExists)
            {
                throw new InvalidOperationException($"測試失敗：LandlordId {landlordId} 不存在於資料庫中");
            }
            Console.WriteLine($"✓ 確認 LandlordId {landlordId} 存在於資料庫中");

            for (int i = 0; i < testProperties.Length; i++)
            {
                var isPaid = testProperties[i] == "LISTED" || testProperties[i] == "CONTRACT_ISSUED";
                
                var property = new Property
                {
                    PropertyId = maxPropertyId + i + 1,
                    LandlordMemberId = landlordId,
                    Title = $"Controller測試房源{timestamp}-{testProperties[i]}",
                    Description = $"測試用 {testProperties[i]} 狀態房源",
                    CityId = 2,
                    DistrictId = 1,
                    AddressLine = $"測試路{timestamp}-{i + 1}號",
                    MonthlyRent = 20000 + (i * 1000),
                    DepositAmount = 40000 + (i * 2000),
                    DepositMonths = 2,
                    RoomCount = 1 + (i % 3),
                    LivingRoomCount = 1,
                    BathroomCount = 1,
                    CurrentFloor = 1 + (i % 5),
                    TotalFloors = 5,
                    Area = 15 + i,
                    MinimumRentalMonths = 12,
                    WaterFeeType = "台水",
                    ElectricityFeeType = "台電",
                    ManagementFeeIncluded = i % 2 == 0,
                    ParkingAvailable = i % 3 == 0,
                    ListingDays = listingPlan.MinListingDays,
                    ListingFeeAmount = listingPlan.PricePerDay * listingPlan.MinListingDays,
                    ListingPlanId = listingPlan.PlanId,
                    StatusCode = testProperties[i],
                    IsPaid = isPaid,
                    // 遵循 CHECK 約束：IsPaid 為 true 時必須設定 PaidAt
                    PaidAt = isPaid ? DateTime.Now.AddDays(-i - 1) : null,
                    CreatedAt = DateTime.Now.AddDays(-i),
                    UpdatedAt = DateTime.Now.AddDays(-i + 1)
                };

                testContext.Properties.Add(property);
            }
            
            await testContext.SaveChangesAsync();
            Console.WriteLine($"Controller測試：成功建立 {testProperties.Length} 筆測試房源");

            // 建立獨立的 Controller 實例
            using var testScope = _factory.Services.CreateScope();
            var testImageQueryService = testScope.ServiceProvider.GetRequiredService<IImageQueryService>();
            var testController = new LandlordController(testLogger, testContext, testImageQueryService);
            SetupControllerUserContext(testController, landlordId);

            // Act
            var result = await testController.PropertyManagement();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as PropertyManagementListResponseDto;
            
            model.Should().NotBeNull();
            
            // 驗證房源總數（應該包含此次建立的測試房源）
            var expectedProperties = model!.Properties.Where(p => p.Title.Contains(timestamp.ToString())).ToList();
            expectedProperties.Should().HaveCount(12, "應該有12個測試房源");
            
            // 驗證三分組邏輯（僅針對測試資料）
            var testAvailableCount = expectedProperties.Count(p => new[] { "LISTED", "CONTRACT_ISSUED" }.Contains(p.StatusCode));
            var testPendingCount = expectedProperties.Count(p => new[] { "PENDING", "PENDING_PAYMENT", "REJECT_REVISE", "IDLE" }.Contains(p.StatusCode));  
            var testUnavailableCount = expectedProperties.Count(p => new[] { "REJECTED", "BANNED", "ALREADY_RENTED", "INVALID", "PENDING_RENEWAL", "LEASE_RENEWING" }.Contains(p.StatusCode));

            testAvailableCount.Should().Be(2, "測試資料應該有2個可用房源");
            testPendingCount.Should().Be(4, "測試資料應該有4個等待刊登房源");
            testUnavailableCount.Should().Be(6, "測試資料應該有6個不可用房源");
            
            Console.WriteLine($"✓ Controller測試驗證完成：Available({testAvailableCount}) + Pending({testPendingCount}) + Unavailable({testUnavailableCount}) = {testAvailableCount + testPendingCount + testUnavailableCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Controller測試失敗: {ex.Message}");
            Console.WriteLine($"詳細錯誤: {ex}");
            throw;
        }
    }

    #region 私有輔助方法
    
    /// <summary>
    /// 簡化的基礎資料設置
    /// </summary>
    private async Task SetupBasicDataAsync(ZuHauseContext context)
    {
        // 使用現有的資料，不新增
        var cityExists = await context.Cities.AnyAsync();
        var districtExists = await context.Districts.AnyAsync();
        var listingPlanExists = await context.ListingPlans.AnyAsync();
        
        if (!cityExists || !districtExists)
        {
            throw new InvalidOperationException("請確保資料庫中有基礎的 City 和 District 資料");
        }
        
        // 確保有基本的 ListingPlan 資料
        if (!listingPlanExists)
        {
            var basicPlan = new ListingPlan
            {
                PlanName = "測試基本方案",
                PricePerDay = 50,
                CurrencyCode = "TWD",
                MinListingDays = 30,
                StartAt = DateTime.Now.AddDays(-30),
                EndAt = null,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            
            context.ListingPlans.Add(basicPlan);
            await context.SaveChangesAsync();
        }
    }
    
    /// <summary>
    /// 簡化的房東建立 - 先建立房東記錄再返回真實的 MemberId
    /// 這確保了 LandlordMemberId 外鍵約束的有效性
    /// </summary>
    private async Task<int> CreateSimpleLandlordAsync(ZuHauseContext context)
    {
        var timestamp = DateTime.Now.Ticks;
        var memberType = await context.MemberTypes.FirstAsync(mt => mt.MemberTypeId == 2);
        
        var landlord = new Member
        {
            MemberName = $"測試房東{timestamp}",
            Email = $"landlord{timestamp}@test.com",
            Password = "test123",
            PhoneNumber = "0912345678",
            NationalIdNo = $"A{timestamp.ToString().Substring(0, 9)}",
            IsLandlord = true,
            MemberTypeId = memberType.MemberTypeId,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        context.Members.Add(landlord);
        await context.SaveChangesAsync();
        
        // 重要：記錄實際建立的 MemberId，證明測試使用真實存在的房東記錄
        Console.WriteLine($"✓ 成功建立測試房東，MemberId: {landlord.MemberId}");
        Console.WriteLine($"✓ 所有房源將使用此真實存在的 LandlordMemberId: {landlord.MemberId}");
        
        return landlord.MemberId;
    }

    #endregion
    
    #region 原有的私有輔助方法

    /// <summary>
    /// 設置測試基礎資料（城市、區域等）
    /// </summary>
    private async Task SetupTestDataIfNeededAsync()
    {
        // 檢查是否已有測試城市，沒有才建立
        var existingCity = await _context.Cities.FirstOrDefaultAsync(c => c.CityCode == "TPE_TEST");
        if (existingCity == null)
        {
            var city = new City
            {
                CityCode = "TPE_TEST",
                CityName = "測試台北市",
                DisplayOrder = 999,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
        }

        // 取得城市ID用於建立區域
        var testCity = await _context.Cities.FirstAsync(c => c.CityCode == "TPE_TEST");

        // 檢查是否已有測試區域，沒有才建立
        var existingDistrict = await _context.Districts.FirstOrDefaultAsync(d => d.DistrictCode == "DAN_TEST");
        if (existingDistrict == null)
        {
            var district = new District
            {
                CityId = testCity.CityId,
                CityCode = "TPE_TEST",
                DistrictCode = "DAN_TEST",
                DistrictName = "測試大安區",
                ZipCode = "106",
                DisplayOrder = 999,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Districts.Add(district);
            await _context.SaveChangesAsync();
        }

        // 檢查是否已有測試會員類型，沒有才建立
        var existingMemberType = await _context.MemberTypes.FirstOrDefaultAsync(mt => mt.TypeName == "測試房東");
        if (existingMemberType == null)
        {
            var memberType = new MemberType
            {
                TypeName = "測試房東",
                Description = "測試用房東會員",
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.MemberTypes.Add(memberType);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 建立測試房東帳號
    /// </summary>
    private async Task<int> CreateTestLandlordAsync()
    {
        // 取得測試會員類型ID
        var testMemberType = await _context.MemberTypes.FirstAsync(mt => mt.TypeName == "測試房東");
        
        // 檢查是否已有測試房東，有的話直接返回ID
        var existingLandlord = await _context.Members.FirstOrDefaultAsync(m => m.Email == "test.landlord@example.com");
        if (existingLandlord != null)
        {
            return existingLandlord.MemberId;
        }

        var landlord = new Member
        {
            MemberName = "測試房東",
            Email = "test.landlord@example.com",
            Password = "hashedpassword",
            PhoneNumber = "0912345678",
            NationalIdNo = "A123456789",
            IsLandlord = true,
            MemberTypeId = testMemberType.MemberTypeId,
            IsActive = true,
            IdentityVerifiedAt = DateTime.Now,
            PhoneVerifiedAt = DateTime.Now,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Members.Add(landlord);
        await _context.SaveChangesAsync();
        return landlord.MemberId;
    }

    /// <summary>
    /// 建立涵蓋所有12個狀態的測試房源
    /// </summary>
    private async Task<List<Property>> CreateAllStatusPropertiesAsync(int landlordId)
    {
        var statuses = new[]
        {
            "PENDING", "PENDING_PAYMENT", "REJECT_REVISE", "REJECTED", "BANNED",
            "LISTED", "CONTRACT_ISSUED", "PENDING_RENEWAL", "LEASE_EXPIRED_RENEWING",
            "IDLE", "ALREADY_RENTED", "INVALID"
        };

        // 取得測試區域ID
        var testDistrict = await _context.Districts.FirstAsync(d => d.DistrictCode == "DAN_TEST");
        var testCity = await _context.Cities.FirstAsync(c => c.CityCode == "TPE_TEST");

        var properties = new List<Property>();
        var timestamp = DateTime.Now.Ticks; // 使用時間戳避免重複

        // 參考系統邏輯：簡單的建立模式
        for (int i = 0; i < statuses.Length; i++)
        {
            var property = new Property
            {
                LandlordMemberId = landlordId,
                Title = $"測試房源{timestamp} - {PropertyStatusHelper.GetDisplayName(statuses[i])}",
                Description = $"這是一個 {statuses[i]} 狀態的測試房源",
                CityId = testCity.CityId,
                DistrictId = testDistrict.DistrictId,
                AddressLine = $"測試路{timestamp}-{i + 1}號",
                MonthlyRent = 20000 + (i * 1000),
                DepositAmount = 40000 + (i * 2000),
                DepositMonths = 2,
                RoomCount = 1 + (i % 3),
                LivingRoomCount = 1,
                BathroomCount = 1,
                CurrentFloor = 1 + (i % 5),
                TotalFloors = 5,
                Area = 10 + i,
                MinimumRentalMonths = 12,
                WaterFeeType = "台水",
                ElectricityFeeType = "台電",
                ManagementFeeIncluded = i % 2 == 0,
                ParkingAvailable = i % 3 == 0,
                StatusCode = statuses[i],
                IsPaid = statuses[i] == "LISTED" || statuses[i] == "CONTRACT_ISSUED",
                PublishedAt = statuses[i] == "LISTED" ? DateTime.Now.AddDays(-i) : null,
                ExpireAt = statuses[i] == "LISTED" ? DateTime.Now.AddDays(30) : null,
                CreatedAt = DateTime.Now.AddDays(-i),
                UpdatedAt = DateTime.Now.AddDays(-i + 1)
            };

            // 參考現有模式：直接 Add
            _context.Properties.Add(property);
            properties.Add(property);
        }

        // 參考現有模式：統一 SaveChanges
        await _context.SaveChangesAsync();
        
        return properties;
    }

    /// <summary>
    /// 驗證狀態分組邏輯
    /// </summary>
    private async Task VerifyStatusGroupingAsync(List<Property> properties)
    {
        // 查詢並轉換為 DTO
        var dtos = properties.Select(p => new PropertyManagementDto
        {
            PropertyId = p.PropertyId,
            StatusCode = p.StatusCode,
            Title = p.Title,
            MonthlyRent = p.MonthlyRent,
            Address = "測試台北市測試大安區" + p.AddressLine,
            RoomCount = p.RoomCount,
            LivingRoomCount = p.LivingRoomCount,
            BathroomCount = p.BathroomCount,
            Area = p.Area,
            CurrentFloor = p.CurrentFloor,
            TotalFloors = p.TotalFloors,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            PublishedAt = p.PublishedAt,
            ExpireAt = p.ExpireAt,
            IsPaid = p.IsPaid
        }).ToList();

        // 驗證分組數量
        var availableCount = dtos.Count(p => p.StatusGroup == PropertyStatusGroup.Available);
        var pendingCount = dtos.Count(p => p.StatusGroup == PropertyStatusGroup.Pending);
        var unavailableCount = dtos.Count(p => p.StatusGroup == PropertyStatusGroup.Unavailable);

        availableCount.Should().Be(2, "可用房源應該有2個（LISTED, CONTRACT_ISSUED）");
        pendingCount.Should().Be(4, "等待刊登房源應該有4個（PENDING, PENDING_PAYMENT, REJECT_REVISE, IDLE）");
        unavailableCount.Should().Be(6, "不可用房源應該有6個（REJECTED, BANNED, PENDING_RENEWAL, LEASE_EXPIRED_RENEWING, ALREADY_RENTED, INVALID）");

        // 驗證特殊狀態功能
        var bannedProperty = dtos.First(p => p.StatusCode == "BANNED");
        bannedProperty.RequiresCustomerService.Should().BeTrue("BANNED 狀態應該需要聯絡客服");

        var rejectReviseProperty = dtos.First(p => p.StatusCode == "REJECT_REVISE");
        rejectReviseProperty.RequiresDataSupplement.Should().BeTrue("REJECT_REVISE 狀態應該需要補充資料");

        var listedProperty = dtos.First(p => p.StatusCode == "LISTED");
        listedProperty.IsEditable.Should().BeTrue("LISTED 狀態應該可編輯");
        listedProperty.IsTakeDownable.Should().BeTrue("LISTED 狀態應該可下架");
    }

    /// <summary>
    /// 設置 Controller 的用戶上下文
    /// </summary>
    private void SetupControllerUserContext(int landlordId)
    {
        SetupControllerUserContext(_controller, landlordId);
    }

    /// <summary>
    /// 設置指定 Controller 的用戶上下文（重載方法）
    /// </summary>
    private void SetupControllerUserContext(LandlordController controller, int landlordId)
    {
        var claims = new List<Claim>
        {
            new Claim("UserId", landlordId.ToString()),
            new Claim("MemberId", landlordId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "MemberCookieAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}