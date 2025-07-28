using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using zuHause.Interfaces.TenantInterfaces;
using zuHause.ViewModels.TenantViewModel;


namespace zuHause.Controllers
{
    public class TenantController : Controller // 這是你的 TenantController
    {

        private readonly IDataAccessService _dataAccessService;

        // Controller 的建構子，用於接收 ZuHauseContext
        public TenantController(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
        }

        public IActionResult FrontPage()
        {
            var viewModel = new FrontPageViewModel();

            // 僅獲取跑馬燈資料
            viewModel.Marquee.MarqueeMessages = _dataAccessService.GetMarqueeMessages();

            // 填充輪播圖數據
            viewModel.Carousel.ImageUrls = _dataAccessService.GetCarouselImageUrls();


            return View(viewModel);
        }

        [HttpGet]
        [Route("/api/Member/UserCityCode")]
        [Produces("application/json")]
        public IActionResult GetUserCityCodeApi()
        {
            try
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                // 從 ClaimTypes.NameIdentifier 獲取字串類型的用戶 ID
                var memberIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberIdString))
                {
                    return BadRequest(new { message = "Member ID claim not found in authentication token." });
                }

                int memberId;
                // 嘗試將字串 ID 解析為 int
                if (!int.TryParse(memberIdString, out memberId))
                {
                    // 如果解析失敗，表示 Claim 中的 Member ID 不是預期的 int 類型
                    return BadRequest(new { message = "Invalid member ID format in authentication token." });
                }

                // 調用 DataAccessService 中的 GetUserCityCode 方法
                string? cityCode = _dataAccessService.GetUserCityCode(memberId);

                if (string.IsNullOrEmpty(cityCode))
                {
                    // 返回 404 Not Found 如果 CityCode 不存在，表示會員沒有設定主要承租區域或區域代碼不存在
                    return NotFound(new { message = "CityCode not found for the current member's primary rental district." });
                }

                return Ok(new { cityCode = cityCode });
            }
            catch (Exception ex)
            {
                // 記錄錯誤 (例如使用 ILogger)
                Console.Error.WriteLine($"Error fetching user city code: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error: Unable to retrieve user city code." });
            }
        }

    }
}