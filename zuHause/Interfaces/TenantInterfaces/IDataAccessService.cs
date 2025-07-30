using zuHause.ViewModels.TenantViewModel;
using zuHause.Models; // 假設您的 Member, City, District 模型在這裡定義
using System.Threading.Tasks; // 因為我們在 Controller 中使用了 async/await

namespace zuHause.Interfaces.TenantInterfaces
{
    public interface IDataAccessService
    {
        // 跑馬燈功能
        List<MarqueeMessageViewModel> GetMarqueeMessages();

        // 輪播圖功能
        List<string> GetCarouselImageUrls();

        // 原本的 GetUserCityCode，可以修改為返回更詳細的資訊，或者拆分成更精細的方法

        // 新增：根據 MemberID 獲取會員的詳細資料 (包含 PrimaryRentalCityID 和 PrimaryRentalDistrictID)
        Task<Member?> GetMemberByIdAsync(int memberId);

        // 新增：根據 CityID 獲取 CityCode
        Task<string?> GetCityCodeByIdAsync(int cityId);

        // 新增：根據 DistrictID 獲取 DistrictCode
        Task<string?> GetDistrictCodeByIdAsync(int districtId);

        // 也可以考慮新增一個方法，直接返回用戶的偏好城市和區域代碼的組合物件
        // 但如果只是為了在 GetUserCityCodeAndDistrictCodeApi 中使用，拆分更清晰。
        // Task<UserLocationPreference?> GetUserLocationPreferencesAsync(int memberId);
    }
}