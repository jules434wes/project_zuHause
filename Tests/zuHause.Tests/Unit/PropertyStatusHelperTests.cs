using FluentAssertions;
using zuHause.Helpers;
using Xunit;

namespace zuHause.Tests.Unit;

/// <summary>
/// PropertyStatusHelper 純邏輯單元測試
/// 驗證 12 種房源狀態的三分組邏輯，無資料庫依賴
/// </summary>
public class PropertyStatusHelperTests
{
    /// <summary>
    /// 測試所有 12 種狀態都有正確的顯示名稱
    /// </summary>
    [Fact]
    public void GetDisplayName_AllStatuses_ReturnsValidChineseNames()
    {
        // Arrange: 完整的 12 種房源狀態
        var allStatuses = new[]
        {
            // === Available (可用房源) - 2種 ===
            "LISTED", "CONTRACT_ISSUED",
            
            // === Pending (等待刊登) - 4種 ===
            "PENDING", "PENDING_PAYMENT", "REJECT_REVISE", "IDLE",
            
            // === Unavailable (不可用) - 6種 ===
            "REJECTED", "BANNED", "PENDING_RENEWAL", "LEASE_RENEWING", "ALREADY_RENTED", "INVALID"
        };

        // Act & Assert: 驗證每個狀態都有中文顯示名稱
        foreach (var status in allStatuses)
        {
            var displayName = PropertyStatusHelper.GetDisplayName(status);
            displayName.Should().NotBeNullOrEmpty($"狀態 {status} 應該有中文顯示名稱");
            displayName.Should().NotBe(status, $"狀態 {status} 的顯示名稱不應該是英文代碼本身");
        }
    }

    /// <summary>
    /// 測試所有狀態都有有效的樣式類別
    /// </summary>
    [Fact]
    public void GetStatusStyle_AllStatuses_ReturnsValidBootstrapClasses()
    {
        // Arrange
        var allStatuses = new[]
        {
            "LISTED", "CONTRACT_ISSUED", "PENDING", "PENDING_PAYMENT", 
            "REJECT_REVISE", "IDLE", "REJECTED", "BANNED", 
            "PENDING_RENEWAL", "LEASE_RENEWING", "ALREADY_RENTED", "INVALID"
        };

        var validBootstrapClasses = new[] { "success", "primary", "warning", "danger", "secondary", "info", "dark" };

        // Act & Assert
        foreach (var status in allStatuses)
        {
            var statusStyle = PropertyStatusHelper.GetStatusStyle(status);
            statusStyle.Should().NotBeNullOrEmpty($"狀態 {status} 應該有樣式類別");
            validBootstrapClasses.Should().Contain(statusStyle, $"狀態 {status} 的樣式 '{statusStyle}' 應該是有效的 Bootstrap 類別");
        }
    }

    /// <summary>
    /// 測試可用房源 (Available) 分組邏輯
    /// </summary>
    [Fact]
    public void GetStatusGroup_AvailableStatuses_ReturnsAvailableGroup()
    {
        // Arrange: 可用房源狀態
        var availableStatuses = new[] { "CONTRACT_ISSUED", "LISTED" };

        // Act & Assert
        foreach (var status in availableStatuses)
        {
            var group = PropertyStatusHelper.GetStatusGroup(status);
            group.Should().Be(PropertyStatusGroup.Available, 
                $"狀態 {status} 應該歸類為可用房源 (Available)");
        }
    }

    /// <summary>
    /// 測試等待刊登房源 (Pending) 分組邏輯
    /// </summary>
    [Fact]
    public void GetStatusGroup_PendingStatuses_ReturnsPendingGroup()
    {
        // Arrange: 等待刊登房源狀態
        var pendingStatuses = new[] { "PENDING", "PENDING_PAYMENT", "REJECT_REVISE", "IDLE" };

        // Act & Assert
        foreach (var status in pendingStatuses)
        {
            var group = PropertyStatusHelper.GetStatusGroup(status);
            group.Should().Be(PropertyStatusGroup.Pending, 
                $"狀態 {status} 應該歸類為等待刊登房源 (Pending)");
        }
    }

