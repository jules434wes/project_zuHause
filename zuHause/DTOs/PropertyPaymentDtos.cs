using Stripe.Checkout;

namespace zuHause.DTOs
{
    /// <summary>
    /// 房源付款 Session 建立結果
    /// </summary>
    public class PropertyPaymentSessionResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public Session? StripeSession { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TWD";
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public int ListingDays { get; set; }

        public static PropertyPaymentSessionResult Success(Session session, int propertyId, string propertyTitle, decimal amount, int listingDays)
        {
            return new PropertyPaymentSessionResult
            {
                IsSuccess = true,
                StripeSession = session,
                Amount = amount,
                PropertyId = propertyId,
                PropertyTitle = propertyTitle,
                ListingDays = listingDays
            };
        }

        public static PropertyPaymentSessionResult Failure(string errorMessage, string? errorCode = null)
        {
            return new PropertyPaymentSessionResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// 房源付款完成處理結果
    /// </summary>
    public class PropertyPaymentCompletionResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public DateTime PaidAt { get; set; }
        public DateTime? ExpireAt { get; set; }
        public string NewStatus { get; set; } = string.Empty;

        public static PropertyPaymentCompletionResult Success(int propertyId, string propertyTitle, decimal paidAmount, DateTime paidAt, DateTime? expireAt, string newStatus)
        {
            return new PropertyPaymentCompletionResult
            {
                IsSuccess = true,
                PropertyId = propertyId,
                PropertyTitle = propertyTitle,
                PaidAmount = paidAmount,
                PaidAt = paidAt,
                ExpireAt = expireAt,
                NewStatus = newStatus
            };
        }

        public static PropertyPaymentCompletionResult Failure(string errorMessage, string? errorCode = null)
        {
            return new PropertyPaymentCompletionResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// 房源刊登費用計算結果
    /// </summary>
    public class PropertyListingFeeCalculation
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TWD";
        public int ListingDays { get; set; }
        public decimal PricePerDay { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int? ListingPlanId { get; set; }

        public static PropertyListingFeeCalculation Success(decimal amount, int listingDays, decimal pricePerDay, string planName, int? listingPlanId = null)
        {
            return new PropertyListingFeeCalculation
            {
                IsSuccess = true,
                Amount = amount,
                ListingDays = listingDays,
                PricePerDay = pricePerDay,
                PlanName = planName,
                ListingPlanId = listingPlanId
            };
        }

        public static PropertyListingFeeCalculation Failure(string errorMessage)
        {
            return new PropertyListingFeeCalculation
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>
    /// 房源付款資格驗證結果
    /// </summary>
    public class PropertyPaymentValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public int PropertyId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
        public bool HasValidListingPlan { get; set; }

        public static PropertyPaymentValidationResult Valid(int propertyId, string currentStatus)
        {
            return new PropertyPaymentValidationResult
            {
                IsValid = true,
                PropertyId = propertyId,
                CurrentStatus = currentStatus,
                IsOwner = true,
                HasValidListingPlan = true
            };
        }

        public static PropertyPaymentValidationResult Invalid(string errorMessage, string? errorCode = null, int propertyId = 0, string currentStatus = "")
        {
            return new PropertyPaymentValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                PropertyId = propertyId,
                CurrentStatus = currentStatus
            };
        }
    }
}