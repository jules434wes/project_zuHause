using zuHause.ViewModels.TenantViewModel;


namespace zuHause.Interfaces.TenantInterfaces
{
    public interface IDataAccessService
    {
        //跑馬燈功能
        List<MarqueeMessageViewModel> GetMarqueeMessages();

        // 輪播圖功能
        List<string> GetCarouselImageUrls();

        string? GetUserCityCode(int memberId);
    }
}
