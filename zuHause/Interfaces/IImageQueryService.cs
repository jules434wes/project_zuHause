using zuHause.Enums;
using zuHause.Models;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 圖片查詢服務介面
    /// 提供統一的圖片查詢、主圖判斷和 URL 生成功能
    /// </summary>
    public interface IImageQueryService
    {
        /// <summary>
        /// 根據實體類型和實體ID獲取所有圖片
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <returns>圖片列表，按 DisplayOrder 排序</returns>
        Task<List<Image>> GetImagesByEntityAsync(EntityType entityType, int entityId);

        /// <summary>
        /// 根據實體類型和實體ID獲取主圖
        /// 主圖定義：DisplayOrder 最小值的圖片
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <returns>主圖，如果沒有圖片則返回 null</returns>
        Task<Image?> GetMainImageAsync(EntityType entityType, int entityId);

        /// <summary>
        /// 根據實體類型、實體ID和圖片分類獲取圖片
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <returns>符合條件的圖片列表</returns>
        Task<List<Image>> GetImagesByCategoryAsync(EntityType entityType, int entityId, ImageCategory category);

        /// <summary>
        /// 根據圖片GUID獲取圖片
        /// </summary>
        /// <param name="imageGuid">圖片GUID</param>
        /// <returns>圖片實體，如果不存在則返回 null</returns>
        Task<Image?> GetImageByGuidAsync(Guid imageGuid);

        /// <summary>
        /// 根據圖片ID獲取圖片
        /// </summary>
        /// <param name="imageId">圖片ID</param>
        /// <returns>圖片實體，如果不存在則返回 null</returns>
        Task<Image?> GetImageByIdAsync(long imageId);

        /// <summary>
        /// 檢查指定實體是否存在圖片
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <returns>是否存在圖片</returns>
        Task<bool> HasImagesAsync(EntityType entityType, int entityId);

        /// <summary>
        /// 獲取指定實體的圖片數量
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <returns>圖片數量</returns>
        Task<int> GetImageCountAsync(EntityType entityType, int entityId);

        /// <summary>
        /// 生成圖片存取 URL（使用完整的圖片物件資訊）
        /// </summary>
        /// <param name="image">圖片物件</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>圖片 URL</returns>
        string GenerateImageUrl(Image image, ImageSize size = ImageSize.Original);

        /// <summary>
        /// 生成圖片存取 URL
        /// </summary>
        /// <param name="storedFileName">儲存檔名</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>圖片 URL</returns>
        string GenerateImageUrl(string storedFileName, ImageSize size = ImageSize.Original);

        /// <summary>
        /// 批量生成圖片 URL
        /// </summary>
        /// <param name="images">圖片列表</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>圖片及其對應 URL 的字典</returns>
        Dictionary<long, string> GenerateImageUrls(List<Image> images, ImageSize size = ImageSize.Original);
    }
}