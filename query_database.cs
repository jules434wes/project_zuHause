using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

partial class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "Server=tcp:zuhause.database.windows.net,1433;Initial Catalog=zuHause;Persist Security Info=False;User ID=zuHause_dev;Password=DB$MSIT67;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        Console.WriteLine("🔍 正在查詢 Azure SQL Database 中的測試房源和圖片...");
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        // 查詢最新的測試房源
        var propertyQuery = @"
            SELECT TOP 10 
                PropertyId, Title, CreatedAt, StatusCode, PreviewImageUrl, PropertyProofUrl
            FROM Properties 
            WHERE Title LIKE '%測試%' OR Title LIKE '%test%'
            ORDER BY CreatedAt DESC";
            
        using var propertyCmd = new SqlCommand(propertyQuery, connection);
        using var propertyReader = await propertyCmd.ExecuteReaderAsync();
        
        Console.WriteLine("\n📋 最新測試房源：");
        Console.WriteLine("PropertyId | Title | CreatedAt | StatusCode | PreviewImageUrl | PropertyProofUrl");
        Console.WriteLine("".PadRight(120, '-'));
        
        var latestPropertyId = 0;
        while (await propertyReader.ReadAsync())
        {
            var propertyId = propertyReader.GetInt32("PropertyId");
            var title = propertyReader.GetString("Title");
            var createdAt = propertyReader.GetDateTime("CreatedAt");
            var statusCode = propertyReader.IsDBNull("StatusCode") ? "NULL" : propertyReader.GetString("StatusCode");
            var previewUrl = propertyReader.IsDBNull("PreviewImageUrl") ? "NULL" : propertyReader.GetString("PreviewImageUrl");
            var proofUrl = propertyReader.IsDBNull("PropertyProofUrl") ? "NULL" : propertyReader.GetString("PropertyProofUrl");
            
            Console.WriteLine($"{propertyId} | {title} | {createdAt:yyyy-MM-dd HH:mm:ss} | {statusCode} | {previewUrl} | {proofUrl}");
            
            if (latestPropertyId == 0) latestPropertyId = propertyId;
        }
        
        await propertyReader.CloseAsync();
        
        if (latestPropertyId > 0)
        {
            // 查詢該房源的圖片記錄
            var imageQuery = @"
                SELECT 
                    ImageId, ImageGuid, EntityId, Category, StoredFileName, DisplayOrder, IsActive, OriginalFileName
                FROM Images 
                WHERE EntityType = 1 AND EntityId = @PropertyId
                ORDER BY DisplayOrder";
                
            using var imageCmd = new SqlCommand(imageQuery, connection);
            imageCmd.Parameters.AddWithValue("@PropertyId", latestPropertyId);
            using var imageReader = await imageCmd.ExecuteReaderAsync();
            
            Console.WriteLine($"\n🖼️ 房源 {latestPropertyId} 的圖片記錄：");
            Console.WriteLine("ImageId | ImageGuid | Category | StoredFileName | DisplayOrder | IsActive | OriginalFileName");
            Console.WriteLine("".PadRight(120, '-'));
            
            var imageCount = 0;
            while (await imageReader.ReadAsync())
            {
                var imageId = imageReader.GetInt64("ImageId");
                var imageGuid = imageReader.GetGuid("ImageGuid");
                var category = imageReader.GetInt32("Category");
                var storedFileName = imageReader.IsDBNull("StoredFileName") ? "NULL" : imageReader.GetString("StoredFileName");
                var displayOrder = imageReader.IsDBNull("DisplayOrder") ? "NULL" : imageReader.GetInt32("DisplayOrder").ToString();
                var isActive = imageReader.GetBoolean("IsActive");
                var originalFileName = imageReader.IsDBNull("OriginalFileName") ? "NULL" : imageReader.GetString("OriginalFileName");
                
                Console.WriteLine($"{imageId} | {imageGuid} | {category} | {storedFileName} | {displayOrder} | {isActive} | {originalFileName}");
                imageCount++;
            }
            
            Console.WriteLine($"\n✅ 總計找到 {imageCount} 張圖片記錄");
        }
        else
        {
            Console.WriteLine("\n❌ 沒有找到測試房源");
        }
        
        Console.WriteLine("\n🔍 查詢完成");
    }
}