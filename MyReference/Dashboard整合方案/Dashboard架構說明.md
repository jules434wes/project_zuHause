# Dashboard 架構說明

## 架構概述

zuHause 儀表板採用動態標籤頁架構，支援基於角色的權限控制和模組化內容載入。系統使用 ASP.NET Core MVC 後端和 JavaScript 前端進行協作。

## 核心架構組件

### 1. 佈局系統 (`_DashboardLayout.cshtml`)

#### 佈局結構
- **固定側邊欄**: 寬度 260px，包含功能選單
- **主要內容區**: 動態標籤頁容器，左邊距 260px
- **響應式設計**: 使用 Bootstrap 5.3.0

#### 關鍵元素
```html
<!-- 側邊欄選單容器 -->
<div id="menuButtons" class="px-2"></div>

<!-- 標籤頁標題區 -->
<ul class="nav nav-tabs" id="tabHeader"></ul>

<!-- 標籤頁內容區 -->
<div id="tabContent" class="tab-content border rounded p-3 mt-2 bg-light"></div>
```

#### 數據注入機制
通過 ViewBag 將後端數據注入前端：
```javascript
const currentUserRole = @Html.Raw(JsonSerializer.Serialize(ViewBag.Role ?? ""));
const EmployeeID = @Html.Raw(JsonSerializer.Serialize(ViewBag.EmployeeID ?? ""));
const roleAccess = @Html.Raw(JsonSerializer.Serialize(ViewBag.RoleAccess ?? new Dictionary<string, List<string>>()));
```

### 2. 控制器邏輯 (`DashboardController.cs`)

#### 路由配置
- 基礎路由: `/Dashboard`
- 動態標籤頁: `/Dashboard/{id}`

#### 權限控制
```csharp
ViewBag.RoleAccess = new Dictionary<string, List<string>> {
    { "超級管理員", new List<string>{ "overview", "monitor", "behavior", "orders", "system", "roles", "Backend_user_list", "contract_template", "platform_fee", "imgup", "furniture_fee", "Marquee_edit", "furniture_management" } },
    { "管理員", new List<string>{ "overview", "behavior", "orders" } },
    { "房源審核員", new List<string>{ "monitor" } },
    { "客服", new List<string>{ "behavior", "orders" } }
};
```

#### 動態內容載入
`LoadTab(string id)` 方法處理標籤頁內容：
- 特殊處理: `platform_fee`、`furniture_management` 等需要數據預處理的標籤頁
- 通用處理: 直接載入對應的 Partial View
- 錯誤處理: 檔案不存在時返回錯誤訊息

### 3. 前端控制邏輯 (`dashboard.js`)

#### 核心設計原則

##### 模組化標籤頁管理
```javascript
const tabNames = {
    overview: "📊 平台整體概況",
    monitor: "🧭 商品與房源監控",
    behavior: "👣 用戶行為監控",
    // ... 其他標籤頁
};
```

##### 功能分組架構
```javascript
const tabGroups = {
    Dashboard: {
        title: "📊 儀表板",
        keys: ['overview', 'monitor', 'behavior', 'orders', 'system']
    },
    Permission: {
        title: "🛡️ 權限管理", 
        keys: ['roles', 'Backend_user_list']
    },
    // ... 其他分組
};
```

#### 動態標籤頁生命週期

##### 1. 標籤頁創建 (`openTab`)
```javascript
function openTab(tabKey) {
    // 1. 檢查標籤頁是否已存在
    // 2. 創建標籤頁標題和內容容器
    // 3. AJAX 載入內容
    // 4. 動態載入對應的 JavaScript 檔案
    // 5. 執行初始化函數
}
```

##### 2. 內容載入機制
- **AJAX 請求**: `fetch(/Dashboard/${tabKey})`
- **腳本映射**: 每個標籤頁對應特定的 JavaScript 檔案
- **版本控制**: 使用時間戳防止快取問題

##### 3. 腳本初始化策略
```javascript
const scriptMap = {
    overview: `/js/overview.js?v=${timestamp}`,
    monitor: `/js/monitor.js?v=${timestamp}`,
    // ... 其他映射
};

// 針對不同標籤頁執行特定初始化
if (tabKey === "roles" && typeof updateRoleListWithPermissions === "function") {
    updateRoleListWithPermissions();
}
```

#### 標籤頁狀態管理
- **切換邏輯**: `switchTab()` 管理 active 狀態
- **關閉邏輯**: `closeTab()` 處理標籤頁移除和狀態維護
- **權限過濾**: 根據使用者角色動態生成選單

## 核心架構規則

### 1. 權限驅動的選單生成
```javascript
function initSidebar() {
    // 只顯示使用者有權限的功能
    keys.forEach(key => {
        if (!roleAccess[currentUserRole]?.includes(key)) return;
        // 創建選單項目
    });
}
```

