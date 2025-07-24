# 傢俱訂單管理頁面 (admin_furniture_orders) - 規格需求書

## 1. 總覽 (Overview)

本文件旨在定義後台管理系統中的「傢俱訂單管理」功能。此頁面提供管理員一個中心化的介面，用於查看、追蹤、篩選所有用戶的傢俱訂單，並以視覺化、直覺的方式更新訂單的處理狀態。

- **主要用戶**: 網站管理員 (Admin)。
- **核心目標**: 提升訂單處理效率，確保訂單狀態的準確性，並提供與客服系統的連動。

## 2. 參考文件

本規格書基於以下文件進行規劃：

- `Reference/AllColumnDDLscript.txt`
- `Reference/業務邏輯.md`
- `Reference/邏輯架構規劃.md`
- `Reference/負責開發功能.md`
- `Reference/常用狀態碼.csv`

## 3. 功能需求 (Functional Requirements)

### 3.1 訂單列表與篩選 (Order List & Filtering)

管理員進入此頁面時，會看到一個包含所有傢俱訂單的列表。

-   **預設排序**: 最新的訂單顯示在最上方。
-   **列表顯示欄位**:
    -   `訂單編號`: 唯一的訂單識別碼。
    -   `訂購會員`: 顯示用戶名稱或ID，並可點擊連結至該用戶的「用戶詳情頁」。
    -   `訂單日期`: 訂單成立的時間，格式為 `2025-07-15 10:30:00`。
    -   `訂單狀態`: 以視覺化標籤 (Badge) 顯示目前的狀態。
    -   `總金額`: 該訂單的總價格。
    -   `操作`: 提供可執行的動作按鈕 (如：查看詳情、變更狀態)。
-   **搜尋與篩選功能**:
    -   **關鍵字搜尋**: 可輸入訂單編號、會員名稱進行模糊搜尋，為一個text input與一個下拉選單的組合，使用下拉選單搜尋指定欄位。
    -   **狀態篩選**: 提供下拉選單或按鈕組，可篩選一種或多種訂單狀態 (待處理, 備貨中, 已出貨, 已送達)。
    -   **日期篩選**: 提供日期區間選擇器 (Date Range Picker)，篩選特定時間範圍內的訂單。

### 3.2 訂單狀態管理 (Order Status Management)

這是此功能的核心，目標是提供一個高度視覺化且操作簡易的狀態變更介面。

-   **狀態定義** (根據 `常用狀態碼.csv` 的 `FURNI_ORDER_STATUS` 類別):
    1.  **待處理 (PENDING)**: 用戶下單後的初始狀態。
    2.  **備貨中 (PROCESSING)**: 已確認訂單，正在進行揀貨、包裝。
    3.  **已出貨 (SHIPPED)**: 商品已交由物流運送。
    4.  **已送達 (DELIVERED)**: 用戶確認收到商品或系統自動完成。
-   **視覺化操作介面**:
    -   顯示要進入訂單下一個狀態的變化鈕，比如：待處理的訂單顯示"轉換備貨中"button，備貨中的訂單顯示"確定出貨"button，已出貨訂單顯示"確認送達"button。
    -   當訂單轉換狀態時，系統應彈出一個確認 Modal。
    
-   **操作確認 (Confirmation)**:
    -   所有狀態變更操作都必須有確認步驟。
    -   使用 Bootstrap Modal 彈窗進行二次確認 (Double Check)。
    -   Modal 應設定 `backdrop: 'static'` 和 `keyboard: false`，防止意外關閉。
    -   Modal 中應清楚顯示：「您確定要將訂單 #12345 的狀態從 '待處理' 更新為 '備貨中' 嗎？」。

### 3.3 關聯操作 (Associated Actions)

-   **客服案件**:
    -   如果用戶針對指定訂單發出客服，應於訂單上顯示"前往處理"的button
    -   點擊後，會跳轉至客服系統頁面，並自動帶入該筆訂單的編號 (`binding 該訂單`)，方便管理員處理與此訂單相關的問題。

## 4. 視覺與使用者體驗 (Visual & UX Design)

-   **設計風格**: 遵循「現代、簡約、平面、極簡」的原則。
-   **狀態視覺化**:
    -   在列表和卡片上，使用不同顏色的標籤 (Badges) 來表示不同狀態，提高辨識度。
        -   **待處理 (PENDING)**: 藍色 (Primary)
        -   **備貨中 (PROCESSING)**: 橘色 (Warning)
        -   **已出貨 (SHIPPED)**: 青色 (Info)
        -   **已送達 (DELIVERED)**: 綠色 (Success)
        -   **已取消/退貨 (可選)**: 紅色 (Danger)
-   **UX設計**:
    -   button用outline button以區分出badge與button，outline button避免使用黃色等難以辨別的色調。
    -   所有操作（如：狀態更新、篩選）後，介面應提供即時、流暢的回饋。

## 5. 資料庫欄位 (Database Columns)

以下為根據業務邏輯推斷的 `FurnitureOrders` 資料表可能欄位，最終應以 `AllColumnDDLscript.txt` 的定義為準。

-   `OrderId` (PK, int, auto-increment)
-   `UserId` (FK, int) - 關聯至 `Users` 表
-   `OrderDate` (datetime)
-   `Status` (varchar or int) - 儲存訂單狀態
-   `TotalPrice` (decimal)
-   `ShippingAddress` (nvarchar)
-   `LastUpdatedBy` (int) - 記錄最後操作的管理員ID
-   `LastUpdatedDate` (datetime)

## 6. Controller 與 View

-   **Controller**: `AdminOrdersController`
-   **Actions**:
    -   `Index()`: 顯示訂單列表與看板視圖。
    -   `GetOrders(filterParams)`: 提供給前端 AJAX 調用，用於搜尋和篩選。
    -   `UpdateStatus(orderId, newStatus)`: 處理狀態變更的邏輯。
-   **View**: `~/Views/Admin/admin_furniture_orders.cshtml`
-   **Partial Views**: 可將列表、看板、篩選器等拆分為部分視圖，方便管理。

