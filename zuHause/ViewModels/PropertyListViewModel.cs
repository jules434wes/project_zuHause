namespace zuHause.ViewModels
{
    public class PropertyListViewModel
    {
        public List<PropertySummaryViewModel> Properties { get; set; } = new List<PropertySummaryViewModel>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public PropertySearchViewModel SearchCriteria { get; set; } = new PropertySearchViewModel();
    }
    
    public class PropertySummaryViewModel
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Address { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string MainImagePath { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsFavorite { get; set; }
        public int ViewCount { get; set; }
    }
    
    public class PropertySearchViewModel
    {
        public string? Keyword { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public string? SortOrder { get; set; } = "desc";
    }
}