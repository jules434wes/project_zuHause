using Microsoft.AspNetCore.Mvc;
using zuHause.AdminViewModels;
using zuHause.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace zuHause.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminCookies")]
    public class AdminController : Controller
    {
        private readonly ZuHauseContext _context;

        public AdminController(ZuHauseContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult admin_usersList()
        {
            var viewModel = new AdminUserListViewModel(_context);
            return View(viewModel);
        }

        public IActionResult admin_landlordList()
        {
            var viewModel = new AdminUserListViewModel(_context, landlordsOnly: true);
            return View(viewModel);
        }

        public IActionResult admin_propertiesList()
        {
            var viewModel = new AdminPropertyListViewModel();
            return View(viewModel);
        }
        
        public IActionResult admin_userDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = new AdminUserDetailsViewModel(_context, id.Value);
            return View(viewModel);
        }

        public IActionResult admin_propertyDetails()
        {
            return View();
        }

        public IActionResult admin_customerServiceList()
        {
            return View();
        }

        public IActionResult admin_customerServiceDetails(string id = "CS001")
        {
            var viewModel = new AdminCustomerServiceDetailsViewModel(id);
            return View(viewModel);
        }

        public IActionResult admin_systemMessageList()
        {
            var viewModel = new AdminSystemMessageListViewModel();
            return View(viewModel);
        }

        public IActionResult admin_systemMessageNew()
        {
            var viewModel = new AdminSystemMessageNewViewModel(_context);
            return View(viewModel);
        }

        // AJAX endpoints for dynamic data
        [HttpPost]
        public IActionResult SearchUsers(string keyword, string searchField)
        {
            // 模擬用戶搜尋結果
            var users = new[]
            {
                new { id = "M001", name = "王小明", email = "wang@example.com" },
                new { id = "M002", name = "李小華", email = "lee@example.com" },
                new { id = "M003", name = "張小美", email = "zhang@example.com" }
            };
            
            return Json(users.Where(u => 
                u.name.Contains(keyword) || 
                u.email.Contains(keyword) || 
                u.id.Contains(keyword)).Take(5));
        }

        [HttpPost]
        public IActionResult GetTemplates(string category = "")
        {
            // 模擬模板資料
            var templates = new[]
            {
                new { id = 1, title = "歡迎訊息", category = "welcome", content = "歡迎加入zuHause平台..." },
                new { id = 2, title = "驗證通知", category = "verification", content = "您的身分驗證已通過..." },
                new { id = 3, title = "租屋提醒", category = "reminder", content = "提醒您注意租屋相關事項..." }
            };

            if (!string.IsNullOrEmpty(category))
            {
                templates = templates.Where(t => t.category == category).ToArray();
            }

            return Json(templates);
        }

        [HttpGet]
        public IActionResult GetCities()
        {
            var cities = _context.Cities
                .Where(c => c.IsActive)
                .OrderBy(c => c.CityId)
                .Select(c => new { id = c.CityId, name = c.CityName })
                .ToList();

            return Json(cities);
        }

        [HttpPost]
        public IActionResult FilterLandlords(
            string? keyword = null,
            string? searchField = "memberID",
            string? accountStatus = null,
            int? gender = null,
            int? residenceCityID = null,
            DateTime? registerDateStart = null,
            DateTime? registerDateEnd = null,
            DateTime? lastLoginDateStart = null,
            DateTime? lastLoginDateEnd = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _context.Members
                    .Where(m => m.IsLandlord) // 只查詢房東
                    .AsQueryable();

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(keyword))
                {
                    switch (searchField?.ToLower())
                    {
                        case "memberid":
                            query = query.Where(m => m.MemberId.ToString().Contains(keyword));
                            break;
                        case "membername":
                            query = query.Where(m => m.MemberName.Contains(keyword));
                            break;
                        case "email":
                            query = query.Where(m => m.Email.Contains(keyword));
                            break;
                        case "phonenumber":
                            query = query.Where(m => m.PhoneNumber.Contains(keyword));
                            break;
                        case "nationalidno":
                            query = query.Where(m => m.NationalIdNo != null && m.NationalIdNo.Contains(keyword));
                            break;
                        default:
                            // 預設搜尋多個欄位
                            query = query.Where(m => 
                                m.MemberId.ToString().Contains(keyword) ||
                                m.MemberName.Contains(keyword) ||
                                m.Email.Contains(keyword) ||
                                m.PhoneNumber.Contains(keyword) ||
                                (m.NationalIdNo != null && m.NationalIdNo.Contains(keyword)));
                            break;
                    }
                }

                // 帳戶狀態篩選
                if (!string.IsNullOrEmpty(accountStatus))
                {
                    bool isActive = accountStatus == "active";
                    query = query.Where(m => m.IsActive == isActive);
                }

                // 性別篩選
                if (gender.HasValue && gender > 0)
                {
                    query = query.Where(m => (int)m.Gender == gender.Value);
                }

                // 居住縣市篩選
                if (residenceCityID.HasValue && residenceCityID > 0)
                {
                    query = query.Where(m => m.ResidenceCityId == residenceCityID.Value);
                }

                // 註冊日期範圍篩選
                if (registerDateStart.HasValue)
                {
                    query = query.Where(m => m.CreatedAt >= registerDateStart.Value);
                }
                if (registerDateEnd.HasValue)
                {
                    var endDate = registerDateEnd.Value.Date.AddDays(1); // 包含結束日期的整天
                    query = query.Where(m => m.CreatedAt < endDate);
                }

                // 最後登入日期範圍篩選
                if (lastLoginDateStart.HasValue)
                {
                    query = query.Where(m => m.LastLoginAt.HasValue && m.LastLoginAt >= lastLoginDateStart.Value);
                }
                if (lastLoginDateEnd.HasValue)
                {
                    var endDate = lastLoginDateEnd.Value.Date.AddDays(1);
                    query = query.Where(m => m.LastLoginAt.HasValue && m.LastLoginAt < endDate);
                }

                // 總數計算
                var totalCount = query.Count();

                // 分頁處理
                var landlords = query
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new 
                    {
                        Member = m,
                        ResidenceCity = _context.Cities.FirstOrDefault(c => c.CityId == m.ResidenceCityId)
                    })
                    .Select(x => new
                    {
                        MemberID = x.Member.MemberId.ToString(),
                        MemberName = x.Member.MemberName,
                        Email = x.Member.Email,
                        PhoneNumber = x.Member.PhoneNumber,
                        AccountStatus = x.Member.IsActive ? "active" : "inactive",
                        RegistrationDate = x.Member.CreatedAt.ToString("yyyy-MM-dd"),
                        LastLoginTime = x.Member.LastLoginAt.HasValue ? 
                            x.Member.LastLoginAt.Value.ToString("yyyy-MM-dd HH:mm") : "--",
                        Gender = (int)x.Member.Gender,
                        ResidenceCityName = x.ResidenceCity != null ? x.ResidenceCity.CityName : "",
                        IsLandlord = x.Member.IsLandlord
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = landlords,
                    totalCount = totalCount,
                    currentPage = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "篩選過程中發生錯誤：" + ex.Message
                });
            }
        }
    }
}
