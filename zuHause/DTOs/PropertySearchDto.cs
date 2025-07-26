using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace zuHause.DTOs
{
    public class PropertySearchResultDto
    {
        public List<PropertyDto> Properties { get; set; } = new List<PropertyDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PropertyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string MainImageUrl { get; set; } = string.Empty;
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public decimal? Area { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class CreateApplicationDto
    {
        public string? Message { get; set; }
    }

    public class ApplicationDto
    {
        public int ApplicationId { get; set; }
        public int PropertyId { get; set; }
        public int ApplicantId { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class ImageDto
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMainImage { get; set; }
        public DateTime UploadTime { get; set; }
    }

    // Additional DTOs for existing PropertySearchController
    public class PropertySearchRequestDto
    {
        public string? Keyword { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? PropertyType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "newest";
    }

    public class PropertySearchResponseDto
    {
        public List<PropertySummaryDto> Properties { get; set; } = new List<PropertySummaryDto>();
        public PaginationDto Pagination { get; set; } = new PaginationDto();
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int SearchTime { get; set; }
    }

    public class PropertySummaryDto
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public decimal Rent { get; set; }
        public string Address { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public decimal? Area { get; set; }
        public int? RoomCount { get; set; }
        public int? BathroomCount { get; set; }
        public string? MainImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class PaginationDto
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    // 房源基本資訊建立 DTO - 用於房東提交房源基本資訊
    public class PropertyBasicInfoCreateDto
    {
        /// <summary>
        /// 房源標題，最多30字
        /// </summary>
        [Required(ErrorMessage = "房源標題為必填項目")]
        [StringLength(30, ErrorMessage = "房源標題最多30個字元")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 月租金，必須為正數
        /// </summary>
        [Required(ErrorMessage = "月租金為必填項目")]
        [Range(1.0, double.MaxValue, ErrorMessage = "月租金必須大於0")]
        [JsonPropertyName("monthlyRent")]
        public decimal MonthlyRent { get; set; }

        /// <summary>
        /// 城市ID
        /// </summary>
        [Required(ErrorMessage = "城市為必填項目")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的城市")]
        [JsonPropertyName("cityId")]
        public int CityId { get; set; }

        /// <summary>
        /// 區域ID
        /// </summary>
        [Required(ErrorMessage = "區域為必填項目")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的區域")]
        [JsonPropertyName("districtId")]
        public int DistrictId { get; set; }

        /// <summary>
        /// 詳細地址
        /// </summary>
        [Required(ErrorMessage = "詳細地址為必填項目")]
        [StringLength(255, ErrorMessage = "詳細地址最多255個字元")]
        [JsonPropertyName("addressLine")]
        public string AddressLine { get; set; } = string.Empty;

        /// <summary>
        /// 房數，必須為正數
        /// </summary>
        [Required(ErrorMessage = "房數為必填項目")]
        [Range(1, 20, ErrorMessage = "房數必須在1-20之間")]
        [JsonPropertyName("roomCount")]
        public int RoomCount { get; set; }

        /// <summary>
        /// 廳數，必須為正數
        /// </summary>
        [Required(ErrorMessage = "廳數為必填項目")]
        [Range(1, 10, ErrorMessage = "廳數必須在1-10之間")]
        [JsonPropertyName("livingRoomCount")]
        public int LivingRoomCount { get; set; }

        /// <summary>
        /// 衛數，必須為正數
        /// </summary>
        [Required(ErrorMessage = "衛數為必填項目")]
        [Range(1, 10, ErrorMessage = "衛數必須在1-10之間")]
        [JsonPropertyName("bathroomCount")]
        public int BathroomCount { get; set; }

        /// <summary>
        /// 坪數，必須為正數
        /// </summary>
        [Required(ErrorMessage = "坪數為必填項目")]
        [Range(0.1, 1000.0, ErrorMessage = "坪數必須在0.1-1000之間")]
        [JsonPropertyName("area")]
        public decimal Area { get; set; }

        /// <summary>
        /// 所在樓層
        /// </summary>
        [Required(ErrorMessage = "所在樓層為必填項目")]
        [Range(1, 200, ErrorMessage = "所在樓層必須在1-200之間")]
        [JsonPropertyName("currentFloor")]
        public int CurrentFloor { get; set; }

        /// <summary>
        /// 總樓層數
        /// </summary>
        [Required(ErrorMessage = "總樓層數為必填項目")]
        [Range(1, 200, ErrorMessage = "總樓層數必須在1-200之間")]
        [JsonPropertyName("totalFloors")]
        public int TotalFloors { get; set; }
    }

    // 房源基本資訊回應 DTO - 用於回傳建立成功的房源資訊
    public class PropertyBasicInfoResponseDto
    {
        /// <summary>
        /// 新建立的房源ID
        /// </summary>
        [JsonPropertyName("propertyId")]
        public int PropertyId { get; set; }

        /// <summary>
        /// 房源狀態 (預設為 PENDING)
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "PENDING";

        /// <summary>
        /// 建立時間
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 房源標題
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 房東會員ID
        /// </summary>
        [JsonPropertyName("landlordMemberId")]
        public int LandlordMemberId { get; set; }
    }

    // === 完整房源創建DTO系統 ===

    /// <summary>
    /// 完整房源創建DTO - 涵蓋所有房源創建欄位
    /// </summary>
    public class PropertyCreateDto
    {
        // === 房屋資訊區塊 ===
        
        /// <summary>
        /// 房源標題，最多30字
        /// </summary>
        [Required(ErrorMessage = "房源標題為必填項目")]
        [StringLength(30, ErrorMessage = "房源標題最多30個字元")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 月租金，必須為正數
        /// </summary>
        [Required(ErrorMessage = "月租金為必填項目")]
        [Range(1.0, double.MaxValue, ErrorMessage = "月租金必須大於0")]
        [JsonPropertyName("monthlyRent")]
        public decimal MonthlyRent { get; set; }

        /// <summary>
        /// 城市ID
        /// </summary>
        [Required(ErrorMessage = "城市為必填項目")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的城市")]
        [JsonPropertyName("cityId")]
        public int CityId { get; set; }

        /// <summary>
        /// 區域ID
        /// </summary>
        [Required(ErrorMessage = "區域為必填項目")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的區域")]
        [JsonPropertyName("districtId")]
        public int DistrictId { get; set; }

        /// <summary>
        /// 詳細地址
        /// </summary>
        [Required(ErrorMessage = "詳細地址為必填項目")]
        [StringLength(255, ErrorMessage = "詳細地址最多255個字元")]
        [JsonPropertyName("addressLine")]
        public string AddressLine { get; set; } = string.Empty;

        /// <summary>
        /// 房數，必須為正數
        /// </summary>
        [Required(ErrorMessage = "房數為必填項目")]
        [Range(1, 20, ErrorMessage = "房數必須在1-20之間")]
        [JsonPropertyName("roomCount")]
        public int RoomCount { get; set; }

        /// <summary>
        /// 廳數，必須為正數
        /// </summary>
        [Required(ErrorMessage = "廳數為必填項目")]
        [Range(1, 10, ErrorMessage = "廳數必須在1-10之間")]
        [JsonPropertyName("livingRoomCount")]
        public int LivingRoomCount { get; set; }

        /// <summary>
        /// 衛數，必須為正數
        /// </summary>
        [Required(ErrorMessage = "衛數為必填項目")]
        [Range(1, 10, ErrorMessage = "衛數必須在1-10之間")]
        [JsonPropertyName("bathroomCount")]
        public int BathroomCount { get; set; }

        /// <summary>
        /// 坪數，必須為正數
        /// </summary>
        [Required(ErrorMessage = "坪數為必填項目")]
        [Range(0.1, 1000.0, ErrorMessage = "坪數必須在0.1-1000之間")]
        [JsonPropertyName("area")]
        public decimal Area { get; set; }

        /// <summary>
        /// 所在樓層
        /// </summary>
        [Required(ErrorMessage = "所在樓層為必填項目")]
        [Range(1, 200, ErrorMessage = "所在樓層必須在1-200之間")]
        [JsonPropertyName("currentFloor")]
        public int CurrentFloor { get; set; }

        /// <summary>
        /// 總樓層數
        /// </summary>
        [Required(ErrorMessage = "總樓層數為必填項目")]
        [Range(1, 200, ErrorMessage = "總樓層數必須在1-200之間")]
        [JsonPropertyName("totalFloors")]
        public int TotalFloors { get; set; }

        // === 費用及守則區塊 ===

        /// <summary>
        /// 押金金額
        /// </summary>
        [Required(ErrorMessage = "押金金額為必填項目")]
        [Range(0.0, double.MaxValue, ErrorMessage = "押金金額不能為負數")]
        [JsonPropertyName("depositAmount")]
        public decimal DepositAmount { get; set; }

        /// <summary>
        /// 押金月數
        /// </summary>
        [Required(ErrorMessage = "押金月數為必填項目")]
        [Range(0, 12, ErrorMessage = "押金月數必須在0-12之間")]
        [JsonPropertyName("depositMonths")]
        public int DepositMonths { get; set; }

        /// <summary>
        /// 最短租期(月)，只可輸入6、12、24
        /// </summary>
        [Required(ErrorMessage = "最短租期為必填項目")]
        [RegularExpression("^(6|12|24)$", ErrorMessage = "最短租期只能選擇6個月、12個月或24個月")]
        [JsonPropertyName("minimumRentalMonths")]
        public int MinimumRentalMonths { get; set; }

        /// <summary>
        /// 管理費含租金，true為包含，false為須另計
        /// </summary>
        [Required(ErrorMessage = "管理費計算方式為必填項目")]
        [JsonPropertyName("managementFeeIncluded")]
        public bool ManagementFeeIncluded { get; set; }

        /// <summary>
        /// 管理費金額（須另計時必填）
        /// </summary>
        [JsonPropertyName("managementFeeAmount")]
        public decimal? ManagementFeeAmount { get; set; }

        /// <summary>
        /// 水費計算方式，台水/自訂金額
        /// </summary>
        [Required(ErrorMessage = "水費計算方式為必填項目")]
        [RegularExpression("^(台水|自訂金額)$", ErrorMessage = "水費計算方式只能選擇台水或自訂金額")]
        [JsonPropertyName("waterFeeType")]
        public string WaterFeeType { get; set; } = string.Empty;

        /// <summary>
        /// 自訂水費（選擇自訂金額時必填）
        /// </summary>
        [JsonPropertyName("customWaterFee")]
        public decimal? CustomWaterFee { get; set; }

        /// <summary>
        /// 電費計算方式，台電/自訂金額
        /// </summary>
        [Required(ErrorMessage = "電費計算方式為必填項目")]
        [RegularExpression("^(台電|自訂金額)$", ErrorMessage = "電費計算方式只能選擇台電或自訂金額")]
        [JsonPropertyName("electricityFeeType")]
        public string ElectricityFeeType { get; set; } = string.Empty;

        /// <summary>
        /// 自訂電費（選擇自訂金額時必填）
        /// </summary>
        [JsonPropertyName("customElectricityFee")]
        public decimal? CustomElectricityFee { get; set; }

        /// <summary>
        /// 特殊守則，最多輸入20字，可多筆
        /// </summary>
        [JsonPropertyName("specialRules")]
        public string? SpecialRules { get; set; }

        /// <summary>
        /// 是否須清潔費
        /// </summary>
        [Required(ErrorMessage = "請選擇是否須清潔費")]
        [JsonPropertyName("cleaningFeeRequired")]
        public bool CleaningFeeRequired { get; set; }

        /// <summary>
        /// 清潔費金額（須清潔費時必填）
        /// </summary>
        [JsonPropertyName("cleaningFeeAmount")]
        public decimal? CleaningFeeAmount { get; set; }

        /// <summary>
        /// 是否有停車位
        /// </summary>
        [Required(ErrorMessage = "請選擇是否有停車位")]
        [JsonPropertyName("parkingAvailable")]
        public bool ParkingAvailable { get; set; }

        /// <summary>
        /// 停車費須額外收費
        /// </summary>
        [JsonPropertyName("parkingFeeRequired")]
        public bool ParkingFeeRequired { get; set; }

        /// <summary>
        /// 停車位費用（停車費須額外收費時必填）
        /// </summary>
        [JsonPropertyName("parkingFeeAmount")]
        public decimal? ParkingFeeAmount { get; set; }

        // === 設備與服務區塊 ===

        /// <summary>
        /// 選中的設備ID列表
        /// </summary>
        [JsonPropertyName("selectedEquipmentIds")]
        public List<int> SelectedEquipmentIds { get; set; } = new List<int>();

        /// <summary>
        /// 設備數量設定（設備ID -> 數量）
        /// </summary>
        [JsonPropertyName("equipmentQuantities")]
        public Dictionary<int, int> EquipmentQuantities { get; set; } = new Dictionary<int, int>();

        // === 描述區塊 ===

        /// <summary>
        /// 房源描述，可選
        /// </summary>
        [StringLength(2000, ErrorMessage = "描述最多2000個字元")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        // === 刊登方案區塊 ===

        /// <summary>
        /// 選擇的刊登方案ID
        /// </summary>
        [Required(ErrorMessage = "請選擇刊登方案")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的刊登方案")]
        [JsonPropertyName("listingPlanId")]
        public int ListingPlanId { get; set; }

        // === 房產證明文件 ===

        /// <summary>
        /// 房產證明文件URL
        /// </summary>
        [JsonPropertyName("propertyProofUrl")]
        public string? PropertyProofUrl { get; set; }

        // === 編輯模式支援 ===

        /// <summary>
        /// 房源ID（編輯模式使用）
        /// </summary>
        [JsonPropertyName("propertyId")]
        public int? PropertyId { get; set; }
    }

    /// <summary>
    /// 房源設備選擇DTO
    /// </summary>
    public class PropertyEquipmentSelectionDto
    {
        /// <summary>
        /// 設備分類ID
        /// </summary>
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        /// <summary>
        /// 設備分類名稱
        /// </summary>
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 是否選中
        /// </summary>
        [JsonPropertyName("selected")]
        public bool Selected { get; set; }

        /// <summary>
        /// 數量（若適用）
        /// </summary>
        [Range(0, 100, ErrorMessage = "數量必須在0-100之間")]
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// 父分類ID（用於層級結構）
        /// </summary>
        [JsonPropertyName("parentCategoryId")]
        public int? ParentCategoryId { get; set; }
    }

    /// <summary>
    /// 房源圖片上傳DTO（整合雙語分類系統）
    /// </summary>
    public class PropertyImageUploadDto
    {
        /// <summary>
        /// 房源ID
        /// </summary>
        [Required(ErrorMessage = "房源ID為必填項目")]
        [Range(1, int.MaxValue, ErrorMessage = "無效的房源ID")]
        [JsonPropertyName("propertyId")]
        public int PropertyId { get; set; }

        /// <summary>
        /// 中文圖片分類（總覽、客廳、臥室、陽台、飯廳、書房、衛浴、衣帽間、其他）
        /// </summary>
        [Required(ErrorMessage = "圖片分類為必填項目")]
        [JsonPropertyName("chineseCategory")]
        public string ChineseCategory { get; set; } = "總覽";

        /// <summary>
        /// 圖片檔案（前端上傳用）
        /// </summary>
        [JsonPropertyName("imageFiles")]
        public List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();

        /// <summary>
        /// 顯示順序（可選）
        /// </summary>
        [JsonPropertyName("displayOrder")]
        public int? DisplayOrder { get; set; }
    }

    /// <summary>
    /// 房源創建回應DTO
    /// </summary>
    public class PropertyCreateResponseDto
    {
        /// <summary>
        /// 創建是否成功
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// 新建立的房源ID
        /// </summary>
        [JsonPropertyName("propertyId")]
        public int? PropertyId { get; set; }

        /// <summary>
        /// 回應訊息
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 驗證錯誤列表
        /// </summary>
        [JsonPropertyName("validationErrors")]
        public Dictionary<string, List<string>> ValidationErrors { get; set; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// 房源狀態
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "PENDING";

        /// <summary>
        /// 創建時間
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 刊登費用總計
        /// </summary>
        [JsonPropertyName("totalListingFee")]
        public decimal? TotalListingFee { get; set; }

        /// <summary>
        /// 預計下架日期
        /// </summary>
        [JsonPropertyName("expectedExpireDate")]
        public DateTime? ExpectedExpireDate { get; set; }
    }

    /// <summary>
    /// 城市區域級聯DTO
    /// </summary>
    public class CityDistrictDto
    {
        /// <summary>
        /// 城市ID
        /// </summary>
        [JsonPropertyName("cityId")]
        public int CityId { get; set; }

        /// <summary>
        /// 城市名稱
        /// </summary>
        [JsonPropertyName("cityName")]
        public string CityName { get; set; } = string.Empty;

        /// <summary>
        /// 區域列表
        /// </summary>
        [JsonPropertyName("districts")]
        public List<DistrictDto> Districts { get; set; } = new List<DistrictDto>();
    }

    /// <summary>
    /// 區域DTO
    /// </summary>
    public class DistrictDto
    {
        /// <summary>
        /// 區域ID
        /// </summary>
        [JsonPropertyName("districtId")]
        public int DistrictId { get; set; }

        /// <summary>
        /// 區域名稱
        /// </summary>
        [JsonPropertyName("districtName")]
        public string DistrictName { get; set; } = string.Empty;

        /// <summary>
        /// 所屬城市ID
        /// </summary>
        [JsonPropertyName("cityId")]
        public int CityId { get; set; }
    }

    /// <summary>
    /// 刊登方案DTO
    /// </summary>
    public class ListingPlanDto
    {
        /// <summary>
        /// 方案ID
        /// </summary>
        [JsonPropertyName("planId")]
        public int PlanId { get; set; }

        /// <summary>
        /// 方案名稱
        /// </summary>
        [JsonPropertyName("planName")]
        public string PlanName { get; set; } = string.Empty;

        /// <summary>
        /// 最小刊登天數
        /// </summary>
        [JsonPropertyName("minListingDays")]
        public int MinListingDays { get; set; }

        /// <summary>
        /// 每日價格
        /// </summary>
        [JsonPropertyName("pricePerDay")]
        public decimal PricePerDay { get; set; }

        /// <summary>
        /// 方案總價
        /// </summary>
        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// 方案描述
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}