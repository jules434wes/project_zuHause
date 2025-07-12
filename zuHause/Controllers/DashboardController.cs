using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Data;
using zuHause.Models;
using zuHause.ViewModels;

namespace zuHause.Controllers
{

    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly ZuhauseDbContext _context;
        public DashboardController(ZuhauseDbContext context)
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
                           join i in _context.furnitureInventories on p.FurnitureProductId equals i.productId
                           join c in _context.FurnitureCategories on p.CategoryId equals c.FurnitureCategoriesId into pc
                           from c in pc.DefaultIfEmpty()
                           where p.DeletedAt == null
                           select new FurnitureCardViewModel
                           {
                               FurnitureID = p.FurnitureProductId,
                               Name = p.ProductName,
                               Status = p.Status == true ? "上架" : "下架",
                               Stock = i.availableQuantity,
                               RentedCount = i.rentedQuantity,
                               Type = c != null ? c.Name : "(未分類)",
                               Description = p.Description,
                               OriginalPrice = p.ListPrice,
                               RentPerDay = p.DailyRental,
                               ListDate = p.ListedAt ?? DateTime.MinValue,
                               DelistDate = p.DelistedAt ?? DateTime.MaxValue,
                               CreatedAt = p.CreatedAt,
                               UpdatedAt = p.UpdatedAt,
                               DeletedAt = p.DeletedAt

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

        [HttpPost]
        public IActionResult SoftDeleteFurniture(string id)
        {
            var item = _context.FurnitureProducts.FirstOrDefault(f => f.FurnitureProductId == id);
            if (item == null) return NotFound("找不到家具");

            item.DeletedAt = DateTime.Now;
            _context.SaveChanges();
            return Content("✅ 已軟刪除");
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
            product.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            return Content("✅ 已提前下架");
        }

        [HttpPost("UploadFurniture")]
        public IActionResult UploadFurniture([FromBody] FurnitureUploadViewModel vm)
        {
            if (vm == null || string.IsNullOrWhiteSpace(vm.Name))
                return BadRequest("家具資料不完整");

            var newId = "FP" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);

            if (string.IsNullOrWhiteSpace(vm.Type))
                return BadRequest("❌ 請選擇一個家具分類");

            var categoryId = vm.Type.Trim();

            if (!_context.FurnitureCategories.Any(c => c.FurnitureCategoriesId == categoryId))
                return BadRequest("❌ 所選分類不存在，請重新選擇");


            var product = new FurnitureProduct
            {
                FurnitureProductId = newId,
                ProductName = vm.Name,
                Description = vm.Description,
                CategoryId = categoryId,
                ListPrice = vm.OriginalPrice,
                DailyRental = vm.RentPerDay,
                Status = vm.Status,
                ListedAt = vm.StartDate ?? DateTime.Now,
                DelistedAt = vm.EndDate ?? DateTime.Now.AddMonths(1),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.FurnitureProducts.Add(product);
            _context.SaveChanges(); // ← 保證 FK 可用

            var inventory = new furnitureInventory
            {
                furnitureInventoryId = Guid.NewGuid().ToString(),
                productId = newId,
                totalQuantity = vm.Stock,
                availableQuantity = vm.Stock,
                rentedQuantity = 0
            };

            _context.furnitureInventories.Add(inventory);
            _context.SaveChanges();

            return Ok("✅ 家具已成功上傳！");
        }









    }


}
