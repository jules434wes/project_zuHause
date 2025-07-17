using zuHause.Enums;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 並發安全的 DisplayOrder 管理介面
    /// </summary>
    public interface IDisplayOrderManager
    {
        /// <summary>
        /// 為圖片分配 DisplayOrder（支援並發控制）
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <param name="imageIds">圖片ID列表</param>
        /// <param name="concurrencyStrategy">並發控制策略</param>
        /// <returns>分配結果的非同步任務</returns>
        Task<DisplayOrderAssignResult> AssignDisplayOrdersAsync(
            EntityType entityType, 
            int entityId, 
            ImageCategory category,
            List<long> imageIds, 
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock);

        /// <summary>
        /// 重新排序 DisplayOrder（填補空隙）
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <param name="concurrencyStrategy">並發控制策略</param>
        /// <returns>重新排序結果的非同步任務</returns>
        Task<DisplayOrderReorderResult> ReorderDisplayOrdersAsync(
            EntityType entityType, 
            int entityId, 
            ImageCategory category,
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock);

        /// <summary>
        /// 取得下一個可用的 DisplayOrder（並發安全）
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="category">圖片分類</param>
        /// <param name="concurrencyStrategy">並發控制策略</param>
        /// <returns>下一個可用的 DisplayOrder 的非同步任務</returns>
        Task<int> GetNextDisplayOrderAsync(
            EntityType entityType, 
            int entityId, 
            ImageCategory category,
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock);

        /// <summary>
        /// 移動圖片到指定位置
        /// </summary>
        /// <param name="imageId">圖片ID</param>
        /// <param name="newDisplayOrder">新的顯示順序</param>
        /// <param name="concurrencyStrategy">並發控制策略</param>
        /// <returns>移動結果的非同步任務</returns>
        Task<DisplayOrderMoveResult> MoveImageToPositionAsync(
            long imageId, 
            int newDisplayOrder,
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock);

        /// <summary>
        /// 移除圖片並調整後續順序
        /// </summary>
        /// <param name="imageId">圖片ID</param>
        /// <param name="concurrencyStrategy">並發控制策略</param>
        /// <returns>移除結果的非同步任務</returns>
        Task<DisplayOrderRemoveResult> RemoveImageAndAdjustOrdersAsync(
            long imageId,
            ConcurrencyControlStrategy concurrencyStrategy = ConcurrencyControlStrategy.OptimisticLock);
    }

    /// <summary>
    /// 並發控制策略
    /// </summary>
    public enum ConcurrencyControlStrategy
    {
        /// <summary>
        /// 樂觀鎖（使用版本號或時間戳）
        /// </summary>
        OptimisticLock,

        /// <summary>
        /// 悲觀鎖（使用資料庫行鎖）
        /// </summary>
        PessimisticLock,

        /// <summary>
        /// 無鎖定（適用於低並發場景）
        /// </summary>
        NoLock
    }

    /// <summary>
    /// DisplayOrder 分配結果
    /// </summary>
    public class DisplayOrderAssignResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 分配的 DisplayOrder 字典（ImageId -> DisplayOrder）
        /// </summary>
        public Dictionary<long, int> AssignedOrders { get; set; } = new();

        /// <summary>
        /// 受影響的圖片數量
        /// </summary>
        public int AffectedCount { get; set; }

        /// <summary>
        /// 重試次數（並發衝突時）
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 建立成功的結果
        /// </summary>
        public static DisplayOrderAssignResult Success(Dictionary<long, int> assignedOrders, int retryCount = 0) =>
            new DisplayOrderAssignResult 
            { 
                IsSuccess = true, 
                AssignedOrders = assignedOrders,
                AffectedCount = assignedOrders.Count,
                RetryCount = retryCount
            };

        /// <summary>
        /// 建立失敗的結果
        /// </summary>
        public static DisplayOrderAssignResult Failure(string errorMessage, int retryCount = 0) =>
            new DisplayOrderAssignResult 
            { 
                IsSuccess = false, 
                ErrorMessage = errorMessage,
                RetryCount = retryCount
            };
    }

    /// <summary>
    /// DisplayOrder 重新排序結果
    /// </summary>
    public class DisplayOrderReorderResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 重新排序前的數量
        /// </summary>
        public int BeforeCount { get; set; }

        /// <summary>
        /// 重新排序後的數量
        /// </summary>
        public int AfterCount { get; set; }

        /// <summary>
        /// 更新的圖片ID列表
        /// </summary>
        public List<long> UpdatedImageIds { get; set; } = new();

        /// <summary>
        /// 建立成功的結果
        /// </summary>
        public static DisplayOrderReorderResult Success(int beforeCount, int afterCount, List<long> updatedImageIds) =>
            new DisplayOrderReorderResult 
            { 
                IsSuccess = true, 
                BeforeCount = beforeCount,
                AfterCount = afterCount,
                UpdatedImageIds = updatedImageIds
            };

        /// <summary>
        /// 建立失敗的結果
        /// </summary>
        public static DisplayOrderReorderResult Failure(string errorMessage) =>
            new DisplayOrderReorderResult 
            { 
                IsSuccess = false, 
                ErrorMessage = errorMessage
            };
    }

    /// <summary>
    /// DisplayOrder 移動結果
    /// </summary>
    public class DisplayOrderMoveResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 原始位置
        /// </summary>
        public int OldPosition { get; set; }

        /// <summary>
        /// 新位置
        /// </summary>
        public int NewPosition { get; set; }

        /// <summary>
        /// 受影響的圖片數量
        /// </summary>
        public int AffectedCount { get; set; }

        /// <summary>
        /// 建立成功的結果
        /// </summary>
        public static DisplayOrderMoveResult Success(int oldPosition, int newPosition, int affectedCount) =>
            new DisplayOrderMoveResult 
            { 
                IsSuccess = true, 
                OldPosition = oldPosition,
                NewPosition = newPosition,
                AffectedCount = affectedCount
            };

        /// <summary>
        /// 建立失敗的結果
        /// </summary>
        public static DisplayOrderMoveResult Failure(string errorMessage) =>
            new DisplayOrderMoveResult 
            { 
                IsSuccess = false, 
                ErrorMessage = errorMessage
            };
    }

    /// <summary>
    /// DisplayOrder 移除結果
    /// </summary>
    public class DisplayOrderRemoveResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 被移除的位置
        /// </summary>
        public int RemovedPosition { get; set; }

        /// <summary>
        /// 調整順序的圖片數量
        /// </summary>
        public int AdjustedCount { get; set; }

        /// <summary>
        /// 建立成功的結果
        /// </summary>
        public static DisplayOrderRemoveResult Success(int removedPosition, int adjustedCount) =>
            new DisplayOrderRemoveResult 
            { 
                IsSuccess = true, 
                RemovedPosition = removedPosition,
                AdjustedCount = adjustedCount
            };

        /// <summary>
        /// 建立失敗的結果
        /// </summary>
        public static DisplayOrderRemoveResult Failure(string errorMessage) =>
            new DisplayOrderRemoveResult 
            { 
                IsSuccess = false, 
                ErrorMessage = errorMessage
            };
    }
}