// Controllers/SearchController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // 用於 DbSet 和各種 LINQ 擴展方法 (如 Where, CountAsync, ToListAsync)
using System; // 用於 Math.Ceiling (計算總頁數)
using System.Collections.Generic; // 用於 List<T> 和 HashSet<T>
using System.Linq; // 用於 LINQ 查詢方法 (如 Select, Contains, Any, OrderBy, GroupBy, Join 等)
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks; // 用於非同步操作 (如 async/await)
using zuHause.Models; // 引入資料庫模型，例如 Property, City, District 等
using zuHause.ViewModels.TenantViewModel; // 引入用於前端顯示的 ViewModel
using zuHause.Enums; // 引入枚舉類型
using zuHause.Interfaces; // 引入服務介面

namespace zuHause.Controllers
{
    // SearchController 負責處理房源搜尋相關的 API 請求
    public class SearchController : Controller
    {
        private readonly ZuHauseContext _context; // 資料庫上下文，用於與資料庫互動
        private readonly IBlobUrlGenerator _blobUrlGenerator; // Blob URL 生成服務

        // 建構子：透過依賴注入 (DI) 獲取資料庫上下文實例和 Blob URL 生成服務
        public SearchController(ZuHauseContext context, IBlobUrlGenerator blobUrlGenerator)
        {
            _context = context;
            _blobUrlGenerator = blobUrlGenerator;
        }

        // Search Action：用於返回搜尋頁面的 View
        // 通常此 View 會包含前端的 JavaScript (例如 Search.js) 來發送 AJAX 請求獲取房源資料
        public IActionResult Search()
        {
            return View();
        }

