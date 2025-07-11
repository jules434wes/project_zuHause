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
                               Stock = i.AvailableQuantity,
                               RentedCount = i.RentedQuantity,
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
        [HttpPost]
        public IActionResult SetOffline(string id)
        {
            var product = _context.FurnitureProducts.FirstOrDefault(p => p.FurnitureProductId == id && p.DeletedAt == null);
            if (product == null) return NotFound("找不到資料");

            product.Status = false;
            product.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
            return Content("✅ 已提前下架");
        }
        [HttpPost]
        public IActionResult UploadFurniture([FromBody] FurnitureUploadViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Name))
                return BadRequest("家具名稱不能為空");

            var newId = "FP" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);


            var product = new FurnitureProduct
            {
                FurnitureProductId = newId,
                ProductName = vm.Name,
                Description = vm.Description,
                CategoryId = string.IsNullOrWhiteSpace(vm.Type) ? "UNCATEGORIZED" : vm.Type,
                ListPrice = vm.OriginalPrice,
                DailyRental = vm.RentPerDay,
                Status = vm.Status,
                ListedAt = vm.StartDate ?? DateTime.UtcNow,
                DelistedAt = vm.EndDate ?? DateTime.UtcNow.AddMonths(1),
                CreatedAt = DateTime.UtcNow
            };

            var inventory = new FurnitureInventory
            {
                FurnitureInventoryId = Guid.NewGuid().ToString(),
                ProductId = newId,
                TotalQuantity = vm.Stock,
                AvailableQuantity = vm.Stock,
                RentedQuantity = 0
            };

            _context.FurnitureProducts.Add(product);
            _context.FurnitureInventories.Add(inventory);
            _context.SaveChanges();

            return Ok("✅ 家具已成功上傳！");
        }








    }


}
