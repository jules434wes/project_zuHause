namespace zuHause.ViewModels.TenantViewModel
{
    public class PagedSearchResultViewModel
    {
        public List<SearchViewModel> Properties { get; set; } = new List<SearchViewModel>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize); // 計算總頁數

    }
}