### 2. 按需載入原則
- 標籤頁內容和腳本只在首次開啟時載入
- 已載入的標籤頁直接切換，不重複載入
- 每個功能模組的 JavaScript 獨立管理

### 3. 模組化 Partial View 架構
- 每個功能對應獨立的 Partial View
- 特殊功能 (如 `furniture_management`) 需要數據預處理
- 統一的錯誤處理和使用者回饋

### 4. 狀態同步機制
- 前端維護標籤頁的 active 狀態
- 關閉當前標籤頁時自動切換到最後一個標籤頁
- 使用者角色和權限資訊全域可用

### 5. 腳本生命週期管理
- 動態載入標籤頁專用腳本
- 載入完成後執行對應的初始化函數
- 支援多種初始化模式 (資料載入、事件綁定等)

## 擴展指南

### 新增功能標籤頁
1. 在 `tabNames` 中定義標籤頁名稱
2. 在 `tabGroups` 中加入適當分組
3. 在權限設定中加入對應角色
4. 創建對應的 Partial View
5. 創建對應的 JavaScript 檔案
6. 在 `scriptMap` 中加入腳本映射
7. 在載入邏輯中加入初始化處理

### 權限控制擴展
- 在 `DashboardController.cs` 的 `ViewBag.RoleAccess` 中定義新角色權限
- 前端會自動根據權限生成對應選單

### 特殊功能處理
- 需要數據預處理的功能在 `LoadTab` 方法中特殊處理
- 複雜初始化邏輯在腳本載入完成後執行

## 技術特色

1. **動態權限控制**: 基於角色的功能可見性
2. **按需載入**: 提升初始化效能
3. **模組化設計**: 功能獨立，易於維護
4. **狀態管理**: 完整的標籤頁生命週期管理
5. **錯誤處理**: 統一的錯誤回饋機制
6. **響應式設計**: 適應不同螢幕尺寸

## 新增頁面開發規範

### 完整開發流程

#### 步驟 1: 前端配置 (`dashboard.js`)

##### 1.1 添加標籤頁名稱
```javascript
const tabNames = {
    // 現有項目...
    my_new_feature: "🆕 我的新功能", // 新增項目
};
```

##### 1.2 設定功能分組
```javascript
const tabGroups = {
    // 現有分組...
    MyGroup: {
        title: "🆕 我的功能群組",
        keys: ['my_new_feature', 'another_feature']
    },
    // 或者加入現有分組
    Dashboard: {
        title: "📊 儀表板",
        keys: ['overview', 'monitor', 'behavior', 'orders', 'system', 'my_new_feature'] // 加入現有分組
    }
};
```

##### 1.3 註冊腳本映射
```javascript
const scriptMap = {
    // 現有映射...
    my_new_feature: `/js/my_new_feature.js?v=${timestamp}`, // 新增映射
};
```

##### 1.4 設定初始化邏輯
```javascript
// 在 script.onload 回調中添加
if (tabKey === "my_new_feature") {
    // 執行初始化函數
    if (typeof initMyNewFeature === "function") {
        initMyNewFeature();
    }
    
    // 綁定事件
    if (typeof bindMyNewFeatureEvents === "function") {
        bindMyNewFeatureEvents();
    }
    
    // 載入資料
    if (typeof loadMyNewFeatureData === "function") {
        loadMyNewFeatureData();
    }
}
```

#### 步驟 2: 後端配置 (`DashboardController.cs`)

##### 2.1 添加權限設定
```csharp
ViewBag.RoleAccess = new Dictionary<string, List<string>> {
    { "超級管理員", new List<string>{ 
        /* 現有權限 */, 
        "my_new_feature"  // 新增權限
    }},
    { "管理員", new List<string>{ 
        /* 現有權限 */, 
        "my_new_feature"  // 根據需要添加
    }},
    // 其他角色...
};
```

##### 2.2 處理特殊載入邏輯 (可選)
```csharp
[HttpGet("{id}")]
public IActionResult LoadTab(string id)
{
    // 現有邏輯...
    
    // 如果需要數據預處理
    if (id == "my_new_feature")
    {
        var data = _context.MyEntities
            .Where(e => e.IsActive)
            .OrderBy(e => e.CreatedAt)
            .ToList();

        return PartialView("~/Views/Dashboard/Partial/my_new_feature.cshtml", data);
    }
    
    // 通用處理邏輯...
    var viewPath = $"~/Views/Dashboard/Partial/{id}.cshtml";
    return PartialView(viewPath);
}
```

##### 2.3 添加相關 API 端點 (可選)
```csharp
[HttpGet("GetMyNewFeatureData")]
public IActionResult GetMyNewFeatureData()
{
    var data = _context.MyEntities
        .Select(e => new {
            e.Id,
            e.Name,
            e.Status,
            CreatedAt = e.CreatedAt.ToString("yyyy-MM-dd HH:mm")
        })
        .ToList();

    return Json(data);
}

[HttpPost("CreateMyNewFeature")]
public IActionResult CreateMyNewFeature([FromBody] MyNewFeatureModel model)
{
    if (model == null) 
        return BadRequest("資料不完整");

    try
    {
        var entity = new MyEntity
        {
            Name = model.Name,
            Status = model.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.MyEntities.Add(entity);
        _context.SaveChanges();

        return Ok("✅ 創建成功");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"❌ 創建失敗：{ex.Message}");
    }
}
```

