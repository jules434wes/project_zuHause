using Microsoft.EntityFrameworkCore;
using zuHause.Models;

public class DebugPreviewUrl
{
    public static async Task CheckPreviewImageUrl()
    {
        var connectionString = "Server=tcp:zuhause.database.windows.net,1433;Initial Catalog=zuHause;Persist Security Info=False;User ID=zuHause_dev;Password=DB$MSIT67;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        var options = new DbContextOptionsBuilder<ZuHauseContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new ZuHauseContext(options);
        
        // 檢查 PropertyId 5065 的 PreviewImageUrl
        Console.WriteLine("=== 檢查 PropertyId 5065 的 PreviewImageUrl ===");
        
        var property = await context.Properties
            .Where(p => p.PropertyId == 5065)
            .Select(p => new 
            { 
                PropertyId = p.PropertyId, 
                Title = p.Title,
                PreviewImageUrl = p.PreviewImageUrl,
                StatusCode = p.StatusCode
            })
            .FirstOrDefaultAsync();
            
        if (property != null)
        {
            Console.WriteLine($"PropertyId: {property.PropertyId}");
            Console.WriteLine($"Title: {property.Title}");
            Console.WriteLine($"StatusCode: {property.StatusCode}");
            Console.WriteLine($"PreviewImageUrl: '{property.PreviewImageUrl}'");
            Console.WriteLine($"PreviewImageUrl is null: {property.PreviewImageUrl == null}");
            Console.WriteLine($"PreviewImageUrl is empty: {string.IsNullOrEmpty(property.PreviewImageUrl)}");
        }
        else
        {
            Console.WriteLine("Property 5065 not found!");
        }

        // 檢查房東 UserId 16 的所有房源
        Console.WriteLine("\n=== 檢查房東 UserId 16 的所有房源 ===");
        
        var properties = await context.Properties
            .Where(p => p.LandlordMemberId == 16 && p.DeletedAt == null)
            .Select(p => new 
            { 
                PropertyId = p.PropertyId, 
                Title = p.Title,
                PreviewImageUrl = p.PreviewImageUrl
            })
            .ToListAsync();
            
        foreach (var prop in properties)
        {
            Console.WriteLine($"PropertyId: {prop.PropertyId}, Title: {prop.Title}, PreviewImageUrl: '{prop.PreviewImageUrl}'");
        }
    }
}