using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using zuHause.Models;
using System;
using System.Collections.Generic; // Add for List<object> if you return empty list


namespace zuHause.Controllers
{
    [ApiController]
    // 移除 [Route("api/[controller]")] 是正確的，因為你使用了完整路由
    public class CommonController : ControllerBase
    {
        private readonly ZuHauseContext _context;

        public CommonController(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 獲取所有啟用的城市列表。
        /// 路徑: /api/Common/City/list
        /// 返回 CityCode 和 CityName。
        /// </summary>
        /// <returns>城市列表，JSON 格式，包含 CityCode 和 CityName。</returns>
        [HttpGet("api/Common/City/list")]
        public async Task<IActionResult> GetCities()
        {
            try
            {
                var cities = await _context.Cities
                                               .Where(c => c.IsActive)
                                               .OrderBy(c => c.DisplayOrder)
                                               .Select(c => new
                                               {
                                                   c.CityCode,
                                                   c.CityName
                                               })
                                               .ToListAsync();

                if (cities == null || !cities.Any())
                {
                    return NoContent(); // 204 No Content
                }

                return Ok(cities); // 200 OK
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cities: {ex.Message}");
                // 通常在生產環境中不直接返回內部錯誤訊息給客戶端
                return StatusCode(500, "Internal server error: Failed to retrieve city list.");
            }
        }

        /// <summary>
        /// 根據城市代碼獲取所有啟用的行政區列表。
        /// 路徑: /api/Common/District/listByCityCode?cityCode={cityCode}
        /// 返回 DistrictCode 和 DistrictName。
        /// </summary>
        /// <param name="cityCode">城市的 CityCode，例如 "TPE"。</param>
        /// <returns>行政區列表，JSON 格式，包含 DistrictCode 和 DistrictName。</returns>
        [HttpGet("api/Common/District/listByCityCode")] // <--- **修改這裡，移除 {cityCode}**
                                                        // 現在它只匹配 /api/Common/District/listByCityCode
        public async Task<IActionResult> GetDistrictsByCityCode([FromQuery] string cityCode) // <--- **建議明確使用 [FromQuery]**
        {
            if (string.IsNullOrWhiteSpace(cityCode))
            {
                return BadRequest("CityCode cannot be null or empty."); // 400 Bad Request
            }

            try
            {
                var cityId = await _context.Cities
                                               .Where(c => c.CityCode == cityCode && c.IsActive)
                                               .Select(c => c.CityId)
                                               .FirstOrDefaultAsync();

                if (cityId == 0) // 如果找不到城市或城市不活躍，則沒有行政區
                {
                    // 返回 200 OK 和空列表，因為找不到匹配的行政區不是伺服器錯誤
                    return Ok(new List<object>()); // 返回空列表，讓前端知道沒有數據
                }

                var districts = await _context.Districts
                                                  .Where(d => d.CityId == cityId && d.IsActive)
                                                  .OrderBy(d => d.DisplayOrder)
                                                  .Select(d => new
                                                  {
                                                      d.DistrictCode,
                                                      d.DistrictName
                                                  })
                                                  .ToListAsync();

                // 如果行政區列表為空，也返回 200 OK 和空列表
                if (districts == null || !districts.Any())
                {
                    return Ok(new List<object>());
                }

                return Ok(districts); // 200 OK
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting districts for {cityCode}: {ex.Message}");
                // 在生產環境中，考慮使用 ILogger 來記錄錯誤
                return StatusCode(500, $"Internal server error: Failed to retrieve district list for {cityCode}.");
            }
        }
    }
}