#### 步驟 3: 視圖檔案 (`Views/Dashboard/Partial/my_new_feature.cshtml`)

##### 3.1 基本結構範本
```html
@model List<MyEntity>
@{
    ViewData["Title"] = "我的新功能";
}

<div class="container-fluid">
    <!-- 標題區 -->
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h4>🆕 我的新功能管理</h4>
        <button class="btn btn-primary" onclick="openCreateModal()">
            <i class="bi bi-plus-circle"></i> 新增項目
        </button>
    </div>

    <!-- 篩選區 (可選) -->
    <div class="card mb-3">
        <div class="card-body">
            <div class="row">
                <div class="col-md-3">
                    <label class="form-label">狀態篩選</label>
                    <select class="form-select" id="statusFilter">
                        <option value="">全部</option>
                        <option value="active">啟用</option>
                        <option value="inactive">停用</option>
                    </select>
                </div>
                <div class="col-md-3 d-flex align-items-end">
                    <button class="btn btn-outline-primary" onclick="applyFilter()">
                        <i class="bi bi-search"></i> 篩選
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- 資料表格區 -->
    <div class="card">
        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-hover" id="myNewFeatureTable">
                    <thead class="table-light">
                        <tr>
                            <th>編號</th>
                            <th>名稱</th>
                            <th>狀態</th>
                            <th>建立時間</th>
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var item in Model)
                        {
                            <tr>
                                <td>@item.Id</td>
                                <td>@item.Name</td>
                                <td>
                                    <span class="badge bg-@(item.Status ? "success" : "secondary")">
                                        @(item.Status ? "啟用" : "停用")
                                    </span>
                                </td>
                                <td>@item.CreatedAt.ToString("yyyy-MM-dd HH:mm")</td>
                                <td>
                                    <button class="btn btn-sm btn-outline-primary" onclick="editItem('@item.Id')">
                                        <i class="bi bi-pencil"></i> 編輯
                                    </button>
                                    <button class="btn btn-sm btn-outline-danger" onclick="deleteItem('@item.Id')">
                                        <i class="bi bi-trash"></i> 刪除
                                    </button>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- 建立/編輯 Modal -->
    <div class="modal fade" id="myNewFeatureModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalTitle">新增項目</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <form id="myNewFeatureForm">
                        <input type="hidden" id="itemId" />
                        <div class="mb-3">
                            <label for="itemName" class="form-label">名稱 <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="itemName" required>
                        </div>
                        <div class="mb-3">
                            <label for="itemStatus" class="form-label">狀態</label>
                            <select class="form-select" id="itemStatus">
                                <option value="true">啟用</option>
                                <option value="false">停用</option>
                            </select>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                    <button type="button" class="btn btn-primary" onclick="saveItem()">儲存</button>
                </div>
            </div>
        </div>
    </div>
</div>
```

#### 步驟 4: JavaScript 檔案 (`wwwroot/js/my_new_feature.js`)

