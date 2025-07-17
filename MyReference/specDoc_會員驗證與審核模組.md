# 會員驗證與審核模組

本模組處理會員的身分驗證與房東資格驗證，是平台安全與信任的基石。所有相關邏輯圍繞以下核心資料表、原則與流程。

### 涉及主表

-   **`members`**: 會員資料主表。
-   **`approvals`**: 審核案件主表，為一共用表，處理多種類型的審核。
-   **`approvalItems`**: 審核案件的詳細操作歷程記錄。
-   **`userUploads`**: 使用者上傳檔案記錄表，與審核案件關聯。

### 大原則與核心設計

1.  **審核類型區分 (`approvals.moduleCode`)**: 為確保業務邏輯清晰，審核類型由 `moduleCode` 嚴格區分：
    -   `IDENTITY`: 用於會員**身分驗證**。
    -   `LANDLORD`: 用於**房東資格驗證**。
    -   `PROPERTY`: 用於**房源刊登**審核。
2.  **房東身份標識**: `members` 表中同時使用兩個欄位來定義房東身份，其功能各有側重：
    -   `isLandlord` (bit): 作為布林旗標，主要供後端業務邏輯快速判斷帳號的房東狀態。
    -   `memberTypeID` (int): 外鍵，主要供前端根據用戶類型（1=一般會員, 2=房東）切換不同的介面與功能頁面。
3.  **共用審核表的設計決策 (`approvals` 表)**:
    -   **情境**: `approvals` 表需同時處理對「會員」和對「房源」的審核。
    -   **設計**: 當審核會員時（`moduleCode` 為 `IDENTITY` 或 `LANDLORD`），`sourcePropertyID` 欄位儲存 `NULL`，審核的具體對象由 `applicantMemberID` 欄位識別。
    -   **唯一約束優化**: 原有的 `UQ_approvals_module_source (moduleCode, sourcePropertyID)` 約束已修改為 `UQ_approvals_member_module (moduleCode, applicantMemberID, sourcePropertyID)`，解決了並行審核的限制問題。
    -   **並行審核支援**: 新約束允許多個會員同時進行相同類型的審核申請，但防止同一會員重複提交相同類型申請。

### 核心業務流程

#### 1. 提交申請
-   **觸發**: 使用者上傳所需證件（如身分證）並提交申請。
-   **系統動作**:
    -   在 `approvals` 表中創建一筆或多筆審核記錄，`statusCode` 設為 `PENDING` (待審核)。
    -   在 `approvalItems` 表中創建對應的初始記錄，`actionType` 設為 `SUBMIT` (提交)。
    -   **[身分證驗證特定]**: 在 `userUploads` 表中創建對應的檔案上傳記錄，至少包含：
        -   `moduleCode='MemberInfo'`
        -   `uploadTypeCode='USER_ID_FRONT'` (身分證正面)
        -   `uploadTypeCode='USER_ID_BACK'` (身分證反面)  
        -   `approvalID` 外鍵關聯到對應的審核案件
    -   `members.nationalIdNo` 在此階段保持 `NULL`，等待管理員審核後填入。

#### 2. 審核通過
-   **觸發**: 管理員在後台審核申請，並點擊「通過」。
-   **系統動作**:
    -   管理員根據使用者上傳的證件，手動將身分證號碼填入 `members.nationalIdNo`。
    -   系統更新 `members.identityVerifiedAt` 為當前時間。
    -   更新對應的 `approvals` 記錄的 `statusCode` 為 `APPROVED`。
    -   在 `approvalItems` 中新增一筆操作紀錄，`actionType` 為 `APPROVED`，並在 `actionBy` 欄位記錄該管理員的 `admins.adminID`。
    -   **[房東驗證特定]**: 如果是房東資格驗證通過，系統還需額外更新 `members.isLandlord = true` 以及 `members.memberTypeID = 2`。

