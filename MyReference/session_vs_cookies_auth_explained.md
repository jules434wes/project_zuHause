# 登入驗證：Session vs. Cookie 全解析

本文旨在深入探討網站開發中兩種核心的登入驗證與狀態管理機制：Session 和 Cookie。我們將分析它們的運作原理、應用場景、優缺點，並結合 `zuHause` 專案中的實踐，提供一份清晰的比較與說明。

## 總覽：核心差異

| 特性 | Cookie | Session |
| :--- | :--- | :--- |
| **儲存位置** | **客戶端** (使用者瀏覽器) | **伺服器端** |
| **儲存內容** | 少量文字資料 (通常加密或簽章的識別碼) | 任何型別的資料 (使用者物件、購物車、臨時狀態) |
| **安全性** | **較低** (資料暴露在客戶端，易被竊取或竄改) | **較高** (實際資料儲存在伺服器，客戶端只有一個Session ID) |
| **效能影響** | 對伺服器幾乎無影響 | 佔用伺服器記憶體或儲存空間，使用者過多時可能影響效能 |
| **生命週期** | 可設定為瀏覽器關閉後失效，或設定具體的過期時間 | 通常有閒置超時 (Idle Timeout)，或在伺服器端手動清除 |
| **跨網域** | 可透過設定實現跨子網域共享 | 預設無法跨網域，因為 Session ID 與特定伺服器綁定 |

---

## 1. Cookie-based Authentication (基於 Cookie 的驗證)

Cookie 是一種由伺服器傳送給使用者瀏覽器並儲存在本地的**小型文字檔案**。當瀏覽器下次向同一伺服器發出請求時，會自動帶上這個 Cookie，讓伺服器能夠識別使用者身份。

### 運作流程

1.  **登入**：使用者提交帳號密碼。
2.  **驗證**：伺服器驗證資料無誤。
3.  **簽發 Cookie**：伺服器生成一個包含使用者資訊 (如使用者ID、角色等) 的加密或簽章過的字串，並將其作為 Cookie 傳送給瀏覽器。
4.  **儲存 Cookie**：瀏覽器將此 Cookie 儲存起來。
5.  **後續請求**：瀏覽器向伺服器發送任何請求時，都會自動附上這個 Cookie。
6.  **伺服器驗證**：伺服器收到請求後，解密或驗證 Cookie 的簽章，確認使用者身份與權限，然後回傳相應資源。

### `zuHause` 專案中的應用