        // GetSearchList API 端點：處理前端的 GET 請求，根據篩選和排序條件返回房源列表
        // [HttpGet("api/Tenant/Search/list")] 定義了這個方法的 HTTP 類型和路由
        [HttpGet("api/Tenant/Search/list")] // 添加路由屬性
        public async Task<IActionResult> GetSearchList(
            // --- 分頁和排序參數 ---
            string sortField = "PublishedAt", // 排序欄位，預設為「刊登日期」
            string sortOrder = "desc",          // 排序順序，預設為「降序」
            int pageNumber = 1,                 // 當前頁碼，預設為第一頁
            int pageSize = 6,                   // 每頁顯示的項目數量，預設為 6 個房源

            // --- 篩選參數 (從前端的 URL 查詢字串中接收) ---
            string? keyword = null, // 關鍵字搜尋 (例如標題、描述、地址等)
            string? cityCode = null, // 城市代碼 (例如 "TXG" 代表臺中市)
            [FromQuery] List<string>? districtCode = null, // 行政區代碼列表 (例如 ["BTT", "NTN"])
                                                            // [FromQuery] 確保 ASP.NET Core 能正確綁定多個同名查詢參數到 List<T>
            [FromQuery] List<string>? rentRanges = null, // 新增：租金預設範圍列表 (例如 ["5000-10000", "20000-30000"])
            int? minRent = null, // 最小租金
            int? maxRent = null, // 最大租金

            [FromQuery] List<int>? roomCounts = null, // 房間數列表 (例如 [1, 2, 4])
            [FromQuery] List<int>? bathroomCounts = null, // 衛浴數列表 (例如 [1, 2, 4])

             [FromQuery] List<string>? selectedFloorRanges = null,

             [FromQuery] List<string>? areaRanges = null, // 新增：坪數預設範圍列表 (例如 ["5-10", "20-30"])
            int? minArea = null, // 最小坪數
            int? maxArea = null, // 最大坪數
            string? features = null,           // 特色名稱列表 (逗號分隔字串)
            string? facilities = null,         // 設施名稱列表 (逗號分隔字串)
            string? equipments = null          // 設備名稱列表 (逗號分隔字串)
        )
        {
            // 1. 初始查詢：
            // 從 Properties 表開始查詢，只選擇 DeletedAt 為 null (未刪除) 和p.IsPaid == true(已付款)。

            IQueryable<Property> propertiesQuery = _context.Properties
                                                            .Where(p => p.DeletedAt == null && p.IsPaid == true && p.PublishedAt!= null);

            // --- 2. 應用篩選條件 ---
            // 每個篩選條件都透過 if 判斷式來檢查是否有傳入值。
            // 如果沒有傳入值 (例如 keyword 為 null 或空白，List 為 null 或空)，則不對 propertiesQuery 應用該篩選。
            // 這確保了「未選擇」或「不限」的篩選條件不會限制搜尋結果。

            // 2.1 關鍵字篩選 (將對應 CityName 和 DistrictName 的部分放到 Join 後處理)
            // 這裡只對 Property 自身的欄位進行關鍵字篩選
            if (!string.IsNullOrEmpty(keyword))
            {
                // 先移除關鍵字中的所有空白字元
                var cleanedKeyword = keyword.Replace(" ", ""); // 移除所有空白

                if (!string.IsNullOrEmpty(cleanedKeyword)) // 如果移除空白後仍有內容才進行搜尋
                {
                    // 將清理後的關鍵字拆分為單個字元
                    var searchTerms = cleanedKeyword.ToCharArray().Select(c => c.ToString()).ToList();

                    // 應用 AND 搜尋邏輯
                    foreach (var term in searchTerms)
                    {
                        // 每個字元都必須包含在 Title 中
                        propertiesQuery = propertiesQuery.Where(p => p.Title.Contains(term));
                    }
                }
            }

            // 2.2 城市篩選
            // 確定 currentCityId。這應該是唯一獲取 cityId 的地方。
            var currentCityId = 0; // 預設為 0 (未選擇城市)
            if (!string.IsNullOrEmpty(cityCode))
            {
                var fetchedCityId = await _context.Cities
                                                  .Where(c => c.CityCode == cityCode && c.IsActive)
                                                  .Select(c => c.CityId)
                                                  .FirstOrDefaultAsync();

                if (fetchedCityId != 0) // 如果城市代碼有效
                {
                    currentCityId = fetchedCityId; // 將有效 CityId 賦值給 currentCityId
                    propertiesQuery = propertiesQuery.Where(p => p.CityId == currentCityId); // 應用城市篩選
                }
                else
                {
                    // 如果城市代碼無效，直接返回空結果
                    return Json(new PagedSearchResultViewModel
                    {
                        Properties = new List<SearchViewModel>(),
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    });
                }
            }

            // ... (其他篩選條件，例如關鍵字篩選)

            // 2.3 行政區篩選 (多選)
            if (districtCode != null && districtCode.Any())
            {
                // 在這裡直接使用上面已經確定的 currentCityId
                var districtIds = await _context.Districts
                                                .Where(d => districtCode.Contains(d.DistrictCode) && d.IsActive &&
                                                           (currentCityId != 0 ? d.CityId == currentCityId : true)) // <-- 直接使用 currentCityId
                                                .Select(d => d.DistrictId)
                                                .ToListAsync();

                if (districtIds.Any())
                {
                    propertiesQuery = propertiesQuery.Where(p => districtIds.Contains(p.DistrictId));
                }
                else
                {
                    return Json(new PagedSearchResultViewModel
                    {
                        Properties = new List<SearchViewModel>(),
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    });
                }
            }

            // 2.4 租金範圍篩選
            // --- 2.4 租金範圍篩選 (多選或自定義) ---
            // 優先處理自定義租金範圍
            if (minRent.HasValue || maxRent.HasValue)
            {
                if (minRent.HasValue)
                {
                    propertiesQuery = propertiesQuery.Where(p => p.MonthlyRent >= minRent.Value);
                }
                if (maxRent.HasValue)
                {
                    propertiesQuery = propertiesQuery.Where(p => p.MonthlyRent <= maxRent.Value);
                }
            }
            // 如果沒有自定義租金範圍，則處理多選的預設租金範圍
            else if (rentRanges != null && rentRanges.Any())
            {
                var rentConditions = new List<Expression<Func<Property, bool>>>();

                foreach (var range in rentRanges)
                {
                    var parts = range.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int min) && int.TryParse(parts[1], out int max))
                    {
                        rentConditions.Add(p => p.MonthlyRent >= min && p.MonthlyRent <= max);
                    }
                    // TODO: 考慮處理無效或單一數字的範圍字符串
                }

