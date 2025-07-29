using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Enums;
using Microsoft.EntityFrameworkCore;

namespace zuHause.Services
{
    /// <summary>
    /// Blob 檔案遷移服務實作
    /// 負責處理臨時區域到正式區域的檔案移動邏輯
    /// </summary>
    public class BlobMigrationService : IBlobMigrationService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IBlobUrlGenerator _urlGenerator;
        private readonly ITempSessionService _tempSessionService;
        private readonly ZuHauseContext _context;
        private readonly IDisplayOrderManager _displayOrderManager;
        private readonly ILogger<BlobMigrationService> _logger;

        public BlobMigrationService(
            IBlobStorageService blobStorageService,
            IBlobUrlGenerator urlGenerator,
            ITempSessionService tempSessionService,
            ZuHauseContext context,
            IDisplayOrderManager displayOrderManager,
            ILogger<BlobMigrationService> logger)
        {
            _blobStorageService = blobStorageService;
            _urlGenerator = urlGenerator;
            _tempSessionService = tempSessionService;
            _context = context;
            _displayOrderManager = displayOrderManager;
            _logger = logger;
        }

        /// <summary>
        /// 將臨時區域的檔案移動到正式區域
        /// </summary>
        public async Task<MigrationResult> MoveTempToPermanentAsync(
            string tempSessionId, 
            IEnumerable<Guid> imageGuids, 
            ImageCategory category, 
            int entityId)
        {
            return await MoveTempToPermanentAsync(tempSessionId, imageGuids, category, entityId, null);
        }

        /// <summary>
        /// 將臨時區域的檔案移動到正式區域（包含圖片順序）
        /// </summary>
        public async Task<MigrationResult> MoveTempToPermanentAsync(
            string tempSessionId, 
            IEnumerable<Guid> imageGuids, 
            ImageCategory category, 
            int entityId,
            IEnumerable<string>? imageOrder)
        {
            _logger.LogInformation("開始檔案遷移: TempSession={TempSessionId}, Category={Category}, EntityId={EntityId}, ImageCount={ImageCount}",
                tempSessionId, category, entityId, imageGuids.Count());

            // 記錄圖片順序信息
            if (imageOrder != null && imageOrder.Any())
            {
                _logger.LogInformation("收到圖片順序信息，共 {OrderCount} 項: {ImageOrder}", 
                    imageOrder.Count(), string.Join(", ", imageOrder));
            }

            try
            {
                // 1. 驗證臨時檔案
                var validationResult = await ValidateTempFilesAsync(tempSessionId, imageGuids);
                if (!validationResult.IsValid)
                {
                    return MigrationResult.CreateFailure($"臨時檔案驗證失敗: {validationResult.ErrorMessage}");
                }

                // 2. 準備移動操作（根據 MIME 類型決定需要移動的尺寸）
                var moveOperations = new Dictionary<string, string>();
                var migrationDetails = new Dictionary<Guid, Dictionary<ImageSize, BlobUploadResult>>();

                // 獲取臨時圖片資訊以取得 MIME 類型
                var tempImages = await _tempSessionService.GetTempImagesAsync(tempSessionId);
                var tempImageMap = tempImages.ToDictionary(img => img.ImageGuid, img => img);

                foreach (var imageGuid in imageGuids)
                {
                    migrationDetails[imageGuid] = new Dictionary<ImageSize, BlobUploadResult>();

                    // 獲取檔案的 MIME 類型
                    if (!tempImageMap.TryGetValue(imageGuid, out var tempImageInfo))
                    {
                        _logger.LogError("無法找到圖片的臨時資訊進行移動: {ImageGuid}", imageGuid);
                        continue;
                    }

                    // 根據 MIME 類型獲取需要移動的尺寸
                    var requiredSizes = GetRequiredSizesForMimeType(tempImageInfo.MimeType);
                    
                    _logger.LogDebug("準備移動檔案 {ImageGuid} (MIME: {MimeType})，尺寸: {RequiredSizes}", 
                        imageGuid, tempImageInfo.MimeType, string.Join(", ", requiredSizes));

                    // 為每個必要尺寸準備移動操作
                    foreach (var size in requiredSizes)
                    {
                        var tempPath = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, size);
                        var permanentPath = _urlGenerator.GetBlobPath(category, entityId, imageGuid, size);
                        
                        moveOperations[tempPath] = permanentPath;
                    }
                }

                _logger.LogDebug("準備移動 {OperationCount} 個檔案", moveOperations.Count);

                // 3. 執行批次移動
                var moveResults = await _blobStorageService.MoveBatchAsync(moveOperations, deleteSource: true);

                // 4. 處理結果（只處理實際移動的尺寸）
                var movedFilePaths = new List<string>();
                var allSuccess = true;

                foreach (var imageGuid in imageGuids)
                {
                    // 再次獲取檔案的 MIME 類型以確定需要處理的尺寸
                    if (!tempImageMap.TryGetValue(imageGuid, out var tempImageInfo))
                    {
                        _logger.LogError("無法找到圖片的臨時資訊進行結果處理: {ImageGuid}", imageGuid);
                        allSuccess = false;
                        continue;
                    }

                    var requiredSizes = GetRequiredSizesForMimeType(tempImageInfo.MimeType);

                    foreach (var size in requiredSizes)
                    {
                        var tempPath = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, size);
                        
                        if (moveResults.TryGetValue(tempPath, out var result))
                        {
                            migrationDetails[imageGuid][size] = result;
                            
                            if (result.Success)
                            {
                                movedFilePaths.Add(result.BlobPath ?? "");
                            }
                            else
                            {
                                allSuccess = false;
                                _logger.LogError("檔案移動失敗: {TempPath} -> {Error}", tempPath, result.Message);
                            }
                        }
                        else
                        {
                            // 如果沒有在結果中找到，表示移動失敗
                            var failureResult = BlobUploadResult.CreateFailure($"移動操作未執行: {tempPath}");
                            migrationDetails[imageGuid][size] = failureResult;
                            allSuccess = false;
                        }
                    }
                }

