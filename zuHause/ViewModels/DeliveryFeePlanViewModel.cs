namespace zuHause.ViewModels
{
    public class DeliveryFeePlanViewModel
    {
        public int? PlanId { get; set; } // 編輯時使用
        public string PlanName { get; set; } = null!;
        public decimal BaseFee { get; set; }
        public decimal RemoteAreaSurcharge { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}
