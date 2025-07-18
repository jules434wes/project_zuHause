using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using zuHause.Models; // 確保有這個 using
using System; // For Exception

namespace zuHause.Controllers
{
    // 建議這裡也加上 [ApiController] 屬性
    [ApiController]
    // 移除 [Route("api/[controller]")]，因為您的方法已經使用了明確的絕對路徑
    public class CommonController : ControllerBase // 繼承 ControllerBase 而非 Controller
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
                return StatusCode(500, "Internal server error: Failed to retrieve city list.");
            }
        }

        /// <summary>
        /// 根據城市代碼獲取所有啟用的行政區列表。
        /// 路徑: /api/Common/District/list/{cityCode}
        /// 返回 DistrictCode 和 DistrictName。
        /// </summary>
        /// <param name="cityCode">城市的 CityCode，例如 "TXG"。</param>
        /// <returns>行政區列表，JSON 格式，包含 DistrictCode 和 DistrictName。</returns>
        [HttpGet("api/Common/District/list/{cityCode}")]
        public async Task<IActionResult> GetDistrictsByCityCode(string cityCode)
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

                if (cityId == 0)
                {
                    return NoContent(); // 204 No Content
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

                if (districts == null || !districts.Any())
                {
                    return NoContent(); // 204 No Content
                }

                return Ok(districts); // 200 OK
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting districts for {cityCode}: {ex.Message}");
                return StatusCode(500, $"Internal server error: Failed to retrieve district list for {cityCode}.");
            }
        }
    }
}