using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Azure.Core;

using zuHause.Models;
using zuHause.ViewModels;

namespace zuHause.Controllers
{

    [Route("Dashboard")]
    [Authorize(AuthenticationSchemes = "AdminCookies")]
    public class DashboardController : Controller
    {
        private readonly ZuHauseContext _context;
        public DashboardController(ZuHauseContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Dashboard 首頁 - 管理員權限控制的核心邏輯
        /// </summary>
        /// <returns>Dashboard 主頁面視圖</returns>
        /// <remarks>
        /// 權限控制流程：
        /// 1. 從 Cookie Claims 中取得管理員登入時存儲的權限資訊
        /// 2. 解析 PermissionsJSON (來自資料庫 AdminRoles.PermissionsJson)
        /// 3. 定義系統所有可用的功能鍵值 (allKeys)
        /// 4. 根據權限類型建立前端權限物件 ViewBag.RoleAccess
        /// 5. 傳遞給前端 JavaScript 進行動態選單控制
        /// 
        /// 權限資料來源：
        /// - AuthController 登入時從資料庫 AdminRoles 表讀取 PermissionsJson
        /// - 儲存至 Cookie Claims 的 "PermissionsJSON" 欄位
        /// - 此 Controller 解析並轉換為前端可用格式
        /// 
        /// 前端權限物件格式：
        /// - 完全權限時：{ "角色名稱": { "all": true } }
        /// - 部分權限時：{ "角色名稱": ["overview", "monitor", ...] }
        /// </remarks>
        #region 登入API
        [HttpGet("")]
        public IActionResult Index()
        {
            // === 步驟1：從 Cookie Claims 獲取管理員資訊 ===
            // 這些資訊在 AuthController.Login() 時從資料庫查詢並存入 Claims
            ViewBag.EmployeeID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.Role = HttpContext.User.FindFirst("RoleName")?.Value;
            ViewBag.Name = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;


            // === 步驟2：解析權限JSON ===
            // PermissionsJSON 來自資料庫 AdminRoles.PermissionsJson 欄位
            // 格式範例：{"all": true} 或 {"overview": true, "monitor": false, ...}
            var permissionsJSON = HttpContext.User.FindFirst("PermissionsJSON")?.Value ?? "{}";
            var permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(permissionsJSON);

            // === 步驟3：定義系統所有可用功能 ===
            // 這是唯一的硬編碼部分，定義了系統支援哪些功能模組
            // 實際權限完全由資料庫的 PermissionsJson 控制
            var allKeys = new List<string>
            {
                "overview", "monitor", "behavior", "orders", "system",
                "roles", "Backend_user_list", "contract_template",
                "platform_fee", "imgup", "furniture_fee", "Marquee_edit", "furniture_management",
                "announcement_management", "message_template_management", "member_list", "landlord_list", "property_list",
                "property_complaint_list", "customer_service_list", "system_message_list"
            };

            // === 步驟4：建立前端權限控制物件 ===
            // 根據 PermissionsJson 的格式決定傳給前端的資料結構
            if (permissions.TryGetValue("all", out bool isAll) && isAll)
            {
                // 超級管理員：完全權限，前端顯示所有功能
                ViewBag.RoleAccess = new Dictionary<string, object>
                {
                    [ViewBag.Role] = new { all = true }
                };
            }
            else
            {
                // 一般管理員：部分權限，只顯示有權限的功能
                // 從 permissions 中篩選出值為 true 且在 allKeys 中的功能
                var grantedKeys = permissions
                    .Where(p => p.Value && allKeys.Contains(p.Key))
                    .Select(p => p.Key)
                    .ToList();

                ViewBag.RoleAccess = new Dictionary<string, object>
                {
                    [ViewBag.Role] = grantedKeys
                };
            }

            // === 步驟5：傳遞給前端 ===
            // ViewBag.RoleAccess 會在 _DashboardLayout.cshtml 中序列化為 JavaScript 的 roleAccess 變數
            // 前端 dashboard.js 使用此變數控制左側選單的顯示
            return View();
        }
        #endregion

        #region 家具管理分頁
        //家具管理分頁
        [HttpGet("{id}")]
        public IActionResult LoadTab(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            if (id == "announcement_management")
            {
                // 取得所有 ANNOUNCEMENT 類型的公告資料，按建立時間排序
                var announcements = _context.SiteMessages
                    .Where(m => m.Category == "ANNOUNCEMENT" && m.DeletedAt == null)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new
                    {
                        m.SiteMessagesId,
                        m.Title,
                        m.SiteMessageContent,
                        m.ModuleScope,
                        m.DisplayOrder,
                        m.StartAt,
                        m.EndAt,
                        m.IsActive,
                        m.AttachmentUrl,
                        m.CreatedAt,
                        m.UpdatedAt
                    })
                    .ToList();

                return PartialView("~/Views/Dashboard/Partial/announcement_management.cshtml", announcements);
            }
            if (id == "message_template_management")
            {
                return PartialView("~/Views/Dashboard/Partial/message_template_management.cshtml");
            }
            if (id == "platform_fee")
            {
                var plans = _context.ListingPlans
                    .OrderBy(p => p.PlanId)
                    .ToList();

                return PartialView("~/Views/Dashboard/Partial/platform_fee.cshtml", plans);
            }
            if (id == "furniture_management")
            {
                var categories = _context.FurnitureCategories
                    .Where(c => c.ParentId == null) // 你可以保留或移除條件
                    .OrderBy(c => c.DisplayOrder)
                    .ToList();
                var data = from p in _context.FurnitureProducts
                           join i in _context.FurnitureInventories on p.FurnitureProductId equals i.ProductId
                           join c in _context.FurnitureCategories on p.CategoryId equals c.FurnitureCategoriesId into pc
                           from c in pc.DefaultIfEmpty()
                           where p.DeletedAt == null
                           select new FurnitureCardViewModel
                           {
                               FurnitureID = p.FurnitureProductId,
                               Name = p.ProductName,
                               Status = p.Status == true ? "上架" : "下架",
                               TotalQuantity = i.TotalQuantity,
                               Stock = i.AvailableQuantity,
                               SafetyStock = i.SafetyStock,
                               RentedCount = i.RentedQuantity,
                               Type = c != null ? c.Name : "(未分類)",
                               Description = p.Description,
                               OriginalPrice = p.ListPrice,
                               RentPerDay = p.DailyRental,
                               ListDate = p.ListedAt.HasValue
                                ? p.ListedAt.Value.ToDateTime(TimeOnly.MinValue).AddHours(8)
                                : DateTime.MinValue,
                               DelistDate = p.DelistedAt.HasValue
                                ? p.DelistedAt.Value.ToDateTime(TimeOnly.MinValue).AddHours(8)
                                : DateTime.MaxValue,
                               CreatedAt = p.CreatedAt.AddHours(8),
                               UpdatedAt = p.UpdatedAt.AddHours(8),
                               DeletedAt = p.DeletedAt.HasValue ? p.DeletedAt.Value.AddHours(8) : null,
                               ImageUrl = p.ImageUrl // 圖片URL
                           };
                ViewBag.Categories = categories;

                return PartialView("~/Views/Dashboard/Partial/furniture_management.cshtml", data.ToList());
            }
            var viewPath = $"~/Views/Dashboard/Partial/{id}.cshtml";
            if (!System.IO.File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Views", "Dashboard", "Partial", $"{id}.cshtml")))
            {
                return Content($"⚠️ 找不到對應的分頁檔案：{id}");
            }

            return PartialView(viewPath);

        }

        public IActionResult Backend_user_list()
        {
            return PartialView("Partial/_Backend_user_list");
        }
        [HttpPost("SoftDeleteFurniture")]
        public IActionResult SoftDeleteFurniture([FromQuery] string id)
        {
            var product = _context.FurnitureProducts.FirstOrDefault(p => p.FurnitureProductId == id);
            if (product == null)
                return NotFound("找不到對應的家具資料");
            product.Status = false; // 先將狀態設為下架,避免使用者誤租
            product.DeletedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return Content("✅ 家具已軟刪除！");
        }
        [HttpGet("GetFurnitureById")]
        public IActionResult GetFurnitureById(string id)
        {
            var product = (from p in _context.FurnitureProducts
                           join i in _context.FurnitureInventories on p.FurnitureProductId equals i.ProductId
                           where p.FurnitureProductId == id
                           select new
                           {
                               p.FurnitureProductId,
                               p.ProductName,
                               p.Description,
                               p.CategoryId,
                               p.ListPrice,
                               p.DailyRental,
                               p.Status,
                               p.ListedAt,
                               p.DelistedAt,
                               i.TotalQuantity,
                               i.AvailableQuantity,
                               i.RentedQuantity,
                               i.SafetyStock,
                               p.CreatedAt,
                               p.UpdatedAt
                           }).FirstOrDefault();

            if (product == null)
                return NotFound("❌ 查無此家具");

            return Json(product);
        }

        // 提前下架
        public class FurnitureOfflineRequest
        {
            public string? Id { get; set; }
        }
        [HttpPost("SetOffline")]
        public IActionResult SetOffline([FromBody] FurnitureOfflineRequest request)
        {

            var id = request?.Id;
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("❌ 家具ID不能為空");

            // 避免拉出 categoryId 或 imageUrl 造成 Null 崩潰
            var exists = _context.FurnitureProducts
                .Any(p => p.FurnitureProductId == id && p.DeletedAt == null);

            if (!exists) return NotFound("找不到資料");

            // 進行更新
            var product = _context.FurnitureProducts.First(p => p.FurnitureProductId == id);
            product.Status = false;
            product.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            return Content("✅ 已提前下架");
        }

        [HttpPost("UploadFurniture")]
        public async Task<IActionResult> UploadFurniture([FromForm] FurnitureUploadViewModel vm)
        {
            using var transaction = _context.Database.BeginTransaction();

            if (vm == null || string.IsNullOrWhiteSpace(vm.Name))
                return BadRequest("家具資料不完整");

            if (string.IsNullOrWhiteSpace(vm.Type))
                return BadRequest("❌ 請選擇一個家具分類");

            var categoryId = vm.Type.Trim();
            if (!_context.FurnitureCategories.Any(c => c.FurnitureCategoriesId == categoryId))
                return BadRequest("❌ 所選分類不存在");

            var newId = "FP" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);

            string? imageUrl = null;
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(vm.ImageFile.FileName);
                var fileName = $"{newId}{ext}";
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/images/{fileName}"; // 儲存到資料庫的路徑
            }

