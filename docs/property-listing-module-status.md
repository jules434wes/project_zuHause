# 刊登房源模組開發狀態記錄

## 📋 當前完成狀態 (2025-07-16)

### ✅ 已完成的原子任務
1. **IImageProcessor 圖片處理核心模組** - 完成
   - 支援 JPG/PNG 轉 WebP 格式
   - 多尺寸圖片生成 (原圖、中圖、縮圖)
   - 使用 SixLabors.ImageSharp 函式庫
   - 完整單元測試覆蓋

2. **PropertyImageService 房源圖片處理服務** - 完成
   - 檔案驗證 (格式、大小、數量)
   - 多尺寸圖片生成和檔案系統儲存
   - 資料庫記錄 (PropertyImage 表)
   - 事務管理確保資料一致性
   - 10 個單元測試案例

### 🔄 下一階段任務規劃
依據 `/mnt/c/Codes/MSIT_Final/Project/zuHause/docs/250715_刊登房源頁面.md` 規劃：

1. **房源基本資料 API Controller** - 待開發
   - 建立、更新、刪除房源基本資料
   - 資料驗證和業務邏輯
   
2. **房源圖片上傳 API Controller** - 待開發
   - 整合 PropertyImageService
   - 權限驗證 (房東身份)
   - RESTful API 設計

3. **房源刊登前端頁面** - 待開發
   - 多步驟表單設計
   - 圖片拖拽上傳功能
   - 即時預覽和驗證

## ⚠️ 重要注意事項

### 開發原則
- **原子化開發**: 每個任務必須是完整的交付單元 (介面+實作+測試+DI)
- **防過度 Mocking**: 使用小而聚焦的介面設計
- **實體自主行為**: 避免貧血模型，DTO 包含自主邏輯方法

### Git 工作流程
- **衝突檢查**: 合併前必須檢查並解決衝突
- **完整流程**: 建立 Issue → 功能分支 → 實作 → PR → 合併 → 清理
- **GitHub API**: 使用 token `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`

### 技術架構
- **前後端分離**: 前端使用 Components，後端提供 Web API
- **DTO 傳輸**: 使用 DTO 而非 ViewModel
- **Service 層**: 業務邏輯在 Service 層，Controller 僅負責協調

### 測試策略
- **三層測試**: 單元測試 + 整合測試 + 端到端測試
- **真實資料庫**: 使用 Entity Framework Core，避免 Mock Repository
- **InMemory 測試**: 修正 ZuHauseContext.OnConfiguring 支援測試環境

## 📁 關鍵檔案位置
- 主要規劃文件: `/docs/250715_刊登房源頁面.md`
- 開發原則: `/.claude/master-development-principles.md`
- 任務模板: `/.claude/atomic-task-template.md`
- 防 Mocking 指南: `/.claude/anti-mocking-guidelines.md`

## 🎯 下次工作提醒
1. 先閱讀開發原則和任務模板
2. 展示完整任務計劃並等待確認
3. 執行原子化開發流程
4. 確保編譯測試通過
5. 執行完整 Git 工作流程

---
*最後更新: 2025-07-16*
*更新者: Claude Code Assistant*