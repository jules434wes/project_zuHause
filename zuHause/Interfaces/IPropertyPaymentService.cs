using Stripe.Checkout;
using zuHause.DTOs;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 房源付款服務介面
    /// 處理房源刊登費用的計算和 Stripe 付款整合
    /// </summary>
    public interface IPropertyPaymentService
    {
        /// <summary>
        /// 為指定房源建立 Stripe 付款 Session
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="landlordId">房東ID</param>
        /// <returns>Stripe Session 和付款資訊</returns>
        Task<PropertyPaymentSessionResult> CreatePaymentSessionAsync(int propertyId, int landlordId);

        /// <summary>
        /// 處理付款成功回調，更新房源狀態
        /// </summary>
        /// <param name="sessionId">Stripe Session ID</param>
        /// <returns>處理結果</returns>
        Task<PropertyPaymentCompletionResult> HandlePaymentSuccessAsync(string sessionId);

        /// <summary>
        /// 計算房源刊登費用
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>費用計算結果</returns>
        Task<PropertyListingFeeCalculation> CalculateAndPersistListingFeeAsync(int propertyId);

        /// <summary>
        /// 驗證房源是否可以進行付款
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="landlordId">房東ID</param>
        /// <returns>驗證結果</returns>
        Task<PropertyPaymentValidationResult> ValidatePaymentEligibilityAsync(int propertyId, int landlordId);
    }
}