在 `Program.cs` 中，我們明確看到了兩種基於 Cookie 的驗證設定：

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MemberCookieAuth";
})
.AddCookie("MemberCookieAuth", options => // <<-- 會員使用的 Cookie
{
    options.LoginPath = "/Member/Login";
    options.AccessDeniedPath = "/Member/AccessDenied";
})
.AddCookie("AdminCookies", options => // <<-- 管理員使用的 Cookie
{
    options.LoginPath = "/Auth/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // 8 小時後過期
    options.SlidingExpiration = true; // 若使用者有活動，則自動延長過期時間
});
```

- **`MemberCookieAuth`**：用於一般會員的登入。
- **`AdminCookies`**：用於管理員登入，並設定了 8 小時的有效期限與滑動到期，提供了更好的安全性與使用者體驗。

### 優點

- **無狀態 (Stateless)**：伺服器不需要儲存任何使用者的登入資訊，每次請求都由 Cookie 自我驗證。這使得伺服器擴展 (Scale-out) 變得非常容易。
- **伺服器效能佳**：驗證邏輯僅涉及加解密運算，不消耗伺服器記憶體來儲存使用者狀態。
- **實作簡單**：ASP.NET Core 提供了強大的內建支援，設定直觀。

### 缺點

- **安全性較低**：
    - **XSS (跨站腳本)**：如果網站有 XSS 漏洞，攻擊者可能透過 JavaScript 竊取 Cookie。
    - **CSRF (跨站請求偽造)**：攻擊者可能誘騙使用者點擊惡意連結，利用瀏覽器自動發送 Cookie 的特性，冒用使用者身份執行操作。
- **儲存容量限制**：大多數瀏覽器限制 Cookie 大小約為 4KB，無法儲存複雜資料。
- **網路傳輸**：每次請求都會攜帶 Cookie，如果 Cookie 內容過大，會增加網路流量。

---

## 2. Session-based Authentication (基於 Session 的驗證)

Session 是一種在**伺服器端**儲存使用者資訊的機制。它彌補了 HTTP 協定的無狀態性，讓伺服器能夠「記住」特定使用者的狀態。

### 運作流程

1.  **登入**：使用者提交帳號密碼。
2.  **驗證**：伺服器驗證資料無誤。
3.  **建立 Session**：伺服器為該使用者建立一個專屬的 Session 物件，並在其中儲存資料 (如使用者ID、權限、購物車內容等)。同時，生成一個獨一無二的 **Session ID**。
4.  **傳送 Session ID**：伺服器將這個 Session ID 作為一個 Cookie (通常稱為 `session cookie`) 傳送給瀏覽器。
5.  **儲存 Session ID**：瀏覽器儲存這個 Session ID Cookie。
6.  **後續請求**：瀏覽器向伺服器發送請求時，會附上這個 Session ID Cookie。
7.  **伺服器查找**：伺服器根據 Session ID 找到對應的 Session 物件，從而得知使用者身份與狀態，並回傳資源。

### `zuHause` 專案中的應用

`Program.cs` 中也啟用了 Session 服務：

```csharp
// === 新增 Session 服務配置 ===
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 閒置 30 分鐘後 Session 過期
    options.Cookie.HttpOnly = true; // 防止客戶端腳本存取 Session Cookie
    options.Cookie.IsEssential = true; // 確保 Session 機制正常運作
});

// ... 中間件 ...
app.UseSession(); // 啟用 Session 中間件
```

這段設定表明，`zuHause` 專案可以利用 Session 來儲存一些**臨時性、非身份核心**的資料，例如：
- 多步驟表單的暫存資料。
- 使用者搜尋條件或篩選器偏好。
- 顯示給使用者的臨時通知訊息。

### 優點

- **安全性高**：
    - 敏感資料儲存在伺服器，客戶端只有一個無意義的 Session ID。
    - 即使 Session ID 被竊取，攻擊者也無法直接讀取其中的資料。
    - 伺服器可以主動銷毀 Session，強制使用者登出。
- **儲存容量大**：可以儲存任意大小和型別的複雜物件，只受伺服器記憶體/儲存空間限制。
- **不增加網路負擔**：客戶端只需傳輸一個簡短的 Session ID。

### 缺點

- **佔用伺服器資源**：每個使用者的 Session 都會消耗伺服器的記憶體或儲存空間。當線上使用者數量龐大時，會對伺服器造成壓力。
- **不易擴展 (Stateful)**：Session 預設儲存在單一伺服器的記憶體中。如果系統需要水平擴展到多台伺服器，必須額外處理 Session 共享問題 (例如使用 Sticky Sessions、分散式快取如 Redis 等)，增加了架構的複雜性。

---

## 結論：`zuHause` 的混合策略

`zuHause` 專案採用的是一種**混合策略**，這在現代 Web 應用中非常普遍且高效：

1.  **使用 Cookie 進行身份驗證**：
    - 利用 Cookie 的無狀態特性，讓身份驗證機制變得輕量且易於擴展。
    - 伺服器無需為每個登入使用者維護一個持續的記錄，降低了伺服器負擔。
    - 透過加密和簽章確保 Cookie 的安全性，防止竄改。

2.  **使用 Session 儲存臨時狀態**：
    - 對於一些需要在多次請求之間共享，但又與核心身份無關的臨時資料 (如表單進度、使用者偏好設定等)，則利用 Session 儲存在伺服器端。
    - 這樣既保證了資料的安全性，也避免了將大量非敏感資訊塞入 Cookie 中，保持了 Cookie 的輕量。

這種**「用 Cookie 識別你，用 Session 記住你的事」**的模式，結合了兩者的優點，是建構安全、高效能 Web 應用的理想選擇。
