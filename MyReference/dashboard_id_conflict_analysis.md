# Dashboard 模組 ID 衝突分析與修復建議

## 1. 問題描述

在 Dashboard 的「平台圖片與文字資料管理」區塊中，當「公告管理」與「後台訊息模板管理」兩個功能分頁 (Tab) 同時被載入時，會發生 ID 衝突。這導致了第二次點擊任一模組的「預覽」按鈕時，對應的 Modal (彈出視窗) 無法順利彈出的問題。

## 2. 根本原因分析

問題的根源在於**DOM (Document Object Model) 中存在重複的 `id` 屬性**。

當兩個模組的內容被同時載入到同一個頁面時，它們各自的 HTML 結構（包含 Modal 視窗）也同時存在於 DOM 中。如果這兩個模組使用了完全相同的 `id` 來標識它們的預覽視窗（例如，都使用了 `id="previewModal"`），瀏覽器的 JavaScript 選擇器 (`document.getElementById` 或 `$('#...')`) 在尋找元素時只會回傳**第一個**匹配的結果。

這導致第二次操作總是試圖去控制第一個已經被初始化過的 Modal 元素，而不是它自己對應的那個，最終因內部狀態混亂或腳本錯誤而導致功能失效。

### 2.1. 已確認的衝突 ID 列表

| 衝突的 ID 名稱 | 所在模組 | 功能描述 |
| :--- | :--- | :--- |
| `loadingRow` | 公告管理 & 模板管理 | 表格中的「載入中」提示列 |
| `paginationList` | 公告管理 & 模板管理 | 分頁的 `<ul>` 元素 |
| `previewModal` | 公告管理 & 模板管理 | **預覽內容的 Modal 視窗 (最關鍵的衝突點)** |
| `previewModalLabel` | 公告管理 & 模板管理 | 預覽 Modal 的標題 |
| `previewContent` | 公告管理 & 模板管理 | 預覽 Modal 內用於顯示內容的 `<div>` |

---

## 3. 列點式改進建議

核心修改原則是：**為每個模組的所有 `id` 和相關的 JavaScript 變數/函式，都加上一個獨一無二的前綴 (Prefix)**，以確保它們在全域環境中的唯一性。

-   **公告管理模組**：建議使用 `announcement-` 作為前綴。
-   **訊息模板管理模組**：建議使用 `template-` 作為前綴。

### 3.1. `message_template_management.cshtml` (模板管理 View) 修改建議

1.  **【首要】修復預覽 Modal 衝突**：
    -   將 `<div class="modal" id="previewModal">` 修改為 `<div class="modal" id="template-previewModal">`。
    -   同步將其內部的 `id="previewModalLabel"` 改為 `id="template-previewModalLabel"`。
    -   同步將其內部的 `id="previewContent"` 改為 `id="template-previewContent"`。

2.  **修改新增/編輯 Modal 的 ID**：
    -   將 `id="templateModal"` 修改為 `id="template-editModal"`，使其語意更清晰。
    -   將 `id="templateModalLabel"` 修改為 `id="template-editModalLabel"`。

3.  **為所有其他功能性 ID 加上前綴**：
    -   `addTemplateBtn` → `template-addBtn`
    -   `templateSearchInput` → `template-searchInput`
    -   `templateTableBody` → `template-tableBody`
    -   `paginationList` → `template-pagination`
    -   `loadingRow` → `template-loadingRow`
    -   `saveTemplateBtn` → `template-saveBtn`
    -   ...以此類推，對檔案中所有 `id` 進行系統性修改。

### 3.2. `announcement_management.cshtml` (公告管理 View) 修改建議

1.  **【首要】修復預覽 Modal 衝突**：
    -   將 `<div class="modal" id="previewModal">` 修改為 `<div class="modal" id="announcement-previewModal">`。
    -   同步將其內部的 `id="previewModalLabel"` 改為 `id="announcement-previewModalLabel"`。
    -   同步將其內部的 `id="previewContent"` 改為 `id="announcement-previewContent"`。

2.  **修改新增/編輯 Modal 的 ID**：
    -   將 `id="announcementModal"` 修改為 `id="announcement-editModal"`。
    -   將 `id="announcementModalLabel"` 修改為 `id="announcement-editModalLabel"`。

3.  **為所有其他功能性 ID 加上前綴**：
    -   `addAnnouncementBtn` → `announcement-addBtn`
    -   `announcementSearchInput` → `announcement-searchInput`
    -   `announcementTableBody` → `announcement-tableBody`
    -   `paginationList` → `announcement-pagination`
    -   `loadingRow` → `announcement-loadingRow`
    -   `saveAnnouncementBtn` → `announcement-saveBtn`
    -   ...以此類推，對檔案中所有 `id` 進行系統性修改。

### 3.3. `message_template_management.js` (模板管理 JS) 修改建議

-   更新所有 JavaScript 中的選擇器，使其能對應到新的 `template-` 前綴 ID。
    ```javascript
    // 原本
    const previewModalEl = document.getElementById('previewModal');
    
    // 修改後
    const previewModalEl = document.getElementById('template-previewModal');
    ```
-   建議將 Modal 的實例變數也加上前綴，或使用物件/閉包封裝，以避免全域變數衝突。
    ```javascript
    // 原本
    let previewModal = new bootstrap.Modal(...);

    // 修改後
    const templatePreviewModal = new bootstrap.Modal(...);
    ```

### 3.4. `announcement_management.js` (公告管理 JS) 修改建議

-   同樣地，更新所有 JavaScript 中的選擇器，使其能對應到新的 `announcement-` 前綴 ID。
    ```javascript
    // 原本
    const previewModal = new bootstrap.Modal(document.getElementById('previewModal'));

    // 修改後
    const announcementPreviewModal = new bootstrap.Modal(document.getElementById('announcement-previewModal'));
    ```
-   將相關的 JavaScript 變數和函式也加上 `announcement_` 前綴或進行封裝。

## 4. 結論

透過以上**系統性的前綴添加**，可以徹底解決因 `id` 衝突導致的 Modal 無法正常顯示問題。當修改完成後，兩個模組的預覽功能將能獨立運作、互不干擾。

此修改不僅是修復一個 Bug，更是提升前端程式碼健壯性、可讀性與長期可維護性的重要實踐。