                if (rentConditions.Any())
                {
                    // 使用 OR 邏輯組合所有租金範圍條件
                    var combinedRentCondition = rentConditions.First();
                    for (int i = 1; i < rentConditions.Count; i++)
                    {
                        combinedRentCondition = Expression.Lambda<Func<Property, bool>>(
                            Expression.OrElse(combinedRentCondition.Body, Expression.Invoke(rentConditions[i], combinedRentCondition.Parameters)),
                            combinedRentCondition.Parameters);
                    }
                    propertiesQuery = propertiesQuery.Where(combinedRentCondition);
                }
            }

            // 2.5 格局篩選 (房間數，多選)
            if (roomCounts != null && roomCounts.Any())
            {
                // 建立一個可動態調整的查詢條件
                var roomConditions = new List<Expression<Func<Property, bool>>>();

                foreach (var roomCount in roomCounts)
                {
                    if (roomCount == 4) // 特殊處理：如果選了4房，表示4房以上
                    {
                        roomConditions.Add(p => p.RoomCount >= 4);
                    }
                    else // 其他房數 (例如1, 2, 3) 則精確匹配
                    {
                        roomConditions.Add(p => p.RoomCount == roomCount);
                    }
                }

                // 將所有條件組合起來 (使用 OR 邏輯)
                // 例如：(p.RoomCount == 2) || (p.RoomCount == 3) || (p.RoomCount >= 4)
                if (roomConditions.Any())
                {
                    var combinedRoomCondition = roomConditions.First();
                    for (int i = 1; i < roomConditions.Count; i++)
                    {
                        combinedRoomCondition = Expression.Lambda<Func<Property, bool>>(
                            Expression.OrElse(combinedRoomCondition.Body, Expression.Invoke(roomConditions[i], combinedRoomCondition.Parameters)),
                            combinedRoomCondition.Parameters);
                    }
                    propertiesQuery = propertiesQuery.Where(combinedRoomCondition);
                }
            }

            // 2.6 衛浴篩選 (衛浴數，多選)
            if (bathroomCounts != null && bathroomCounts.Any())
            {
                // 建立一個可動態調整的查詢條件
                var bathroomConditions = new List<Expression<Func<Property, bool>>>();

                foreach (var bathroomCount in bathroomCounts)
                {
                    if (bathroomCount == 4) // 特殊處理：如果選了4衛，表示4衛以上
                    {
                        bathroomConditions.Add(p => p.BathroomCount >= 4);
                    }
                    else // 其他衛浴數 (例如1, 2, 3) 則精確匹配
                    {
                        bathroomConditions.Add(p => p.BathroomCount == bathroomCount);
                    }
                }

                // 將所有條件組合起來 (使用 OR 邏輯)
                if (bathroomConditions.Any())
                {
                    var combinedBathroomCondition = bathroomConditions.First();
                    for (int i = 1; i < bathroomConditions.Count; i++)
                    {
                        combinedBathroomCondition = Expression.Lambda<Func<Property, bool>>(
                            Expression.OrElse(combinedBathroomCondition.Body, Expression.Invoke(bathroomConditions[i], combinedBathroomCondition.Parameters)),
                            combinedBathroomCondition.Parameters);
                    }
                    propertiesQuery = propertiesQuery.Where(combinedBathroomCondition);
                }
            }

