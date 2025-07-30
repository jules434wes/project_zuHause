using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // For ClaimTypes
using System.Threading.Tasks; // For async operations
using zuHause.Interfaces.TenantInterfaces;
// 假設這些是您的模型和服務
// using YourProject.Models;
// using YourProject.Services;

[ApiController]
[Route("api/Member")] // Controller 級別路由，可以簡化下面的 Route 屬性
public class MemberController : ControllerBase
{
    private readonly IDataAccessService _dataAccessService; // 假設您有一個資料存取服務

    public MemberController(IDataAccessService dataAccessService)
    {
        _dataAccessService = dataAccessService;
    }

    [HttpGet("UserCityCode")] // 方法級別路由，完整路徑為 api/Member/UserCityCode
    [Produces("application/json")]
    [Authorize] // 確保只有已驗證的使用者才能訪問此方法
    public async Task<IActionResult> GetUserCityCodeAndDistrictCodeApi() // 改名以反映返回更多數據
    {
        try
        {     
            
            // 1. 從已驗證的使用者主體中獲取會員 ID
            // 推薦使用 ClaimTypes.NameIdentifier，除非您確定您的JWT中Claim名稱就是"UserId"
            //var memberIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 如果您確定JWT中Claim名稱就是"UserId"，則使用：
             var memberIdString = User.FindFirst("UserId")?.Value;
            
            if (string.IsNullOrEmpty(memberIdString))
            {
                // 這通常不應該發生在 [Authorize] 的方法中，除非 JWT 配置有問題。
                return BadRequest(new { message = "Member ID claim not found in authentication token." });
            }

            if (!int.TryParse(memberIdString, out int memberId))
            {
                // 如果解析失敗，表示 Claim 中的 Member ID 不是預期的 int 類型
                return BadRequest(new { message = "Invalid member ID format in authentication token." });
            }

            // 2. 從 Members 表中抓取會員資料
            // 假設您的 DataAccessService 有一個 GetMemberByIdAsync 方法
            var member = await _dataAccessService.GetMemberByIdAsync(memberId);
            if (member == null)
            {
                return NotFound(new { message = "Member not found." });
            }

            string? cityCode = null;
            string? districtCode = null;

            // 3. 根據 primaryRentalCityID 抓取 cityCode
            if (member.PrimaryRentalCityId.HasValue) // 假設 PrimaryRentalCityID 是 int?
            {
                // 假設您的 DataAccessService 有一個 GetCityCodeByIdAsync 方法
                cityCode = await _dataAccessService.GetCityCodeByIdAsync(member.PrimaryRentalCityId.Value);
            }

            // 4. 根據 primaryRentalDistrictID 抓取 districtCode
            if (member.PrimaryRentalDistrictId.HasValue) // 假設 PrimaryRentalDistrictID 是 int?
            {
                // 假設您的 DataAccessService 有一個 GetDistrictCodeByIdAsync 方法
                districtCode = await _dataAccessService.GetDistrictCodeByIdAsync(member.PrimaryRentalDistrictId.Value);
            }

            // 5. 返回 cityCode 和 districtCode 給前端
            // 即使某些代碼為 null/empty，也會一併返回，讓前端判斷處理。
            return Ok(new
            {
                cityCode = cityCode,
                districtCode = districtCode,
               
            });
        }
        catch (Exception ex)
        {
            // 記錄錯誤 (生產環境請使用 ILogger)
            Console.Error.WriteLine($"Error fetching user city/district code: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error: Unable to retrieve user city/district code." });
        }
    }
}

