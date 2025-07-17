# 建立統一圖片管理系統 - 基於新 Images 資料表

## 📋 專案上下文
zuHause 租屋平台需要統一的圖片管理系統，取代原有的 PropertyImages 等個別資料表。新的 Images 資料表支援多種實體類型（Property、Member、Furniture、Announcement）和圖片分類，並整合 Azure Blob Storage 進行圖片儲存。

## 🎯 任務目標
建立基於新 Images 資料表的統一圖片管理系統，提供類型安全的圖片上傳、查詢服務，支援並發安全的 DisplayOrder 管理，並重構現有的 PropertyImageService 以保持向後相容性。

## 🗂️ 新 Images 資料表結構
```sql
CREATE TABLE [dbo].[Images](
    [ImageId] [bigint] IDENTITY(1,1) NOT NULL,
    [ImageGuid] [uniqueidentifier] NOT NULL,
    [EntityType] [nvarchar](50) NOT NULL,  -- Property, Member, Furniture, Announcement
    [EntityId] [int] NOT NULL,
    [Category] [nvarchar](50) NOT NULL,    -- BedRoom, Living, Kitchen, Avatar, etc.
    [MimeType] [nvarchar](50) NOT NULL,
    [OriginalFileName] [nvarchar](255) NOT NULL,
    [StoredFileName] AS (computed column),  -- {guid}.{extension}
    [FileSizeBytes] [bigint] NOT NULL,
    [Width] [int] NOT NULL,
    [Height] [int] NOT NULL,
    [DisplayOrder] [int] NULL,
    [IsActive] [bit] NOT NULL,
    [UploadedByUserId] [uniqueidentifier] NULL,
    [UploadedAt] [datetime2](7) NOT NULL,
    -- 約束和索引
)
```

## 🔧 技術規格

### 核心實體和枚舉
```csharp
public class Image
{
    public long ImageId { get; set; }
    public Guid ImageGuid { get; set; }
    public EntityType EntityType { get; set; }      // enum -> string 轉換
    public int EntityId { get; set; }
    public ImageCategory Category { get; set; }     // enum -> string 轉換
    public string MimeType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;  // 計算欄位
    public long FileSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; }
}

public enum EntityType { Property, Member, Furniture, Announcement }
public enum ImageCategory { BedRoom, Living, Kitchen, Balcony, Gallery, Avatar, Product }
```

### 核心服務介面
```csharp
// 圖片上傳服務
public interface IImageUploadService
{
    Task<List<ImageUploadResult>> UploadImagesAsync(IFormFileCollection files, EntityType entityType, int entityId, ImageCategory category, Guid? uploadedByUserId = null);
    Task<ImageUploadResult> UploadImageAsync(Stream imageStream, string originalFileName, EntityType entityType, int entityId, ImageCategory category, Guid? uploadedByUserId = null);
    Task<bool> DeleteImageAsync(long imageId);
    Task<bool> DeleteImagesByEntityAsync(EntityType entityType, int entityId);
}

// 圖片查詢服務
public interface IImageQueryService
{
    Task<List<Image>> GetImagesByEntityAsync(EntityType entityType, int entityId, ImageCategory? category = null);
    Task<Image?> GetMainImageAsync(EntityType entityType, int entityId, ImageCategory? category = null);
    Task<Image?> GetImageByIdAsync(long imageId);
    Task<string> GetImageUrlAsync(long imageId, ImageSize size = ImageSize.Original);
    Task<bool> IsMainImageAsync(long imageId);
}

// 實體存在性驗證服務
public interface IEntityExistenceChecker
{
    Task<bool> ExistsAsync(EntityType entityType, int entityId);
    Task<string> GetEntityNameAsync(EntityType entityType, int entityId);
}
```