            var product = new FurnitureProduct
            {
                FurnitureProductId = newId,
                ProductName = vm.Name,
                Description = vm.Description,
                CategoryId = categoryId,
                ListPrice = vm.OriginalPrice,
                DailyRental = vm.RentPerDay,
                Status = vm.Status,
                ListedAt = vm.StartDate.HasValue ? DateOnly.FromDateTime(vm.StartDate.Value) : DateOnly.FromDateTime(DateTime.UtcNow),
                DelistedAt = vm.EndDate.HasValue ? DateOnly.FromDateTime(vm.EndDate.Value) : DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ImageUrl = imageUrl
            };

            var inventory = new FurnitureInventory
            {
                FurnitureInventoryId = Guid.NewGuid().ToString(),
                ProductId = newId,
                TotalQuantity = vm.Stock,
                SafetyStock = vm.SafetyStock,
                AvailableQuantity = vm.Stock,
                RentedQuantity = 0
            };

            var inventoryEvent = new InventoryEvent
            {
                FurnitureInventoryId = Guid.NewGuid(),
                ProductId = newId,
                EventType = "adjust_in",
                Quantity = vm.Stock,
                SourceType = "manual",
                SourceId = newId,
                OccurredAt = DateTime.UtcNow,
                RecordedAt = DateTime.UtcNow
            };

