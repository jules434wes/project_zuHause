using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.ViewModels;
using zuHause.Models;
using zuHause.Interfaces;

namespace zuHause.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ZuHauseContext _context;
        private readonly ILogger<PropertyController> _logger;
        private readonly IPropertyImageService _propertyImageService;

        public PropertyController(ZuHauseContext context, ILogger<PropertyController> logger, IPropertyImageService propertyImageService)
        {
            _context = context;
            _logger = logger;
            _propertyImageService = propertyImageService;
        }

        /// <summary>
        /// 房源詳細資訊頁面
        /// </summary>
        /// <param name="id">房源 ID</param>
        /// <returns>房源詳細資訊視圖</returns>
        [Route("property/{id:int}")]
        [Route("property/detail/{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
        {
                // 從資料庫載入房源詳細資訊，包含設備分類
                var property = await _context.Properties
                    .Include(p => p.LandlordMember)
                    .Include(p => p.PropertyImages)
                    .Include(p => p.PropertyEquipmentRelations)
                        .ThenInclude(r => r.Category)
                            .ThenInclude(c => c.ParentCategory)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PropertyId == id);

                if (property == null)
                {
                    return NotFound("找不到指定的房源");
                }

                // 取得縣市/區域名稱
                var cityName = await _context.Cities
                    .Where(c => c.CityId == property.CityId)
                    .Select(c => c.CityName)
                    .FirstOrDefaultAsync() ?? string.Empty;

                var districtName = await _context.Districts
                    .Where(d => d.DistrictId == property.DistrictId)
                    .Select(d => d.DistrictName)
                    .FirstOrDefaultAsync() ?? string.Empty;

                // 取得圖片 (使用統一圖片管理系統)
                var images = await _propertyImageService.GetPropertyImagesAsync(id);

                // 建立 ViewModel
                var viewModel = new PropertyDetailViewModel
                {
                    PropertyId = property.PropertyId,
                    Title = property.Title,
                    Description = property.Description ?? string.Empty,
                    Price = property.MonthlyRent,
                    Address = property.AddressLine ?? string.Empty,
                    CityName = cityName,
                    DistrictName = districtName,
                    LandlordName = property.LandlordMember?.MemberName ?? string.Empty,
                    LandlordPhone = property.LandlordMember?.PhoneNumber ?? string.Empty,
                    LandlordEmail = property.LandlordMember?.Email ?? string.Empty,
                    CreatedDate = property.CreatedAt,
                    IsActive = true,
                    IsFavorite = false,
                    ViewCount = 158,
                    FavoriteCount = 23,
                    ApplicationCount = 7,
                    Images = images
                        .OrderBy(img => img.DisplayOrder ?? int.MaxValue)
                        .ThenBy(img => img.ImageId)
                        .Select(img => new PropertyImageViewModel
                    {
                        PropertyImageId = (int)img.ImageId,
                        ImagePath = _propertyImageService.GeneratePropertyImageUrl(img.StoredFileName!),
                        ImageDescription = img.OriginalFileName,
                        IsMainImage = img.DisplayOrder == 1,
                        SortOrder = img.DisplayOrder ?? 0
                    }).ToList(),
                    Equipment = property.PropertyEquipmentRelations.Select(eq => new PropertyEquipmentViewModel
                    {
                        EquipmentName = eq.Category.CategoryName,
                        EquipmentType = eq.Category.ParentCategory?.CategoryName ?? eq.Category.CategoryName,
                        Quantity = eq.Quantity,
                        Condition = string.Empty
                    }).ToList(),
                    HouseInfo = new PropertyInfoSection
                    {
                        PropertyType = "公寓",
                        Floor = $"{property.CurrentFloor}/{property.TotalFloors}樓",
                        Area = $"{property.Area}坪",
                        Rooms = $"{property.RoomCount}房",
                        Bathrooms = $"{property.BathroomCount}衛",
                        Balcony = "1個",
                        Parking = property.ParkingAvailable ? "有" : "無",
                        Direction = "朝南",
                        Age = 15
                    },
                    RulesAndFees = new PropertyRulesSection
                    {
                        MonthlyRent = property.MonthlyRent,
                        Deposit = property.DepositAmount,
                        ManagementFee = property.ManagementFeeAmount ?? 0,
                        UtilityDeposit = 3000,
                        LeaseMinimum = "一年",
                        PaymentTerms = "押二付一",
                        HouseRules = new List<string>(),
                        AllowPets = property.SpecialRules?.Contains("寵物") ?? false,
                        AllowSmoking = property.SpecialRules?.Contains("吸菸") ?? false,
                        AllowCooking = property.SpecialRules?.Contains("開伙") ?? false
                    },
                    Location = new PropertyLocationSection
                    {
                        Latitude = 25.0330,
                        Longitude = 121.5654,
                        NearbyTransport = "捷運信義安和站步行5分鐘",
                        NearbySchools = "師大附中、台大",
                        NearbyShopping = "信義商圈、101購物中心",
                        NearbyHospitals = "台大醫院、榮總",
                        NearbyAttractions = new List<string> { "大安森林公園", "信義商圈", "101大樓" }
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入房源詳細資訊時發生錯誤，房源 ID: {PropertyId}", id);
                return View("Error");
            }
        }

        /// <summary>
        /// 房源列表頁面
        /// </summary>
        /// <returns>房源列表視圖</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                var properties = await _context.Properties
                    .Include(p => p.LandlordMember)
                    .Include(p => p.PropertyImages)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(20)
                    .Select(p => new PropertySummaryViewModel
                    {
                        PropertyId = p.PropertyId,
                        Title = p.Title,
                        Price = p.MonthlyRent,
                        Address = p.AddressLine ?? "",
                        CityName = "台北市",
                        DistrictName = "大安區",
                        MainImagePath = p.PropertyImages.Any() ? p.PropertyImages.First().ImagePath : "/images/default-property.jpg",
                        CreatedDate = p.CreatedAt,
                        IsFavorite = false,
                        ViewCount = 158
                    })
                    .ToListAsync();

                var viewModel = new PropertyListViewModel
                {
                    Properties = properties,
                    TotalCount = properties.Count()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入房源列表時發生錯誤");
                return View("Error");
            }
        }
    }
}