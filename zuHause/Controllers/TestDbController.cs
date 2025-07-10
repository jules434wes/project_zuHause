using Microsoft.AspNetCore.Mvc;
using zuHause.Data;
using System.Linq;

namespace zuHause.Controllers
{
    public class TestDbController : Controller
    {
        private readonly ZuhauseDbContext _context;

        public TestDbController(ZuhauseDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var members = _context.Members.Take(5).ToList();
            return View(members); // 用 View 測試回傳內容
        }
    }
}
