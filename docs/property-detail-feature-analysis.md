# 房源詳細資訊頁面 - 功能分析

## 原始設計來源
- 來源：https://preview-housing-rental-webpage-kzmgnlwe0amevnoogj7d.vusercontent.net/
- 頁面標題：房源詳細資訊頁面
- 轉換目標：完全重構為 .NET 8 + Bootstrap 5 架構

## 技術規格
- 後端：.NET 8 + ASP.NET Core MVC + Entity Framework Core
- 前端：Bootstrap 5 + 現代 ES6+ Vanilla JavaScript
- 架構：ViewModel + View Components + Tag Helpers
- 功能：多語言支援 + 快取機制 + 響應式設計

## 詳細功能清單

### 1. 導覽系統
#### 1.1 主導覽列
- [x] 固定於頂部
- [x] 可後期替換結構（其他團隊成員開發）
- [ ] 響應式收合功能

#### 1.2 滾動觸發子導覽
- [ ] 滾動時在主導覽下方展開
- [ ] 緊貼主導覽列，隨頁面滾動保持固定
- [ ] 導覽項目：「房屋資訊」「費用及守則」「設備與服務」「描述」「位置」
- [ ] 輕微陰影效果
- [ ] 當前區塊高亮顯示
- [ ] 平滑滾動到對應錨點

### 2. 房屋標題區域
- [ ] 房屋標題顯示
- [ ] 分享功能按鈕
- [ ] 收藏功能按鈕
- [ ] 小選單（展開顯示「回報此房源問題」）

### 3. 圖片展示系統
#### 3.1 主圖區域
- [ ] 一張主圖顯示
- [ ] 點擊主圖放大檢視
- [ ] 左右箭頭切換圖片
- [ ] 圖片懶加載

#### 3.2 縮圖區域
- [ ] 其他圖片縮圖顯示
- [ ] 點選縮圖切換主圖
- [ ] 當前圖片高亮顯示

### 4. 右側資訊面板
#### 4.1 卡片樣式
- [ ] 卡片形式設計
- [ ] 輕微陰影效果
- [ ] 隨頁面滾動跟隨移動

#### 4.2 房屋基本資訊
- [ ] 租金顯示
- [ ] 地址顯示
- [ ] 格局資訊顯示

#### 4.3 房東資訊
- [ ] 房東基本資料顯示
- [ ] 房東聯絡資訊

#### 4.4 操作按鈕
- [ ] 立即申租按鈕（特殊按鈕：#e95d54 背景，#ffffff 文字）
- [ ] 預約看房按鈕（主要按鈕：#1a7482 背景，#ffffff 文字）
- [ ] 立即聯絡按鈕（次要按鈕：#ffffff 背景，#67bfa3 框線和文字）

### 5. 房屋詳細資訊區塊
#### 5.1 房屋資訊
- [ ] 基本房屋資訊展示
- [ ] 類別標題列（背景色：#eff9f6）

#### 5.2 費用及守則
- [ ] 費用明細顯示
- [ ] 租房守則說明
- [ ] 類別標題列（背景色：#eff9f6）

#### 5.3 設備與服務
- [ ] 最多三欄顯示
- [ ] 「其他」類別無限制欄數
- [ ] 每欄最多三個小主題
- [ ] 超過三個自動換欄
- [ ] 類別標題列（背景色：#eff9f6）

#### 5.4 描述
- [ ] 房屋詳細描述
- [ ] 類別標題列（背景色：#eff9f6）

#### 5.5 位置
- [ ] 地圖顯示
- [ ] 位置相關資訊
- [ ] 類別標題列（背景色：#eff9f6）

### 6. 響應式設計
#### 6.1 Bootstrap 5 斷點
- [ ] xs (extra small)
- [ ] sm (small)
- [ ] md (medium)
- [ ] lg (large)
- [ ] xl (extra large)
- [ ] xxl (extra extra large)

#### 6.2 裝置適配
- [ ] 手機版：收合為 offcanvas 或 dropdown
- [ ] 平板版：水平滾動導覽
- [ ] 桌面版：完整展開

### 7. 進階功能
#### 7.1 多語言支援
- [ ] IStringLocalizer 整合
- [ ] 所有可見文字本地化

#### 7.2 快取機制
- [ ] IMemoryCache 整合
- [ ] 房屋資料快取

#### 7.3 Configuration 系統
- [ ] 導覽設定讀取
- [ ] 樣式設定讀取

#### 7.4 Entity Framework 整合
- [ ] 動態載入房屋資料
- [ ] PropertyDetailViewModel 填充

#### 7.5 JavaScript 進階功能
- [ ] ES6 modules 結構
- [ ] Intersection Observer API
- [ ] Lazy loading
- [ ] 防抖動處理

#### 7.6 建置優化
- [ ] Bundling 和 Minification
- [ ] SCSS 變數系統
- [ ] CSS 優化

## ViewComponent 架構設計

### 核心 ViewComponents
1. **ScrollNavigationViewComponent** - 滾動觸發導覽
2. **PropertyImageGalleryViewComponent** - 圖片展示系統
3. **PropertyInfoPanelViewComponent** - 右側資訊面板
4. **PropertyDetailsViewComponent** - 房屋詳細資訊
5. **PropertyFeaturesViewComponent** - 設備與服務

### ViewModel 結構
1. **PropertyDetailViewModel** - 主要頁面 ViewModel
2. **PropertyImageViewModel** - 圖片相關 ViewModel
3. **PropertyInfoViewModel** - 房屋資訊 ViewModel
4. **PropertyFeatureViewModel** - 設備服務 ViewModel

## 檔案結構規劃

```
zuHause/
├── Controllers/
│   └── PropertyController.cs
├── ViewModels/
│   ├── PropertyDetailViewModel.cs
│   ├── PropertyImageViewModel.cs
│   └── PropertyInfoViewModel.cs
├── Components/
│   ├── ScrollNavigationViewComponent.cs
│   ├── PropertyImageGalleryViewComponent.cs
│   ├── PropertyInfoPanelViewComponent.cs
│   ├── PropertyDetailsViewComponent.cs
│   └── PropertyFeaturesViewComponent.cs
├── Views/
│   ├── Property/
│   │   └── Detail.cshtml
│   └── Shared/
│       └── Components/
│           ├── ScrollNavigation/
│           ├── PropertyImageGallery/
│           ├── PropertyInfoPanel/
│           ├── PropertyDetails/
│           └── PropertyFeatures/
├── wwwroot/
│   ├── css/
│   │   ├── property-detail.scss
│   │   └── components/
│   ├── js/
│   │   ├── modules/
│   │   │   ├── scroll-navigation.js
│   │   │   ├── image-gallery.js
│   │   │   └── property-interactions.js
│   │   └── property-detail.js
│   └── images/
└── TagHelpers/
    ├── PropertyNavigationTagHelper.cs
    └── PropertyButtonTagHelper.cs
```

## 驗收標準
- [ ] 資料庫正確建立並可執行 Migration
- [ ] 基礎頁面可正常載入
- [ ] Bootstrap 5 樣式正確引用
- [ ] 所有功能符合原始設計需求
- [ ] 響應式設計在所有斷點正常運作
- [ ] JavaScript 功能流暢無卡頓
- [ ] ViewComponent 架構清晰且可重用
- [ ] 多語言和快取功能正確整合