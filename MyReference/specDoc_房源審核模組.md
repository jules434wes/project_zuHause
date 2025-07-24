# 房源審核模組

本模組處理房源的刊登審核流程，確保平台上的房源資訊真實可靠。所有相關邏輯圍繞以下核心資料表、原則與流程。

### 涉及主表

-   **`properties`**: 房源資料主表。
-   **`approvals`**: 審核案件主表，為一共用表，處理多種類型的審核。
-   **`approvalItems`**: 審核案件的詳細操作歷程記錄。

### 大原則與核心設計

1.  **審核類型區分 (`approvals.moduleCode`)**: 房源審核使用 `moduleCode = 'PROPERTY'` 進行識別。

2.  **房源狀態管理**: `properties` 表中的 `statusCode` 欄位管理房源的完整狀態：
    
    **審核相關狀態**：
    -   `PENDING`: 待審核狀態，房源已提交但未審核完成。
    -   `PENDING_PAYMENT`: 審核通過狀態，等待房東完成付費。
    -   `REJECT_REVISE`: 審核須補件，房東需補充資料後重新提交。
    -   `REJECTED`: 審核拒絕，房源無法上架。
    -   `BANNED`: 因違規被管理員強制下架，需重新申請審核。
    
    **房東管理狀態**：
    -   由用戶介面負責開發者定義，不影響後台功能實作。
    -   後台管理員無法操作這些狀態，僅能看到狀態名稱。
    
3.  **房源可見性條件**: 房源對外可見（顯示於用戶介面、可被搜尋到）需要同時滿足：
    -   **審核通過且已付費**: `properties.statusCode = 'LISTED'`（表示審核通過並完成付費）
    -   **有效期限**: 當前時間早於 `properties.expireAt`
    -   **非違規狀態**: 排除 `BANNED` 和其他非正常上架狀態
    -   **注意**: `PENDING_PAYMENT` 狀態表示審核通過但未付費，不對外可見

4.  **審核對象識別**: 
    -   當審核房源時，`approvals.sourcePropertyID` 欄位儲存被審核的房源ID。
    -   `approvals.applicantMemberID` 欄位記錄申請房東的會員ID。

5.  **前置條件驗證**: 
    -   僅限已驗證的房東 (`members.isLandlord = true`) 才能提交房源申請。
    -   必須提供房源證明文件 (`properties.propertyProofURL`)。

6.  **管理員職責範圍**: 
    -   管理員專注於「審核房源文件，確定審核有無通過」功能。
    -   **不涉及上架費用處理**：付費邏輯由其他模組負責。
    -   審核決策僅基於房源資訊的真實性和完整性。

7.  **唯一約束運用**: 
    -   利用 `UQ_approvals_member_module (moduleCode, applicantMemberID, sourcePropertyID)` 約束。
    -   **約束效果**: 同一房源只能有一筆審核記錄，防止重複申請。
    -   **重新申請機制**: 房源重新申請時更新現有 `approvals` 記錄，而非創建新記錄。

### 核心業務流程

#### 1. 提交房源申請
-   **觸發**: 房東填寫房源資訊並上傳證明文件後提交申請。
-   **系統動作**:
    -   在 `properties` 表中創建房源記錄，`statusCode` 設為 `PENDING`。
    -   在 `approvals` 表中創建審核記錄，`moduleCode = 'PROPERTY'`，`statusCode` 設為 `PENDING`。
    -   在 `approvalItems` 表中創建初始記錄，`actionType` 設為 `SUBMIT`。
    -   `properties.publishedAt` 保持 `NULL`，等待審核通過後設定。

#### 2. 審核通過
-   **觸發**: 管理員檢查房源資訊和證明文件後，點擊「通過」。
-   **系統動作**:
    -   更新 `approvals` 記錄的 `statusCode` 為 `APPROVED`。
    -   觸發器自動同步 `properties.statusCode` 為 `PENDING_PAYMENT`。
    -   `properties.publishedAt` 暫時保持 `NULL`，等待付費完成後設定。
    -   在 `approvalItems` 中新增一筆操作記錄，`actionType` 為 `APPROVED`，並記錄審核管理員的 `admins.adminID`。
-   **重要提醒**: 審核通過後房源進入 `PENDING_PAYMENT` 狀態，需要房東完成付費流程後才會變更為 `LISTED` 狀態並對外可見。

