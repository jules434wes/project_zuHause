using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using zuHause.Models; // EF Core 的資料模型

namespace zuHause.Controllers
{
    public class FurnitureController : Controller
    {
        private readonly ZuHauseContext _context;

        // 模擬登入中的會員 ID（目前固定為 2 號會員）
        private readonly int _currentMemberId = 2;

        public FurnitureController(ZuHauseContext context)
        {
            _context = context;
        }

        // 家具首頁
        public IActionResult FurnitureHomePage()
        {
            SetCurrentMemberInfo();
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
        //會員登入資料
        private void SetCurrentMemberInfo()
        {
            ViewBag.CurrentMember = _context.Members.FirstOrDefault(m => m.MemberId == _currentMemberId);
        }

        // 家具分類頁面
        public IActionResult ClassificationItems(string categoryId)
        {
            SetCurrentMemberInfo();
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
            SetCurrentMemberInfo();
            int memberId = _currentMemberId;

            var product = _context.FurnitureProducts.FirstOrDefault(p => p.FurnitureProductId == id);
            if (product == null) return NotFound();

            // 取得該會員的房源清單（使用 Property 原始資料模型）
            var propertyList = _context.Properties
                .Where(p => p.LandlordMemberId == memberId)
                .ToList();
           
            ViewBag.PropertyList = propertyList;

            // 找到綁定的房源
            var currentProperty = _context.Properties
                .FirstOrDefault(p => p.LandlordMemberId == memberId);

            var contract = currentProperty != null
                ? _context.Contracts.FirstOrDefault(c =>
                    c.RentalApplicationId == currentProperty.PropertyId &&
                    c.Status == "active")
                : null;

            ViewBag.CurrentProperty = currentProperty;
            ViewBag.ContractEndDate = contract?.EndDate?.ToString("yyyy-MM-dd") ?? "無合約";
            ViewBag.RentalDays = contract?.EndDate != null
                ? (contract.EndDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Now).Days
                : 0;

            // 左側分類清單
            ViewBag.categories = GetAllCategories();

            return View(product); // 使用 FurnitureProduct 模型
        }

        //購物車頁面
        public IActionResult RentalCart()
        {
            SetCurrentMemberInfo();
            int memberId = _currentMemberId;

            // 查詢綁定房源（有 active 合約）
            var propertyList = _context.Properties
              .Where(p => p.LandlordMemberId == memberId)
              .Select(p => new
                {
                    p.PropertyId,
                    p.Title,
                    ContractEndDate = _context.Contracts
                        .Where(c => c.RentalApplicationId == p.PropertyId && c.Status == "active")
                        .Select(c => (DateOnly?)c.EndDate)
                        .FirstOrDefault()
                })
               .ToList();

            ViewBag.PropertyList = propertyList;

            var selectedPropertyId = propertyList.FirstOrDefault()?.PropertyId;

            var contractEnd = _context.Contracts
                .Where(c => c.RentalApplicationId == selectedPropertyId && c.Status == "active")
                .Select(c => c.EndDate)
                .FirstOrDefault();

            ViewBag.RentalEndDate = contractEnd;
            ViewBag.RentalDaysLeft = contractEnd.HasValue ? (contractEnd.Value.ToDateTime(new TimeOnly(0)) - DateTime.Now).Days : 0;

            var cart = _context.FurnitureCarts
                .Include(c => c.FurnitureCartItems)
                    .ThenInclude(i => i.Product)
                .Include(c => c.Property)
                .FirstOrDefault(c => c.MemberId == memberId && c.DeletedAt == null && c.Status == "active");


            return View("RentalCart", cart);
        }
        //加入商品到購物車清單add
        [HttpPost]
        public IActionResult AddToCart(string productId, int propertyId)
        {
            
            int memberId = _currentMemberId;

            var product = _context.FurnitureProducts.FirstOrDefault(p => p.FurnitureProductId == productId);
            if (product == null)
            {
                return NotFound();
            }

            var contract = _context.Contracts
                .FirstOrDefault(c => c.RentalApplicationId == propertyId && c.Status == "active");

            if (contract == null || contract.EndDate == null)
            {
                return BadRequest("此房源無有效合約");
            }

            int rentalDays = (contract.EndDate.Value.ToDateTime(new TimeOnly(0)) - DateTime.Now).Days;

            // 找到或建立購物車
            var cart = _context.FurnitureCarts
                .Include(c => c.FurnitureCartItems)
                .FirstOrDefault(c => c.MemberId == memberId && c.PropertyId == propertyId && c.Status == "active" && c.DeletedAt == null);

            if (cart == null)
            {
                cart = new FurnitureCart
                {
                    FurnitureCartId = Guid.NewGuid().ToString(),
                    MemberId = memberId,
                    PropertyId = propertyId,
                    Status = "active",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.FurnitureCarts.Add(cart);
            }

            // 新增或更新明細
            var item = cart.FurnitureCartItems.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                cart.FurnitureCartItems.Add(new FurnitureCartItem
                {
                    CartItemId = Guid.NewGuid().ToString(),
                    CartId = cart.FurnitureCartId,
                    ProductId = productId,
                    Quantity = 1,
                    RentalDays = rentalDays,
                    UnitPriceSnapshot = product.DailyRental,
                    SubTotal = product.DailyRental * rentalDays,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                item.Quantity += 1;
                item.SubTotal = item.Quantity * item.RentalDays * product.DailyRental;
            }

            _context.SaveChanges();

            return RedirectToAction("RentalCart");
        }


        // 租借說明頁面
        public IActionResult InstructionsForUse()
        {
            SetCurrentMemberInfo();
            return View();
        }

        // 資料庫撈分類資料
        private List<FurnitureCategory> GetAllCategories()
        {
            // 取得所有最上層分類（ParentId 為 null）
            var categories = _context.FurnitureCategories
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.DisplayOrder)
            .Select(parent => new FurnitureCategory
            {
                FurnitureCategoriesId = parent.FurnitureCategoriesId,
                Name = parent.Name,
                ParentId = parent.ParentId,
                Depth = parent.Depth,
                DisplayOrder = parent.DisplayOrder,
                CreatedAt = parent.CreatedAt,
                UpdatedAt = parent.UpdatedAt,
                // 子分類清單放進 InverseParent（因為你模型裡 InverseParent 是 ICollection<FurnitureCategory>）
                InverseParent = _context.FurnitureCategories
                    .Where(child => child.ParentId == parent.FurnitureCategoriesId)
                    .OrderBy(c => c.DisplayOrder)
                    .ToList()
            }).ToList();

            ViewBag.categories = categories;


            return categories;
        }

       
        //歷史訂單紀錄
        public IActionResult OrderHistory()
        {
            SetCurrentMemberInfo();
            int memberId = _currentMemberId;

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
            SetCurrentMemberInfo();
            int memberId = _currentMemberId;

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
            SetCurrentMemberInfo();
            int memberId = _currentMemberId;

            var member = _context.Members.FirstOrDefault(m => m.MemberId == memberId);
            var properties = _context.Properties
                                     .Where(p => p.LandlordMemberId == memberId)
                                     .ToList();

            var subjects = new List<string>
    {
          "產品訂製", "運費問題", "訂單問題",
        "服務問題",  "其他服務", "退換貨服務","其他問題"
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
          
            int memberId = _currentMemberId;
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
