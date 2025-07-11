using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using zuHause.Models; // EF Core 的資料模型

namespace zuHause.Controllers
{
    public class FurnitureController : Controller
    {
        private readonly ZuHauseContext _context;

        public FurnitureController(ZuHauseContext context)
        {
            _context = context;
        }

        // 家具首頁
        public IActionResult FurnitureHomePage()
        {
            // 左側分類清單
            ViewBag.categories = GetAllCategories();

            // 輪播圖（圖片可保留靜態路徑）
            ViewBag.carouselImages = new List<string>
            {
                "/img/banner1.jpg",
                "/img/banner2.jpg",
                "/img/banner3.jpg"
            };

            //  從資料庫抓出「熱門商品」（目前用最新上架前6筆商品代表熱門）
            var hotProducts = _context.FurnitureProducts
                .Where(p => p.Status) // 只抓上架的
                .OrderByDescending(p => p.CreatedAt) // 按建立時間倒序
                .Take(6) // 只抓前6筆
                .ToList();

            ViewBag.hotProducts = hotProducts;

            return View();
        }

        // 家具分類頁面
        public IActionResult ClassificationItems(string categoryId)
        {
            // 取得分類名稱
            var category = _context.FurnitureCategories.FirstOrDefault(c => c.FurnitureCategoriesId == categoryId);
            if (category == null)
            {
                return NotFound();
            }

            // 取得該分類的商品清單
            var products = _context.FurnitureProducts
                .Where(p => p.CategoryId == categoryId && p.Status == true) // 只顯示上架商品
                .ToList();

            ViewBag.categories = GetAllCategories(); // 左側分類清單
            ViewBag.CategoryName = category.Name;    // 當前分類名稱
            return View(products); // view 應接收 List<FurnitureProduct>
        }

        // 家具商品購買頁面
        public IActionResult ProductPurchasePage(string id)
        {
            var product = _context.FurnitureProducts
     .FirstOrDefault(p => p.FurnitureProductId == id && p.Status);

            if (product == null)
                return NotFound();

            ViewBag.categories = GetAllCategories(); // 左側分類清單

            // 模擬從登入會員取出房源清單（之後改成實際查詢）
            var properties = _context.Properties
            .Where(p => p.LandlordMemberId == 1) // 先假設固定會員ID
            .ToList();

            ViewBag.BoundProperties = properties;
            ViewBag.SelectedProperty = properties.FirstOrDefault();
            return View(product);
        }

        // 租借說明頁面
        public IActionResult InstructionsForUse()
        {
            return View();
        }

        // 資料庫撈分類資料
        private List<FurnitureCategory> GetAllCategories()
        {
            return _context.FurnitureCategories.ToList();
        }

        //上架商品頁面 //目前無法使用
        public IActionResult Create()
        {
            var categories = _context.FurnitureCategories
                             .OrderBy(c => c.DisplayOrder)
                             .ToList();
            ViewBag.Categories = categories;

            return View();
        }

        //上架商品頁面 //目前無法使用
        [HttpPost]
        public async Task<IActionResult> Create(FurnitureProduct model, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                model.FurnitureProductId = Guid.NewGuid().ToString(); // 產生主鍵
                model.CreatedAt = DateTime.Now;

                // 儲存圖片（略）

                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["success"] = "新增成功！";
                return RedirectToAction("FurnitureHomePage");
            }

            ViewBag.Categories = _context.FurnitureCategories.ToList(); // 若驗證失敗要重綁
            return View(model);
        }

        //歷史訂單紀錄
        public IActionResult OrderHistory()
        {
            int memberId = 1; // TODO: 之後從登入會員資訊取得

            // 進行中訂單（來源：FurnitureOrderItem）
            var ongoingOrders = _context.FurnitureOrderItems
                .Where(item => item.Order.MemberId == memberId)
                .OrderByDescending(item => item.CreatedAt)
                .Select(item => new
                {
                    item.FurnitureOrderItemId,
                    item.OrderId,
                    item.Product.ProductName,
                    item.Quantity,
                    item.DailyRentalSnapshot,
                    item.RentalDays,
                    item.SubTotal,
                    item.CreatedAt,
                    item.Order.Status // 若 Order 有訂單狀態欄位
                })
                .ToList();

            // 歷史訂單（來源：FurnitureOrderHistory）
            var completedOrders = _context.FurnitureOrderHistories
                .Where(his => his.Order.MemberId == memberId)
                .OrderByDescending(his => his.CreatedAt)
                .Select(his => new
                {
                    his.FurnitureOrderHistoryId,
                    his.OrderId,
                    his.ProductNameSnapshot,
                    his.Quantity,
                    his.DailyRentalSnapshot,
                    his.RentalStart,
                    his.RentalEnd,
                    his.SubTotal,
                    his.ItemStatus,
                    his.CreatedAt
                })
                .ToList();

            ViewBag.OngoingOrders = ongoingOrders;
            ViewBag.CompletedOrders = completedOrders;

            return View("OrderHistory");
        }

        // 客服聯繫頁面
        public IActionResult ContactRecords()
        {
            int memberId = 1; // 假設為登入者ID，實際應由登入系統取得

            var tickets = _context.CustomerServiceTickets
                .Include(t => t.Member)
                .Include(t => t.Property)
                .Include(t => t.FurnitureOrder)
                .Where(t => t.MemberId == memberId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            ViewBag.MemberName = "XX先生 / 小姐"; // 實際應由登入資訊取得
            return View("ContactRecords", tickets);
        }

       //客服表單畫面
        public IActionResult ContactUsForm(string orderId)
        {
            int memberId = 1; // 這裡請改成實際登入的會員 ID（從 Session 或 Claims 取得）

            var member = _context.Members.FirstOrDefault(m => m.MemberId == memberId);
            var properties = _context.Properties
                                     .Where(p => p.LandlordMemberId == memberId)
                                     .ToList();

            var subjects = new List<string>
    {
        "產品詢價", "產品尺寸", "產品訂製", "運費問題", "訂單問題", "查詢庫存",
        "服務問題", "異業合作", "其他服務", "退換貨服務"
    };

            ViewBag.Member = member;
            ViewBag.Properties = properties;
            ViewBag.Subjects = subjects;
            ViewBag.OrderId = orderId;

            return View("ContactUsForm");
        }

        [HttpPost]
        public IActionResult SubmitContactForm(string Subject, string TicketContent, int? PropertyId)
        {
            int memberId = 1; // 實務上從登入取得

            var ticket = new CustomerServiceTicket
            {
                MemberId = memberId,
                Subject = Subject,
                TicketContent = TicketContent,
                PropertyId = PropertyId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                StatusCode = "NEW",
                IsResolved = false,
                CategoryCode = "GENERAL"
            };

            _context.CustomerServiceTickets.Add(ticket);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "表單已送出，我們將盡快與您聯繫。";
            return RedirectToAction("ContactRecords");
        }

    }
}
