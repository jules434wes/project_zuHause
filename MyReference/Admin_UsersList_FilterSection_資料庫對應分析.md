# Admin UsersList FilterSection 資料庫對應分析

## 概述

本文件詳細說明 `admin_usersList.cshtml` 中 FilterSection 的每個篩選條件選項與資料庫狀態的對應關係，基於會員驗證與審核模組的業務邏輯。

## 篩選條件與資料庫對應

### 1. 關鍵字搜尋 (Basic Search)

#### 搜尋欄位選項
```html
<select class="form-select" id="searchField">
    <option value="memberID">會員ID</option>
    <option value="memberName">姓名</option>
    <option value="email">電子郵件</option>
    <option value="phoneNumber">手機號碼</option>
    <option value="nationalIdNo">身分證字號</option>
</select>
```

#### 資料庫對應
| 選項 | 資料庫欄位 | 說明 |
|------|------------|------|
| `memberID` | `members.memberID` | 會員唯一識別碼 |
| `memberName` | `members.memberName` | 會員姓名 |
| `email` | `members.email` | 會員電子郵件地址 |
| `phoneNumber` | `members.phoneNumber` | 會員手機號碼 |
| `nationalIdNo` | `members.nationalIdNo` | 身分證字號（通過身分驗證後才有值）|

### 2. 身分證驗證狀態 (Identity Verification Status)

#### 三個標籤頁的不同配置

##### 2.1 全部會員標籤頁
```csharp
ViewData["HasVerificationStatus"] = true;
ViewData["VerificationStatusOptions"] = "full";
```

選項與資料庫狀態對應：

| 選項 | 資料庫狀態判斷邏輯 | 業務含義 |
|------|-------------------|----------|
| `全部` | 不篩選 | 顯示所有會員 |
| `已驗證` | `members.identityVerifiedAt IS NOT NULL` | 身分證驗證已通過的會員 |
| `等待驗證` | `approvals.moduleCode='IDENTITY' AND approvals.statusCode='PENDING'` | 已提交身分證申請，等待管理員審核 |
| `尚未驗證` | `members.identityVerifiedAt IS NULL AND 無對應 PENDING 審核記錄` | 尚未提交身分證驗證申請 |

##### 2.2 等待身分證驗證標籤頁
```csharp
ViewData["HasVerificationStatus"] = false;
ViewData["HasIdUpload"] = false;
```
**無身分證驗證篩選** - 因為此標籤頁本身就是待驗證會員的子集

##### 2.3 申請成為房東標籤頁
```csharp
ViewData["HasVerificationStatus"] = true;
ViewData["VerificationStatusOptions"] = "limited";
```

選項與資料庫狀態對應（限制版本）：

| 選項 | 資料庫狀態判斷邏輯 | 業務含義 |
|------|-------------------|----------|
| `全部` | 不篩選 | 顯示所有房東申請者 |
| `已驗證` | `members.identityVerifiedAt IS NOT NULL` | 已完成身分驗證的房東申請者 |
| `等待驗證` | `approvals.moduleCode='IDENTITY' AND approvals.statusCode='PENDING'` | 同時申請身分驗證和房東資格的用戶 |

**注意**：`尚未驗證` 選項在房東申請標籤頁中被隱藏，因為房東申請通常需要先完成身分驗證。

### 3. 帳戶狀態 (Account Status)

#### 配置條件
```csharp
// 全部會員標籤頁
ViewData["HasAccountStatus"] = true;

// 等待身分證驗證標籤頁
ViewData["HasAccountStatus"] = false;

// 申請成為房東標籤頁  
ViewData["HasAccountStatus"] = false;
```

#### 選項與資料庫對應（僅在全部會員標籤頁顯示）

| 選項 | 資料庫狀態判斷邏輯 | 業務含義 |
|------|-------------------|----------|
| `全部` | 不篩選 | 顯示所有帳戶狀態的會員 |
| `啟用中` | `members.isActive = true` | 正常使用中的會員帳戶 |
| `停用` | `members.isActive = false` | 被管理員停用的會員帳戶 |

#### 帳戶停用的審核記錄
當會員帳戶被停用時，系統會在 `approvalItems` 表中記錄：
```sql
INSERT INTO approvalItems (
    approvalID, 
    actionType, 
    actionBy, 
    actionNote,
    snapshotJSON
) VALUES (
    對應的審核記錄ID,
    'FORCE_BANNED',
    管理員ID,
    '違規原因和停用理由',
    '停用時的會員狀態快照'
);
```

### 4. 進階篩選條件

#### 4.1 性別篩選
```html
<select class="form-select" id="gender">
    <option value="">全部</option>
    <option value="1">男性</option>
    <option value="2">女性</option>
    <option value="other">其他</option>
</select>
```

| 選項 | 資料庫對應 | 說明 |
|------|------------|------|
| `全部` | 不篩選 | 顯示所有性別 |
| `1` | `members.gender = 1` | 男性會員 |
| `2` | `members.gender = 2` | 女性會員 |
| `other` | `members.gender NOT IN (1, 2)` | 其他性別選項 |

