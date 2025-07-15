using System.ComponentModel.DataAnnotations;

namespace zuHause.DTOs
{
    /// <summary>
    /// 房源搜尋請求 DTO
    /// </summary>
    public class PropertySearchRequestDto
    {
        /// <summary>
        /// 搜尋關鍵字 (房源標題、地址)
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 縣市ID
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>
        /// 區域ID
        /// </summary>
        public int? DistrictId { get; set; }

        /// <summary>
        /// 最低租金
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "最低租金必須大於等於0")]
        public decimal? MinRent { get; set; }

        /// <summary>
        /// 最高租金
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "最高租金必須大於等於0")]
        public decimal? MaxRent { get; set; }

        /// <summary>
        /// 房間數
        /// </summary>
        public int? RoomCount { get; set; }

        /// <summary>
        /// 排序方式 (rent_asc, rent_desc, area_asc, area_desc, newest, oldest)
        /// </summary>
        public string SortBy { get; set; } = "newest";

        /// <summary>
        /// 頁碼 (從1開始)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於0")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        [Range(1, 100, ErrorMessage = "每頁筆數必須在1-100之間")]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 房源搜尋回應 DTO
    /// </summary>
    public class PropertySearchResponseDto
    {
        /// <summary>
        /// 房源清單
        /// </summary>
        public List<PropertySummaryDto> Properties { get; set; } = new();

        /// <summary>
        /// 分頁資訊
        /// </summary>
        public PaginationDto Pagination { get; set; } = new();

        /// <summary>
        /// 搜尋統計
        /// </summary>
        public SearchStatsDto Stats { get; set; } = new();
    }

    /// <summary>
    /// 房源摘要 DTO (用於列表顯示)
    /// </summary>
    public class PropertySummaryDto
    {
        /// <summary>
        /// 房源ID
        /// </summary>
        public int PropertyId { get; set; }

        /// <summary>
        /// 房源標題
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 月租金
        /// </summary>
        public decimal MonthlyRent { get; set; }

        /// <summary>
        /// 坪數
        /// </summary>
        public decimal Area { get; set; }

        /// <summary>
        /// 房間配置 (ex: "1房1廳1衛")
        /// </summary>
        public string RoomLayout { get; set; } = string.Empty;

        /// <summary>
        /// 縣市名稱
        /// </summary>
        public string CityName { get; set; } = string.Empty;

        /// <summary>
        /// 區域名稱
        /// </summary>
        public string DistrictName { get; set; } = string.Empty;

        /// <summary>
        /// 預覽圖片URL
        /// </summary>
        public string? PreviewImageUrl { get; set; }

        /// <summary>
        /// 房東名稱
        /// </summary>
        public string LandlordName { get; set; } = string.Empty;

        /// <summary>
        /// 發布時間
        /// </summary>
        public DateTime? PublishedAt { get; set; }
    }

    /// <summary>
    /// 分頁資訊 DTO
    /// </summary>
    public class PaginationDto
    {
        /// <summary>
        /// 目前頁碼
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// 搜尋統計 DTO
    /// </summary>
    public class SearchStatsDto
    {
        /// <summary>
        /// 搜尋結果總數
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// 搜尋執行時間(毫秒)
        /// </summary>
        public long SearchTimeMs { get; set; }

        /// <summary>
        /// 平均租金
        /// </summary>
        public decimal? AverageRent { get; set; }

        /// <summary>
        /// 最低租金
        /// </summary>
        public decimal? MinRent { get; set; }

        /// <summary>
        /// 最高租金
        /// </summary>
        public decimal? MaxRent { get; set; }
    }
}