##### 4.1 功能實現範本
```javascript
// ====== 全域變數 ======
let currentItemId = null;
const modal = new bootstrap.Modal(document.getElementById('myNewFeatureModal'));

// ====== 初始化函數 ======
function initMyNewFeature() {
    console.log('🆕 我的新功能初始化完成');
    loadMyNewFeatureData();
}

// ====== 資料載入 ======
function loadMyNewFeatureData() {
    fetch('/Dashboard/GetMyNewFeatureData')
        .then(response => response.json())
        .then(data => {
            renderTable(data);
        })
        .catch(error => {
            console.error('載入資料失敗:', error);
            showToast('載入資料失敗', 'error');
        });
}

// ====== 渲染表格 ======
function renderTable(data) {
    const tbody = document.querySelector('#myNewFeatureTable tbody');
    tbody.innerHTML = '';

    data.forEach(item => {
        const row = `
            <tr>
                <td>${item.id}</td>
                <td>${item.name}</td>
                <td>
                    <span class="badge bg-${item.status ? 'success' : 'secondary'}">
                        ${item.status ? '啟用' : '停用'}
                    </span>
                </td>
                <td>${item.createdAt}</td>
                <td>
                    <button class="btn btn-sm btn-outline-primary" onclick="editItem('${item.id}')">
                        <i class="bi bi-pencil"></i> 編輯
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteItem('${item.id}')">
                        <i class="bi bi-trash"></i> 刪除
                    </button>
                </td>
            </tr>
        `;
        tbody.innerHTML += row;
    });
}

// ====== Modal 操作 ======
function openCreateModal() {
    currentItemId = null;
    document.getElementById('modalTitle').textContent = '新增項目';
    document.getElementById('myNewFeatureForm').reset();
    modal.show();
}

function editItem(id) {
    currentItemId = id;
    document.getElementById('modalTitle').textContent = '編輯項目';
    
    // 載入項目資料
    fetch(`/Dashboard/GetMyNewFeatureById?id=${id}`)
        .then(response => response.json())
        .then(data => {
            document.getElementById('itemId').value = data.id;
            document.getElementById('itemName').value = data.name;
            document.getElementById('itemStatus').value = data.status.toString();
            modal.show();
        })
        .catch(error => {
            console.error('載入項目失敗:', error);
            showToast('載入項目失敗', 'error');
        });
}

// ====== 儲存操作 ======
function saveItem() {
    const formData = {
        id: currentItemId,
        name: document.getElementById('itemName').value,
        status: document.getElementById('itemStatus').value === 'true'
    };

    const url = currentItemId ? '/Dashboard/UpdateMyNewFeature' : '/Dashboard/CreateMyNewFeature';
    
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData)
    })
    .then(response => response.text())
    .then(result => {
        showToast(result, 'success');
        modal.hide();
        loadMyNewFeatureData(); // 重新載入資料
    })
    .catch(error => {
        console.error('儲存失敗:', error);
        showToast('儲存失敗', 'error');
    });
}

// ====== 刪除操作 ======
function deleteItem(id) {
    if (!confirm('確定要刪除這個項目嗎？')) return;

    fetch('/Dashboard/DeleteMyNewFeature', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(id)
    })
    .then(response => response.text())
    .then(result => {
        showToast(result, 'success');
        loadMyNewFeatureData(); // 重新載入資料
    })
    .catch(error => {
        console.error('刪除失敗:', error);
        showToast('刪除失敗', 'error');
    });
}

// ====== 篩選功能 ======
function applyFilter() {
    const status = document.getElementById('statusFilter').value;
    const url = status ? `/Dashboard/GetMyNewFeatureData?status=${status}` : '/Dashboard/GetMyNewFeatureData';
    
    fetch(url)
        .then(response => response.json())
        .then(data => renderTable(data))
        .catch(error => {
            console.error('篩選失敗:', error);
            showToast('篩選失敗', 'error');
        });
}

// ====== 事件綁定 ======
function bindMyNewFeatureEvents() {
    // 篩選欄位變更事件
    document.getElementById('statusFilter').addEventListener('change', applyFilter);
    
    // 表單驗證事件
    document.getElementById('myNewFeatureForm').addEventListener('submit', function(e) {
        e.preventDefault();
        saveItem();
    });
}
```

### 語法規範與最佳實踐

#### 1. 命名約定
- **標籤頁 Key**: 使用底線分隔 (snake_case)，如 `my_new_feature`
- **函數名稱**: 使用駝峰式命名 (camelCase)，如 `initMyNewFeature`
- **CSS Class**: 使用 Bootstrap 5 規範
- **API 端點**: 使用 PascalCase，如 `GetMyNewFeatureData`

#### 2. 檔案結構約定
```
Views/Dashboard/Partial/my_new_feature.cshtml  # 視圖檔案
wwwroot/js/my_new_feature.js                   # JavaScript 檔案
```

#### 3. 權限設定規範
- 確保所有新功能都有對應的權限設定
- 權限 Key 與標籤頁 Key 保持一致
- 根據功能重要性分配給適當角色

#### 4. API 設計規範
- 使用 RESTful 命名約定
- 統一錯誤處理格式
- 返回適當的 HTTP 狀態碼
- 使用中文錯誤訊息

#### 5. 前端交互規範
- 使用 `showToast()` 顯示操作結果
- 使用 Bootstrap Modal 進行互動
- 保持 UI 組件的一致性
- 實現適當的載入狀態顯示

#### 6. 資料驗證規範
- 前端和後端都要進行輸入驗證
- 使用 Bootstrap 驗證樣式
- 提供清晰的錯誤訊息
- 實現必要的安全檢查

這套規範確保新功能能夠無縫整合到現有架構中，並保持代碼的一致性和可維護性。

## Dashboard 架構使用的 JavaScript 檔案

### 核心架構檔案

#### 1. `dashboard.js` - 主控制檔案
- **功能**: Dashboard 架構的核心控制邏輯
- **職責**: 
  - 動態標籤頁管理
  - 權限控制
  - 側邊欄選單生成
  - 腳本動態載入
  - 標籤頁生命週期管理

### 功能模組 JavaScript 檔案

根據 `dashboard.js` 中的 `scriptMap` 配置，Dashboard 架構動態載入以下檔案：

