using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using zuHause.Interfaces.TenantInterfaces;
using zuHause.Models;
using zuHause.ViewModels.TenantViewModel;

namespace zuHause.Services.TenantServices
{
    public class DataAccessService: IDataAccessService
    {
        private readonly ZuHauseContext _context; // 依賴注入您的 ZuHauseContext

        // Controller 的建構子，用於接收 ZuHauseContext
        public DataAccessService(ZuHauseContext context)
        {
            _context = context;
        }
        //公告跑馬燈列表
        public List<MarqueeMessageViewModel> GetMarqueeMessages()
        {
            var now = DateTime.Now;


            var activeMarquees = _context.SiteMessages
                .Where(m =>
                    m.IsActive == true &&
                    m.DeletedAt == null &&
                    m.Category == "MARQUEE" &&
                    m.ModuleScope == "TENANT" &&
                    m.StartAt <= now &&
                    (m.EndAt == null || m.EndAt >= now)
                )
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new MarqueeMessageViewModel // 創建 MarqueeMessageViewModel 實例
                {
                    MessageText = m.SiteMessageContent, // 假設 SiteMessageContent 是跑馬燈文字
                    AttachmentUrl = m.AttachmentUrl // 假設你的 SiteMessages 實體中有一個 AttachmentUrl 屬性
                })
                .ToList();

            return activeMarquees;

        }

        //獲取輪播圖圖片 URL 列表
        public List<string> GetCarouselImageUrls()
        {
            var now = DateTime.Now;

            var activeCarouselImages = _context.CarouselImages // 假設你的實體表名為 CarouselImages
                .Where(c =>
                    c.IsActive == true &&
                    c.DeletedAt == null &&
                    c.Category == "tenant" &&                   
                     c.StartAt <= now &&
                    ( c.EndAt >= now || c.EndAt == null) // EndAt==null 表示沒有限制時間
                )
                .OrderBy(c => c.DisplayOrder) // 根據 DisplayOrder 排序
                .Select(c => c.ImageUrl)
                .ToList();

            return activeCarouselImages;
        }

        public string? GetUserCityCode(int memberId) // 參數類型為 int
        {
            // 使用 LINQ Join 查詢，比對 Member.MemberId 和 Member.PrimaryRentalDistrictId
            var cityCode = (from member in _context.Members
                            join district in _context.Districts
                            on member.PrimaryRentalDistrictId equals district.DistrictId // 注意這裡是 PrimaryRentalDistrictId
                            where member.MemberId == memberId // 比對 MemberId
                            select district.CityCode)
                           .FirstOrDefault();

            return cityCode;
        }


        // **新增：實現 GetMemberByIdAsync 方法**
        public async Task<Member?> GetMemberByIdAsync(int memberId)
        {
            // 使用 FirstOrDefaultAsync 異步地查詢會員
            return await _context.Members
                                 .FirstOrDefaultAsync(m => m.MemberId == memberId);
        }

        // **新增：實現 GetCityCodeByIdAsync 方法**
        public async Task<string?> GetCityCodeByIdAsync(int cityId)
        {
            // 使用 FirstOrDefaultAsync 異步地查詢 CityCode
            return await _context.Cities
                                 .Where(c => c.CityId == cityId) 
                                 .Select(c => c.CityCode)
                                 .FirstOrDefaultAsync();
        }

        // **新增：實現 GetDistrictCodeByIdAsync 方法**
        public async Task<string?> GetDistrictCodeByIdAsync(int districtId)
        {
            // 使用 FirstOrDefaultAsync 異步地查詢 DistrictCode
            return await _context.Districts
                                 .Where(d => d.DistrictId == districtId) 
                                 .Select(d => d.DistrictCode)
                                 .FirstOrDefaultAsync();
        }
    }
    
}