            try
            {
                _context.FurnitureProducts.Add(product);
                _context.FurnitureInventories.Add(inventory);
                _context.InventoryEvents.Add(inventoryEvent);
                await _context.SaveChangesAsync();
                transaction.Commit();
                return Ok("✅ 家具已成功上傳！");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, $"❌ 上傳失敗：{ex.Message}");
            }
        }

        [HttpPost("UpdateFurniture")]
        public async Task<IActionResult> UpdateFurniture([FromForm] FurnitureUploadViewModel vm)
        {
            if (vm == null || string.IsNullOrWhiteSpace(vm.FurnitureProductId))
                return BadRequest("❌ 家具資料不完整");

            var product = _context.FurnitureProducts
                .FirstOrDefault(p => p.FurnitureProductId == vm.FurnitureProductId);

            var inventory = _context.FurnitureInventories
                .FirstOrDefault(i => i.ProductId == vm.FurnitureProductId);

            if (product == null || inventory == null)
                return NotFound("❌ 找不到對應的家具或庫存資料");

            if (!_context.FurnitureCategories.Any(c => c.FurnitureCategoriesId == vm.Type))
                return BadRequest("❌ 所選分類不存在，請重新選擇");

            // ✅ 更新欄位
            product.ProductName = vm.Name;
            product.Description = vm.Description;
            product.CategoryId = vm.Type;
            product.ListPrice = vm.OriginalPrice;
            product.DailyRental = vm.RentPerDay;
            product.Status = vm.Status;
            product.ListedAt = vm.StartDate.HasValue ? DateOnly.FromDateTime(vm.StartDate.Value) : product.ListedAt;
            product.DelistedAt = vm.EndDate.HasValue ? DateOnly.FromDateTime(vm.EndDate.Value) : product.DelistedAt;
            product.UpdatedAt = DateTime.UtcNow;

            inventory.SafetyStock = vm.SafetyStock;
            inventory.UpdatedAt = DateTime.UtcNow;

            // ✅ 圖片處理
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                // 刪除舊圖片
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // 上傳新圖片
                var ext = Path.GetExtension(vm.ImageFile.FileName);
                var newFileName = $"{vm.FurnitureProductId}_{Guid.NewGuid().ToString("N").Substring(0, 6)}{ext}";
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", newFileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = $"/images/{newFileName}";
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok("✅ 家具資料已更新！");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ 更新失敗：{ex.Message}");
            }
        }


        [HttpGet("AllInventoryEvents")]
        public IActionResult AllInventoryEvents()
        {
            var events = _context.InventoryEvents
                .OrderByDescending(e => e.OccurredAt)
                .Select(e => new
                {
                    ProductId = e.ProductId,
                    e.EventType,
                    e.Quantity,
                    e.SourceType,
                    e.SourceId,
                    OccurredAt = e.OccurredAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                    RecordedAt = e.RecordedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                }).ToList();

            return Json(events);
        }
        [HttpPost("AdjustInventory")]
        public IActionResult AdjustInventory([FromBody] InventoryEvent data)
        {
            if (string.IsNullOrWhiteSpace(data.ProductId))
                return BadRequest("商品ID 為必填");

            // 🔍 查詢對應的庫存快照
            var inventory = _context.FurnitureInventories.FirstOrDefault(f => f.ProductId == data.ProductId);
            if (inventory == null)
                return NotFound("找不到對應的庫存資料");

            // 💾 建立異動事件
            var entity = new InventoryEvent
            {
                ProductId = data.ProductId,
                Quantity = data.Quantity,
                SourceType = data.SourceType,
                SourceId = data.SourceId,
                EventType = data.Quantity > 0 ? "adjust_in" : "adjust_out",
                OccurredAt = DateTime.UtcNow,
                RecordedAt = DateTime.UtcNow
            };
            _context.InventoryEvents.Add(entity);

            // 📦 更新快照資料
            inventory.TotalQuantity += data.Quantity;
            inventory.AvailableQuantity += data.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;

            // ✅ 避免負值
            if (inventory.TotalQuantity < 0) inventory.TotalQuantity = 0;
            if (inventory.AvailableQuantity < 0) inventory.AvailableQuantity = 0;

            _context.SaveChanges();

            return Content("✅ 手動庫存異動已紀錄並同步更新快照！");
        }
        #endregion

        #region 上架費方案相關 API 
        [HttpGet("GetAllListingPlans")]
        public IActionResult GetAllListingPlans()
        {
            var plans = _context.ListingPlans
                .OrderBy(p => p.PlanId)
                .Select(p => new
                {
                    p.PlanId,
                    p.PlanName,
                    p.PricePerDay,
                    p.MinListingDays,
                    p.CurrencyCode,
                    p.StartAt,
                    p.EndAt,
                    p.IsActive

                })
                .ToList();

            return Json(plans);
        }
        [HttpPost("CreateListingPlan")]
        public IActionResult CreateListingPlan([FromBody] ListingPlan model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PlanName))
                return BadRequest("❌ 資料不完整");
            var now = DateTime.Now;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.IsActive = false;

            if (model.StartAt == default)
                model.StartAt = DateTime.Now;

            if (model.StartAt < now)
            {
                return BadRequest("❌ 新方案的開始時間不可早於現在");
            }
            // 找出同 MinListingDays、尚未結束、時間區間重疊的方案
            var previous = _context.ListingPlans
                .Where(p => p.MinListingDays == model.MinListingDays
                         && p.StartAt < model.StartAt
                         && (p.EndAt == null || p.EndAt >= model.StartAt))
                .OrderByDescending(p => p.StartAt)
                .FirstOrDefault();

            if (previous != null)
            {
                if (model.StartAt <= previous.StartAt)
                    return BadRequest("❌ 新方案時間不可早於現有相同天數方案");

                // -- 判斷要不要補回原方案
                if (previous.EndAt == null && model.EndAt != null)
                {
                    // 限期活動：中斷原方案 → 補一筆回來
                    previous.EndAt = model.StartAt.AddSeconds(-1);

                    var restored = new ListingPlan
                    {
                        PlanName = previous.PlanName + "（接續）",
                        PricePerDay = previous.PricePerDay,
                        MinListingDays = previous.MinListingDays,
                        CurrencyCode = previous.CurrencyCode,
                        StartAt = model.EndAt.Value.AddSeconds(1),
                        EndAt = null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsActive = false
                    };

                    _context.ListingPlans.Add(restored);
                }
                else
                {
                    // 新方案是無限期 → 直接結束前一筆，不補回來
                    previous.EndAt = model.StartAt.AddSeconds(-1);
                }
            }

            _context.ListingPlans.Add(model);
            _context.SaveChanges();

            return Ok(new { success = true, planId = model.PlanId });
        }

        [HttpPost("UpdateListingPlan")]
        public IActionResult UpdateListingPlan([FromBody] ListingPlan model)
        {
            var existing = _context.ListingPlans.FirstOrDefault(p => p.PlanId == model.PlanId);
            if (existing == null)
                return NotFound("找不到要編輯的方案");

            var now = DateTime.Now;

            // ✅ 若此方案目前是執行中 → StartAt 不可被改成晚於現在
            bool isCurrentlyActive =
                    existing.IsActive &&
                    existing.StartAt <= now &&
                    (existing.EndAt == null || existing.EndAt >= now);


            if (isCurrentlyActive && model.StartAt > now)
            {
                return BadRequest("❌ 執行中方案的開始時間不可設定為未來，\n否則會造成中斷");
            }

            // ✅ 通過驗證後，更新資料
            existing.PlanName = model.PlanName;
            existing.PricePerDay = model.PricePerDay;
            existing.MinListingDays = model.MinListingDays;
            existing.CurrencyCode = model.CurrencyCode;
            existing.StartAt = model.StartAt;
            existing.EndAt = model.EndAt;
            existing.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return Ok(new { success = true });
        }

        [HttpGet("GetGroupedActivePlans")]
        public IActionResult GetGroupedActivePlans()
        {
            var now = DateTime.Now;

            var activePlans = _context.ListingPlans
                .Where(p => p.IsActive && p.StartAt <= now && (p.EndAt == null || p.EndAt >= now))
                .OrderBy(p => p.MinListingDays)
                .ThenBy(p => p.StartAt)
                .ToList();

            // Group by MinListingDays
            var grouped = activePlans
                .GroupBy(p => p.MinListingDays)
                .Select(g => new
                {
                    MinListingDays = g.Key,
                    Plans = g.Select(p => new
                    {
                        p.PlanName,
                        p.PricePerDay,
                        p.StartAt,
                        p.EndAt
                    }).ToList()
                })
                .ToList(); // ✅ 加上這個才會真的 materialize 為 List<object>

            return Json(grouped);
        }
        [HttpGet("GetScheduledPlans")]
        public IActionResult GetScheduledPlans()
        {
            var now = DateTime.Now;

            // 找出目前執行中的方案（依 MinListingDays 分組）
            var activeGroups = _context.ListingPlans
                .Where(p => p.IsActive && p.StartAt <= now && (p.EndAt == null || p.EndAt >= now))
                .GroupBy(p => p.MinListingDays)
                .Select(g => new
                {
                    MinListingDays = g.Key,
                    MaxEndAt = g.Max(p => p.EndAt ?? DateTime.MaxValue)
                })
                .ToList();

            // 撈出每個群組接續的方案（StartAt > now 及接續於該群組 EndAt）
            var scheduled = new List<object>();

            foreach (var group in activeGroups)
            {
                var upcomingPlans = _context.ListingPlans
                    .Where(p =>
                        p.MinListingDays == group.MinListingDays &&
                        p.StartAt > now &&
                        p.StartAt >= group.MaxEndAt
                    )
                    .OrderBy(p => p.StartAt)
                    .Select(p => new
                    {
                        p.PlanName,
                        p.PricePerDay,
                        p.StartAt,
                        p.EndAt
                    })
                    .ToList();

                if (upcomingPlans.Any())
                {
                    scheduled.Add(new
                    {
                        MinListingDays = group.MinListingDays,
                        Plans = upcomingPlans
                    });
                }
            }

            return Json(scheduled);
        }
        #endregion

        #region 運費設定API
        //載入所有方案
        [HttpGet("GetAllDeliveryPlans")]
        public IActionResult GetAllDeliveryPlans()
        {
            var plans = _context.DeliveryFeePlans
                        .OrderBy(p => p.PlanId)
                        .ToList();


            return Json(plans);
        }
        // 創建新的配送方案
        [HttpPost("CreateDeliveryPlan")]
        public IActionResult CreateDeliveryPlan([FromBody] DeliveryFeePlanViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.PlanName) || vm.BaseFee <= 0)
                return BadRequest("❌ 方案名稱與基礎費用為必填");

            var plan = new DeliveryFeePlan
            {
                PlanName = vm.PlanName,
                BaseFee = vm.BaseFee,
                RemoteAreaSurcharge = vm.RemoteAreaSurcharge,
                CurrencyCode = vm.CurrencyCode,
                StartAt = vm.StartAt,
                EndAt = vm.EndAt,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = false,
            };

            // 自訂 PlanId 最大值 +1
            plan.PlanId = (_context.DeliveryFeePlans.Max(p => (int?)p.PlanId) ?? 0) + 1;

            // 重疊區間檢查
            var newStart = plan.StartAt;
            var newEnd = plan.EndAt ?? DateTime.MaxValue;

            var overlap = _context.DeliveryFeePlans
                .Where(p => p.BaseFee == plan.BaseFee)
                .Where(p =>
                    (p.EndAt == null || newStart < p.EndAt) &&
                    (newEnd == DateTime.MaxValue || p.StartAt < newEnd)
                )
                .Any();

            if (overlap)
                return BadRequest("該基礎費用區間已有重疊的方案，請調整時間。");

            _context.DeliveryFeePlans.Add(plan);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        // 編輯方案
        [HttpPost("UpdateDeliveryPlan")]
        public IActionResult UpdateDeliveryPlan([FromBody] DeliveryFeePlan plan)
        {
            var existing = _context.DeliveryFeePlans.FirstOrDefault(p => p.PlanId == plan.PlanId);
            if (existing == null)
                return NotFound("❌ 找不到該配送方案");

            existing.PlanName = plan.PlanName;
            existing.BaseFee = plan.BaseFee;
            existing.RemoteAreaSurcharge = plan.RemoteAreaSurcharge;
            existing.CurrencyCode = plan.CurrencyCode;

            existing.StartAt = plan.StartAt;
            existing.EndAt = plan.EndAt;
            existing.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return Json(new { success = true });
        }
        // 取得目前啟用的配送方案，並依 BaseFee 分組

        [HttpGet("GetGroupedActiveDeliveryPlans")]
        public IActionResult GetGroupedActiveDeliveryPlans()
        {
            var now = DateTime.Now;

            var activePlans = _context.DeliveryFeePlans
                .Where(p => p.StartAt <= now && (p.EndAt == null || p.EndAt > now) && p.IsActive)
                .GroupBy(p => p.BaseFee)
                .Select(g => new
                {
                    BaseFee = g.Key,
                    Plans = g.OrderBy(p => p.StartAt).ToList()
                })
                .OrderBy(g => g.BaseFee)
                .ToList();

            return Json(activePlans);
        }

        // 取得未來的配送方案，並依 BaseFee 分組
        [HttpGet("GetScheduledDeliveryPlans")]
        public IActionResult GetScheduledDeliveryPlans()
        {
            var now = DateTime.Now;

            var futurePlans = _context.DeliveryFeePlans
                .Where(p => p.StartAt > now && !p.IsActive)
                .GroupBy(p => p.BaseFee)
                .Select(g => new
                {
                    BaseFee = g.Key,
                    Plans = g.OrderBy(p => p.StartAt).ToList()
                })
                .OrderBy(g => g.BaseFee)
                .ToList();

            return Json(futurePlans);
        }

        #endregion

        #region 合約範本相關 API

        // 取得所有合約範本
        [HttpGet("GetContractTemplates")]
        public IActionResult GetContractTemplates()
        {
            var list = _context.ContractTemplates
                        .OrderByDescending(t => t.UploadedAt)
                        .ToList();
            return Json(list);
        }

        // 上傳新範本
        [HttpPost("UploadContractTemplate")]
        public IActionResult UploadContractTemplate([FromBody] ContractTemplateUploadViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.TemplateName) || string.IsNullOrWhiteSpace(vm.TemplateContent))
                return BadRequest("請填寫完整資料");

            // ✅ 取得目前最大 ID，並 +1 當作新 ID
            int nextId = (_context.ContractTemplates.Max(t => (int?)t.ContractTemplateId) ?? 0) + 1;

            var entity = new ContractTemplate
            {
                ContractTemplateId = nextId, // ✅ 手動指定
                TemplateName = vm.TemplateName,
                TemplateContent = vm.TemplateContent,
                UploadedAt = DateTime.UtcNow
            };

            _context.ContractTemplates.Add(entity);
            _context.SaveChanges();

            return Ok(entity);
        }


        // 刪除範本
        [HttpPost("DeleteContractTemplate")]
        public IActionResult DeleteContractTemplate([FromBody] int id)
        {
            var item = _context.ContractTemplates.Find(id);
            if (item == null)
                return NotFound();

            _context.ContractTemplates.Remove(item);
            _context.SaveChanges();

            return Ok();
        }
        //編輯範本
        [HttpPost("UpdateContractTemplate")]
        public IActionResult UpdateContractTemplate([FromBody] ContractTemplate model)
        {
            var existing = _context.ContractTemplates.FirstOrDefault(x => x.ContractTemplateId == model.ContractTemplateId);
            if (existing == null) return NotFound("找不到範本");

            existing.TemplateContent = model.TemplateContent;
            existing.UploadedAt = DateTime.Now;
            _context.SaveChanges();

            return Ok();
        }
        #endregion

        #region 輪播圖相關 API
        //取得所有圖片
        [HttpGet("GetCarouselImages")]
        public IActionResult GetCarouselImages()
        {
            var images = _context.CarouselImages
                .Where(c => c.DeletedAt == null)
                .OrderBy(c => c.DisplayOrder)
                .ToList();

            return Json(images);
        }

        [HttpPost("UploadCarouselImage")]
        public async Task<IActionResult> UploadCarouselImage([FromForm] IFormFile imageFile, [FromForm] CarouselImage model)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("請選擇圖片");

            var category = model.Category;
            var requestedOrder = model.DisplayOrder;

            // 取得此分類下目前最大順序
            var maxOrder = _context.CarouselImages
            .Where(c => c.Category == category && c.DeletedAt == null)
            .Select(c => c.DisplayOrder)
            .ToList() // ✅ 轉成記憶體集合
            .DefaultIfEmpty(0)
            .Max();


            // 如果使用者沒輸入或輸入超過最大值 → 強制設為 max + 1
            if (requestedOrder <= 0 || requestedOrder > maxOrder + 1)
                model.DisplayOrder = maxOrder + 1;
            else
            {
                // 插入指定位置，調整其他順序（後移）
                var affected = _context.CarouselImages
                    .Where(c => c.Category == category && c.DeletedAt == null && c.DisplayOrder >= requestedOrder)
                    .ToList();

                foreach (var img in affected)
                {
                    img.DisplayOrder += 1;
                }
            }

            // 儲存圖片
            var ext = Path.GetExtension(imageFile.FileName);
            var fileName = $"carousel_{Guid.NewGuid().ToString("N").Substring(0, 6)}{ext}";
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
            var startAtStr = Request.Form["StartAt"];
            if (string.IsNullOrWhiteSpace(startAtStr))
                return BadRequest("請選擇開始時間");

            model.StartAt = DateTime.Parse(startAtStr);

            // EndAt 可空
            var endAtStr = Request.Form["EndAt"];
            model.EndAt = string.IsNullOrWhiteSpace(endAtStr) ? null : DateTime.Parse(endAtStr);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            model.ImageUrl = $"/images/{fileName}";
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;


            _context.CarouselImages.Add(model);
            await _context.SaveChangesAsync();

            return Ok("✅ 上傳成功");
        }


        //編輯圖片資訊
        [HttpPost("UpdateCarouselImage")]
        public async Task<IActionResult> UpdateCarouselImage([FromBody] CarouselImage model)
        {
            try
            {
                var entity = _context.CarouselImages
                    .FirstOrDefault(c => c.CarouselImageId == model.CarouselImageId);
                if (entity == null) return NotFound("找不到圖片");

                var sameCategory = model.Category;
                var newOrder = model.DisplayOrder;

                if (entity.DisplayOrder != newOrder)
                {
                    var target = _context.CarouselImages.FirstOrDefault(c =>
                        c.Category == sameCategory &&
                        c.DisplayOrder == newOrder &&
                        c.CarouselImageId != model.CarouselImageId &&
                        c.DeletedAt == null);

                    if (target != null)
                    {
                        // 交換順序
                        target.DisplayOrder = entity.DisplayOrder;
                    }

                    entity.DisplayOrder = newOrder;
                }
                if (model.StartAt == null)
                    return BadRequest("請選擇開始時間");

                entity.ImagesName = model.ImagesName;
                entity.Category = model.Category;
                entity.WebUrl = model.WebUrl;
                entity.StartAt = model.StartAt;
                entity.EndAt = model.EndAt;
                entity.IsActive = model.IsActive;
                entity.UpdatedAt = DateTime.UtcNow;
                // 若 PageCode 為空，自動用 Category 填補 目前只有首頁輪播 就先這樣存
                entity.PageCode = string.IsNullOrWhiteSpace(model.PageCode) ? model.Category : model.PageCode;
                await _context.SaveChangesAsync();
                return Ok("✅ 編輯成功");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ 編輯失敗：{ex.Message} -- {ex.InnerException?.Message}");
            }
        }



        //刪除圖片
        [HttpPost("DeleteCarouselImage")]
        public IActionResult DeleteCarouselImage([FromBody] int id)
        {
            var entity = _context.CarouselImages.FirstOrDefault(c => c.CarouselImageId == id);
            if (entity == null) return NotFound();

            var category = entity.Category;
            var deletedOrder = entity.DisplayOrder;

            entity.DeletedAt = DateTime.UtcNow;
            entity.IsActive = false;

            // 找出同分類中比這個圖片順序高的，全部 -1
            var others = _context.CarouselImages
                .Where(c => c.Category == category && c.DisplayOrder > deletedOrder && c.DeletedAt == null)
                .ToList();

            foreach (var item in others)
            {
                item.DisplayOrder -= 1;
            }

            _context.SaveChanges();

            return Ok("✅ 已刪除並調整順序");
        }
        //交換順序

        [HttpPost("SwapCarouselOrder")]
        public IActionResult SwapCarouselOrder([FromBody] SwapOrderDto dto)
        {
            if (dto.ImageId1 == dto.ImageId2)
                return BadRequest("不能交換自己");

            var image1 = _context.CarouselImages.FirstOrDefault(c => c.CarouselImageId == dto.ImageId1 && c.DeletedAt == null);
            var image2 = _context.CarouselImages.FirstOrDefault(c => c.CarouselImageId == dto.ImageId2 && c.DeletedAt == null);

            if (image1 == null || image2 == null)
                return NotFound("找不到要交換的圖片");

            if (image1.Category != image2.Category)
                return BadRequest("只能交換相同分類的圖片");

            // 交換順序
            int temp = image1.DisplayOrder;
            image1.DisplayOrder = image2.DisplayOrder;
            image2.DisplayOrder = temp;

            image1.UpdatedAt = DateTime.UtcNow;
            image2.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok("✅ 順序已交換");
        }

        public class SwapOrderDto
        {
            public int ImageId1 { get; set; }
            public int ImageId2 { get; set; }
        }
        [HttpPost("ToggleCarouselActive")]
        public IActionResult ToggleCarouselActive([FromBody] int id)
        {
            var image = _context.CarouselImages.FirstOrDefault(c => c.CarouselImageId == id && c.DeletedAt == null);
            if (image == null) return NotFound("找不到圖片");

            image.IsActive = !image.IsActive;
            image.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok($"已{(image.IsActive ? "啟用" : "停用")}");
        }

        //抓取輪播分類
        [HttpGet("GetCarouselCategories")]
        public IActionResult GetCarouselCategories()
        {
            var categories = _context.Pages
                .Where(p => p.ModuleScope == "carousel" && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new
                {
                    p.PageCode,
                    p.PageName
                })
                .ToList();

            return Json(categories);
        }
        #endregion

        #region 跑馬燈相關 API
        //取得訊息表的跑馬燈
        [HttpGet("GetMarquees")]
        public IActionResult GetMarquees(string scope)
        {
            var messages = _context.SiteMessages
                .Where(m =>
                    m.Category == "MARQUEE" &&
                    m.ModuleScope == scope &&
                    m.DeletedAt == null)
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new
                {
                    m.SiteMessagesId,
                    m.Title,
                    m.SiteMessageContent,
                    m.DisplayOrder,
                    m.StartAt,
                    m.EndAt,
                    m.IsActive,
                    m.AttachmentUrl
                })
                .ToList();

            return Json(messages);
        }

        [HttpPost("AddMarquee")]
        public IActionResult AddMarquee([FromBody] MarqueeCreateViewModel dto)
        {
            var newMessage = new SiteMessage
            {
                // ❌ 千萬不要設定 SiteMessagesId
                SiteMessageContent = dto.SiteMessageContent,
                DisplayOrder = dto.DisplayOrder,
                StartAt = dto.StartAt,
                EndAt = dto.EndAt,
                IsActive = dto.IsActive,
                ModuleScope = dto.ModuleScope,
                Category = "MARQUEE",
                MessageType = "SYSTEM",
                AttachmentUrl = dto.AttachmentUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now

            };

            _context.Entry(newMessage).State = EntityState.Added; // 🔧 可選保險
            _context.SiteMessages.Add(newMessage);
            _context.SaveChanges();

            return Ok(new { success = true, newMessage.SiteMessagesId });
        }



        [HttpPost("BatchUpdateMarquees")]
        public IActionResult BatchUpdateMarquees([FromBody] List<MarqueeUpdateViewModel> updates)
        {
            foreach (var dto in updates)
            {
                var message = _context.SiteMessages
                    .FirstOrDefault(m => m.SiteMessagesId == dto.SiteMessagesId && m.DeletedAt == null);

                if (message != null)
                {
                    message.SiteMessageContent = dto.SiteMessageContent;
                    message.DisplayOrder = dto.DisplayOrder;
                    message.StartAt = dto.StartAt;
                    message.EndAt = dto.EndAt;
                    message.IsActive = dto.IsActive;
                    message.UpdatedAt = DateTime.Now;
                    message.AttachmentUrl = dto.AttachmentUrl;
                }
            }

            _context.SaveChanges();
            return Ok(new { success = true });
        }


        [HttpPost("DeleteMarquee")]
        public IActionResult DeleteMarquee([FromBody] int id)
        {
            var message = _context.SiteMessages.FirstOrDefault(m => m.SiteMessagesId == id && m.DeletedAt == null);
            if (message == null) return NotFound();
            message.IsActive = false;
            message.DeletedAt = DateTime.Now;
            message.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            return Ok(new { success = true });
        }
        #endregion

        #region ========== 公告管理相關 API ==========

        /// <summary>
        /// 取得所有公告 - 支援分頁和篩選
        /// </summary>
        /// <param name="scope">模組範圍篩選 (TENANT, LANDLORD, FURNITURE, COMMON)</param>
        /// <param name="keyword">關鍵字搜尋 (標題或內容)</param>
        /// <param name="status">發布狀態篩選 (true=已發布, false=未發布)</param>
        /// <param name="page">頁碼，預設為 1</param>
        /// <param name="pageSize">每頁筆數，預設為 10</param>
        [HttpGet("GetAnnouncements")]
        public IActionResult GetAnnouncements(string? scope = null, string? keyword = null, bool? status = null, int page = 1, int pageSize = 10)
        {
            var query = _context.SiteMessages
                .Where(m => m.Category == "ANNOUNCEMENT" && m.DeletedAt == null);

            // 依模組範圍篩選
            if (!string.IsNullOrWhiteSpace(scope))
            {
                query = query.Where(m => m.ModuleScope == scope.ToUpper());
            }

            // 關鍵字搜尋 (標題或內容)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(m => m.Title.Contains(keyword) || m.SiteMessageContent.Contains(keyword));
            }

            // 發布狀態篩選
            if (status.HasValue)
            {
                query = query.Where(m => m.IsActive == status.Value);
            }

            var total = query.Count();
            var announcements = query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.SiteMessagesId,
                    m.Title,
                    m.SiteMessageContent,
                    m.ModuleScope,
                    m.DisplayOrder,
                    m.StartAt,
                    m.EndAt,
                    m.IsActive,
                    m.AttachmentUrl,
                    CreatedAt = m.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = m.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            return Json(new
            {
                data = announcements,
                total = total,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        /// <summary>
        /// 新增公告
        /// </summary>
        [HttpPost("CreateAnnouncement")]
        public IActionResult CreateAnnouncement([FromBody] AnnouncementCreateViewModel dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SiteMessageContent))
                return BadRequest("標題和內容不能為空");

            // 驗證模組範圍
            var validScopes = new[] { "TENANT", "LANDLORD", "FURNITURE", "COMMON" };
            if (!validScopes.Contains(dto.ModuleScope?.ToUpper()))
                return BadRequest("無效的模組範圍，必須是 TENANT、LANDLORD、FURNITURE 或 COMMON");

            var announcement = new SiteMessage
            {
                Title = dto.Title,
                SiteMessageContent = dto.SiteMessageContent,
                Category = "ANNOUNCEMENT",
                ModuleScope = dto.ModuleScope.ToUpper(),
                MessageType = "SYSTEM",
                DisplayOrder = dto.DisplayOrder ?? 1,
                StartAt = dto.StartAt ?? DateTime.Now,
                EndAt = dto.EndAt,
                IsActive = dto.IsActive ?? true,
                AttachmentUrl = dto.AttachmentUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.SiteMessages.Add(announcement);
            _context.SaveChanges();

            return Ok(new { success = true, id = announcement.SiteMessagesId });
        }

        /// <summary>
        /// 更新公告
        /// </summary>
        [HttpPost("UpdateAnnouncement")]
        public IActionResult UpdateAnnouncement([FromBody] AnnouncementUpdateViewModel dto)
        {
            var announcement = _context.SiteMessages
                .FirstOrDefault(m => m.SiteMessagesId == dto.SiteMessagesId &&
                                   m.Category == "ANNOUNCEMENT" &&
                                   m.DeletedAt == null);

            if (announcement == null)
                return NotFound("找不到指定的公告");

            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SiteMessageContent))
                return BadRequest("標題和內容不能為空");

            // 驗證模組範圍
            var validScopes = new[] { "TENANT", "LANDLORD", "FURNITURE", "COMMON" };
            if (!validScopes.Contains(dto.ModuleScope?.ToUpper()))
                return BadRequest("無效的模組範圍，必須是 TENANT、LANDLORD、FURNITURE 或 COMMON");

            announcement.Title = dto.Title;
            announcement.SiteMessageContent = dto.SiteMessageContent;
            announcement.ModuleScope = dto.ModuleScope.ToUpper();
            announcement.DisplayOrder = dto.DisplayOrder ?? announcement.DisplayOrder;
            announcement.StartAt = dto.StartAt ?? announcement.StartAt;
            announcement.EndAt = dto.EndAt;
            announcement.IsActive = dto.IsActive ?? announcement.IsActive;
            announcement.AttachmentUrl = dto.AttachmentUrl;
            announcement.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return Ok(new { success = true });
        }

        /// <summary>
        /// 刪除公告 (軟刪除)
        /// </summary>
        [HttpPost("DeleteAnnouncement")]
        public IActionResult DeleteAnnouncement([FromBody] int id)
        {
            var announcement = _context.SiteMessages
                .FirstOrDefault(m => m.SiteMessagesId == id &&
                                   m.Category == "ANNOUNCEMENT" &&
                                   m.DeletedAt == null);

            if (announcement == null)
                return NotFound("找不到指定的公告");

            announcement.IsActive = false;
            announcement.DeletedAt = DateTime.Now;
            announcement.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return Ok(new { success = true });
        }

        /// <summary>
        /// 切換公告狀態 (啟用/停用)
        /// </summary>
        [HttpPost("ToggleAnnouncementStatus")]
        public IActionResult ToggleAnnouncementStatus([FromBody] int id)
        {
            var announcement = _context.SiteMessages
                .FirstOrDefault(m => m.SiteMessagesId == id &&
                                   m.Category == "ANNOUNCEMENT" &&
                                   m.DeletedAt == null);

            if (announcement == null)
                return NotFound("找不到指定的公告");

            announcement.IsActive = !announcement.IsActive;
            announcement.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return Ok(new { success = true, isActive = announcement.IsActive });
        }

        /// <summary>
        /// 取得公告詳細資料
        /// </summary>
        [HttpGet("GetAnnouncementById/{id}")]
        public IActionResult GetAnnouncementById(int id)
        {
            var announcement = _context.SiteMessages
                .Where(m => m.SiteMessagesId == id &&
                          m.Category == "ANNOUNCEMENT" &&
                          m.DeletedAt == null)
                .Select(m => new
                {
                    m.SiteMessagesId,
                    m.Title,
                    m.SiteMessageContent,
                    m.ModuleScope,
                    m.DisplayOrder,
                    m.StartAt,
                    m.EndAt,
                    m.IsActive,
                    m.AttachmentUrl,
                    CreatedAt = m.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = m.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .FirstOrDefault();

            if (announcement == null)
                return NotFound("找不到指定的公告");

            return Json(announcement);
        }

        /// <summary>
        /// 取得可用的模組範圍清單
        /// </summary>
        [HttpGet("GetAnnouncementScopes")]
        public IActionResult GetAnnouncementScopes()
        {
            var scopes = new[]
            {
                new { value = "TENANT", text = "租戶端" },
                new { value = "LANDLORD", text = "房東端" },
                new { value = "FURNITURE", text = "家具端" },
                new { value = "COMMON", text = "通用" }
            };

            return Json(scopes);
        }

        // ViewModel classes for announcement management
        public class AnnouncementCreateViewModel
        {
            public string Title { get; set; }
            public string SiteMessageContent { get; set; }
            public string ModuleScope { get; set; }
            public int? DisplayOrder { get; set; }
            public DateTime? StartAt { get; set; }
            public DateTime? EndAt { get; set; }
            public bool? IsActive { get; set; }
            public string? AttachmentUrl { get; set; }
        }

        public class AnnouncementUpdateViewModel
        {
            public int SiteMessagesId { get; set; }
            public string Title { get; set; }
            public string SiteMessageContent { get; set; }
            public string ModuleScope { get; set; }
            public int? DisplayOrder { get; set; }
            public DateTime? StartAt { get; set; }
            public DateTime? EndAt { get; set; }
            public bool? IsActive { get; set; }
            public string? AttachmentUrl { get; set; }
        }
        #endregion

        #region 權限身分表相關 API
        //權限身分表
        [HttpGet("roles/list")]
        public IActionResult GetRolesWithPermissions()
        {
            var roles = _context.AdminRoles
                .Where(r => r.IsActive)
                .OrderBy(r => r.CreatedAt)
                .ToList() // ✅ 先取出資料
                .Select(r => new
                {
                    roleCode = r.RoleCode,
                    roleName = r.RoleName,
                    permissions = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(r.PermissionsJson)
                }).ToList();

            return Json(roles);
        }

        [HttpPost("roles/create")]
        public IActionResult CreateRole([FromBody] CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoleName))
                return BadRequest("角色名稱不得為空");

            // 權限物件轉 JSON（轉成 Dictionary 格式）
            var permissions = request.AccessKeys
                .Distinct()
                .ToDictionary(k => k, v => true);

            var newRole = new AdminRole
            {
                RoleCode = $"ROLE_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                RoleName = request.RoleName,
                PermissionsJson = JsonSerializer.Serialize(permissions),
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.AdminRoles.Add(newRole);
            _context.SaveChanges();

            return Ok(new { success = true, message = "角色新增成功" });
        }

        public class CreateRoleRequest
        {
            public string RoleName { get; set; }
            public List<string> AccessKeys { get; set; }
        }

        [HttpPost("roles/delete")]
        public IActionResult DeleteRole([FromBody] string roleCode)
        {
            var role = _context.AdminRoles.FirstOrDefault(r => r.RoleCode == roleCode);
            if (role == null) return NotFound("找不到此角色");

            role.IsActive = false;
            role.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            return Ok(new { success = true, message = "角色已刪除" });
        }
        [HttpPut("roles/update")]
        public IActionResult UpdateRole([FromBody] UpdateRoleRequest request)
        {
            var role = _context.AdminRoles.FirstOrDefault(r => r.RoleCode == request.RoleCode && r.IsActive);
            if (role == null) return NotFound("找不到角色");

            role.RoleName = request.RoleName;
            role.PermissionsJson = JsonSerializer.Serialize(
                request.AccessKeys?.Distinct().ToDictionary(k => k, v => true) ?? new Dictionary<string, bool>()
            );
            role.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return Ok(new { success = true, message = "角色已更新" });
        }

        public class UpdateRoleRequest
        {
            public string RoleCode { get; set; }
            public string RoleName { get; set; }
            public List<string> AccessKeys { get; set; }
        }

        [HttpGet("admins/list")]
        public IActionResult GetAdminList()
        {
            var admins = _context.Admins
                .Where(a => a.DeletedAt == null && a.IsActive)
                .Select(a => new
                {
                    a.AdminId,
                    a.Account,
                    a.Name,
                    a.RoleCode
                })
                .OrderBy(a => a.AdminId)
                .ToList();

            return Json(admins);
        }

        [HttpPost("admins/create")]
        public IActionResult CreateAdmin([FromBody] CreateAdminRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Account) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.RoleCode))
            {
                return BadRequest("請填寫完整資料");
            }

            // 檢查帳號是否重複
            if (_context.Admins.Any(a => a.Account == request.Account && a.DeletedAt == null))
            {
                return Conflict("帳號已存在");
            }

            // 🔢 自訂 ID：取最大 adminId + 1
            var maxId = _context.Admins.Max(a => (int?)a.AdminId) ?? 1000;
            var nextId = maxId + 1;

            // 產生 Salt 並雜湊密碼
            var salt = Guid.NewGuid().ToString("N").Substring(0, 8);
            var passwordHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password + salt))
            );

            var now = DateTime.Now;

            var admin = new Admin
            {
                AdminId = nextId, // ✅ 手動指定主鍵 ID
                Account = request.Account,
                PasswordHash = passwordHash,
                PasswordSalt = salt,
                Name = request.Name,
                RoleCode = request.RoleCode,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                PasswordUpdatedAt = now
            };

            _context.Admins.Add(admin);
            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "後台管理人員新增成功 ID:",
                adminId = admin.AdminId // ✅ 回傳 ID
            });
        }


        public class CreateAdminRequest
        {
            public string Account { get; set; }
            public string Password { get; set; }
            public string Name { get; set; }
            public string RoleCode { get; set; }
        }
        [HttpGet("roles/activeList")]
        public IActionResult GetActiveRoles()
        {
            var roles = _context.AdminRoles
                .Where(r => r.IsActive)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new
                {
                    roleCode = r.RoleCode,
                    roleName = r.RoleName,
                    permissions = r.PermissionsJson
                }).ToList();

            return Json(roles);
        }
        [HttpGet("admins/detail/{id}")]
        public IActionResult GetAdminDetail(int id)
        {
            var admin = _context.Admins
                .Where(a => a.AdminId == id && a.DeletedAt == null)
                .Select(a => new
                {
                    a.AdminId,
                    a.Account,
                    a.Name,
                    a.RoleCode
                })
                .FirstOrDefault();

            if (admin == null) return NotFound("找不到此員工");
            return Json(admin);
        }
        [HttpPut("admins/update")]
        public IActionResult UpdateAdmin([FromBody] UpdateAdminRequest request)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.AdminId == request.AdminId && a.DeletedAt == null);
            if (admin == null) return NotFound("找不到此員工");

            admin.Name = request.Name;
            admin.RoleCode = request.RoleCode;
            admin.UpdatedAt = DateTime.Now;

            // ✅ 若有填密碼則更新
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var salt = Guid.NewGuid().ToString("N").Substring(0, 8);
                var hash = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.Create()
                        .ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password + salt))
                );

                admin.PasswordHash = hash;
                admin.PasswordSalt = salt;
                admin.PasswordUpdatedAt = DateTime.Now;
            }

            _context.SaveChanges();
            return Ok(new { success = true, message = "員工資料已更新" });
        }

        public class UpdateAdminRequest
        {
            public int AdminId { get; set; }
            public string Name { get; set; }
            public string RoleCode { get; set; }
            public string? Password { get; set; } // 可選填
        }
        [HttpPost("admins/delete")]
        public IActionResult DeleteAdmin([FromBody] int adminId)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.AdminId == adminId && a.DeletedAt == null);
            if (admin == null)
                return NotFound("找不到指定的用戶");

            admin.DeletedAt = DateTime.Now;
            admin.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            return Ok("刪除成功");
        }
        #endregion

        #region 訊息模板管理 API

        /// <summary>
        /// 取得訊息模板列表（支援搜尋、篩選、分頁）
        /// </summary>
        [HttpGet("GetMessageTemplates")]
        public IActionResult GetMessageTemplates(string? category = null, string? keyword = null, bool? status = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.AdminMessageTemplates.AsQueryable();

                // 分類篩選
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(t => t.CategoryCode == category.ToUpper());
                }

                // 關鍵字搜尋（標題或內容）
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(t => t.Title.Contains(keyword) || t.TemplateContent.Contains(keyword));
                }

                // 狀態篩選
                if (status.HasValue)
                {
                    query = query.Where(t => t.IsActive == status.Value);
                }

                // 計算總數
                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // 分頁與排序
                var templates = query
                    .OrderByDescending(t => t.UpdatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        templateID = t.TemplateId,
                        categoryCode = t.CategoryCode,
                        title = t.Title,
                        templateContent = t.TemplateContent,
                        isActive = t.IsActive,
                        createdAt = t.CreatedAt,
                        updatedAt = t.UpdatedAt
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = templates,
                    page = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "載入模板列表時發生錯誤：" + ex.Message });
            }
        }

        /// <summary>
        /// 根據ID取得單一訊息模板
        /// </summary>
        [HttpGet("GetMessageTemplateById/{id}")]
        public IActionResult GetMessageTemplateById(int id)
        {
            try
            {
                var template = _context.AdminMessageTemplates
                    .Where(t => t.TemplateId == id)
                    .Select(t => new
                    {
                        templateID = t.TemplateId,
                        categoryCode = t.CategoryCode,
                        title = t.Title,
                        templateContent = t.TemplateContent,
                        isActive = t.IsActive,
                        createdAt = t.CreatedAt,
                        updatedAt = t.UpdatedAt
                    })
                    .FirstOrDefault();

                if (template == null)
                {
                    return NotFound(new { success = false, message = "找不到指定的模板" });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "載入模板時發生錯誤：" + ex.Message });
            }
        }

        /// <summary>
        /// 新增訊息模板
        /// </summary>
        [HttpPost("CreateMessageTemplate")]
        public IActionResult CreateMessageTemplate([FromBody] MessageTemplateCreateDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Title) || string.IsNullOrEmpty(dto.TemplateContent) || string.IsNullOrEmpty(dto.CategoryCode))
                {
                    return BadRequest(new { success = false, message = "標題、內容和分類為必填欄位" });
                }

                var template = new AdminMessageTemplate
                {
                    CategoryCode = dto.CategoryCode.ToUpper(),
                    Title = dto.Title,
                    TemplateContent = dto.TemplateContent,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.AdminMessageTemplates.Add(template);
                _context.SaveChanges();

                return Ok(new { success = true, message = "模板新增成功", templateId = template.TemplateId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "新增模板時發生錯誤：" + ex.Message });
            }
        }

        /// <summary>
        /// 更新訊息模板
        /// </summary>
        [HttpPost("UpdateMessageTemplate")]
        public IActionResult UpdateMessageTemplate([FromBody] MessageTemplateUpdateDto dto)
        {
            try
            {
                var template = _context.AdminMessageTemplates.FirstOrDefault(t => t.TemplateId == dto.TemplateID);
                if (template == null)
                {
                    return NotFound(new { success = false, message = "找不到指定的模板" });
                }

                if (string.IsNullOrEmpty(dto.Title) || string.IsNullOrEmpty(dto.TemplateContent) || string.IsNullOrEmpty(dto.CategoryCode))
                {
                    return BadRequest(new { success = false, message = "標題、內容和分類為必填欄位" });
                }

                template.CategoryCode = dto.CategoryCode.ToUpper();
                template.Title = dto.Title;
                template.TemplateContent = dto.TemplateContent;
                template.IsActive = dto.IsActive;
                template.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Ok(new { success = true, message = "模板更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "更新模板時發生錯誤：" + ex.Message });
            }
        }

        /// <summary>
        /// 刪除訊息模板
        /// </summary>
        [HttpPost("DeleteMessageTemplate")]
        public IActionResult DeleteMessageTemplate([FromBody] int id)
        {
            try
            {
                var template = _context.AdminMessageTemplates.FirstOrDefault(t => t.TemplateId == id);
                if (template == null)
                {
                    return NotFound(new { success = false, message = "找不到指定的模板" });
                }

                _context.AdminMessageTemplates.Remove(template);
                _context.SaveChanges();

                return Ok(new { success = true, message = "模板刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "刪除模板時發生錯誤：" + ex.Message });
            }
        }

        /// <summary>
        /// 切換訊息模板啟用狀態
        /// </summary>
        [HttpPost("ToggleMessageTemplateStatus")]
        public IActionResult ToggleMessageTemplateStatus([FromBody] int id)
        {
            try
            {
                var template = _context.AdminMessageTemplates.FirstOrDefault(t => t.TemplateId == id);
                if (template == null)
                {
                    return NotFound(new { success = false, message = "找不到指定的模板" });
                }

                template.IsActive = !template.IsActive;
                template.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new { success = true, isActive = template.IsActive, message = $"模板已{(template.IsActive ? "啟用" : "停用")}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "切換模板狀態時發生錯誤：" + ex.Message });
            }
        }

        #endregion

        #region ViewModels/DTOs

        public class MessageTemplateCreateDto
        {
            public string CategoryCode { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string TemplateContent { get; set; } = string.Empty;
            public bool IsActive { get; set; } = true;
        }

        public class MessageTemplateUpdateDto
        {
            public int TemplateID { get; set; }
            public string CategoryCode { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string TemplateContent { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }

        /// <summary>
        /// 取得平台公告專用的模板選項 (用於公告新增功能)
        /// </summary>
        [HttpGet("GetPlatformAnnounceTemplates")]
        public IActionResult GetPlatformAnnounceTemplates()
        {
            try
            {
                var templates = _context.AdminMessageTemplates
                    .Where(t => t.CategoryCode == "PLATFORM_ANNOUNCE" && t.IsActive == true)
                    .OrderBy(t => t.Title)
                    .Select(t => new
                    {
                        templateId = t.TemplateId,
                        title = t.Title,
                        templateContent = t.TemplateContent
                    })
                    .ToList();

                return Ok(new { success = true, data = templates });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "載入平台公告模板時發生錯誤：" + ex.Message });
            }
        }

        #endregion

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] string range = "week")
        {
            DateTime today = DateTime.Today;
            DateTime start, end;

            if (int.TryParse(range, out int month)) // 1~12 指定月
            {
                start = new DateTime(today.Year, month, 1);
                end = start.AddMonths(1);
            }
            else if (range == "month") // 本月
            {
                start = new DateTime(today.Year, today.Month, 1);
                end = start.AddMonths(1);
            }
            else // 預設為本週
            {
                start = today.AddDays(-(int)today.DayOfWeek + 1);
                end = start.AddDays(7);
            }

            // 今日統計
            int todayRegister = await _context.Members.CountAsync(m => m.CreatedAt.Date == today);
            int todayProperty = await _context.Properties.CountAsync(p => p.PublishedAt.HasValue && p.PublishedAt.Value.Date == today && p.DeletedAt == null);
            int todayFurniture = await _context.FurnitureOrders.CountAsync(f => f.CreatedAt.Date == today && f.DeletedAt == null);

            // 日期文字串（"MM/dd"）
            var labelDates = Enumerable.Range(0, (end - start).Days)
                .Select(i => start.AddDays(i).Date)
                .ToList();

            // 註冊量
            var dataRegister = await _context.Members
                .Where(m => m.CreatedAt >= start && m.CreatedAt < end)
                .GroupBy(m => m.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // 上架房源
            var dataProperty = await _context.Properties
                .Where(p => p.PublishedAt >= start && p.PublishedAt < end && p.DeletedAt == null)
                .GroupBy(p => p.PublishedAt.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // 家具訂單數
            var dataFurniture = await _context.FurnitureOrders
                .Where(f => f.CreatedAt >= start && f.CreatedAt < end && f.DeletedAt == null)
                .GroupBy(f => f.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // 家具收入
            var dataRevenue = await _context.FurnitureOrders
                .Where(f => f.CreatedAt >= start && f.CreatedAt < end && f.DeletedAt == null)
                .GroupBy(f => f.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Sum = g.Sum(x => x.TotalAmount) })
                .ToListAsync();

            // 上架服務費
            var dataFee = await _context.Properties
                .Where(p => p.PublishedAt >= start && p.PublishedAt < end && p.DeletedAt == null && p.ListingFeeAmount.HasValue)
                .GroupBy(p => p.PublishedAt.Value.Date)
                .Select(g => new { Date = g.Key, Sum = g.Sum(x => x.ListingFeeAmount!.Value) })
                .ToListAsync();

            // labels
            var labels = labelDates.Select(d => d.ToString("MM/dd")).ToList();

            return Json(new
            {
                today = new
                {
                    register = todayRegister,
                    property = todayProperty,
                    furniture = todayFurniture
                },
                weekly = new
                {
                    labels = labels,
                    register = labelDates.Select(d => dataRegister.FirstOrDefault(x => x.Date == d)?.Count ?? 0).ToList(),
                    property = labelDates.Select(d => dataProperty.FirstOrDefault(x => x.Date == d)?.Count ?? 0).ToList(),
                    furniture = labelDates.Select(d => dataFurniture.FirstOrDefault(x => x.Date == d)?.Count ?? 0).ToList(),
                    furnitureRevenue = labelDates.Select(d => dataRevenue.FirstOrDefault(x => x.Date == d)?.Sum ?? 0).ToList(),
                    listingFee = labelDates.Select(d => dataFee.FirstOrDefault(x => x.Date == d)?.Sum ?? 0).ToList()
                }
            });
        }

        [HttpGet("hot-rankings")]
        public async Task<IActionResult> GetHotRankings()
        {
            // 熱門家具承租排行榜（依歷史訂單統計）
            var hotFurniture = await _context.FurnitureOrderHistories
                .Where(h => h.Product.DeletedAt == null)
                .GroupBy(h => new { h.ProductId, h.Product.ProductName })
                .Select(g => new
                {
                    Name = g.Key.ProductName,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            // 熱門租屋地區排行榜（依有效合約 + 房源城市）
            var hotRentalCities = await (
                from c in _context.Contracts
                where c.Status == "SIGNED" && c.RentalApplication != null
                let property = c.RentalApplication.Property
                where property != null
                join d in _context.Districts on property.DistrictId equals d.DistrictId
                join city in _context.Cities on d.CityId equals city.CityId
                group c by city.CityName into g
                orderby g.Count() descending
                select new
                {
                    Name = g.Key,
                    Count = g.Count()
                }
            ).Take(5).ToListAsync();
            // 驗證房源數量排行榜（依有效狀態的房源）
            // 統一視為「已審核房源」的 statusCode 範圍
            string[] verifiedStatusCodes = new[]
            {
                "PENDING_PAYMENT", "LISTED"
            };

            var verifiedProperties = await (
                from p in _context.Properties
                join d in _context.Districts on p.DistrictId equals d.DistrictId
                join city in _context.Cities on d.CityId equals city.CityId
                where verifiedStatusCodes.Contains(p.StatusCode)
                group p by city.CityName into g
                orderby g.Count() descending
                select new
                {
                    Name = g.Key,
                    Count = g.Count()
                }
            ).Take(5).ToListAsync();



            // 待審核房源數量排行榜（依待審核狀態的房源）
            var pendingProperties = await (
                    from p in _context.Properties
                    join d in _context.Districts on p.DistrictId equals d.DistrictId
                    join city in _context.Cities on d.CityId equals city.CityId
                    where p.StatusCode == "PENDING"
                    group p by city.CityName into g
                    orderby g.Count() descending
                    select new
                    {
                        Name = g.Key,
                        Count = g.Count()
                    }
                ).Take(5).ToListAsync();


            return Json(new
            {
                furniture = hotFurniture,
                rental = hotRentalCities,
                verified = verifiedProperties,
                pending = pendingProperties
            });
        }
        [HttpGet("dashboard/statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var today = DateTime.Today;

            // 近五日每日 DAU
            var last5Days = Enumerable.Range(0, 5)
                .Select(i => today.AddDays(-4 + i)) // 從五天前開始
                .ToList();

            var dauRaw = await _context.Members
                .Where(m => m.LastLoginAt != null && m.LastLoginAt >= last5Days.First())
                .GroupBy(m => m.LastLoginAt!.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var dau = last5Days
                .Select(d => new
                {
                    Date = d,
                    Count = dauRaw.FirstOrDefault(x => x.Date == d)?.Count ?? 0
                })
                .ToList();

            // 本月每日 DAU
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var daysInMonth = Enumerable.Range(0, (today - startOfMonth).Days + 1)
                .Select(i => startOfMonth.AddDays(i))
                .ToList();

            var monthRaw = await _context.Members
                .Where(m => m.LastLoginAt != null && m.LastLoginAt >= startOfMonth)
                .GroupBy(m => m.LastLoginAt!.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var month = daysInMonth
                .Select(d => new
                {
                    Date = d,
                    Count = monthRaw.FirstOrDefault(x => x.Date == d)?.Count ?? 0
                })
                .ToList();

            // 今年每月 DAU
            var startOfYear = new DateTime(today.Year, 1, 1);
            var months = Enumerable.Range(1, 12).Select(m => new DateTime(today.Year, m, 1)).ToList();

            var yearRaw = await _context.Members
                .Where(m => m.LastLoginAt != null && m.LastLoginAt >= startOfYear)
                .GroupBy(m => new { m.LastLoginAt!.Value.Year, m.LastLoginAt!.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var year = months
                .Select(d => new
                {
                    Month = d.ToString("yyyy-MM"),
                    Count = yearRaw.FirstOrDefault(x => x.Month == d.Month && x.Year == d.Year)?.Count ?? 0
                })
                .ToList();
            var applicationRaw = await _context.RentalApplications
            .Where(a => a.CreatedAt >= last5Days.First() && a.IsActive == true)
            .GroupBy(a => new { a.CreatedAt.Date, a.ApplicationType })
            .Select(g => new
            {
                Date = g.Key.Date,
                Type = g.Key.ApplicationType, // "RENTAL" / "HOUSE_VIEWING"
                Count = g.Count()
            })
            .ToListAsync();

            var applicationStats = last5Days.Select(d => new
             {
               Date = d,
               RentalCount = applicationRaw.FirstOrDefault(x => x.Date == d && x.Type == "RENTAL")?.Count ?? 0,
               ViewingCount = applicationRaw.FirstOrDefault(x => x.Date == d && x.Type == "HOUSE_VIEWING")?.Count ?? 0
             }).ToList();

            var applicationYearRaw = await _context.RentalApplications
                .Where(a => a.CreatedAt >= startOfYear && a.IsActive == true)
                .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month, a.ApplicationType })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Type = g.Key.ApplicationType,
                    Count = g.Count()
                })
                .ToListAsync();
            //今年每月申請趨勢
            var applicationYear = months.Select(d => new
            {
                Month = d.ToString("yyyy-MM"),
                RentalCount = applicationYearRaw.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month && x.Type == "RENTAL")?.Count ?? 0,
                ViewingCount = applicationYearRaw.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month && x.Type == "HOUSE_VIEWING")?.Count ?? 0
            }).ToList();
            //今月每日申請趨勢
            var applicationMonthRaw = await _context.RentalApplications
                .Where(a => a.CreatedAt >= startOfMonth && a.IsActive == true)
                .GroupBy(a => new { a.CreatedAt.Date, a.ApplicationType })
                .Select(g => new
                {
                    Date = g.Key.Date,
                    Type = g.Key.ApplicationType,
                    Count = g.Count()
                })
                .ToListAsync();

            var applicationMonth = daysInMonth.Select(d => new
            {
                Date = d,
                RentalCount = applicationMonthRaw.FirstOrDefault(x => x.Date == d && x.Type == "RENTAL")?.Count ?? 0,
                ViewingCount = applicationMonthRaw.FirstOrDefault(x => x.Date == d && x.Type == "HOUSE_VIEWING")?.Count ?? 0
            }).ToList();


            return Json(new
            {
                dau = dau,
                month = month,
                year = year,
                application = applicationStats,
                applicationMonth = applicationMonth,
                applicationYear = applicationYear
            });
        }

        [HttpGet("listing-fee-stats")]
        public IActionResult GetListingFeeStats(int? year = null, int? month = null)
        {
            var now = DateTime.Now;
            int y = year ?? now.Year;
            int m = month ?? now.Month;
            var startOfMonth = new DateTime(y, m, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            var properties = _context.Properties
                .Where(p => p.ListingFeeAmount != null && p.DeletedAt == null &&
                            p.PublishedAt.HasValue &&
                            p.PublishedAt.Value >= startOfMonth &&
                            p.PublishedAt.Value < endOfMonth);
            // 總統計
            var totalDue = properties.Sum(p => p.ListingFeeAmount ?? 0);
            var totalPaid = properties.Where(p => p.IsPaid).Sum(p => p.ListingFeeAmount ?? 0);
            var totalUnpaid = totalDue - totalPaid;
            // 每日統計
            var trend = Enumerable.Range(0, (endOfMonth - startOfMonth).Days).Select(offset => {
                var date = startOfMonth.AddDays(offset);
                return new {
                    date = date.ToString("MM/dd"),
                    due = properties.Where(p => p.PublishedAt.HasValue && p.PublishedAt.Value.Date == date.Date).Sum(p => p.ListingFeeAmount ?? 0),
                    paid = properties.Where(p => p.PaidAt.HasValue && p.PaidAt.Value.Date == date.Date).Sum(p => p.ListingFeeAmount ?? 0)
                };
            });
            return Json(new {
                totalDue,
                totalPaid,
                totalUnpaid,
                trend
            });
        }
        [HttpGet("monthly-trend")]
        public async Task<IActionResult> GetMonthlyListingFeeTrend(int? year = null, int? month = null)
        {
            var today = DateTime.Today;
            int y = year ?? today.Year;
            int m = month ?? today.Month;
            var startOfMonth = new DateTime(y, m, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            // 撈資料
            var rawData = await _context.Properties
                .Where(p => p.PublishedAt != null &&
                            p.PublishedAt >= startOfMonth &&
                            p.PublishedAt < endOfMonth &&
                            p.ListingFeeAmount != null)
                .GroupBy(p => p.PublishedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalDue = g.Sum(p => p.ListingFeeAmount ?? 0),
                    TotalPaid = g.Where(p => p.IsPaid).Sum(p => p.ListingFeeAmount ?? 0)
                })
                .ToListAsync();
            // 建立本月完整日期
            var allDates = Enumerable.Range(0, (endOfMonth - startOfMonth).Days)
                .Select(offset => startOfMonth.AddDays(offset).Date)
                .ToList();
            // 合併結果
            var listingData = allDates.Select(date =>
            {
                var match = rawData.FirstOrDefault(d => d.Date == date);
                return new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    totalDue = match?.TotalDue ?? 0,
                    totalPaid = match?.TotalPaid ?? 0
                };
            });
            return Json(listingData);
        }
        [HttpGet("order-stats")]
        public async Task<IActionResult> GetOrderStats(int? year = null, int? month = null)
        {
            var now = DateTime.Now;
            int y = year ?? now.Year;
            int m = month ?? now.Month;
            var startOfMonth = new DateTime(y, m, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            int completedCount = await _context.Contracts
                .CountAsync(c => c.Status == "SIGNED" && c.CreatedAt >= startOfMonth && c.CreatedAt < endOfMonth);

            int unpaidCount = await _context.Properties
                        .CountAsync(c =>
                            c.StatusCode == "PENDING_PAYMENT" &&
                            c.DeletedAt == null &&
                            c.CreatedAt >= startOfMonth &&
                            c.CreatedAt < endOfMonth
                        );

            var complaints = await _context.PropertyComplaints
                .Where(c => c.CreatedAt >= startOfMonth && c.CreatedAt < endOfMonth)
                .GroupBy(c => c.StatusCode)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int resolved = complaints.FirstOrDefault(c => c.Status == "RESOLVED")?.Count ?? 0;
            int progress = complaints.FirstOrDefault(c => c.Status == "PROGRESS")?.Count ?? 0;
            int pending = complaints.FirstOrDefault(c => c.Status == "PENDING")?.Count ?? 0;

            return Json(new[]
            {
                new { type = "✅ 租屋成交單數", count = completedCount, note = "已完成付款並確認租約" },
                new { type = "💳 已驗證未付款單數", count = unpaidCount, note = "尚未完成金流，等待付款" },
                new { type = "🟢 已處理檢舉", count = resolved, note = "檢舉已完成並結案" },
                new { type = "🟡 處理中檢舉", count = progress, note = "檢舉正在處理中" },
                new { type = "🔴 待處理檢舉", count = pending, note = "尚未處理的檢舉案件" }
            });
        }

        [HttpGet("property-status-stats")]
        public async Task<IActionResult> GetPropertyStatusStats(int? year = null, int? month = null)
        {
            var now = DateTime.Now;
            int y = year ?? now.Year;
            int m = month ?? now.Month;
            var startOfMonth = new DateTime(y, m, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            // 對照表：statusCode => (label, note)
            var statusMap = new Dictionary<string, (string label, string note)> {
        { "PENDING", ("審核中", "房源建立事件") },
        { "PENDING_PAYMENT", ("待付款", "房源審核通過事件") },
        { "REJECT_REVISE", ("審核未通過(待補件)", "房源審核須補件事件") },
        { "REJECTED", ("審核未通過", "房源審核不通過事件") },
        { "LISTED", ("上架中", "刊登費繳清並上架事件") },
        { "CONTRACT_ISSUED", ("已發出合約", "送出合約簽署事件") },
        { "PENDING_RENEWAL", ("待續約", "續約事件(租約到期前一個月)") },
        { "LEASE_EXPIRED_RENEWING", ("續約(房客申請中)", "重新送出合約事件") },
        { "IDLE", ("閒置中", "重新刊登事件") },
        { "ALREADY_RENTED", ("出租中", "房源成功出租事件") },
        { "INVALID", ("房源已下架", "無效房源事件") }
    };

            // 篩選資料
            var data = await _context.Properties
                .Where(p => p.CreatedAt >= startOfMonth && p.CreatedAt < endOfMonth)
                .GroupBy(p => p.StatusCode)
                .Select(g => new { StatusCode = g.Key, Count = g.Count() })
                .ToListAsync();

            // 整理成完整表格（包含未出現的也顯示）
            var result = statusMap.Select(kvp =>
            {
                var match = data.FirstOrDefault(d => d.StatusCode == kvp.Key);
                return new
                {
                    type = kvp.Value.label,
                    count = match?.Count ?? 0,
                    note = kvp.Value.note
                };
            }).ToList();

            return Json(result);
        }



        [HttpGet("server-status")]
        public IActionResult GetServerStatus()
        {
            double cpuUsage = 0;
            double ramUsageMB = 0;
            double diskFreeGB = 0;
            double netUploadMbps = 0;
            double netDownloadMbps = 0;

            // 取得 CPU 使用率（僅 Windows 有效，Linux 會回傳 -1）
            try
            {
                using (var cpuCounter = new System.Diagnostics.PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName))
                {
                    cpuCounter.NextValue();
                    System.Threading.Thread.Sleep(500);
                    cpuUsage = Math.Round(cpuCounter.NextValue() / Environment.ProcessorCount, 1);
                }
            }
            catch
            {
                cpuUsage = -1;
            }

            // 取得應用程式記憶體用量（MB）
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                ramUsageMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 1);
            }
            catch
            {
                ramUsageMB = -1;
            }

            // 磁碟剩餘空間
            try
            {
                var drive = System.IO.DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name == AppDomain.CurrentDomain.BaseDirectory.Substring(0, 3));
                if (drive != null)
                    diskFreeGB = Math.Round(drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0, 2);
            }
            catch { diskFreeGB = -1; }

            // 網路流量（全系統）
            try
            {
                var category = "Network Interface";
                var uploadCounter = new System.Diagnostics.PerformanceCounter(category, "Bytes Sent/sec");
                var downloadCounter = new System.Diagnostics.PerformanceCounter(category, "Bytes Received/sec");
                // 取第一個網卡
                var instanceNames = System.Diagnostics.PerformanceCounterCategory.GetCategories()
                    .FirstOrDefault(c => c.CategoryName == category)?.GetInstanceNames();
                if (instanceNames != null && instanceNames.Length > 0)
                {
                    uploadCounter.InstanceName = instanceNames[0];
                    downloadCounter.InstanceName = instanceNames[0];
                    uploadCounter.NextValue();
                    downloadCounter.NextValue();
                    System.Threading.Thread.Sleep(500);
                    netUploadMbps = Math.Round(uploadCounter.NextValue() * 8 / 1024 / 1024, 2); // 轉 Mbps
                    netDownloadMbps = Math.Round(downloadCounter.NextValue() * 8 / 1024 / 1024, 2);
                }
            }
            catch { netUploadMbps = -1; netDownloadMbps = -1; }

            return Json(new
            {
                cpu = cpuUsage,
                ram = ramUsageMB,
                disk = diskFreeGB,
                netUpload = netUploadMbps,
                netDownload = netDownloadMbps
            });
        }

    }
}
