using zuHause.Models;

namespace zuHause.Services.Interfaces
{
    /// <summary>
    /// 設備分類查詢服務介面
    /// 提供三層階層架構的設備分類查詢功能
    /// </summary>
    public interface IEquipmentCategoryQueryService
    {
        /// <summary>
        /// 獲取完整的三層階層設備分類
        /// 結構：大分類 → 子項 → 細項
        /// </summary>
        /// <returns>階層式設備分類清單</returns>
        Task<List<PropertyEquipmentCategoryHierarchy>> GetCategoriesHierarchyAsync();

        /// <summary>
        /// 獲取所有啟用的設備分類
        /// 條件：IsActive == true
        /// </summary>
        /// <returns>啟用的設備分類清單</returns>
        Task<List<PropertyEquipmentCategory>> GetActiveCategoriesAsync();

        /// <summary>
        /// 根據父分類ID獲取子分類
        /// </summary>
        /// <param name="parentId">父分類ID</param>
        /// <returns>子分類清單</returns>
        Task<List<PropertyEquipmentCategory>> GetSubCategoriesAsync(int parentId);

        /// <summary>
        /// 獲取頂層分類（大分類）
        /// 條件：ParentCategoryId == null 且 IsActive == true
        /// </summary>
        /// <returns>頂層分類清單</returns>
        Task<List<PropertyEquipmentCategory>> GetTopLevelCategoriesAsync();

        /// <summary>
        /// 根據分類ID獲取分類詳細資訊
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>分類資訊</returns>
        Task<PropertyEquipmentCategory?> GetCategoryByIdAsync(int categoryId);

        /// <summary>
        /// 檢查分類是否有子分類
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>是否有子分類</returns>
        Task<bool> HasSubCategoriesAsync(int categoryId);
    }

    /// <summary>
    /// 階層式設備分類結構
    /// 用於表示三層架構的設備分類
    /// </summary>
    public class PropertyEquipmentCategoryHierarchy
    {
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 上層分類ID
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 階層等級 (1=大分類, 2=子項, 3=細項)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 子分類清單
        /// </summary>
        public List<PropertyEquipmentCategoryHierarchy> Children { get; set; } = new List<PropertyEquipmentCategoryHierarchy>();

        /// <summary>
        /// 是否為必選項（用於前端邏輯：如選擇可養寵，則必須選擇寵物類型）
        /// </summary>
        public bool IsRequired { get; set; }
    }
}