            // 2.7 樓層範圍篩選 (多選)
            if (selectedFloorRanges != null && selectedFloorRanges.Any())
            {
                var floorConditions = new List<Expression<Func<Property, bool>>>();

                foreach (var range in selectedFloorRanges)
                {
                    var parts = range.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int min) && int.TryParse(parts[1], out int max))
                    {
                        // 為每個解析出的範圍添加條件
                        floorConditions.Add(p => p.CurrentFloor >= min && p.CurrentFloor <= max);
                    }
                    // TODO: 考慮處理無效或單一數字的範圍字符串，如果前端可能發送這種情況
                }

                // 將所有樓層條件用 OR (或) 邏輯組合起來
                if (floorConditions.Any())
                {
                    var combinedFloorCondition = floorConditions.First();
                    for (int i = 1; i < floorConditions.Count; i++)
                    {
                        combinedFloorCondition = Expression.Lambda<Func<Property, bool>>(
                            Expression.OrElse(combinedFloorCondition.Body, Expression.Invoke(floorConditions[i], combinedFloorCondition.Parameters)),
                            combinedFloorCondition.Parameters);
                    }
                    propertiesQuery = propertiesQuery.Where(combinedFloorCondition);
                }
            }


            // 2.8 坪數範圍篩選
            if (minArea.HasValue || maxArea.HasValue)
            {
                if (minArea.HasValue)
                {
                    propertiesQuery = propertiesQuery.Where(p => p.Area >= minArea.Value);
                }
                if (maxArea.HasValue)
                {
                    propertiesQuery = propertiesQuery.Where(p => p.Area <= maxArea.Value);
                }
            }
            // 如果沒有自定義坪數範圍，則處理多選的預設坪數範圍
            else if (areaRanges != null && areaRanges.Any())
            {
                var areaConditions = new List<Expression<Func<Property, bool>>>();

                foreach (var range in areaRanges)
                {
                    var parts = range.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int min) && int.TryParse(parts[1], out int max))
                    {
                        areaConditions.Add(p => p.Area >= min && p.Area <= max);
                    }
                    // TODO: 考慮處理無效或單一數字的範圍字符串
                }

                if (areaConditions.Any())
                {
                    // 使用 OR 邏輯組合所有坪數範圍條件
                    var combinedAreaCondition = areaConditions.First();
                    for (int i = 1; i < areaConditions.Count; i++)
                    {
                        combinedAreaCondition = Expression.Lambda<Func<Property, bool>>(
                            Expression.OrElse(combinedAreaCondition.Body, Expression.Invoke(areaConditions[i], combinedAreaCondition.Parameters)),
                            combinedAreaCondition.Parameters);
                    }
                    propertiesQuery = propertiesQuery.Where(combinedAreaCondition);
                }
            }
            var allSelectedCategoryIds = new HashSet<int>();

            // 輔助函式，用於解析字串並查詢對應的 CategoryId
            // 由於邏輯重複，將其封裝成一個方法來提高可讀性和維護性
            async Task AddCategoryIdsFromString(string? categoryNamesString)
            {
                if (!string.IsNullOrEmpty(categoryNamesString))
                {
                    var names = categoryNamesString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(s => s.Trim())
                                                   .ToList();
                    if (names.Any())
                    {
                        //比對 CategoryName：Where(pec => names.Contains(pec.CategoryName)) 這一部分會將從前端接收到的 names 列表
                        //與PropertyEquipmentCategories 表中的 CategoryName 欄位進行比對，確保所有的 CategoryId。
                        var ids = await _context.PropertyEquipmentCategories
                                                .Where(pec => names.Contains(pec.CategoryName))
                                                .Select(pec => pec.CategoryId)
                                                .ToListAsync();
                        allSelectedCategoryIds.UnionWith(ids);
                    }
                }
            }

            // 調用輔助函式處理 features, facilities, equipments 參數
            await AddCategoryIdsFromString(features);
            await AddCategoryIdsFromString(facilities);
            await AddCategoryIdsFromString(equipments);

            //比對allSelectedCategoryIds 在.PropertyEquipmentRelations表中的CategoryId
            if (allSelectedCategoryIds.Any())
            {
                propertiesQuery = propertiesQuery.Where(p =>
                    allSelectedCategoryIds.All(selectedId =>
                        _context.PropertyEquipmentRelations.Any(per =>
                            per.PropertyId == p.PropertyId && per.CategoryId == selectedId
                        )
                    )
                );
            }

            // --- 3. 獲取總數 (在排序和分頁之前) ---
            int totalCount = await propertiesQuery.CountAsync();

            // --- 4. 處理排序 ---
            switch (sortField)
            {
                case "MonthlyRent":
                    propertiesQuery = (sortOrder == "asc") ? propertiesQuery.OrderBy(p => p.MonthlyRent) : propertiesQuery.OrderByDescending(p => p.MonthlyRent);
                    break;
                case "Area":
                    propertiesQuery = (sortOrder == "asc") ? propertiesQuery.OrderBy(p => p.Area) : propertiesQuery.OrderByDescending(p => p.Area);
                    break;
                case "PublishedAt":
                default:
                    propertiesQuery = (sortOrder == "asc") ? propertiesQuery.OrderBy(p => p.PublishedAt) : propertiesQuery.OrderByDescending(p => p.PublishedAt);
                    break;
            }

            // --- 5. 應用分頁 (在 Select 之前，以確保分頁的正確性) ---
            propertiesQuery = propertiesQuery
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize);

            // --- 6. 最終的 Select 投影：使用 Join 獲取 CityName 和 DistrictName ---
            // 在這裡進行 Join，將篩選和分頁後的房源與 City 和 District 表聯結
            var propertyData = await (from p in propertiesQuery
                                      join c in _context.Cities on p.CityId equals c.CityId
                                      join d in _context.Districts on p.DistrictId equals d.DistrictId
                                      where c.IsActive && d.IsActive // 確保城市和行政區是啟用的
                                      select new
                                      {
                                          PropertyId = p.PropertyId,
                                          Title = p.Title,
                                          AddressLine = p.AddressLine,
                                          RoomCount = p.RoomCount,
                                          LivingRoomCount = p.LivingRoomCount,
                                          BathroomCount = p.BathroomCount,
                                          CurrentFloor = p.CurrentFloor,
                                          TotalFloors = p.TotalFloors,
                                          Area = p.Area,
                                          MonthlyRent = p.MonthlyRent,
                                          PublishedAt = p.PublishedAt,
                                          Features = _context.PropertyEquipmentRelations
                                                 .Where(per => per.PropertyId == p.PropertyId)
                                                 .Select(per => per.Category.CategoryName)
                                                 .ToList()
                                      }).ToListAsync();

            // --- 7. 在記憶體中處理圖片路徑和最終轉換 ---
            var houseListings = new List<SearchViewModel>();
            
            foreach (var property in propertyData)
            {
                // 從 Images 表獲取 Azure Blob 圖片路徑
                var imageInfo = await _context.Images
                    .Where(img => img.EntityId == property.PropertyId && 
                                  img.EntityType == EntityType.Property && 
                                  img.Category == ImageCategory.Gallery && 
                                  img.IsActive)
                    .OrderBy(img => img.DisplayOrder ?? int.MaxValue)
                    .ThenBy(img => img.UploadedAt)
                    .Select(img => new { img.Category, img.EntityId, img.ImageGuid })
                    .FirstOrDefaultAsync();

                string imagePath = null;
                if (imageInfo != null)
                {
                    imagePath = _blobUrlGenerator.GenerateImageUrl(
                        imageInfo.Category, 
                        imageInfo.EntityId, 
                        imageInfo.ImageGuid, 
                        ImageSize.Medium);
                }

                houseListings.Add(new SearchViewModel
                {
                    PropertyId = property.PropertyId,
                    Title = property.Title,
                    AddressLine = property.AddressLine,
                    RoomCount = property.RoomCount,
                    LivingRoomCount = property.LivingRoomCount,
                    BathroomCount = property.BathroomCount,
                    CurrentFloor = property.CurrentFloor,
                    TotalFloors = property.TotalFloors,
                    Area = property.Area,
                    MonthlyRent = property.MonthlyRent,
                    ImagePath = imagePath, // 使用 Azure Blob 路徑，沒有時為 null
                    PublishedAt = property.PublishedAt,
                    Features = property.Features,
                    IsFavorited = false
                });
            }


            // --- 8. 返回結果 ---
            var pagedResult = new PagedSearchResultViewModel
            {
                Properties = houseListings,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount // 總數依然使用原始查詢的總數
            };

            return Json(pagedResult);
        }

        // TODO: 您可能還需要為收藏功能添加 API 端點 (例如 POST /api/User/Favorite/{propertyId} 和 DELETE /api/User/Favorite/{propertyId})
        // 這將需要處理用戶認證和資料庫的收藏表 (例如 Favorites 表)。
    }
}