                // 5. 準備資料庫記錄更新：根據分類處理不同邏輯（不立即提交）
                if (allSuccess)
                {
                    if (category == ImageCategory.Document)
                    {
                        // Document 分類：更新 propertyProofURL，不存入 images 表
                        await PreparePropertyProofUrlUpdateAsync(tempSessionId, imageGuids, entityId);
                    }
                    else
                    {
                        // Gallery 分類：準備遷移到 images 表（內部會自動處理 PreviewImageUrl 更新）
                        await PrepareImageEntityCreationAsync(tempSessionId, imageGuids, entityId, imageOrder);
                    }
                }

                // 7. 更新臨時會話（移除已成功遷移的圖片）
                if (allSuccess)
                {
                    foreach (var imageGuid in imageGuids)
                    {
                        await _tempSessionService.RemoveTempImageAsync(tempSessionId, imageGuid);
                    }
                }

                // 8. 建立結果
                if (allSuccess)
                {
                    _logger.LogInformation("檔案遷移成功完成: {FileCount} 個檔案", movedFilePaths.Count);
                    return MigrationResult.CreateSuccess(migrationDetails, movedFilePaths);
                }
                else
                {
                    var errorMessage = "部分檔案遷移失敗，請檢查日誌";
                    _logger.LogWarning(errorMessage);
                    return MigrationResult.CreateFailure(errorMessage, migrationDetails, movedFilePaths);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檔案遷移過程中發生異常");
                return MigrationResult.CreateFailure($"遷移過程中發生異常: {ex.Message}");
            }
        }

        /// <summary>
        /// 驗證臨時檔案是否存在且有效
        /// </summary>
        public async Task<BlobValidationResult> ValidateTempFilesAsync(
            string tempSessionId, 
            IEnumerable<Guid> imageGuids)
        {
            _logger.LogDebug("開始驗證臨時檔案: TempSession={TempSessionId}, ImageCount={ImageCount}",
                tempSessionId, imageGuids.Count());

            try
            {
                // 1. 驗證會話有效性
                if (!await _tempSessionService.IsValidTempSessionAsync(tempSessionId))
                {
                    return BlobValidationResult.CreateFailure("無效的臨時會話ID");
                }

                // 2. 驗證會話中的圖片資訊
                var tempImages = await _tempSessionService.GetTempImagesAsync(tempSessionId);
                var tempImageGuids = tempImages.Select(img => img.ImageGuid).ToHashSet();

                var missingInSession = imageGuids.Where(guid => !tempImageGuids.Contains(guid)).ToList();
                if (missingInSession.Any())
                {
                    return BlobValidationResult.CreateFailure(
                        $"以下圖片不存在於臨時會話中: {string.Join(", ", missingInSession)}");
                }

                // 3. 建立 ImageGuid -> TempImageInfo 的映射，以便獲取 MIME 類型
                var tempImageMap = tempImages.ToDictionary(img => img.ImageGuid, img => img);

                // 4. 驗證實際檔案存在性（根據 MIME 類型檢查對應尺寸）
                var validFiles = new Dictionary<Guid, List<string>>();
                var missingFiles = new Dictionary<Guid, List<string>>();

                foreach (var imageGuid in imageGuids)
                {
                    validFiles[imageGuid] = new List<string>();
                    missingFiles[imageGuid] = new List<string>();

                    // 從 tempImageMap 獲取檔案的 MIME 類型
                    if (!tempImageMap.TryGetValue(imageGuid, out var tempImageInfo))
                    {
                        _logger.LogError("無法找到圖片的臨時資訊: {ImageGuid}", imageGuid);
                        missingFiles[imageGuid].Add($"Missing temp info for {imageGuid}");
                        continue;
                    }

                    // 根據 MIME 類型獲取需要檢查的尺寸
                    var requiredSizes = GetRequiredSizesForMimeType(tempImageInfo.MimeType);
                    
                    _logger.LogDebug("檢查檔案 {ImageGuid} (MIME: {MimeType})，需要的尺寸: {RequiredSizes}", 
                        imageGuid, tempImageInfo.MimeType, string.Join(", ", requiredSizes));

                    foreach (var size in requiredSizes)
                    {
                        var tempPath = _urlGenerator.GetTempBlobPath(tempSessionId, imageGuid, size);
                        var exists = await _blobStorageService.ExistsAsync(tempPath);

                        if (exists)
                        {
                            validFiles[imageGuid].Add(tempPath);
                        }
                        else
                        {
                            missingFiles[imageGuid].Add(tempPath);
                            _logger.LogWarning("臨時檔案不存在: {TempPath} (MIME: {MimeType}, Size: {Size})", 
                                tempPath, tempImageInfo.MimeType, size);
                        }
                    }
                }

                // 5. 檢查是否有遺失的檔案
                var totalMissingFiles = missingFiles.SelectMany(m => m.Value).Count();
                if (totalMissingFiles > 0)
                {
                    return BlobValidationResult.CreateFailure(
                        $"發現 {totalMissingFiles} 個遺失的臨時檔案",
                        validFiles,
                        missingFiles);
                }

                _logger.LogDebug("臨時檔案驗證通過: {ValidFileCount} 個檔案", 
                    validFiles.SelectMany(v => v.Value).Count());

                return BlobValidationResult.CreateSuccess(validFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證臨時檔案時發生異常");
                return BlobValidationResult.CreateFailure($"驗證過程中發生異常: {ex.Message}");
            }
        }

        /// <summary>
        /// 根據 MIME 類型獲取需要檢查的圖片尺寸
        /// </summary>
        /// <param name="mimeType">MIME 類型</param>
        /// <returns>需要檢查的 ImageSize 列表</returns>
        private List<ImageSize> GetRequiredSizesForMimeType(string mimeType)
        {
            // PDF 檔案只需要 Original 尺寸
            if (string.Equals(mimeType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return new List<ImageSize> { ImageSize.Original };
            }

            // 圖片檔案需要所有尺寸
            if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return Enum.GetValues<ImageSize>().ToList();
            }

            // 未知類型預設使用所有尺寸（保守做法）
            _logger.LogWarning("未知的 MIME 類型，使用所有尺寸進行檢查: {MimeType}", mimeType);
            return Enum.GetValues<ImageSize>().ToList();
        }

        /// <summary>
        /// 回滾已移動的檔案（發生錯誤時使用）
        /// </summary>
        public async Task<bool> RollbackMigrationAsync(MigrationResult migrationResult)
        {
            if (migrationResult.MovedFilePaths.Count == 0)
            {
                _logger.LogInformation("沒有需要回滾的檔案");
                return true;
            }

            _logger.LogWarning("開始回滾檔案遷移: {FileCount} 個檔案", migrationResult.MovedFilePaths.Count);

            try
            {
                var deleteResults = await _blobStorageService.DeleteMultipleAsync(migrationResult.MovedFilePaths);
                var successCount = deleteResults.Count(r => r.Value);
                var failureCount = deleteResults.Count(r => !r.Value);

                if (failureCount > 0)
                {
                    _logger.LogError("回滾過程中 {FailureCount} 個檔案刪除失敗", failureCount);
                    return false;
                }

                _logger.LogInformation("檔案回滾成功完成: {SuccessCount} 個檔案已刪除", successCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回滾檔案時發生異常");
                return false;
            }
        }

        /// <summary>
        /// 準備房源證明文件 URL 更新：從 TempSession 獲取資訊並設定到 properties.propertyProofURL（不提交）
        /// </summary>
        private async Task PreparePropertyProofUrlUpdateAsync(string tempSessionId, IEnumerable<Guid> imageGuids, int propertyId)
        {
            try
            {
                _logger.LogInformation("開始更新房源證明文件 URL: PropertyId={PropertyId}, 文件數量={DocumentCount}",
                    propertyId, imageGuids.Count());

                // 查詢房源記錄
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    _logger.LogWarning("未找到房源記錄: PropertyId={PropertyId}", propertyId);
                    return;
                }

                // 從 TempSession 獲取 Document 類型的圖片資訊
                var tempImages = await _tempSessionService.GetTempImagesAsync(tempSessionId);
                var tempDocuments = tempImages.Where(ti => 
                    imageGuids.Contains(ti.ImageGuid) && 
                    ti.Category == ImageCategory.Document).ToList();

                if (tempDocuments.Count > 0)
                {
                    // 取第一個文件作為證明文件（通常應該只有一個 PDF）
                    var documentImage = tempDocuments.First();
                    
                    // 生成新的證明文件 URL（正式區域的路徑）
                    var propertyProofUrl = _urlGenerator.GetBlobPath(ImageCategory.Document, propertyId, documentImage.ImageGuid, ImageSize.Original);
                    
                    _logger.LogInformation("設定房源證明文件 URL: PropertyId={PropertyId}, DocumentGuid={DocumentGuid}, URL={ProofUrl}",
                        propertyId, documentImage.ImageGuid, propertyProofUrl);

                    // 更新房源的證明文件 URL（不立即提交）
                    property.PropertyProofUrl = propertyProofUrl;
                    
                    _logger.LogInformation("房源證明文件 URL 準備完成: PropertyId={PropertyId}（待外部交易提交）", propertyId);
                }
                else
                {
                    _logger.LogInformation("未找到需要處理的證明文件: PropertyId={PropertyId}", propertyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新房源證明文件 URL 時發生異常: PropertyId={PropertyId}", propertyId);
                throw;
            }
        }

        /// <summary>
        /// 準備圖片資料庫記錄創建：從 TempSession 獲取資訊並準備正式的 Image 記錄，並同時更新房源預覽圖
        /// </summary>
        private async Task PrepareImageEntityCreationAsync(string tempSessionId, IEnumerable<Guid> imageGuids, int actualEntityId, IEnumerable<string>? imageOrder = null)
        {
            try
            {
                _logger.LogInformation("開始創建圖片資料庫記錄: TempSessionId={TempSessionId}, EntityId={EntityId}, 圖片數量={ImageCount}",
                    tempSessionId, actualEntityId, imageGuids.Count());

                // 從 TempSession 獲取圖片資訊
                var tempImages = await _tempSessionService.GetTempImagesAsync(tempSessionId);
                var relevantTempImages = tempImages.Where(ti => imageGuids.Contains(ti.ImageGuid)).ToList();

                if (relevantTempImages.Count == 0)
                {
                    _logger.LogWarning("未在臨時會話中找到相關圖片: TempSessionId={TempSessionId}, ImageGuids={ImageGuids}", 
                        tempSessionId, string.Join(",", imageGuids));
                    return;
                }

                // 為每個圖片創建 Image 記錄
                var newImages = new List<Image>();
                foreach (var tempImage in relevantTempImages)
                {
                    // 生成正式區域的檔案路徑（保持 Category 原始大小寫以便解析）
                    var storedFileName = $"{tempImage.Category}/{actualEntityId}/{tempImage.ImageGuid:N}.webp";
                    
                    var image = new Image
                    {
                        ImageGuid = tempImage.ImageGuid,
                        EntityType = EntityType.Property,
                        EntityId = actualEntityId,
                        Category = tempImage.Category,
                        MimeType = tempImage.MimeType,
                        OriginalFileName = tempImage.OriginalFileName,
                        StoredFileName = storedFileName,
                        FileSizeBytes = tempImage.FileSizeBytes,
                        Width = 0, // 將在後續處理中更新
                        Height = 0, // 將在後續處理中更新
                        DisplayOrder = null, // 將由 DisplayOrderManager 分配
                        IsActive = true,
                        UploadedByMemberId = null // 沒有用戶上下文，UploadedAt 使用資料庫預設值 (UTC+8)
                    };

                    newImages.Add(image);
                    
                    _logger.LogDebug("創建圖片記錄: ImageGuid={ImageGuid}, EntityId={EntityId}, FileName={FileName}",
                        tempImage.ImageGuid, actualEntityId, tempImage.OriginalFileName);
                }

                // 批次新增圖片記錄並保存以獲取 ImageId
                _context.Images.AddRange(newImages);
                await _context.SaveChangesAsync(); // 立即保存以獲取 ImageId
                
                // 根據前端傳入的順序分配 DisplayOrder
                _logger.LogInformation("開始為 {ImageCount} 張圖片分配 DisplayOrder", newImages.Count);
                
                if (imageOrder != null && imageOrder.Any())
                {
                    _logger.LogInformation("使用前端傳入的圖片順序進行排序");
                    
                    // 將 imageOrder 轉換為 Guid 字典，便於查詢
                    var orderMap = imageOrder
                        .Select((guidStr, index) => new { GuidStr = guidStr, Order = index + 1 })
                        .Where(x => Guid.TryParse(x.GuidStr, out _))
                        .ToDictionary(x => Guid.Parse(x.GuidStr), x => x.Order);
                    
                    // 根據前端順序分配 DisplayOrder
                    foreach (var image in newImages)
                    {
                        if (orderMap.TryGetValue(image.ImageGuid, out var order))
                        {
                            image.DisplayOrder = order;
                            _logger.LogDebug("根據前端順序分配 DisplayOrder {Order} 給圖片 {ImageGuid}", order, image.ImageGuid);
                        }
                        else
                        {
                            // 如果在前端順序中找不到，分配到最後
                            var maxOrder = orderMap.Values.DefaultIfEmpty(0).Max();
                            image.DisplayOrder = maxOrder + 1;
                            _logger.LogWarning("圖片 {ImageGuid} 不在前端順序中，分配到最後位置 {Order}", image.ImageGuid, image.DisplayOrder);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("未提供前端排序，使用預設順序（建立順序）");
                    
                    // 使用預設順序
                    for (int i = 0; i < newImages.Count; i++)
                    {
                        newImages[i].DisplayOrder = i + 1;
                        _logger.LogDebug("分配預設 DisplayOrder {Order} 給圖片 {ImageGuid}", i + 1, newImages[i].ImageGuid);
                    }
                }
                
                // 保存 DisplayOrder 更新
                await _context.SaveChangesAsync();
                _logger.LogInformation("DisplayOrder 分配完成，共分配 {Count} 個順序", newImages.Count);
                
                // 立即更新房源的 PreviewImageUrl（使用剛建立的第一張圖片）
                await UpdatePropertyPreviewImageDirectly(actualEntityId, newImages);
                
                _logger.LogInformation("圖片資料庫記錄準備完成: 準備了 {CreatedCount} 個圖片記錄（待外部交易提交）", newImages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建圖片資料庫記錄時發生異常: tempSessionId={TempSessionId}, actualEntityId={ActualEntityId}",
                    tempSessionId, actualEntityId);
                throw;
            }
        }

        /// <summary>
        /// 直接更新房源的預覽圖 URL（使用已知的圖片列表）
        /// </summary>
        private async Task UpdatePropertyPreviewImageDirectly(int propertyId, List<Image> newImages)
        {
            try
            {
                _logger.LogInformation("直接更新房源預覽圖: PropertyId={PropertyId}, 圖片數量={ImageCount}", 
                    propertyId, newImages.Count);

                // 查詢房源記錄
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    _logger.LogWarning("未找到房源記錄: PropertyId={PropertyId}", propertyId);
                    return;
                }

                // 找到 DisplayOrder = 1 的主圖（第一張圖片）
                var mainImage = newImages.FirstOrDefault(img => img.DisplayOrder == 1);
                
                if (mainImage != null)
                {
                    // 使用 GenerateImageUrl 生成完整的縮圖 URL
                    var previewImageUrl = _urlGenerator.GenerateImageUrl(mainImage.Category, mainImage.EntityId, mainImage.ImageGuid, ImageSize.Medium);
                    
                    _logger.LogInformation("設定房源預覽圖: PropertyId={PropertyId}, ImageGuid={ImageGuid}, URL={PreviewUrl}",
                        propertyId, mainImage.ImageGuid, previewImageUrl);

                    property.PreviewImageUrl = previewImageUrl;
                }
                else
                {
                    _logger.LogInformation("未找到主圖，清空房源預覽圖: PropertyId={PropertyId}", propertyId);
                    property.PreviewImageUrl = null;
                }
                
                // 保存 PreviewImageUrl 變更到資料庫
                await _context.SaveChangesAsync();
                _logger.LogInformation("房源預覽圖已直接更新: PropertyId={PropertyId}, PreviewImageUrl={PreviewImageUrl}", 
                    propertyId, property.PreviewImageUrl ?? "NULL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "直接更新房源預覽圖失敗: PropertyId={PropertyId}", propertyId);
                throw;
            }
        }

        /// <summary>
        /// 準備房源縮圖更新：將排序為 1 的圖片設為 properties.previewImageURL（不提交）
        /// </summary>
        private async Task PreparePropertyPreviewImageUpdateAsync(int propertyId)
        {
            try
            {
                _logger.LogInformation("開始更新房源縮圖: PropertyId={PropertyId}", propertyId);

                // 查詢房源記錄
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    _logger.LogWarning("未找到房源記錄: PropertyId={PropertyId}", propertyId);
                    return;
                }

                // 清除 DbContext 快取以確保查詢最新資料
                _context.ChangeTracker.Clear();
                
                // 查詢排序為 1 的 Gallery 圖片
                var previewImage = await _context.Images
                    .Where(img => img.EntityId == propertyId &&
                                  img.EntityType == EntityType.Property &&
                                  img.Category == ImageCategory.Gallery &&
                                  img.DisplayOrder == 1)
                    .FirstOrDefaultAsync();
                    
                _logger.LogInformation("查詢縮圖結果: PropertyId={PropertyId}, 找到圖片={HasImage}", 
                    propertyId, previewImage != null);
                    
                if (previewImage != null)
                {
                    _logger.LogInformation("縮圖圖片詳細: ImageGuid={ImageGuid}, DisplayOrder={DisplayOrder}", 
                        previewImage.ImageGuid, previewImage.DisplayOrder);
                }

                if (previewImage != null)
                {
                    // 使用 GenerateImageUrl 生成完整的縮圖 URL
                    var previewImageUrl = _urlGenerator.GenerateImageUrl(previewImage.Category, previewImage.EntityId, previewImage.ImageGuid, ImageSize.Medium);
                    
                    _logger.LogInformation("設定房源縮圖: PropertyId={PropertyId}, ImageGuid={ImageGuid}, URL={PreviewUrl}",
                        propertyId, previewImage.ImageGuid, previewImageUrl);

                    property.PreviewImageUrl = previewImageUrl;
                }
                else
                {
                    _logger.LogInformation("未找到排序為 1 的圖片，清空縮圖: PropertyId={PropertyId}", propertyId);
                    
                    // 如果沒有排序為 1 的圖片，清空縮圖
                    property.PreviewImageUrl = null;
                }
                
                // 保存 PreviewImageUrl 變更到資料庫
                await _context.SaveChangesAsync();
                _logger.LogInformation("房源縮圖 URL 已保存到資料庫: PropertyId={PropertyId}, PreviewImageUrl={PreviewImageUrl}", 
                    propertyId, property.PreviewImageUrl ?? "NULL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新房源縮圖時發生異常: PropertyId={PropertyId}", propertyId);
                throw;
            }
        }
    }
}