#### 3. 審核須補件
-   **觸發**: 管理員檢查後發現資料不完整但可補正，點擊「須補件」。
-   **系統動作**:
    -   更新 `approvals` 記錄的 `statusCode` 為 `REJECT_REVISE`。
    -   觸發器自動同步 `properties.statusCode` 為 `REJECT_REVISE`。
    -   在 `approvalItems` 中新增一筆操作記錄，`actionType` 為 `REJECT_REVISE`，並記錄需補件的具體要求。
    -   `properties.publishedAt` 保持 `NULL`。
-   **後續處理**: 房東補充資料後可重新提交審核。

#### 4. 審核拒絕
-   **觸發**: 管理員檢查後發現嚴重問題，點擊「拒絕」。
-   **系統動作**:
    -   更新 `approvals` 記錄的 `statusCode` 為 `REJECT_FINAL`。
    -   觸發器自動同步 `properties.statusCode` 為 `REJECTED`。
    -   在 `approvalItems` 中新增一筆操作記錄，`actionType` 為 `REJECT_FINAL`，並記錄拒絕原因。
    -   `properties.publishedAt` 保持 `NULL`。

#### 5. 違規強制下架
-   **觸發**: 管理員發現房源違規（如虛假資訊、不當內容等）。
-   **系統動作**:
    -   管理員直接設置 `properties.statusCode` 為 `BANNED`。
    -   房源立即從公開列表中移除。
    -   **審核記錄處理**：
        -   `approvals` 表記錄保持不變（維持歷史審核狀態）。
        -   在 `approvalItems` 表中新增強制下架操作記錄：
            - `actionType` 設為 `FORCE_BANNED`
            - `actionBy` 記錄執行管理員的 `adminID`
            - `actionNote` 記錄違規原因和下架理由
            - `snapshotJSON` 記錄下架時的房源狀態快照
    -   房東需要修正違規內容後重新申請審核。
-   **重新上架流程**:
    -   房東修正違規內容並重新提交申請。
    -   更新現有的 `approvals` 記錄，`statusCode` 重置為 `PENDING`。
    -   在 `approvalItems` 中新增重新申請操作記錄，`actionType` 為 `SUBMIT`。
    -   進入標準審核流程。

#### 6. 重新申請上架
-   **情境**: 被拒絕（`REJECTED`）或違規下架（`BANNED`）的房源，房東修改後重新申請上架。
-   **處理**: 
    -   檢查是否有進行中的審核，避免重複申請。
    -   更新現有的 `approvals` 記錄，`statusCode` 重置為 `PENDING`。
    -   在 `approvalItems` 中新增重新申請操作記錄，`actionType` 為 `SUBMIT`。
    -   觸發器自動同步 `properties.statusCode` 為 `PENDING`。
-   **約束說明**: 
    -   受 `UQ_approvals_member_module` 約束限制，同一房源只能有一筆審核記錄。
    -   重新申請時更新現有記錄而非創建新記錄，確保資料一致性。

#### 7. 房源修改後重新審核（暫不實作）
-   **情境**: 已上架房源的房東修改關鍵資訊（如地址、租金等），觸發重新審核。
-   **實作狀態**: **暫時不實作此功能**
-   **原因**: 為簡化初期系統複雜度，暫時允許房東修改資訊而不觸發重新審核。
-   **未來規劃**: 後續版本可考慮實作關鍵欄位變更觸發重新審核機制。

---

### 子表與補充說明

#### `properties.propertyProofURL` 欄位
-   **業務規則**: 房源審核必須提供證明文件，如房屋所有權狀、租賃契約等。
-   **格式要求**: 儲存文件的URL路徑，支援PDF、JPG、PNG等格式。
-   **審核重點**: 管理員需要驗證文件的真實性和完整性。

#### 付費與日期欄位機制

##### `properties.paidAt` 欄位
-   **設定時機**: 房東完成付費時自動記錄繳費時間點。
-   **資料類型**: datetime2，記錄精確的付費時間。
-   **管理範圍**: 由付費模組自動管理，審核模組不涉及。

##### `properties.publishedAt` 欄位
-   **設定時機**: 由 `paidAt` 時間推算生成的發布日期。
-   **計算邏輯**: 基於付費時間點計算對外發布的日期。
-   **業務用途**: 用於房源搜尋排序和顯示順序。
-   **管理範圍**: 由付費模組根據 `paidAt` 自動計算。