### ImageUploadResult DTO
```csharp
public class ImageUploadResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    
    // 資料庫記錄資訊
    public long? ImageId { get; set; }
    public Guid? ImageGuid { get; set; }
    public string StoredFileName { get; set; } = string.Empty;
    
    // 檔案資訊
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // 實體關聯
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }
    public ImageCategory Category { get; set; }
    
    // URL 生成方法
    public string GetImageUrl(ImageSize size = ImageSize.Original)
    {
        if (!ImageGuid.HasValue) return string.Empty;
        
        var sizeFolder = size switch
        {
            ImageSize.Original => "original",
            ImageSize.Medium => "medium", 
            ImageSize.Thumbnail => "thumbnail",
            _ => "original"
        };
        
        return $"https://zuhauseimg.blob.core.windows.net/images/{EntityType.ToString().ToLower()}/{EntityId}/{sizeFolder}/{StoredFileName}";
    }
    
    // 靜態工廠方法
    public static ImageUploadResult Success(...) => new ImageUploadResult { ... };
    public static ImageUploadResult Failure(string originalFileName, string errorMessage, string errorCode) => new ImageUploadResult { ... };
}
```

### 並發安全的 DisplayOrder 管理
```csharp
public class DisplayOrderManager
{
    // 高並發場景：使用悲觀鎖
    public async Task AssignDisplayOrdersWithLockAsync(EntityType entityType, int entityId, List<long> imageIds)
    {
        // 使用 SELECT ... WITH (UPDLOCK, HOLDLOCK) 避免競態條件
    }
    
    // 一般場景：依賴事務隔離級別
    public async Task AssignDisplayOrdersAsync(EntityType entityType, int entityId, List<long> imageIds)
    {
        // 計算 MaxOrder 並分配連續序號
    }
    
    // 重新排序（處理刪除後的空隙）
    public async Task ReorderDisplayOrdersAsync(EntityType entityType, int entityId);
}
```

### IsMainImage 邏輯
- **主圖判斷**: DisplayOrder 最小的圖片為主圖
- **動態計算**: 不儲存 IsMainImage 欄位，透過查詢計算
- **向後相容**: PropertyImageService.GetMainPropertyImageAsync() 回傳主圖資訊

## 🔄 圖片處理流程
```
1. 實體存在性驗證 (EntityType + EntityId)
   ↓
2. 檔案驗證 (格式、大小、數量)
   ↓
3. 生成 ImageGuid 和 StoredFileName
   ↓
4. 圖片處理 (Resize + WebP 轉換)
   - 原圖: 最大 1200px
   - 中圖: 800px 寬
   - 縮圖: 300x200px
   ↓
5. 上傳到 Blob Storage (三種尺寸)
   ↓
6. 儲存到 Images 資料表 (DisplayOrder 初始為 NULL)
   ↓
7. 批次分配 DisplayOrder
   ↓
8. 回傳 ImageUploadResult
```

## 📄 範例用法

### 新服務使用方式
```csharp
// 房源圖片上傳
var results = await _imageUploadService.UploadImagesAsync(files, EntityType.Property, propertyId, ImageCategory.Gallery);

// 會員頭像上傳
var result = await _imageUploadService.UploadImageAsync(stream, "avatar.jpg", EntityType.Member, memberId, ImageCategory.Avatar);

// 查詢房源主圖
var mainImage = await _imageQueryService.GetMainImageAsync(EntityType.Property, propertyId, ImageCategory.Gallery);

// 查詢所有房源圖片
var propertyImages = await _imageQueryService.GetImagesByEntityAsync(EntityType.Property, propertyId);
```

### 向後相容 (PropertyImageService Facade)
```csharp
// 保持舊介面不變
var results = await _propertyImageService.UploadPropertyImagesAsync(propertyId, files);

// 新增功能
var mainImage = await _propertyImageService.GetMainPropertyImageAsync(propertyId);
var allImages = await _propertyImageService.GetPropertyImagesAsync(propertyId);
```

## 🗂️ Blob Storage 路徑結構
```
images/
├── property/{entityId}/
│   ├── original/{guid}.webp
│   ├── medium/{guid}.webp
│   └── thumbnail/{guid}.webp
├── member/{entityId}/
│   ├── original/{guid}.webp
│   └── thumbnail/{guid}.webp
├── furniture/{entityId}/
└── announcement/{entityId}/
```