#### 2. 儀表板分組檔案
```javascript
// 儀表板核心功能
overview: `/js/overview.js`           // 📊 平台整體概況
monitor: `/js/monitor.js`             // 🧭 商品與房源監控  
behavior: `/js/behavior.js`           // 👣 用戶行為監控
orders: `/js/orders.js`               // 💳 訂單與金流
system: `/js/system.js`               // 🛠️ 系統通知與健康
```

#### 3. 權限管理分組檔案
```javascript
// 權限管理功能
roles: `/js/roles.js`                 // 🛡️ 身分權限列表
Backend_user_list: `/js/Backend_user_list.js`  // 👨‍💻 後臺使用者
```

#### 4. 模板管理分組檔案
```javascript
// 合約範本管理
contract_template: `/js/contract_template.js`   // 📄 合約範本管理
```

#### 5. 平台圖片與文字資料管理分組檔案
```javascript
// 圖片與文字資料管理
imgup: `/js/imgup.js`                 // 🖼️ 輪播圖片管理
Marquee_edit: `/js/Marquee_edit.js`   // 🌀 跑馬燈管理
furniture_management: `/js/furniture_management.js`  // 🛋️ 家具列表管理
```

#### 6. 平台費用設定分組檔案
```javascript
// 費用設定功能
platform_fee: `/js/platform_fee.js`  // 💰 平台收費設定
furniture_fee: `/js/furniture_fee.js` // 📦 家具配送費
```

### 檔案載入機制

#### 動態載入流程
1. **按需載入**: 只有當使用者開啟對應標籤頁時才載入對應的 JavaScript 檔案
2. **版本控制**: 使用時間戳 `?v=${timestamp}` 防止瀏覽器快取
3. **初始化執行**: 檔案載入完成後自動執行對應的初始化函數

#### 初始化函數對應表
```javascript
// 各模組的初始化函數對應
roles → updateRoleListWithPermissions()
Backend_user_list → renderUserTable()
contract_template → renderTemplateList() + bindContractUploadEvents()
furniture_management → resetForm() + loadAllInventoryEvents() + 事件綁定
platform_fee → onload() + renderListingPlans()
furniture_fee → initShipFee()
imgup → initCarouselManager()
```

### 檔案結構說明

#### 主要檔案分佈
```
wwwroot/js/
├── dashboard.js                 # 核心架構控制檔案
├── overview.js                  # 平台概況
├── monitor.js                   # 商品房源監控
├── behavior.js                  # 用戶行為
├── orders.js                    # 訂單金流
├── system.js                    # 系統健康
├── roles.js                     # 權限管理
├── Backend_user_list.js         # 後臺使用者
├── contract_template.js         # 合約範本
├── imgup.js                     # 輪播圖片
├── Marquee_edit.js             # 跑馬燈
├── furniture_management.js      # 家具管理
├── platform_fee.js             # 平台費用
├── furniture_fee.js             # 配送費用
└── admin_js/                    # 其他管理功能檔案
    ├── customer-service-details.js
    ├── customer-service.js
    ├── property-details.js
    ├── property-management.js
    ├── system-message-new.js
    ├── system-message.js
    ├── user-details.js
    └── user-management.js
```

### 非 Dashboard 架構檔案

以下檔案不屬於 Dashboard 架構，但存在於同一目錄中：

#### 前台功能檔案
- `Announcement.js` - 公告功能
- `CollectionAndComparison.js` - 收藏比較功能
- `FrontPage.js` - 前台首頁
- `Search.js` - 搜尋功能

#### 會員功能檔案
- `memberProfile.js` - 會員資料
- `memberInboxJs/changeContentName.js` - 會員收件匣

#### 通用工具檔案
- `site.js` - 網站通用功能
- `admin-common.js` - 管理介面通用功能
- `checkUpload.js` - 檔案上傳檢查
- `hiddenPassword.js` - 密碼顯示/隱藏
- `selectDistrict.js` - 地區選擇
- `showMemberToast.js` - 會員提示訊息
- `switchSignatureMode.js` - 簽名模式切換

### 檔案命名規範

#### Dashboard 模組檔案命名規則
1. **功能導向命名**: 檔案名稱直接反映功能，如 `furniture_management.js`
2. **底線分隔**: 使用底線分隔多個單字，如 `contract_template.js`
3. **一致性原則**: 檔案名稱與標籤頁 key 保持一致

#### 載入順序與依賴
- **無依賴性**: 各模組檔案相互獨立，無載入順序要求
- **全域函數**: 使用全域函數避免模組間衝突
- **事件隔離**: 每個模組管理自己的事件綁定

### 性能優化策略

#### 1. 按需載入
- 只載入當前需要的功能模組
- 減少初始頁面載入時間
- 提升使用者體驗

#### 2. 快取控制
- 使用時間戳防止不必要的快取
- 確保更新後的檔案能正確載入

#### 3. 錯誤處理
- 檔案載入失敗時的容錯機制
- 初始化函數不存在時的安全檢查

這個檔案組織架構確保了 Dashboard 系統的模組化、可維護性和擴展性。

