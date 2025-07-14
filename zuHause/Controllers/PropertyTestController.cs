using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels;

namespace zuHause.Controllers
{
    /// <summary>
    /// 房源測試控制器 - 提供假資料用於樣式測試
    /// </summary>
    public class PropertyTestController : Controller
    {
        /// <summary>
        /// 房源詳細資訊頁面 - 錨點導航版本
        /// </summary>
        /// <param name="id">房源 ID</param>
        /// <returns>房源詳細資訊視圖</returns>
        [Route("property-test/{id:int}")]
        [Route("property-test/detail/{id:int}")]
        public IActionResult Detail(int id = 1)
        {
            var viewModel = CreateFakePropertyDetailViewModel(id);
            return View("~/Views/Property/Detail.cshtml", viewModel);
        }

        /// <summary>
        /// 房源列表頁面 - 使用假資料
        /// </summary>
        /// <returns>房源列表視圖</returns>
        [Route("properties-test")]
        public IActionResult Index()
        {
            var viewModel = CreateFakePropertyListViewModel();
            return View("~/Views/Property/Index.cshtml", viewModel);
        }

        /// <summary>
        /// 建立假的房源詳細資訊 ViewModel
        /// </summary>
        /// <param name="propertyId">房源 ID</param>
        /// <returns>PropertyDetailViewModel</returns>
        private PropertyDetailViewModel CreateFakePropertyDetailViewModel(int propertyId)
        {
            var viewModel = new PropertyDetailViewModel
            {
                PropertyId = propertyId,
                Title = "大安區精美兩房公寓",
                Description = "位於大安區核心地帶，近捷運站，生活機能完善。全新裝潢，採光良好，適合小家庭或上班族。周邊有多所知名學校，交通便利，步行5分鐘可達捷運大安站。房屋保養良好，格局方正，每間房都有對外窗，通風採光極佳。社區管理完善，有中庭花園和健身房設施。",
                Price = 35000,
                Address = "台北市大安區復興南路一段100號",
                CityName = "台北市",
                DistrictName = "大安區",
                LandlordName = "張先生",
                LandlordPhone = "0912-345-678",
                LandlordEmail = "landlord@example.com",
                CreatedDate = new DateTime(2025, 6, 29), // 2025-06-29
                IsActive = true,
                IsFavorite = false,
                FavoriteCount = 12,
                ApplicationCount = 3,
                ViewCount = 156,

                // 導覽項目
                NavigationItems = new List<NavigationItem>
                {
                    new NavigationItem { Id = "house-info", Title = "房屋資訊", Icon = "fas fa-home" },
                    new NavigationItem { Id = "rules-fees", Title = "費用守則", Icon = "fas fa-money-bill" },
                    new NavigationItem { Id = "facilities", Title = "設備服務", Icon = "fas fa-cogs" },
                    new NavigationItem { Id = "description", Title = "描述", Icon = "fas fa-file-text" },
                    new NavigationItem { Id = "location", Title = "位置", Icon = "fas fa-map-marker-alt" }
                },

                // 房源圖片 (使用更穩定的圖片源)
                Images = new List<PropertyImageViewModel>
                {
                    new PropertyImageViewModel
                    {
                        PropertyImageId = 1,
                        ImagePath = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800&h=600&fit=crop",
                        ImageDescription = "客廳",
                        IsMainImage = true,
                        SortOrder = 1
                    },
                    new PropertyImageViewModel
                    {
                        PropertyImageId = 2,
                        ImagePath = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&h=600&fit=crop",
                        ImageDescription = "主臥室",
                        IsMainImage = false,
                        SortOrder = 2
                    },
                    new PropertyImageViewModel
                    {
                        PropertyImageId = 3,
                        ImagePath = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=800&h=600&fit=crop",
                        ImageDescription = "廚房",
                        IsMainImage = false,
                        SortOrder = 3
                    },
                    new PropertyImageViewModel
                    {
                        PropertyImageId = 4,
                        ImagePath = "https://images.unsplash.com/photo-1552321554-5fefe8c9ef14?w=800&h=600&fit=crop",
                        ImageDescription = "浴室",
                        IsMainImage = false,
                        SortOrder = 4
                    },
                    new PropertyImageViewModel
                    {
                        PropertyImageId = 5,
                        ImagePath = "https://images.unsplash.com/photo-1572120360610-d971b9d7767c?w=800&h=600&fit=crop",
                        ImageDescription = "陽台",
                        IsMainImage = false,
                        SortOrder = 5
                    }
                },

                // 房源設備
                Equipment = new List<PropertyEquipmentViewModel>
                {
                    new PropertyEquipmentViewModel { EquipmentName = "冷氣", EquipmentType = "電器", Quantity = 2, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "洗衣機", EquipmentType = "電器", Quantity = 1, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "冰箱", EquipmentType = "電器", Quantity = 1, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "電視", EquipmentType = "電器", Quantity = 1, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "微波爐", EquipmentType = "電器", Quantity = 1, Condition = "良好" },
                    
                    new PropertyEquipmentViewModel { EquipmentName = "雙人床", EquipmentType = "家具", Quantity = 1, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "書桌", EquipmentType = "家具", Quantity = 2, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "衣櫃", EquipmentType = "家具", Quantity = 2, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "沙發", EquipmentType = "家具", Quantity = 1, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "餐桌", EquipmentType = "家具", Quantity = 1, Condition = "良好" },
                    
                    new PropertyEquipmentViewModel { EquipmentName = "WiFi", EquipmentType = "其他", Quantity = 1, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "有線電視", EquipmentType = "其他", Quantity = 1, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "管理費含", EquipmentType = "其他", Quantity = 1, Condition = "良好" },
                    new PropertyEquipmentViewModel { EquipmentName = "停車位", EquipmentType = "其他", Quantity = 1, Condition = "良好" }
                },

                // 房屋資訊
                HouseInfo = new PropertyInfoSection
                {
                    PropertyType = "公寓",
                    Floor = "3樓/5樓",
                    Area = "25坪",
                    Rooms = "2房1廳1衛",
                    Bathrooms = "1間",
                    Balcony = "有",
                    Parking = "有",
                    Age = 8
                },

                // 費用和守則
                RulesAndFees = new PropertyRulesSection
                {
                    MonthlyRent = 35000,
                    Deposit = 70000,
                    ManagementFee = 3000,
                    UtilityDeposit = 5000,
                    LeaseMinimum = "12個月",
                    PaymentTerms = "押二付一",
                    AllowPets = false,
                    AllowCooking = true,
                    HouseRules = new List<string>
                    {
                        "禁止吸菸",
                        "不可養寵物",
                        "22:00後請降低音量",
                        "垃圾請分類回收",
                        "不可私自更改室內裝潢",
                        "訪客過夜請事先告知"
                    }
                },

                // 位置資訊
                Location = new PropertyLocationSection
                {
                    NearbyTransport = "捷運大安站步行5分鐘，公車站牌步行2分鐘，交通便利四通八達"
                }
            };

            return viewModel;
        }

