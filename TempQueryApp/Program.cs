using Microsoft.Data.SqlClient;

var connectionString = "Server=tcp:zuhause.database.windows.net,1433;Initial Catalog=zuHause;Persist Security Info=False;User ID=zuHause_dev;Password=DB$MSIT67;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

Console.WriteLine("🔍 檢查 Images 表的詳細信息和時間設定...");

using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

// 檢查 images 表詳細信息
var imageQuery = @"
    SELECT TOP 5
        ImageId, 
        ImageGuid, 
        StoredFileName, 
        MimeType,
        UploadedAt,
        DATEADD(hour, 8, UploadedAt) as UploadedAt_TPE,
        Category,
        DisplayOrder
    FROM Images 
    WHERE EntityType = 'Property' AND EntityId = 5064
    ORDER BY DisplayOrder";

using var imageCmd = new SqlCommand(imageQuery, connection);
using var imageReader = await imageCmd.ExecuteReaderAsync();

Console.WriteLine("📊 Images 表詳細信息:");
Console.WriteLine("ImageId | ImageGuid | StoredFileName | MimeType | UploadedAt (UTC) | UploadedAt (TPE) | Category | DisplayOrder");
Console.WriteLine("".PadRight(150, '-'));

while (await imageReader.ReadAsync())
{
    var imageId = imageReader.GetInt64(0);
    var imageGuid = imageReader.GetGuid(1);
    var storedFileName = imageReader.IsDBNull(2) ? "NULL" : imageReader.GetString(2);
    var mimeType = imageReader.IsDBNull(3) ? "NULL" : imageReader.GetString(3);
    var uploadedAtUtc = imageReader.IsDBNull(4) ? "NULL" : imageReader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss");
    var uploadedAtTpe = imageReader.IsDBNull(5) ? "NULL" : imageReader.GetDateTime(5).ToString("yyyy-MM-dd HH:mm:ss");
    var category = imageReader.IsDBNull(6) ? "NULL" : imageReader.GetValue(6).ToString();
    var displayOrder = imageReader.IsDBNull(7) ? "NULL" : imageReader.GetInt32(7).ToString();
    
    Console.WriteLine($"{imageId} | {imageGuid} | {storedFileName} | {mimeType} | {uploadedAtUtc} | {uploadedAtTpe} | {category} | {displayOrder}");
}

await imageReader.CloseAsync();

// 檢查對應的 properties 表時間和 PropertyProofUrl
var propertyQuery = @"
    SELECT 
        PropertyId,
        Title,
        CreatedAt,
        DATEADD(hour, 8, CreatedAt) as CreatedAt_TPE,
        UpdatedAt,
        DATEADD(hour, 8, UpdatedAt) as UpdatedAt_TPE,
        PropertyProofUrl
    FROM Properties 
    WHERE PropertyId = 5064";

using var propertyCmd = new SqlCommand(propertyQuery, connection);
using var propertyReader = await propertyCmd.ExecuteReaderAsync();

Console.WriteLine("\n🏠 Properties 表時間信息:");
Console.WriteLine("PropertyId | Title | CreatedAt (UTC) | CreatedAt (TPE) | UpdatedAt (UTC) | UpdatedAt (TPE) | PropertyProofUrl");
Console.WriteLine("".PadRight(150, '-'));

while (await propertyReader.ReadAsync())
{
    var propertyId = propertyReader.GetInt32(0);
    var title = propertyReader.GetString(1);
    var createdAtUtc = propertyReader.IsDBNull(2) ? "NULL" : propertyReader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss");
    var createdAtTpe = propertyReader.IsDBNull(3) ? "NULL" : propertyReader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss");
    var updatedAtUtc = propertyReader.IsDBNull(4) ? "NULL" : propertyReader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss");
    var updatedAtTpe = propertyReader.IsDBNull(5) ? "NULL" : propertyReader.GetDateTime(5).ToString("yyyy-MM-dd HH:mm:ss");
    var propertyProofUrl = propertyReader.IsDBNull(6) ? "NULL" : propertyReader.GetString(6);
    
    Console.WriteLine($"{propertyId} | {title} | {createdAtUtc} | {createdAtTpe} | {updatedAtUtc} | {updatedAtTpe} | {propertyProofUrl}");
}

await propertyReader.CloseAsync();

Console.WriteLine("\n✅ 檢查完成");