#### 4.2 房東身分篩選（僅在全部會員標籤頁顯示）
```csharp
ViewData["HasLandlordFilter"] = true;  // 全部會員
ViewData["HasLandlordFilter"] = false; // 其他標籤頁
```

| 選項 | 資料庫對應 | 說明 |
|------|------------|------|
| `全部` | 不篩選 | 顯示所有會員類型 |
| `是` | `members.isLandlord = true AND members.memberTypeID = 2` | 已取得房東資格的會員 |
| `否` | `members.isLandlord = false AND members.memberTypeID = 1` | 一般會員 |

#### 4.3 地理位置篩選
- **居住縣市**: `members.residenceCityID` → `cities.cityName`
- **偏好租賃縣市**: `members.primaryRentalCityID` → `cities.cityName`

選項動態載入自 `cities` 表：
```sql
SELECT cityID, cityName 
FROM cities 
WHERE isActive = true 
ORDER BY cityID;
```

#### 4.4 日期範圍篩選

##### 註冊日期範圍
- **資料庫欄位**: `members.createdAt`
- **篩選邏輯**: `BETWEEN 開始日期 AND 結束日期`

##### 申請日期範圍（僅在特定標籤頁顯示）
```csharp
ViewData["HasApplyDate"] = false; // 全部會員
ViewData["HasApplyDate"] = true;  // 等待身分證驗證、申請成為房東
```

- **資料庫欄位**: `approvals.submittedAt`
- **篩選邏輯**: 
  ```sql
  SELECT * FROM approvals 
  WHERE moduleCode IN ('IDENTITY', 'LANDLORD') 
  AND submittedAt BETWEEN 開始日期 AND 結束日期
  ```

##### 最後登入日期範圍
- **資料庫欄位**: `members.lastLoginAt`
- **篩選邏輯**: `BETWEEN 開始日期 AND 結束日期`

## 複合查詢邏輯

### 會員驗證狀態的複合判斷

#### 已驗證 (verified)
```sql
members.identityVerifiedAt IS NOT NULL
```

#### 等待驗證 (pending)
```sql
EXISTS (
    SELECT 1 FROM approvals 
    WHERE applicantMemberID = members.memberID 
    AND moduleCode = 'IDENTITY' 
    AND statusCode = 'PENDING'
)
```

#### 尚未驗證 (unverified)
```sql
members.identityVerifiedAt IS NULL 
AND NOT EXISTS (
    SELECT 1 FROM approvals 
    WHERE applicantMemberID = members.memberID 
    AND moduleCode = 'IDENTITY' 
    AND statusCode = 'PENDING'
)
```

### 房東申請狀態的複合判斷

#### 已取得房東資格
```sql
members.isLandlord = true 
AND members.memberTypeID = 2
AND EXISTS (
    SELECT 1 FROM approvals 
    WHERE applicantMemberID = members.memberID 
    AND moduleCode = 'LANDLORD' 
    AND statusCode = 'APPROVED'
)
```

#### 等待房東審核
```sql
EXISTS (
    SELECT 1 FROM approvals 
    WHERE applicantMemberID = members.memberID 
    AND moduleCode = 'LANDLORD' 
    AND statusCode = 'PENDING'
)
```

## AdminUserListViewModel 資料載入邏輯

### LoadUsersFromDatabase() - 全部會員
```csharp
var members = context.Members
    .OrderByDescending(m => m.CreatedAt)
    .Select(m => new 
    {
        Member = m,
        HasPendingApproval = context.Approvals.Any(a => 
            a.ApplicantMemberId == m.MemberId && 
            a.ModuleCode == "IDENTITY" && 
            a.StatusCode == "PENDING"),
        ResidenceCity = context.Cities.FirstOrDefault(c => c.CityId == m.ResidenceCityId),
        PrimaryRentalCity = context.Cities.FirstOrDefault(c => c.CityId == m.PrimaryRentalCityId)
    })
```

### LoadPendingVerificationUsers() - 等待身分證驗證
```csharp
var pendingUsers = context.Members
    .Join(context.Approvals,
        m => m.MemberId,
        a => a.ApplicantMemberId,
        (m, a) => new { Member = m, Approval = a })
    .Where(x => x.Approval.ModuleCode == "IDENTITY" && 
                x.Approval.StatusCode == "PENDING")
```

### LoadPendingLandlordUsers() - 申請成為房東
```csharp
var pendingLandlordUsers = context.Members
    .Join(context.Approvals,
        m => m.MemberId,
        a => a.ApplicantMemberId,
        (m, a) => new { Member = m, Approval = a })
    .Where(x => x.Approval.ModuleCode == "LANDLORD" && 
                x.Approval.StatusCode == "PENDING")
```

## 結論

FilterSection 的設計充分考慮了會員驗證與審核模組的業務複雜性，透過：

1. **條件性顯示**：不同標籤頁顯示不同的篩選條件
2. **狀態複合判斷**：結合 `members` 和 `approvals` 表的資料
3. **階層式權限**：房東申請需要先完成身分驗證
4. **完整追蹤**：所有操作都有對應的審核記錄

這樣的設計確保了管理員可以精確篩選和管理不同狀態的會員，同時維護了資料的一致性和業務邏輯的完整性。