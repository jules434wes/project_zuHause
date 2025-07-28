using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.DTOs;
using zuHause.Interfaces;
using zuHause.Helpers;
using zuHause.Enums;
using System.ComponentModel.DataAnnotations;
using zuHause.DTOs.GoogleMaps;

namespace zuHause.Controllers
{
    [ApiController]
    [Route("api/properties")]
    public class PropertyApiController : ControllerBase
    {
        private readonly ZuHauseContext _context;
        private readonly IImageUploadService _imageUploadService;
        private readonly IBlobMigrationService _blobMigrationService;
        private readonly ITempSessionService _tempSessionService;
        private readonly IPropertyImageService _propertyImageService;
        private readonly IImageQueryService _imageQueryService;
        private readonly IGoogleMapsService _googleMapsService;
        private readonly ILogger<PropertyApiController> _logger;

        public PropertyApiController(
            ZuHauseContext context,
            IImageUploadService imageUploadService,
            IBlobMigrationService blobMigrationService,
            ITempSessionService tempSessionService,
            IPropertyImageService propertyImageService,
            IImageQueryService imageQueryService,
            IGoogleMapsService googleMapsService,
            ILogger<PropertyApiController> logger)
        {
            _context = context;
            _imageUploadService = imageUploadService;
            _blobMigrationService = blobMigrationService;
            _tempSessionService = tempSessionService;
            _propertyImageService = propertyImageService;
            _imageQueryService = imageQueryService;
            _googleMapsService = googleMapsService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<ActionResult<PropertySearchResultDto>> SearchProperties(
            [FromQuery] string? location = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? propertyType = null,
            [FromQuery] string? keywords = null,
            [FromQuery] string sortBy = "newest",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Properties
                    .Where(p => p.StatusCode == "上架"); // 使用 StatusCode 而非 IsActive

                // Location filter - 需要包含 City 和 District 的 Include
                if (!string.IsNullOrEmpty(location))
                {
                    query = query.Where(p => 
                        p.AddressLine != null && p.AddressLine.Contains(location));
                }

                // Price range filter
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.MonthlyRent >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.MonthlyRent <= maxPrice.Value);
                }

                // Property type filter - 需要檢查 Property model 是否有 PropertyType 欄位
                if (!string.IsNullOrEmpty(propertyType))
                {
                    // 暫時註解，因為 Property model 中沒有看到 PropertyType 欄位
                    // query = query.Where(p => p.PropertyType == propertyType);
                }

                // Keywords search
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(p => 
                        p.Title.Contains(keywords) ||
                        (p.Description != null && p.Description.Contains(keywords)) ||
                        (p.AddressLine != null && p.AddressLine.Contains(keywords)));
                }

