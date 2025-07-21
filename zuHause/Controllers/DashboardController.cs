using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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


        [HttpGet("")]
        public IActionResult Index()
        {
            // 從 Cookie Claims 獲取管理員資訊
            ViewBag.EmployeeID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.Role = HttpContext.User.FindFirst("RoleName")?.Value;

            var permissionsJSON = HttpContext.User.FindFirst("PermissionsJSON")?.Value ?? "{}";
            var permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(permissionsJSON);

            var allKeys = new List<string>
    {
        "overview", "monitor", "behavior", "orders", "system",
        "roles", "Backend_user_list", "contract_template",
        "platform_fee", "imgup", "furniture_fee", "Marquee_edit", "furniture_management"
    };

            // 根據是否為 all 權限決定要塞什麼資料格式
            if (permissions.TryGetValue("all", out bool isAll) && isAll)
            {
                ViewBag.RoleAccess = new Dictionary<string, object>
                {
                    [ViewBag.Role] = new { all = true }
                };
            }
            else
            {
                var grantedKeys = permissions
                    .Where(p => p.Value && allKeys.Contains(p.Key))
                    .Select(p => p.Key)
                    .ToList();

                ViewBag.RoleAccess = new Dictionary<string, object>
                {
                    [ViewBag.Role] = grantedKeys
                };
            }

            return View();
        }



        //家具管理分頁
        [HttpGet("{id}")]
        public IActionResult LoadTab(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
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
        // 房源方案相關 API
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
        // 合約範本相關 API

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
        //輪播圖相關 API
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
                .Select(p => new {
                    p.PageCode,
                    p.PageName
                })
                .ToList();

            return Json(categories);
        }
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
                .Select(m => new {
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







    }


}
