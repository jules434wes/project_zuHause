using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.ViewModels;
using zuHause.Models;

namespace zuHause.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ZuHauseContext _context;
        private readonly ILogger<PropertyController> _logger;

        public PropertyController(ZuHauseContext context, ILogger<PropertyController> logger)
        {
            _context = context;
            _logger = logger;
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
                // 從資料庫載入房源詳細資訊
                var property = await _context.Properties
                    .Include(p => p.LandlordMember)
                    .Include(p => p.PropertyImages)
                    .Include(p => p.PropertyEquipmentRelations)
                    .FirstOrDefaultAsync(p => p.PropertyId == id);

                if (property == null)
                {
                    return NotFound("找不到指定的房源");
                }

                // 建立 ViewModel
                var viewModel = new PropertyDetailViewModel
                {
                    PropertyId = property.PropertyId,
                    Title = property.Title,
                    Description = property.Description ?? "",
                    Price = property.MonthlyRent,
                    Address = property.AddressLine ?? "",
                    CityName = "台北市",
                    DistrictName = "大安區",
                    LandlordName = property.LandlordMember?.MemberName ?? "未提供",
                    LandlordPhone = "02-2345-6789",
                    LandlordEmail = "landlord@example.com",
                    CreatedDate = property.CreatedAt,
                    IsActive = true,
                    IsFavorite = false,
                    ViewCount = 158,
                    FavoriteCount = 23,
                    ApplicationCount = 7,
                    Images = property.PropertyImages.Select(img => new PropertyImageViewModel
                    {
                        PropertyImageId = img.ImageId,
                        ImagePath = img.ImagePath,
                        ImageDescription = "",
                        IsMainImage = img.DisplayOrder == 1,
                        SortOrder = img.DisplayOrder
                    }).ToList(),
                    Equipment = property.PropertyEquipmentRelations.Select(eq => new PropertyEquipmentViewModel
                    {
                        EquipmentName = "設備項目",
                        EquipmentType = "基本設備",
                        Quantity = eq.Quantity,
                        Condition = "良好"
                    }).ToList(),
                    HouseInfo = new PropertyInfoSection
                    {
                        PropertyType = "公寓",
                        Floor = $"{property.CurrentFloor}/{property.TotalFloors}樓",
                        Area = $"{property.Area}坪",
                        Rooms = $"{property.RoomCount}房",
                        Bathrooms = $"{property.BathroomCount}衛",
                        Balcony = "1個",
                        Parking = "無",
                        Direction = "朝南",
                        Age = 15
                    },
                    RulesAndFees = new PropertyRulesSection
                    {
                        MonthlyRent = property.MonthlyRent,
                        Deposit = property.DepositAmount,
                        ManagementFee = 2000,
                        UtilityDeposit = 3000,
                        LeaseMinimum = "一年",
                        PaymentTerms = "押二付一",
                        HouseRules = new List<string> { "禁止吸菸", "可養寵物", "可開伙" },
                        AllowPets = true,
                        AllowSmoking = false,
                        AllowCooking = true
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