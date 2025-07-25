# admin_userslist.cshtml API 文件與資料庫串接機制

## 概述

`admin_userslist.cshtml` 是 zuHause 後台管理系統的會員管理頁面，採用 ASP.NET Core MVC 架構，實現三分頁會員管理介面，支援進階篩選、排序、分頁和批量操作功能。

## 資料庫串接架構

### 1. MVC 架構層級

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   View Layer    │◄──►│ Controller Layer │◄──►│  Data Layer     │
│ admin_userslist │    │ AdminController  │    │ ZuHauseContext  │
│ Partial Views   │    │ ViewModels       │    │ Entity Models   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### 2. 資料庫實體對應

| 資料庫表格 | Entity 模型 | 功能說明 |
|------------|-------------|----------|
| `members` | `Member` | 會員基本資料 |
| `approvals` | `Approval` | 審核記錄 |
| `cities` | `City` | 城市資料 |
| `memberVerifications` | `MemberVerification` | 身分驗證記錄 |

## API 端點文件

### 基礎路由

#### GET /Admin/admin_usersList

**功能描述：** 載入會員管理頁面

**權限要求：** `AdminPermissions.MemberList`

**請求參數：** 無

**響應格式：** HTML 頁面

**Controller 實作：**
```csharp
[RequireAdminPermission(AdminPermissions.MemberList)]
public IActionResult admin_usersList()
{
    var viewModel = new AdminUserListViewModel(_context);
    return View(viewModel);
}
```

**資料庫查詢邏輯：**
```sql
-- 全部會員查詢 (簡化版)
SELECT m.memberID, m.memberName, m.email, m.phoneNumber,
       m.isLandlord, m.createdAt, m.lastLoginAt,
       c1.cityName AS residenceCityName,
       c2.cityName AS primaryRentalCityName,
       CASE WHEN a.approvalID IS NOT NULL THEN 'PENDING' ELSE 'VERIFIED' END AS verificationStatus
FROM members m
LEFT JOIN cities c1 ON m.residenceCityID = c1.cityID
LEFT JOIN cities c2 ON m.primaryRentalCityID = c2.cityID
LEFT JOIN approvals a ON m.memberID = a.applicantMemberID 
    AND a.moduleCode = 'IDENTITY' 
    AND a.statusCode = 'PENDING'
ORDER BY m.createdAt DESC
```

### 輔助 API 端點

#### GET /Admin/GetCities

**功能描述：** 取得城市列表供篩選器使用

**請求參數：** 無

**響應格式：** JSON

**響應範例：**
```json
[
  {"id": 1, "name": "台北市"},
  {"id": 2, "name": "新北市"},
  {"id": 3, "name": "桃園市"}
]
```

**資料庫查詢：**
```sql
SELECT cityID AS id, cityName AS name 
FROM cities 
WHERE isActive = 1 
ORDER BY cityID
```

## ViewModel 資料結構

### AdminUserListViewModel

```csharp
public class AdminUserListViewModel : BaseListViewModel<MemberData>
{
    // 繼承自 BaseListViewModel
    public List<MemberData> Items { get; set; }          // 全部會員
    public int TotalCount { get; set; }                  // 總數量
    public BulkOperationConfig BulkConfig { get; set; }  // 批量操作配置
    
    // 專屬屬性
    public List<MemberData> PendingVerificationUsers { get; set; }  // 待身分驗證會員
    public List<MemberData> PendingLandlordUsers { get; set; }      // 申請房東會員
}
```

### MemberData 資料模型

```csharp
public class MemberData
{
    // 基本資料
    public string MemberID { get; set; }              // 會員編號
    public string MemberName { get; set; }            // 姓名
    public string Email { get; set; }                 // 電子郵件
    public string PhoneNumber { get; set; }           // 手機號碼
    
    // 狀態欄位
    public string AccountStatus { get; set; }         // 帳戶狀態
    public string VerificationStatus { get; set; }    // 驗證狀態
    public bool IsLandlord { get; set; }              // 是否為房東
    
    // 進階篩選屬性
    public int Gender { get; set; }                   // 性別 (1:男, 2:女)
    public int ResidenceCityID { get; set; }          // 居住城市ID
    public string ResidenceCityName { get; set; }     // 居住城市名稱
    public int PrimaryRentalCityID { get; set; }      // 租屋偏好城市ID
    public string PrimaryRentalCityName { get; set; } // 租屋偏好城市名稱
    
    // 時間戳記
    public string RegistrationDate { get; set; }      // 註冊日期 (yyyy-MM-dd HH:mm:ss)
    public string LastLoginTime { get; set; }         // 最後登入時間
    public string ApplyTime { get; set; }             // 申請時間 (適用於待審核項目)
    
    // 身分驗證相關
    public bool HasIdUpload { get; set; }             // 是否已上傳身分證
    public string IdUploadStatus { get; set; }        // 身分證上傳狀態
}
```

