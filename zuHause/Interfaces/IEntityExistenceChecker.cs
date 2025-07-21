using zuHause.Enums;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 實體存在性驗證服務介面
    /// </summary>
    public interface IEntityExistenceChecker
    {
        /// <summary>
        /// 驗證實體是否存在
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <returns>是否存在的非同步任務</returns>
        Task<bool> ExistsAsync(EntityType entityType, int entityId);

        /// <summary>
        /// 取得實體名稱
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <returns>實體名稱的非同步任務，若不存在則返回 null</returns>
        Task<string?> GetEntityNameAsync(EntityType entityType, int entityId);

        /// <summary>
        /// 批次驗證多個實體是否存在
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityIds">實體ID列表</param>
        /// <returns>存在的實體ID列表的非同步任務</returns>
        Task<List<int>> GetExistingEntityIdsAsync(EntityType entityType, List<int> entityIds);

        /// <summary>
        /// 驗證實體是否為指定會員所擁有
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="memberId">會員ID</param>
        /// <returns>是否為該會員所擁有的非同步任務</returns>
        Task<bool> IsOwnedByMemberAsync(EntityType entityType, int entityId, int memberId);
    }

    /// <summary>
    /// 實體存在性驗證結果
    /// </summary>
    public class EntityExistenceResult
    {
        /// <summary>
        /// 實體是否存在
        /// </summary>
        public bool Exists { get; set; }

        /// <summary>
        /// 實體名稱
        /// </summary>
        public string? EntityName { get; set; }

        /// <summary>
        /// 實體擁有者會員ID
        /// </summary>
        public int? OwnerId { get; set; }

        /// <summary>
        /// 建立存在的結果
        /// </summary>
        /// <param name="entityName">實體名稱</param>
        /// <param name="ownerId">擁有者ID</param>
        /// <returns>存在的結果</returns>
        public static EntityExistenceResult Found(string entityName, int? ownerId = null) =>
            new EntityExistenceResult 
            { 
                Exists = true, 
                EntityName = entityName, 
                OwnerId = ownerId 
            };

        /// <summary>
        /// 建立不存在的結果
        /// </summary>
        /// <returns>不存在的結果</returns>
        public static EntityExistenceResult NotFound() =>
            new EntityExistenceResult { Exists = false };
    }
}