## JavaScript 模組封裝模式 - IIFE

### 什麼是 IIFE (Immediately Invoked Function Expression)

Dashboard 架構中的所有 JavaScript 檔案都使用 **IIFE (立即執行函數表達式)** 模式：

```javascript
(() => {
    // 模組代碼
})();
```

### 為什麼使用 IIFE 封裝

#### 1. **避免全域命名空間污染**

**問題場景**：
```javascript
// ❌ 不使用 IIFE 的問題
// overview.js
var chartData = [1, 2, 3];
var config = { type: 'bar' };

// monitor.js  
var chartData = [4, 5, 6];  // 覆蓋了 overview.js 的變數
var config = { type: 'line' }; // 覆蓋了 overview.js 的變數
```

**IIFE 解決方案**：
```javascript
// ✅ 使用 IIFE 避免衝突
// overview.js
(() => {
    const chartData = [1, 2, 3];  // 局部變數，不會污染全域
    const config = { type: 'bar' };
    
    // 圖表初始化邏輯
    new Chart(document.getElementById('chartOrders'), config);
})();

// monitor.js
(() => {
    const chartData = [4, 5, 6];  // 獨立的局部變數
    const config = { type: 'line' };
    
    // 不會與 overview.js 衝突
    new Chart(document.getElementById('chartMonitor'), config);
})();
```

#### 2. **模組隔離與封裝**

**變數作用域隔離**：
```javascript
(() => {
    // 私有變數，外部無法訪問
    let currentEditingId = null;
    const API_BASE_URL = '/Dashboard';
    
    // 私有函數
    function validateInput(data) {
        // 驗證邏輯
    }
    
    // 只有需要的函數才暴露到全域
    window.editFurniture = function(id) {
        currentEditingId = id;
        // 編輯邏輯
    };
    
    window.saveFurniture = function() {
        // 使用私有變數和函數
        if (validateInput(formData)) {
            // 儲存邏輯
        }
    };
})();
```

#### 3. **防止意外的變數覆蓋**

在 Dashboard 架構中，多個標籤頁可能同時載入，IIFE 確保各模組互不干擾：

```javascript
// roles.js
(() => {
    const emojiMap = {
        overview: "📊", 
        monitor: "📦"
    };
    
    window.updateRoleListWithPermissions = function() {
        // 使用局部的 emojiMap
    };
})();

// furniture_management.js  
(() => {
    const emojiMap = {
        success: "✅",
        error: "❌"
    };
    
    window.submitFurniture = function() {
        // 使用自己的 emojiMap，不會衝突
    };
})();
```

#### 4. **記憶體管理與垃圾回收**

```javascript
(() => {
    // 大型資料對象
    const heavyData = {
        // 大量數據
    };
    
    // 事件監聽器
    const eventHandlers = {
        // 處理器
    };
    
    // 當模組不再使用時，這些變數會被垃圾回收
    // 不會永久佔用記憶體
})();
```

### Dashboard 架構中的實際應用

#### 1. **全域函數暴露模式**

許多模組需要暴露函數供 HTML onclick 或其他模組調用：

```javascript
// furniture_management.js
(() => {
    // 私有變數
    let isEditMode = false;
    let currentItemId = null;
    
    // 暴露給 HTML onclick 使用
    window.editFurniture = function(id) {
        currentItemId = id;
        isEditMode = true;
        // 編輯邏輯
    };
    
    // 暴露給 dashboard.js 初始化調用
    window.resetForm = function() {
        isEditMode = false;
        currentItemId = null;
        // 重置邏輯
    };
})();
```

#### 2. **模組初始化模式**

```javascript
// roles.js
(() => {
    // 檢查依賴
    if (typeof roleAccess === 'undefined') {
        console.warn("🚫 roleAccess 未定義，無法初始化角色清單");
        return;
    }
    
    // 私有配置
    const emojiMap = { /* 配置 */ };
    
    // 暴露初始化函數給 dashboard.js
    window.updateRoleListWithPermissions = function() {
        // 初始化邏輯
    };
})();
```

#### 3. **事件綁定封裝模式**

```javascript
// imgup.js 
(() => {
    // 私有事件處理器
    function handleImageUpload(event) {
        // 處理邏輯
    }
    
    function handleImageDelete(id) {
        // 刪除邏輯
    }
    
    // 暴露初始化函數
    window.initCarouselManager = function() {
        // 綁定事件到 DOM 元素
        document.getElementById('uploadBtn').addEventListener('click', handleImageUpload);
        // 其他初始化
    };
})();
```

### 最佳實踐規範

#### 1. **變數宣告原則**
```javascript
(() => {
    // ✅ 使用 const/let，避免 var
    const CONFIG = { /* 不可變配置 */ };
    let state = { /* 可變狀態 */ };
    
    // ❌ 避免使用 var（會提升到函數作用域）
    // var data = {};
})();
```