        /// <summary>
        /// 建立假的房源列表 ViewModel
        /// </summary>
        /// <returns>PropertyListViewModel</returns>
        private PropertyListViewModel CreateFakePropertyListViewModel()
        {
            var properties = new List<PropertySummaryViewModel>
            {
                new PropertySummaryViewModel
                {
                    PropertyId = 1,
                    Title = "大安區精美兩房公寓",
                    Price = 35000,
                    Address = "台北市大安區復興南路一段100號",
                    CityName = "台北市",
                    DistrictName = "大安區",
                    MainImagePath = "https://picsum.photos/400/300?random=1",
                    CreatedDate = new DateTime(2025, 6, 29), // 2025-06-29
                    IsFavorite = false,
                    ViewCount = 156
                },
                new PropertySummaryViewModel
                {
                    PropertyId = 2,
                    Title = "信義區現代化套房",
                    Price = 28000,
                    Address = "台北市信義區松高路50號",
                    CityName = "台北市",
                    DistrictName = "信義區",
                    MainImagePath = "https://picsum.photos/400/300?random=2",
                    CreatedDate = new DateTime(2025, 7, 2), // 2025-07-02
                    IsFavorite = true,
                    ViewCount = 89
                },
                new PropertySummaryViewModel
                {
                    PropertyId = 3,
                    Title = "板橋區溫馨三房",
                    Price = 25000,
                    Address = "新北市板橋區文化路二段200號",
                    CityName = "新北市",
                    DistrictName = "板橋區",
                    MainImagePath = "https://picsum.photos/400/300?random=3",
                    CreatedDate = new DateTime(2025, 7, 4), // 2025-07-04
                    IsFavorite = false,
                    ViewCount = 234
                },
                new PropertySummaryViewModel
                {
                    PropertyId = 4,
                    Title = "內湖區科技園區套房",
                    Price = 22000,
                    Address = "台北市內湖區瑞光路100號",
                    CityName = "台北市",
                    DistrictName = "內湖區",
                    MainImagePath = "https://picsum.photos/400/300?random=4",
                    CreatedDate = new DateTime(2025, 7, 6), // 2025-07-06
                    IsFavorite = false,
                    ViewCount = 67
                },
                new PropertySummaryViewModel
                {
                    PropertyId = 5,
                    Title = "中山區商務公寓",
                    Price = 32000,
                    Address = "台北市中山區林森北路80號",
                    CityName = "台北市",
                    DistrictName = "中山區",
                    MainImagePath = "https://picsum.photos/400/300?random=5",
                    CreatedDate = new DateTime(2025, 7, 9), // 2025-07-09
                    IsFavorite = true,
                    ViewCount = 145
                },
                new PropertySummaryViewModel
                {
                    PropertyId = 6,
                    Title = "新莊區家庭式住宅",
                    Price = 30000,
                    Address = "新北市新莊區中正路300號",
                    CityName = "新北市",
                    DistrictName = "新莊區",
                    MainImagePath = "https://picsum.photos/400/300?random=6",
                    CreatedDate = new DateTime(2025, 7, 11), // 2025-07-11
                    IsFavorite = false,
                    ViewCount = 78
                }
            };

            var viewModel = new PropertyListViewModel
            {
                Properties = properties,
                TotalCount = properties.Count,
                CurrentPage = 1,
                PageSize = 12,
                TotalPages = 1,
                SearchCriteria = new PropertySearchViewModel()
            };

            return viewModel;
        }
    }
}