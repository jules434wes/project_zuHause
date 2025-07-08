using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels;

namespace zuHause.Controllers
{
    public class FurnitureController : Controller
    {
        //家具首頁
        public IActionResult FurnitureHomePage()
        {
            ViewBag.categories = GetAllCategories();

            // 輪播圖圖片路徑
            ViewBag.carouselImages = new List<string>
            {
                "/img/banner1.jpg",
                "/img/banner2.jpg",
                "/img/banner3.jpg"  //之後補上圖片
            };

            // 熱門商品資料
            ViewBag.hotProducts = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> {
                    { "ProductName", "電鍋" },
                    { "ImageUrl", "/img/products/ricecooker.jpg" },
                    { "Tag", "特惠" }
                },
                new Dictionary<string, string> {
                    { "ProductName", "除濕機" },
                    { "ImageUrl", "/img/products/dehumidifier.jpg" },
                    { "Tag", "" }
                },
                new Dictionary<string, string> {
                    { "ProductName", "風扇" },
                    { "ImageUrl", "/img/products/fan.jpg" },
                    { "Tag", "新上架" }
                }
            };

            return View();
        }

        //家具分類頁面
        public IActionResult ClassificationItems(int categoryId) 
        {
            // 左側分類選單資料
            ViewBag.categories = GetAllCategories();
            // 用於高亮選中的分類
            ViewBag.selectedCategoryId = categoryId;

            // 右側商品區：這裡放一筆假的商品資料來測試顯示效果 
            //尚未連接資料庫，只能先用假資料測試
            ViewBag.Products = new List<ProductViewModel>
   
        {
          new ProductViewModel { productId = "test001", name = "測試商品", description = "這是測試商品描述", imageUrl = "~/img/demo.png", categoryId = categoryId },
        new ProductViewModel { productId = "test002", name = "測試商品", description = "這是測試商品描述", imageUrl = "~/img/demo.png", categoryId = categoryId },
        new ProductViewModel { productId = "test003", name = "測試商品", description = "這是測試商品描述", imageUrl = "~/img/demo.png", categoryId = categoryId },
        new ProductViewModel { productId = "test004", name = "測試商品", description = "這是測試商品描述", imageUrl = "~/img/demo.png", categoryId = categoryId },
        new ProductViewModel { productId = "test005", name = "測試商品", description = "這是測試商品描述", imageUrl = "~/img/demo.png", categoryId = categoryId },
        new ProductViewModel { productId = "test006", name = "測試商品", description = "這是測試商品描述", imageUrl = "~/img/demo.png", categoryId = categoryId },
        
    };

            // 將 categoryId 傳給 View 作為 model（如果你的 View 是 @model int）
            return View(categoryId);
        }

        //分類資料來源
        private List<FurnitureCategories> GetAllCategories()
        {

            return new List<FurnitureCategories>
            {
                new FurnitureCategories { categoryId = 1, categoryName = "玄關鞋櫃" },
                new FurnitureCategories { categoryId = 2, categoryName = "臥房家具" },
                new FurnitureCategories { categoryId = 3, categoryName = "書房家具" },
                new FurnitureCategories { categoryId = 4, categoryName = "沙發" },
                new FurnitureCategories { categoryId = 5, categoryName = "桌子" },
                new FurnitureCategories { categoryId = 6, categoryName = "餐廳家具" },
                new FurnitureCategories { categoryId = 7, categoryName = "餐廳電器" },
                new FurnitureCategories { categoryId = 8, categoryName = "收納傢俱" },
                new FurnitureCategories { categoryId = 9, categoryName = "休閒家具" },
                new FurnitureCategories { categoryId = 10, categoryName = "家電/電子產品" },
                new FurnitureCategories { categoryId = 11, categoryName = "未分類" }
            };
        }


        //家具商品購買頁面
        public IActionResult ProductPurchasePage(string id)
        {
            // 模擬取得該商品資訊（未來串資料庫）
            var product = new ProductViewModel
            {
                productId = id,
                name = "豪華電動起身椅",
                description = "這是一張豪華功能的電動椅",
                imageUrl = "~/img/demo.png", // 暫用圖片
                categoryId = 4 // 假設分類是沙發
            };

            // 模擬分類資料（之後串資料庫）
            ViewBag.categories = GetAllCategories();

            return View(product);
        }



        //租借說明頁面
        public IActionResult InstructionsForUse()
        {
            return View();
        }
     


    }
}
