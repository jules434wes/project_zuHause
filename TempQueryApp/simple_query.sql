-- 簡單查詢房源和 PreviewImageUrl
SELECT TOP 5 
    PropertyId, Title, CreatedAt, PreviewImageUrl
FROM Properties 
WHERE Title LIKE '%測試%'
ORDER BY CreatedAt DESC;

-- 查詢房源 5064 的圖片
SELECT TOP 10
    ImageId, ImageGuid, EntityId, Category, DisplayOrder, IsActive
FROM Images 
WHERE EntityType = 'Property' AND EntityId = 5064
ORDER BY DisplayOrder;