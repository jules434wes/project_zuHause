using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.Services.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// 設備分類查詢服務實作
    /// 提供三層階層架構的設備分類查詢功能
    /// </summary>
    public class EquipmentCategoryQueryService : IEquipmentCategoryQueryService
    {
        private readonly ZuHauseContext _context;

        public EquipmentCategoryQueryService(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 獲取完整的三層階層設備分類
        /// 結構：大分類 → 子項 → 細項
        /// </summary>
        public async Task<List<PropertyEquipmentCategoryHierarchy>> GetCategoriesHierarchyAsync()
        {
            var allCategories = await GetActiveCategoriesAsync();
            var topLevelCategories = allCategories.Where(c => c.ParentCategoryId == null)
                                                 .OrderBy(c => c.CategoryId)
                                                 .ToList();

            var hierarchyList = new List<PropertyEquipmentCategoryHierarchy>();

            foreach (var topCategory in topLevelCategories)
            {
                var hierarchy = BuildCategoryHierarchy(topCategory, allCategories, 1);
                hierarchyList.Add(hierarchy);
            }

            return hierarchyList;
        }

        /// <summary>
        /// 獲取所有啟用的設備分類
        /// 條件：IsActive == true
        /// </summary>
        public async Task<List<PropertyEquipmentCategory>> GetActiveCategoriesAsync()
        {
            return await _context.PropertyEquipmentCategories
                .Where(c => c.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// 根據父分類ID獲取子分類
        /// </summary>
        public async Task<List<PropertyEquipmentCategory>> GetSubCategoriesAsync(int parentId)
        {
            return await _context.PropertyEquipmentCategories
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.CategoryId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// 獲取頂層分類（大分類）
        /// 條件：ParentCategoryId == null 且 IsActive == true
        /// </summary>
        public async Task<List<PropertyEquipmentCategory>> GetTopLevelCategoriesAsync()
        {
            return await _context.PropertyEquipmentCategories
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.CategoryId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// 根據分類ID獲取分類詳細資訊
        /// </summary>
        public async Task<PropertyEquipmentCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.PropertyEquipmentCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        /// <summary>
        /// 檢查分類是否有子分類
        /// </summary>
        public async Task<bool> HasSubCategoriesAsync(int categoryId)
        {
            return await _context.PropertyEquipmentCategories
                .AnyAsync(c => c.ParentCategoryId == categoryId && c.IsActive);
        }

        /// <summary>
        /// 遞迴建構分類階層結構
        /// </summary>
        /// <param name="category">當前分類</param>
        /// <param name="allCategories">所有分類資料</param>
        /// <param name="level">階層等級</param>
        /// <returns>階層結構</returns>
        private PropertyEquipmentCategoryHierarchy BuildCategoryHierarchy(
            PropertyEquipmentCategory category, 
            List<PropertyEquipmentCategory> allCategories, 
            int level)
        {
            var hierarchy = new PropertyEquipmentCategoryHierarchy
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                ParentCategoryId = category.ParentCategoryId,
                IsActive = category.IsActive,
                Level = level,
                IsRequired = DetermineIfRequired(category, level)
            };

            // 尋找子分類
            var childCategories = allCategories
                .Where(c => c.ParentCategoryId == category.CategoryId)
                .OrderBy(c => c.CategoryId)
                .ToList();

            // 遞迴建構子分類
            foreach (var child in childCategories)
            {
                var childHierarchy = BuildCategoryHierarchy(child, allCategories, level + 1);
                hierarchy.Children.Add(childHierarchy);
            }

            return hierarchy;
        }

        /// <summary>
        /// 判斷分類是否為必選項
        /// 依照需求：如勾選可養寵，則跟在該子項後的細項必須選一樣
        /// </summary>
        /// <param name="category">分類</param>
        /// <param name="level">階層等級</param>
        /// <returns>是否必選</returns>
        private bool DetermineIfRequired(PropertyEquipmentCategory category, int level)
        {
            // 業務邏輯：特定分類的細項為必選
            // 例如：如果父分類包含「寵物」相關關鍵字，則其子分類為必選
            if (level == 3) // 細項層級
            {
                var categoryNameLower = category.CategoryName.ToLower();
                return categoryNameLower.Contains("寵物") || 
                       categoryNameLower.Contains("pet") ||
                       categoryNameLower.Contains("養寵");
            }

            return false;
        }
    }
}