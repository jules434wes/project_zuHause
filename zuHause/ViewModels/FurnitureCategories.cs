namespace zuHause.ViewModels
{
    public class FurnitureCategories //所有頁面左側的分類清單
    {
        public int? categoryId { get; set; } //分類編號
        public string? categoryName { get; set; } //分類名稱
        public List<FurnitureCategories> SubCategories { get; set; } = new List<FurnitureCategories>(); //蜂巢狀分類
    }
    public class ProductViewModel // 用於前台產品顯示
    {
        public string? productId { get; set; }        // 商品編號
        public string? name { get; set; }             // 商品名稱
        public string? description { get; set; }      // 商品簡述
        public string? imageUrl { get; set; }         // 商品圖片路徑，例如 ~/img/xxx.png
        public int categoryId { get; set; }
    }


}
