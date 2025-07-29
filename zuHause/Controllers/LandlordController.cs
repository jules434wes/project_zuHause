using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.ViewModels;
using zuHause.DTOs;
using zuHause.Helpers;
using zuHause.Constants;
using zuHause.Interfaces;
using zuHause.Enums;
using System.Security.Claims;
using System.Diagnostics;

namespace zuHause.Controllers
{
    public class LandlordController : Controller
    {
        private readonly ILogger<LandlordController> _logger;
        private readonly ZuHauseContext _context;
        private readonly IImageQueryService _imageQueryService;
        
        public LandlordController(
            ILogger<LandlordController> logger, 
            ZuHauseContext context,
            IImageQueryService imageQueryService)
        {
            _logger = logger;
            _context = context;
            _imageQueryService = imageQueryService;
        }

        // GET: /landlord/become
        [HttpGet("landlord/become")]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> Become()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Member");
            }
            
            var member = await _context.Members.FindAsync(int.Parse(userId));
            if (member == null)
            {
                return RedirectToAction("Login", "Member");
            }
            
            var viewModel = new LandlordApplicationViewModel
            {
                MemberName = member.MemberName,
                IsAlreadyLandlord = member.IsLandlord,
                CanApply = CheckEligibility(member, out string errorMessage),
                ErrorMessage = errorMessage
            };
            