                // Sorting
                query = sortBy.ToLower() switch
                {
                    "priceasc" => query.OrderBy(p => p.MonthlyRent),
                    "pricedesc" => query.OrderByDescending(p => p.MonthlyRent),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    "oldest" => query.OrderBy(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var properties = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 批量查詢主圖（避免 N+1 問題）
                var propertyIds = properties.Select(p => p.PropertyId).ToList();
                var mainImages = new Dictionary<int, Image>();

                foreach (var propertyId in propertyIds)
                {
                    var mainImage = await _imageQueryService.GetMainImageAsync(EntityType.Property, propertyId);
                    if (mainImage != null)
                    {
                        mainImages[propertyId] = mainImage;
                    }
                }

                var result = new PropertySearchResultDto
                {
                    Properties = properties.Select(p => new PropertyDto
                    {
                        Id = p.PropertyId,
                        Title = p.Title,
                        Location = "台北市", // 暫時固定值，需要查詢 City 和 District 表
                        Address = p.AddressLine ?? "",
                        Price = p.MonthlyRent,
                        PropertyType = "一般", // 暫時固定值，需要確認 Property model 中的房型欄位
                        MainImageUrl = mainImages.ContainsKey(p.PropertyId) 
                            ? _imageQueryService.GenerateImageUrl(mainImages[p.PropertyId].StoredFileName, ImageSize.Medium)
                            : "/images/placeholder.jpg",
                        Bedrooms = p.RoomCount,
                        Bathrooms = p.BathroomCount,
                        Area = p.Area,
                        IsFeatured = false, // 暫時固定值，需要確認 Property model 中的精選欄位
                        CreateTime = p.CreatedAt
                    }).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "搜尋房源時發生錯誤", error = ex.Message });
            }
        }

        [HttpPost("{propertyId}/applications")]
        public async Task<ActionResult<ApplicationDto>> CreateApplication(
            int propertyId,
            [FromBody] CreateApplicationDto createDto)
        {
            try
            {
                // Validate property exists and is active
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId && p.StatusCode == "上架");

                if (property == null)
                {
                    return NotFound(new { message = "房源不存在或已下架" });
                }

                // 取得當前登入使用者 ID
                var applicantId = await GetCurrentUserIdAsync();
                if (applicantId == null)
                {
                    return Unauthorized(new { message = "請先登入才能申請房源" });
                }

                // Check for duplicate application
                var existingApplication = await _context.RentalApplications
                    .FirstOrDefaultAsync(ra => ra.PropertyId == propertyId && ra.MemberId == applicantId.Value);

                if (existingApplication != null)
                {
                    return BadRequest(new { message = "您已經申請過這個房源" });
                }

                // Create new application
                var application = new RentalApplication
                {
                    PropertyId = propertyId,
                    MemberId = applicantId.Value,
                    ApplicationType = "租屋申請", // 根據 RentalApplication model
                    CurrentStatus = "待審核", // 使用 CurrentStatus 而非 Status
                    Message = createDto.Message,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.RentalApplications.Add(application);
                await _context.SaveChangesAsync();

                var resultDto = new ApplicationDto
                {
                    ApplicationId = application.ApplicationId,
                    PropertyId = application.PropertyId,
                    ApplicantId = application.MemberId,
                    ApplicationDate = application.CreatedAt, // 使用 CreatedAt
                    Status = application.CurrentStatus,
                    Message = application.Message
                };

                return CreatedAtAction(nameof(GetApplication), new { id = application.ApplicationId }, resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "申請房源時發生錯誤", error = ex.Message });
            }
        }

        [HttpGet("applications/{id}")]
        public async Task<ActionResult<ApplicationDto>> GetApplication(int id)
        {
            try
            {
                var application = await _context.RentalApplications
                    .FirstOrDefaultAsync(ra => ra.ApplicationId == id);

                if (application == null)
                {
                    return NotFound(new { message = "申請記錄不存在" });
                }

                var dto = new ApplicationDto
                {
                    ApplicationId = application.ApplicationId,
                    PropertyId = application.PropertyId,
                    ApplicantId = application.MemberId,
                    ApplicationDate = application.CreatedAt, // 使用 CreatedAt
                    Status = application.CurrentStatus,
                    Message = application.Message
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得申請記錄時發生錯誤", error = ex.Message });
            }
        }

        /// <summary>
        /// 房源圖片上傳 - 支援雙模式：直接上傳和臨時圖片遷移 (Azure Blob Storage 整合)
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="files">圖片檔案（直接上傳模式）</param>
        /// <param name="tempSessionId">臨時會話ID（遷移模式）</param>
        /// <param name="category">圖片分類（預設為Gallery）</param>
        /// <returns>上傳結果</returns>
        [HttpPost("{propertyId}/images")]
        public async Task<ActionResult<List<ImageDto>>> UploadImages(
            int propertyId,
            [FromForm] IFormFileCollection files,
            [FromForm] string? tempSessionId = null,
            [FromForm] string category = "Gallery")
        {
            try
            {
                // 驗證房源是否存在
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    return NotFound(new { message = "房源不存在" });
                }

                // TODO: 驗證房源所有權
                // var currentUserId = await GetCurrentUserIdAsync();
                // if (property.LandlordMemberId != currentUserId) return Forbid();

                // 解析圖片分類
                if (!Enum.TryParse<ImageCategory>(category, true, out var imageCategory))
                {
                    return BadRequest(new { message = $"無效的圖片分類: {category}" });
                }

                List<ImageUploadResult> uploadResults;

                // 雙模式處理：臨時圖片遷移 vs 直接上傳
                if (!string.IsNullOrEmpty(tempSessionId))
                {
                    // 模式1: 臨時圖片遷移
                    var isValidSession = await _tempSessionService.IsValidTempSessionAsync(tempSessionId);
                    if (!isValidSession)
                    {
                        return BadRequest(new { message = "無效的臨時會話ID" });
                    }

                    // 取得臨時圖片列表
                    var tempImages = await _tempSessionService.GetTempImagesAsync(tempSessionId);
                    var imageGuids = tempImages.Select(img => img.ImageGuid).ToList();

                    if (!imageGuids.Any())
                    {
                        return BadRequest(new { message = "沒有可遷移的臨時圖片" });
                    }

                    // 使用 MoveTempToPermanentAsync 方法
                    var migrationResult = await _blobMigrationService.MoveTempToPermanentAsync(
                        tempSessionId, 
                        imageGuids,
                        imageCategory,
                        propertyId
                    );

                    if (!migrationResult.IsSuccess)
                    {
                        return BadRequest(new { message = migrationResult.ErrorMessage });
                    }

                    // 轉換為 ImageUploadResult 格式 (從遷移結果轉換)
                    uploadResults = new List<ImageUploadResult>();
                    foreach (var detail in migrationResult.Details)
                    {
                        var imageGuid = detail.Key;
                        var sizeResults = detail.Value;
                        var isSuccess = sizeResults.Values.All(r => r.Success);
                        
                        // 建立對應的 ImageUploadResult
                        var uploadResult = new ImageUploadResult
                        {
                            Success = isSuccess,
                            ImageGuid = imageGuid,
                            EntityType = EntityType.Property,
                            EntityId = propertyId,
                            Category = imageCategory,
                            OriginalFileName = $"migrated-{imageGuid}.webp",
                            StoredFileName = sizeResults.ContainsKey(ImageSize.Original) ? 
                                sizeResults[ImageSize.Original].BlobPath ?? "" : "",
                            ErrorMessage = isSuccess ? null : 
                                string.Join(", ", sizeResults.Values.Where(r => !r.Success).Select(r => r.Message))
                        };
                        
                        uploadResults.Add(uploadResult);
                    }
                    _logger.LogInformation("完成臨時圖片遷移: PropertyId={PropertyId}, TempSessionId={TempSessionId}, Count={Count}", 
                        propertyId, tempSessionId, uploadResults.Count);
                }
                else if (files != null && files.Count > 0)
                {
                    // 模式2: 直接上傳
                    uploadResults = await _imageUploadService.UploadImagesAsync(
                        files, 
                        EntityType.Property, 
                        propertyId, 
                        imageCategory,
                        skipEntityValidation: false
                    );

                    _logger.LogInformation("完成直接圖片上傳: PropertyId={PropertyId}, Count={Count}", 
                        propertyId, uploadResults.Count);
                }
                else
                {
                    return BadRequest(new { message = "請提供檔案或有效的臨時會話ID" });
                }

                // 處理上傳結果
                var successResults = uploadResults.Where(r => r.Success).ToList();
                var failedResults = uploadResults.Where(r => !r.Success).ToList();

                if (!successResults.Any())
                {
                    return BadRequest(new { 
                        message = "所有圖片上傳失敗", 
                        errors = failedResults.Select(r => new { 
                            fileName = r.OriginalFileName, 
                            error = r.ErrorMessage 
                        })
                    });
                }

                // 建構回應資料
                var imageDtos = successResults.Select(r => new ImageDto
                {
                    ImageId = (int)(r.ImageId ?? 0),
                    ImageUrl = r.StoredFileName ?? "",
                    IsMainImage = r.IsMainImage,
                    UploadTime = DateTime.Now
                }).ToList();

                var response = new
                {
                    success = true,
                    message = $"成功上傳 {successResults.Count} 張圖片" + 
                             (failedResults.Any() ? $"，{failedResults.Count} 張失敗" : ""),
                    propertyId = propertyId,
                    category = category,
                    uploadedCount = successResults.Count,
                    failedCount = failedResults.Count,
                    images = imageDtos,
                    failedUploads = failedResults.Any() ? failedResults.Select(r => new { 
                        fileName = r.OriginalFileName, 
                        error = r.ErrorMessage 
                    }).ToList() : null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳房源圖片時發生錯誤，PropertyId: {PropertyId}", propertyId);
                return StatusCode(500, new { message = "上傳圖片時發生系統錯誤" });
            }
        }

        /// <summary>
        /// 創建房源 - API端點
        /// </summary>
        /// <param name="dto">房源創建資料</param>
        /// <returns>創建結果</returns>
        [HttpPost]
        public async Task<ActionResult<PropertyCreateResponseDto>> CreateProperty([FromBody] PropertyCreateDto dto)
        {
            int? currentUserId = null; // 聲明在方法層級
            
            try
            {
                // TODO: 實作真實身份驗證系統
                // 從 Session、JWT Token 或其他認證方式取得當前使用者 ID
                currentUserId = await GetCurrentUserIdAsync();
                
                if (currentUserId == null)
                {
                    return Unauthorized(new PropertyCreateResponseDto
                    {
                        Success = false,
                        Message = "請先登入才能創建房源"
                    });
                }
                
                // 驗證使用者是否為房東
                var isLandlord = await IsUserLandlordAsync(currentUserId.Value);
                if (!isLandlord)
                {
                    return Forbid("只有房東會員才能創建房源");
                }

                // Model 驗證
                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        );

                    return BadRequest(new PropertyCreateResponseDto
                    {
                        Success = false,
                        Message = "輸入資料驗證失敗",
                        ValidationErrors = validationErrors
                    });
                }

                // 後端驗證
                var validationResult = await ValidatePropertyCreateDto(dto);
                if (!validationResult.IsValid)
                {
                    var validationErrors = validationResult.Errors
                        .GroupBy(e => e.PropertyName ?? "General")
                        .ToDictionary(
                            g => g.Key, 
                            g => g.Select(e => e.ErrorMessage).ToList()
                        );

                    return BadRequest(new PropertyCreateResponseDto
                    {
                        Success = false,
                        Message = "資料驗證失敗",
                        ValidationErrors = validationErrors
                    });
                }

                // 開始資料庫事務
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 建立房源基本資料
                    var property = await CreatePropertyFromDto(dto, currentUserId.Value);
                    _context.Properties.Add(property);
                    await _context.SaveChangesAsync();

                    // 建立設備關聯
                    if (dto.SelectedEquipmentIds.Any())
                    {
                        await CreatePropertyEquipmentRelations(property.PropertyId, dto);
                    }

                    await transaction.CommitAsync();

                    // 計算預計下架日期
                    var listingPlan = await _context.ListingPlans
                        .FirstAsync(lp => lp.PlanId == dto.ListingPlanId);

                    var response = new PropertyCreateResponseDto
                    {
                        Success = true,
                        PropertyId = property.PropertyId,
                        Message = "房源創建成功",
                        Status = property.StatusCode,
                        CreatedAt = property.CreatedAt,
                        TotalListingFee = property.ListingFeeAmount,
                        ExpectedExpireDate = property.ExpireAt
                    };

                    _logger.LogInformation("成功創建房源 (API)，房源ID: {PropertyId}, 房東ID: {LandlordId}", 
                        property.PropertyId, currentUserId.Value);

                    return CreatedAtAction(nameof(GetProperty), new { id = property.PropertyId }, response);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建房源時發生錯誤 (API)，房東ID: {LandlordId}", currentUserId?.ToString() ?? "Unknown");
                return StatusCode(500, new PropertyCreateResponseDto
                {
                    Success = false,
                    Message = "創建房源時發生系統錯誤"
                });
            }
        }

        /// <summary>
        /// 獲取單一房源資訊 - 用於 CreatedAtAction 的參考
        /// </summary>
        /// <param name="id">房源ID</param>
        /// <returns>房源資訊</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PropertyDto>> GetProperty(int id)
        {
            try
            {
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == id);

                if (property == null)
                {
                    return NotFound(new { message = "找不到指定的房源" });
                }

                // 查詢主圖
                var mainImage = await _imageQueryService.GetMainImageAsync(EntityType.Property, id);

                var propertyDto = new PropertyDto
                {
                    Id = property.PropertyId,
                    Title = property.Title,
                    Location = "台北市", // TODO: 查詢城市名稱
                    Address = property.AddressLine ?? "",
                    Price = property.MonthlyRent,
                    PropertyType = "一般", // TODO: 增加房型欄位
                    MainImageUrl = mainImage != null 
                        ? _imageQueryService.GenerateImageUrl(mainImage.StoredFileName, ImageSize.Medium)
                        : "/images/placeholder.jpg",
                    Bedrooms = property.RoomCount,
                    Bathrooms = property.BathroomCount,
                    Area = property.Area,
                    IsFeatured = false, // TODO: 增加精選欄位
                    CreateTime = property.CreatedAt
                };

                return Ok(propertyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取房源資訊時發生錯誤，房源ID: {PropertyId}", id);
                return StatusCode(500, new { message = "獲取房源資訊時發生錯誤" });
            }
        }

        /// <summary>
        /// 上傳房源圖片 - 整合雙語分類系統 (API)
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="files">圖片檔案</param>
        /// <param name="chineseCategory">中文分類</param>
        /// <returns>上傳結果</returns>
        [HttpPost("{propertyId:int}/images/chinese-category")]
        public async Task<ActionResult> UploadPropertyImages(
            int propertyId,
            [FromForm] IFormFileCollection files,
            [FromForm] string chineseCategory = "總覽")
        {
            try
            {
                // 驗證房源是否存在
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    return NotFound(new { success = false, message = "找不到指定的房源" });
                }

                // TODO: 驗證房源所有權
                // if (property.LandlordMemberId != currentUserId) return Forbid();

                // 驗證圖片檔案
                if (files == null || !files.Any())
                {
                    return BadRequest(new { success = false, message = "請選擇要上傳的圖片檔案" });
                }

                // 驗證檔案數量限制（每筆房源最多15張）
                var existingImageCount = await _propertyImageService.GetPropertyImageCountAsync(propertyId);
                if (existingImageCount + files.Count > 15)
                {
                    return BadRequest(new { success = false, message = $"每筆房源最多只能上傳15張圖片，目前已有{existingImageCount}張" });
                }

                // 驗證中文分類
                if (!PropertyImageCategoryHelper.IsValidPropertyCategory(chineseCategory))
                {
                    return BadRequest(new { success = false, message = "無效的圖片分類" });
                }

                // 使用PropertyImageService上傳圖片（支援中文分類）
                var uploadResults = await _propertyImageService.UploadPropertyImagesByChineseCategoryAsync(
                    propertyId, 
                    files, 
                    chineseCategory
                );

                var successCount = uploadResults.Count(r => r.Success);
                var failureCount = uploadResults.Count(r => !r.Success);

                _logger.LogInformation("房源圖片上傳完成 (API)，房源ID: {PropertyId}, 成功: {SuccessCount}, 失敗: {FailureCount}, 分類: {Category}", 
                    propertyId, successCount, failureCount, chineseCategory);

                return Ok(new 
                { 
                    success = true, 
                    message = $"成功上傳 {successCount} 張圖片" + (failureCount > 0 ? $"，{failureCount} 張失敗" : ""),
                    propertyId = propertyId,
                    uploadedCount = successCount,
                    failedCount = failureCount,
                    category = chineseCategory,
                    englishCategory = PropertyImageCategoryHelper.GetEnglishCategory(chineseCategory).ToString(),
                    results = uploadResults.Select(r => new 
                    {
                        success = r.Success,
                        fileName = r.OriginalFileName,
                        message = r.Success ? "上傳成功" : r.ErrorMessage,
                        imageId = r.Success ? r.PropertyImageId : (int?)null,
                        imagePath = r.Success ? r.OriginalImagePath : null,
                        thumbnailPath = r.Success ? r.ThumbnailImagePath : null
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳房源圖片時發生錯誤 (API)，房源ID: {PropertyId}", propertyId);
                return StatusCode(500, new { success = false, message = "上傳圖片時發生系統錯誤" });
            }
        }

        /// <summary>
        /// 取得房源縮圖 URL（對外簡化 API）
        /// </summary>
        [HttpGet("{id:int}/preview-image")]
        public async Task<ActionResult<PropertyPreviewImageDto>> GetPreviewImage(int id)
        {
            // 1. 讀取房源
            var property = await _context.Properties
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PropertyId == id && p.DeletedAt == null);

            if (property == null)
                return NotFound($"Property {id} not found");

            var url = property.PreviewImageUrl;

            // 2. 若資料庫未存，嘗試即時解析第一張 Gallery 主圖
            if (string.IsNullOrEmpty(url))
            {
                var mainImage = await _context.Images
                    .Where(img => img.EntityId == id && img.EntityType == EntityType.Property && img.Category == ImageCategory.Gallery)
                    .OrderBy(img => img.DisplayOrder)
                    .FirstOrDefaultAsync();

                if (mainImage != null)
                {
                    url = _imageQueryService.GenerateImageUrl(mainImage, ImageSize.Medium);
                }
            }

            if (string.IsNullOrEmpty(url))
                return NotFound("Preview image not available");

            return Ok(new PropertyPreviewImageDto { PropertyId = id, PreviewImageUrl = url });
        }

        // === 私有輔助方法 ===

        /// <summary>
        /// 獲取當前登入使用者的ID
        /// 這裡需要根據實際的認證系統來實作
        /// </summary>
        /// <returns>使用者ID，如果未登入則返回 null</returns>
        private async Task<int?> GetCurrentUserIdAsync()
        {
            // 方式1: 從 Session 取得
            if (HttpContext.Session.TryGetValue("UserId", out var userIdBytes))
            {
                return BitConverter.ToInt32(userIdBytes, 0);
            }
            
            // 方式2: 從 JWT Token 取得 (如果有實作 JWT 認證)
            // var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            // if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            // {
            //     return userId;
            // }
            
            // 方式3: 從 Cookie 或其他認證方式取得
            // if (Request.Cookies.TryGetValue("UserId", out var userIdCookie) && int.TryParse(userIdCookie, out var cookieUserId))
            // {
            //     return cookieUserId;
            // }
            
            // 如果都取不到，返回 null 表示未登入
            return null;
        }

        /// <summary>
        /// 驗證指定使用者是否為房東
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>是否為房東</returns>
        private async Task<bool> IsUserLandlordAsync(int userId)
        {
            var user = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == userId && 
                                         m.IsActive && 
                                         m.IsLandlord && 
                                         m.MemberTypeId == 2);
            
            return user != null;
        }

        /// <summary>
        /// 驗證PropertyCreateDto
        /// </summary>
        private async Task<PropertyValidationResult> ValidatePropertyCreateDto(PropertyCreateDto dto)
        {
            var result = new PropertyValidationResult();

            // 驗證城市和區域的有效性
            var districtExists = await _context.Districts
                .AnyAsync(d => d.DistrictId == dto.DistrictId && 
                              d.CityId == dto.CityId && 
                              d.IsActive == true);

            if (!districtExists)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "DistrictId", 
                    ErrorMessage = "選擇的城市和區域不匹配或已停用" 
                });
            }

            // 驗證樓層邏輯
            if (dto.CurrentFloor > dto.TotalFloors)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "CurrentFloor", 
                    ErrorMessage = "所在樓層不能大於總樓層數" 
                });
            }

            // 驗證管理費邏輯
            if (!dto.ManagementFeeIncluded && !dto.ManagementFeeAmount.HasValue)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "ManagementFeeAmount", 
                    ErrorMessage = "選擇須另計時，管理費金額為必填項目" 
                });
            }

            // 驗證水電費邏輯
            if (dto.WaterFeeType == "自訂金額" && !dto.CustomWaterFee.HasValue)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "CustomWaterFee", 
                    ErrorMessage = "選擇自訂水費時，金額為必填項目" 
                });
            }

            if (dto.ElectricityFeeType == "自訂金額" && !dto.CustomElectricityFee.HasValue)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "CustomElectricityFee", 
                    ErrorMessage = "選擇自訂電費時，金額為必填項目" 
                });
            }

            // 驗證清潔費邏輯
            if (dto.CleaningFeeRequired && !dto.CleaningFeeAmount.HasValue)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "CleaningFeeAmount", 
                    ErrorMessage = "選擇須清潔費時，金額為必填項目" 
                });
            }

            // 驗證停車費邏輯
            if (dto.ParkingFeeRequired && !dto.ParkingFeeAmount.HasValue)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "ParkingFeeAmount", 
                    ErrorMessage = "選擇停車費須額外收費時，金額為必填項目" 
                });
            }

            // 驗證刊登方案
            var listingPlanExists = await _context.ListingPlans
                .AnyAsync(lp => lp.PlanId == dto.ListingPlanId && lp.IsActive == true);

            if (!listingPlanExists)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "ListingPlanId", 
                    ErrorMessage = "選擇的刊登方案不存在或已停用" 
                });
            }

            // 驗證房源標題和地址組合的唯一性
            var duplicateExists = await _context.Properties
                .AnyAsync(p => p.Title == dto.Title && 
                              p.AddressLine == dto.AddressLine && 
                              p.DeletedAt == null);

            if (duplicateExists)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "Title", 
                    ErrorMessage = "相同標題和地址的房源已存在" 
                });
            }

            return result;
        }

        /// <summary>
        /// 從DTO創建Property實體
        /// </summary>
        private async Task<Property> CreatePropertyFromDto(PropertyCreateDto dto, int landlordMemberId)
        {
            // 獲取刊登方案資訊以計算費用和到期日
            var listingPlan = await _context.ListingPlans
                .FirstAsync(lp => lp.PlanId == dto.ListingPlanId);

            var now = DateTime.Now;
            var listingFee = listingPlan.PricePerDay * listingPlan.MinListingDays;
            var expireDate = now.AddDays(listingPlan.MinListingDays + 1).Date; // 下架日為刊登天數+1天的00:00

            var property = new Property
            {
                LandlordMemberId = landlordMemberId,
                Title = dto.Title,
                Description = dto.Description,
                CityId = dto.CityId,
                DistrictId = dto.DistrictId,
                AddressLine = dto.AddressLine,
                MonthlyRent = dto.MonthlyRent,
                DepositAmount = dto.DepositAmount,
                DepositMonths = dto.DepositMonths,
                RoomCount = dto.RoomCount,
                LivingRoomCount = dto.LivingRoomCount,
                BathroomCount = dto.BathroomCount,
                CurrentFloor = dto.CurrentFloor,
                TotalFloors = dto.TotalFloors,
                Area = dto.Area,
                MinimumRentalMonths = dto.MinimumRentalMonths,
                SpecialRules = dto.SpecialRules,
                WaterFeeType = dto.WaterFeeType,
                CustomWaterFee = dto.CustomWaterFee,
                ElectricityFeeType = dto.ElectricityFeeType,
                CustomElectricityFee = dto.CustomElectricityFee,
                ManagementFeeIncluded = dto.ManagementFeeIncluded,
                ManagementFeeAmount = dto.ManagementFeeAmount,
                ParkingAvailable = dto.ParkingAvailable,
                ParkingFeeRequired = dto.ParkingFeeRequired,
                ParkingFeeAmount = dto.ParkingFeeAmount,
                CleaningFeeRequired = dto.CleaningFeeRequired,
                CleaningFeeAmount = dto.CleaningFeeAmount,
                ListingDays = listingPlan.MinListingDays,
                ListingFeeAmount = listingFee,
                ListingPlanId = dto.ListingPlanId,
                PropertyProofUrl = dto.PropertyProofUrl,
                StatusCode = "PENDING", // 預設為審核中狀態
                IsPaid = false, // 預設未付款
                ExpireAt = expireDate,
                CreatedAt = now,
                UpdatedAt = now
            };

            // === 嘗試 Geocoding ===
            try
            {
                var cityName = await _context.Cities
                    .Where(c => c.CityId == dto.CityId)
                    .Select(c => c.CityName)
                    .FirstOrDefaultAsync();

                var districtName = await _context.Districts
                    .Where(d => d.DistrictId == dto.DistrictId)
                    .Select(d => d.DistrictName)
                    .FirstOrDefaultAsync();

                var fullAddress = $"{cityName}{districtName}{dto.AddressLine}";

                var geoResult = await _googleMapsService.GeocodeAsync(new GeocodingRequest
                {
                    Address = fullAddress
                });

                if (geoResult.IsSuccess && geoResult.Latitude.HasValue && geoResult.Longitude.HasValue)
                {
                    property.Latitude = geoResult.Latitude.Value;
                    property.Longitude = geoResult.Longitude.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Geocoding 失敗，將不設定座標");
            }

            return property;
        }

        /// <summary>
        /// 創建房源設備關聯
        /// </summary>
        private async Task CreatePropertyEquipmentRelations(int propertyId, PropertyCreateDto dto)
        {
            var equipmentRelations = new List<PropertyEquipmentRelation>();

            foreach (var equipmentId in dto.SelectedEquipmentIds)
            {
                var quantity = dto.EquipmentQuantities.TryGetValue(equipmentId, out var qty) ? qty : 1;
                
                equipmentRelations.Add(new PropertyEquipmentRelation
                {
                    PropertyId = propertyId,
                    CategoryId = equipmentId,
                    Quantity = quantity
                });
            }

            _context.PropertyEquipmentRelations.AddRange(equipmentRelations);
            await _context.SaveChangesAsync();
        }
    }
}