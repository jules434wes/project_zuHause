using Microsoft.EntityFrameworkCore;
using zuHause.DTOs;
using zuHause.Models;
using zuHause.Services.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// 刊登方案驗證服務實作
    /// 提供刊登方案驗證、時間計算和費用計算功能
    /// </summary>
    public class ListingPlanValidationService : IListingPlanValidationService
    {
        private readonly ZuHauseContext _context;

        public ListingPlanValidationService(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 驗證刊登方案是否有效
        /// </summary>
        public async Task<PropertyValidationResult> ValidateListingPlanAsync(int planId)
        {
            var result = new PropertyValidationResult();

            if (planId <= 0)
            {
                result.Errors.Add(new PropertyValidationError
                {
                    PropertyName = nameof(planId),
                    ErrorMessage = "方案ID必須大於0"
                });
                return result;
            }

            var plan = await GetListingPlanByIdAsync(planId);
            
            if (plan == null)
            {
                result.Errors.Add(new PropertyValidationError
                {
                    PropertyName = nameof(planId),
                    ErrorMessage = $"找不到方案ID為 {planId} 的刊登方案"
                });
                return result;
            }

            // 檢查方案是否啟用
            if (!plan.IsActive)
            {
                result.Errors.Add(new PropertyValidationError
                {
                    PropertyName = "IsActive",
                    ErrorMessage = "此刊登方案目前未啟用"
                });
            }

            // 檢查方案是否在有效期間內
            var now = DateTime.Now;
            if (now < plan.StartAt)
            {
                result.Errors.Add(new PropertyValidationError
                {
                    PropertyName = "StartAt",
                    ErrorMessage = $"此方案尚未生效，生效時間：{plan.StartAt:yyyy-MM-dd HH:mm}"
                });
            }

            if (plan.EndAt.HasValue && now > plan.EndAt.Value)
            {
                result.Errors.Add(new PropertyValidationError
                {
                    PropertyName = "EndAt",
                    ErrorMessage = $"此方案已過期，過期時間：{plan.EndAt:yyyy-MM-dd HH:mm}"
                });
            }

            return result;
        }

        /// <summary>
        /// 計算刊登到期日
        /// 規則：開始日期 + 方案天數 + 1天的 00:00
        /// </summary>
        public async Task<DateTime?> CalculateExpireDateAsync(int planId, DateTime startDate)
        {
            var plan = await GetListingPlanByIdAsync(planId);
            if (plan == null) return null;

            // 依照需求：系統讀取使用者設備日期 + 刊登方案天數 + 1天的 00:00
            var expireDate = startDate.Date.AddDays(plan.MinListingDays + 1);
            return expireDate; // 已是 00:00:00
        }

        /// <summary>
        /// 計算刊登總費用
        /// 規則：每日費用 × 最小刊登天數
        /// </summary>
        public async Task<decimal?> CalculateTotalFeeAsync(int planId)
        {
            var plan = await GetListingPlanByIdAsync(planId);
            if (plan == null) return null;

            // 依照需求：listingPlans.pricePerDay * listingPlans.minListingDays
            return plan.PricePerDay * plan.MinListingDays;
        }

        /// <summary>
        /// 獲取目前可用的刊登方案
        /// </summary>
        public async Task<List<ListingPlan>> GetActiveListingPlansAsync()
        {
            var now = DateTime.Now;
            
            return await _context.ListingPlans
                .Where(p => p.IsActive && 
                           p.StartAt <= now && 
                           (!p.EndAt.HasValue || p.EndAt >= now))
                .OrderBy(p => p.PricePerDay)
                .ToListAsync();
        }

        /// <summary>
        /// 驗證刊登時間期間是否有效
        /// </summary>
        public async Task<PropertyValidationResult> ValidateListingPeriodAsync(int planId, DateTime requestDate)
        {
            var result = new PropertyValidationResult();
            var plan = await GetListingPlanByIdAsync(planId);

            if (plan == null)
            {
                result.Errors.Add(new PropertyValidationError
                {
                    PropertyName = nameof(planId),
                    ErrorMessage = "找不到指定的刊登方案"
                });
                return result;
            }

            // 檢查請求日期是否在方案有效期內
            if (requestDate < plan.StartAt)
            {
                result.Errors.Add(new PropertyValidationError
                {
                    PropertyName = "RequestDate",
                    ErrorMessage = $"請求日期早於方案生效時間（{plan.StartAt:yyyy-MM-dd HH:mm}）"
                });
            }

            if (plan.EndAt.HasValue && requestDate > plan.EndAt.Value)
            {
                result.Errors.Add(new PropertyValidationError
                {
                    PropertyName = "RequestDate", 
                    ErrorMessage = $"請求日期晚於方案結束時間（{plan.EndAt:yyyy-MM-dd HH:mm}）"
                });
            }

            return result;
        }

        /// <summary>
        /// 根據方案ID獲取方案詳細資訊
        /// </summary>
        public async Task<ListingPlan?> GetListingPlanByIdAsync(int planId)
        {
            return await _context.ListingPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlanId == planId);
        }
    }
}