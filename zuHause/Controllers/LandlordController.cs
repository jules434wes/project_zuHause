using Microsoft.AspNetCore.Mvc;

namespace zuHause.Controllers
{
    public class LandlordController : Controller
    {
        private readonly ILogger<LandlordController> _logger;
        public LandlordController(ILogger<LandlordController> logger)
        {
            _logger = logger;
        }

        // GET: /landlord/become
        [HttpGet("landlord/become")]
        public IActionResult Become()
        {
            return View();
        }
    }
} 