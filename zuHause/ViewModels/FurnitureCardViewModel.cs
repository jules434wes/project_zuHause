namespace zuHause.ViewModels
{
    public class FurnitureCardViewModel
    {
        public string FurnitureID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int Stock { get; set; }
        public int RentedCount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal RentPerDay { get; set; }
        public decimal RentPerYear { get; set; }
        public bool IsHot { get; set; }
        public int TotalQuantity { get; set; }
        public bool IsSpecial { get; set; }
        public int SafetyStock { get; set; }
        public DateTime ListDate { get; set; }
        public DateTime DelistDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }  // 軟刪除判斷用
                                                  
        public string? ImageUrl { get; set; }

    }
}