##### `properties.expireAt` 欄位
-   **設定時機**: 根據繳費方案自動推算上架到期時間。
-   **計算邏輯**: 基於付費時間點和選擇的上架方案計算。
-   **業務用途**: 決定房源可見性的有效期限。
-   **管理範圍**: 由付費模組負責計算和管理。

##### `properties.isPaid` 欄位
-   **業務規則**: 房源可見性的關鍵欄位，與審核狀態獨立。
-   **設定邏輯**: 基於付費狀態和有效期自動更新。
-   **管理範圍**: 由付費模組負責管理。

**後台管理系統職責**：
-   僅需要**顯示**上述付費相關日期資訊
-   **不需要操作**付費邏輯或日期計算
-   專注於審核功能的實現

#### `approvalItems.snapshotJSON` 欄位
-   **房源特定用途**: 儲存房源在審核時的完整資料快照。
-   **內容包含**: 房源基本資訊、租金、地址、證明文件URL等。
-   **稽核價值**: 如果房源資料在審核後被修改，可追溯審核時的原始狀態。

#### 狀態同步機制
-   **自動同步實現**: 通過資料庫觸發器 `trg_approvals_status_sync` 實現自動同步。
-   **同步邏輯**: 當 `approvals.statusCode` 更新時，自動同步對應的 `properties.statusCode`：
    -   `APPROVED` → `PENDING_PAYMENT`
    -   `PENDING` → `PENDING`
    -   `REJECT_REVISE` → `REJECT_REVISE`
    -   `REJECT_FINAL` → `REJECTED`
-   **獨立狀態管理**: 房東可直接修改 `properties.statusCode` 進行房源管理（如暫時下架），不影響審核記錄。
-   **房東身份驗證**: 觸發器 `trg_properties_validate_landlord` 確保只有房東能操作房源。

---

## 管理員後台房源總表篩選器設計

### 篩選器功能設計

管理員後台的「房源總表」頁面提供 **3種篩選狀態**，透過智能識別邏輯能夠應對用戶介面開發者可能新增的未知狀態碼。

#### 篩選選項定義

1. **待審核**
   - **篩選條件**: `properties.statusCode = 'PENDING'`
   - **業務含義**: 房東已提交申請，等待管理員審核
   - **包含情境**: 新申請、重新申請上架

2. **審核通過**
   - **篩選條件**: `approvals.statusCode = 'APPROVED' AND properties.statusCode != 'BANNED'`
   - **業務含義**: 已通過審核的房源，無論房東後續如何管理
   - **包含情境**: 待付款、正常上架、房東暫時下架、維護中等所有非違規狀態

3. **強制下架需重新審核**
   - **篩選條件**: `properties.statusCode = 'BANNED'`
   - **業務含義**: 因違規被管理員強制下架的房源
   - **包含情境**: 違規下架後等待重新申請

#### 實作建議的 SQL 查詢

```sql
-- 管理員後台房源總表篩選器查詢
SELECT 
    p.propertyID,
    p.title,
    p.statusCode as property_status,
    a.statusCode as approval_status,
    p.isPaid,
    p.expireAt,
    p.createdAt,
    p.updatedAt,
    -- 篩選器狀態分類
    CASE 
        WHEN p.statusCode = 'PENDING' THEN '待審核'
        WHEN a.statusCode = 'APPROVED' AND p.statusCode != 'BANNED' THEN '審核通過'
        WHEN p.statusCode = 'BANNED' THEN '強制下架需重新審核'
        ELSE '其他狀態'
    END AS filter_category,
    -- 詳細狀態說明（供管理員參考）
    CASE 
        WHEN p.statusCode = 'PENDING' THEN '等待管理員審核'
        WHEN p.statusCode = 'PENDING_PAYMENT' THEN '審核通過・待付款'
        WHEN p.statusCode = 'LISTED' AND a.statusCode = 'APPROVED' THEN '審核通過・正常上架'
        WHEN p.statusCode = 'REJECT_REVISE' THEN '審核須補件'
        WHEN p.statusCode = 'BANNED' THEN '因違規被強制下架'
        WHEN p.statusCode = 'REJECTED' THEN '審核未通過'
        WHEN a.statusCode = 'APPROVED' AND p.statusCode != 'BANNED' THEN '審核通過・房東管理中'
        ELSE '未知狀態・需檢查'
    END AS status_description
FROM properties p
LEFT JOIN approvals a ON p.propertyID = a.sourcePropertyID 
    AND a.moduleCode = 'PROPERTY'
ORDER BY p.updatedAt DESC;
```

