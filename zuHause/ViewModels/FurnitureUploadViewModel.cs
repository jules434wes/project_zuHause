namespace zuHause.ViewModels
{
    public class FurnitureUploadViewModel
    {
        public string FurnitureProductId { get; set; } = null!;
       
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // CategoryId
        public decimal OriginalPrice { get; set; }
        public decimal RentPerDay { get; set; }
        public int Stock { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int SafetyStock { get; set; }
        public bool Status { get; set; } // 是否上架

        //public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; } // ✅ 圖片欄位
    }

}
