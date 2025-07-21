using Microsoft.AspNetCore.Http;
using zuHause.DTOs;
using zuHause.Enums;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 統一圖片上傳服務介面 - 支援多實體類型的圖片上傳
    /// </summary>
    public interface IImageUploadService
    {
        /// <summary>
        /// 批次上傳圖片 - 主要上傳方法
        /// </summary>
        /// <param name="files">上傳的圖片檔案集合</param>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <param name="uploadedByMemberId">上傳者會員ID (可選)</param>
        /// <param name="skipEntityValidation">是否略過實體存在性驗證 (預設為 false)</param>
        /// <returns>上傳結果列表</returns>
        Task<List<ImageUploadResult>> UploadImagesAsync(
            IFormFileCollection files, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category, 
            int? uploadedByMemberId = null,
            bool skipEntityValidation = false);

        /// <summary>
        /// 單一圖片上傳 - 便利方法
        /// </summary>
        /// <param name="file">單一圖片檔案</param>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <param name="uploadedByMemberId">上傳者會員ID (可選)</param>
        /// <param name="skipEntityValidation">是否略過實體存在性驗證 (預設為 false)</param>
        /// <returns>上傳結果</returns>
        Task<ImageUploadResult> UploadImageAsync(
            IFormFile file, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category, 
            int? uploadedByMemberId = null,
            bool skipEntityValidation = false);

        /// <summary>
        /// 從串流上傳圖片 - 進階方法，支援非 Web 環境
        /// </summary>
        /// <param name="imageStream">圖片資料串流</param>
        /// <param name="originalFileName">原始檔名</param>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <param name="uploadedByMemberId">上傳者會員ID (可選)</param>
        /// <param name="skipEntityValidation">是否略過實體存在性驗證 (預設為 false)</param>
        /// <returns>上傳結果</returns>
        Task<ImageUploadResult> UploadImageFromStreamAsync(
            Stream imageStream, 
            string originalFileName, 
            EntityType entityType, 
            int entityId, 
            ImageCategory category, 
            int? uploadedByMemberId = null,
            bool skipEntityValidation = false);

        /// <summary>
        /// 刪除圖片 - 軟刪除或硬刪除
        /// </summary>
        /// <param name="imageId">圖片ID</param>
        /// <param name="hardDelete">是否硬刪除 (預設為軟刪除)</param>
        /// <returns>刪除是否成功</returns>
        Task<bool> DeleteImageAsync(long imageId, bool hardDelete = false);

        /// <summary>
        /// 批次刪除實體的所有圖片
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類 (可選，不指定則刪除所有分類)</param>
        /// <param name="hardDelete">是否硬刪除 (預設為軟刪除)</param>
        /// <returns>刪除是否成功</returns>
        Task<bool> DeleteImagesByEntityAsync(
            EntityType entityType, 
            int entityId, 
            ImageCategory? category = null, 
            bool hardDelete = false);

        /// <summary>
        /// 替換主圖 - 將指定圖片設為主圖，其他圖片調整排序
        /// </summary>
        /// <param name="imageId">要設為主圖的圖片ID</param>
        /// <returns>替換是否成功</returns>
        Task<bool> SetMainImageAsync(long imageId);

        /// <summary>
        /// 重新排序圖片 - 批次調整 DisplayOrder
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="imageIds">按新順序排列的圖片ID列表</param>
        /// <returns>重新排序是否成功</returns>
        Task<bool> ReorderImagesAsync(EntityType entityType, int entityId, List<long> imageIds);

        /// <summary>
        /// 驗證上傳限制 - 檢查是否可以上傳更多圖片
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="additionalCount">要新增的圖片數量</param>
        /// <returns>驗證結果和錯誤訊息</returns>
        Task<(bool IsValid, string ErrorMessage)> ValidateUploadLimitAsync(
            EntityType entityType, 
            int entityId, 
            int additionalCount);
    }
}