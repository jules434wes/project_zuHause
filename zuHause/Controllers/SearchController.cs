// Controllers/SearchController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using zuHause.Models;
using zuHause.ViewModels.TenantViewModel;
using System; // For Math.Ceiling

namespace zuHause.Controllers
{

    public class SearchController : Controller
    {
        private readonly ZuHauseContext _context;

        public SearchController(ZuHauseContext context)
        {
            _context = context;
        }


        // 修改 Search 方法，使其可以接收初始的篩選和分頁參數 (可選，如果希望首頁載入時就有分頁)
        // 這裡我們先保持原樣，讓 GetSearchList 處理實際的分頁數據
        public IActionResult Search()
        {
            return View();
        }

        [HttpGet("api/Tenant/Search/list")]
        public async Task<IActionResult> GetSearchList(
            string sortField = "PublishedAt",
            string sortOrder = "desc",
            int pageNumber = 1, // 新增分頁參數
            int pageSize = 6)   // 新增分頁參數，每頁6個
        {
            IQueryable<Property> propertiesQuery = _context.Properties;

            // TODO: 在這裡添加篩選條件 (如果用戶從首頁或其他地方傳遞了篩選參數)
            // 例如:
            // if (!string.IsNullOrEmpty(city))
            // {
            //     propertiesQuery = propertiesQuery.Where(p => p.City == city);
            // }

            // 獲取總數 (在應用 Skip/Take 之前獲取)
            int totalCount = await propertiesQuery.CountAsync();

            // 處理排序
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

            // 應用分頁
            var properties = await propertiesQuery
                                .Skip((pageNumber - 1) * pageSize) // 跳過之前的頁面數據
                                .Take(pageSize)                   // 獲取當前頁的數據
                                .ToListAsync();

            var propertyEquipments = await _context.PropertyEquipmentRelations
                                                    .Include(per => per.Category)
                                                    .ToListAsync();

            var houseListings = new List<SearchViewModel>();

            foreach (var prop in properties)
            {
                string imageUrl = "/images/placeholder-house.png"; // 預設圖片路徑
                var firstImage = await _context.PropertyImages
                                               .Where(img => img.PropertyId == prop.PropertyId)
                                               .OrderBy(img => img.DisplayOrder)
                                               .FirstOrDefaultAsync();
                if (firstImage != null && !string.IsNullOrEmpty(firstImage.ImagePath))
                {
                    imageUrl = firstImage.ImagePath;
                }

                var features = propertyEquipments
                                    .Where(pe => pe.PropertyId == prop.PropertyId)
                                    .Select(pe => pe.Category.CategoryName)
                                    .ToList();

                houseListings.Add(new SearchViewModel
                {
                    PropertyId = prop.PropertyId,
                    Title = prop.Title,
                    AddressLine = prop.AddressLine,
                    RoomCount = prop.RoomCount,
                    LivingRoomCount = prop.LivingRoomCount,
                    BathroomCount = prop.BathroomCount,
                    CurrentFloor = prop.CurrentFloor,
                    TotalFloors = prop.TotalFloors,
                    Area = prop.Area,
                    MonthlyRent = prop.MonthlyRent,
                    Features = features,
                    ImagePath = imageUrl,
                    PublishedAt = prop.PublishedAt
                });
            }

            // 將結果包裝在新的 PagedSearchResultViewModel 中
            var pagedResult = new PagedSearchResultViewModel
            {
                Properties = houseListings,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            // 注意：現在這個 API 端點應該返回 JSON，而不是 View，因為前端會使用 AJAX 來消費它。
            return Json(pagedResult); // 返回 JSON
        }
    }
}