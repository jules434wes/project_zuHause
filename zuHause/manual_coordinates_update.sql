-- 手動新增座標欄位到 Properties 表
-- 只執行座標相關的變更，避開現有物件衝突

BEGIN TRANSACTION;

-- 檢查欄位是否已存在
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'properties' AND COLUMN_NAME = 'latitude')
BEGIN
    ALTER TABLE properties 
    ADD latitude DECIMAL(10,8) NULL;
    
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'緯度',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'properties',
         @level2type = N'COLUMN', @level2name = N'latitude';
END;

-- 檢查欄位是否已存在
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'properties' AND COLUMN_NAME = 'longitude')
BEGIN
    ALTER TABLE properties 
    ADD longitude DECIMAL(11,8) NULL;
    
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'經度',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'properties',
         @level2type = N'COLUMN', @level2name = N'longitude';
END;

-- 建立 GoogleMapsApiUsage 表（如果不存在）
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GoogleMapsApiUsage')
BEGIN
    CREATE TABLE GoogleMapsApiUsage (
        googleMapsApiId INT IDENTITY(1,1) PRIMARY KEY,
        apiType NVARCHAR(50) NOT NULL,
        requestDate DATETIME2 NOT NULL,
        requestCount INT NOT NULL DEFAULT 0,
        estimatedCost DECIMAL(10,4) NOT NULL DEFAULT 0,
        isLimitReached BIT NOT NULL DEFAULT 0,
        createdAt DATETIME2(0) NOT NULL DEFAULT CONVERT(DATETIME2(0), SYSDATETIME())
    );
    
    -- 加入表格註解
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'Google Maps API 使用量追蹤表',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'GoogleMapsApiUsage';
    
    -- 加入欄位註解
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'Google Maps API 使用量ID',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'GoogleMapsApiUsage',
         @level2type = N'COLUMN', @level2name = N'googleMapsApiId';
         
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'API 類型',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'GoogleMapsApiUsage',
         @level2type = N'COLUMN', @level2name = N'apiType';
         
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'請求日期',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'GoogleMapsApiUsage',
         @level2type = N'COLUMN', @level2name = N'requestDate';
         
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'當日請求次數',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'GoogleMapsApiUsage',
         @level2type = N'COLUMN', @level2name = N'requestCount';
         
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'預估成本',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'GoogleMapsApiUsage',
         @level2type = N'COLUMN', @level2name = N'estimatedCost';
         
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'是否達到限制',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'GoogleMapsApiUsage',
         @level2type = N'COLUMN', @level2name = N'isLimitReached';
         
    EXEC sp_addextendedproperty 
         @name = N'MS_Description', @value = N'建立時間',
         @level0type = N'SCHEMA', @level0name = N'dbo',
         @level1type = N'TABLE', @level1name = N'GoogleMapsApiUsage',
         @level2type = N'COLUMN', @level2name = N'createdAt';
END;

COMMIT TRANSACTION;

-- 驗證結果
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    NUMERIC_PRECISION, 
    NUMERIC_SCALE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'properties' 
  AND COLUMN_NAME IN ('latitude', 'longitude')
ORDER BY COLUMN_NAME;

-- 檢查 GoogleMapsApiUsage 表
SELECT COUNT(*) AS TableExists 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'GoogleMapsApiUsage';