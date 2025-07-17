# 後台管理 - 系統訊息管理模組需求規格書

## 1. 總體目標與設計原則

本模組的核心目標是提供一個強大且靈活的介面，讓管理員能夠向不同群體的用戶（全體會員、全體房東、或個別用戶）發送系統訊息。這些訊息將顯示在使用者的前台通知中心，作為平台與用戶溝通的重要渠道。

設計需嚴格遵循 `邏輯架構規劃.md` 和 `業務邏輯.md` 中定義的原則：

-   **目標導向**: 介面設計需清晰、直觀，讓管理員能快速完成訊息的撰寫與發送。
-   **權責分離**: 訊息的發送、記錄與前台的接收通知應分離處理，後台僅負責訊息的建立與發送觸發。
-   **符合現有規範**:
    -   視覺風格遵循現代、簡約的設計。
    -   所有關鍵操作（如發送訊息）必須使用 Bootstrap Modal 進行二次確認，並設定 `backdrop: 'static'` 與 `keyboard: 'false'`，以防止誤操作。
    -   所有相關的 Controller 和 View 應遵循 `admin_` 前綴的命名慣例。

---

## 2. 頁面佈局與功能

此功能模組將包含兩個主要頁面：**系統訊息列表頁**和**新增系統訊息頁**。

### 2.1 系統訊息列表頁 (`admin_systemMessageList.cshtml`)

此頁面為系統訊息的管理中心，以列表形式展示所有已建立或已發送的系統訊息歷史紀錄。

#### 2.1.1 功能

-   **篩選器 (Filters)**:
    -   **發送對象**: 下拉選單，選項包含「全體會員」、「全體房東」、「個別用戶」。(對應 `systemMessages.audienceTypeCode`)
    -   **訊息分類**: 下拉選單，篩選訊息類別。(對應 `systemMessages.categoryCode`)
    -   **發送日期**: 可選擇 `sentAt` 的起訖日期。
-   **搜尋 (Search)**:
    -   輸入框，可依「訊息標題 (`title`)」或「訊息內容 (`messageContent`)」進行關鍵字搜尋。
-   **操作按鈕**:
    -   一個醒目的「<i class="bi bi-plus-circle-fill"></i> 新增訊息」按鈕，點擊後跳轉至新增頁面。
-   **列表 (Table)**:
    -   **資料來源**: `systemMessages`
    -   **列表欄位**:
        -   `訊息ID (messageID)`
        -   `標題 (title)`
        -   `發送對象 (audienceTypeCode)`: 顯示易於理解的文字，如「全體房東」。
        -   `接收者 (receiverID)`: 若為個別用戶，則顯示該用戶的姓名 (`members.memberName`) 及 ID。
        -   `發送者 (adminID)`: 顯示操作的管理員姓名 (`admins.name`)。
        -   `發送時間 (sentAt)`: 並可做排序切換。
    -   **互動**:
        -   提供「檢視」按鈕，可查看完整的訊息內容。

---

### 2.2 新增系統訊息頁 (`admin_systemMessageNew.cshtml`)

此頁面為系統訊息的建立與發送介面。**訊息一旦發送，即不可修改、刪除或撤回。**

#### 2.2.1 表單欄位

-   **發送對象 (`audienceTypeCode`)**:
    -   使用 Radio Button 或下拉選單提供以下選項：
        -   `全體會員`
        -   `全體房東`
        -   `個別用戶`
-   **個別用戶選擇器**:
    -   當「發送對象」選擇「個別用戶」時，此欄位才會出現並變為必填。
    -   應為一個具備自動完成 (Autocomplete) 功能的搜尋框，可輸入用戶姓名或 `memberID` 來快速查找並選定單一用戶。
-   **訊息分類 (`categoryCode`)**:
    -   下拉選單，選項來自 `systemCodes` 資料表中 `codeCategory` 為 'MessageCategory' 的項目 (例如：「系統公告」、「優惠活動」、「維護通知」)。
-   **訊息標題 (`title`)**:
    -   文字輸入框，為訊息的主要標題。
-   **訊息內容 (`messageContent`)**:
    -   應使用富文本編輯器 (Rich Text Editor)，允許管理員進行基本的文字排版。
-   **附件 (`attachmentUrl`)**:
    -   非必填欄位，提供檔案上傳功能，用於附加圖片或文件。

#### 2.2.2 操作按鈕

-   一個主要的「<i class="bi bi-send-fill"></i> 發送訊息」按鈕。
-   一個「取消」按鈕，點擊後返回列表頁。

---

## 3. 核心業務邏輯與流程

1.  **建立與發送訊息**:
    -   管理員進入新增頁面，填寫所有必填欄位後，點擊「發送訊息」按鈕。
    -   系統彈出二次確認 Modal，內容需明確提示發送的對象與後果，例如：「您確定要發送此訊息給 **全體會員** 嗎？**此操作無法復原、刪除或撤回。**」
    -   管理員點擊 Modal 中的「確認發送」按鈕。
    -   後端執行以下操作：
        1.  在 `systemMessages` 資料表中新增一筆紀錄，包含表單中的所有資訊，並記錄當前的 `adminID` 和 `sentAt` 時間。此紀錄為永久保存，不應被刪除。
        2.  根據 `audienceTypeCode` 決定接收者列表：
            -   若為「全體會員」，則查詢 `members` 資料表獲取所有 `isActive = 1` 的 `memberID`。
            -   若為「全體房東」，則查詢 `members` 資料表獲取所有 `isLandlord = 1` 且 `isActive = 1` 的 `memberID`。
            -   若為「個別用戶」，則直接使用表單中選定的 `receiverID`。
        3.  **針對接收者列表中的每一個 `memberID`**，在 `userNotifications` 資料表中新增一筆對應的通知紀錄。此紀錄應包含訊息的標題、內容、連結等資訊，並將 `isRead` 設為 `false`。這些通知紀錄同樣為永久性的。
        4.  操作完成後，將管理員導向至訊息列表頁，並顯示操作成功的提示訊息。

---

## 4. 資料庫關聯與欄位對應

| 功能/UI元件 | 主要資料表 | 關聯資料表 | 關鍵欄位 |
| :--- | :--- | :--- | :--- |
| **列表頁 - 篩選/搜尋** | `systemMessages` | - | `audienceTypeCode`, `categoryCode`, `sentAt`, `title`, `messageContent` |
| **列表頁 - 列表** | `systemMessages` | `members`, `admins` | `messageID`, `title`, `audienceTypeCode`, `receiverID`, `adminID`, `sentAt` |
| **新增頁 - 表單** | `systemMessages` | `members`, `systemCodes` | `audienceTypeCode`, `receiverID`, `categoryCode`, `title`, `messageContent`, `attachmentUrl` |
| **發送邏輯** | `systemMessages` | `members`, `userNotifications` | `memberID`, `isLandlord`, `notificationContent`, `isRead` |