#### 3. 審核失敗
-   **觸發**: 管理員在後台審核申請，並點擊「駁回」。
-   **系統動作**:
    -   更新對應的 `approvals` 記錄的 `statusCode` 為 `REJECTED`。
    -   在 `approvalItems` 中新增一筆操作紀錄，`actionType` 為 `REJECT_FINAL`，並記錄 `actionBy`。
    -   `members` 表的 `identityVerifiedAt` 和 `nationalIdNo` 欄位保持 `NULL` 或不變。

#### 4. 複合申請情境
-   **情境**: 一個尚未通過身分驗證的會員，直接申請成為房東。
-   **處理**: 系統應在 `approvals` 表中**同時創建兩筆**獨立的審核記錄：一筆 `moduleCode = 'IDENTITY'`，另一筆 `moduleCode = 'LANDLORD'`。會員必須兩筆申請都通過後，其身份才會正式變更為房東。

#### 5. 會員帳號停用
-   **觸發**: 管理員發現會員違規（如虛假資訊、惡意行為等）需要停用帳號。
-   **系統動作**:
    -   管理員直接設置 `members.isActive = false`。
    -   會員立即無法登入系統，所有相關功能被禁用。
    -   **審核記錄處理**：
        -   在現有的會員審核記錄中新增操作歷程（無需創建新的 `approvals` 記錄）
        -   在 `approvalItems` 表中新增停用操作記錄：
            - `actionType` 設為 `FORCE_BANNED`（與房源強制下架共用代碼）
            - `actionBy` 記錄執行管理員的 `adminID`
            - `actionNote` 記錄違規原因和停用理由
            - `snapshotJSON` 記錄停用時的會員狀態快照
    -   會員需要申請恢復帳號才能重新使用平台。（用戶介面負責開發，管理者介面只需管理 `members.isActive` 的切換）

#### 6. 會員帳號恢復
-   **觸發**: 被停用的會員提交帳號恢復申請，管理員審核通過。
-   **處理流程**:
    -   會員提交恢復申請（可能包含相關證明文件）（尚未實作，管理者介面目前只需演示 `members.isActive` 的切換）
    -   創建新的 `approvals` 記錄，`moduleCode = 'MEMBER_RECOVERY'`，`statusCode = 'PENDING'`
    -   管理員審核恢復申請
-   **系統動作**（審核通過時）:
    -   設置 `members.isActive = true`，恢復會員正常權限
    -   在 `approvalItems` 中新增恢復操作記錄：
        - `actionType` 設為 `REACTIVATED`（專用恢復代碼）
        - `actionBy` 記錄審核管理員的 `adminID`
        - `actionNote` 記錄恢復理由和審核意見
        - `snapshotJSON` 記錄恢復時的會員狀態快照

---

### 子表與補充說明

#### `approvalItems.snapshotJSON` 欄位
-   **用途**: 用於儲存審核對象在**某個操作時間點**的完整資料快照（JSON格式）。
-   **目的**: 核心是為了**稽核與追溯**。如果原始資料在審核後被修改，此快照可作為不可變的歷史證據，還原當時的資料原貌，避免業務爭議。

#### `approvalItems.actionBy` 欄位
-   **記錄規則**: 此欄位被設定為可為 `NULL`。
    -   當使用者提交申請時，對應的 `SUBMIT` 紀錄中，`actionBy` 欄位為 `NULL`。
    -   當管理員執行審核動作時，對應的 `APPROVED` 或 `REJECT_FINAL` 紀錄中，`actionBy` 欄位記錄該管理員的 `admins.adminID`。

#### `members.phoneVerifiedAt` 欄位
-   **業務規則**: 業務上要求所有會員都必須通過手機驗證。
-   **設計彈性**: 資料庫中此欄位設計為 `NULLable`，是為了提供資料庫層面的彈性，實際的非空約束由應用程式層保證。

---

## 10種會員驗證情境示例

以下為完整的會員驗證與審核模組測試案例，涵蓋所有可能的業務情境：

### 情境1: 一般會員，身分證未驗證
**會員狀態**: 
- `identityVerifiedAt`: NULL
- `isLandlord`: false  
- `memberTypeID`: 1
- `nationalIdNo`: NULL