#### 前端篩選器實作建議

```javascript
// 篩選器選項定義
const filterOptions = [
    { 
        value: 'pending', 
        label: '待審核', 
        condition: "properties.statusCode = 'PENDING'",
        color: 'orange' 
    },
    { 
        value: 'approved', 
        label: '審核通過', 
        condition: "approvals.statusCode = 'APPROVED' AND properties.statusCode != 'BANNED'",
        color: 'green' 
    },
    { 
        value: 'banned', 
        label: '強制下架需重新審核', 
        condition: "properties.statusCode = 'BANNED'",
        color: 'red' 
    }
];
```

#### 設計優勢

1. **靈活性高**: 能應對用戶介面開發者新增的未知狀態碼
2. **邏輯清晰**: 管理員僅需關注審核相關狀態
3. **實用性強**: 涵蓋了主要的管理場景
4. **擴展性好**: 未來可以輕鬆添加新的篩選條件

---

## 6種房源審核情境示例

以下為完整的房源審核模組測試案例，涵蓋房源生命週期的主要情境：

### 情境1: 新房源提交審核 (待審核)
**房源狀態**:
- `properties.statusCode`: `PENDING`
- `properties.publishedAt`: NULL
- `properties.propertyProofURL`: 有值

**審核記錄**:
- `approvals`: moduleCode='PROPERTY', statusCode='PENDING'
- `approvalItems`: actionType='SUBMIT' (actionBy=NULL)

**業務含義**: 房東已提交房源申請，等待管理員審核

### 情境2: 房源審核通過 (待付款)
**房源狀態**:
- `properties.statusCode`: `PENDING_PAYMENT`
- `properties.publishedAt`: NULL (等待付費完成後設定)
- `properties.propertyProofURL`: 有值

**審核記錄**:
- `approvals`: moduleCode='PROPERTY', statusCode='APPROVED'
- `approvalItems`: 
  - actionType='SUBMIT' (actionBy=NULL)
  - actionType='APPROVED' (actionBy=管理員ID)

**業務含義**: 房源審核通過，等待房東完成付費流程後變更為 `LISTED` 狀態並對外可見

### 情境3: 房源審核須補件
**房源狀態**:
- `properties.statusCode`: `REJECT_REVISE`
- `properties.publishedAt`: NULL
- `properties.propertyProofURL`: 有值

**審核記錄**:
- `approvals`: moduleCode='PROPERTY', statusCode='REJECT_REVISE'
- `approvalItems`:
  - actionType='SUBMIT' (actionBy=NULL)
  - actionType='REJECT_REVISE' (actionBy=管理員ID)

**業務含義**: 房源資料不完整但可補正，房東需補充資料後重新申請

### 情境4: 房源審核被拒絕
**房源狀態**:
- `properties.statusCode`: `REJECTED`
- `properties.publishedAt`: NULL
- `properties.propertyProofURL`: 有值

**審核記錄**:
- `approvals`: moduleCode='PROPERTY', statusCode='REJECT_FINAL'
- `approvalItems`:
  - actionType='SUBMIT' (actionBy=NULL)
  - actionType='REJECT_FINAL' (actionBy=管理員ID)

**業務含義**: 房源審核不通過，無法上架，需要房東修改後重新申請

### 情境5: 已上架房源修改後重新審核
**房源狀態**:
- `properties.statusCode`: `PENDING` (從LISTED變更)
- `properties.publishedAt`: 有值 (保留原發布時間)
- `properties.propertyProofURL`: 有值

**審核記錄**:
- 更新後的審核: moduleCode='PROPERTY', statusCode='PENDING' (由APPROVED重置)
- 對應的 `approvalItems` 操作歷程包含完整的修改與重新申請記錄

**業務含義**: 房源因修改重要資訊需重新審核，暫時從公開列表移除

### 情境6: 被拒絕房源重新申請上架
**房源狀態**:
- `properties.statusCode`: `PENDING` (從REJECTED變更)
- `properties.publishedAt`: NULL
- `properties.propertyProofURL`: 有值 (可能已更新)

**審核記錄**:
- 更新後的審核: moduleCode='PROPERTY', statusCode='PENDING' (由REJECTED重置)
- 完整的操作歷程追蹤，包含拒絕原因和重新申請記錄

**業務含義**: 房東修改被拒絕的房源後重新申請，進入新的審核流程

