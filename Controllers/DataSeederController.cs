using Microsoft.AspNetCore.Mvc;
using zuHause.Data;

namespace zuHause.Controllers
{
    [ApiController]
    [Route("api/seed")]
    public class DataSeederController : ControllerBase
    {
        private readonly ZuHauseContext _context;
        private readonly RealDataSeeder _seeder;

        public DataSeederController(ZuHauseContext context)
        {
            _context = context;
            _seeder = new RealDataSeeder(context);
        }

        [HttpPost("all")]
        public async Task<IActionResult> SeedAllData()
        {
            try
            {
                await _seeder.SeedAsync();
                return Ok(new { message = "所有測試資料已成功建立", timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "建立測試資料時發生錯誤", error = ex.Message });
            }
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetData()
        {
            try
            {
                await _seeder.ResetTestDataAsync();
                return Ok(new { message = "測試資料已重置", timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "重置測試資料時發生錯誤", error = ex.Message });
            }
        }

        [HttpPost("landlord-with-property")]
        public async Task<IActionResult> CreateTestLandlordWithProperty()
        {
            try
            {
                var landlord = await _seeder.CreateTestLandlordWithPropertyAsync();
                return Ok(new 
                { 
                    message = "測試房東和房源已建立", 
                    landlordId = landlord.MemberID,
                    landlordName = landlord.MemberName,
                    timestamp = DateTime.Now 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "建立測試房東時發生錯誤", error = ex.Message });
            }
        }

        [HttpPost("tenant")]
        public async Task<IActionResult> CreateTestTenant()
        {
            try
            {
                var tenant = await _seeder.CreateTestTenantAsync();
                return Ok(new 
                { 
                    message = "測試租客已建立", 
                    tenantId = tenant.MemberID,
                    tenantName = tenant.MemberName,
                    timestamp = DateTime.Now 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "建立測試租客時發生錯誤", error = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetDataStatus()
        {
            try
            {
                var memberCount = _context.Members.Count();
                var propertyCount = _context.Properties.Count();
                var applicationCount = _context.RentalApplications.Count();
                var imageCount = _context.PropertyImages.Count();

                return Ok(new
                {
                    members = memberCount,
                    properties = propertyCount,
                    applications = applicationCount,
                    images = imageCount,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得資料狀態時發生錯誤", error = ex.Message });
            }
        }
    }
}