**審核記錄**: 無相關審核記錄
**業務含義**: 新註冊會員，尚未提交任何驗證申請

### 情境2: 一般會員，提出身分證驗證申請  
**會員狀態**:
- `identityVerifiedAt`: NULL
- `isLandlord`: false
- `memberTypeID`: 1  
- `nationalIdNo`: NULL

**審核記錄**: 
- `approvals`: moduleCode='IDENTITY', statusCode='PENDING'
- `approvalItems`: actionType='SUBMIT' (actionBy=NULL)
- `userUploads`: 
  - moduleCode='MemberInfo'
  - uploadTypeCode='USER_ID_FRONT' (身分證正面)
  - uploadTypeCode='USER_ID_BACK' (身分證反面)
  - 兩筆檔案記錄皆與 approvalID 關聯

**業務含義**: 會員已提交身分證驗證申請及相關證件，等待管理員審核

### 情境3: 一般會員，身分證驗證通過
**會員狀態**:
- `identityVerifiedAt`: 有值 (審核通過時間)
- `isLandlord`: false
- `memberTypeID`: 1
- `nationalIdNo`: 有值 (管理員審核時填入)

**審核記錄**:
- `approvals`: moduleCode='IDENTITY', statusCode='APPROVED'  
- `approvalItems`: 
  - actionType='SUBMIT' (actionBy=NULL)
  - actionType='APPROVED' (actionBy=管理員ID)

**業務含義**: 身分驗證完成，可享有完整會員權限

### 情境4: 一般會員，身分證驗證被拒絕
**會員狀態**:
- `identityVerifiedAt`: NULL
- `isLandlord`: false
- `memberTypeID`: 1
- `nationalIdNo`: NULL

**審核記錄**:
- `approvals`: moduleCode='IDENTITY', statusCode='REJECTED'
- `approvalItems`:
  - actionType='SUBMIT' (actionBy=NULL)  
  - actionType='REJECT_FINAL' (actionBy=管理員ID)

**業務含義**: 身分驗證失敗，需重新提交正確資料

### 情境5: 已驗證身分證的會員，申請成為房東
**會員狀態**:
- `identityVerifiedAt`: 有值 (之前已驗證)
- `isLandlord`: false → true (申請通過後更新)
- `memberTypeID`: 1 → 2 (申請通過後更新)
- `nationalIdNo`: 有值

**審核記錄**:
- 歷史身分驗證: moduleCode='IDENTITY', statusCode='APPROVED'
- 房東申請: moduleCode='LANDLORD', statusCode='APPROVED'
- 對應的 `approvalItems` 操作歷程

**業務含義**: 已驗證會員升級為房東，可刊登房源

### 情境6: 未驗證身分證的會員，申請房東且驗證成功 (複合申請)
**會員狀態**:
- `identityVerifiedAt`: NULL → 有值
- `isLandlord`: false → true  
- `memberTypeID`: 1 → 2
- `nationalIdNo`: NULL → 有值

**審核記錄**:
- `approvals`: 兩筆記錄
  - moduleCode='IDENTITY', statusCode='APPROVED'
  - moduleCode='LANDLORD', statusCode='APPROVED'
- 每筆都有完整的 `approvalItems` 操作歷程

**業務含義**: 同時完成身分驗證和房東資格審核

### 情境7: 未驗證身分證的會員，申請房東但驗證失敗 (複合申請)
**會員狀態**:
- `identityVerifiedAt`: NULL
- `isLandlord`: false
- `memberTypeID`: 1
- `nationalIdNo`: NULL

**審核記錄**:
- `approvals`: 兩筆記錄
  - moduleCode='IDENTITY', statusCode='REJECTED'  
  - moduleCode='LANDLORD', statusCode='REJECTED'
- 每筆都有對應的拒絕操作歷程

**業務含義**: 因身分驗證失敗，房東申請一併被拒絕