            return View(viewModel);
        }
        
        // POST: /landlord/agreetotems
        [HttpPost("landlord/agreetotems")]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> AgreeToTerms()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Member");
            }
            
            var member = await _context.Members.FindAsync(int.Parse(userId));
            if (member == null)
            {
                return RedirectToAction("Login", "Member");
            }
            
            // 再次檢查資格（防止前端繞過）
            if (!CheckEligibility(member, out string errorMessage))
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("Become");
            }
            
            if (!member.IsLandlord)
            {
                member.IsLandlord = true;
                member.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "恭喜您！已成功申請成為房東。";
            }
            
            return RedirectToAction("Become");
        }

        /// <summary>
        /// 房源管理主頁面
        /// </summary>
        [HttpGet("landlord/propertymanagement")]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> PropertyManagement()
        {
            var stopwatch = Stopwatch.StartNew();
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("房東查詢開始 - UserId: {UserId}, Timestamp: {Timestamp}, IP: {IpAddress}", 
                userId, DateTime.UtcNow, HttpContext.Connection.RemoteIpAddress);
            
            try
            {
                // 1. 房東身份驗證
                var landlordId = GetCurrentLandlordId();
                if (landlordId == null)
                {
                    _logger.LogWarning("房東身份驗證失敗 - UserId: {UserId}, Duration: {Duration}ms", 
                        userId, stopwatch.ElapsedMilliseconds);
                    TempData["ErrorMessage"] = "無效的房東身份，請重新登入";
                    return RedirectToAction("Login", "Member");
                }

                // 2. 查詢房東房源（基本資料，不含圖片）
                var properties = await (from p in _context.Properties
                                      join c in _context.Cities on p.CityId equals c.CityId
                                      join d in _context.Districts on p.DistrictId equals d.DistrictId
                                      where p.LandlordMemberId == landlordId.Value && p.DeletedAt == null
                                      orderby p.CreatedAt descending
                                      select new PropertyManagementDto
                                      {
                                          PropertyId = p.PropertyId,
                                          Title = p.Title,
                                          MonthlyRent = p.MonthlyRent,
                                          Address = $"{c.CityName}{d.DistrictName}{p.AddressLine ?? string.Empty}",
                                          StatusCode = p.StatusCode,
                                          RoomCount = p.RoomCount,
                                          LivingRoomCount = p.LivingRoomCount,
                                          BathroomCount = p.BathroomCount,
                                          Area = p.Area,
                                          CurrentFloor = p.CurrentFloor,
                                          TotalFloors = p.TotalFloors,
                                          CreatedAt = p.CreatedAt,
                                          UpdatedAt = p.UpdatedAt,
                                          PublishedAt = p.PublishedAt,
                                          ExpireAt = p.ExpireAt,
                                          IsPaid = p.IsPaid,
                                          ParkingAvailable = p.ParkingAvailable,
                                          ManagementFeeIncluded = p.ManagementFeeIncluded,
                                          ManagementFeeAmount = p.ManagementFeeAmount,
                                          DepositAmount = p.DepositAmount,
                                          DepositMonths = p.DepositMonths,
                                          MinimumRentalMonths = p.MinimumRentalMonths,
                                          // 直接讀取資料庫中的 PreviewImageUrl，如果是相對路徑則轉為完整 URL
                                          ThumbnailUrl = !string.IsNullOrEmpty(p.PreviewImageUrl) && !p.PreviewImageUrl.StartsWith("http") 
                                              ? $"https://zuhauseimg.blob.core.windows.net/zuhaus-images/{p.PreviewImageUrl}" 
                                              : p.PreviewImageUrl ?? "/images/default-picture.png",
                                          ImageUrls = new List<string>(),
                                          // 統計資料稍後批量查詢
                                          ViewCount = 0,
                                          FavoriteCount = 0,
                                          ApplicationCount = 0
                                      }).ToListAsync();

                // 3. 為沒有 PreviewImageUrl 的房源動態查詢主圖
                var propertyIds = properties.Select(p => p.PropertyId).ToList();
                
                // 批量查詢主圖資料
                var primaryImages = await _context.Images
                    .Where(img => propertyIds.Contains(img.EntityId) &&
                                  img.EntityType == EntityType.Property &&
                                  img.Category == ImageCategory.Gallery &&
                                  img.DisplayOrder == 1)
                    .Select(img => new { img.EntityId, img.StoredFileName })
                    .ToListAsync();

                var primaryImageDict = primaryImages.ToDictionary(img => img.EntityId, img => img.StoredFileName);

                // 更新沒有 PreviewImageUrl 的房源的 ThumbnailUrl
                foreach (var property in properties)
                {
                    if (property.ThumbnailUrl == "/images/default-picture.png" && 
                        primaryImageDict.TryGetValue(property.PropertyId, out var storedFileName))
                    {
                        try
                        {
                            var dynamicUrl = _imageQueryService.GenerateImageUrl(storedFileName, ImageSize.Medium);
                            if (!string.IsNullOrEmpty(dynamicUrl))
                            {
                                property.ThumbnailUrl = dynamicUrl;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("生成房源 {PropertyId} 主圖URL失敗: {Error}", 
                                property.PropertyId, ex.Message);
                        }
                    }
                }

                // 4. 批量載入統計資料（效能優化）
                var statisticsDict = await LoadPropertyStatistics(propertyIds);
                foreach (var property in properties)
                {
                    if (statisticsDict.TryGetValue(property.PropertyId, out var propStats))
                    {
                        property.ViewCount = propStats.ViewCount;
                        property.FavoriteCount = propStats.FavoriteCount;
                        property.ApplicationCount = propStats.ApplicationCount;
                    }
                }

                // 6. 建立統計資料（簡化版）
                var stats = new PropertyManagementStatsDto
                {
                    TotalProperties = properties.Count()
                };

                // 7. 狀態摘要統計
                var statusSummary = properties
                    .GroupBy(p => p.StatusCode)
                    .ToDictionary(g => g.Key, g => g.Count());

                // 8. 建立回應 DTO
                var response = new PropertyManagementListResponseDto
                {
                    Properties = properties,
                    TotalCount = properties.Count(),
                    Stats = stats,
                    StatusSummary = statusSummary
                };

                stopwatch.Stop();
                
                // 查詢成功日誌
                _logger.LogInformation("房東查詢成功 - UserId: {UserId}, LandlordId: {LandlordId}, Count: {PropertyCount}, Duration: {Duration}ms",
                    userId, landlordId, properties.Count, stopwatch.ElapsedMilliseconds);

                return View(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // 查詢失敗日誌
                _logger.LogError(ex, "房東查詢失敗 - UserId: {UserId}, Duration: {Duration}ms, Error: {ErrorMessage}",
                    userId, stopwatch.ElapsedMilliseconds, ex.Message);
                    
                TempData["ErrorMessage"] = "載入房源資料時發生錯誤，請稍後再試";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// 審核不通過資料補充頁面
        /// </summary>
        [HttpGet("property/supplement/{propertyId:int}")]
        [Authorize(AuthenticationSchemes = "MemberCookieAuth")]
        public async Task<IActionResult> PropertySupplement(int propertyId)
        {
            try
            {
                // 1. 房東身份驗證
                var landlordId = GetCurrentLandlordId();
                if (landlordId == null)
                {
                    return Unauthorized("無效的房東身份");
                }

                // 2. 查詢房源並驗證狀態
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId 
                                           && p.LandlordMemberId == landlordId.Value
                                           && p.DeletedAt == null);

                if (property == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的房源";
                    return RedirectToAction("PropertyManagement");
                }

                // 3. 驗證房源狀態（僅限 REJECT_REVISE）
                if (property.StatusCode != PropertyStatusConstants.REJECT_REVISE)
                {
                    TempData["ErrorMessage"] = "此房源不需要補充資料";
                    return RedirectToAction("PropertyManagement");
                }

                // 4. TODO: 查詢審核不通過的具體原因
                // 這部分需要從 approvals 和 approvalItems 表查詢具體的補件要求

                return View(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入房源補充頁面失敗，房源ID: {PropertyId}", propertyId);
                TempData["ErrorMessage"] = "載入頁面時發生錯誤";
                return RedirectToAction("PropertyManagement");
            }
        }

        /// <summary>
        /// 取得當前房東ID
        /// 驗證房東身份：IsLandlord == true && MemberTypeId == 2 && IdentityVerifiedAt != null
        /// </summary>
        private int? GetCurrentLandlordId()
        {
            // 從 MemberCookieAuth 取得會員ID
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out var memberId))
                return null;

            // 驗證是否為已驗證的房東
            var member = _context.Members.FirstOrDefault(m => 
                m.MemberId == memberId 
                && m.IsLandlord 
                && m.MemberTypeId == 2 
                && m.IdentityVerifiedAt != null);

            return member?.MemberId;
        }
        
        private bool CheckEligibility(Member member, out string errorMessage)
        {
            if (!member.IsActive)
            {
                errorMessage = "很抱歉，您不符合申請資格。";
                return false;
            }
            
            if (member.IdentityVerifiedAt == null || 
                member.PhoneVerifiedAt == null || 
                string.IsNullOrEmpty(member.NationalIdNo))
            {
                errorMessage = "請先完善會員資料後再進行申請。";
                return false;
            }
            
            errorMessage = string.Empty;
            return true;
        }
        
        // === 稽核日誌和安全檢查方法 ===
        
        /// <summary>
        /// 安全的用戶ID取得
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogError("安全錯誤 - 無法取得有效的用戶ID, IP: {IpAddress}", 
                    HttpContext.Connection.RemoteIpAddress);
                throw new UnauthorizedAccessException("無效的用戶認證");
            }
            
            return userId;
        }
        
        /// <summary>
        /// 驗證房東對房源的所有權
        /// </summary>
        private async Task<bool> ValidatePropertyOwnership(int propertyId, int userId)
        {
            var property = await _context.Properties
                .Where(p => p.PropertyId == propertyId && p.LandlordMemberId == userId)
                .FirstOrDefaultAsync();
                
            if (property == null)
            {
                _logger.LogWarning("安全警告 - 用戶嘗試存取非本人房源，UserId: {UserId}, PropertyId: {PropertyId}, IP: {IpAddress}",
                    userId, propertyId, HttpContext.Connection.RemoteIpAddress);
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 驗證用戶房東權限
        /// </summary>
        private async Task<bool> ValidateLandlordPermission(int userId)
        {
            var member = await _context.Members
                .Where(m => m.MemberId == userId && m.IsLandlord == true)
                .FirstOrDefaultAsync();
                
            if (member == null)
            {
                _logger.LogWarning("安全警告 - 非房東用戶嘗試存取房東功能，UserId: {UserId}, IP: {IpAddress}",
                    userId, HttpContext.Connection.RemoteIpAddress);
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 房源狀態變更稽核日誌
        /// </summary>
        private async Task LogPropertyStatusAudit(int propertyId, string oldStatus, string newStatus, int userId)
        {
            var auditLog = new
            {
                PropertyId = propertyId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            
            _logger.LogWarning("房源狀態變更稽核 {@AuditLog}", auditLog);
        }
        
        /// <summary>
        /// 房源編輯操作追蹤
        /// </summary>
        private void LogPropertyEdit(int propertyId, int userId, string changes)
        {
            _logger.LogInformation("房源編輯追蹤 - PropertyId: {PropertyId}, UserId: {UserId}, Changes: {Changes}, IP: {IpAddress}",
                propertyId, userId, changes, HttpContext.Connection.RemoteIpAddress);
        }
        
        /// <summary>
        /// 批量載入房源統計資料
        /// 使用聯合查詢避免 N+1 問題
        /// </summary>
        private async Task<Dictionary<int, PropertyStatistics>> LoadPropertyStatistics(List<int> propertyIds)
        {
            try
            {
                // 基於現有資料結構的基本統計（如果統計表格不存在，使用預設值）
                var result = new Dictionary<int, PropertyStatistics>();
                
                foreach (var propertyId in propertyIds)
                {
                    // 暫時使用預設值，待統計表格建立後可改為真實查詢
                    result[propertyId] = new PropertyStatistics
                    {
                        ViewCount = 0, // 待實作：需要 PropertyViews 表格
                        FavoriteCount = 0, // 待實作：需要 Favorites 表格  
                        ApplicationCount = 0 // 待實作：需要 RentalApplications 表格
                    };
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "載入房源統計資料時發生錯誤，將使用預設值");
                
                // 如果統計表格不存在，返回預設值
                return propertyIds.ToDictionary(
                    id => id,
                    id => new PropertyStatistics { ViewCount = 0, FavoriteCount = 0, ApplicationCount = 0 });
            }
        }
        
        /// <summary>
        /// 房源統計資料 DTO
        /// </summary>
        private class PropertyStatistics
        {
            public int ViewCount { get; set; }
            public int FavoriteCount { get; set; }
            public int ApplicationCount { get; set; }
        }
    }
} 