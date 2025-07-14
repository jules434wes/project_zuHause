using zuHause.Models;

namespace zuHause.ViewModels
{
    public class PropertyDetailViewModel
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Address { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string LandlordName { get; set; } = string.Empty;
        public string LandlordPhone { get; set; } = string.Empty;
        public string LandlordEmail { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsFavorite { get; set; }
        
        // 房源圖片
        public List<PropertyImageViewModel> Images { get; set; } = new List<PropertyImageViewModel>();
        
        // 房源設備 - 按類型分組
        public List<PropertyEquipmentViewModel> Equipment { get; set; } = new List<PropertyEquipmentViewModel>();
        
        // 統計資訊
        public int ViewCount { get; set; }
        public int FavoriteCount { get; set; }
        public int ApplicationCount { get; set; }
        
        // 房屋詳細資訊
        public PropertyInfoSection HouseInfo { get; set; } = new PropertyInfoSection();
        public PropertyRulesSection RulesAndFees { get; set; } = new PropertyRulesSection();
        public PropertyLocationSection Location { get; set; } = new PropertyLocationSection();
        
        // 導覽項目
        public List<NavigationItem> NavigationItems { get; set; } = new List<NavigationItem>
        {
            new NavigationItem { Id = "house-info", Title = "房屋資訊", Icon = "fas fa-home" },
            new NavigationItem { Id = "rules-fees", Title = "費用及守則", Icon = "fas fa-money-bill" },
            new NavigationItem { Id = "facilities", Title = "設備與服務", Icon = "fas fa-cogs" },
            new NavigationItem { Id = "description", Title = "描述", Icon = "fas fa-file-text" },
            new NavigationItem { Id = "location", Title = "位置", Icon = "fas fa-map-marker-alt" }
        };
    }
    
    public class PropertyImageViewModel
    {
        public int PropertyImageId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string ImageDescription { get; set; } = string.Empty;
        public bool IsMainImage { get; set; }
        public int SortOrder { get; set; }
    }
    
    public class PropertyEquipmentViewModel
    {
        public string EquipmentName { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Condition { get; set; } = string.Empty;
    }
    
    public class PropertyInfoSection
    {
        public string PropertyType { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Rooms { get; set; } = string.Empty;
        public string Bathrooms { get; set; } = string.Empty;
        public string Balcony { get; set; } = string.Empty;
        public string Parking { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public int Age { get; set; }
    }
    
    public class PropertyRulesSection
    {
        public decimal MonthlyRent { get; set; }
        public decimal Deposit { get; set; }
        public decimal ManagementFee { get; set; }
        public decimal UtilityDeposit { get; set; }
        public string LeaseMinimum { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = string.Empty;
        public List<string> HouseRules { get; set; } = new List<string>();
        public bool AllowPets { get; set; }
        public bool AllowSmoking { get; set; }
        public bool AllowCooking { get; set; }
    }
    
    public class PropertyLocationSection
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string NearbyTransport { get; set; } = string.Empty;
        public string NearbySchools { get; set; } = string.Empty;
        public string NearbyShopping { get; set; } = string.Empty;
        public string NearbyHospitals { get; set; } = string.Empty;
        public List<string> NearbyAttractions { get; set; } = new List<string>();
    }
    
    public class NavigationItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
    }
}