### 情境8: 房東會員 (完整驗證)
**會員狀態**:
- `identityVerifiedAt`: 有值
- `isLandlord`: true
- `memberTypeID`: 2  
- `nationalIdNo`: 有值

**審核記錄**:
- 歷史完整審核記錄:
  - moduleCode='IDENTITY', statusCode='APPROVED'
  - moduleCode='LANDLORD', statusCode='APPROVED'
- 完整的操作歷程追蹤

**業務含義**: 具備完整房東權限，可進行所有房東相關操作

### 情境9: 會員帳號被停用
**會員狀態**:
- `identityVerifiedAt`: 有值 (保持原驗證狀態)
- `isLandlord`: true/false (保持原房東狀態)
- `memberTypeID`: 2/1 (保持原會員類型)
- `nationalIdNo`: 有值 (保持原身分證號)
- `isActive`: false (帳號被停用)

**審核記錄**:
- 歷史審核記錄保持不變
- 新增停用操作記錄於 `approvalItems`:
  - actionType='FORCE_BANNED' (actionBy=管理員ID)
  - 記錄詳細違規原因和停用時的會員狀態快照

**業務含義**: 會員因違規被停用，無法登入或使用平台功能，需申請恢復

### 情境10: 會員帳號恢復
**會員狀態**:
- `identityVerifiedAt`: 有值 (恢復原驗證狀態)
- `isLandlord`: true/false (恢復原房東狀態)
- `memberTypeID`: 2/1 (恢復原會員類型)
- `nationalIdNo`: 有值 (恢復原身分證號)
- `isActive`: true (帳號已恢復)

**審核記錄**:
- 歷史所有審核記錄保持完整
- `approvals` 表不建立新紀錄
- 恢復操作記錄於 `approvalItems`:
  - actionType='REACTIVATED' (actionBy=管理員ID)
  - 記錄恢復理由和審核通過的依據

**業務含義**: 會員帳號已恢復正常，可重新享有完整平台權限

### 測試資料庫記錄

**會員資料表 (members)**:
- 會員ID 101-110 分別對應上述10種情境
- 其中會員ID 102 對應情境2：已提交身分證驗證申請
- 會員ID 109 對應情境9：帳號被停用的會員
- 會員ID 110 對應情境10：帳號已恢復的會員

**審核記錄表 (approvals)**:  
- 總計包含身分驗證、房東申請、帳號恢復等各種類型的審核記錄
- 審核ID 708：會員102的身分證驗證申請 (PENDING狀態)
- 包含 `moduleCode = 'MEMBER_RECOVERY'` 的帳號恢復審核記錄

**操作歷程表 (approvalItems)**:
- 完整追蹤每個審核的提交和結果，包含停用和恢復操作
- 支援 `actionType = 'FORCE_BANNED'` (會員停用) 和 `actionType = 'REACTIVATED'` (帳號恢復)

**檔案上傳表 (userUploads)**:
- 會員102身分證驗證申請對應的檔案記錄：
  - moduleCode='MemberInfo', uploadTypeCode='USER_ID_FRONT': 身分證正面檔案 (關聯到 approvalID 708)
  - moduleCode='MemberInfo', uploadTypeCode='USER_ID_BACK': 身分證反面檔案 (關聯到 approvalID 708)

**唯一約束驗證**:
- `UQ_approvals_member_module (moduleCode, applicantMemberID, sourcePropertyID)` 
- 成功防止重複申請，同時支援並行審核

---

## 前端介面設計建議

### 會員總表頁面設計

#### 個別會員操作介面
每個會員行提供以下操作按鈕，支援在總表頁面直接完成嚴謹的審核流程：

- **`審核身分證`** - 開啟身分證審核彈窗
- **`審核房東申請`** - 開啟房東資格審核彈窗
- **`停用帳號`** - 停用違規會員帳號
- **`恢復帳號`** - 審核會員帳號恢復申請
- **`查看詳情`** - 進入完整會員詳情頁面

