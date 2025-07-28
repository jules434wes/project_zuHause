-- 檢查 images 表的詳細信息
SELECT TOP 5
    ImageId, 
    ImageGuid, 
    StoredFileName, 
    MimeType,
    CreatedAt,
    DATEADD(hour, 8, CreatedAt) as CreatedAt_TPE,
    Category,
    DisplayOrder
FROM Images 
WHERE EntityType = 'Property' AND EntityId = 5064
ORDER BY DisplayOrder;

-- 檢查對應的 properties 表時間
SELECT 
    PropertyId,
    Title,
    CreatedAt,
    DATEADD(hour, 8, CreatedAt) as CreatedAt_TPE,
    UpdatedAt,
    DATEADD(hour, 8, UpdatedAt) as UpdatedAt_TPE
FROM Properties 
WHERE PropertyId = 5064;