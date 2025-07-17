using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Services.Interfaces
{
    /// <summary>
    /// 顯示順序管理服務介面
    /// </summary>
    public interface IDisplayOrderService
    {
        /// <summary>
        /// 為新圖片分配下一個可用的顯示順序
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <returns>下一個可用的顯示順序的非同步任務</returns>
        Task<int> GetNextDisplayOrderAsync(EntityType entityType, int entityId, ImageCategory category);

        /// <summary>
        /// 重新排列指定實體和分類的所有圖片順序
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <param name="imageIds">圖片ID順序列表</param>
        /// <returns>重新排列結果的非同步任務</returns>
        Task<DisplayOrderResult> ReorderImagesAsync(EntityType entityType, int entityId, ImageCategory category, List<long> imageIds);

        /// <summary>
        /// 設定主圖（將指定圖片設為 DisplayOrder = 1，其他圖片順序後移）
        /// </summary>
        /// <param name="imageId">要設為主圖的圖片ID</param>
        /// <returns>設定結果的非同步任務</returns>
        Task<DisplayOrderResult> SetAsPrimaryImageAsync(long imageId);

        /// <summary>
        /// 批次更新圖片顯示順序
        /// </summary>
        /// <param name="updates">順序更新列表</param>
        /// <returns>更新結果的非同步任務</returns>
        Task<DisplayOrderResult> BatchUpdateDisplayOrderAsync(List<DisplayOrderUpdate> updates);

        /// <summary>
        /// 移除圖片時重新整理順序（填補空隙）
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <param name="removedDisplayOrder">被移除圖片的原顯示順序</param>
        /// <returns>整理結果的非同步任務</returns>
        Task<DisplayOrderResult> CompactDisplayOrderAsync(EntityType entityType, int entityId, ImageCategory category, int removedDisplayOrder);
    }

    /// <summary>
    /// 顯示順序更新項目
    /// </summary>
    public class DisplayOrderUpdate
    {
        /// <summary>
        /// 圖片ID
        /// </summary>
        public long ImageId { get; set; }

        /// <summary>
        /// 新的顯示順序
        /// </summary>
        public int NewDisplayOrder { get; set; }
    }

    /// <summary>
    /// 顯示順序操作結果
    /// </summary>
    public class DisplayOrderResult
    {
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 受影響的圖片數量
        /// </summary>
        public int AffectedCount { get; set; }

        /// <summary>
        /// 更新的圖片ID列表
        /// </summary>
        public List<long> UpdatedImageIds { get; set; } = new List<long>();

        /// <summary>
        /// 建立成功的操作結果
        /// </summary>
        /// <param name="affectedCount">受影響的圖片數量</param>
        /// <param name="updatedImageIds">更新的圖片ID列表</param>
        /// <returns>成功的操作結果</returns>
        public static DisplayOrderResult Success(int affectedCount = 0, List<long>? updatedImageIds = null) => 
            new DisplayOrderResult 
            { 
                IsSuccess = true, 
                AffectedCount = affectedCount,
                UpdatedImageIds = updatedImageIds ?? new List<long>()
            };

        /// <summary>
        /// 建立失敗的操作結果
        /// </summary>
        /// <param name="errorMessage">錯誤訊息</param>
        /// <returns>失敗的操作結果</returns>
        public static DisplayOrderResult Failure(string errorMessage) => 
            new DisplayOrderResult 
            { 
                IsSuccess = false, 
                ErrorMessage = errorMessage 
            };
    }
}