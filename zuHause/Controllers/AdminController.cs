using Microsoft.AspNetCore.Mvc;
using zuHause.AdminViewModels;
using zuHause.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using zuHause.Attributes;
using zuHause.Interfaces;
using zuHause.AdminDTOs;

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
        [RequireAdminPermission(AdminPermissions.MemberList)]
        public IActionResult admin_userDetails(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return NotFound("無效的會員ID");
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
        public IActionResult admin_customerServiceDetails(int ticketId)
        {
            try
            {
                var ticket = _context.CustomerServiceTickets
                    .Include(t => t.Member)
                    .Include(t => t.Property)
                    .Include(t => t.Contract)
                    .Include(t => t.FurnitureOrder)
                    .FirstOrDefault(t => t.TicketId == ticketId);

                if (ticket == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的客服案件";
                    return RedirectToAction("admin_customerServiceList");
                }

                var viewModel = new AdminCustomerServiceDetailsViewModel
                {
                    TicketId = ticket.TicketId,
                    TicketIdDisplay = $"CS-{ticket.TicketId:D4}",
                    MemberName = ticket.Member.MemberName,
                    MemberId = ticket.MemberId,
                    Subject = ticket.Subject,
                    CategoryCode = ticket.CategoryCode,
                    CategoryDisplay = GetCategoryDisplay(ticket.CategoryCode),
                    TicketContent = ticket.TicketContent,
                    ReplyContent = ticket.ReplyContent,
                    StatusCode = ticket.StatusCode,
                    StatusDisplay = GetStatusDisplay(ticket.StatusCode),
                    CreatedAt = ticket.CreatedAt,
                    ReplyAt = ticket.ReplyAt,
                    UpdatedAt = ticket.UpdatedAt,
                    HandledBy = ticket.HandledBy,
                    HandledByName = GetHandledByName(ticket.HandledBy),
                    IsResolved = ticket.IsResolved,
                    PageTitle = $"客服案件詳情 - CS-{ticket.TicketId:D4}"
                };
                
                // 取得當前管理員資訊
                ViewBag.CurrentAdminId = GetCurrentAdminId();
                ViewBag.CurrentAdminName = GetCurrentAdminName();
                ViewBag.CurrentAdminAccount = GetCurrentAdminAccount();
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "載入客服案件詳情時發生錯誤";
                return RedirectToAction("admin_customerServiceList");
            }
        }

        /// <summary>
        /// 客服案件篩選 API
        /// </summary>
        [HttpPost]
        [RequireAdminPermission(AdminPermissions.CustomerServiceList)]
        public async Task<IActionResult> FilterCustomerService([FromForm] CustomerServiceFilterDto filter)
        {
            try
            {
                var query = _context.CustomerServiceTickets
                    .Include(t => t.Member)
                    .Include(t => t.Property)
                    .Include(t => t.Contract)
                    .Include(t => t.FurnitureOrder)
                    .AsQueryable();

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    query = filter.SearchField switch
                    {
                        "subject" => query.Where(t => t.Subject.Contains(filter.Keyword)),
                        "memberName" => query.Where(t => t.Member.MemberName.Contains(filter.Keyword)),
                        "content" => query.Where(t => t.TicketContent.Contains(filter.Keyword)),
                        _ => query.Where(t => t.Subject.Contains(filter.Keyword) || 
                                             t.Member.MemberName.Contains(filter.Keyword) ||
                                             t.TicketContent.Contains(filter.Keyword))
                    };
                }

                // 狀態篩選
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    var statuses = filter.Status.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (statuses.Length > 0)
                    {
                        query = query.Where(t => statuses.Contains(t.StatusCode));
                    }
                }
                
                // 類別篩選
                if (!string.IsNullOrEmpty(filter.Category))
                    query = query.Where(t => t.CategoryCode == filter.Category);

                // 日期範圍篩選
                if (filter.DateStart.HasValue)
                    query = query.Where(t => t.CreatedAt >= filter.DateStart.Value);
                
                if (filter.DateEnd.HasValue)
                    query = query.Where(t => t.CreatedAt <= filter.DateEnd.Value.AddDays(1));

                // 計算總數
                var totalCount = await query.CountAsync();

                // 分頁處理
                var page = filter.Page ?? 1;
                var pageSize = filter.PageSize ?? 10;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var ticketData = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        TicketId = t.TicketId,
                        MemberName = t.Member.MemberName,
                        MemberId = t.MemberId,
                        Subject = t.Subject,
                        CategoryCode = t.CategoryCode,
                        StatusCode = t.StatusCode,
                        CreatedAt = t.CreatedAt,
                        HandledBy = t.HandledBy,
                        HasReply = !string.IsNullOrEmpty(t.ReplyContent),
                        IsResolved = t.IsResolved
                    })
                    .ToListAsync();

                var tickets = ticketData.Select(t => new
                {
                    TicketId = t.TicketId,
                    TicketIdDisplay = $"CS-{t.TicketId:D4}",
                    MemberName = t.MemberName,
                    MemberId = t.MemberId,
                    Subject = t.Subject,
                    CategoryCode = t.CategoryCode,
                    CategoryDisplay = GetCategoryDisplay(t.CategoryCode),
                    StatusCode = t.StatusCode,
                    StatusDisplay = GetStatusDisplay(t.StatusCode),
                    CreatedAt = t.CreatedAt,
                    HandledByName = GetHandledByName(t.HandledBy),
                    HasReply = t.HasReply,
                    IsUrgent = (t.CategoryCode == "CONTRACT" && (DateTime.Now - t.CreatedAt).TotalHours > 24 && !t.IsResolved)
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = tickets,
                    totalCount = totalCount,
                    currentPage = page,
                    pageSize = pageSize,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "篩選客服案件時發生錯誤",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 更新客服案件回覆 API
        /// </summary>
        [HttpPost]
        [RequireAdminPermission(AdminPermissions.CustomerServiceList)]
        public async Task<IActionResult> UpdateCustomerServiceReply([FromForm] UpdateCustomerServiceReplyDto dto)
        {
            try
            {
                var ticket = await _context.CustomerServiceTickets
                    .FirstOrDefaultAsync(t => t.TicketId == dto.TicketId);

                if (ticket == null)
                {
                    return Json(new { success = false, message = "找不到指定的客服案件" });
                }

                // 自動抓取當前登入管理員ID
                var currentAdminId = int.Parse(GetCurrentAdminId());

                // 更新案件資訊
                ticket.ReplyContent = dto.ReplyContent;
                ticket.StatusCode = dto.StatusCode;
                ticket.HandledBy = currentAdminId;
                ticket.ReplyAt = DateTime.Now;
                ticket.UpdatedAt = DateTime.Now;
                ticket.IsResolved = (dto.StatusCode == "RESOLVED");

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "客服回覆已更新",
                    data = new
                    {
                        TicketId = ticket.TicketId,
                        StatusCode = ticket.StatusCode,
                        StatusDisplay = GetStatusDisplay(ticket.StatusCode),
                        ReplyAt = ticket.ReplyAt,
                        UpdatedAt = ticket.UpdatedAt,
                        HandledBy = ticket.HandledBy
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "更新客服回覆時發生錯誤",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 取得回覆模板 API
        /// </summary>
        [HttpGet]
        [RequireAdminPermission(AdminPermissions.CustomerServiceList)]
        public async Task<IActionResult> GetMessageTemplates(string categoryCode)
        {
            try
            {
                var templates = await _context.AdminMessageTemplates
                    .Where(t => t.CategoryCode == categoryCode && t.IsActive)
                    .OrderBy(t => t.Title)
                    .Select(t => new
                    {
                        TemplateId = t.TemplateId,
                        Title = t.Title,
                        Content = t.TemplateContent,
                        CategoryCode = t.CategoryCode,
                        IsActive = t.IsActive
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = templates
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "載入回覆模板時發生錯誤",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 取得客服案件狀態統計 API
        /// </summary>
        [HttpGet]
        [RequireAdminPermission(AdminPermissions.CustomerServiceList)]
        public async Task<IActionResult> GetCustomerServiceStatistics()
        {
            try
            {
                var pendingCount = await _context.CustomerServiceTickets
                    .CountAsync(t => t.StatusCode == "PENDING");
                
                var progressCount = await _context.CustomerServiceTickets
                    .CountAsync(t => t.StatusCode == "PROGRESS");
                    
                var resolvedCount = await _context.CustomerServiceTickets
                    .CountAsync(t => t.StatusCode == "RESOLVED");

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        pendingCount,
                        progressCount,
                        resolvedCount,
                        totalCount = pendingCount + progressCount + resolvedCount,
                        unresolvedCount = pendingCount + progressCount
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "載入統計數據時發生錯誤",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 取得客服案件詳情 API
        /// </summary>
        [HttpGet]
        [RequireAdminPermission(AdminPermissions.CustomerServiceDetails)]
        public async Task<IActionResult> GetCustomerServiceDetails(int ticketId)
        {
            try
            {
                var ticket = await _context.CustomerServiceTickets
                    .Include(t => t.Member)
                    .Include(t => t.Property)
                    .Include(t => t.Contract)
                    .Include(t => t.FurnitureOrder)
                    .FirstOrDefaultAsync(t => t.TicketId == ticketId);

                if (ticket == null)
                {
                    return Json(new { success = false, message = "找不到指定的客服案件" });
                }

                var result = new
                {
                    success = true,
                    data = new
                    {
                        TicketId = ticket.TicketId,
                        TicketIdDisplay = $"CS-{ticket.TicketId:D4}",
                        Member = new
                        {
                            MemberId = ticket.MemberId,
                            MemberName = ticket.Member.MemberName,
                            Email = ticket.Member.Email,
                            PhoneNumber = ticket.Member.PhoneNumber
                        },
                        Subject = ticket.Subject,
                        CategoryCode = ticket.CategoryCode,
                        CategoryDisplay = GetCategoryDisplay(ticket.CategoryCode),
                        TicketContent = ticket.TicketContent,
                        ReplyContent = ticket.ReplyContent,
                        StatusCode = ticket.StatusCode,
                        StatusDisplay = GetStatusDisplay(ticket.StatusCode),
                        CreatedAt = ticket.CreatedAt,
                        ReplyAt = ticket.ReplyAt,
                        UpdatedAt = ticket.UpdatedAt,
                        HandledBy = ticket.HandledBy,
                        HandledByName = GetHandledByName(ticket.HandledBy),
                        IsResolved = ticket.IsResolved,
                        RelatedInfo = new
                        {
                            ContractId = ticket.ContractId,
                            PropertyId = ticket.PropertyId,
                            FurnitureOrderId = ticket.FurnitureOrderId
                        }
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "載取客服案件詳情時發生錯誤",
                    error = ex.Message
                });
            }
        }

        

        /// <summary>
        /// 取得類別顯示名稱
        /// </summary>
        private string GetCategoryDisplay(string categoryCode)
        {
            return categoryCode switch
            {
                "PROPERTY" => "房源",
                "CONTRACT" => "合約",
                "FURNITURE" => "家具",
                _ => categoryCode
            };
        }

        /// <summary>
        /// 取得狀態顯示名稱
        /// </summary>
        private string GetStatusDisplay(string statusCode)
        {
            return statusCode switch
            {
                "PENDING" => "待處理",
                "PROGRESS" => "處理中",
                "RESOLVED" => "已處理",
                _ => statusCode
            };
        }

        /// <summary>
        /// 取得處理人員姓名
        /// </summary>
        private string GetHandledByName(int? handledBy)
        {
            if (!handledBy.HasValue) return "---";
            
            var admin = _context.Admins.FirstOrDefault(a => a.AdminId == handledBy.Value);
            return admin?.Name ?? "未知管理員";
        }

        [HttpGet]
        [RequireAdminPermission(AdminPermissions.CustomerServiceList)]
        public async Task<IActionResult> GetCustomerServiceStats()
        {
            try
            {
                var stats = await _context.CustomerServiceTickets
                    .GroupBy(t => t.StatusCode)
                    .Select(g => new { StatusCode = g.Key, Count = g.Count() })
                    .ToListAsync();

                var result = new
                {
                    pendingCount = stats.FirstOrDefault(s => s.StatusCode == "PENDING")?.Count ?? 0,
                    progressCount = stats.FirstOrDefault(s => s.StatusCode == "PROGRESS")?.Count ?? 0,
                    resolvedCount = stats.FirstOrDefault(s => s.StatusCode == "RESOLVED")?.Count ?? 0
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "載入統計數據時發生錯誤", error = ex.Message });
            }
        }

        /// <summary>
        /// 系統訊息列表頁面
        /// 需要 system_message_list 權限才能存取
        /// </summary>
        [RequireAdminPermission(AdminPermissions.SystemMessageList)]
        public IActionResult admin_systemMessageList()
        {
            var viewModel = new AdminSystemMessageListViewModel(_context);
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
        /// 取得系統訊息模板 API
        /// 需要 system_message_new 權限才能存取
        /// </summary>
        [HttpGet]
        [RequireAdminPermission(AdminPermissions.SystemMessageNew)]
        public async Task<IActionResult> GetSystemMessageTemplates()
        {
            try
            {
                var templates = await _context.AdminMessageTemplates
                    .Where(t => t.CategoryCode == "SYSTEM_MESSAGE" && t.IsActive)
                    .OrderBy(t => t.Title)
                    .Select(t => new
                    {
                        TemplateID = t.TemplateId,
                        Title = t.Title,
                        TemplateContent = t.TemplateContent,
                        ContentPreview = t.TemplateContent.Length > 50 
                            ? t.TemplateContent.Substring(0, 50) + "..." 
                            : t.TemplateContent,
                        CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = templates
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "載入系統訊息模板時發生錯誤",
                    error = ex.Message
                });
            }
        }



        /// <summary>
        /// 發送系統訊息 API
        /// 需要 system_message_new 權限才能存取
        /// </summary>
        [HttpPost]
        [RequireAdminPermission(AdminPermissions.SystemMessageNew)]
        public async Task<IActionResult> SendSystemMessage([FromForm] SendSystemMessageRequest request)
        {
            try
            {
                // 偵錯：記錄收到的請求
                System.Diagnostics.Debug.WriteLine($"SendSystemMessage called with: Title={request.MessageTitle}, Content={request.MessageContent}, AudienceType={request.AudienceType}, Category={request.MessageCategory}");
                
                // 驗證請求資料
                if (string.IsNullOrWhiteSpace(request.MessageTitle))
                {
                    return Json(new { success = false, message = "訊息標題不能為空" });
                }
                if (string.IsNullOrWhiteSpace(request.MessageContent))
                {
                    return Json(new { success = false, message = "訊息內容不能為空" });
                }
                if (string.IsNullOrWhiteSpace(request.MessageCategory))
                {
                    return Json(new { success = false, message = "訊息分類不能為空" });
                }
                if (string.IsNullOrWhiteSpace(request.AudienceType))
                {
                    return Json(new { success = false, message = "發送對象類型不能為空" });
                }

                // 取得當前管理員ID
                var currentAdminIdStr = GetCurrentAdminId();
                System.Diagnostics.Debug.WriteLine($"Current Admin ID string: {currentAdminIdStr}");
                
                if (string.IsNullOrEmpty(currentAdminIdStr) || !int.TryParse(currentAdminIdStr, out int currentAdminId) || currentAdminId == 0)
                {
                    return Json(new { success = false, message = "未取得有效的管理員身分" });
                }
                
                // 驗證管理員是否存在
                var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == currentAdminId);
                System.Diagnostics.Debug.WriteLine($"Current Admin ID: {currentAdminId}, Exists: {adminExists}");
                
                if (!adminExists)
                {
                    return Json(new { success = false, message = $"管理員不存在於資料庫中 (ID: {currentAdminId})" });
                }

                // 根據發送對象類型決定接收者
                List<int> receiverIds = new List<int>();
                string receiverName = "";

                switch (request.AudienceType)
                {
                    case "ALL_MEMBERS":
                        receiverIds = await _context.Members
                            .Where(m => m.IsActive && !m.IsLandlord)
                            .Select(m => m.MemberId)
                            .ToListAsync();
                        receiverName = "全體會員";
                        break;
                        
                    case "ALL_LANDLORDS":
                        receiverIds = await _context.Members
                            .Where(m => m.IsActive && m.IsLandlord)
                            .Select(m => m.MemberId)
                            .ToListAsync();
                        receiverName = "全體房東";
                        break;
                        
                    case "INDIVIDUAL":
                        if (request.SelectedUserId.HasValue)
                        {
                            var selectedMember = await _context.Members
                                .FirstOrDefaultAsync(m => m.MemberId == request.SelectedUserId.Value);
                            if (selectedMember != null)
                            {
                                receiverIds.Add(selectedMember.MemberId);
                                receiverName = selectedMember.MemberName;
                            }
                            else
                            {
                                return Json(new { success = false, message = "找不到指定的用戶" });
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = "未選擇個別用戶" });
                        }
                        break;
                        
                    default:
                        return Json(new { success = false, message = "無效的發送對象類型" });
                }

                if (!receiverIds.Any())
                {
                    return Json(new { success = false, message = "未找到符合條件的接收者" });
                }

                System.Diagnostics.Debug.WriteLine($"Found {receiverIds.Count} receivers for audience type: {request.AudienceType}");
                
                // 只有個別用戶時才需要驗證接收者ID
                if (request.AudienceType == "INDIVIDUAL")
                {
                    // 驗證指定的接收者是否存在於資料庫中
                    var validReceiverIds = await _context.Members
                        .Where(m => receiverIds.Contains(m.MemberId))
                        .Select(m => m.MemberId)
                        .ToListAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"Valid receiver IDs count: {validReceiverIds.Count}");
                    
                    if (validReceiverIds.Count != receiverIds.Count)
                    {
                        var invalidIds = receiverIds.Except(validReceiverIds).ToList();
                        System.Diagnostics.Debug.WriteLine($"Invalid receiver IDs: {string.Join(", ", invalidIds)}");
                        return Json(new { success = false, message = $"指定的接收者不存在於資料庫中: {string.Join(", ", invalidIds)}" });
                    }
                    
                    // 更新 receiverIds 為已驗證的IDs
                    receiverIds = validReceiverIds;
                }
                // 群發訊息不需要驗證個別ID，只需要確認有符合條件的用戶存在

                // 建立系統訊息記錄 - 統一為單筆記錄
                var systemMessage = new SystemMessage
                {
                    Title = request.MessageTitle,
                    MessageContent = request.MessageContent,
                    CategoryCode = request.MessageCategory,
                    AudienceTypeCode = request.AudienceType,
                    ReceiverId = request.AudienceType == "INDIVIDUAL" ? receiverIds.First() : null, // 群發時為 NULL
                    AdminId = currentAdminId,
                    SentAt = DateTime.Now,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                System.Diagnostics.Debug.WriteLine($"Creating system message: AudienceType={request.AudienceType}, ReceiverId={systemMessage.ReceiverId}");

                // 新增單筆記錄到資料庫
                System.Diagnostics.Debug.WriteLine("Adding 1 system message to database");
                
                // 先檢查資料庫狀態
                var beforeCount = await _context.SystemMessages.CountAsync();
                System.Diagnostics.Debug.WriteLine($"SystemMessages count before insert: {beforeCount}");
                
                _context.SystemMessages.Add(systemMessage);
                
                // 檢查是否有變更待處理
                var hasChanges = _context.ChangeTracker.HasChanges();
                System.Diagnostics.Debug.WriteLine($"ChangeTracker.HasChanges: {hasChanges}");
                
                // 檢查追蹤的實體
                var addedEntities = _context.ChangeTracker.Entries()
                    .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added)
                    .ToList();
                System.Diagnostics.Debug.WriteLine($"Added entities count: {addedEntities.Count}");
                
                var changeCount = await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"SaveChangesAsync returned: {changeCount}");
                
                // 檢查儲存後的狀態
                var afterCount = await _context.SystemMessages.CountAsync();
                System.Diagnostics.Debug.WriteLine($"SystemMessages count after insert: {afterCount}");

                var responseMessage = $"系統訊息已成功發送給 {receiverName}";

                return Json(new
                {
                    success = true,
                    message = responseMessage,
                    data = new
                    {
                        MessageId = systemMessage.MessageId,
                        MessageCount = 1, // 總是只有1筆記錄
                        ReceiverCount = receiverIds.Count, // 實際影響的用戶數
                        AudienceType = request.AudienceType,
                        ReceiverName = receiverName,
                        ReceiverId = systemMessage.ReceiverId
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SendSystemMessage: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return Json(new
                {
                    success = false,
                    message = "發送系統訊息時發生錯誤",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// 用戶搜尋 API (供個別用戶選擇器使用)
        /// 需要 system_message_new 權限才能存取
        /// </summary>
        [HttpPost]
        [RequireAdminPermission(AdminPermissions.SystemMessageNew)]
        public async Task<IActionResult> SearchMembersForMessage([FromForm] string keyword, [FromForm] string searchField)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return Json(new { success = true, data = new List<object>() });
                }

                var query = _context.Members.Where(m => m.IsActive).AsQueryable();

                switch (searchField?.ToLower())
                {
                    case "membername":
                        query = query.Where(m => m.MemberName.Contains(keyword));
                        break;
                    case "memberid":
                        if (int.TryParse(keyword, out int memberId))
                        {
                            query = query.Where(m => m.MemberId == memberId);
                        }
                        else
                        {
                            return Json(new { success = true, data = new List<object>() });
                        }
                        break;
                    case "email":
                        query = query.Where(m => m.Email.Contains(keyword));
                        break;
                    case "nationalno":
                        query = query.Where(m => m.NationalIdNo != null && m.NationalIdNo.Contains(keyword));
                        break;
                    default:
                        // 全欄位搜尋
                        query = query.Where(m => 
                            m.MemberName.Contains(keyword) ||
                            m.Email.Contains(keyword) ||
                            (m.NationalIdNo != null && m.NationalIdNo.Contains(keyword)));
                        break;
                }

                var members = await query
                    .Take(10) // 限制回傳數量
                    .Select(m => new
                    {
                        MemberId = m.MemberId,
                        MemberName = m.MemberName,
                        Email = m.Email,
                        PhoneNumber = m.PhoneNumber,
                        IsLandlord = m.IsLandlord,
                        AccountType = m.IsLandlord ? "房東" : "租客"
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = members
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "搜尋用戶時發生錯誤",
                    error = ex.Message
                });
            }
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

        /// <summary>
        /// 取得會員身分證上傳檔案 API
        /// 需要 member_details 權限才能存取
        /// </summary>
        [HttpGet]
        [RequireAdminPermission(AdminPermissions.MemberList)]
        public async Task<IActionResult> GetMemberIdentityDocuments(int memberId)
        {
            try
            {
                // 查找會員的身分證驗證申請
                var approval = await _context.Approvals
                    .FirstOrDefaultAsync(a => a.ApplicantMemberId == memberId && 
                                             a.ModuleCode == "IDENTITY" && 
                                             a.StatusCode == "PENDING");

                if (approval == null)
                {
                    return Json(new { success = false, message = "找不到待審核的身分驗證申請" });
                }

                // 查找相關的檔案上傳記錄 - 直接查詢，簡化邏輯
                var uploads = await _context.UserUploads
                    .Where(u => u.MemberId == memberId && 
                               u.ModuleCode == "MemberInfo" && 
                               u.IsActive &&
                               (u.UploadTypeCode == "USER_ID_FRONT" || u.UploadTypeCode == "USER_ID_BACK"))
                    .OrderByDescending(u => u.UploadedAt)
                    .Select(u => new
                    {
                        UploadId = u.UploadId,
                        FileName = u.OriginalFileName,
                        FileUrl = u.FilePath.StartsWith("/") ? u.FilePath : "/" + u.FilePath,
                        UploadTypeCode = u.UploadTypeCode,
                        TypeDisplay = u.UploadTypeCode == "USER_ID_FRONT" ? "身分證正面" : "身分證反面",
                        UploadedAt = u.UploadedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        FileSize = FormatFileSize(u.FileSize),
                        ApprovalIdInfo = u.ApprovalId // 除錯用
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        ApprovalId = approval.ApprovalId,
                        Documents = uploads,
                        // 除錯資訊
                        Debug = new
                        {
                            MemberId = memberId,
                            ApprovalExists = approval != null,
                            UploadCount = uploads.Count,
                            AllUploads = uploads.Select(u => new { u.UploadTypeCode, u.ApprovalIdInfo }).ToList()
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "載入身分證檔案時發生錯誤",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 審核通過身分驗證 API
        /// 需要 member_details 權限才能存取
        /// </summary>
        [HttpPost]
        [RequireAdminPermission(AdminPermissions.MemberList)]
        public async Task<IActionResult> ApproveIdentityVerification([FromForm] int memberId, [FromForm] string nationalIdNo)
        {
            try
            {
                // 驗證身分證字號格式
                if (string.IsNullOrWhiteSpace(nationalIdNo) || nationalIdNo.Length != 10)
                {
                    return Json(new { success = false, message = "身分證字號格式不正確" });
                }

                // 檢查身分證字號是否已被使用
                var existingMember = await _context.Members
                    .FirstOrDefaultAsync(m => m.NationalIdNo == nationalIdNo && m.MemberId != memberId);

                if (existingMember != null)
                {
                    return Json(new { success = false, message = "此身分證字號已被其他會員使用" });
                }

                // 查找會員記錄
                var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == memberId);
                if (member == null)
                {
                    return Json(new { success = false, message = "找不到指定的會員" });
                }

                // 查找待審核的身分驗證申請
                var approval = await _context.Approvals
                    .FirstOrDefaultAsync(a => a.ApplicantMemberId == memberId && 
                                             a.ModuleCode == "IDENTITY" && 
                                             a.StatusCode == "PENDING");

                if (approval == null)
                {
                    return Json(new { success = false, message = "找不到待審核的身分驗證申請" });
                }

                var currentAdminId = int.Parse(GetCurrentAdminId() ?? "0");
                var currentTime = DateTime.Now;

                // 根據規格文件更新會員資料
                member.NationalIdNo = nationalIdNo;
                member.IdentityVerifiedAt = currentTime;

                // 更新審核狀態
                approval.StatusCode = "APPROVED";
                approval.UpdatedAt = currentTime;

                // 新增審核歷程記錄
                var approvalItem = new ApprovalItem
                {
                    ApprovalId = approval.ApprovalId,
                    ActionType = "APPROVED",
                    ActionBy = currentAdminId,
                    ActionNote = $"身分驗證審核通過，身分證字號：{nationalIdNo}",
                    SnapshotJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        MemberId = member.MemberId,
                        MemberName = member.MemberName,
                        NationalIdNo = nationalIdNo,
                        IdentityVerifiedAt = currentTime,
                        ApprovedBy = currentAdminId,
                        ApprovedAt = currentTime
                    }),
                    CreatedAt = currentTime
                };

                _context.ApprovalItems.Add(approvalItem);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "身分驗證審核通過",
                    data = new
                    {
                        MemberId = memberId,
                        MemberName = member.MemberName,
                        NationalIdNo = nationalIdNo,
                        IdentityVerifiedAt = currentTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ApprovedBy = currentAdminId
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "審核身分驗證時發生錯誤",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 拒絕身分驗證 API
        /// 需要 member_details 權限才能存取
        /// </summary>
        [HttpPost]
        [RequireAdminPermission(AdminPermissions.MemberList)]
        public async Task<IActionResult> RejectIdentityVerification([FromForm] int memberId, [FromForm] string rejectReason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rejectReason))
                {
                    return Json(new { success = false, message = "請填寫拒絕原因" });
                }

                // 查找會員記錄
                var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == memberId);
                if (member == null)
                {
                    return Json(new { success = false, message = "找不到指定的會員" });
                }

                // 查找待審核的身分驗證申請
                var approval = await _context.Approvals
                    .FirstOrDefaultAsync(a => a.ApplicantMemberId == memberId && 
                                             a.ModuleCode == "IDENTITY" && 
                                             a.StatusCode == "PENDING");

                if (approval == null)
                {
                    return Json(new { success = false, message = "找不到待審核的身分驗證申請" });
                }

                var currentAdminId = int.Parse(GetCurrentAdminId() ?? "0");
                var currentTime = DateTime.Now;

                // 更新審核狀態
                approval.StatusCode = "REJECTED";
                approval.UpdatedAt = currentTime;

                // 新增審核歷程記錄
                var approvalItem = new ApprovalItem
                {
                    ApprovalId = approval.ApprovalId,
                    ActionType = "REJECT_FINAL",
                    ActionBy = currentAdminId,
                    ActionNote = $"身分驗證審核拒絕：{rejectReason}",
                    SnapshotJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        MemberId = member.MemberId,
                        MemberName = member.MemberName,
                        RejectReason = rejectReason,
                        RejectedBy = currentAdminId,
                        RejectedAt = currentTime
                    }),
                    CreatedAt = currentTime
                };

                _context.ApprovalItems.Add(approvalItem);

                // 根據規格文件，拒絕時不更新會員的身分驗證相關欄位
                // members.identityVerifiedAt 和 nationalIdNo 保持 NULL

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "身分驗證審核已拒絕",
                    data = new
                    {
                        MemberId = memberId,
                        MemberName = member.MemberName,
                        RejectReason = rejectReason,
                        RejectedBy = currentAdminId,
                        RejectedAt = currentTime.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "拒絕身分驗證時發生錯誤",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 格式化檔案大小
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }
    }

    /// <summary>
    /// 發送系統訊息請求模型
    /// </summary>
    public class SendSystemMessageRequest
    {
        public string MessageTitle { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
        public string MessageCategory { get; set; } = string.Empty;
        public string AudienceType { get; set; } = string.Empty;
        public int? SelectedUserId { get; set; }
    }
}
