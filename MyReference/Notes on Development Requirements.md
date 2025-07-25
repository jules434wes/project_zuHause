## Notes on Development Requirements

### 格式化與命名

- 前端顯示日期格式必須遵照：2025-07-08 01:22:30
- C#語言命名原則統一使用小駝峰命名法（Camel Case）
- 命名規則：使用Model-View-Controller (MVC)架構慣例的命名原則。
- 由於本專案是一個複合式應用程式 (Hybrid Application)，其特色在於同時包含了供一般使用者（User-facing / Public） 存取的前台網站 (Forestage / Client Portal)，以及供經營者或管理員（Administrator / Management） 專用的後台管理系統 (Backstage / Admin Panel)，並由不同的協作者一同進行多人開發。為了確保程式碼的清晰度 (Clarity)、避免與其他協作者命名衝突 (Naming Conflicts)，遵循以下更具體的原則：
  - 請避免使用僅僅是「user」這樣高度通用的詞彙，特別是在根目錄或頂層模組的命名上。
  - 由我負責開發的功能模塊，凡是與backstage相關的controller、views，請加上"admin_"的前綴作為區分


### 開發原則

- 視覺設計上，符合現代化（Modern Design）、簡約設計 （Simple Design）、平面化 （Flat Design）、極簡風（Minimalism）
- 使用者操作體驗上，符合以下原則：
  - 直覺式設計 （Intuitive Design）：按鈕和連結的視覺回饋明確 （Visual Feedback）、導航結構清晰 （Clear Navigation）、符合使用者習慣的操作邏輯
  - 內容導向設計 （Content-first Design）：重要資訊優先顯示 （Information Hierarchy）、減少使用者認知負擔 （Cognitive Load）、內容易於掃描和理解 （Scannable Content）
  - 一目了然的操作方式：主要功能按鈕醒目易見 （Prominent CTAs）、表單欄位標籤清楚 （Clear Form Labels）、錯誤訊息具體明確 （Specific Error Messages）
- 開發方針：
  - 最小可行方案 （MVP - Minimum Viable Product）：核心功能優先開發、避免過度設計 （Over-engineering）、快速迭代驗證 （Rapid Prototyping）
  - 程式碼風格：簡潔易讀 （Clean Code）、容易維護 （Maintainable）、遵循 SOLID 原則
  - 註解規範 （Code Documentation）：複雜方法加上 XML 文檔註解、複雜邏輯加上行內註解說明、重要業務邏輯加上說明註解


### 其他須知 Other Notes

1. 管理員登入功能為必要前置條件 （Administrator Login as a Mandatory Pre-requisite）

   - 所有您目前描述或未來將描述的功能，都必須在管理員成功登入 （Administrator Login） 系統後才能執行和存取。

   - 這表示在開發您負責的功能模組時，請預期會有一個登入驗證機制 （Authentication Mechanism）。


2. 登入模組由其他協作開發者負責 （Login Module Developed by Other Collaborators）

   - 目前此專案的根目錄 （Root Directory） 中不包含 （Does Not Contain） 登入功能模組的相關程式碼。

   - 這個登入模組 （Login Module） 將由其他協作開發者 （Other Collaborating Developers） 負責開發並整合。


   - 在您開始開發時，請考慮到未來需要與這個外部提供的登入模組進行整合 （Integration）。這可能涉及到使用者會話管理 （User Session Management） 或權限驗證 （Authorization / Permission Checking） 的介面。
   - 所有「驗證」都是嚴謹的，應該使用Bootstrap的Modal彈窗，且將backdrop選項設定為 'static'、keyboard 選項設定為 'false'，按下確定還需要有double check的機會