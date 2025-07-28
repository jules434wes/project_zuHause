
namespace zuHause.ViewModels.TenantViewModel
{
    public class FrontPageViewModel
    {
        //跑馬燈
        public MarqueeViewModel Marquee { get; set; } = new MarqueeViewModel();

        //輪播圖
        public CarouselViewModel Carousel { get; set; } = new CarouselViewModel();
    }



    public class MarqueeViewModel
    {
        public List<MarqueeMessageViewModel> MarqueeMessages { get; set; } = new List<MarqueeMessageViewModel>(); // 跑馬燈內容列表
    }

    public class MarqueeMessageViewModel 
    {
        public string? MessageText { get; set; } // 跑馬燈顯示的文字

        // AttachmentUrl 可以為 null 或空字串。
        // 如果有值，點擊將跳轉到此 URL；如果為 null 或空，則不跳轉。
        public string? AttachmentUrl { get; set; }
    }

    public class CarouselViewModel
    {
        public List<string> ImageUrls { get; set; } = new List<string>(); // 輪播圖圖片 URL 列表
    }
}
