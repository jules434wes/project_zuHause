using Microsoft.AspNetCore.Mvc;
using zuHause.Data;

namespace zuHause.Controllers
{
    /// <summary>
    /// 資料播種控制器 - 提供手動觸發資料播種的 API
    /// 僅在開發環境使用
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataSeederController : ControllerBase
    {
        private readonly RealDataSeeder _seeder;
        private readonly ILogger<DataSeederController> _logger;
        private readonly IWebHostEnvironment _environment;

        public DataSeederController(
            RealDataSeeder seeder, 
            ILogger<DataSeederController> logger,
            IWebHostEnvironment environment)
        {
            _seeder = seeder;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// 手動重置測試資料
        /// </summary>
        [HttpPost("reset")]
        public async Task<IActionResult> ResetTestData()
        {
            // 僅在開發環境允許執行
            if (!_environment.IsDevelopment())
            {
                return BadRequest("此功能僅在開發環境可用");
            }

            try
            {
                await _seeder.ResetTestDataAsync();
                _logger.LogInformation("手動重置測試資料成功");
                
                return Ok(new { 
                    success = true, 
                    message = "測試資料重置成功",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手動重置測試資料失敗");
                
                return StatusCode(500, new { 
                    success = false, 
                    message = "測試資料重置失敗", 
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 檢查資料播種器狀態
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            if (!_environment.IsDevelopment())
            {
                return BadRequest("此功能僅在開發環境可用");
            }

            return Ok(new {
                environment = _environment.EnvironmentName,
                isAvailable = true,
                timestamp = DateTime.Now,
                message = "RealDataSeeder 已準備就緒"
            });
        }
    }
}