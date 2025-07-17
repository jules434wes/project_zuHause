using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using zuHause.Models;

namespace zuHause.Controllers
{
    public class FurniturePaymentController : Controller
    {
        // 顯示付款頁，傳入總金額
        public IActionResult Index(int totalAmount)
        {
            ViewBag.StripePublishableKey = ThirdInstallmentPaymentSettings.PublishableKey;
            ViewBag.TotalAmount = totalAmount;
            return View();
        }

        // 建立 Stripe Checkout Session
        [HttpPost]
        public IActionResult CreateCheckoutSession(int totalAmount)
        {
            StripeConfiguration.ApiKey = ThirdInstallmentPaymentSettings.SecretKey;

            // ✅ 取用 ASP.NET Core 專用的網址組合方式
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

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
                            UnitAmount = totalAmount * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "家具租借付款"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{baseUrl}/FurniturePayment/Success",
                CancelUrl = $"{baseUrl}/FurniturePayment/Cancel"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Json(new { id = session.Id });
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Cancel()
        {
            return View();
        }
    }
}
