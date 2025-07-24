using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.Data;

// 臨時資料查詢程式 - 用於測試環境資料驗證
// 使用方式: dotnet script test_data_query.cs

var connectionString = "Server=tcp:zuhause.database.windows.net,1433;Initial Catalog=zuHause;Persist Security Info=False;User ID=zuhause;Password=DB$MSIT67;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

var options = new DbContextOptionsBuilder<ZuHauseContext>()
    .UseSqlServer(connectionString)
    .Options;

using var context = new ZuHauseContext(options);

Console.WriteLine("=== 資料庫資料驗證報告 ===");
Console.WriteLine($"執行時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine();

try
{
    // 1. 查詢城市數量
    var cityCount = await context.Cities.CountAsync();
    Console.WriteLine($"Cities 表記錄數: {cityCount}");
    
    var cities = await context.Cities
        .Where(c => c.IsActive == true)
        .OrderBy(c => c.DisplayOrder)
        .Take(5)
        .Select(c => new { c.CityId, c.CityName, c.CityCode })
        .ToListAsync();
    
    Console.WriteLine("前5筆啟用城市:");
    foreach (var city in cities)
    {
        Console.WriteLine($"  ID: {city.CityId}, Name: {city.CityName}, Code: {city.CityCode}");
    }
    Console.WriteLine();

    // 2. 查詢區域數量
    var districtCount = await context.Districts.CountAsync();
    Console.WriteLine($"Districts 表記錄數: {districtCount}");
    
    var districts = await context.Districts
        .Where(d => d.IsActive == true)
        .OrderBy(d => d.CityId)
        .ThenBy(d => d.DisplayOrder)
        .Take(5)
        .Select(d => new { d.DistrictId, d.DistrictName, d.CityId })
        .ToListAsync();
    
    Console.WriteLine("前5筆啟用區域:");
    foreach (var district in districts)
    {
        Console.WriteLine($"  ID: {district.DistrictId}, Name: {district.DistrictName}, CityID: {district.CityId}");
    }
    Console.WriteLine();

    // 3. 查詢 memberID = 108
    var member108 = await context.Members
        .Where(m => m.MemberId == 108)
        .Select(m => new { 
            m.MemberId, 
            m.MemberName, 
            m.MemberTypeId, 
            m.IsLandlord, 
            m.IsActive,
            m.Email 
        })
        .FirstOrDefaultAsync();
    
    if (member108 != null)
    {
        Console.WriteLine($"memberID = 108 找到:");
        Console.WriteLine($"  姓名: {member108.MemberName}");
        Console.WriteLine($"  會員類型ID: {member108.MemberTypeId}");
        Console.WriteLine($"  是否為房東: {member108.IsLandlord}");
        Console.WriteLine($"  是否啟用: {member108.IsActive}");
        Console.WriteLine($"  電子信箱: {member108.Email}");
    }
    else
    {
        Console.WriteLine("memberID = 108 不存在");
    }
    Console.WriteLine();

    // 4. 查詢刊登方案
    var listingPlans = await context.ListingPlans
        .Where(lp => lp.IsActive == true)
        .OrderBy(lp => lp.PricePerDay * lp.MinListingDays)
        .Select(lp => new { 
            lp.PlanId, 
            lp.PlanName, 
            lp.MinListingDays, 
            lp.PricePerDay,
            TotalPrice = lp.PricePerDay * lp.MinListingDays
        })
        .ToListAsync();
    
    Console.WriteLine($"啟用的刊登方案數量: {listingPlans.Count}");
    Console.WriteLine("刊登方案列表:");
    foreach (var plan in listingPlans)
    {
        Console.WriteLine($"  ID: {plan.PlanId}, Name: {plan.PlanName}, Days: {plan.MinListingDays}, Price/Day: ${plan.PricePerDay}, Total: ${plan.TotalPrice}");
    }
    Console.WriteLine();

    // 5. 查詢設備分類
    var equipmentCount = await context.PropertyEquipmentCategories
        .Where(pec => pec.IsActive == true)
        .CountAsync();
    
    Console.WriteLine($"啟用的設備分類數量: {equipmentCount}");
    
    var equipment = await context.PropertyEquipmentCategories
        .Where(pec => pec.IsActive == true)
        .OrderBy(pec => pec.ParentCategoryId ?? 0)
        .ThenBy(pec => pec.CategoryName)
        .Take(10)
        .Select(pec => new { 
            pec.CategoryId, 
            pec.CategoryName, 
            pec.ParentCategoryId 
        })
        .ToListAsync();
    
    Console.WriteLine("前10筆設備分類:");
    foreach (var eq in equipment)
    {
        Console.WriteLine($"  ID: {eq.CategoryId}, Name: {eq.CategoryName}, ParentID: {eq.ParentCategoryId}");
    }
    Console.WriteLine();

    // 6. 產生測試參數組合建議
    Console.WriteLine("=== 測試參數建議 ===");
    if (cities.Any() && districts.Any())
    {
        var firstCity = cities.First();
        var matchingDistrict = districts.FirstOrDefault(d => d.CityId == firstCity.CityId);
        
        if (matchingDistrict != null)
        {
            Console.WriteLine($"建議使用 CityId: {firstCity.CityId} ({firstCity.CityName})");
            Console.WriteLine($"建議使用 DistrictId: {matchingDistrict.DistrictId} ({matchingDistrict.DistrictName})");
        }
    }
    
    if (listingPlans.Any())
    {
        var cheapestPlan = listingPlans.First();
        Console.WriteLine($"建議使用 ListingPlanId: {cheapestPlan.PlanId} ({cheapestPlan.PlanName})");
    }
    
    if (equipment.Any())
    {
        var firstEquipment = equipment.Take(3);
        Console.WriteLine("建議使用設備ID:");
        foreach (var eq in firstEquipment)
        {
            Console.WriteLine($"  {eq.CategoryId} ({eq.CategoryName})");
        }
    }

    Console.WriteLine();
    Console.WriteLine("=== 資料驗證總結 ===");
    Console.WriteLine($"✓ Cities: {cityCount} 筆記錄");
    Console.WriteLine($"✓ Districts: {districtCount} 筆記錄");
    Console.WriteLine($"{(member108 != null ? "✓" : "✗")} memberID = 108: {(member108 != null ? "存在" : "不存在")}");
    Console.WriteLine($"✓ ListingPlans: {listingPlans.Count} 筆記錄");
    Console.WriteLine($"✓ Equipment Categories: {equipmentCount} 筆記錄");
    
    if (member108 != null && member108.MemberTypeId == 2 && member108.IsLandlord && member108.IsActive)
    {
        Console.WriteLine("✓ memberID = 108 具備房東身份且已啟用");
    }
    else if (member108 != null)
    {
        Console.WriteLine($"⚠ memberID = 108 存在但身份驗證失敗: MemberTypeId={member108.MemberTypeId}, IsLandlord={member108.IsLandlord}, IsActive={member108.IsActive}");
    }
    else
    {
        Console.WriteLine("✗ memberID = 108 需要建立");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"發生錯誤: {ex.Message}");
    Console.WriteLine($"堆疊追蹤: {ex.StackTrace}");
}