#### 2. **全域函數暴露原則**
```javascript
(() => {
    // ✅ 只暴露必要的公共接口
    window.myModuleInit = function() { /* 初始化 */ };
    window.myModulePublicMethod = function() { /* 公共方法 */ };
    
    // ❌ 不要暴露內部實現細節
    // window.myModulePrivateHelper = function() { /* 私有輔助函數 */ };
})();
```

#### 3. **依賴檢查模式**
```javascript
(() => {
    // ✅ 檢查外部依賴
    if (typeof requiredGlobalVar === 'undefined') {
        console.warn("依賴未滿足，模組無法初始化");
        return;
    }
    
    // 模組邏輯
})();
```

### IIFE 的優勢總結

1. **命名空間隔離**: 防止變數名稱衝突
2. **封裝性**: 隱藏內部實現細節
3. **記憶體效率**: 自動垃圾回收
4. **模組化**: 清晰的模組邊界
5. **安全性**: 防止意外修改
6. **可維護性**: 獨立的模組更容易維護

這種模式確保了 Dashboard 架構中多個 JavaScript 模組能夠和諧共存，同時保持各自的獨立性和安全性。

## 避免 JavaScript 衝突的核心機制

### 問題背景：兩層 JavaScript 執行環境

Dashboard 架構面臨一個複雜的 JavaScript 執行環境問題：

1. **第一層**：`dashboard.js` - 負責生成和管理 tabContent
2. **第二層**：動態載入的模組 JavaScript - 負責 tabContent 頁面內部邏輯

### 衝突避免的四大機制

#### 1. **時序分離 (Temporal Separation)**

**載入順序控制**：
```javascript
// dashboard.js 中的載入流程
fetch(`/Dashboard/${tabKey}`)
    .then(r => r.text())
    .then(html => {
        // 步驟 1: 先載入 HTML 內容
        tabContent.innerHTML = html;
        
        // 步驟 2: 再動態載入對應的 JavaScript
        const script = document.createElement('script');
        script.src = scriptMap[tabKey];
        
        // 步驟 3: 腳本載入完成後才執行初始化
        script.onload = () => {
            if (tabKey === "furniture_management" && typeof resetForm === "function") {
                resetForm(); // 確保 DOM 已就緒再執行
            }
        };
        
        document.body.appendChild(script);
    });
```

**為什麼有效**：
- HTML 先載入，DOM 元素已存在
- JavaScript 後載入，可以安全地查找 DOM 元素
- 避免了「找不到 DOM 元素」的問題

#### 2. **命名空間隔離 (Namespace Isolation)**

**dashboard.js 的全域函數**：
```javascript
// dashboard.js 中的函數使用特定前綴或明確作用域
function openTab(tabKey) { /* 管理標籤頁 */ }
function switchTab(tabId) { /* 切換標籤頁 */ }
function closeTab(tabId) { /* 關閉標籤頁 */ }
function initSidebar() { /* 初始化側邊欄 */ }
```

**模組 JavaScript 的全域函數**：
```javascript
// furniture_management.js
(() => {
    // 使用 window 明確暴露，避免與 dashboard.js 衝突
    window.editFurniture = function(id) { /* 編輯家具 */ };
    window.resetForm = function() { /* 重置表單 */ };
    window.submitFurniture = function() { /* 提交家具 */ };
})();
```

**命名約定規則**：
- dashboard.js：使用通用動詞 (open, switch, close, init)
- 模組 JavaScript：使用特定領域名詞 (editFurniture, resetForm)

#### 3. **作用域封裝 (Scope Encapsulation)**

**dashboard.js 的變數作用域**：
```javascript
// dashboard.js 中的變數
const tabNames = { /* 標籤頁名稱 */ };
const tabGroups = { /* 功能分組 */ };
const scriptMap = { /* 腳本映射 */ };

function openTab(tabKey) {
    // 這些變數只在 dashboard.js 中可見
    const tabId = `tab-${tabKey}`;
    const tabExists = document.getElementById(tabId);
    // ...
}
```

**模組 JavaScript 的 IIFE 封裝**：
```javascript
// furniture_management.js
(() => {
    // 私有變數，不會與 dashboard.js 衝突
    let currentEditingId = null;
    let isEditMode = false;
    const API_BASE_URL = '/Dashboard';
    
    // 私有函數，不會與 dashboard.js 衝突
    function validateInput(data) { /* 驗證邏輯 */ }
    function resetFormFields() { /* 重置欄位 */ }
    
    // 只暴露必要的公共接口
    window.editFurniture = function(id) {
        currentEditingId = id; // 使用私有變數
        isEditMode = true;
        // ...
    };
})();
```

#### 4. **DOM 元素隔離 (DOM Isolation)**

**dashboard.js 操作的 DOM**：
```javascript
// dashboard.js 只操作框架層級的 DOM
document.getElementById("menuButtons")     // 側邊欄選單
document.getElementById("tabHeader")       // 標籤頁標題
document.getElementById("tabContent")      // 標籤頁容器
```

