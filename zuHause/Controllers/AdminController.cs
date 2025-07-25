using Microsoft.AspNetCore.Mvc;
using zuHause.AdminViewModels;
using zuHause.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using zuHause.Attributes;
using zuHause.Interfaces;

namespace zuHause.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminCookies")]
    public class AdminController : Controller
    {
        private readonly ZuHauseContext _context;
        private readonly IImageQueryService _imageQueryService;

        public AdminController(ZuHauseContext context, IImageQueryService imageQueryService)
        {
            _context = context;
            _imageQueryService = imageQueryService;
        }

        /// <summary>
        /// 取得當前登入管理員ID
        /// </summary>
        protected string? GetCurrentAdminId()
        {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 取得當前登入管理員姓名
        /// </summary>
        protected string? GetCurrentAdminName()
        {
            return HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// 取得當前登入管理員帳號
        /// </summary>
        protected string? GetCurrentAdminAccount()
        {
            return HttpContext.User.FindFirst("Account")?.Value;
        }

        /// <summary>
        /// 取得當前登入管理員角色代碼
        /// </summary>
        protected string? GetCurrentAdminRoleCode()
        {
            return HttpContext.User.FindFirst("RoleCode")?.Value;
        }

        /// <summary>
        /// 取得當前登入管理員角色名稱
        /// </summary>
        protected string? GetCurrentAdminRoleName()
        {
            return HttpContext.User.FindFirst("RoleName")?.Value;
        }

        /// <summary>
        /// 取得當前登入管理員權限JSON
        /// </summary>
        protected string? GetCurrentAdminPermissions()
        {
            return HttpContext.User.FindFirst("PermissionsJSON")?.Value;
        }

        /// <summary>
        /// 取得當前登入管理員完整資訊
        /// </summary>
        protected object GetCurrentAdminInfo()
        {
            return new
            {
                AdminId = GetCurrentAdminId(),
                Name = GetCurrentAdminName(),
                Account = GetCurrentAdminAccount(),
                RoleCode = GetCurrentAdminRoleCode(),
                RoleName = GetCurrentAdminRoleName(),
                Permissions = GetCurrentAdminPermissions()
            };
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 會員列表與驗證頁面
        /// 需要 member_list 權限才能存取
        /// </summary>
        [RequireAdminPermission(AdminPermissions.MemberList)]
        public IActionResult admin_usersList()
        {
            var viewModel = new AdminUserListViewModel(_context);
            return View(viewModel);
        }

        /// <summary>
        /// 房東列表頁面
        /// 需要 landlord_list 權限才能存取
        /// </summary>
        [RequireAdminPermission(AdminPermissions.LandlordList)]
        public IActionResult admin_landlordList()
        {
            var viewModel = new AdminUserListViewModel(_context, landlordsOnly: true);
            return View(viewModel);
        }

        /// <summary>
        /// 房源列表頁面
        /// 需要 property_list 權限才能存取
        /// </summary>
        [RequireAdminPermission(AdminPermissions.PropertyList)]
        public IActionResult admin_propertiesList()
        {
            var viewModel = new AdminPropertyListViewModel(_context);
            return View(viewModel);
        }
        
        /// <summary>
        /// 會員詳細資料頁面
        /// 需要 member_list 權限才能存取（與會員列表使用相同權限）
        /// </summary>
        [RequireAdminPermission(AdminPermissions.MemberDetails)]
        public IActionResult admin_userDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = new AdminUserDetailsViewModel(_context, id.Value);
            return View(viewModel);
        }

        /// <summary>
        /// 房源詳細資料頁面
        /// 需要 property_list 權限才能存取（與房源列表使用相同權限）
        /// </summary>
        [RequireAdminPermission(AdminPermissions.PropertyDetails)]
        public IActionResult admin_propertyDetails(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound("房源ID不能為空");
            }

            var viewModel = new AdminPropertyDetailsViewModel(_context, _imageQueryService, id.Value);
            
            if (viewModel.Data == null)
            {
                return NotFound($"找不到房源ID: {id}");
            }

            return View(viewModel);
        }

        /// <summary>
        /// 客戶服務列表頁面
        /// 需要 customer_service_list 權限才能存取
        /// </summary>
        [RequireAdminPermission(AdminPermissions.CustomerServiceList)]
        public IActionResult admin_customerServiceList()
        {
            return View();
        }

        /// <summary>
        /// 客戶服務詳細處理頁面
        /// 需要 customer_service_list 權限才能存取（與客服列表使用相同權限）
        /// </summary>
        [RequireAdminPermission(AdminPermissions.CustomerServiceDetails)]
        public IActionResult admin_customerServiceDetails(string id = "CS001")
        {
            var viewModel = new AdminCustomerServiceDetailsViewModel(id);
            
            // 示範：取得當前管理員資訊
            ViewBag.CurrentAdminId = GetCurrentAdminId();
            ViewBag.CurrentAdminName = GetCurrentAdminName();
            ViewBag.CurrentAdminAccount = GetCurrentAdminAccount();
            
            return View(viewModel);
        }

        /// <summary>
        /// 系統訊息列表頁面
        /// 需要 system_message_list 權限才能存取
        /// </summary>
        [RequireAdminPermission(AdminPermissions.SystemMessageList)]
        public IActionResult admin_systemMessageList()
        {
            var viewModel = new AdminSystemMessageListViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// 新增系統訊息頁面
        /// 需要 system_message_list 權限才能存取（與系統訊息列表使用相同權限）
        /// </summary>
        [RequireAdminPermission(AdminPermissions.SystemMessageNew)]
        public IActionResult admin_systemMessageNew()
        {
            var viewModel = new AdminSystemMessageNewViewModel(_context);
            return View(viewModel);
        }

        /// <summary>
        /// 房源投訴管理頁面
        /// 需要 property_complaint_list 權限才能存取
        /// </summary>
        [RequireAdminPermission(AdminPermissions.PropertyComplaintList)]
        public IActionResult admin_propertyComplaints()
        {
            // 調試：檢查資料庫連接和資料
            try
            {
                var totalComplaints = _context.PropertyComplaints.Count();
                var totalMembers = _context.Members.Count();
                var totalProperties = _context.Properties.Count();
                var totalLandlords = _context.Members.Count(m => m.IsLandlord);
                
                // 記錄在ViewBag中用於調試
                ViewBag.Debug = $"PropertyComplaints: {totalComplaints}, Members: {totalMembers}, Properties: {totalProperties}, Landlords: {totalLandlords}";
                
                // 如果沒有投訴資料，嘗試創建一些測試資料
                if (totalComplaints == 0 && totalMembers > 0 && totalProperties > 0)
                {
                    var firstMember = _context.Members.First();
                    var firstProperty = _context.Properties.First();
                    var firstLandlord = _context.Members.Where(m => m.IsLandlord).FirstOrDefault() ?? firstMember;
                    
                    var testComplaint = new PropertyComplaint
                    {
                        ComplainantId = firstMember.MemberId,
                        PropertyId = firstProperty.PropertyId,
                        LandlordId = firstLandlord.MemberId,
                        ComplaintContent = "測試投訴內容 - 這是一個測試投訴記錄，用於驗證系統功能是否正常運作。",
                        StatusCode = "OPEN",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    
                    _context.PropertyComplaints.Add(testComplaint);
                    
                    // 再創建一個已處理的測試投訴
                    var testComplaint2 = new PropertyComplaint
                    {
                        ComplainantId = firstMember.MemberId,
                        PropertyId = firstProperty.PropertyId,
                        LandlordId = firstLandlord.MemberId,
                        ComplaintContent = "第二個測試投訴內容 - 這個投訴已經被處理完成。",
                        StatusCode = "RESOLVED",
                        CreatedAt = DateTime.Now.AddDays(-1),
                        UpdatedAt = DateTime.Now,
                        ResolvedAt = DateTime.Now,
                        InternalNote = "測試註記：此投訴已處理完成"
                    };
                    
                    _context.PropertyComplaints.Add(testComplaint2);
                    _context.SaveChanges();
                    
                    totalComplaints = _context.PropertyComplaints.Count();
                    ViewBag.Debug += $" | 已創建 2 筆測試資料，總投訴數: {totalComplaints}";
                }
                
                // 檢查數據完整性
                if (totalComplaints > 0)
                {
                    var complaintsWithDetails = _context.PropertyComplaints
                        .Select(pc => new
                        {
                            ComplaintId = pc.ComplaintId,
                            HasComplainant = _context.Members.Any(m => m.MemberId == pc.ComplainantId),
                            HasProperty = _context.Properties.Any(p => p.PropertyId == pc.PropertyId),
                            HasLandlord = _context.Members.Any(m => m.MemberId == pc.LandlordId)
                        })
                        .Take(3)
                        .ToList();
                    
                    var integrityInfo = string.Join(", ", complaintsWithDetails.Select(c => 
                        $"ID{c.ComplaintId}(C:{c.HasComplainant},P:{c.HasProperty},L:{c.HasLandlord})"));
                    
                    ViewBag.Debug += $" | 資料完整性: {integrityInfo}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Debug = $"調試錯誤: {ex.Message}";
                if (ex.InnerException != null)
                {
                    ViewBag.Debug += $" | 內部錯誤: {ex.InnerException.Message}";
                }
            }
            
            var viewModel = new AdminPropertyComplaintsViewModel(_context);
            return View(viewModel);
        }

        /// <summary>
        /// 取得投訴詳情 (AJAX)
        /// </summary>
        [HttpGet]
        public IActionResult GetComplaintDetails(int complaintId)
        {
            try
            {
                var complaint = _context.PropertyComplaints
                    .Where(pc => pc.ComplaintId == complaintId)
                    .Select(pc => new {
                        ComplaintId = pc.ComplaintId,
                        ComplainantName = pc.Complainant != null ? pc.Complainant.MemberName : "未知用戶",
                        ComplainantId = pc.ComplainantId,
                        PropertyTitle = pc.Property != null ? pc.Property.Title : "未知房源",
                        PropertyId = pc.PropertyId,
                        LandlordName = pc.Landlord != null ? pc.Landlord.MemberName : "未知房東",
                        LandlordId = pc.LandlordId,
                        ComplaintContent = pc.ComplaintContent ?? "",
                        StatusCode = pc.StatusCode ?? "UNKNOWN",
                        CreatedAt = pc.CreatedAt,
                        UpdatedAt = pc.UpdatedAt,
                        ResolvedAt = pc.ResolvedAt,
                        InternalNote = pc.InternalNote,
                        HandledBy = pc.HandledBy
                    })
                    .FirstOrDefault();

                if (complaint == null)
                {
                    return Json(new { success = false, message = "找不到投訴記錄" });
                }

                var result = new
                {
                    success = true,
                    data = new
                    {
                        ComplaintId = complaint.ComplaintId,
                        ComplaintIdDisplay = $"CMPL-{complaint.ComplaintId:0000}",
                        ComplainantName = complaint.ComplainantName,
                        ComplainantId = complaint.ComplainantId,
                        PropertyTitle = complaint.PropertyTitle,
                        PropertyId = complaint.PropertyId,
                        LandlordName = complaint.LandlordName,
                        LandlordId = complaint.LandlordId,
                        ComplaintContent = complaint.ComplaintContent,
                        Status = complaint.StatusCode,
                        StatusDisplay = GetComplaintStatusDisplay(complaint.StatusCode),
                        CreatedAt = complaint.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedAt = complaint.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        ResolvedAt = complaint.ResolvedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        InternalNote = complaint.InternalNote ?? "",
                        HandledBy = complaint.HandledBy
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "取得投訴詳情時發生錯誤：" + ex.Message });
            }
        }

        /// <summary>
        /// 更新內部註記 (AJAX)
        /// </summary>
        [HttpPost]
        public IActionResult UpdateInternalNote(int complaintId, string internalNote)
        {
            try
            {
                var complaint = _context.PropertyComplaints
                    .FirstOrDefault(pc => pc.ComplaintId == complaintId);

                if (complaint == null)
                {
                    return Json(new { success = false, message = "找不到投訴記錄" });
                }

                complaint.InternalNote = internalNote;
                complaint.UpdatedAt = DateTime.Now;
                complaint.HandledBy = int.Parse(GetCurrentAdminId() ?? "0");

                _context.SaveChanges();

                return Json(new 
                { 
                    success = true, 
                    message = "內部註記已更新",
                    data = new
                    {
                        UpdatedAt = complaint.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        HandledBy = complaint.HandledBy
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "更新內部註記時發生錯誤：" + ex.Message });
            }
        }

        /// <summary>
        /// 更新投訴狀態 (AJAX)
        /// </summary>
        [HttpPost]
        public IActionResult UpdateComplaintStatus(int complaintId, string status)
        {
            try
            {
                var complaint = _context.PropertyComplaints
                    .FirstOrDefault(pc => pc.ComplaintId == complaintId);

                if (complaint == null)
                {
                    return Json(new { success = false, message = "找不到投訴記錄" });
                }

                complaint.StatusCode = status;
                complaint.UpdatedAt = DateTime.Now;
                complaint.HandledBy = int.Parse(GetCurrentAdminId() ?? "0");

                if (status == "RESOLVED" || status == "CLOSED")
                {
                    complaint.ResolvedAt = DateTime.Now;
                }

                _context.SaveChanges();

                return Json(new 
                { 
                    success = true, 
                    message = "投訴狀態已更新",
                    data = new
                    {
                        Status = complaint.StatusCode,
                        StatusDisplay = GetComplaintStatusDisplay(complaint.StatusCode),
                        UpdatedAt = complaint.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        ResolvedAt = complaint.ResolvedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        HandledBy = complaint.HandledBy
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "更新投訴狀態時發生錯誤：" + ex.Message });
            }
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

        /// <summary>
        /// 篩選房源投訴 (AJAX)
        /// </summary>
        [HttpPost]
        public IActionResult FilterComplaints(
            string? keyword = null,
            string? searchField = "all",
            string? status = null,
            DateTime? dateStart = null,
            DateTime? dateEnd = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"FilterComplaints called with parameters: keyword={keyword}, searchField={searchField}, status={status}, dateStart={dateStart}, dateEnd={dateEnd}, page={page}");
                
                var query = _context.PropertyComplaints.AsQueryable();
                
                // 檢查總記錄數
                var totalRecords = query.Count();
                System.Diagnostics.Debug.WriteLine($"Total PropertyComplaints records: {totalRecords}");
                
                if (totalRecords == 0)
                {
                    return Json(new
                    {
                        success = true,
                        data = new List<object>(),
                        totalCount = 0,
                        currentPage = page,
                        pageSize = pageSize,
                        totalPages = 0,
                        message = "資料庫中沒有投訴記錄"
                    });
                }

                // 關鍵字搜尋 - 使用更高效的 Join 查詢
                if (!string.IsNullOrEmpty(keyword?.Trim()))
                {
                    keyword = keyword.Trim();
                    System.Diagnostics.Debug.WriteLine($"Applying keyword filter: {keyword} in field: {searchField}");
                    
                    switch (searchField?.ToLower())
                    {
                        case "complainantname":
                            query = from pc in query
                                   join m in _context.Members on pc.ComplainantId equals m.MemberId
                                   where m.MemberName.Contains(keyword)
                                   select pc;
                            break;
                        case "propertytitle":
                            query = from pc in query
                                   join p in _context.Properties on pc.PropertyId equals p.PropertyId
                                   where p.Title.Contains(keyword)
                                   select pc;
                            break;
                        case "landlordname":
                            query = from pc in query
                                   join m in _context.Members on pc.LandlordId equals m.MemberId
                                   where m.MemberName.Contains(keyword)
                                   select pc;
                            break;
                        case "complaintcontent":
                            query = query.Where(pc => pc.ComplaintContent.Contains(keyword));
                            break;
                        case "all":
                        default:
                            // 全部欄位搜尋 - 使用複合查詢
                            var complainantMatches = from pc in query
                                                   join m in _context.Members on pc.ComplainantId equals m.MemberId
                                                   where m.MemberName.Contains(keyword)
                                                   select pc.ComplaintId;
                            
                            var propertyMatches = from pc in query
                                                join p in _context.Properties on pc.PropertyId equals p.PropertyId
                                                where p.Title.Contains(keyword)
                                                select pc.ComplaintId;
                            
                            var landlordMatches = from pc in query
                                                 join m in _context.Members on pc.LandlordId equals m.MemberId
                                                 where m.MemberName.Contains(keyword)
                                                 select pc.ComplaintId;
                            
                            var contentMatches = query.Where(pc => pc.ComplaintContent.Contains(keyword))
                                                    .Select(pc => pc.ComplaintId);
                            
                            var allMatchIds = complainantMatches.Union(propertyMatches)
                                                               .Union(landlordMatches)
                                                               .Union(contentMatches);
                            
                            query = query.Where(pc => allMatchIds.Contains(pc.ComplaintId));
                            break;
                    }
                }

                // 狀態篩選
                if (!string.IsNullOrEmpty(status?.Trim()))
                {
                    System.Diagnostics.Debug.WriteLine($"Applying status filter: {status}");
                    query = query.Where(pc => pc.StatusCode == status.Trim());
                }

                // 日期範圍篩選
                if (dateStart.HasValue)
                {
                    var startDate = dateStart.Value.Date;
                    System.Diagnostics.Debug.WriteLine($"Applying date start filter: {startDate}");
                    query = query.Where(pc => pc.CreatedAt >= startDate);
                }
                if (dateEnd.HasValue)
                {
                    var endDate = dateEnd.Value.Date.AddDays(1); // 包含當天結束
                    System.Diagnostics.Debug.WriteLine($"Applying date end filter: {endDate}");
                    query = query.Where(pc => pc.CreatedAt < endDate);
                }

                // 計算篩選後的總數
                var totalCount = query.Count();
                System.Diagnostics.Debug.WriteLine($"Filtered count: {totalCount}");

                // 分頁查詢 - 使用 Include 來載入相關資料
                var pagedComplaints = query
                    .Include(pc => pc.Complainant)
                    .Include(pc => pc.Property)
                    .Include(pc => pc.Landlord)
                    .OrderByDescending(pc => pc.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Paged complaints loaded: {pagedComplaints.Count}");

                // 格式化輸出資料
                var complaints = pagedComplaints.Select(pc => new
                {
                    ComplaintId = pc.ComplaintId,
                    ComplaintIdDisplay = $"CMPL-{pc.ComplaintId:0000}",
                    ComplainantName = pc.Complainant?.MemberName ?? "未知用戶",
                    ComplainantId = pc.ComplainantId,
                    PropertyTitle = pc.Property?.Title ?? "未知房源",
                    PropertyId = pc.PropertyId,
                    LandlordName = pc.Landlord?.MemberName ?? "未知房東",
                    LandlordId = pc.LandlordId,
                    ComplaintContent = pc.ComplaintContent ?? "",
                    Summary = (pc.ComplaintContent?.Length > 50) 
                        ? pc.ComplaintContent.Substring(0, 50) + "..."
                        : (pc.ComplaintContent ?? "無內容"),
                    Status = pc.StatusCode ?? "PENDING",
                    StatusDisplay = GetComplaintStatusDisplay(pc.StatusCode ?? "PENDING"),
                    CreatedAt = pc.CreatedAt,
                    UpdatedAt = pc.UpdatedAt,
                    ResolvedAt = pc.ResolvedAt
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"Formatted complaints: {complaints.Count}");
                
                return Json(new
                {
                    success = true,
                    data = complaints,
                    totalCount = totalCount,
                    currentPage = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    message = totalCount == 0 ? "沒有符合條件的投訴記錄" : null
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FilterComplaints error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return Json(new
                {
                    success = false,
                    message = $"篩選投訴記錄時發生錯誤：{ex.Message}"
                });
            }
        }

        /// <summary>
        /// 取得投訴狀態顯示文字
        /// </summary>
        private string GetComplaintStatusDisplay(string statusCode)
        {
            return statusCode switch
            {
                "PENDING" => "待處理",
                "OPEN" => "處理中",
                "RESOLVED" => "已處理", 
                "CLOSED" => "已關閉",
                _ => "未知狀態"
            };
        }
    }
}