## 前端 JavaScript API

### 搜尋與篩選功能

#### performSearch(suffix, tabName)

**功能描述：** 執行搜尋和篩選操作

**參數：**
- `suffix`: 分頁後綴 ("", "Pending", "Landlord")
- `tabName`: 分頁名稱

**篩選條件：**
```javascript
var filters = {
    searchText: string,          // 搜尋文字
    searchField: string,         // 搜尋欄位 (memberID|memberName|email|phoneNumber)
    verificationStatus: string,  // 驗證狀態 (verified|pending|rejected)
    accountStatus: string,       // 帳戶狀態 (active|inactive|banned)
    isLandlord: string,         // 是否房東 (true|false|"")
    gender: string,             // 性別 (1|2|"")
    residenceCityID: string,    // 居住城市ID
    primaryRentalCityID: string, // 租屋偏好城市ID
    startDate: string,          // 開始日期 (yyyy-MM-dd)
    endDate: string             // 結束日期 (yyyy-MM-dd)
};
```

### 排序功能

#### sortTable(table, sortField, direction)

**功能描述：** 表格排序

**參數：**
- `table`: 表格 DOM 元素
- `sortField`: 排序欄位
- `direction`: 排序方向 ("asc"|"desc")

**支援排序欄位：**
- `memberID` - 會員編號
- `memberName` - 姓名
- `email` - 電子郵件
- `registerDate` - 註冊日期
- `lastLogin` - 最後登入時間
- `verificationStatus` - 驗證狀態

### 分頁功能

#### updateTableDisplay(tableType, pageSize, currentPage)

**功能描述：** 更新表格顯示和分頁

**參數：**
- `tableType`: 表格類型 ("all"|"pending"|"landlord")
- `pageSize`: 每頁筆數 (10|25|50|100)
- `currentPage`: 當前頁碼

## 資料庫查詢詳細分析

### 核心查詢邏輯

#### 1. 載入全部會員

```csharp
private List<MemberData> LoadUsersFromDatabase(ZuHauseContext context, bool landlordsOnly = false)
{
    var query = context.Members.AsQueryable();
    
    if (landlordsOnly)
    {
        query = query.Where(m => m.IsLandlord);
    }
    
    var members = query
        .OrderByDescending(m => m.CreatedAt)
        .Select(m => new 
        {
            Member = m,
            HasPendingApproval = context.Approvals.Any(a => 
                a.ApplicantMemberId == m.MemberId && 
                a.ModuleCode == "IDENTITY" && 
                a.StatusCode == "PENDING"
            ),
            ResidenceCity = context.Cities.FirstOrDefault(c => c.CityId == m.ResidenceCityId),
            PrimaryRentalCity = context.Cities.FirstOrDefault(c => c.CityId == m.PrimaryRentalCityId)
        })
        .ToList();
        
    return MapToMemberData(members);
}
```

#### 2. 載入待身分驗證會員

```csharp
private List<MemberData> LoadPendingVerificationUsers(ZuHauseContext context)
{
    var pendingUsers = context.Members
        .Where(m => context.Approvals.Any(a => 
            a.ApplicantMemberId == m.MemberId && 
            a.ModuleCode == "IDENTITY" && 
            a.StatusCode == "PENDING"
        ))
        .OrderBy(m => m.CreatedAt)
        .ToList();
        
    return MapToMemberData(pendingUsers);
}
```

#### 3. 載入申請房東會員

```csharp
private List<MemberData> LoadPendingLandlordUsers(ZuHauseContext context)
{
    var landlordApplicants = context.Members
        .Where(m => context.Approvals.Any(a => 
            a.ApplicantMemberId == m.MemberId && 
            a.ModuleCode == "LANDLORD" && 
            a.StatusCode == "PENDING"
        ))
        .OrderBy(m => m.CreatedAt)
        .ToList();
        
    return MapToMemberData(landlordApplicants);
}
```

