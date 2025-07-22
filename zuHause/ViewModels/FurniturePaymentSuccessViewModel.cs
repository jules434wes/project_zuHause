using zuHause.Models;

namespace zuHause.ViewModels
{
    public class FurniturePaymentSuccessViewModel
    {
        //Stripe付款編號    第三方金流（如 Stripe）的付款交易編號
        public string StripePaymentIntentId { get; set; }

        ///付款方式   信用卡、Line Pay、街口等
        public string PaymentMethod { get; set; }

        //付款時間    實際完成付款的時間
        public DateTime PaidAt { get; set; }

        //訂單明細清單    包含所有租借商品的詳細資料
        public List<FurnitureOrderHistory> OrderItems { get; set; }

        //商品總金額   所有商品（不含運費）的合計金額
        public decimal TotalProductAmount { get; set; }

        //運費   可能是固定金額或根據距離計算的運費
        public decimal DeliveryFee { get; set; }

        //總金額    商品金額 + 運費的最終付款金額
        public decimal GrandTotal { get; set; }
    }
}
