using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.ViewModels;
using zuHause.Models;
using zuHause.Interfaces;
using zuHause.Helpers;
using zuHause.Enums;
using zuHause.DTOs;
using zuHause.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace zuHause.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ZuHauseContext _context;
        private readonly ILogger<PropertyController> _logger;
        private readonly IPropertyImageService _propertyImageService;
        private readonly IImageUploadService _imageUploadService;
        private readonly IImageQueryService _imageQueryService;
        private readonly IListingPlanValidationService _listingPlanValidationService;
        private readonly IEquipmentCategoryQueryService _equipmentCategoryQueryService;
        private readonly ITempSessionService _tempSessionService;
        private readonly IBlobMigrationService _blobMigrationService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IGoogleMapsService _googleMapsService;

        public PropertyController(
            ZuHauseContext context, 
            ILogger<PropertyController> logger, 
            IPropertyImageService propertyImageService,
            IImageUploadService imageUploadService,
            IImageQueryService imageQueryService,
            IListingPlanValidationService listingPlanValidationService,
            IEquipmentCategoryQueryService equipmentCategoryQueryService,
            ITempSessionService tempSessionService,
            IBlobMigrationService blobMigrationService,
            IBlobStorageService blobStorageService,
            IGoogleMapsService googleMapsService)
        {
            _context = context;
            _logger = logger;
            _propertyImageService = propertyImageService;
            _imageUploadService = imageUploadService;
            _imageQueryService = imageQueryService;
            _listingPlanValidationService = listingPlanValidationService;
            _equipmentCategoryQueryService = equipmentCategoryQueryService;
            _tempSessionService = tempSessionService;
            _blobMigrationService = blobMigrationService;
            _blobStorageService = blobStorageService;
            _googleMapsService = googleMapsService ?? throw new ArgumentNullException(nameof(googleMapsService));
        }

        /// <summary>
        /// 房源詳細資訊頁面
        /// </summary>
        /// <param name="id">房源 ID</param>
        /// <returns>房源詳細資訊視圖</returns>
        [Route("property/{id:int}")]
        [Route("property/detail/{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
        {
                // 從資料庫載入房源詳細資訊，包含設備分類
                var property = await _context.Properties
                    .Include(p => p.LandlordMember)
                    .Include(p => p.PropertyEquipmentRelations)
                        .ThenInclude(r => r.Category)
                            .ThenInclude(c => c.ParentCategory)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PropertyId == id);

                if (property == null)
                {
                    return NotFound("找不到指定的房源");
                }

                // 取得縣市/區域名稱
                var cityName = await _context.Cities
                    .Where(c => c.CityId == property.CityId)
                    .Select(c => c.CityName)
                    .FirstOrDefaultAsync() ?? string.Empty;

                var districtName = await _context.Districts
                    .Where(d => d.DistrictId == property.DistrictId)
                    .Select(d => d.DistrictName)
                    .FirstOrDefaultAsync() ?? string.Empty;

                // 取得圖片 (使用統一圖片管理系統)
                var images = await _propertyImageService.GetPropertyImagesAsync(id);

                // 建立 ViewModel
                var viewModel = new PropertyDetailViewModel
                {
                    PropertyId = property.PropertyId,
                    Title = property.Title,
                    Description = property.Description ?? string.Empty,
                    Price = property.MonthlyRent,
                    Address = property.AddressLine ?? string.Empty,
                    CityName = cityName,
                    DistrictName = districtName,
                    LandlordName = property.LandlordMember?.MemberName ?? string.Empty,
                    LandlordPhone = property.LandlordMember?.PhoneNumber ?? string.Empty,
                    LandlordEmail = property.LandlordMember?.Email ?? string.Empty,
                    CreatedDate = property.CreatedAt,
                    IsActive = true,
                    IsFavorite = false,
                    ViewCount = 158,
                    FavoriteCount = 23,
                    ApplicationCount = 7,
                    Images = images
                        .OrderBy(img => img.DisplayOrder ?? int.MaxValue)
                        .ThenBy(img => img.ImageId)
                        .Select(img => new ImageDisplayDto
                    {
                        ImageId = (int)img.ImageId,
                        ImagePath = _propertyImageService.GeneratePropertyImageUrl(img.StoredFileName!, ImageSize.Medium),
                        Category = PropertyImageCategoryHelper.GetChineseCategory(img.Category), // 使用中文分類標籤取代檔案名稱
                        IsMainImage = img.DisplayOrder == 1,
                        DisplayOrder = img.DisplayOrder ?? 0
                    }).ToList(),
                    Equipment = property.PropertyEquipmentRelations.Select(eq => new PropertyEquipmentViewModel
                    {
                        EquipmentName = eq.Category.CategoryName,
                        EquipmentType = eq.Category.ParentCategory?.CategoryName ?? eq.Category.CategoryName,
                        Quantity = eq.Quantity,
                        Condition = string.Empty
                    }).ToList(),
                    HouseInfo = new PropertyInfoSection
                    {
                        PropertyType = "公寓",
                        Floor = $"{property.CurrentFloor}/{property.TotalFloors}樓",
                        Area = $"{property.Area}坪",
                        Rooms = $"{property.RoomCount}房",
                        Bathrooms = $"{property.BathroomCount}衛",
                        Balcony = "1個",
                        Parking = property.ParkingAvailable ? "有" : "無",
                        Direction = "朝南",
                        Age = 15
                    },
                    RulesAndFees = new PropertyRulesSection
                    {
                        MonthlyRent = property.MonthlyRent,
                        Deposit = property.DepositAmount,
                        ManagementFee = property.ManagementFeeAmount ?? 0,
                        UtilityDeposit = 3000,
                        LeaseMinimum = "一年",
                        PaymentTerms = "押二付一",
                        HouseRules = new List<string>(),
                        AllowPets = property.SpecialRules?.Contains("寵物") ?? false,
                        AllowSmoking = property.SpecialRules?.Contains("吸菸") ?? false,
                        AllowCooking = property.SpecialRules?.Contains("開伙") ?? false
                    },
                    Location = new PropertyLocationSection
                    {
                        Latitude = 25.0330,
                        Longitude = 121.5654,
                        NearbyTransport = "捷運信義安和站步行5分鐘",
                        NearbySchools = "師大附中、台大",
                        NearbyShopping = "信義商圈、101購物中心",
                        NearbyHospitals = "台大醫院、榮總",
                        NearbyAttractions = new List<string> { "大安森林公園", "信義商圈", "101大樓" }
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入房源詳細資訊時發生錯誤，房源 ID: {PropertyId}", id);
                return View("Error");
            }
        }

        /// <summary>
        /// 房源列表頁面
        /// </summary>
        /// <returns>房源列表視圖</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                var propertyData = await _context.Properties
                    .Include(p => p.LandlordMember)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(20)
                    .Select(p => new { 
                        p.PropertyId, 
                        p.Title, 
                        p.MonthlyRent, 
                        p.AddressLine, 
                        p.CreatedAt 
                    })
                    .ToListAsync();

                var properties = new List<PropertySummaryViewModel>();
                foreach (var p in propertyData)
                {
                    // 查詢房源的主要圖片
                    var images = await _imageQueryService.GetImagesByEntityAsync(
                        EntityType.Property, p.PropertyId);
                    var mainImage = images
                        .Where(img => img.Category == ImageCategory.Gallery)
                        .OrderBy(img => img.DisplayOrder ?? int.MaxValue)
                        .FirstOrDefault();
                    var mainImagePath = mainImage != null 
                        ? _imageQueryService.GenerateImageUrl(mainImage.StoredFileName, ImageSize.Medium)
                        : "/images/default-property.jpg";

                    properties.Add(new PropertySummaryViewModel
                    {
                        PropertyId = p.PropertyId,
                        Title = p.Title,
                        Price = p.MonthlyRent,
                        Address = p.AddressLine ?? "",
                        CityName = "台北市",
                        DistrictName = "大安區",
                        MainImagePath = mainImagePath,
                        CreatedDate = p.CreatedAt,
                        IsFavorite = false,
                        ViewCount = 158
                    });
                }

                var viewModel = new PropertyListViewModel
                {
                    Properties = properties,
                    TotalCount = properties.Count()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入房源列表時發生錯誤");
                return View("Error");
            }
        }

        /// <summary>
        /// 房源創建頁面 (刊登新房源)
        /// </summary>
        [HttpGet("property/new")]
        [HttpGet("property/create")] // 向後相容性保留
        public async Task<IActionResult> Create(bool reset = false)
        {
            // 強制禁用快取
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            _logger.LogInformation("用戶訪問房源創建頁面 - IP: {IpAddress}, Reset: {Reset}", 
                HttpContext.Connection.RemoteIpAddress, reset);
            
            if (reset)
            {
                // 清除可能存在的表單資料暫存
                TempData.Clear();
                
                // 清除臨時會話數據（圖片上傳等）
                try
                {
                    var currentTempSessionId = _tempSessionService.GetOrCreateTempSessionId(HttpContext);
                    if (!string.IsNullOrEmpty(currentTempSessionId))
                    {
                        await _tempSessionService.InvalidateTempSessionAsync(currentTempSessionId);
                        _logger.LogInformation("清除臨時會話數據: {TempSessionId}", currentTempSessionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "清除臨時會話數據時發生警告，但不影響表單載入");
                }
                
                // 清除相關的 Session 數據
                HttpContext.Session.Remove("SelectedPropertyId");
                
                _logger.LogInformation("清除表單暫存資料，建立全新房源表單");
            }
            
            return await BuildPropertyForm(PropertyFormMode.Create);
        }

        /// <summary>
        /// 房源編輯頁面
        /// </summary>
        [HttpGet("property/{id:int}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            // 強制禁用快取
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                _logger.LogWarning("未登入用戶嘗試編輯房源 - PropertyId: {PropertyId}, IP: {IpAddress}", 
                    id, HttpContext.Connection.RemoteIpAddress);
                return RedirectToAction("Login", "Member");
            }
            
            _logger.LogInformation("用戶訪問房源編輯頁面 - PropertyId: {PropertyId}, UserId: {UserId}", 
                id, currentUserId);
            
            return await BuildPropertyForm(PropertyFormMode.Edit, id, currentUserId.Value);
        }

        /// <summary>
        /// 共享的表單建構邏輯
        /// </summary>
        private async Task<IActionResult> BuildPropertyForm(PropertyFormMode mode, int? propertyId = null, int? userId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // 載入基礎資料
                var cities = await GetActiveCitiesAsync();
                var listingPlans = await _listingPlanValidationService.GetActiveListingPlansAsync();
                var equipmentCategoriesHierarchy = await _equipmentCategoryQueryService.GetCategoriesHierarchyAsync();
                
                PropertyCreateDto propertyData = new PropertyCreateDto();
                
                // 編輯模式：載入現有房源資料
                if (mode == PropertyFormMode.Edit && propertyId.HasValue && userId.HasValue)
                {
                    var existingProperty = await LoadExistingPropertyForEdit(propertyId.Value, userId.Value);
                    if (existingProperty == null)
                    {
                        _logger.LogWarning("房源編輯權限驗證失敗 - PropertyId: {PropertyId}, UserId: {UserId}", 
                            propertyId, userId);
                        TempData["ErrorMessage"] = "找不到指定的房源或您無權編輯該房源";
                        return RedirectToAction("PropertyManagement", "Landlord");
                    }
                    propertyData = existingProperty;
                }
                
                // 建構ViewModel
                var viewModel = new PropertyCreateViewModel
                {
                    PropertyData = propertyData,
                    Cities = cities,
                    ListingPlans = listingPlans,
                    EquipmentCategoriesHierarchy = equipmentCategoriesHierarchy,
                    AvailableChineseCategories = PropertyImageCategoryHelper.GetAllPropertyChineseCategories(),
                    FormMode = mode,
                    IsEditMode = mode == PropertyFormMode.Edit
                };
                
                // 設定頁面元資料
                ViewBag.PageTitle = mode == PropertyFormMode.Create ? "刊登新房源" : "編輯房源";
                ViewBag.SubmitText = mode == PropertyFormMode.Create ? "提交審核" : "更新房源";
                ViewBag.FormAction = mode == PropertyFormMode.Create ? "Create" : "Update";
                
                stopwatch.Stop();
                _logger.LogInformation("房源表單建構完成 - Mode: {Mode}, PropertyId: {PropertyId}, Duration: {Duration}ms", 
                    mode, propertyId, stopwatch.ElapsedMilliseconds);
                
                return View("Create", viewModel);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "建構房源表單時發生錯誤 - Mode: {Mode}, PropertyId: {PropertyId}, Duration: {Duration}ms", 
                    mode, propertyId, stopwatch.ElapsedMilliseconds);
                TempData["ErrorMessage"] = "載入頁面時發生錯誤，請稍後再試";
                return RedirectToAction("PropertyManagement", "Landlord");
            }
        }

        /// <summary>
        /// 處理房源創建表單提交
        /// </summary>
        /// <param name="dto">房源創建資料</param>
        /// <returns>創建結果</returns>
        [HttpPost]
        [Route("property/create")]
        public async Task<IActionResult> Create(PropertyCreateDto dto)
        {
            try
            {
                // 🔍 詳細記錄請求開始信息和資料庫連接
                _logger.LogInformation("🏠 PropertyController.Create 方法開始執行");
                _logger.LogInformation("🔍 [DEBUG] 方法開始 - 檢查點 1");
                
                // 🔍 檢查 DTO 是否為 null
                if (dto == null)
                {
                    _logger.LogError("❌ PropertyCreateDto 為 null");
                    TempData["ErrorMessage"] = "請求資料無效";
                    return RedirectToAction("Create");
                }
                
                // 🔍 原始 Request 檢查點
                _logger.LogInformation("🔍 [CHECKPOINT] 原始 Request 檢查:");
                _logger.LogInformation("  - Request.Method: {Method}", Request.Method);
                _logger.LogInformation("  - Request.ContentType: {ContentType}", Request.ContentType ?? "NULL");
                _logger.LogInformation("  - Request.HasFormContentType: {HasFormContentType}", Request.HasFormContentType);
                
                if (Request.HasFormContentType && Request.Form != null)
                {
                    _logger.LogInformation("  - Request.Form.Count: {FormCount}", Request.Form.Count);
                    
                    // 檢查是否有 TempSessionId 表單字段
                    if (Request.Form.ContainsKey("TempSessionId"))
                    {
                        var tempSessionIdValue = Request.Form["TempSessionId"].ToString();
                        _logger.LogInformation("  - Request.Form['TempSessionId']: '{TempSessionIdValue}'", tempSessionIdValue);
                        _logger.LogInformation("  - Request.Form['TempSessionId'] Length: {Length}", tempSessionIdValue?.Length ?? 0);
                    }
                    else
                    {
                        _logger.LogWarning("  - ❌ Request.Form 中沒有找到 'TempSessionId' 字段");
                        
                        // 列出所有表單字段
                        _logger.LogInformation("  - 所有表單字段:");
                        foreach (var key in Request.Form.Keys)
                        {
                            _logger.LogInformation("    - '{Key}': '{Value}'", key, Request.Form[key].ToString());
                        }
                    }
                }
                
                // 🔍 輸出資料庫連接信息以診斷問題
                var connectionString = _context.Database.GetConnectionString();
                var providerName = _context.Database.ProviderName;
                Console.WriteLine($"DB_PROVIDER: {providerName}");
                
                // 安全處理連接字串顯示
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("DB_CONNECTION: NULL_OR_EMPTY");
                }
                else
                {
                    var maxLength = Math.Min(connectionString.Length, 50);
                    Console.WriteLine($"DB_CONNECTION: {connectionString.Substring(0, maxLength)}...");
                }
                
                // 🔍 檢查是否為 InMemory 資料庫
                var isInMemory = providerName?.Contains("InMemory") == true;
                Console.WriteLine($"IS_INMEMORY: {isInMemory}");
                
                if (isInMemory)
                {
                    Console.WriteLine("ALERT: PropertyController is using InMemory database!");
                    _logger.LogError("❌ 警告：PropertyController 正在使用 InMemory 資料庫，這將導致資料不會真實保存！");
                }
                _logger.LogInformation("📋 收到的 DTO 基本信息:");
                _logger.LogInformation("  - Title: {Title}", dto?.Title ?? "NULL");
                
                // 🔍 TempSessionId 詳細檢查點
                _logger.LogInformation("🔍 [CHECKPOINT] TempSessionId 詳細檢查:");
                _logger.LogInformation("  - dto?.TempSessionId 原始值: '{RawValue}'", dto?.TempSessionId ?? "NULL");
                _logger.LogInformation("  - dto?.TempSessionId IsNull: {IsNull}", dto?.TempSessionId == null);
                _logger.LogInformation("  - dto?.TempSessionId IsEmpty: {IsEmpty}", string.IsNullOrEmpty(dto?.TempSessionId));
                _logger.LogInformation("  - dto?.TempSessionId IsWhiteSpace: {IsWhiteSpace}", string.IsNullOrWhiteSpace(dto?.TempSessionId));
                if (!string.IsNullOrEmpty(dto?.TempSessionId))
                {
                    _logger.LogInformation("  - dto?.TempSessionId Length: {Length}", dto.TempSessionId.Length);
                }
                
                _logger.LogInformation("  - MonthlyRent: {MonthlyRent}", dto?.MonthlyRent ?? 0);
                
                // 安全處理 SelectedEquipmentIds
                var equipmentCount = 0;
                try
                {
                    equipmentCount = dto?.SelectedEquipmentIds?.Count ?? 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("❌ 計算 SelectedEquipmentIds 數量時發生異常: {Exception}", ex.Message);
                    equipmentCount = 0;
                }
                _logger.LogInformation("  - SelectedEquipmentIds Count: {Count}", equipmentCount);
                
                // 記錄 ModelState 初始狀態
                _logger.LogInformation("🔍 [DEBUG] ModelState 檢查前 - 檢查點 2");
                _logger.LogInformation("📊 ModelState 初始狀態: Valid={IsValid}, ErrorCount={ErrorCount}", 
                    ModelState.IsValid, ModelState.ErrorCount);
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("🔍 [DEBUG] ModelState 無效，將返回表單 - 檢查點 2a");
                    foreach (var error in ModelState)
                    {
                        if (error.Value?.Errors.Any() == true)
                        {
                            _logger.LogWarning("❌ ModelState 錯誤 - {Key}: {Errors}", 
                                error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("🔍 [DEBUG] ModelState 有效，繼續執行 - 檢查點 2b");
                }

                // TODO: 實作真實身份驗證系統
                // 從 Session、JWT Token 或其他認證方式取得當前使用者 ID
                _logger.LogInformation("🔍 [DEBUG] 身份驗證檢查前 - 檢查點 3");
                _logger.LogInformation("🔑 開始身份驗證檢查");
                var currentUserId = await GetCurrentUserIdAsync();
                _logger.LogInformation("🔑 取得當前用戶 ID: {UserId}", currentUserId);
                
                if (currentUserId == null)
                {
                    _logger.LogWarning("🔍 [DEBUG] 身份驗證失敗，將重導向 - 檢查點 3a");
                    _logger.LogWarning("❌ 用戶未登入，重導向到登入頁面");
                    TempData["ErrorMessage"] = "請先登入才能創建房源";
                    return RedirectToAction("Login", "Member");
                }
                else
                {
                    _logger.LogInformation("🔍 [DEBUG] 身份驗證成功，繼續執行 - 檢查點 3b");
                }
                
                // 驗證使用者是否為房東
                _logger.LogInformation("🔍 [DEBUG] 房東身份驗證前 - 檢查點 4");
                _logger.LogInformation("🏠 驗證房東身份");
                var isLandlord = await IsUserLandlordAsync(currentUserId.Value);
                _logger.LogInformation("🏠 房東身份驗證結果: {IsLandlord}", isLandlord);
                
                if (!isLandlord)
                {
                    _logger.LogWarning("🔍 [DEBUG] 房東身份驗證失敗，將重導向 - 檢查點 4a");
                    _logger.LogWarning("❌ 用戶不是房東，拒絕創建房源 - UserId: {UserId}", currentUserId.Value);
                    TempData["ErrorMessage"] = "只有房東會員才能創建房源";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogInformation("🔍 [DEBUG] 房東身份驗證成功，繼續執行 - 檢查點 4b");
                }

                // 後端驗證
                _logger.LogInformation("🔍 [DEBUG] 後端驗證前 - 檢查點 5");
                _logger.LogInformation("✅ 開始後端驗證");
                var validationResult = await ValidatePropertyCreateDto(dto);
                _logger.LogInformation("✅ 後端驗證結果: IsValid={IsValid}, ErrorCount={ErrorCount}", 
                    validationResult.IsValid, validationResult.Errors.Count);
                
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("🔍 [DEBUG] 後端驗證失敗 - 檢查點 5a");
                    foreach (var error in validationResult.Errors)
                    {
                        _logger.LogWarning("❌ 驗證錯誤 - {Property}: {Message}", 
                            error.PropertyName, error.ErrorMessage);
                        ModelState.AddModelError(error.PropertyName ?? string.Empty, error.ErrorMessage);
                    }
                }
                else
                {
                    _logger.LogInformation("🔍 [DEBUG] 後端驗證成功 - 檢查點 5b");
                }

                // 再次檢查 ModelState
                _logger.LogInformation("🔍 [DEBUG] 最終 ModelState 檢查前 - 檢查點 6");
                _logger.LogInformation("📊 最終 ModelState 狀態: Valid={IsValid}, ErrorCount={ErrorCount}", 
                    ModelState.IsValid, ModelState.ErrorCount);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("🔍 [DEBUG] 最終 ModelState 無效，將返回表單 - 檢查點 6a");
                    _logger.LogWarning("❌ ModelState 無效，返回表單視圖");
                    
                    // 記錄所有錯誤
                    foreach (var error in ModelState)
                    {
                        if (error.Value?.Errors.Any() == true)
                        {
                            _logger.LogWarning("❌ 最終 ModelState 錯誤 - {Key}: {Errors}", 
                                error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                        }
                    }
                    
                    // 添加到 TempData 顯示給用戶
                    var errorMessages = ModelState
                        .Where(x => x.Value?.Errors.Any() == true)
                        .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                        .ToList();
                    
                    TempData["ErrorMessage"] = "表單驗證失敗：\n" + string.Join("\n", errorMessages);
                    TempData["ValidationErrors"] = errorMessages;
                    
                    // 重新載入下拉選單資料 - 使用新的服務
                    var cities = await GetActiveCitiesAsync();
                    var listingPlans = await _listingPlanValidationService.GetActiveListingPlansAsync();
                    var equipmentCategoriesHierarchy = await _equipmentCategoryQueryService.GetCategoriesHierarchyAsync();

                    var viewModel = new PropertyCreateViewModel
                    {
                        PropertyData = dto,
                        Cities = cities,
                        ListingPlans = listingPlans,
                        EquipmentCategoriesHierarchy = equipmentCategoriesHierarchy,
                        AvailableChineseCategories = PropertyImageCategoryHelper.GetAllPropertyChineseCategories()
                    };

                    return View(viewModel);
                }
                else
                {
                    _logger.LogInformation("🔍 [DEBUG] 最終 ModelState 有效，開始資料庫事務 - 檢查點 6b");
                }

                // 開始資料庫事務 - 包含完整的兩階段上傳流程
                _logger.LogInformation("🔍 [DEBUG] 準備開始資料庫事務 - 檢查點 7");
                _logger.LogInformation("💾 開始資料庫事務 - 包含房源創建和圖片遷移");
                using var transaction = await _context.Database.BeginTransactionAsync();
                _logger.LogInformation("💾 資料庫事務已開始 - TransactionId: {TransactionId}", transaction.TransactionId);

                // 用於回滾時的清理信息
                var rollbackInfo = new
                {
                    PropertyId = (int?)null,
                    MigratedBlobPaths = new List<string>(),
                    TempSessionId = dto.TempSessionId
                };

                try
                {
                    // 步驟 1: 建立房源基本資料
                    _logger.LogInformation("🏠 [步驟1/4] 建立房源基本資料");
                    var property = await CreatePropertyFromDto(dto, currentUserId.Value);
                    _logger.LogInformation("🏠 房源實體已建立，準備儲存到資料庫 - Title: {Title}", property.Title);
                    
                    _context.Properties.Add(property);
                    await _context.SaveChangesAsync();
                    rollbackInfo = rollbackInfo with { PropertyId = property.PropertyId };
                    
                    _logger.LogInformation("✅ [步驟1/4] 房源基本資料已儲存 - PropertyId: {PropertyId}, Title: {Title}", 
                        property.PropertyId, property.Title);

                    // 步驟 2: 處理臨時會話圖片遷移（在同一交易內）
                    if (!string.IsNullOrEmpty(dto.TempSessionId))
                    {
                        _logger.LogInformation("📸 [步驟2/4] 開始處理臨時會話圖片遷移 - TempSessionId: {TempSessionId}", dto.TempSessionId);
                        
                        // 在同一交易內進行圖片遷移
                        var migrationResult = await ProcessTempImageMigrationInTransactionAsync(property.PropertyId, dto.TempSessionId, dto.ImageOrder);
                        rollbackInfo = rollbackInfo with { MigratedBlobPaths = migrationResult.MovedBlobPaths };
                        
                        if (!migrationResult.Success)
                        {
                            throw new InvalidOperationException($"圖片遷移失敗: {migrationResult.ErrorMessage}");
                        }
                        
                        _logger.LogInformation("✅ [步驟2/4] 臨時圖片遷移完成 - 遷移了 {FileCount} 個檔案", migrationResult.MovedBlobPaths.Count);
                    }
                    else
                    {
                        _logger.LogInformation("ℹ️ [步驟2/4] 沒有 TempSessionId，跳過圖片遷移");
                    }

                    // 步驟 3: 建立設備關聯
                    if (dto.SelectedEquipmentIds?.Any() == true)
                    {
                        _logger.LogInformation("🔧 [步驟3/4] 建立設備關聯 - 數量: {Count}", dto.SelectedEquipmentIds.Count);
                        await CreatePropertyEquipmentRelations(property.PropertyId, dto);
                        _logger.LogInformation("✅ [步驟3/4] 設備關聯已建立");
                    }
                    else
                    {
                        _logger.LogInformation("ℹ️ [步驟3/4] 沒有選擇設備，跳過設備關聯建立");
                    }

                    // 步驟 4: 提交整個交易
                    _logger.LogInformation("💾 [步驟4/4] 準備提交完整交易 - PropertyId: {PropertyId}", property.PropertyId);
                    await _context.SaveChangesAsync(); // 確保所有變更都被追蹤
                    await transaction.CommitAsync();
                    _logger.LogInformation("✅ [步驟4/4] 完整交易提交成功 - PropertyId: {PropertyId}", property.PropertyId);

                    // 🔍 立即驗證資料是否真實寫入資料庫
                    _logger.LogInformation("🔍 開始驗證房源是否真實寫入資料庫 - PropertyId: {PropertyId}", property.PropertyId);
                    var verificationProperty = await _context.Properties
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PropertyId == property.PropertyId);
                    
                    if (verificationProperty != null)
                    {
                        _logger.LogInformation("✅ 驗證成功：房源已真實存在於資料庫中");
                        _logger.LogInformation("✅ 驗證詳情 - Title: {Title}, CreatedAt: {CreatedAt}, StatusCode: {StatusCode}",
                            verificationProperty.Title, verificationProperty.CreatedAt, verificationProperty.StatusCode);
                    }
                    else
                    {
                        _logger.LogError("❌ 驗證失敗：房源在資料庫中不存在！這是一個嚴重的資料一致性問題");
                        _logger.LogError("❌ 預期PropertyId: {PropertyId}，但查詢結果為 null", property.PropertyId);
                        
                        // 嘗試查詢最近創建的房源以進行調試
                        var recentProperties = await _context.Properties
                            .AsNoTracking()
                            .Where(p => p.CreatedAt >= DateTime.Now.AddMinutes(-5))
                            .OrderByDescending(p => p.CreatedAt)
                            .Take(5)
                            .ToListAsync();
                        
                        _logger.LogError("❌ 最近5分鐘創建的房源數量: {Count}", recentProperties.Count);
                        foreach (var recent in recentProperties)
                        {
                            _logger.LogError("❌ 房源記錄 - ID: {PropertyId}, Title: {Title}, Created: {CreatedAt}", 
                                recent.PropertyId, recent.Title, recent.CreatedAt);
                        }
                        
                        throw new InvalidOperationException($"房源創建驗證失敗：PropertyId {property.PropertyId} 在資料庫中不存在");
                    }

                    _logger.LogInformation("🎉 成功創建房源，房源ID: {PropertyId}, 房東ID: {LandlordId}", 
                        property.PropertyId, currentUserId.Value);

                    // 移除吐司訊息，因為 CreationSuccess 頁面本身就是成功確認頁面
                    return RedirectToAction("CreationSuccess", new { id = property.PropertyId });
                }
                catch (Exception transactionEx)
                {
                    _logger.LogError(transactionEx, "💥 事務執行過程中發生異常，正在回滾事務");
                    _logger.LogError("💥 異常類型: {ExceptionType}", transactionEx.GetType().Name);
                    _logger.LogError("💥 異常訊息: {ExceptionMessage}", transactionEx.Message);
                    _logger.LogError("💥 回滾信息 - PropertyId: {PropertyId}, MigratedBlobCount: {BlobCount}, TempSessionId: {TempSessionId}", 
                        rollbackInfo.PropertyId, rollbackInfo.MigratedBlobPaths.Count, rollbackInfo.TempSessionId);
                    
                    if (transactionEx.InnerException != null)
                    {
                        _logger.LogError("💥 內部異常: {InnerExceptionType} - {InnerExceptionMessage}", 
                            transactionEx.InnerException.GetType().Name, transactionEx.InnerException.Message);
                    }
                    
                    try
                    {
                        _logger.LogInformation("🔄 開始交易回滾流程");
                        await transaction.RollbackAsync();
                        _logger.LogInformation("✅ 資料庫交易回滾成功");
                        
                        // 如果有遷移的 blob 檔案，嘗試清理
                        if (rollbackInfo.MigratedBlobPaths.Any())
                        {
                            _logger.LogWarning("🧹 嘗試清理已遷移的 Blob 檔案 - 數量: {Count}", rollbackInfo.MigratedBlobPaths.Count);
                            
                            try
                            {
                                var deleteResults = await _blobStorageService.DeleteMultipleAsync(rollbackInfo.MigratedBlobPaths);
                                var successCount = deleteResults.Count(r => r.Value);
                                var failureCount = deleteResults.Count(r => !r.Value);
                                
                                _logger.LogInformation("🧹 Blob 清理結果 - 成功: {SuccessCount}, 失敗: {FailureCount}", 
                                    successCount, failureCount);
                                    
                                if (failureCount > 0)
                                {
                                    _logger.LogWarning("⚠️ 部分 Blob 檔案清理失敗，可能需要手動清理");
                                }
                            }
                            catch (Exception blobCleanupEx)
                            {
                                _logger.LogError(blobCleanupEx, "💥 Blob 檔案清理時發生異常");
                            }
                        }
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "💥 事務回滾失敗 - 資料庫可能處於不一致狀態");
                        _logger.LogError("💥 回滾失敗詳情 - PropertyId: {PropertyId}, TempSessionId: {TempSessionId}", 
                            rollbackInfo.PropertyId, rollbackInfo.TempSessionId);
                    }
                    
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 PropertyController.Create 頂級異常 - 異常類型: {ExceptionType}, 異常訊息: {ExceptionMessage}", 
                    ex.GetType().Name, ex.Message);
                    
                if (ex.InnerException != null)
                {
                    _logger.LogError("💥 頂級內部異常: {InnerExceptionType} - {InnerExceptionMessage}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                
                _logger.LogError("💥 堆疊追蹤: {StackTrace}", ex.StackTrace);
                
                TempData["ErrorMessage"] = "創建房源時發生錯誤，請稍後再試";
                return RedirectToAction("Create");
            }
        }

        /// <summary>
        /// 處理房源更新 (編輯模式專用)
        /// </summary>
        [HttpPost]
        [Route("property/{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, PropertyCreateDto dto)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // 身份驗證
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    _logger.LogWarning("未登入用戶嘗試更新房源 - PropertyId: {PropertyId}", id);
                    return RedirectToAction("Login", "Member");
                }
                
                // 驗證房源所有權
                if (!await ValidatePropertyOwnership(id, currentUserId.Value))
                {
                    _logger.LogWarning("房源所有權驗證失敗 - PropertyId: {PropertyId}, UserId: {UserId}", 
                        id, currentUserId);
                    TempData["ErrorMessage"] = "您無權修改此房源";
                    return RedirectToAction("PropertyManagement", "Landlord");
                }
                
                // 驗證房源狀態
                var property = await _context.Properties.FindAsync(id);
                if (property == null || !CanEditPropertyStatus(property.StatusCode))
                {
                    _logger.LogWarning("房源狀態不允許編輯 - PropertyId: {PropertyId}, Status: {Status}", 
                        id, property?.StatusCode);
                    TempData["ErrorMessage"] = "此房源當前狀態不允許編輯";
                    return RedirectToAction("PropertyManagement", "Landlord");
                }
                
                // 後端驗證
                var validationResult = await ValidatePropertyCreateDto(dto);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName ?? string.Empty, error.ErrorMessage);
                    }
                }
                
                if (!ModelState.IsValid)
                {
                    // 驗證失敗，重新顯示編輯表單
                    return await Edit(id);
                }
                
                // 開始資料庫事務
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // 更新房源資料
                    await UpdatePropertyFromDto(id, dto);
                    
                    // 更新設備關聯
                    if (dto.SelectedEquipmentIds?.Any() == true)
                    {
                        await UpdatePropertyEquipmentRelations(id, dto);
                    }
                    
                    await transaction.CommitAsync();
                    
                    stopwatch.Stop();
                    _logger.LogInformation("房源更新成功 - PropertyId: {PropertyId}, UserId: {UserId}, Duration: {Duration}ms", 
                        id, currentUserId, stopwatch.ElapsedMilliseconds);
                    
                    TempData["SuccessMessage"] = "房源更新成功";
                    return RedirectToAction("PropertyManagement", "Landlord");
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "更新房源時發生錯誤 - PropertyId: {PropertyId}, Duration: {Duration}ms", 
                    id, stopwatch.ElapsedMilliseconds);
                TempData["ErrorMessage"] = "更新房源時發生錯誤，請稍後再試";
                return await Edit(id);
            }
        }

        /// <summary>
        /// 房源預覽功能 - In-memory 模型預覽，不存入資料庫
        /// </summary>
        /// <param name="dto">房源創建資料</param>
        /// <returns>預覽視圖</returns>
        [HttpPost]
        [Route("property/preview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview([FromBody] PropertyCreateDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return Json(new { success = false, message = "未收到有效的預覽資料" });
                }

                // 基本資料驗證（不包含必填欄位驗證，允許部分填寫的預覽）
                var validationResult = await ValidatePropertyCreateDtoForPreview(dto);
                if (!validationResult.IsValid)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = "資料驗證失敗", 
                        errors = validationResult.Errors.Select(e => e.ErrorMessage) 
                    });
                }

                // 建立預覽用 PropertyDetailViewModel（不存入資料庫）
                var previewViewModel = await CreatePreviewViewModelFromDto(dto);

                // 直接返回部分視圖
                return PartialView("_PropertyPreview", previewViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生房源預覽時發生錯誤");
                return Json(new { success = false, message = "產生預覽時發生系統錯誤，請稍後再試" });
            }
        }

        /// <summary>
        /// 房源創建成功頁面
        /// </summary>
        /// <param name="id">房源 ID</param>
        /// <returns>成功頁面視圖</returns>
        [Route("property/creation-success/{id:int?}")]
        public IActionResult CreationSuccess(int? id)
        {
            // 不需要驗證房源存在，因為這是成功提示頁面
            // 即使 id 為空也可以顯示成功頁面
            return View();
        }

        /// <summary>
        /// 處理房源圖片上傳（整合雙語分類系統）
        /// </summary>
        /// <param name="uploadDto">圖片上傳資料</param>
        /// <returns>上傳結果</returns>
        [HttpPost]
        [Route("property/{propertyId:int}/upload-images")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(int propertyId, PropertyImageUploadDto uploadDto)
        {
            try
            {
                // 驗證房源是否存在且屬於當前使用者
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    return Json(new { success = false, message = "找不到指定的房源" });
                }

                // TODO: 驗證房源所有權
                // if (property.LandlordMemberId != currentUserId) return Forbid();

                // 驗證圖片檔案
                if (uploadDto.ImageFiles == null || !uploadDto.ImageFiles.Any())
                {
                    return Json(new { success = false, message = "請選擇要上傳的圖片檔案" });
                }

                // 驗證檔案數量限制（每筆房源最多15張）
                var existingImageCount = await _propertyImageService.GetPropertyImageCountAsync(propertyId);
                if (existingImageCount + uploadDto.ImageFiles.Count > 15)
                {
                    return Json(new { success = false, message = $"每筆房源最多只能上傳15張圖片，目前已有{existingImageCount}張" });
                }

                // 驗證中文分類
                if (!PropertyImageCategoryHelper.IsValidPropertyCategory(uploadDto.ChineseCategory))
                {
                    return Json(new { success = false, message = "無效的圖片分類" });
                }

                // 將 IFormFile 列表轉換為 IFormFileCollection
                var formFiles = new FormFileCollection();
                foreach (var file in uploadDto.ImageFiles)
                {
                    formFiles.Add(file);
                }

                // 使用PropertyImageService上傳圖片（支援中文分類）
                var uploadResults = await _propertyImageService.UploadPropertyImagesByChineseCategoryAsync(
                    propertyId, 
                    formFiles, 
                    uploadDto.ChineseCategory
                );

                var successCount = uploadResults.Count(r => r.Success);
                var failureCount = uploadResults.Count(r => !r.Success);

                _logger.LogInformation("房源圖片上傳完成，房源ID: {PropertyId}, 成功: {SuccessCount}, 失敗: {FailureCount}, 分類: {Category}", 
                    propertyId, successCount, failureCount, uploadDto.ChineseCategory);

                return Json(new 
                { 
                    success = true, 
                    message = $"成功上傳 {successCount} 張圖片" + (failureCount > 0 ? $"，{failureCount} 張失敗" : ""),
                    uploadedCount = successCount,
                    failedCount = failureCount,
                    results = uploadResults.Select(r => new 
                    {
                        success = r.Success,
                        fileName = r.OriginalFileName,
                        message = r.Success ? "上傳成功" : r.ErrorMessage,
                        imageId = r.Success ? r.PropertyImageId : (int?)null,
                        imagePath = r.Success ? r.OriginalImagePath : null
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳房源圖片時發生錯誤，房源ID: {PropertyId}", propertyId);
                return Json(new { success = false, message = "上傳圖片時發生系統錯誤" });
            }
        }
        // === 私有輔助方法 ===

        /// <summary>
        /// 生成唯一的 PropertyId
        /// 因為 PropertyId 不是 IDENTITY 欄位，需要手動生成
        /// </summary>
        /// <returns>唯一的 PropertyId</returns>
        private async Task<int> GenerateUniquePropertyIdAsync()
        {
            try
            {
                // 使用簡單的遞增方式：最大 PropertyId + 1
                var maxPropertyId = await _context.Properties.MaxAsync(p => (int?)p.PropertyId) ?? 2000;
                var newPropertyId = maxPropertyId + 1;
                
                _logger.LogInformation("生成新 PropertyId: {PropertyId} (最大ID: {MaxId})", newPropertyId, maxPropertyId);
                return newPropertyId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成 PropertyId 時發生錯誤");
                // 備用方案：使用預設起始值
                return 3000;
            }
        }

        /// <summary>
        /// 生成唯一的 RelationId
        /// 因為 RelationId 不是 IDENTITY 欄位，需要手動生成
        /// </summary>
        /// <returns>唯一的 RelationId</returns>
        private async Task<int> GenerateUniqueRelationIdAsync()
        {
            try
            {
                // 基於時間戳的唯一 ID
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var random = new Random().Next(100, 999);
                var candidateId = (int)(timestamp % 1000000) * 1000 + random;

                // 檢查是否已存在，如果存在則重新生成
                var maxAttempts = 10;
                var attempts = 0;
                
                while (attempts < maxAttempts)
                {
                    var exists = await _context.PropertyEquipmentRelations
                        .AnyAsync(r => r.RelationId == candidateId);
                    
                    if (!exists)
                    {
                        _logger.LogInformation("✅ 生成唯一RelationId成功: {RelationId}", candidateId);
                        return candidateId;
                    }
                    
                    // 如果已存在，生成新的候選ID
                    candidateId = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000000) * 1000 + new Random().Next(100, 999);
                    attempts++;
                    
                    _logger.LogWarning("⚠️ RelationId {RelationId} 已存在，嘗試第 {Attempt} 次重新生成", candidateId, attempts);
                }
                
                // 如果多次嘗試仍無法生成唯一 ID，則使用資料庫最大 ID + 1 的方式
                var maxRelationId = await _context.PropertyEquipmentRelations.MaxAsync(r => (int?)r.RelationId) ?? 2000;
                var fallbackId = maxRelationId + 1;
                
                _logger.LogWarning("使用備用方案生成 RelationId: {RelationId}", fallbackId);
                return fallbackId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成 RelationId 時發生錯誤");
                // 最終備用方案：使用當前時間戳
                return (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000000);
            }
        }

        /// <summary>
        /// 獲取啟用的城市列表
        /// </summary>
        private async Task<List<CityDistrictDto>> GetActiveCitiesAsync()
        {
            return await _context.Cities
                .Where(c => c.IsActive == true)
                .Select(c => new CityDistrictDto
                {
                    CityId = c.CityId,
                    CityName = c.CityName,
                    Districts = c.Districts
                        .Where(d => d.IsActive == true)
                        .Select(d => new DistrictDto
                        {
                            DistrictId = d.DistrictId,
                            DistrictName = d.DistrictName,
                            CityId = d.CityId
                        })
                        .OrderBy(d => d.DistrictName)
                        .ToList()
                })
                .OrderBy(c => c.CityName)
                .ToListAsync();
        }

        /// <summary>
        /// 獲取啟用的刊登方案列表
        /// </summary>
        private async Task<List<ListingPlanDto>> GetActiveListingPlansAsync()
        {
            return await _context.ListingPlans
                .Where(lp => lp.IsActive == true)
                .Select(lp => new ListingPlanDto
                {
                    PlanId = lp.PlanId,
                    PlanName = lp.PlanName,
                    MinListingDays = lp.MinListingDays,
                    PricePerDay = lp.PricePerDay,
                    TotalPrice = lp.PricePerDay * lp.MinListingDays,
                    Description = lp.PlanName // 使用PlanName作為描述
                })
                .OrderBy(lp => lp.TotalPrice)
                .ToListAsync();
        }

        /// <summary>
        /// 獲取啟用的設備分類列表
        /// </summary>
        private async Task<List<PropertyEquipmentSelectionDto>> GetActiveEquipmentCategoriesAsync()
        {
            return await _context.PropertyEquipmentCategories
                .Where(pec => pec.IsActive == true)
                .Select(pec => new PropertyEquipmentSelectionDto
                {
                    CategoryId = pec.CategoryId,
                    CategoryName = pec.CategoryName,
                    ParentCategoryId = pec.ParentCategoryId,
                    Selected = false,
                    Quantity = 1
                })
                .OrderBy(pec => pec.ParentCategoryId ?? 0)
                .ThenBy(pec => pec.CategoryName)
                .ToListAsync();
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

            // 驗證設備選擇：至少要選擇一個設備
            if (dto.SelectedEquipmentIds == null || !dto.SelectedEquipmentIds.Any())
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "SelectedEquipmentIds", 
                    ErrorMessage = "設備與服務至少需要選擇一項" 
                });
            }

            // ✨ 新增：驗證 TempSessionId 必要性（兩階段上傳的關鍵驗證）
            _logger.LogInformation("🔍 [驗證] 檢查 TempSessionId 必要性：'{TempSessionId}'", dto.TempSessionId ?? "NULL");
            
            if (string.IsNullOrWhiteSpace(dto.TempSessionId))
            {
                _logger.LogWarning("❌ [驗證] TempSessionId 為空或 null，這是必填欄位");
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "TempSessionId", 
                    ErrorMessage = "請先上傳圖片和房屋所有權證明文件，TempSessionId 不能為空" 
                });
            }
            else
            {
                // 驗證 TempSessionId 是否有效以及是否包含必要的檔案
                try
                {
                    _logger.LogInformation("🔍 [驗證] 檢查臨時會話有效性：{TempSessionId}", dto.TempSessionId);
                    
                    var isValidSession = await _tempSessionService.IsValidTempSessionAsync(dto.TempSessionId);
                    if (!isValidSession)
                    {
                        _logger.LogWarning("❌ [驗證] 臨時會話無效：{TempSessionId}", dto.TempSessionId);
                        result.Errors.Add(new PropertyValidationError 
                        { 
                            PropertyName = "TempSessionId", 
                            ErrorMessage = "臨時會話已過期或無效，請重新上傳圖片和文件" 
                        });
                    }
                    else
                    {
                        _logger.LogInformation("✅ [驗證] 臨時會話有效，檢查檔案");
                        
                        // 檢查是否有上傳的檔案
                        var tempImages = await _tempSessionService.GetTempImagesAsync(dto.TempSessionId);
                        _logger.LogInformation("📊 [驗證] 臨時檔案數量：{Count}", tempImages.Count);
                        
                        if (!tempImages.Any())
                        {
                            _logger.LogWarning("❌ [驗證] 臨時會話中沒有檔案");
                            result.Errors.Add(new PropertyValidationError 
                            { 
                                PropertyName = "TempSessionId", 
                                ErrorMessage = "請至少上傳一張房源圖片和房屋所有權證明文件" 
                            });
                        }
                        else
                        {
                            // 檢查是否有必要的檔案類型（至少一張圖片和一份 PDF）
                            var imageFiles = tempImages.Where(t => t.Category == Enums.ImageCategory.Gallery).ToList();
                            var documentFiles = tempImages.Where(t => t.Category == Enums.ImageCategory.Document).ToList();
                            
                            _logger.LogInformation("📊 [驗證] 圖片檔案數量：{ImageCount}，文件檔案數量：{DocumentCount}", 
                                imageFiles.Count, documentFiles.Count);
                            
                            if (!imageFiles.Any())
                            {
                                _logger.LogWarning("❌ [驗證] 沒有上傳房源圖片");
                                result.Errors.Add(new PropertyValidationError 
                                { 
                                    PropertyName = "TempSessionId", 
                                    ErrorMessage = "請至少上傳一張房源圖片" 
                                });
                            }
                            
                            if (!documentFiles.Any())
                            {
                                _logger.LogWarning("❌ [驗證] 沒有上傳房屋所有權證明文件");
                                result.Errors.Add(new PropertyValidationError 
                                { 
                                    PropertyName = "TempSessionId", 
                                    ErrorMessage = "請上傳房屋所有權證明文件（PDF 格式）" 
                                });
                            }
                            
                            if (imageFiles.Any() && documentFiles.Any())
                            {
                                _logger.LogInformation("✅ [驗證] TempSessionId 驗證通過：圖片 {ImageCount} 張，文件 {DocumentCount} 份", 
                                    imageFiles.Count, documentFiles.Count);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ [驗證] 檢查 TempSessionId 時發生異常：{TempSessionId}", dto.TempSessionId);
                    result.Errors.Add(new PropertyValidationError 
                    { 
                        PropertyName = "TempSessionId", 
                        ErrorMessage = "檢查上傳檔案時發生錯誤，請重新上傳" 
                    });
                }
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
            // 使用驗證服務驗證刊登方案
            var validationResult = await _listingPlanValidationService.ValidateListingPlanAsync(dto.ListingPlanId);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"刊登方案驗證失敗: {string.Join(", ", validationResult.Errors)}");
            }

            // 使用驗證服務計算費用和到期日
            var now = DateTime.Now;
            var listingFee = await _listingPlanValidationService.CalculateTotalFeeAsync(dto.ListingPlanId);
            var expireDate = await _listingPlanValidationService.CalculateExpireDateAsync(dto.ListingPlanId, now);
            var listingPlan = await _listingPlanValidationService.GetListingPlanByIdAsync(dto.ListingPlanId);

            if (listingFee == null || expireDate == null || listingPlan == null)
            {
                throw new InvalidOperationException("刊登方案計算失敗");
            }

            // 生成唯一的 PropertyId (因為資料庫中 PropertyId 不是 IDENTITY 欄位)
            var newPropertyId = await GenerateUniquePropertyIdAsync();

            // 生成房源座標資料
            decimal? latitude = null;
            decimal? longitude = null;
            
            if (!string.IsNullOrWhiteSpace(dto.AddressLine))
            {
                try
                {
                    _logger.LogInformation("🗺️ 開始為房源生成座標 - PropertyId: {PropertyId}, Address: {Address}", 
                        newPropertyId, dto.AddressLine);

                    var geocodingRequest = new zuHause.DTOs.GoogleMaps.GeocodingRequest
                    {
                        Address = dto.AddressLine,
                        Language = "zh-TW",
                        Region = "TW"
                    };

                    var geocodingResult = await _googleMapsService.GeocodeAsync(geocodingRequest);
                    
                    if (geocodingResult.IsSuccess && geocodingResult.Latitude.HasValue && geocodingResult.Longitude.HasValue)
                    {
                        latitude = (decimal)geocodingResult.Latitude.Value;
                        longitude = (decimal)geocodingResult.Longitude.Value;
                        
                        _logger.LogInformation("✅ 座標生成成功 - PropertyId: {PropertyId}, Lat: {Lat}, Lng: {Lng}", 
                            newPropertyId, latitude, longitude);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ 座標生成失敗 - PropertyId: {PropertyId}, Address: {Address}, Status: {Status}, Error: {Error}", 
                            newPropertyId, dto.AddressLine, geocodingResult.Status, geocodingResult.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ 座標生成過程中發生異常 - PropertyId: {PropertyId}, Address: {Address}", 
                        newPropertyId, dto.AddressLine);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ 地址為空，無法生成座標 - PropertyId: {PropertyId}", newPropertyId);
            }

            return new Property
            {
                PropertyId = newPropertyId,
                LandlordMemberId = landlordMemberId,
                Title = dto.Title,
                Description = dto.Description,
                CityId = dto.CityId,
                DistrictId = dto.DistrictId,
                AddressLine = dto.AddressLine,
                Latitude = latitude,
                Longitude = longitude,
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
                ListingFeeAmount = listingFee.Value,
                ListingPlanId = dto.ListingPlanId,
                PropertyProofUrl = dto.PropertyProofUrl,
                StatusCode = "PENDING", // 預設為審核中狀態
                IsPaid = false, // 預設未付款
                ExpireAt = expireDate.Value,
                CreatedAt = now,
                UpdatedAt = now
            };
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
                
                // 生成唯一的 RelationId (因為資料庫中 RelationId 不是 IDENTITY 欄位)
                var newRelationId = await GenerateUniqueRelationIdAsync();
                
                equipmentRelations.Add(new PropertyEquipmentRelation
                {
                    RelationId = newRelationId,
                    PropertyId = propertyId,
                    CategoryId = equipmentId,
                    Quantity = quantity,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            _context.PropertyEquipmentRelations.AddRange(equipmentRelations);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 獲取當前登入使用者的ID
        /// 這裡需要根據實際的認證系統來實作
        /// </summary>
        /// <returns>使用者ID，如果未登入則返回 null</returns>
        private async Task<int?> GetCurrentUserIdAsync()
        {
            try
            {
                _logger.LogInformation("🔑 開始獲取當前用戶ID");
                
                // 方式1: 從 Claims 取得 (與 LandlordController 保持一致)
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var claimsUserId))
                {
                    _logger.LogInformation("🔑 從 Claims 獲取用戶ID: {UserId}", claimsUserId);
                    return claimsUserId;
                }
                
                // 方式2: 從 Session 取得
                if (HttpContext.Session.TryGetValue("UserId", out var userIdBytes))
                {
                    var sessionUserId = BitConverter.ToInt32(userIdBytes, 0);
                    _logger.LogInformation("🔑 從 Session 獲取用戶ID: {UserId}", sessionUserId);
                    return sessionUserId;
                }
                
                // 方式3: 從 Cookie 或其他認證方式取得
                if (Request.Cookies.TryGetValue("UserId", out var userIdCookie) && int.TryParse(userIdCookie, out var cookieUserId))
                {
                    _logger.LogInformation("🔑 從 Cookie 獲取用戶ID: {UserId}", cookieUserId);
                    return cookieUserId;
                }
                
                // 檢查是否已登入
                _logger.LogInformation("🔑 User.Identity.IsAuthenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
                _logger.LogInformation("🔑 User.Identity.Name: {IdentityName}", User.Identity?.Name);
                
                // 檢查所有可用的 Claims
                var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                _logger.LogWarning("🔑 未找到 UserId，所有 Claims: {@AllClaims}", allClaims);
                
                // 檢查所有可用的 Cookies
                var allCookies = Request.Cookies.Select(c => new { c.Key, Value = c.Value.Length > 50 ? c.Value.Substring(0, 50) + "..." : c.Value }).ToList();
                _logger.LogInformation("🔑 所有 Cookies: {@AllCookies}", allCookies);
                
                // 如果都取不到，返回 null 表示未登入
                _logger.LogWarning("❌ 無法獲取當前用戶ID - 用戶可能未登入");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 獲取當前用戶ID時發生異常");
                return null;
            }
        }

        /// <summary>
        /// 驗證指定使用者是否為房東
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>是否為房東</returns>
        private async Task<bool> IsUserLandlordAsync(int userId)
        {
            try
            {
                _logger.LogInformation("🔍 開始驗證房東身份 - UserId: {UserId}", userId);
                
                var user = await _context.Members
                    .FirstOrDefaultAsync(m => m.MemberId == userId);
                
                if (user == null)
                {
                    _logger.LogWarning("❌ 找不到用戶 - UserId: {UserId}", userId);
                    return false;
                }
                
                _logger.LogInformation("🔍 用戶資料 - UserId: {UserId}, IsActive: {IsActive}, IsLandlord: {IsLandlord}, MemberTypeId: {MemberTypeId}", 
                    userId, user.IsActive, user.IsLandlord, user.MemberTypeId);
                
                if (!user.IsActive)
                {
                    _logger.LogWarning("❌ 用戶未激活 - UserId: {UserId}", userId);
                    return false;
                }
                
                if (!user.IsLandlord)
                {
                    _logger.LogWarning("❌ 用戶不是房東 - UserId: {UserId}", userId);
                    return false;
                }
                
                if (user.MemberTypeId != 2)
                {
                    _logger.LogWarning("❌ 用戶會員類型錯誤 - UserId: {UserId}, MemberTypeId: {MemberTypeId} (預期: 2)", 
                        userId, user.MemberTypeId);
                    return false;
                }
                
                _logger.LogInformation("✅ 房東身份驗證成功 - UserId: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 房東身份驗證過程中發生異常 - UserId: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// 預覽專用的驗證方法 - 僅進行基本邏輯驗證，不檢查必填欄位
        /// </summary>
        private async Task<PropertyValidationResult> ValidatePropertyCreateDtoForPreview(PropertyCreateDto dto)
        {
            var result = new PropertyValidationResult();

            // 只驗證有值時的邏輯正確性，不驗證必填
            
            // 驗證樓層邏輯（如果兩個欄位都有值）
            if (dto.CurrentFloor > 0 && dto.TotalFloors > 0 && dto.CurrentFloor > dto.TotalFloors)
            {
                result.Errors.Add(new PropertyValidationError 
                { 
                    PropertyName = "CurrentFloor", 
                    ErrorMessage = "所在樓層不能大於總樓層數" 
                });
            }

            // 驗證城市和區域的有效性（如果兩個欄位都有值）
            if (dto.CityId > 0 && dto.DistrictId > 0)
            {
                var districtExists = await _context.Districts
                    .AnyAsync(d => d.DistrictId == dto.DistrictId && 
                                  d.CityId == dto.CityId && 
                                  d.IsActive == true);

                if (!districtExists)
                {
                    result.Errors.Add(new PropertyValidationError 
                    { 
                        PropertyName = "DistrictId", 
                        ErrorMessage = "選擇的城市和區域不匹配" 
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// 取得當前使用者ID (編輯功能用)
        /// </summary>
        /// <returns>使用者ID，如果未登入則返回 null</returns>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogError("安全錯誤 - 無法取得有效的用戶ID, IP: {IpAddress}", 
                    HttpContext.Connection.RemoteIpAddress);
                return null;
            }
            
            return userId;
        }

        /// <summary>
        /// 載入現有房源資料供編輯使用
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="userId">用戶ID</param>
        /// <returns>房源編輯DTO，如果不存在或無權限則返回null</returns>
        private async Task<PropertyCreateDto?> LoadExistingPropertyForEdit(int propertyId, int userId)
        {
            try
            {
                // 驗證房東身份和房源所有權，包含必要的關聯資料
                var property = await _context.Properties
                    .Include(p => p.PropertyEquipmentRelations)
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId 
                                           && p.LandlordMemberId == userId
                                           && p.DeletedAt == null);

                if (property == null)
                {
                    _logger.LogWarning("房源編輯權限驗證失敗 - PropertyId: {PropertyId}, UserId: {UserId}, IP: {IpAddress}",
                        propertyId, userId, HttpContext.Connection.RemoteIpAddress);
                    return null;
                }

                // 只允許編輯特定狀態的房源
                if (!CanEditPropertyStatus(property.StatusCode))
                {
                    _logger.LogWarning("房源狀態不允許編輯 - PropertyId: {PropertyId}, Status: {Status}, UserId: {UserId}",
                        propertyId, property.StatusCode, userId);
                    return null;
                }

                // 轉換為編輯DTO
                var editDto = MapPropertyToCreateDto(property);
                
                // 載入現有圖片資訊（編輯模式專用）
                await LoadExistingImagesForEdit(editDto, propertyId);
                
                _logger.LogInformation("成功載入房源編輯資料 - PropertyId: {PropertyId}, UserId: {UserId}",
                    propertyId, userId);
                
                return editDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入房源編輯資料時發生錯誤 - PropertyId: {PropertyId}, UserId: {UserId}",
                    propertyId, userId);
                return null;
            }
        }

        /// <summary>
        /// 檢查房源狀態是否允許編輯
        /// </summary>
        /// <param name="statusCode">房源狀態碼</param>
        /// <returns>是否允許編輯</returns>
        private static bool CanEditPropertyStatus(string statusCode)
        {
            // 允許編輯的狀態：草稿、已上架、審核不通過需修正
            return statusCode switch
            {
                "PENDING" => true,       // 審核中
                "IDLE" => true,        // 閒置（已建立但未上架）
                "LISTED" => true,      // 已上架
                "REJECT_REVISE" => true, // 審核不通過，需修正
                _ => false              // 其他狀態不允許編輯
            };
        }

        /// <summary>
        /// 載入現有圖片資訊（編輯模式專用）
        /// </summary>
        /// <param name="dto">編輯DTO</param>
        /// <param name="propertyId">房源ID</param>
        private async Task LoadExistingImagesForEdit(PropertyCreateDto dto, int propertyId)
        {
            try
            {
                _logger.LogInformation("🖼️ 開始載入現有圖片資訊: PropertyId={PropertyId}", propertyId);

                // 查詢房源的所有圖片
                var existingImages = await _context.Images
                    .Where(img => img.EntityId == propertyId &&
                                  img.EntityType == EntityType.Property &&
                                  img.IsActive)
                    .OrderBy(img => img.DisplayOrder)
                    .ThenBy(img => img.UploadedAt)
                    .ToListAsync();

                _logger.LogInformation("📊 找到 {ImageCount} 張現有圖片", existingImages.Count);

                // 轉換為 ExistingImageDto
                dto.ExistingImages = existingImages.Select(img => new ExistingImageDto
                {
                    ImageId = img.ImageId,
                    ImageGuid = img.ImageGuid,
                    OriginalFileName = img.OriginalFileName,
                    Category = img.Category,
                    DisplayOrder = img.DisplayOrder,
                    UploadedAt = img.UploadedAt,
                    ImageUrls = GenerateImageUrls(img.Category, propertyId, img.ImageGuid)
                }).ToList();

                _logger.LogInformation("✅ 現有圖片資訊載入完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 載入現有圖片資訊時發生錯誤: PropertyId={PropertyId}", propertyId);
                dto.ExistingImages = new List<ExistingImageDto>(); // 確保不為 null
            }
        }

        /// <summary>
        /// 生成圖片的各種尺寸URL
        /// </summary>
        /// <param name="category">圖片分類</param>
        /// <param name="entityId">實體ID</param>
        /// <param name="imageGuid">圖片GUID</param>
        /// <returns>各種尺寸的URL字典</returns>
        private Dictionary<string, string> GenerateImageUrls(ImageCategory category, int entityId, Guid imageGuid)
        {
            var urls = new Dictionary<string, string>();
            var basePath = $"{category.ToString().ToLowerInvariant()}/{entityId}";

            // 生成各種尺寸的URL
            foreach (var size in new[] { "thumbnail", "medium", "large", "original" })
            {
                urls[size] = $"/api/images/{basePath}/{size}/{imageGuid:N}.webp";
            }

            return urls;
        }

        /// <summary>
        /// 將 Property 實體轉換為 PropertyCreateDto
        /// </summary>
        /// <param name="property">房源實體</param>
        /// <returns>房源建立DTO</returns>
        private PropertyCreateDto MapPropertyToCreateDto(Property property)
        {
            return new PropertyCreateDto
            {
                // 編輯模式：設定房源ID
                PropertyId = property.PropertyId,
                
                // 基本資訊
                Title = property.Title,
                Description = property.Description ?? string.Empty,
                
                // 地址資訊
                CityId = property.CityId,
                DistrictId = property.DistrictId,
                AddressLine = property.AddressLine ?? string.Empty,
                
                // 價格資訊
                MonthlyRent = property.MonthlyRent,
                DepositAmount = property.DepositAmount,
                DepositMonths = property.DepositMonths,
                
                // 房屋規格
                RoomCount = property.RoomCount,
                LivingRoomCount = property.LivingRoomCount,
                BathroomCount = property.BathroomCount,
                CurrentFloor = property.CurrentFloor,
                TotalFloors = property.TotalFloors,
                Area = property.Area,
                
                // 租賃條件
                MinimumRentalMonths = property.MinimumRentalMonths,
                SpecialRules = property.SpecialRules ?? string.Empty,
                
                // 費用設定
                WaterFeeType = property.WaterFeeType ?? "台水",
                CustomWaterFee = property.CustomWaterFee,
                ElectricityFeeType = property.ElectricityFeeType ?? "台電",
                CustomElectricityFee = property.CustomElectricityFee,
                ManagementFeeIncluded = property.ManagementFeeIncluded,
                ManagementFeeAmount = property.ManagementFeeAmount,
                
                // 停車與清潔
                ParkingAvailable = property.ParkingAvailable,
                ParkingFeeRequired = property.ParkingFeeRequired,
                ParkingFeeAmount = property.ParkingFeeAmount,
                CleaningFeeRequired = property.CleaningFeeRequired,
                CleaningFeeAmount = property.CleaningFeeAmount,
                
                // 刊登資訊
                ListingPlanId = property.ListingPlanId ?? 1, // 使用預設方案ID
                PropertyProofUrl = property.PropertyProofUrl ?? string.Empty,
                
                // 設備資訊
                SelectedEquipmentIds = property.PropertyEquipmentRelations
                    .Select(r => r.CategoryId)
                    .ToList(),
                EquipmentQuantities = property.PropertyEquipmentRelations
                    .ToDictionary(r => r.CategoryId, r => r.Quantity)
            };
        }

        /// <summary>
        /// 更新房源基本資料
        /// </summary>
        private async Task UpdatePropertyFromDto(int propertyId, PropertyCreateDto dto)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
            {
                throw new InvalidOperationException($"房源不存在: {propertyId}");
            }
            
            // 更新房源資料
            property.Title = dto.Title;
            property.Description = dto.Description;
            property.CityId = dto.CityId;
            property.DistrictId = dto.DistrictId;
            property.AddressLine = dto.AddressLine;
            property.MonthlyRent = dto.MonthlyRent;
            property.DepositAmount = dto.DepositAmount;
            property.DepositMonths = dto.DepositMonths;
            property.RoomCount = dto.RoomCount;
            property.LivingRoomCount = dto.LivingRoomCount;
            property.BathroomCount = dto.BathroomCount;
            property.CurrentFloor = dto.CurrentFloor;
            property.TotalFloors = dto.TotalFloors;
            property.Area = dto.Area;
            property.MinimumRentalMonths = dto.MinimumRentalMonths;
            property.SpecialRules = dto.SpecialRules;
            property.WaterFeeType = dto.WaterFeeType;
            property.CustomWaterFee = dto.CustomWaterFee;
            property.ElectricityFeeType = dto.ElectricityFeeType;
            property.CustomElectricityFee = dto.CustomElectricityFee;
            property.ManagementFeeIncluded = dto.ManagementFeeIncluded;
            property.ManagementFeeAmount = dto.ManagementFeeAmount;
            property.ParkingAvailable = dto.ParkingAvailable;
            property.ParkingFeeRequired = dto.ParkingFeeRequired;
            property.ParkingFeeAmount = dto.ParkingFeeAmount;
            property.CleaningFeeRequired = dto.CleaningFeeRequired;
            property.CleaningFeeAmount = dto.CleaningFeeAmount;
            property.PropertyProofUrl = dto.PropertyProofUrl;
            property.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 更新房源設備關聯
        /// </summary>
        private async Task UpdatePropertyEquipmentRelations(int propertyId, PropertyCreateDto dto)
        {
            // 移除現有關聯
            var existingRelations = await _context.PropertyEquipmentRelations
                .Where(r => r.PropertyId == propertyId)
                .ToListAsync();
            
            _context.PropertyEquipmentRelations.RemoveRange(existingRelations);
            
            // 建立新關聯
            var newRelations = new List<PropertyEquipmentRelation>();
            foreach (var equipmentId in dto.SelectedEquipmentIds)
            {
                var quantity = dto.EquipmentQuantities?.TryGetValue(equipmentId, out var qty) == true ? qty : 1;
                
                newRelations.Add(new PropertyEquipmentRelation
                {
                    PropertyId = propertyId,
                    CategoryId = equipmentId,
                    Quantity = quantity
                });
            }
            
            _context.PropertyEquipmentRelations.AddRange(newRelations);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 從 PropertyCreateDto 建立預覽用的 PropertyDetailViewModel
        /// </summary>
        private async Task<PropertyDetailViewModel> CreatePreviewViewModelFromDto(PropertyCreateDto dto)
        {
            // 取得城市區域名稱
            var cityName = string.Empty;
            var districtName = string.Empty;

            if (dto.CityId > 0)
            {
                cityName = await _context.Cities
                    .Where(c => c.CityId == dto.CityId)
                    .Select(c => c.CityName)
                    .FirstOrDefaultAsync() ?? "未指定";
            }

            if (dto.DistrictId > 0)
            {
                districtName = await _context.Districts
                    .Where(d => d.DistrictId == dto.DistrictId)
                    .Select(d => d.DistrictName)
                    .FirstOrDefaultAsync() ?? "未指定";
            }

            // 取得選中的設備資訊
            var equipmentList = new List<PropertyEquipmentViewModel>();
            if (dto.SelectedEquipmentIds != null && dto.SelectedEquipmentIds.Any())
            {
                var equipmentData = await _context.PropertyEquipmentCategories
                    .Where(pec => dto.SelectedEquipmentIds.Contains(pec.CategoryId))
                    .Include(pec => pec.ParentCategory)
                    .AsNoTracking()
                    .ToListAsync();

                equipmentList = equipmentData.Select(eq => new PropertyEquipmentViewModel
                {
                    EquipmentName = eq.CategoryName,
                    EquipmentType = eq.ParentCategory?.CategoryName ?? eq.CategoryName,
                    Quantity = dto.EquipmentQuantities?.TryGetValue(eq.CategoryId, out var qty) == true ? qty : 1,
                    Condition = "良好"
                }).ToList();
            }

            // 建立預覽 ViewModel
            var viewModel = new PropertyDetailViewModel
            {
                PropertyId = 0, // 預覽用，沒有實際 ID
                Title = dto.Title ?? "房源預覽",
                Description = dto.Description ?? "暫無描述",
                Price = dto.MonthlyRent,
                Address = dto.AddressLine ?? "地址未填寫",
                CityName = cityName,
                DistrictName = districtName,
                LandlordName = "預覽模式",
                LandlordPhone = "請洽客服",
                LandlordEmail = "preview@zuhause.com",
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsFavorite = false,
                ViewCount = 0,
                FavoriteCount = 0,
                ApplicationCount = 0,
                Images = new List<ImageDisplayDto>
                {
                    new ImageDisplayDto
                    {
                        ImageId = 0,
                        ImagePath = "/images/property-preview-placeholder.jpg",
                        Category = "預覽圖片",
                        IsMainImage = true,
                        DisplayOrder = 1
                    }
                },
                Equipment = equipmentList,
                HouseInfo = new PropertyInfoSection
                {
                    PropertyType = "住宅",
                    Floor = dto.TotalFloors > 0 && dto.CurrentFloor > 0 ? $"{dto.CurrentFloor}/{dto.TotalFloors}樓" : "未填寫",
                    Area = dto.Area > 0 ? $"{dto.Area}坪" : "未填寫",
                    Rooms = dto.RoomCount > 0 ? $"{dto.RoomCount}房" : "未填寫",
                    Bathrooms = dto.BathroomCount > 0 ? $"{dto.BathroomCount}衛" : "未填寫",
                    Balcony = dto.LivingRoomCount > 0 ? $"{dto.LivingRoomCount}廳" : "未填寫",
                    Parking = dto.ParkingAvailable ? "有" : "無",
                    Direction = "預覽模式",
                    Age = 0
                },
                RulesAndFees = new PropertyRulesSection
                {
                    MonthlyRent = dto.MonthlyRent,
                    Deposit = dto.DepositAmount,
                    ManagementFee = dto.ManagementFeeAmount ?? 0,
                    UtilityDeposit = 0,
                    LeaseMinimum = dto.MinimumRentalMonths > 0 ? $"{dto.MinimumRentalMonths}個月" : "未指定",
                    PaymentTerms = $"押{dto.DepositMonths}付1",
                    HouseRules = !string.IsNullOrEmpty(dto.SpecialRules) ? 
                        new List<string> { dto.SpecialRules } : new List<string>(),
                    AllowPets = false,
                    AllowSmoking = false,
                    AllowCooking = true
                },
                Location = new PropertyLocationSection
                {
                    Latitude = 25.0330,
                    Longitude = 121.5654,
                    NearbyTransport = "預覽模式 - 交通資訊",
                    NearbySchools = "預覽模式 - 學校資訊", 
                    NearbyShopping = "預覽模式 - 購物資訊",
                    NearbyHospitals = "預覽模式 - 醫療資訊",
                    NearbyAttractions = new List<string> { "預覽模式", "景點資訊" }
                }
            };

            return viewModel;
        }

        /// <summary>
        /// 驗證房源所有權
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="userId">用戶ID</param>
        /// <returns>是否為房源擁有者</returns>
        private async Task<bool> ValidatePropertyOwnership(int propertyId, int userId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId 
                                        && p.LandlordMemberId == userId
                                        && p.DeletedAt == null);
            
            return property != null;
        }

        /// <summary>
        /// 處理臨時會話圖片遷移（兩階段上傳流程）
        /// 從臨時儲存區域遷移圖片到正式房源儲存區域
        /// </summary>
        /// <summary>
        /// 在交易內處理臨時圖片遷移 - 新版本，支援交易內執行
        /// </summary>
        private async Task<(bool Success, string ErrorMessage, List<string> MovedBlobPaths)> ProcessTempImageMigrationInTransactionAsync(int propertyId, string tempSessionId, IEnumerable<string>? imageOrder = null)
        {
            var movedBlobPaths = new List<string>();
            
            try
            {
                _logger.LogInformation("🔄 [交易內] 開始處理臨時圖片遷移，房源ID: {PropertyId}, TempSessionId: {TempSessionId}", 
                    propertyId, tempSessionId);

                // 1. 驗證臨時會話有效性
                _logger.LogInformation("🔍 [交易內] 驗證臨時會話有效性: {TempSessionId}", tempSessionId);
                
                var isValidSession = await _tempSessionService.IsValidTempSessionAsync(tempSessionId);
                _logger.LogInformation("✅ [交易內] 臨時會話驗證結果: {IsValid}", isValidSession);
                
                if (!isValidSession)
                {
                    var errorMessage = $"無效的臨時會話ID: {tempSessionId}";
                    _logger.LogError("❌ [交易內] 臨時會話驗證失敗: {ErrorMessage}", errorMessage);
                    return (false, errorMessage, movedBlobPaths);
                }

                // 2. 取得臨時圖片列表
                _logger.LogInformation("📋 [交易內] 取得臨時圖片列表: {TempSessionId}", tempSessionId);
                
                var tempImages = await _tempSessionService.GetTempImagesAsync(tempSessionId);
                _logger.LogInformation("📊 [交易內] 臨時圖片清單取得成功，數量: {Count}", tempImages.Count);
                
                foreach (var tempImg in tempImages)
                {
                    _logger.LogInformation("  - 圖片: {ImageGuid}, 分類: {Category}, 檔名: {FileName}", 
                        tempImg.ImageGuid, tempImg.Category, tempImg.OriginalFileName);
                }
                
                if (!tempImages.Any())
                {
                    _logger.LogWarning("⚠️ [交易內] 臨時會話中沒有圖片需要遷移，TempSessionId: {TempSessionId}", tempSessionId);
                    return (true, "", movedBlobPaths);
                }

                var galleryImages = tempImages.Where(img => img.Category == ImageCategory.Gallery).ToList();
                var documentImages = tempImages.Where(img => img.Category == ImageCategory.Document).ToList();
                
                _logger.LogInformation("📂 [交易內] 圖片分類統計 - Gallery: {GalleryCount}, Document: {DocumentCount}", 
                    galleryImages.Count, documentImages.Count);

                // 3. 遷移相簿圖片（Gallery）- 在同一交易內
                if (galleryImages.Any())
                {
                    _logger.LogInformation("🖼️ [交易內] 開始遷移 {Count} 張相簿圖片", galleryImages.Count);
                    
                    var galleryGuids = galleryImages.Select(img => img.ImageGuid).ToList();
                    _logger.LogInformation("🔄 [交易內] 調用 BlobMigrationService.MoveTempToPermanentAsync - Gallery");
                    
                    var galleryMigrationResult = await _blobMigrationService.MoveTempToPermanentAsync(
                        tempSessionId,
                        galleryGuids,
                        ImageCategory.Gallery,
                        propertyId,
                        imageOrder
                    );
                    
                    _logger.LogInformation("📊 [交易內] Gallery 遷移結果: Success={Success}, ErrorMessage={ErrorMessage}", 
                        galleryMigrationResult.IsSuccess, galleryMigrationResult.ErrorMessage);

                    if (!galleryMigrationResult.IsSuccess)
                    {
                        var errorMessage = $"相簿圖片遷移失敗: {galleryMigrationResult.ErrorMessage}";
                        _logger.LogError("❌ [交易內] 相簿圖片遷移失敗: {ErrorMessage}", errorMessage);
                        return (false, errorMessage, movedBlobPaths);
                    }

                    movedBlobPaths.AddRange(galleryMigrationResult.MovedFilePaths);
                    
                    _logger.LogInformation("✅ [交易內] 成功遷移 {Count} 張相簿圖片，房源ID: {PropertyId}", 
                        galleryImages.Count, propertyId);
                }

                // 4. 遷移證明文件（Document）- 在同一交易內
                if (documentImages.Any())
                {
                    _logger.LogInformation("📄 [交易內] 開始遷移 {Count} 個證明文件", documentImages.Count);
                    
                    var documentGuids = documentImages.Select(img => img.ImageGuid).ToList();
                    _logger.LogInformation("🔄 [交易內] 調用 BlobMigrationService.MoveTempToPermanentAsync - Document");
                    
                    var documentMigrationResult = await _blobMigrationService.MoveTempToPermanentAsync(
                        tempSessionId,
                        documentGuids,
                        ImageCategory.Document,
                        propertyId
                    );
                    
                    _logger.LogInformation("📊 [交易內] Document 遷移結果: Success={Success}, ErrorMessage={ErrorMessage}", 
                        documentMigrationResult.IsSuccess, documentMigrationResult.ErrorMessage);

                    if (!documentMigrationResult.IsSuccess)
                    {
                        var errorMessage = $"證明文件遷移失敗: {documentMigrationResult.ErrorMessage}";
                        _logger.LogError("❌ [交易內] 證明文件遷移失敗: {ErrorMessage}", errorMessage);
                        return (false, errorMessage, movedBlobPaths);
                    }

                    movedBlobPaths.AddRange(documentMigrationResult.MovedFilePaths);
                    
                    _logger.LogInformation("✅ [交易內] 證明文件遷移完成，PropertyProofUrl 已準備更新");
                    _logger.LogInformation("✅ [交易內] 成功遷移 {Count} 個證明文件，房源ID: {PropertyId}", 
                        documentImages.Count, propertyId);
                }

                // 5. 清理臨時會話（在交易提交後進行）
                _logger.LogInformation("ℹ️ [交易內] 臨時會話清理將在交易提交後進行: {TempSessionId}", tempSessionId);

                _logger.LogInformation("🎉 [交易內] 臨時圖片遷移完成，房源ID: {PropertyId}, TempSessionId: {TempSessionId}, 總計: {Total} 個檔案", 
                    propertyId, tempSessionId, tempImages.Count);
                    
                return (true, "", movedBlobPaths);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [交易內] 臨時圖片遷移過程中發生異常");
                return (false, ex.Message, movedBlobPaths);
            }
        }

        /// <summary>
        /// 舊版本方法 - 保持向後相容性
        /// </summary>
        private async Task ProcessTempImageMigrationAsync(int propertyId, string tempSessionId, IEnumerable<string>? imageOrder = null)
        {
            try
            {
                _logger.LogInformation("🔄 開始處理臨時圖片遷移，房源ID: {PropertyId}, TempSessionId: {TempSessionId}", 
                    propertyId, tempSessionId);

                // 1. 驗證臨時會話有效性
                _logger.LogInformation("🔍 驗證臨時會話有效性: {TempSessionId}", tempSessionId);
                
                try
                {
                    var isValidSession = await _tempSessionService.IsValidTempSessionAsync(tempSessionId);
                    _logger.LogInformation("✅ 臨時會話驗證結果: {IsValid}", isValidSession);
                    
                    if (!isValidSession)
                    {
                        var errorMessage = $"無效的臨時會話ID: {tempSessionId}";
                        _logger.LogError("❌ 臨時會話驗證失敗: {ErrorMessage}", errorMessage);
                        throw new InvalidOperationException(errorMessage);
                    }
                }
                catch (Exception sessionValidationEx)
                {
                    _logger.LogError(sessionValidationEx, "❌ 臨時會話驗證過程中發生異常: {TempSessionId}", tempSessionId);
                    throw;
                }

                // 2. 取得臨時圖片列表
                _logger.LogInformation("📋 取得臨時圖片列表: {TempSessionId}", tempSessionId);
                
                List<TempImageInfo> tempImages;
                try
                {
                    tempImages = await _tempSessionService.GetTempImagesAsync(tempSessionId);
                    _logger.LogInformation("📊 臨時圖片清單取得成功，數量: {Count}", tempImages.Count);
                    
                    foreach (var tempImg in tempImages)
                    {
                        _logger.LogInformation("  - 圖片: {ImageGuid}, 分類: {Category}, 檔名: {FileName}", 
                            tempImg.ImageGuid, tempImg.Category, tempImg.OriginalFileName);
                    }
                }
                catch (Exception getTempImagesEx)
                {
                    _logger.LogError(getTempImagesEx, "❌ 取得臨時圖片列表時發生異常: {TempSessionId}", tempSessionId);
                    throw;
                }
                
                if (!tempImages.Any())
                {
                    _logger.LogWarning("⚠️ 臨時會話中沒有圖片需要遷移，TempSessionId: {TempSessionId}", tempSessionId);
                    return;
                }

                var imageGuids = tempImages.Select(img => img.ImageGuid).ToList();
                var galleryImages = tempImages.Where(img => img.Category == ImageCategory.Gallery).ToList();
                var documentImages = tempImages.Where(img => img.Category == ImageCategory.Document).ToList();
                
                _logger.LogInformation("📂 圖片分類統計 - Gallery: {GalleryCount}, Document: {DocumentCount}", 
                    galleryImages.Count, documentImages.Count);

                // 3. 遷移相簿圖片（Gallery）
                if (galleryImages.Any())
                {
                    _logger.LogInformation("🖼️ 開始遷移 {Count} 張相簿圖片", galleryImages.Count);
                    
                    try
                    {
                        var galleryGuids = galleryImages.Select(img => img.ImageGuid).ToList();
                        _logger.LogInformation("🔄 調用 BlobMigrationService.MoveTempToPermanentAsync - Gallery");
                        
                        var galleryMigrationResult = await _blobMigrationService.MoveTempToPermanentAsync(
                            tempSessionId,
                            galleryGuids,
                            ImageCategory.Gallery,
                            propertyId,
                            imageOrder
                        );
                        
                        _logger.LogInformation("📊 Gallery 遷移結果: Success={Success}, ErrorMessage={ErrorMessage}", 
                            galleryMigrationResult.IsSuccess, galleryMigrationResult.ErrorMessage);

                        if (!galleryMigrationResult.IsSuccess)
                        {
                            var errorMessage = $"相簿圖片遷移失敗: {galleryMigrationResult.ErrorMessage}";
                            _logger.LogError("❌ 相簿圖片遷移失敗: {ErrorMessage}", errorMessage);
                            throw new InvalidOperationException(errorMessage);
                        }

                        _logger.LogInformation("✅ 成功遷移 {Count} 張相簿圖片，房源ID: {PropertyId}", 
                            galleryImages.Count, propertyId);
                    }
                    catch (Exception galleryMigrationEx)
                    {
                        _logger.LogError(galleryMigrationEx, "❌ 相簿圖片遷移過程中發生異常");
                        throw;
                    }
                }

                // 4. 遷移證明文件（Document）
                if (documentImages.Any())
                {
                    _logger.LogInformation("📄 開始遷移 {Count} 個證明文件", documentImages.Count);
                    
                    try
                    {
                        var documentGuids = documentImages.Select(img => img.ImageGuid).ToList();
                        _logger.LogInformation("🔄 調用 BlobMigrationService.MoveTempToPermanentAsync - Document");
                        
                        var documentMigrationResult = await _blobMigrationService.MoveTempToPermanentAsync(
                            tempSessionId,
                            documentGuids,
                            ImageCategory.Document,
                            propertyId
                        );
                        
                        _logger.LogInformation("📊 Document 遷移結果: Success={Success}, ErrorMessage={ErrorMessage}", 
                            documentMigrationResult.IsSuccess, documentMigrationResult.ErrorMessage);

                        if (!documentMigrationResult.IsSuccess)
                        {
                            var errorMessage = $"證明文件遷移失敗: {documentMigrationResult.ErrorMessage}";
                            _logger.LogError("❌ 證明文件遷移失敗: {ErrorMessage}", errorMessage);
                            throw new InvalidOperationException(errorMessage);
                        }

                        // PropertyProofUrl 已由 BlobMigrationService 自動處理，無需額外更新
                        _logger.LogInformation("✅ 證明文件遷移完成，PropertyProofUrl 已自動更新");

                        _logger.LogInformation("✅ 成功遷移 {Count} 個證明文件，房源ID: {PropertyId}", 
                            documentImages.Count, propertyId);
                    }
                    catch (Exception documentMigrationEx)
                    {
                        _logger.LogError(documentMigrationEx, "❌ 證明文件遷移過程中發生異常");
                        throw;
                    }
                }

                // 5. 清理臨時會話
                _logger.LogInformation("🧹 清理臨時會話: {TempSessionId}", tempSessionId);
                
                try
                {
                    await _tempSessionService.InvalidateTempSessionAsync(tempSessionId);
                    _logger.LogInformation("✅ 臨時會話清理完成: {TempSessionId}", tempSessionId);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "❌ 清理臨時會話時發生異常: {TempSessionId}", tempSessionId);
                    throw;
                }

                _logger.LogInformation("🎉 臨時圖片遷移完成，房源ID: {PropertyId}, TempSessionId: {TempSessionId}, 總計: {Total} 個檔案", 
                    propertyId, tempSessionId, tempImages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 臨時圖片遷移失敗，房源ID: {PropertyId}, TempSessionId: {TempSessionId}, 異常類型: {ExceptionType}, 異常訊息: {ExceptionMessage}", 
                    propertyId, tempSessionId, ex.GetType().Name, ex.Message);
                
                if (ex.InnerException != null)
                {
                    _logger.LogError("💥 內部異常: {InnerExceptionType} - {InnerExceptionMessage}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                
                _logger.LogError("💥 堆疊追蹤: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// 房源座標自動補全 API
        /// </summary>
        [HttpPost]
        [Route("api/property/coordinate-completion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CoordinateCompletion([FromBody] CoordinateCompletionRequest request)
        {
            try
            {
                _logger.LogInformation("🗺️ 開始房源座標自動補全: PropertyId={PropertyId}, Address={Address}", 
                    request.PropertyId, request.Address);

                // 驗證請求參數
                if (request.PropertyId <= 0 || string.IsNullOrWhiteSpace(request.Address))
                {
                    return Json(new { success = false, message = "無效的房源ID或地址" });
                }

                // 查詢房源是否存在且需要補全座標
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId && p.DeletedAt == null);

                if (property == null)
                {
                    return Json(new { success = false, message = "找不到指定的房源" });
                }

                // 檢查是否確實需要補全座標
                if (property.Latitude.HasValue && property.Longitude.HasValue && 
                    property.Latitude != 0 && property.Longitude != 0)
                {
                    _logger.LogInformation("房源座標已存在，無需補全: PropertyId={PropertyId}", request.PropertyId);
                    return Json(new { success = true, message = "房源座標已存在", skipped = true });
                }

                // 使用 GoogleMapsService 進行地理編碼
                var geocodingRequest = new zuHause.DTOs.GoogleMaps.GeocodingRequest
                {
                    Address = request.Address,
                    Language = "zh-TW",
                    Region = "TW"
                };

                var geocodingResult = await _googleMapsService.GeocodeAsync(geocodingRequest);

                if (geocodingResult.IsSuccess && geocodingResult.Latitude.HasValue && geocodingResult.Longitude.HasValue)
                {
                    // 更新房源座標
                    property.Latitude = (decimal)geocodingResult.Latitude.Value;
                    property.Longitude = (decimal)geocodingResult.Longitude.Value;
                    property.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ 房源座標補全成功: PropertyId={PropertyId}, Lat={Lat}, Lng={Lng}", 
                        request.PropertyId, geocodingResult.Latitude.Value, geocodingResult.Longitude.Value);

                    return Json(new 
                    { 
                        success = true, 
                        message = "座標補全成功",
                        latitude = geocodingResult.Latitude.Value,
                        longitude = geocodingResult.Longitude.Value
                    });
                }
                else
                {
                    _logger.LogWarning("⚠️ 房源座標轉換失敗: PropertyId={PropertyId}, Error={Error}", 
                        request.PropertyId, geocodingResult.ErrorMessage);

                    return Json(new 
                    { 
                        success = false, 
                        message = geocodingResult.ErrorMessage ?? "無法轉換地址為座標" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 房源座標補全發生異常: PropertyId={PropertyId}", request.PropertyId);
                
                return Json(new 
                { 
                    success = false, 
                    message = "座標補全服務暫時無法使用，請稍後再試" 
                });
            }
        }

    }

    /// <summary>
    /// 座標補全請求模型
    /// </summary>
    public class CoordinateCompletionRequest
    {
        public int PropertyId { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}