### 效能優化策略

#### 1. 查詢優化

- **投影查詢**: 使用 `Select` 只取需要的欄位
- **索引使用**: 在常用查詢欄位建立索引
- **避免 N+1**: 使用 `Include` 或投影避免多次查詢

```csharp
// 好的做法：一次性載入相關資料
var members = context.Members
    .Include(m => m.ResidenceCity)
    .Include(m => m.PrimaryRentalCity)
    .ToList();

// 避免的做法：會造成 N+1 查詢
var members = context.Members.ToList();
foreach(var member in members)
{
    var city = context.Cities.Find(member.ResidenceCityId); // N+1 問題
}
```

#### 2. 分頁實作

前端 JavaScript 實現客戶端分頁：

```javascript
function updatePagination(container, currentPage, totalPages, pageSize, allRows, tableType) {
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = Math.min(startIndex + pageSize, allRows.length);
    
    // 隱藏所有行
    allRows.forEach(row => row.style.display = 'none');
    
    // 顯示當前頁
    for (let i = startIndex; i < endIndex; i++) {
        if (allRows[i]) {
            allRows[i].style.display = '';
        }
    }
    
    // 更新分頁控制項
    updatePaginationControls(container, currentPage, totalPages);
}
```

## 安全性考量

### 1. 權限控制

```csharp
[RequireAdminPermission(AdminPermissions.MemberList)]
public IActionResult admin_usersList()
{
    // 方法實作
}
```

### 2. 資料驗證

- **前端驗證**: JavaScript 即時驗證使用者輸入
- **後端驗證**: Controller 和 Model 層驗證
- **SQL 注入防護**: 使用 Entity Framework 參數化查詢

### 3. Modal 安全性

```html
<!-- 防止意外關閉的安全設定 -->
<div class="modal fade" 
     data-bs-backdrop="static" 
     data-bs-keyboard="false">
```

## 效能監控建議

### 1. 資料庫查詢監控

```sql
-- 監控慢查詢
SELECT TOP 10 
    total_elapsed_time/execution_count AS avg_elapsed_time,
    text 
FROM sys.dm_exec_query_stats 
CROSS APPLY sys.dm_exec_sql_text(sql_handle)
WHERE text LIKE '%members%'
ORDER BY avg_elapsed_time DESC
```

### 2. 前端效能監控

```javascript
// 監控頁面載入時間
window.addEventListener('load', function() {
    const loadTime = performance.now();
    console.log(`頁面載入時間: ${loadTime}ms`);
});

// 監控搜尋操作時間
function performSearch(suffix, tabName) {
    const startTime = performance.now();
    
    // 搜尋邏輯...
    
    const endTime = performance.now();
    console.log(`搜尋耗時: ${endTime - startTime}ms`);
}
```

## 擴展功能建議

### 1. 資料匯出功能

```csharp
[HttpGet]
public IActionResult ExportMembers(string format = "excel")
{
    var members = LoadUsersFromDatabase(_context);
    
    switch(format.ToLower())
    {
        case "excel":
            return ExportToExcel(members);
        case "csv":
            return ExportToCsv(members);
        default:
            return BadRequest("不支援的匯出格式");
    }
}
```

### 2. 即時通知功能

```javascript
// 使用 SignalR 實現即時更新
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/memberHub")
    .build();

connection.on("MemberUpdated", function(memberData) {
    updateMemberRow(memberData);
});
```

### 3. 進階分析功能

```sql
-- 會員成長趨勢分析
SELECT 
    YEAR(createdAt) as year,
    MONTH(createdAt) as month,
    COUNT(*) as new_members,
    SUM(COUNT(*)) OVER (ORDER BY YEAR(createdAt), MONTH(createdAt)) as total_members
FROM members
GROUP BY YEAR(createdAt), MONTH(createdAt)
ORDER BY year, month
```

## 維護注意事項

### 1. 定期資料庫維護

- 索引重建和更新統計資料
- 查詢計劃快取清理
- 死鎖監控和優化

### 2. 前端資源優化

- CSS/JS 檔案壓縮
- 圖片優化
- CDN 使用策略

### 3. 監控指標

- 頁面載入時間
- 搜尋響應時間  
- 資料庫查詢執行時間
- 記憶體使用率

此文件提供了 `admin_userslist.cshtml` 的完整技術架構說明，包含資料庫串接機制、API 設計和效能考量，可作為開發團隊的技術參考文件。