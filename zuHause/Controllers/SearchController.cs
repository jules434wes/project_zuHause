// Controllers/SearchController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // 用於 DbSet 和各種 LINQ 擴展方法 (如 Where, CountAsync, ToListAsync)
using System.Collections.Generic; // 用於 List<T> 和 HashSet<T>
using System.Linq; // 用於 LINQ 查詢方法 (如 Select, Contains, Any, OrderBy, GroupBy, Join 等)
using System.Threading.Tasks; // 用於非同步操作 (如 async/await)
using zuHause.Models; // 引入資料庫模型，例如 Property, City, District 等
using zuHause.ViewModels.TenantViewModel; // 引入用於前端顯示的 ViewModel
using System; // 用於 Math.Ceiling (計算總頁數)

namespace zuHause.Controllers
{
    // SearchController 負責處理房源搜尋相關的 API 請求
    public class SearchController : Controller
    {
        private readonly ZuHauseContext _context; // 資料庫上下文，用於與資料庫互動

        // 建構子：透過依賴注入 (DI) 獲取資料庫上下文實例
        public SearchController(ZuHauseContext context)
        {
            _context = context;
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
            [FromQuery] List<string>? districtCodes = null, // 行政區代碼列表 (例如 ["BTT", "NTN"])
                                                           // [FromQuery] 確保 ASP.NET Core 能正確綁定多個同名查詢參數到 List<T>
            int? minRent = null, // 最小租金
            int? maxRent = null, // 最大租金
            [FromQuery] List<int>? roomCounts = null, // 房間數列表 (例如 [1, 2, 4])
            [FromQuery] List<int>? bathroomCounts = null, // 衛浴數列表 (例如 [1, 2, 4])
            int? minFloor = null, // 最小樓層
            int? maxFloor = null, // 最大樓層
            int? minArea = null, // 最小坪數
            int? maxArea = null, // 最大坪數
            [FromQuery] List<int>? featureCategoryIds = null, // 特色類別 ID 列表
            [FromQuery] List<int>? facilityCategoryIds = null, // 設施類別 ID 列表
            [FromQuery] List<int>? equipmentCategoryIds = null // 設備類別 ID 列表
        )
        {
            // 1. 初始查詢：
            // 從 Properties 表開始查詢，只選擇 DeletedAt 為 null (未刪除) 和p.IsPaid == true(已付款)。

            IQueryable<Property> propertiesQuery = _context.Properties
                                                            .Where(p => p.DeletedAt == null && p.IsPaid == true);

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
            if (districtCodes != null && districtCodes.Any())
            {
                // 在這裡直接使用上面已經確定的 currentCityId
                var districtIds = await _context.Districts
                                                .Where(d => districtCodes.Contains(d.DistrictCode) && d.IsActive &&
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
            if (minRent.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.MonthlyRent >= minRent.Value);
            }
            if (maxRent.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.MonthlyRent <= maxRent.Value);
            }

            // 2.5 格局篩選 (房間數，多選)
            if (roomCounts != null && roomCounts.Any())
            {
                if (roomCounts.Count == 1 && roomCounts.Contains(4))
                {
                    propertiesQuery = propertiesQuery.Where(p => p.RoomCount >= 4);
                }
                else
                {
                    propertiesQuery = propertiesQuery.Where(p => roomCounts.Contains(p.RoomCount));
                }
            }

            // 2.6 衛浴篩選 (衛浴數，多選)
            if (bathroomCounts != null && bathroomCounts.Any())
            {
                if (bathroomCounts.Count == 1 && bathroomCounts.Contains(4))
                {
                    propertiesQuery = propertiesQuery.Where(p => p.BathroomCount >= 4);
                }
                else
                {
                    propertiesQuery = propertiesQuery.Where(p => bathroomCounts.Contains(p.BathroomCount));
                }
            }

            // 2.7 樓層範圍篩選
            if (minFloor.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.CurrentFloor >= minFloor.Value);
            }
            if (maxFloor.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.CurrentFloor <= maxFloor.Value);
            }

            // 2.8 坪數範圍篩選
            if (minArea.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.Area >= minArea.Value);
            }
            if (maxArea.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.Area <= maxArea.Value);
            }

            // 2.9 特色、設施、設備篩選 (多選，直接接收 CategoryID)
            //這初始化了一個 HashSet<int> 集合。HashSet 是一個無序的集合，它的主要優勢是查詢（例如 Contains()）的效率非常高
            //，並且它會自動處理重複的元素（如果多次添加相同的 ID，只會儲存一份）。
            //這個集合將用來儲存所有從前端傳入的特色、設施和設備的分類 ID。
            var allSelectedCategoryIds = new HashSet<int>();
            if (featureCategoryIds != null) allSelectedCategoryIds.UnionWith(featureCategoryIds);
            if (facilityCategoryIds != null) allSelectedCategoryIds.UnionWith(facilityCategoryIds);
            if (equipmentCategoryIds != null) allSelectedCategoryIds.UnionWith(equipmentCategoryIds);

            if (allSelectedCategoryIds.Any())
            {
                propertiesQuery = propertiesQuery.Where(p =>
                    _context.PropertyEquipmentRelations
                            .Where(per => per.PropertyId == p.PropertyId && allSelectedCategoryIds.Contains(per.CategoryId))
                            .Select(per => per.CategoryId)
                            .Distinct()
                            .Count() == allSelectedCategoryIds.Count
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
            var houseListings = await (from p in propertiesQuery
                                       join c in _context.Cities on p.CityId equals c.CityId
                                       join d in _context.Districts on p.DistrictId equals d.DistrictId
                                       where c.IsActive && d.IsActive // 確保城市和行政區是啟用的
                                       select new SearchViewModel
                                       {
                                           PropertyId = p.PropertyId,
                                           Title = p.Title,
                                           AddressLine = p.AddressLine, // AddressLine 已包含 CityName 和 DistrictName
                                           RoomCount = p.RoomCount,
                                           LivingRoomCount = p.LivingRoomCount,
                                           BathroomCount = p.BathroomCount,
                                           CurrentFloor = p.CurrentFloor,
                                           TotalFloors = p.TotalFloors,
                                           Area = p.Area,
                                           MonthlyRent = p.MonthlyRent,
                                           ImagePath = p.PreviewImageUrl, // 使用 PreviewImageUrl
                                           PublishedAt = p.PublishedAt,
                                           // Features 需要額外載入或在這邊做子查詢
                                           Features = _context.PropertyEquipmentRelations
                                                  .Where(per => per.PropertyId == p.PropertyId)
                                                  .Select(per => per.Category.CategoryName) // 直接使用導航屬性獲取 CategoryName
                                                  .ToList(),
                                           IsFavorited = false // TODO: 此處需要根據用戶登入狀態判斷是否收藏
                                       }).ToListAsync(); // 執行資料庫查詢


            // --- 7. 返回結果 ---
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