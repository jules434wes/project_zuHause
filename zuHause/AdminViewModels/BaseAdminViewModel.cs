using System.Collections.Generic;

namespace zuHause.AdminViewModels
{
    /// <summary>
    /// Admin 後台 ViewModel 基底類別
    /// 定義共用屬性，避免重複定義
    /// </summary>
    public abstract class BaseAdminViewModel
    {
        /// <summary>
        /// 頁面標題
        /// </summary>
        public string PageTitle { get; set; } = string.Empty;

        /// <summary>
        /// 當前分頁 ID (用於多分頁功能)
        /// </summary>
        public string TabId { get; set; } = string.Empty;

        /// <summary>
        /// 是否有篩選功能
        /// </summary>
        public bool HasFilter { get; set; } = true;

        /// <summary>
        /// 是否有進階篩選
        /// </summary>
        public bool HasAdvancedFilter { get; set; } = true;

        /// <summary>
        /// 分頁資訊
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new();
    }

    /// <summary>
    /// 列表頁面 ViewModel 基底類別
    /// </summary>
    /// <typeparam name="T">資料項目類型</typeparam>
    public abstract class BaseListViewModel<T> : BaseAdminViewModel where T : class
    {
        /// <summary>
        /// 資料項目列表
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 是否有批量操作功能
        /// </summary>
        public bool HasBulkActions { get; set; } = true;

        /// <summary>
        /// 批量操作設定
        /// </summary>
        public BulkActionConfig BulkConfig { get; set; } = new();
    }

    /// <summary>
    /// 詳情頁面 ViewModel 基底類別  
    /// </summary>
    /// <typeparam name="T">資料類型</typeparam>
    public abstract class BaseDetailsViewModel<T> : BaseAdminViewModel where T : class
    {
        /// <summary>
        /// 詳情資料
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 是否為編輯模式
        /// </summary>
        public bool IsEditMode { get; set; } = false;

        /// <summary>
        /// 分頁設定
        /// </summary>
        public List<TabConfig> Tabs { get; set; } = new();
    }

    /// <summary>
    /// 分頁資訊
    /// </summary>
    public class PaginationInfo
    {
        /// <summary>
        /// 當前頁數 (從1開始)
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = AdminConstants.DefaultPageSize;

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// 分頁標籤文字
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// 批量操作設定
    /// </summary>
    public class BulkActionConfig
    {
        /// <summary>
        /// 全選 Checkbox ID
        /// </summary>
        public string SelectAllId { get; set; } = string.Empty;

        /// <summary>
        /// 項目 Checkbox CSS Class
        /// </summary>
        public string CheckboxClass { get; set; } = string.Empty;

        /// <summary>
        /// 批量操作按鈕 ID
        /// </summary>
        public string BulkButtonId { get; set; } = string.Empty;

        /// <summary>
        /// 批量操作按鈕文字
        /// </summary>
        public string BulkButtonText { get; set; } = "批量操作";
    }

    /// <summary>
    /// 分頁配置
    /// </summary>
    public class TabConfig
    {
        /// <summary>
        /// 分頁 ID
        /// </summary>
        public string TabId { get; set; } = string.Empty;

        /// <summary>
        /// 分頁標題
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 是否為預設分頁
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// 徽章數量 (顯示在分頁標題後)
        /// </summary>
        public int? BadgeCount { get; set; }

        /// <summary>
        /// 徽章樣式
        /// </summary>
        public string BadgeStyle { get; set; } = "bg-primary";
    }

    /// <summary>
    /// 篩選條件基底類別
    /// </summary>
    public abstract class BaseFilterCriteria
    {
        /// <summary>
        /// 關鍵字搜尋
        /// </summary>
        public string SearchKeyword { get; set; } = string.Empty;

        /// <summary>
        /// 搜尋欄位
        /// </summary>
        public string SearchField { get; set; } = string.Empty;

        /// <summary>
        /// 分頁資訊
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = AdminConstants.DefaultPageSize;

        /// <summary>
        /// 排序欄位
        /// </summary>
        public string SortBy { get; set; } = string.Empty;

        /// <summary>
        /// 排序方向 (asc/desc)
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }
}