## 🧪 測試需求

### 單元測試
1. **ImageUploadService**
   - 實體存在性驗證
   - 檔案格式和大小驗證
   - 圖片處理流程
   - 並發上傳處理
   - 錯誤處理情境

2. **ImageQueryService**
   - 圖片查詢功能
   - 主圖判斷邏輯
   - URL 生成正確性

3. **DisplayOrderManager**
   - 序號分配邏輯
   - 並發安全測試
   - 重新排序功能

4. **EntityExistenceChecker**
   - 不同實體類型的存在性檢查
   - 無效 EntityId 處理

### 整合測試
1. **完整上傳流程**
   - 檔案處理 → Blob Storage → 資料庫儲存
   - 事務一致性驗證

2. **PropertyImageService Facade**
   - 向後相容性測試
   - 新舊介面對比驗證

3. **並發測試**
   - 多用戶同時上傳
   - DisplayOrder 競態條件測試

## 📦 預期交付內容

### 1. Entity Framework 相關
- `zuHause/Models/Image.cs`
- `zuHause/Data/Configurations/ImageConfiguration.cs`
- `zuHause/Data/ZuHauseContext.cs` (新增 DbSet<Image>)
- `zuHause/Enums/EntityType.cs`
- `zuHause/Enums/ImageCategory.cs`
- Migration 檔案

### 2. 核心服務
- `zuHause/Services/ImageUploadService.cs`
- `zuHause/Services/ImageQueryService.cs`
- `zuHause/Services/EntityExistenceChecker.cs`
- `zuHause/Services/DisplayOrderManager.cs`
- `zuHause/Services/BlobStorageService.cs`
- 對應的介面檔案

### 3. DTO 和配置
- `zuHause/DTOs/ImageUploadResult.cs`
- `zuHause/Options/ImageUploadOptions.cs`

### 4. 重構的舊服務
- `zuHause/Services/PropertyImageService.cs` (Facade 模式)

### 5. 測試檔案
- `Tests/zuHause.Tests/Services/ImageUploadServiceTests.cs`
- `Tests/zuHause.Tests/Services/ImageQueryServiceTests.cs`
- `Tests/zuHause.Tests/Services/DisplayOrderManagerTests.cs`
- `Tests/zuHause.Tests/Services/EntityExistenceCheckerTests.cs`
- `Tests/zuHause.Tests/Integration/ImageManagementIntegrationTests.cs`

### 6. 依賴注入註冊
- `zuHause/Program.cs` 更新

## ⚠️ 注意事項

### 並發控制
- 生產環境使用悲觀鎖處理 DisplayOrder 競態條件
- 開發環境使用簡單的事務隔離級別
- 提供可配置的並發控制策略

### 資料一致性
- 實體存在性驗證：上傳前檢查 EntityType + EntityId 的有效性
- 事務管理：批次上傳使用資料庫事務確保一致性
- 應用程式層級聯：刪除實體時同步刪除相關圖片

### 向後相容性
- PropertyImageService 採用 Facade 模式，保持舊介面不變
- 舊的 PropertyImages 資料表暫時保留，待遷移完成後移除
- 測試覆蓋舊介面的所有功能

### 效能考量
- 計算欄位 StoredFileName 避免程式端字串拼接
- 索引優化：EntityType + EntityId + DisplayOrder 複合索引
- 圖片處理：先 Resize 後轉 WebP，減少記憶體消耗

### 錯誤處理
- 完整的錯誤類型定義和處理
- 詳細的日誌記錄（成功和失敗情境）
- 優雅的降級處理

## 🔗 相關任務
- **下一個任務**: 實作資料遷移工具（舊表 → 新表）
- **後續任務**: 其他模組（Member、Furniture）的圖片服務整合

---
**遵循原子化開發原則，此任務為完整的交付單元 (實體+服務+測試+配置)**

---
*建立時間: 2025-07-16*
*更新者: Claude Code Assistant*