### 情境7: 違規強制下架房源
**房源狀態**:
- `properties.statusCode`: `BANNED` (管理員設置)
- `properties.publishedAt`: 有值 (保留原發布時間)
- `properties.propertyProofURL`: 有值
- `properties.isPaid`: true (付費狀態保持)
- `properties.expireAt`: 有值 (過期時間保持)

**審核記錄**:
- 歷史審核: moduleCode='PROPERTY', statusCode='APPROVED' (不變)
- 原操作歷程保持不變

**業務含義**: 房源因違規被管理員強制下架，需要修正後重新申請審核

### 系統改進實施狀態

#### ✅ 已完成改進項目
1. **✅ 狀態格式統一**: 所有狀態值統一使用大寫格式 ('PENDING', 'ACTIVE', 'REJECTED')
2. **✅ 完善審核記錄**: 所有房源都有對應的審核歷程記錄
3. **✅ 狀態同步機制**: 通過觸發器實現自動同步
   - `trg_approvals_status_sync`: 審核狀態變更時自動同步到房源狀態
   - `trg_properties_status_protection`: 防止直接更改房源狀態
4. **✅ 房東身份驗證**: `trg_properties_validate_landlord` 確保只有房東能操作房源

#### 🔒 系統安全機制
- **觸發器防護**: 禁止直接修改 `properties.statusCode`，必須通過審核系統
- **唯一約束**: `UQ_approvals_member_module` 防止重複審核申請
- **身份驗證**: 確保只有已驗證房東能提交房源申請

#### 📊 資料一致性驗證
- **狀態同步**: 100% properties 和 approvals 狀態同步一致
- **審核記錄**: 100% 房源擁有完整審核歷程
- **業務邏輯**: 符合審核→付費→可見的完整流程

#### 測試資料建議
**房源資料表 (properties)**:
- 房源ID 3001-3005 分別對應上述5種情境

**審核記錄表 (approvals)**:
- 對應的房源審核記錄，包含各種狀態組合

**操作歷程表 (approvalItems)**:
- 完整的審核操作歷程，追蹤每個審核的提交和結果

**約束驗證**:
- 利用 `UQ_approvals_member_module` 約束防止重複審核
- 確保房源審核的業務邏輯完整性

### 測試資料業務邏輯驗證

以下是現有測試資料的業務邏輯分析：

#### 房源可見性檢查
```sql
-- 房源對外可見性查詢
SELECT 
    propertyID,
    title,
    statusCode,
    isPaid,
    expireAt,
    CASE 
        WHEN statusCode = 'LISTED' AND isPaid = 1 AND expireAt > GETDATE() THEN '對外可見'
        WHEN statusCode = 'PENDING_PAYMENT' THEN '審核通過但未付費'
        WHEN statusCode = 'LISTED' AND isPaid = 1 AND expireAt <= GETDATE() THEN '審核通過但已過期'
        WHEN statusCode = 'REJECT_REVISE' THEN '審核須補件'
        WHEN statusCode = 'PENDING' THEN '審核中'
        WHEN statusCode = 'REJECTED' THEN '審核拒絕'
        WHEN statusCode = 'BANNED' THEN '違規強制下架'
        ELSE '其他狀態(由用戶介面開發者定義)'
    END AS 實際狀態
FROM properties 
ORDER BY propertyID;
```

#### 測試資料狀態分析
1. **房源狀態為 PENDING_PAYMENT**: 審核通過但未付費 → 用戶不可見
2. **房源狀態為 REJECTED**: 審核拒絕 → 用戶不可見  
3. **房源狀態為 REJECT_REVISE**: 審核須補件 → 用戶不可見
4. **房源狀態為 PENDING**: 審核中 → 用戶不可見
5. **房源狀態為 LISTED**: 審核通過且已付費 → 用戶可見（需同時滿足未過期條件）
6. **房源狀態為 BANNED**: 違規強制下架 → 用戶不可見（需要重新申請審核）

#### 業務邏輯一致性
- ✅ 審核模組職責清晰：僅處理審核狀態
- ✅ 付費邏輯獨立：由其他模組負責
- ✅ 可見性條件完整：需要 LISTED 狀態 + 未過期
- ✅ 狀態同步機制：觸發器確保資料一致性
- ✅ 安全防護機制：防止非法狀態修改
- ✅ 測試資料完整：涵蓋所有業務場景包括完全可見房源