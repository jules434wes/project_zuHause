# GitHub Actions 設定指南

## 已建立的工作流程

### 1. Claude 相關工作流程
- **claude.yml** - PR 助手，支援 `@claude` 提及
- **claude-auto-review.yml** - 自動 PR 審查
- **claude-pr-path-specific.yml** - 路徑特定 PR 審查
- **claude-review-from-author.yml** - 作者請求的審查

### 2. .NET CI/CD 工作流程
- **dotnet-ci.yml** - 持續整合（建置、測試）
- **azure-deploy.yml** - Azure 自動部署

## 必要的 GitHub Secrets 設定

前往您的 GitHub repository → Settings → Secrets and variables → Actions，建立以下 secrets：

### Claude 相關
```
ANTHROPIC_API_KEY = your_anthropic_api_key_here
```

### Azure 部署相關
```
AZURE_SQL_CONNECTION_STRING = Server=tcp:zuhause.database.windows.net,1433;Initial Catalog=zuHause;User ID=zuhause;Password=DB$MSIT67;...
AZURE_WEBAPP_NAME = your_azure_webapp_name
AZURE_WEBAPP_PUBLISH_PROFILE = your_azure_webapp_publish_profile_xml
```

## 工作流程觸發條件

### 自動觸發
- **Push to main/develop** → 執行 CI/CD
- **Pull Request** → 執行 CI 和 Claude 自動審查
- **PR 評論包含 @claude** → Claude 助手回應

### 手動觸發
- **Azure Deploy** → 可在 Actions 頁面手動執行

## 環境設定

### Production Environment
在 GitHub repository → Settings → Environments 建立 `production` 環境，並設定保護規則：
- 需要審查者批准
- 限制特定分支部署

## 工作流程功能

### dotnet-ci.yml
- 在 Ubuntu 環境建置 .NET 8 專案
- 執行測試
- 檢查 Entity Framework Migrations
- 產生建置產物

### azure-deploy.yml  
- 執行資料庫 Migration
- 部署到 Azure Web App
- 只在 main 分支觸發

### Claude 工作流程
- 自動審查程式碼品質
- 提供建設性回饋
- 支援互動式程式碼討論

## 使用方式

### 觸發 Claude 審查
在 PR 或 Issue 中留言：
```
@claude 請幫我審查這個程式碼
```

### 檢查工作流程狀態
- 前往 repository → Actions 頁面
- 查看各工作流程執行狀態
- 檢視詳細日誌

## 注意事項

1. **Entity Framework Migrations** 會在部署時自動執行
2. **Secrets 不會在 Fork 的 repository 中使用**
3. **Production 部署需要手動批准**
4. **確保 Azure 服務已正確設定**

## 疑難排解

### 常見問題
- **Secrets 未設定** → 檢查 repository settings
- **Azure 連線失敗** → 驗證連線字串和 publish profile
- **Migration 失敗** → 檢查資料庫權限和連線

### 工作流程除錯
1. 查看 Actions 頁面的詳細日誌
2. 檢查 Secrets 是否正確設定
3. 驗證 .NET 版本和相依性
4. 確認 Azure 服務設定