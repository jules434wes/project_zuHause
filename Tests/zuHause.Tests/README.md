# zuHause 測試架構說明

## 🎯 測試分層策略

### 1. 單元測試 (Unit Tests)
- **資料庫**: InMemory Database
- **特點**: 快速、隔離、專注業務邏輯
- **適用場景**: 服務層邏輯、驗證規則、算法測試
- **命名規則**: `*ServiceTests.cs`

### 2. 整合測試 (Integration Tests)
- **資料庫**: SQL Server (透過 Testcontainers 或 LocalDB)
- **特點**: 真實資料庫行為、交易支援、約束驗證
- **適用場景**: 完整業務流程、資料庫操作、檔案處理
- **命名規則**: `*IntegrationTests.cs`

### 3. 並發測試 (Concurrency Tests)
- **資料庫**: SQL Server 
- **特點**: 測試競爭條件、鎖定、死鎖處理
- **適用場景**: DisplayOrder 管理、同時上傳、批次操作
- **命名規則**: `*ConcurrencyTests.cs`

## 🏗️ 測試基類架構

### InMemoryTestBase
```csharp
// 適用於單元測試
public abstract class InMemoryTestBase : IDisposable
{
    protected ZuHauseContext Context { get; }
    
    // 快速、隔離的測試環境
    // 忽略交易警告
    // 專注於業務邏輯驗證
}
```

### SqlServerTestBase
```csharp
// 適用於整合測試
public abstract class SqlServerTestBase : IAsyncLifetime
{
    protected ZuHauseContext Context { get; }
    
    // 真實資料庫行為
    // 交易支援
    // 並發測試輔助方法
}
```

## 📋 測試資料管理

### 單元測試資料
- 使用 `SetupTestData()` 方法
- 最小化資料集
- 專注於測試場景

### 整合測試資料
- 使用 `SeedTestDataAsync()` 方法
- 完整的關聯資料
- 真實的業務場景

## 🔧 環境設定

### 開發環境
```bash
# 單元測試（無需額外設定）
dotnet test --filter "Category=Unit"

# 整合測試（需要 SQL Server 或 LocalDB）
dotnet test --filter "Category=Integration"
```

### CI/CD 環境
```yaml
# 使用 Docker 容器
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=TestPassword123!
      - ACCEPT_EULA=Y
```

## 📊 測試分類標籤

### 使用 xUnit Traits
```csharp
[Fact]
[Trait("Category", "Unit")]
public void UnitTest_ShouldPass() { }

[Fact]
[Trait("Category", "Integration")]
public void IntegrationTest_ShouldPass() { }

[Fact]
[Trait("Category", "Concurrency")]
public void ConcurrencyTest_ShouldPass() { }
```

### 執行特定分類
```bash
# 只執行單元測試
dotnet test --filter "Category=Unit"

# 只執行整合測試
dotnet test --filter "Category=Integration"

# 排除並發測試
dotnet test --filter "Category!=Concurrency"
```

## 🚀 效能考量

### 測試速度比較
- **單元測試**: ~1-10ms 每個測試
- **整合測試**: ~100-500ms 每個測試
- **並發測試**: ~500-2000ms 每個測試

### 最佳實踐
1. **大量快速的單元測試** - 覆蓋業務邏輯
2. **適量的整合測試** - 驗證關鍵流程
3. **少量的並發測試** - 測試競爭條件

## 📁 資料夾結構

```
Tests/zuHause.Tests/
├── Services/
│   ├── ImageValidationServiceTests.cs        # 單元測試
│   ├── DisplayOrderServiceTests.cs           # 單元測試
│   ├── ImageUploadServiceIntegrationTests.cs # 整合測試
│   └── DisplayOrderConcurrencyTests.cs       # 並發測試
├── TestInfrastructure/
│   ├── InMemoryTestBase.cs
│   ├── SqlServerTestBase.cs
│   └── TestDataBuilder.cs
└── README.md
```

## 🔄 開發流程

### Task 開發階段
1. **先寫單元測試** - 快速驗證邏輯
2. **實作功能** - TDD 方式開發
3. **加入整合測試** - 驗證完整流程
4. **並發測試** - 如需要

### CI/CD 管道
1. **PR 階段**: 執行單元測試
2. **合併階段**: 執行整合測試
3. **部署前**: 執行全部測試

## 🎯 未來擴展

### Task 3: 圖片上傳服務
- 加入 Testcontainers.NET
- 檔案系統整合測試
- Azure Blob Storage 模擬

### Task 6: 完整整合測試
- 端到端測試
- 效能測試
- 壓力測試