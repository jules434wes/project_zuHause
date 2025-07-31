using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.ViewModels;

namespace zuHause.ViewComponents
{
    /// <summary>
    /// 房源投訴 Modal ViewComponent
    /// </summary>
    public class PropertyComplaintModalViewComponent : ViewComponent
    {
        private readonly ZuHauseContext _context;

        public PropertyComplaintModalViewComponent(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 載入房源投訴 Modal
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="userId">用戶ID</param>
        /// <returns>ViewComponent結果</returns>
        public async Task<IViewComponentResult> InvokeAsync(int propertyId, int userId)
        {
            try
            {
                // 取得房源資料
                var property = await _context.Properties
                    .Include(p => p.LandlordMember)
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    // 房源不存在，返回空的 ViewModel
                    return View(new PropertyComplaintViewModel());
                }

                // 取得投訴人資料
                var complainant = await _context.Members
                    .FirstOrDefaultAsync(m => m.MemberId == userId);

                if (complainant == null)
                {
                    // 用戶未登入或不存在，返回空的 ViewModel
                    return View(new PropertyComplaintViewModel());
                }

                // 取得投訴類型選項（從系統代碼表）
                var complaintTypes = await _context.SystemCodes
                    .Where(sc => sc.CodeCategory == "COMPLAINT_TYPE" && sc.IsActive)
                    .OrderBy(sc => sc.DisplayOrder)
                    .Select(sc => new SelectListItem
                    {
                        Value = sc.Code,
                        Text = sc.CodeName
                    })
                    .ToListAsync();

                // 如果沒有投訴類型，提供預設選項
                if (!complaintTypes.Any())
                {
                    complaintTypes = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "PROPERTY_CONDITION", Text = "房屋狀況問題" },
                        new SelectListItem { Value = "LANDLORD_BEHAVIOR", Text = "房東行為不當" },
                        new SelectListItem { Value = "MISLEADING_INFO", Text = "資訊不實" },
                        new SelectListItem { Value = "SAFETY_ISSUE", Text = "安全疑慮" },
                        new SelectListItem { Value = "OTHER", Text = "其他問題" }
                    };
                }

                // 建立 ViewModel
                var viewModel = new PropertyComplaintViewModel
                {
                    PropertyId = propertyId,
                    ComplainantId = userId,
                    LandlordId = property.LandlordMemberId,
                    PropertyTitle = property.Title,
                    PropertyAddress = property.AddressLine ?? string.Empty,
                    LandlordName = property.LandlordMember?.MemberName ?? "未知",
                    ComplaintTypes = complaintTypes,
                    ComplainantName = complainant.MemberName,
                    ComplainantPhone = complainant.PhoneNumber ?? string.Empty,
                    ComplainantEmail = complainant.Email ?? string.Empty
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // 記錄錯誤並返回空的 ViewModel
                // 這裡可以加入 ILogger 記錄錯誤
                return View(new PropertyComplaintViewModel());
            }
        }
    }
}