using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using zuHause.Constants;
using zuHause.DTOs;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Services
{
    /// <summary>
    /// 房源付款服務實作
    /// 整合 Stripe 處理房源刊登費用付款
    /// </summary>
    public class PropertyPaymentService : IPropertyPaymentService
    {
        private readonly ZuHauseContext _context;
        private readonly ILogger<PropertyPaymentService> _logger;
        private readonly IConfiguration _configuration;

        public PropertyPaymentService(
            ZuHauseContext context,
            ILogger<PropertyPaymentService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 為指定房源建立 Stripe 付款 Session
        /// </summary>
        public async Task<PropertyPaymentSessionResult> CreatePaymentSessionAsync(int propertyId, int landlordId)
        {
            try
            {
                // 1. 驗證付款資格
                var validation = await ValidatePaymentEligibilityAsync(propertyId, landlordId);
                if (!validation.IsValid)
                {
                    return PropertyPaymentSessionResult.Failure(validation.ErrorMessage!, validation.ErrorCode);
                }

                // 2. 計算付款金額
                var feeCalculation = await CalculateAndPersistListingFeeAsync(propertyId);
                if (!feeCalculation.IsSuccess)
                {
                    return PropertyPaymentSessionResult.Failure(feeCalculation.ErrorMessage!);
                }

                // 3. 查詢房源資訊
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    return PropertyPaymentSessionResult.Failure("房源不存在", "PROPERTY_NOT_FOUND");
                }

                // 4. 建立 Stripe Session
                var domain = _configuration["Application:BaseUrl"] ?? "https://localhost:7010";
                var successUrl = $"{domain}/landlord/property/payment/success?session_id={{CHECKOUT_SESSION_ID}}";
                var cancelUrl = $"{domain}/landlord/propertymanagement?payment_cancelled=true";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "twd",
                                UnitAmount = (long)(feeCalculation.Amount * 100), // 轉為分
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"房源刊登費 - {property.Title}",
                                    Description = $"刊登 {feeCalculation.ListingDays} 天 ({feeCalculation.PlanName})"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "PropertyId", propertyId.ToString() },
                        { "LandlordId", landlordId.ToString() },
                        { "Type", "PropertyListing" },
                        { "ListingDays", feeCalculation.ListingDays.ToString() },
                        { "PlanName", feeCalculation.PlanName }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation("Stripe 付款 Session 建立成功 - PropertyId: {PropertyId}, SessionId: {SessionId}, Amount: {Amount}",
                    propertyId, session.Id, feeCalculation.Amount);

                return PropertyPaymentSessionResult.Success(
                    session, propertyId, property.Title, feeCalculation.Amount, feeCalculation.ListingDays);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe 付款 Session 建立失敗 - PropertyId: {PropertyId}, Error: {Error}",
                    propertyId, ex.Message);
                return PropertyPaymentSessionResult.Failure($"付款系統錯誤：{ex.Message}", "STRIPE_ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立付款 Session 時發生未預期錯誤 - PropertyId: {PropertyId}",
                    propertyId);
                return PropertyPaymentSessionResult.Failure("系統錯誤，請稍後再試", "SYSTEM_ERROR");
            }
        }

        /// <summary>
        /// 處理付款成功回調，更新房源狀態
        /// </summary>
        public async Task<PropertyPaymentCompletionResult> HandlePaymentSuccessAsync(string sessionId)
        {
            try
            {
                // 1. 驗證 Stripe Session
                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(sessionId);

                if (session.PaymentStatus != "paid")
                {
                    return PropertyPaymentCompletionResult.Failure("付款未完成", "PAYMENT_NOT_COMPLETED");
                }

                // 2. 從 metadata 取得房源資訊
                if (!session.Metadata.TryGetValue("PropertyId", out var propertyIdStr) ||
                    !int.TryParse(propertyIdStr, out var propertyId))
                {
                    return PropertyPaymentCompletionResult.Failure("無效的付款資訊", "INVALID_METADATA");
                }

                if (!session.Metadata.TryGetValue("ListingDays", out var listingDaysStr) ||
                    !int.TryParse(listingDaysStr, out var listingDays))
                {
                    listingDays = 14; // 預設值
                }

                // 3. 查詢並更新房源
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    return PropertyPaymentCompletionResult.Failure("房源不存在", "PROPERTY_NOT_FOUND");
                }

                // 4. 檢查是否重複處理
                if (property.IsPaid)
                {
                    _logger.LogWarning("房源已付款，跳過重複處理 - PropertyId: {PropertyId}, SessionId: {SessionId}",
                        propertyId, sessionId);
                    
                    return PropertyPaymentCompletionResult.Success(
                        property.PropertyId, property.Title, property.ListingFeeAmount ?? 0,
                        property.PaidAt ?? DateTime.UtcNow, property.ExpireAt, property.StatusCode);
                }

                // 5. 更新房源狀態
                var paidAt = DateTime.UtcNow;
                var expireAt = paidAt.AddDays(listingDays);
                var paidAmount = (decimal)(session.AmountTotal ?? 0) / 100; // 從分轉為元

                property.IsPaid = true;
                property.PaidAt = paidAt;
                property.ExpireAt = expireAt;
                property.StatusCode = PropertyStatusConstants.LISTED;
                property.PublishedAt = paidAt;
                property.UpdatedAt = paidAt;

                if (!property.ListingFeeAmount.HasValue)
                {
                    property.ListingFeeAmount = paidAmount;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("房源付款處理完成 - PropertyId: {PropertyId}, Amount: {Amount}, ExpireAt: {ExpireAt}",
                    propertyId, paidAmount, expireAt);

                return PropertyPaymentCompletionResult.Success(
                    property.PropertyId, property.Title, paidAmount, paidAt, expireAt, PropertyStatusConstants.LISTED);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe Session 驗證失敗 - SessionId: {SessionId}, Error: {Error}",
                    sessionId, ex.Message);
                return PropertyPaymentCompletionResult.Failure($"付款驗證失敗：{ex.Message}", "STRIPE_VERIFICATION_ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理付款成功回調時發生錯誤 - SessionId: {SessionId}",
                    sessionId);
                return PropertyPaymentCompletionResult.Failure("系統錯誤，請聯絡客服", "SYSTEM_ERROR");
            }
        }

        /// <summary>
        /// 計算並保存房源刊登費用
        /// </summary>
        public async Task<PropertyListingFeeCalculation> CalculateAndPersistListingFeeAsync(int propertyId)
        {
            try
            {
                var property = await _context.Properties
                    .Include(p => p.ListingPlan)
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    return PropertyListingFeeCalculation.Failure("房源不存在");
                }

                // 1. 優先使用房源已設定的刊登費用
                if (property.ListingFeeAmount.HasValue && property.ListingFeeAmount > 0)
                {
                    var listingDays = property.ListingDays ?? 14;
                    var pricePerDay = property.ListingFeeAmount.Value / listingDays;
                    var planName = property.ListingPlan?.PlanName ?? "自訂方案";

                    return PropertyListingFeeCalculation.Success(
                        property.ListingFeeAmount.Value, listingDays, pricePerDay, planName, property.ListingPlanId);
                }

                // 2. 從 ListingPlan 計算費用
                ListingPlan? listingPlan;
                if (property.ListingPlanId.HasValue)
                {
                    listingPlan = property.ListingPlan;
                }
                else
                {
                    // 使用預設的基本方案
                    listingPlan = await _context.ListingPlans
                        .Where(lp => lp.IsActive && lp.StartAt <= DateTime.UtcNow &&
                                   (lp.EndAt == null || lp.EndAt > DateTime.UtcNow))
                        .OrderBy(lp => lp.PricePerDay)
                        .FirstOrDefaultAsync();
                }

                if (listingPlan == null)
                {
                    return PropertyListingFeeCalculation.Failure("找不到可用的刊登方案");
                }

                // 3. 計算總費用
                var finalListingDays = property.ListingDays ?? Math.Max(listingPlan.MinListingDays, 14);
                var totalAmount = listingPlan.PricePerDay * finalListingDays;

                // 4. 更新房源的計算結果（為了保持一致性）
                if (!property.ListingFeeAmount.HasValue)
                {
                    property.ListingFeeAmount = totalAmount;
                    property.ListingDays = finalListingDays;
                    property.ListingPlanId = listingPlan.PlanId;
                    await _context.SaveChangesAsync();
                }

                return PropertyListingFeeCalculation.Success(
                    totalAmount, finalListingDays, listingPlan.PricePerDay, listingPlan.PlanName, listingPlan.PlanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算房源刊登費用時發生錯誤 - PropertyId: {PropertyId}",
                    propertyId);
                return PropertyListingFeeCalculation.Failure("費用計算失敗，請稍後再試");
            }
        }

        /// <summary>
        /// 驗證房源是否可以進行付款
        /// </summary>
        public async Task<PropertyPaymentValidationResult> ValidatePaymentEligibilityAsync(int propertyId, int landlordId)
        {
            try
            {
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    return PropertyPaymentValidationResult.Invalid("房源不存在", "PROPERTY_NOT_FOUND");
                }

                // 1. 驗證擁有權
                if (property.LandlordMemberId != landlordId)
                {
                    return PropertyPaymentValidationResult.Invalid("您沒有權限操作此房源", "ACCESS_DENIED", propertyId, property.StatusCode);
                }

                // 2. 驗證房源狀態
                if (property.StatusCode != PropertyStatusConstants.PENDING_PAYMENT)
                {
                    return PropertyPaymentValidationResult.Invalid(
                        $"此房源目前狀態為「{property.StatusCode}」，無法進行付款", 
                        "INVALID_STATUS", propertyId, property.StatusCode);
                }

                // 3. 檢查是否已付款
                if (property.IsPaid)
                {
                    return PropertyPaymentValidationResult.Invalid("此房源已完成付款", "ALREADY_PAID", propertyId, property.StatusCode);
                }

                // 4. 檢查是否已刪除
                if (property.DeletedAt.HasValue)
                {
                    return PropertyPaymentValidationResult.Invalid("此房源已被刪除", "PROPERTY_DELETED", propertyId, property.StatusCode);
                }

                return PropertyPaymentValidationResult.Valid(propertyId, property.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證房源付款資格時發生錯誤 - PropertyId: {PropertyId}, LandlordId: {LandlordId}",
                    propertyId, landlordId);
                return PropertyPaymentValidationResult.Invalid("系統錯誤，請稍後再試", "SYSTEM_ERROR");
            }
        }
    }
}