**模組 JavaScript 操作的 DOM**：
```javascript
// furniture_management.js 只操作自己標籤頁內的 DOM
(() => {
    window.editFurniture = function(id) {
        // 只操作 furniture_management 標籤頁內的元素
        document.getElementById("formMode").innerText = `編輯模式`;
        document.getElementById("furnitureName").value = data.ProductName;
        document.getElementById("submitBtn").style.display = "none";
    };
})();
```

**DOM 查找安全性**：
```html
<!-- furniture_management.cshtml 中的 HTML -->
<div class="tab-pane" id="tab-furniture_management-content">
    <button onclick="editFurniture('@item.FurnitureID')">編輯</button>
    <input id="furnitureName" />
    <button id="submitBtn">提交</button>
</div>
```

### 具體衝突避免實例

#### 實例 1：事件處理器衝突避免

**問題場景**：多個標籤頁都有「提交」按鈕

**解決方案**：
```javascript
// furniture_management.js
(() => {
    // 私有事件處理，不會與其他模組衝突
    document.getElementById("submitBtn").addEventListener("click", function() {
        // 家具提交邏輯
    });
})();

// contract_template.js  
(() => {
    // 不同的 submitBtn，在不同的標籤頁中
    document.getElementById("submitBtn").addEventListener("click", function() {
        // 合約範本提交邏輯
    });
})();
```

#### 實例 2：全域函數名稱衝突避免

**潛在衝突**：
```javascript
// ❌ 如果都叫 init() 會衝突
window.init = function() { /* furniture_management 初始化 */ };
window.init = function() { /* contract_template 初始化 */ }; // 覆蓋了上面的
```

**實際解決**：
```javascript
// ✅ 使用具體的函數名稱
window.resetForm = function() { /* furniture_management 專用 */ };
window.renderTemplateList = function() { /* contract_template 專用 */ };
window.initCarouselManager = function() { /* imgup 專用 */ };
```

#### 實例 3：變數名稱衝突避免

**潛在衝突**：
```javascript
// ❌ 如果不使用 IIFE
var currentId = null;  // furniture_management.js
var currentId = null;  // contract_template.js - 會覆蓋上面的
```

**實際解決**：
```javascript
// ✅ 使用 IIFE 封裝
// furniture_management.js
(() => {
    let currentId = null; // 局部變數，不會衝突
})();

// contract_template.js
(() => {
    let currentId = null; // 獨立的局部變數
})();
```

### 載入生命週期管理

#### 完整的載入流程
```javascript
// dashboard.js 中的完整流程
function openTab(tabKey) {
    // 1. 檢查是否已存在
    if (document.getElementById(`tab-${tabKey}`)) {
        switchTab(`tab-${tabKey}`);
        return;
    }
    
    // 2. 創建標籤頁框架
    createTabStructure(tabKey);
    
    // 3. AJAX 載入 HTML 內容
    fetch(`/Dashboard/${tabKey}`)
        .then(r => r.text())
        .then(html => {
            // 4. 注入 HTML 到指定容器
            document.getElementById(`tab-${tabKey}-content`).innerHTML = html;
            
            // 5. 動態載入對應的 JavaScript
            loadTabScript(tabKey);
        });
}

function loadTabScript(tabKey) {
    const script = document.createElement('script');
    script.src = `/js/${tabKey}.js?v=${timestamp}`;
    
    // 6. 腳本載入完成後執行初始化
    script.onload = () => executeTabInitialization(tabKey);
    
    document.body.appendChild(script);
}
```

### 衝突檢測與除錯

#### 開發時的除錯技巧
```javascript
// dashboard.js 中可以添加的除錯輔助
function executeTabInitialization(tabKey) {
    console.log(`🔄 初始化 ${tabKey} 模組`);
    
    // 檢查函數是否存在
    const initFunctions = {
        'furniture_management': ['resetForm', 'editFurniture'],
        'contract_template': ['renderTemplateList', 'uploadFile'],
        'imgup': ['initCarouselManager']
    };
    
    if (initFunctions[tabKey]) {
        initFunctions[tabKey].forEach(funcName => {
            if (typeof window[funcName] === 'function') {
                console.log(`✅ ${funcName} 函數已就緒`);
            } else {
                console.warn(`⚠️ ${funcName} 函數未找到`);
            }
        });
    }
}
```

### 總結：架構優勢

這種設計確保了：

1. **時序安全**：HTML 先載入，JavaScript 後執行
2. **命名安全**：不同層級使用不同的命名約定  
3. **作用域安全**：IIFE 封裝防止變數衝突
4. **DOM 安全**：各模組只操作自己的 DOM 範圍
5. **記憶體安全**：模組可以獨立載入和卸載
6. **維護安全**：模組間相互獨立，易於維護

這個架構設計巧妙地解決了動態標籤頁系統中複雜的 JavaScript 衝突問題。