    /// <summary>
    /// 測試不可用房源 (Unavailable) 分組邏輯
    /// </summary>
    [Fact]
    public void GetStatusGroup_UnavailableStatuses_ReturnsUnavailableGroup()
    {
        // Arrange: 不可用房源狀態
        var unavailableStatuses = new[] { "REJECTED", "BANNED", "PENDING_RENEWAL", "LEASE_RENEWING", "ALREADY_RENTED", "INVALID" };

        // Act & Assert
        foreach (var status in unavailableStatuses)
        {
            var group = PropertyStatusHelper.GetStatusGroup(status);
            group.Should().Be(PropertyStatusGroup.Unavailable, 
                $"狀態 {status} 應該歸類為不可用房源 (Unavailable)");
        }
    }

    /// <summary>
    /// 測試三分組完整性：確保所有 12 種狀態都被正確分組
    /// </summary>
    [Fact]
    public void GetStatusGroup_AllTwelveStatuses_AreProperlyGrouped()
    {
        // Arrange: 按預期分組組織所有狀態
        var statusGroups = new Dictionary<PropertyStatusGroup, string[]>
        {
            [PropertyStatusGroup.Available] = new[] { "CONTRACT_ISSUED", "LISTED" },
            [PropertyStatusGroup.Pending] = new[] { "PENDING", "PENDING_PAYMENT", "REJECT_REVISE", "IDLE" },
            [PropertyStatusGroup.Unavailable] = new[] { "REJECTED", "BANNED", "PENDING_RENEWAL", "LEASE_RENEWING", "ALREADY_RENTED", "INVALID" }
        };

        // Act & Assert: 驗證分組數量正確
        statusGroups[PropertyStatusGroup.Available].Should().HaveCount(2, "可用房源應該有 2 種狀態");
        statusGroups[PropertyStatusGroup.Pending].Should().HaveCount(4, "等待刊登房源應該有 4 種狀態");
        statusGroups[PropertyStatusGroup.Unavailable].Should().HaveCount(6, "不可用房源應該有 6 種狀態");

        // 驗證總數為 12
        var totalStatuses = statusGroups.Values.SelectMany(s => s).Count();
        totalStatuses.Should().Be(12, "總共應該有 12 種房源狀態");

        // 驗證每個狀態都被正確分組
        foreach (var (expectedGroup, statuses) in statusGroups)
        {
            foreach (var status in statuses)
            {
                var actualGroup = PropertyStatusHelper.GetStatusGroup(status);
                actualGroup.Should().Be(expectedGroup, 
                    $"狀態 {status} 應該歸類為 {expectedGroup}");
            }
        }
    }

    /// <summary>
    /// 測試條件式操作：可編輯權限
    /// </summary>
    [Fact]
    public void IsEditable_StatusBasedPermissions_ReturnsCorrectPermissions()
    {
        // Arrange & Act & Assert: 可編輯狀態
        PropertyStatusHelper.IsEditable("IDLE").Should().BeTrue("IDLE 狀態應該可編輯");
        PropertyStatusHelper.IsEditable("LISTED").Should().BeTrue("LISTED 狀態應該可編輯");
        
        // 不可編輯狀態
        PropertyStatusHelper.IsEditable("PENDING").Should().BeFalse("PENDING 狀態不應該可編輯");
        PropertyStatusHelper.IsEditable("CONTRACT_ISSUED").Should().BeFalse("CONTRACT_ISSUED 狀態不應該可編輯");
        PropertyStatusHelper.IsEditable("REJECTED").Should().BeFalse("REJECTED 狀態不應該可編輯");
    }

