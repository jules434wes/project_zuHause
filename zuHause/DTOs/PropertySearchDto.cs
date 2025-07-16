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
        /// 房源狀態 (預設為 DRAFT)
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "DRAFT";

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
}