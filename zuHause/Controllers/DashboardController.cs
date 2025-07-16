using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.ViewModels;

namespace zuHause.Controllers
{

    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly ZuHauseContext _context;
        public DashboardController(ZuHauseContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Role = "超級管理員";
            ViewBag.EmployeeID = "9528";
            ViewBag.RoleAccess = new Dictionary<string, List<string>> {
                { "超級管理員", new List<string>{ "overview", "monitor", "behavior", "orders", "system", "roles", "Backend_user_list", "contract_template", "platform_fee", "imgup", "furniture_fee", "Marquee_edit", "furniture_management" } },
                { "管理員", new List<string>{ "overview", "behavior", "orders" } },
                { "房源審核員", new List<string>{ "monitor" } },
                { "客服", new List<string>{ "behavior", "orders" } }
                    };
            return View();


        }
        
        [HttpGet("{id}")]
        public IActionResult LoadTab(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

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
            product.Status=false; // 先將狀態設為下架,避免使用者誤租
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
                    p.StartAt,
                    p.EndAt,
                    p.IsActive,
                    p.DiscountTrigger,
                    p.DiscountUnit
                })
                .ToList();

            return Json(plans);
        }
        [HttpPost("CreateListingPlan")]
        public IActionResult CreateListingPlan([FromBody] ListingPlan model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PlanName))
                return BadRequest("❌ 資料不完整");

            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.IsActive = false;

            if (model.StartAt == default)
                model.StartAt = DateTime.Now;

            // ✅ 直接找出上一筆（依 planId 最大），強制結束時間設為新方案開始時間 -1 秒
            var lastPlan = _context.ListingPlans
                .OrderByDescending(p => p.PlanId)
                .FirstOrDefault();

            if (lastPlan != null && lastPlan.EndAt == null)
            {
                lastPlan.EndAt = model.StartAt.AddSeconds(-1);
            }

            _context.ListingPlans.Add(model);
            _context.SaveChanges();

            return Ok(new { success = true, planId = model.PlanId });
        }










    }


}
