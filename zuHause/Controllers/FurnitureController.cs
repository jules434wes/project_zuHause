using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using zuHause.Models; // EF Core 的資料模型
using Microsoft.AspNetCore.Http; //用於 HttpContext.Session

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

            // 輪播圖
            DateTime now = DateTime.Now;
            var carouselImages = _context.CarouselImages
           .Where(c => c.IsActive
                       && c.StartAt <= now
                       && (c.EndAt == null || c.EndAt > now)
                       && c.DeletedAt == null
                       && c.PageCode == "FurnitureHome")
           .OrderBy(c => c.DisplayOrder)
           .ToList();

            ViewBag.CarouselImages = carouselImages;

            //  從資料庫抓出「熱門商品」（目前用最新上架前6筆商品代表熱門）
            var hotProducts = _context.FurnitureProducts
                .Where(p => p.Status) // 只抓上架的
                .OrderByDescending(p => p.CreatedAt) // 按建立時間倒序
                .Take(8) // 只抓前8筆
                .ToList();

            ViewBag.hotProducts = hotProducts;

            return View();
        }
        //會員登入資料
        private void SetCurrentMemberInfo()
        {
            // ***** 臨時測試修改開始 (請在測試完成後，務必移除或註釋掉這段代碼) *****
            // 強制設定會員ID為2，並從資料庫抓取其資料
            int tempMemberId = 2; // 指定臨時使用的會員ID
            var tempMember = _context.Members.Find(tempMemberId); // 從資料庫查找會員資料

            ViewBag.CurrentMemberId = tempMemberId;
            ViewBag.CurrentMemberName = tempMember?.MemberName; // 使用資料庫中的會員名稱
            ViewBag.CurrentMember = tempMember; // 將整個 Member 物件存入 ViewBag

            return; // 直接返回，跳過正常的 Session 判斷
            // ***** 臨時測試修改結束 *****


            /* // 以下是正常的 Session 判斷邏輯，請在測試完成後，恢復這段代碼
            var memberIdString = HttpContext.Session.GetString("MemberId");
            if (!string.IsNullOrEmpty(memberIdString) && int.TryParse(memberIdString, out int memberId))
            {
                ViewBag.CurrentMemberId = memberId;
                var member = _context.Members.Find(memberId); // 從資料庫查找實際會員資料
                ViewBag.CurrentMemberName = member?.MemberName;
                ViewBag.CurrentMember = member; // 將 Member 物件存入 ViewBag
            }
            else
            {
                ViewBag.CurrentMemberId = null;
                ViewBag.CurrentMemberName = null;
                ViewBag.CurrentMember = null;
            }
            */
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

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.categories = _context.FurnitureCategories
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.DisplayOrder)
                .ToList();

            // 撈會員的綁定房源清單（透過合約 → 租賃申請 → 房源）
            var propertyListTuples = _context.Contracts
                .Where(c =>
                    c.RentalApplication != null &&
                    c.RentalApplication.MemberId == memberId &&
                    c.Status == "active") // 確保只抓取 active 狀態的合約
                .Select(c => Tuple.Create( // 使用 Tuple.Create
                    c.RentalApplication.Property.PropertyId,
                    c.RentalApplication.Property.Title,
                    c.EndDate // EndDate 已經是 DateOnly?，所以直接用
                ))
                .ToList();

            ViewBag.PropertyList = propertyListTuples;

            // 預設選第一個房源
            var currentPropertyTuple = propertyListTuples.FirstOrDefault();

            // **新增或修改的部分：處理租期總天數和租金總額**
            int? totalRentalDays = null;
            decimal? totalRentalAmount = null;

            if (currentPropertyTuple != null)
            {
                ViewBag.CurrentProperty = currentPropertyTuple;

                if (currentPropertyTuple.Item3 != null) // Item3 是 EndDate
                {
                    DateOnly endDate = (DateOnly)currentPropertyTuple.Item3;
                    DateTime today = DateTime.Now;
                    DateTime contractEndDateAsDateTime = endDate.ToDateTime(TimeOnly.MinValue);

                    totalRentalDays = Math.Max(0, (contractEndDateAsDateTime - today).Days);
                    totalRentalAmount = totalRentalDays * product.DailyRental; // 計算總租金

                    ViewBag.ContractEndDate = endDate.ToString("yyyy-MM-dd");
                    ViewBag.RentalDays = totalRentalDays;
                    ViewBag.TotalRentalAmount = totalRentalAmount; // 將總租金加入 ViewBag
                }
                else
                {
                    ViewBag.ContractEndDate = null; // 如果沒有結束日期，則清空
                    ViewBag.RentalDays = null;
                    ViewBag.TotalRentalAmount = null;
                }
            }
            else
            {
                ViewBag.CurrentProperty = null; // 如果沒有找到任何綁定房源，設置為 null
                ViewBag.ContractEndDate = null;
                ViewBag.RentalDays = null;
                ViewBag.TotalRentalAmount = null;
            }

            // 商品詳細描述（假設 product.Description 包含列表內容，例如 "項目1\n項目2"）
            // 如果您的商品描述是多行文字，您可能需要將其拆分為列表
            // 或者如果您的 FurnitureProduct 模型中有一個 List<string> Features 或類似的屬性
            // 這裡假設 Description 可以用 \n 分割
            // 如果 Description 是 HTML 格式，則在前端直接渲染
            // 如果需要更結構化的特點列表，可能需要修改 FurnitureProduct 模型或從其他地方獲取
            ViewBag.ProductFeatures = product.Description?.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return View(product);
        }




        //購物車頁面
        public IActionResult RentalCart()
        {
            SetCurrentMemberInfo();

            if (ViewBag.CurrentMemberId == null)
            {
                return RedirectToAction("Login", "Member");
            }

            int memberId = (int)ViewBag.CurrentMemberId;

            var cart = _context.FurnitureCarts
                .Include(c => c.FurnitureCartItems)
                    .ThenInclude(item => item.Product) // 確保 Product 關聯載入
                .FirstOrDefault(c => c.MemberId == memberId);

            if (cart == null)
            {
                // CS8604: Possible null reference argument for parameter 'entity'
                // 確保 CartId 是非 null 的 string，假設您的 CartId 是 Guid 或類似的字串類型
                cart = new FurnitureCart { FurnitureCartId = Guid.NewGuid().ToString(), MemberId = memberId, CreatedAt = DateTime.Now, Status = "IN_CART" };
                _context.FurnitureCarts.Add(cart);
                Console.WriteLine($"🟢 DEBUG: 新增購物車 Status = {cart.Status}");
                _context.SaveChanges();
            }

            var propertyContracts = _context.Contracts
                .Include(c => c.RentalApplication)
                    .ThenInclude(ra => ra.Property)
                // CS8602: Dereference of a possibly null reference.
                // 檢查 c.RentalApplication 和 c.RentalApplication.Property 是否為 null
                .Where(c => c.RentalApplication != null &&
                            c.RentalApplication.MemberId == memberId &&
                            c.Status == "ACTIVE" &&
                            c.EndDate >= DateOnly.FromDateTime(DateTime.Today))
                .ToList();

            var propertySelectListItems = propertyContracts
                // 檢查 c.RentalApplication.Property 和 c.EndDate 是否為 null
                .Where(c => c.RentalApplication?.Property != null && c.EndDate.HasValue)
                .Select(c => new SelectListItem
                {
                    // CS8602: Dereference of a possibly null reference.
                    // 使用 ! 告訴編譯器，在 Where 條件下，這些不可能為 null
                    Value = c.RentalApplication!.Property!.PropertyId.ToString(),
                    Text = c.RentalApplication.Property.Title
                })
                .ToList();

            int defaultPropertyId = 0;
            DateOnly? defaultRentalEndDate = null;
            int defaultRentalDaysLeft = 0;
            // CS8600: Converting null literal or possible null value to non-nullable type.
            // 讓整個 Tuple 也可以是 null
            Tuple<int, string, DateOnly?>? defaultPropertyInfo = null;

            if (propertyContracts.Any())
            {
                var latestContract = propertyContracts.OrderByDescending(c => c.EndDate).FirstOrDefault();
                // CS8602: Dereference of a possibly null reference.
                // 檢查 latestContract, RentalApplication.Property 和 EndDate 是否為 null
                if (latestContract != null && latestContract.RentalApplication?.Property != null && latestContract.EndDate.HasValue)
                {
                    defaultPropertyId = latestContract.RentalApplication.Property.PropertyId;
                    defaultRentalEndDate = latestContract.EndDate;
                    defaultPropertyInfo = new Tuple<int, string, DateOnly?>(
                        latestContract.RentalApplication.Property.PropertyId,
                        latestContract.RentalApplication.Property.Title,
                        latestContract.EndDate
                    );

                    var today = DateOnly.FromDateTime(DateTime.Today);
                    // CS8629: Nullable value type may be null. (解決 latestContract.EndDate.Value 的警告)
                    var rentalDays = (latestContract.EndDate.Value.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).TotalDays;
                    defaultRentalDaysLeft = Math.Max(0, (int)Math.Ceiling(rentalDays));

                    var selectedItem = propertySelectListItems.FirstOrDefault(item => item.Value == defaultPropertyId.ToString());
                    if (selectedItem != null)
                    {
                        selectedItem.Selected = true;
                    }
                }
            }

            ViewBag.PropertySelectList = new SelectList(propertySelectListItems, "Value", "Text", defaultPropertyId.ToString());
            ViewBag.CurrentPropertyForCart = defaultPropertyInfo;
            ViewBag.CurrentPropertyIdForCart = defaultPropertyId;
            ViewBag.RentalEndDate = defaultRentalEndDate;
            ViewBag.RentalDaysLeft = defaultRentalDaysLeft;

            var propertyInfoForJs = propertyContracts
                // 檢查 c.RentalApplication.Property 和 c.EndDate 是否為 null
                .Where(c => c.RentalApplication?.Property != null && c.EndDate.HasValue)
                .Select(c => new
                {
                    // CS8602: Dereference of a possibly null reference.
                    // 使用 ! 告訴編譯器，在 Where 條件下，這些不可能為 null
                    PropertyId = c.RentalApplication!.Property!.PropertyId,
                    Title = c.RentalApplication.Property.Title,
                    ContractEndDate = c.EndDate!.Value.ToString("yyyy-MM-dd"), // 使用 !
                    DaysLeft = Math.Max(0, (int)(c.EndDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today).TotalDays)
                })
                .ToList();

            ViewBag.PropertyInfoJson = System.Text.Json.JsonSerializer.Serialize(propertyInfoForJs);

            decimal totalAmount = 0;
            // CS8602: Dereference of a possibly null reference.
            // 檢查 cart.FurnitureCartItems 是否為 null
            if (cart.FurnitureCartItems != null)
            {
                foreach (var item in cart.FurnitureCartItems)
                {
                    // CS8602: Dereference of a possibly null reference.
                    // 檢查 item.Product 是否為 null
                    if (item.Product != null)
                    {
                        totalAmount += item.Quantity * item.Product.DailyRental * item.RentalDays;
                    }
                }
            }
            ViewBag.TotalCartAmount = totalAmount;

            return View(cart);
        }

        //加入商品到購物車清單add
        [HttpPost]
        public IActionResult AddToCart(string productId, int quantity, int rentalDays, int selectedPropertyId)
        {
            SetCurrentMemberInfo();

            if (ViewBag.CurrentMemberId == null)
            {
                return RedirectToAction("Login", "Member");
            }

            int memberId = (int)ViewBag.CurrentMemberId;

            var product = _context.FurnitureProducts.FirstOrDefault(p => p.FurnitureProductId == productId);
            if (product == null)
            {
                return NotFound("商品不存在。");
            }

            var contract = _context.Contracts
                .Include(c => c.RentalApplication)
                // CS8602: Dereference of a possibly null reference.
                // 檢查 c.RentalApplication 是否為 null
                .FirstOrDefault(c => c.RentalApplication != null &&
                                     c.RentalApplication.PropertyId == selectedPropertyId &&
                                     c.RentalApplication.MemberId == memberId &&
                                     c.Status == "ACTIVE" &&
                                     c.EndDate >= DateOnly.FromDateTime(DateTime.Today));

            if (contract == null)
            {
                TempData["ErrorMessage"] = "此房源無有效合約或合約到期日。";
                return RedirectToAction("ProductPurchasePage", new { id = productId });
            }

            var cart = _context.FurnitureCarts
                .Include(c => c.FurnitureCartItems)
                .FirstOrDefault(c => c.MemberId == memberId);

            if (cart == null)
            {
                // CS8604: Possible null reference argument for parameter 'entity'
                cart = new FurnitureCart
                {
                    FurnitureCartId = Guid.NewGuid().ToString(),
                    MemberId = memberId,
                    CreatedAt = DateTime.Now,

                };
                _context.FurnitureCarts.Add(cart);


            }

            // CS8602: Dereference of a possibly null reference.
            // 檢查 cart.FurnitureCartItems 是否為 null
            var cartItem = cart.FurnitureCartItems?.FirstOrDefault(item => item.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                cartItem.RentalDays = rentalDays;
                cartItem.SubTotal = cartItem.Quantity * rentalDays * product.DailyRental;
            }
            else
            {
                cartItem = new FurnitureCartItem
                {
                    CartItemId = Guid.NewGuid().ToString(),
                    CartId = cart.FurnitureCartId,
                    ProductId = productId,
                    Quantity = quantity,
                    RentalDays = rentalDays,
                    UnitPriceSnapshot = product.DailyRental,
                    SubTotal = quantity * rentalDays * product.DailyRental,
                    CreatedAt = DateTime.Now
                };
                // CS8602: Dereference of a possibly null reference.
                // 檢查 cart.FurnitureCartItems 是否為 null
                cart.FurnitureCartItems?.Add(cartItem);
            }

            _context.SaveChanges();

            return RedirectToAction("RentalCart");
        }

        //刪除購物車商品
        [HttpPost]
        public IActionResult RemoveCartItem(string cartItemId)
        {
            var item = _context.FurnitureCartItems.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item != null)
            {
                _context.FurnitureCartItems.Remove(item);
                _context.SaveChanges();
            }

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

            // 查詢進行中訂單
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
                    item.Order.Status,
                    // ✅ 新增 CurrentStage 狀態字串（連動庫存事件）
                    CurrentStage =
                        _context.InventoryEvents.Any(e => e.SourceId == item.OrderId && e.ProductId == item.ProductId && e.EventType == "RETURN") ? "RETURNED" :
                        _context.InventoryEvents.Any(e => e.SourceId == item.OrderId && e.ProductId == item.ProductId && e.EventType == "OUT") ? "RENTED" :
                        item.Order.Status == "SHIPPING" ? "SHIPPING" :
                        item.Order.Status == "PROCESSING" ? "PROCESSING" :
                        "PENDING"
                })
                .ToList();

            // 歷史訂單
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