    /// <summary>
    /// 測試條件式操作：可下架權限
    /// </summary>
    [Fact]
    public void IsTakeDownable_StatusBasedPermissions_ReturnsCorrectPermissions()
    {
        // Arrange & Act & Assert: 可下架狀態
        PropertyStatusHelper.IsTakeDownable("LISTED").Should().BeTrue("LISTED 狀態應該可下架");
        
        // 不可下架狀態
        PropertyStatusHelper.IsTakeDownable("PENDING").Should().BeFalse("PENDING 狀態不應該可下架");
        PropertyStatusHelper.IsTakeDownable("IDLE").Should().BeFalse("IDLE 狀態不應該可下架");
        PropertyStatusHelper.IsTakeDownable("CONTRACT_ISSUED").Should().BeFalse("CONTRACT_ISSUED 狀態不應該可下架");
    }

    /// <summary>
    /// 測試條件式操作：需要補充資料
    /// </summary>
    [Fact]
    public void RequiresDataSupplement_StatusBasedRequirements_ReturnsCorrectRequirements()
    {
        // Arrange & Act & Assert: 需要補充資料
        PropertyStatusHelper.RequiresDataSupplement("REJECT_REVISE").Should().BeTrue("REJECT_REVISE 狀態需要補充資料");
        
        // 不需要補充資料
        PropertyStatusHelper.RequiresDataSupplement("PENDING").Should().BeFalse("PENDING 狀態不需要補充資料");
        PropertyStatusHelper.RequiresDataSupplement("LISTED").Should().BeFalse("LISTED 狀態不需要補充資料");
    }

    /// <summary>
    /// 測試條件式操作：需要聯絡客服
    /// </summary>
    [Fact]
    public void RequiresCustomerService_StatusBasedRequirements_ReturnsCorrectRequirements()
    {
        // Arrange & Act & Assert: 需要聯絡客服
        PropertyStatusHelper.RequiresCustomerService("BANNED").Should().BeTrue("BANNED 狀態需要聯絡客服");
        
        // 不需要聯絡客服
        PropertyStatusHelper.RequiresCustomerService("PENDING").Should().BeFalse("PENDING 狀態不需要聯絡客服");
        PropertyStatusHelper.RequiresCustomerService("LISTED").Should().BeFalse("LISTED 狀態不需要聯絡客服");
    }

    /// <summary>
    /// 測試條件式操作：需要用戶行動
    /// </summary>
    [Fact]
    public void RequiresAction_StatusBasedRequirements_ReturnsCorrectRequirements()
    {
        // Arrange & Act & Assert: 需要用戶行動
        PropertyStatusHelper.RequiresAction("REJECT_REVISE").Should().BeTrue("REJECT_REVISE 需要用戶行動");
        PropertyStatusHelper.RequiresAction("PENDING_PAYMENT").Should().BeTrue("PENDING_PAYMENT 需要用戶行動");
        PropertyStatusHelper.RequiresAction("BANNED").Should().BeTrue("BANNED 需要用戶行動");
        
        // 不需要用戶行動
        PropertyStatusHelper.RequiresAction("LISTED").Should().BeFalse("LISTED 不需要用戶行動");
        PropertyStatusHelper.RequiresAction("PENDING").Should().BeFalse("PENDING 不需要用戶行動");
    }

    /// <summary>
    /// 測試未知狀態的處理
    /// </summary>
    [Fact]
    public void PropertyStatusHelper_UnknownStatus_HandlesGracefully()
    {
        // Arrange
        var unknownStatus = "UNKNOWN_STATUS";

        // Act & Assert: 確保未知狀態不會導致例外
        var displayName = PropertyStatusHelper.GetDisplayName(unknownStatus);
        var statusStyle = PropertyStatusHelper.GetStatusStyle(unknownStatus);
        var statusGroup = PropertyStatusHelper.GetStatusGroup(unknownStatus);

        // 未知狀態應該有預設處理
        displayName.Should().NotBeNull("未知狀態應該有預設顯示名稱");
        statusStyle.Should().NotBeNull("未知狀態應該有預設樣式");
        statusGroup.Should().BeOneOf(PropertyStatusGroup.Available, PropertyStatusGroup.Pending, PropertyStatusGroup.Unavailable);
    }
}