#### 身分證審核彈窗設計
**三階段嚴謹驗證流程**：

1. **文件檢視區域**：
   - 展示會員上傳的身分證正反面圖片
   - 支援圖片放大查看功能，便於細節檢查
   - 顯示上傳時間與檔案品質資訊

2. **資料核對區域**：
   - 管理員根據圖片手動輸入身分證字號
   - 系統自動比對註冊姓名與身分證姓名
   - 即時格式驗證與重複性檢查

3. **審核決定區域**：
   - **`通過驗證`** 按鈕 - 更新 `members.nationalIdNo` + `identityVerifiedAt`
   - **`拒絕申請`** 按鈕 - 必須填寫詳細拒絕原因
   - 雙重確認機制防止誤操作

#### 房東申請審核彈窗設計
**前置條件檢查與資格審查**：

1. **身分驗證前置檢查**：
   - 自動檢查該會員是否已完成身分證驗證
   - 未驗證者顯示警告訊息，要求先完成身分驗證

2. **資格文件審查區域**：
   - 展示房東申請時上傳的證明文件
   - 文件類型標示與完整性檢查

3. **審核決定區域**：
   - **`通過房東申請`** - 更新 `members.isLandlord=true`, `memberTypeID=2`
   - **`拒絕房東申請`** - 必須填寫拒絕原因

#### 複合申請審核彈窗設計
**同時處理身分驗證與房東申請**：

- **雙區塊顯示**：上半部為身分證審核區域，下半部為房東資格審核區域
- **強制審核順序**：必須先完成身分證驗證，通過後才能進行房東資格審核
- **連動處理邏輯**：任一環節被拒絕，整個申請流程終止

#### 會員帳號停用彈窗設計
**違規處理與稽核記錄**：

1. **違規資訊確認**：
   - 顯示會員基本資訊和當前狀態
   - 展示相關違規證據或檢舉內容

2. **停用原因記錄**：
   - **`違規類型`** 下拉選單 - 選擇違規分類
   - **`詳細原因`** 文字輸入 - 必填，記錄具體違規行為
   - **`停用期限`** 選項 - 永久停用或暫時停用

3. **確認停用決定**：
   - **`確認停用帳號`** 按鈕 - 更新 `members.isActive=false`
   - 雙重確認機制，防止誤操作
   - 自動記錄操作歷程到 `approvalItems`

#### 會員帳號恢復審核彈窗設計
**恢復申請審核流程**：

1. **申請資訊檢視**：
   - 顯示會員原停用原因和時間
   - 展示會員提交的恢復申請內容

2. **恢復依據評估**：
   - 檢視會員提供的改善證明
   - 評估是否符合恢復條件

3. **審核決定區域**：
   - **`通過恢復申請`** - 更新 `members.isActive=true`
   - **`拒絕恢復申請`** - 必須填寫拒絕原因
   - 記錄審核意見和決定依據

#### 資料庫更新對應
**所有前端操作都有完整的資料庫欄位支援**：

- **Members表更新**：`identityVerifiedAt`, `nationalIdNo`, `isLandlord`, `memberTypeID`, `isActive`
- **Approvals表更新**：`statusCode` 變更為 `APPROVED` 或 `REJECTED`
- **ApprovalItems表記錄**：新增操作歷程，支援所有 `actionType`：
  - `SUBMIT`, `APPROVED`, `REJECT_FINAL` (驗證審核)
  - `FORCE_BANNED` (帳號停用)
  - `REACTIVATED` (帳號恢復)

#### 設計核心原則
1. **嚴謹性保障**：每個會員都必須逐一審核，不允許批次驗證操作
2. **操作效率優化**：在總表頁面透過彈窗直接完成完整審核，無需進入詳情頁
3. **資料完整性**：所有操作完整記錄到審核表，確保稽核追溯能力
4. **會員權限管理**：提供完整的會員生命週期管理，包含停用和恢復機制
5. **操作歷程追蹤**：所有管理操作都有詳細記錄，支援違規處理和帳號恢復審核