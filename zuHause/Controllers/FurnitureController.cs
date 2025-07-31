using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http; //用於 HttpContext.Session
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using zuHause.Models; // EF Core 的資料模型
using zuHause.ViewModels;


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
            SetCurrentMemberInfo();
            // 左側分類清單
            ViewBag.categories = GetAllCategories();

            // 輪播圖
            var carouselImages = _context.CarouselImages
            .Where(img => img.Category == "furniture" && img.IsActive == true)
            .OrderBy(img => img.DisplayOrder)
            .ToList();

            ViewBag.CarouselImages = carouselImages;

            // 熱門商品編號清單
            var popularProductNumbers = new List<string>
            {
                "FP20250729003831_7419f1",
                "FP20250729003657_68276b",
                "FP20250728235327_29c6d1",
                "FP20250728233424_bb45e6",
                "FP20250718154109_09bed9",
                "FP20250718152649_70f278",
                "FP20250718142629_dbab7f",
                "FP20250714160534_c2fb78"
            };
            // 從資料庫撈出熱門商品清單
            var hotProducts = _context.FurnitureProducts
                .Where(p => popularProductNumbers.Contains(p.FurnitureProductId))
                .ToList();

            ViewBag.hotProducts = hotProducts;

            // 特惠商品編號清單（用 FurnitureProductId）
            var discountProductIds = new List<string>
            {
                "FP20250721103313_9b5416",
                "FP20250714160732_a2a78f",
                "FP20250718142806_81ac8a",
                "FP20250729001927_a3297d",
                "FP20250729000557_9a001b"
            };

            // 從資料庫撈出特惠商品清單
            var discountProducts = _context.FurnitureProducts
                .Where(p => discountProductIds.Contains(p.FurnitureProductId))
                .ToList();

            ViewBag.discountProducts = discountProducts;

            //顯示所有商品
            var allProducts = _context.FurnitureProducts
                .Where(p => p.Status && p.DeletedAt == null) // 僅顯示上架中且未刪除商品
                .ToList();

            ViewBag.allProducts = allProducts;


            return View();
        }
        //會員登入資料
        private void SetCurrentMemberInfo()
        {
            // 使用統一的 Cookie 認證機制
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int memberId))
                {
                    ViewBag.CurrentMemberId = memberId;
                    var member = _context.Members.Find(memberId); // 查找完整會員資料（含頭像）
                    ViewBag.CurrentMemberName = member?.MemberName;
                    ViewBag.CurrentMember = member; // ✅ 頭貼資訊從這裡取得
                }
                else
                {
                    ViewBag.CurrentMemberId = null;
                    ViewBag.CurrentMemberName = null;
                    ViewBag.CurrentMember = null;
                }
            }
            else
            {
                ViewBag.CurrentMemberId = null;
                ViewBag.CurrentMemberName = null;
                ViewBag.CurrentMember = null;
            }
        }

        //會員登入動作執行時
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            SetCurrentMemberInfo(); // ✅ 這樣所有 Action 都能自動帶入 ViewBag
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
            var memberIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(memberIdString) || !int.TryParse(memberIdString, out int memberId))
            {
                return RedirectToAction("Login", "Member");
            }

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
                    c.RentalApplication.Property != null && // 加這行！
                    c.RentalApplication.MemberId == memberId &&
                   (c.Status == "active" || c.Status == "signed"))
                .Select(c => Tuple.Create(
                    c.RentalApplication!.Property!.PropertyId,
                    c.RentalApplication.Property.Title,
                    c.EndDate
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
        public IActionResult RentalCart(int? selectedPropertyId = null)
        {
            SetCurrentMemberInfo();

            if (ViewBag.CurrentMemberId == null)
            {
                var returnUrl = Url.Action("RentalCart", "Furniture", new { selectedPropertyId });
                return RedirectToAction("Login", "Member", new { ReturnUrl = returnUrl });
            }

            ViewBag.SelectedPropertyId = selectedPropertyId;

            int memberId = (int)ViewBag.CurrentMemberId;

            // 🔹 所有合約資料
            var propertyContracts = _context.Contracts
                .Include(c => c.RentalApplication)
                .ThenInclude(ra => ra!.Property)
                .Where(c => c.RentalApplication != null &&
                            c.RentalApplication.MemberId == memberId &&
                            ContractStatuses.ValidStatuses.Contains(c.Status) &&
                            c.EndDate >= DateOnly.FromDateTime(DateTime.Today))
                .ToList();

            // 🔹 若未選房源，預設選第一筆房源
            if (!selectedPropertyId.HasValue && propertyContracts.Any())
            {
                selectedPropertyId = propertyContracts.First().RentalApplication!.PropertyId;
            }

            // 抓「指定房源」的購物車
            if (!selectedPropertyId.HasValue && propertyContracts.Any())
            {
                var latestContract = propertyContracts.OrderByDescending(c => c.EndDate).FirstOrDefault();
                if (latestContract?.RentalApplication?.Property != null)
                {
                    selectedPropertyId = latestContract.RentalApplication.Property.PropertyId;
                }
            }

            // 🔹 抓「指定房源」的購物車
            var cart = _context.FurnitureCarts
                .Include(c => c.FurnitureCartItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefault(c => c.MemberId == memberId
                                  && c.PropertyId == selectedPropertyId
                                  && c.Status != "ORDERED");

            if (cart == null && selectedPropertyId.HasValue)
            {
                cart = new FurnitureCart
                {
                    FurnitureCartId = Guid.NewGuid().ToString(),
                    MemberId = memberId,
                    PropertyId = selectedPropertyId.Value,
                    CreatedAt = DateTime.Now,
                    Status = "IN_CART"
                };
                _context.FurnitureCarts.Add(cart);
                _context.SaveChanges();
            }

            // 🔹 房源下拉選單
            var propertySelectListItems = propertyContracts
                .Where(c => c.RentalApplication?.Property != null && c.EndDate.HasValue)
                .Select(c => new SelectListItem
                {
                    Value = c.RentalApplication!.Property!.PropertyId.ToString(),
                    Text = c.RentalApplication.Property.Title,
                    Selected = c.RentalApplication.Property.PropertyId == selectedPropertyId
                })
                .ToList();

            // 🔹 ViewBag 資料綁定（根據選定房源找 EndDate 和標題）
            Tuple<int, string, DateOnly?>? currentPropertyInfo = null;
            DateOnly? rentalEndDate = null;
            int rentalDaysLeft = 0;

            var selectedContract = propertyContracts
                .FirstOrDefault(c => c.RentalApplication?.Property?.PropertyId == selectedPropertyId);

            if (selectedContract != null && selectedContract.EndDate.HasValue)
            {
                var property = selectedContract.RentalApplication!.Property!;
                rentalEndDate = selectedContract.EndDate.Value;
                var today = DateOnly.FromDateTime(DateTime.Today);
                var days = (rentalEndDate.Value.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).TotalDays;
                rentalDaysLeft = Math.Max(0, (int)Math.Ceiling(days));
                currentPropertyInfo = new Tuple<int, string, DateOnly?>(property.PropertyId, property.Title, rentalEndDate);
            }

            ViewBag.PropertySelectList = new SelectList(propertySelectListItems, "Value", "Text", selectedPropertyId?.ToString());
            ViewBag.CurrentPropertyForCart = currentPropertyInfo;
            ViewBag.CurrentPropertyIdForCart = selectedPropertyId;
            ViewBag.RentalEndDate = rentalEndDate;
            ViewBag.RentalDaysLeft = rentalDaysLeft;

            // 🔹 前端 JS 用資料（全房源資訊）
            var propertyInfoForJs = propertyContracts
                .Where(c => c.RentalApplication?.Property != null && c.EndDate.HasValue)
                .Select(c => new
                {
                    PropertyId = c.RentalApplication!.Property!.PropertyId,
                    Title = c.RentalApplication.Property.Title,
                    ContractEndDate = c.EndDate!.Value.ToString("yyyy-MM-dd"),
                    DaysLeft = Math.Max(0, (int)(c.EndDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today).TotalDays)
                })
                .ToList();

            ViewBag.PropertyInfoJson = System.Text.Json.JsonSerializer.Serialize(propertyInfoForJs);

            // 🔹 計算總金額
            decimal totalAmount = 0;
            if (cart != null && cart.FurnitureCartItems != null)
            {
                foreach (var item in cart.FurnitureCartItems)
                {
                    if (item.Product != null)
                    {
                        totalAmount += item.Quantity * item.Product.DailyRental * item.RentalDays;
                    }
                }
            }
            ViewBag.TotalCartAmount = totalAmount;

            // 🔹 庫存資訊
            var inventoryMap = _context.FurnitureInventories
               .GroupBy(inv => inv.ProductId)
               .ToDictionary(g => g.Key, g => g.First().AvailableQuantity);
            ViewBag.InventoryMap = inventoryMap;

            return View(cart);
        }

        //加入商品到購物車清單add
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, int selectedPropertyId, int rentalDays, int quantity)
        {
            // 從 Claims 中取得登入會員 ID
            var memberIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(memberIdStr) || !int.TryParse(memberIdStr, out int memberId))
            {
                return RedirectToAction("Login", "Member");
            }

            // 1. 根據會員 + 房源找出是否已有購物車
            var cart = await _context.FurnitureCarts
                .Include(c => c.FurnitureCartItems)
                .FirstOrDefaultAsync(c => c.MemberId == memberId
                                       && c.PropertyId == selectedPropertyId
                                       && c.Status != "ORDERED");


            // 2. 若沒有就建立一筆購物車資料
            if (cart == null)
            {
                cart = new FurnitureCart
                {
                    FurnitureCartId = Guid.NewGuid().ToString(),
                    MemberId = memberId,
                    PropertyId = selectedPropertyId,
                    Status = "ACTIVE",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.FurnitureCarts.Add(cart);
                await _context.SaveChangesAsync(); // 先儲存讓 cart 有 ID
            }

            // 3. 檢查該商品是否已在購物車內
            var existingItem = cart.FurnitureCartItems
                .FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                // 數量累加
                existingItem.Quantity += quantity;
            }
            else
            {
                // 新增項目
                var newItem = new FurnitureCartItem
                {
                    CartItemId = Guid.NewGuid().ToString(),
                    CartId = cart.FurnitureCartId,
                    ProductId = productId,
                    Quantity = quantity,
                    RentalDays = rentalDays,
                    CreatedAt = DateTime.Now
                };
                _context.FurnitureCartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("RentalCart", new { selectedPropertyId }); // 回購物車頁顯示該房源資料
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

        //更新購物車商品數量
        [HttpPost]
        public IActionResult UpdateCartItemQuantity(string cartItemId, int quantity)
        {
            var cartItem = _context.FurnitureCartItems.FirstOrDefault(c => c.CartItemId == cartItemId);
            if (cartItem != null && quantity > 0)
            {
                cartItem.Quantity = quantity;
                _context.SaveChanges();
                return Ok();
            }
            return BadRequest();
        }

        public class CartItemUpdateRequest
        {
            public string CartItemId { get; set; } = "";
            public int Quantity { get; set; }
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

        // 呈現合約預覽畫面
        [HttpGet]
        public IActionResult ContractPreview(int selectedPropertyId)
        {
            SetCurrentMemberInfo();

            // 先宣告 memberId，並賦初始值，避免「未指派錯誤」
            int memberId = -1;

            // 嘗試從 ViewBag 轉型
            if (ViewBag.CurrentMemberId == null || !int.TryParse(ViewBag.CurrentMemberId.ToString(), out memberId))
            {
                TempData["ErrorMessage"] = "尚未登入會員，請重新操作。";
                return RedirectToAction("RentalCart");
            }

            if (selectedPropertyId == 0)
            {
                TempData["ErrorMessage"] = "房源 ID 不正確，請重新選擇。";
                return RedirectToAction("RentalCart");
            }

            var cart = _context.FurnitureCarts
                .Include(c => c.FurnitureCartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(c =>
                    c.MemberId == memberId &&
                    c.PropertyId == selectedPropertyId &&
                    c.Status != "ORDERED");

            var contract = _context.Contracts
                .Include(c => c.RentalApplication)
                .ThenInclude(ra => ra!.Property)
                .FirstOrDefault(c =>
                    c.RentalApplication != null &&
                    c.RentalApplication.Property != null &&
                    c.RentalApplication.MemberId == memberId &&
                    c.RentalApplication.PropertyId == selectedPropertyId &&
                    ContractStatuses.ValidStatuses.Contains(c.Status));


            if (contract != null && contract.EndDate != null)
            {
                DateOnly endDate = contract.EndDate.Value;
                DateTime today = DateTime.Today;

                int availableDays = (endDate.ToDateTime(TimeOnly.MinValue) - today).Days;
                ViewBag.TotalDays = availableDays > 0 ? availableDays : 0;
            }
            else
            {
                ViewBag.TotalDays = 0;
            }



            var member = _context.Members.FirstOrDefault(m => m.MemberId == memberId);

            ViewBag.Contract = contract;
            ViewBag.Property = contract?.RentalApplication?.Property;
            ViewBag.Member = member;
            ViewBag.Cart = cart;
            ViewBag.cartItems = cart?.FurnitureCartItems?.ToList();
            ViewBag.SelectedPropertyId = selectedPropertyId;

            return View();
        }

        [HttpPost]
        public IActionResult ContractPreviewPost(int selectedPropertyId)
        {
            if (selectedPropertyId == 0)
            {
                TempData["ErrorMessage"] = "請選擇一個房源再繼續簽約。";
                return RedirectToAction("RentalCart"); // 或回到上一頁
            }

            return RedirectToAction("ContractPreview", new { selectedPropertyId });
        }

        [HttpPost]
        public IActionResult SaveSignature([FromBody] SignatureDto dto)
        {
            if (string.IsNullOrEmpty(dto.SignatureDataUrl) || dto.SelectedPropertyId == 0)
            {
                return BadRequest("簽名資料錯誤");
            }

            var memberIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(memberIdStr) || !int.TryParse(memberIdStr, out int memberId))
            {
                return Unauthorized();
            }

            // 儲存簽名圖片
            var base64Data = dto.SignatureDataUrl.Split(',')[1];
            var bytes = Convert.FromBase64String(base64Data);
            var fileName = $"signature_{Guid.NewGuid()}.png";
            var filePath = Path.Combine("wwwroot/uploads/signatures", fileName);
            System.IO.File.WriteAllBytes(filePath, bytes);

            // 儲存簽名資料
            var newSignature = new ContractSignature
            {
                ContractId = _context.Contracts
                    .Where(c => c.RentalApplication != null &&
                                c.RentalApplication.MemberId == memberId &&
                                c.RentalApplication.PropertyId == dto.SelectedPropertyId &&
                                ContractStatuses.ValidStatuses.Contains(c.Status))
                    .Select(c => c.ContractId)
                    .FirstOrDefault(),

                SignatureFileUrl = "/uploads/signatures/" + fileName,
                SignedAt = DateTime.Now,
                SignerId = memberId,
                SignerRole = "TENANT",
                SignMethod = "CANVAS",
                SignVerifyInfo = "canvas pad",
                UploadId = null,
            };

            _context.ContractSignatures.Add(newSignature);
            _context.SaveChanges();

            // 儲存房源 ID 到 Session（重點）
            HttpContext.Session.SetInt32("SelectedPropertyId", dto.SelectedPropertyId);

            return Ok();
        }

        //接前端的電子簽約 POST JSON 內容
        public class SignatureDto
        {
            public string? SignatureDataUrl { get; set; }
            public int SelectedPropertyId { get; set; }
        }

        // 串接 Stripe 付款流程
        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession(string furnitureCartId, int totalAmount)
        {
            //var domain = "https://localhost:7010";
            var domain = $"{Request.Scheme}://{Request.Host}";

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
                    UnitAmount = totalAmount * 100, // 例如 500 會變 50000，Stripe 以「分」為單位
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "家具租借總金額",
                        Description = "總租金 + 搬運費"
                    }
                },
                Quantity = 1,
            },
        },
                Mode = "payment",
                SuccessUrl = $"{domain}/Furniture/Success",
                CancelUrl = $"{domain}/Furniture/CancelPayment?selectedPropertyId={furnitureCartId}",
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Redirect(session.Url);
        }

        //後端接收 TempData 並儲存選擇的房源 ID
        [HttpPost]
        public IActionResult BeforeStripePayment([FromBody] TempDataDto dto)
        {
            if (dto.SelectedPropertyId <= 0)
                return BadRequest();

            TempData["SelectedPropertyId"] = dto.SelectedPropertyId;
            return Ok();
        }

        public class TempDataDto
        {
            public int SelectedPropertyId { get; set; }
        }

        //支付成功
        public async Task<IActionResult> Success()
        {
            Console.WriteLine("✅ 成功進入 Success 方法");
            SetCurrentMemberInfo();

            // 統一認證機制：優先使用 Cookie，回退到 Session，並處理 Stripe 返回場景
            int memberId = 0;
            bool authenticationValid = false;

            // 1. 嘗試從 Cookie 認證獲取會員 ID
            var memberIdString = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(memberIdString) && int.TryParse(memberIdString, out memberId))
            {
                authenticationValid = true;
                Console.WriteLine($"✅ Cookie 認證成功，會員 ID: {memberId}");
            }
            else
            {
                // 2. Cookie 認證失效，嘗試 Session
                var sessionMemberId = HttpContext.Session.GetInt32("MemberId");
                if (sessionMemberId.HasValue)
                {
                    memberId = sessionMemberId.Value;
                    authenticationValid = true;
                    Console.WriteLine($"✅ Session 認證成功，會員 ID: {memberId}");

                    // 重新建立 Cookie 認證以保持一致性
                    var member = _context.Members.FirstOrDefault(m => m.MemberId == memberId);
                    if (member != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim("UserId", member.MemberId.ToString()),
                            new Claim(ClaimTypes.Name, member.MemberName ?? ""),
                            new Claim("PaymentCompleted", DateTime.Now.ToString())
                        };
                        var identity = new ClaimsIdentity(claims, "MemberCookieAuth");
                        var principal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync("MemberCookieAuth", principal);

                        // 同時確保 Session 也有設置
                        HttpContext.Session.SetInt32("MemberId", memberId);
                        SetCurrentMemberInfo();
                    }
                }
            }

            // 3. 認證失效處理 - 顯示友善頁面而非強制登入
            if (!authenticationValid)
            {
                Console.WriteLine("⚠️ 認證失效，顯示友善提示頁面");
                ViewBag.IsAuthExpired = true;
                ViewBag.LoginUrl = Url.Action("Login", "Member");
                ViewBag.CartUrl = Url.Action("RentalCart", "Furniture");
                ViewBag.Message = "支付已完成，但登入狀態過期。請重新登入查看訂單。";
                return View();
            }

            ViewBag.IsAuthExpired = false;
            ViewBag.CurrentMemberId = memberId;

            // 使用 Session 獲取選擇的房源 ID
            var selectedPropertyId = HttpContext.Session.GetInt32("SelectedPropertyId");
            if (selectedPropertyId == null)
            {
                Console.WriteLine("⚠️ 找不到選擇的房源 ID，返回購物車");
                ViewBag.Message = "支付成功，但找不到對應的房源資訊。請檢查您的訂單。";
                return RedirectToAction("RentalCart");
            }

            var cart = _context.FurnitureCarts
                .Include(c => c.FurnitureCartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(c => c.MemberId == memberId && c.PropertyId == selectedPropertyId);

            if (cart == null || cart.FurnitureCartItems == null || !cart.FurnitureCartItems.Any())
            {
                TempData["ErrorMessage"] = "找不到對應購物車資料。";
                return RedirectToAction("RentalCart");
            }

            string orderId = GenerateOrderId(selectedPropertyId.Value);

            // 計算總金額
            decimal totalAmount = 0;
            foreach (var item in cart.FurnitureCartItems)
            {
                totalAmount += item.Quantity * item.Product.DailyRental * item.RentalDays;
            }

            var order = new FurnitureOrder
            {
                FurnitureOrderId = orderId,
                MemberId = memberId,
                PropertyId = selectedPropertyId.Value,
                CreatedAt = DateTime.Now,
                Status = "CONFIRMED",
                PaymentStatus = "PAID",     // 付款狀態：已付款
                TotalAmount = totalAmount,  // 總金額
                UpdatedAt = DateTime.Now    // 更新時間
            };
            _context.FurnitureOrders.Add(order);

            foreach (var item in cart.FurnitureCartItems)
            {
                var orderItem = new FurnitureOrderItem
                {
                    FurnitureOrderItemId = Guid.NewGuid().ToString(),
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    DailyRentalSnapshot = item.Product.DailyRental,
                    RentalDays = item.RentalDays,
                    SubTotal = item.Quantity * item.Product.DailyRental * item.RentalDays,
                    CreatedAt = DateTime.Now
                };
                _context.FurnitureOrderItems.Add(orderItem);

                var history = new FurnitureOrderHistory
                {
                    FurnitureOrderHistoryId = Guid.NewGuid().ToString(),
                    OrderId = orderId,
                    ProductId = item.ProductId,  // 新增：商品 ID
                    ProductNameSnapshot = item.Product.ProductName,
                    Quantity = item.Quantity,
                    DailyRentalSnapshot = item.Product.DailyRental,
                    RentalStart = DateOnly.FromDateTime(DateTime.Today),
                    RentalEnd = DateOnly.FromDateTime(DateTime.Today.AddDays(item.RentalDays)),
                    SubTotal = item.Quantity * item.Product.DailyRental * item.RentalDays,
                    ItemStatus = "PAID",
                    CreatedAt = DateTime.Now
                };
                _context.FurnitureOrderHistories.Add(history);

                var inventory = _context.FurnitureInventories.FirstOrDefault(inv => inv.ProductId == item.ProductId);
                if (inventory != null)
                {
                    inventory.AvailableQuantity -= item.Quantity;
                    inventory.RentedQuantity += item.Quantity;

                    var inventoryEvent = new InventoryEvent
                    {
                        FurnitureInventoryId = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        SourceId = orderId,
                        SourceType = "order",
                        Quantity = -item.Quantity,
                        EventType = "OUTGOING",
                        OccurredAt = DateTime.Now,
                        RecordedAt = DateTime.Now
                    };
                    _context.InventoryEvents.Add(inventoryEvent);
                }
            }

            _context.FurnitureCartItems.RemoveRange(cart.FurnitureCartItems);
            _context.FurnitureCarts.Remove(cart);

            _context.SaveChanges();

            ViewBag.Message = "付款成功！";
            ViewBag.OrderId = orderId;
            return View("Success");
        }

        //付款取消
        [HttpPost]
        public IActionResult CancelPayment(int selectedPropertyId)
        {
            SetCurrentMemberInfo();

            // 彈性認證檢查：優先使用 Cookie 認證，回退到 Session
            var memberIdString = User.FindFirst("UserId")?.Value;
            int memberId;

            if (string.IsNullOrEmpty(memberIdString) || !int.TryParse(memberIdString, out memberId))
            {
                // Cookie 認證失效，嘗試使用 Session
                var sessionMemberId = HttpContext.Session.GetInt32("MemberId");
                if (sessionMemberId == null)
                {
                    // 設置認證失效標記但不強制登入
                    ViewBag.IsAuthExpired = true;
                    ViewBag.LoginUrl = Url.Action("Login", "Member");
                    ViewBag.CartUrl = Url.Action("RentalCart", "Furniture");
                    ViewBag.SelectedPropertyId = selectedPropertyId;
                    return View("CancelPayment");
                }
                memberId = sessionMemberId.Value;
            }

            // 認證有效時才執行購物車清理
            var cart = _context.FurnitureCarts
                .Include(c => c.FurnitureCartItems)
                .FirstOrDefault(c => c.MemberId == memberId && c.PropertyId == selectedPropertyId);

            if (cart != null)
            {
                _context.FurnitureCartItems.RemoveRange(cart.FurnitureCartItems);
                _context.FurnitureCarts.Remove(cart);
                _context.SaveChanges();
                ViewBag.CartCleared = true;
            }

            ViewBag.IsAuthExpired = false;
            ViewBag.SelectedPropertyId = selectedPropertyId;
            return View("CancelPayment");
        }

        //歷史訂單紀錄
        public IActionResult OrderHistory()
        {
            SetCurrentMemberInfo();

            //一套登入但分租屋與家具模組 下面這段是家具使用 Session (HttpContext.Session.GetInt32("MemberId")) 驗證登入
            var memberIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(memberIdString) || !int.TryParse(memberIdString, out int memberId))
            {
                return RedirectToAction("Login", "Member");
            }

            // 進行中訂單（排除 RETURNED）
            var ongoingOrders = _context.FurnitureOrderItems
                .Include(item => item.Order)
                .ThenInclude(order => order.Property)
                .Where(item => item.Order.MemberId == memberId)
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
                    PropertyName = item.Order.Property != null ? item.Order.Property.Title : "未綁定房源",
                    CurrentStage = _context.OrderEvents
                        .Where(e => e.OrderId == item.OrderId)
                        .OrderByDescending(e => e.OccurredAt)
                        .Select(e => e.EventType)
                        .FirstOrDefault() ?? "PENDING"
                })
               .Where(x => x.CurrentStage != "RETURNED" &&
                    x.Status != "DELIVERED" &&
                    x.Status != "RETURNED") // ✅ 排除已完成訂單
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            // 歷史訂單
            // ✅ Step 1: 抓取已完成訂單 ID（只抓 status 是 DELIVERED 或 RETURNED 的）
            var completedOrderIds = _context.FurnitureOrders
                .Where(o => o.MemberId == memberId &&
                            (o.Status == "DELIVERED" || o.Status == "RETURNED"))
                .Select(o => o.FurnitureOrderId)
                .ToList();

            // ✅ Step 2: 再用這些 ID 撈歷史訂單
            var completedOrders = _context.FurnitureOrderHistories
                .Include(his => his.Order)
                .ThenInclude(order => order.Property)
                .Where(his => completedOrderIds.Contains(his.OrderId)) // ✅ 只抓已完成的
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
                    his.CreatedAt,
                    PropertyName = his.Order.Property != null ? his.Order.Property.Title : "未綁定房源"
                })
                .ToList();


            ViewBag.OngoingOrders = ongoingOrders;
            ViewBag.CompletedOrders = completedOrders;

            return View("OrderHistory");
        }

        // 客服聯繫紀錄頁面
        public IActionResult ContactRecords()
        {
            SetCurrentMemberInfo();

            var memberIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(memberIdString) || !int.TryParse(memberIdString, out int memberId))
            {
                return RedirectToAction("Login", "Member");
            }

            // 取得所有該會員的客服票
            var tickets = _context.CustomerServiceTickets
                .Include(t => t.Member)
                .Include(t => t.Property)
                .Include(t => t.FurnitureOrder)
                .Where(t => t.MemberId == memberId && t.CategoryCode == "FURNITURE")
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            // 設定狀態代碼
            foreach (var t in tickets)
            {
                t.StatusCode = string.IsNullOrWhiteSpace(t.ReplyContent) ? "WAITING" : "REPLIED";
            }

            // 🔍 抓出所有客服單關聯的訂單 ID
            var orderIds = tickets
                .Where(t => !string.IsNullOrEmpty(t.FurnitureOrderId))
                .Select(t => t.FurnitureOrderId!)
                .Distinct()
                .ToList();

            // 🔍 抓出每張訂單的一筆商品（或全部）
            var orderItemDict = _context.FurnitureOrderItems
                .Where(i => orderIds.Contains(i.OrderId))
                .GroupBy(i => i.OrderId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault()); // 只取第一項商品

            // ✅ 傳遞商品資訊到 View
            ViewBag.OrderItemDict = orderItemDict;

            // 會員名稱
            ViewBag.MemberName = _context.Members
                .Where(m => m.MemberId == memberId)
                .Select(m => m.MemberName)
                .FirstOrDefault() ?? "會員";

            return View("ContactRecords", tickets);
        }

        //客服表單畫面
        public IActionResult ContactUsForm(string orderId)
        {
            SetCurrentMemberInfo();
            var memberIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(memberIdString) || !int.TryParse(memberIdString, out int memberId))
            {
                return RedirectToAction("Login", "Member");
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberId == memberId);

            // 查詢該會員的房源（這沒問題）
            var properties = _context.FurnitureOrders
                .Include(o => o.Property)
                .Where(o => o.FurnitureOrderId == orderId)
                .Select(o => o.Property)
                .Where(p => p != null)
                .Distinct()
                .ToList();


            // ✅ 用 orderId 去找對應的 propertyId（注意型別為 string）
            int? selectedPropertyId = _context.FurnitureOrders
                .Where(o => o.FurnitureOrderId == orderId)
                .Select(o => (int?)o.PropertyId)
                .FirstOrDefault();

            // 其他 ViewBag
            var subjects = new List<string>
    {
         "運費問題", "訂單問題","商品損壞","服務問題", "退換貨服務", "其他服務",
    };

            ViewBag.Member = member;
            ViewBag.Properties = properties;
            ViewBag.Subjects = subjects;
            ViewBag.OrderId = orderId;
            ViewBag.SelectedPropertyId = selectedPropertyId;

            return View("ContactUsForm");
        }

        //提交聯絡方式
        [HttpPost]
        public IActionResult SubmitContactForm(string Subject, string TicketContent, int? PropertyId)
        {
            var memberIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(memberIdString) || !int.TryParse(memberIdString, out int memberId))
            {
                return RedirectToAction("Login", "Member");
            }
            var ticket = new CustomerServiceTicket
            {
                MemberId = memberId,
                Subject = Subject,
                TicketContent = TicketContent,
                PropertyId = PropertyId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                StatusCode = "PENDING",
                IsResolved = false,
                CategoryCode = "FURNITURE"
            };

            _context.CustomerServiceTickets.Add(ticket);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "表單已送出，我們將盡快與您聯繫。";
            return RedirectToAction("ContactRecords");
        }

        //歷史訂單排序編碼
        private string GenerateOrderId(int propertyId)
        {
            var now = DateTime.Now;
            string timestamp = now.ToString("yyyyMMddHHmmssfff"); // 年月日時分秒毫秒
            return $"FO{timestamp}_{propertyId}";
        }

    }
}
