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

      
    }
}