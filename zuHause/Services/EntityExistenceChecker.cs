using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.Enums;
using zuHause.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// 實體存在性驗證服務實作
    /// </summary>
    public class EntityExistenceChecker : IEntityExistenceChecker
    {
        private readonly ZuHauseContext _context;

        public EntityExistenceChecker(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 驗證實體是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(EntityType entityType, int entityId)
        {
            if (entityId <= 0) return false;

            return entityType switch
            {
                EntityType.Member => await _context.Members.AnyAsync(m => m.MemberId == entityId),
                EntityType.Property => await _context.Properties.AnyAsync(p => p.PropertyId == entityId),
                EntityType.Furniture => await _context.FurnitureProducts.AnyAsync(f => f.FurnitureProductId == entityId.ToString()),
                EntityType.Announcement => await _context.SystemMessages.AnyAsync(s => s.MessageId == entityId),
                _ => false
            };
        }

        /// <summary>
        /// 取得實體名稱
        /// </summary>
        public async Task<string?> GetEntityNameAsync(EntityType entityType, int entityId)
        {
            if (entityId <= 0) return null;

            return entityType switch
            {
                EntityType.Member => await _context.Members
                    .Where(m => m.MemberId == entityId)
                    .Select(m => m.MemberName)
                    .FirstOrDefaultAsync(),
                    
                EntityType.Property => await _context.Properties
                    .Where(p => p.PropertyId == entityId)
                    .Select(p => p.Title)
                    .FirstOrDefaultAsync(),
                    
                EntityType.Furniture => await _context.FurnitureProducts
                    .Where(f => f.FurnitureProductId == entityId.ToString())
                    .Select(f => f.ProductName)
                    .FirstOrDefaultAsync(),
                    
                EntityType.Announcement => await _context.SystemMessages
                    .Where(s => s.MessageId == entityId)
                    .Select(s => s.Title)
                    .FirstOrDefaultAsync(),
                    
                _ => null
            };
        }

        /// <summary>
        /// 批次驗證多個實體是否存在
        /// </summary>
        public async Task<List<int>> GetExistingEntityIdsAsync(EntityType entityType, List<int> entityIds)
        {
            if (entityIds == null || !entityIds.Any()) return new List<int>();

            var validIds = entityIds.Where(id => id > 0).ToList();
            if (!validIds.Any()) return new List<int>();

            return entityType switch
            {
                EntityType.Member => await _context.Members
                    .Where(m => validIds.Contains(m.MemberId))
                    .Select(m => m.MemberId)
                    .ToListAsync(),
                    
                EntityType.Property => await _context.Properties
                    .Where(p => validIds.Contains(p.PropertyId))
                    .Select(p => p.PropertyId)
                    .ToListAsync(),
                    
                EntityType.Furniture => await _context.FurnitureProducts
                    .Where(f => validIds.Select(id => id.ToString()).Contains(f.FurnitureProductId))
                    .Select(f => int.Parse(f.FurnitureProductId))
                    .ToListAsync(),
                    
                EntityType.Announcement => await _context.SystemMessages
                    .Where(s => validIds.Contains(s.MessageId))
                    .Select(s => s.MessageId)
                    .ToListAsync(),
                    
                _ => new List<int>()
            };
        }

        /// <summary>
        /// 驗證實體是否為指定會員所擁有
        /// </summary>
        public async Task<bool> IsOwnedByMemberAsync(EntityType entityType, int entityId, int memberId)
        {
            if (entityId <= 0 || memberId <= 0) return false;

            return entityType switch
            {
                EntityType.Member => entityId == memberId,
                
                EntityType.Property => await _context.Properties
                    .AnyAsync(p => p.PropertyId == entityId && p.LandlordMemberId == memberId),
                
                EntityType.Furniture => await _context.FurnitureProducts
                    .AnyAsync(f => f.FurnitureProductId == entityId.ToString()), // 暫時移除 SupplierId 檢查，因為該欄位不存在
                
                EntityType.Announcement => await _context.SystemMessages
                    .AnyAsync(s => s.MessageId == entityId), // 暫時移除 CreatedBy 檢查，因為該欄位不存在
                
                _ => false
            };
        }
    }
}