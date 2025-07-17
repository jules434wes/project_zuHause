# 後台管理 - 客服頁面需求規格書

## 1. 總體目標與設計原則

本頁面的核心目標是為管理員提供一個集中式的客服案件處理中心，使其能夠高效地檢視、回覆及管理所有來自使用者的客服請求。

設計需遵循 `Notes on Development Requirements.md` 和 `邏輯架構規劃.md` 中定義的原則：

-   **資訊聚合**: 在單一介面中匯集所有客服案件，並根據 `業務邏輯.md` 的描述，清晰標示案件來源（租約、房源、傢俱訂單）。
-   **任務導向**: 將「待處理」的案件優先顯示，並提供清晰的操作流程，讓管理員能快速完成回覆與狀態變更。
-   **符合現有規範**:
    -   視覺風格遵循現代、簡約的設計，並使用 `icon_sample.html` 中定義的圖示。
    -   所有狀態變更、回覆等重要操作，必須使用 Bootstrap Modal 進行二次確認，並設定 `backdrop: 'static'` 與 `keyboard: 'false'`。
    -   所有 Controller 和 View 應遵循 `admin_` 前綴的命名慣例。

---

## 2. 頁面佈局與功能

此功能模組將包含兩個主要頁面：**客服案件列表頁**和**客服案件詳情頁**。

### 2.1 客服案件列表頁 (`admin_customerServiceList.cshtml`)

此頁面為客服功能的主入口，以列表形式展示所有客服案件。

#### 2.1.1 列表功能

-   **篩選器 (Filters)**:
    -   **來源模組**：是來自**租約**、**審核未通過的房源**、還是**傢俱訂單**。
    -   **案件狀態**: 下拉選單，選項包含「待處理」、「處理中」、「已回覆」。(對應 `customerServiceTickets.statusCode`)
    -   **案件類別**: 下拉選單，篩選案件主旨。 (對應 `customerServiceTickets.categoryCode`)
    -   **日期範圍**: 可選擇 `createdAt` 的起訖日期。
    
-   **搜尋 (Search)**:
    -   輸入框，可依「客服單號 (`ticketId`)」、「會員姓名 (`members.memberName`)」或「會員ID (`memberId`)」進行搜尋。
-   **列表 (Table)**:
    -   **資料來源**: `customerServiceTickets`
    -   **列表欄位**:
        -   `客服單號 (ticketId)`
        -   `主旨 (subject)`
        -   `會員姓名 (members.memberName)`
        -   `關聯項目`: 根據 `propertyId`, `contractId`, `furnitureOrderId` 顯示關聯的項目ID，並提供連結跳轉至對應的詳情頁。
        -   `狀態 (statusCode)`: 使用 Badge 標籤顯示，例如 `<span class="badge text-bg-warning">處理中</span>`。
        -   `客服單建立時間(createdAt)`，並可做排序切換。
        -   `最後更新時間 (updatedAt)`，並可做排序切換。
        -   `處理人員 (admins.name)`，由後台登入人員的身份自動寫入。
    -   **互動**: 點擊任一列表行，應跳轉至該案件的**客服案件詳情頁**。

---

### 2.2 客服案件詳情頁 (`admin_customerServiceDetails.cshtml`)

此頁面用於處理單一客服案件。

#### 2.2.1 左欄：對話內容區

-   **卡片：使用者需求內容**
    -   完整顯示使用者提交的 `customerServiceTickets.ticketContent`。
-   **卡片：客服回覆區**
    -   一個 `textarea` 輸入框供管理員撰寫回覆內容。
    -   提供「插入模板」按鈕，點擊後彈出 Modal，可選擇 `adminMessageTemplates` 中的模板快速帶入內容。
    -   一個醒目的「<i class="bi bi-chat-right-text-fill"></i> 送出回覆」按鈕。
    -   送出回應前應有嚴謹的double check流程。

#### 2.2.2 右欄：案件資訊與管理區

-   **卡片：案件資訊**
    -   **客服單號**: `customerServiceTickets.ticketId`
    -   **案件狀態**: `customerServiceTickets.statusCode` (使用 Badge 顯示)
    -   **申請會員**: `members.memberName` (提供連結至會員詳情頁)
    -   **關聯項目**:
        -   若 `propertyId` 有值，顯示「房源ID: [ID]」，並提供連結。
        -   若 `contractId` 有值，顯示「合約ID: [ID]」，並提供連結。
        -   若 `furnitureOrderId` 有值，顯示「傢俱訂單ID: [ID]」，並提供連結。
    -   **申請時間**: `customerServiceTickets.createdAt`
    -   **最後回覆時間**: `customerServiceTickets.replyAt`
-   **卡片：案件管理 (核心功能)**
    -   **處理人員**:
        -   由後台登入人員ID是誰自動記錄。
    -   **變更狀態**:
        -   提供下拉選單，讓管理員可以手動變更案件狀態 (待處理/處理中/已回覆)。
        -   每次變更均需 Modal 確認。

---

## 3. 核心業務邏輯與流程

1.  **接收案件**:
    -   當使用者從前台提交客服請求，系統會新增一筆紀錄至 `customerServiceTickets`，初始狀態 `statusCode` 應為「待處理」。

2.  **回覆案件**:
    -   管理員在詳情頁點擊「送出回覆」。
    -   系統彈出二次確認 Modal。
    -   確認後，執行以下操作：
        -   將 `textarea` 的內容更新至 `customerServiceTickets.replyContent`。
        -   將 `customerServiceTickets.statusCode` 更新為「已回覆」。
        -   將 `customerServiceTickets.isResolved` 更新為 `true`。
        -   記錄當前管理員ID至 `customerServiceTickets.handledBy`。
        -   更新 `customerServiceTickets.replyAt` 和 `customerServiceTickets.updatedAt` 為當前時間。
        -   (可選) 觸發一筆 `userNotifications` 通知使用者已有回覆。

3.  **變更狀態**:
    -   管理員手動從下拉選單選擇新狀態。
    -   系統彈出二次確認 Modal。
    -   確認後，更新 `customerServiceTickets.statusCode` 和 `updatedAt`。若狀態為「已回覆」，則同步更新 `isResolved`。

---

## 4. 資料庫關聯與欄位對應

| 功能/UI元件 | 主要資料表 | 關聯資料表 | 關鍵欄位 |
| :--- | :--- | :--- | :--- |
| **列表頁 - 篩選/搜尋** | `customerServiceTickets` | `members` | `statusCode`, `categoryCode`, `createdAt`, `ticketId`, `memberId` |
| **列表頁 - 列表** | `customerServiceTickets` | `members`, `admins` | `ticketId`, `subject`, `memberName`, `propertyId`, `contractId`, `furnitureOrderId`, `statusCode`, `updatedAt`, `name` |
| **詳情頁 - 內容** | `customerServiceTickets` | `adminMessageTemplates` | `ticketContent`, `replyContent` |
| **詳情頁 - 資訊** | `customerServiceTickets` | `members` | `ticketId`, `statusCode`, `memberName`, `createdAt`, `replyAt` |
| **詳情頁 - 管理** | `customerServiceTickets` | `admins` | `handledBy`, `statusCode` |