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
        private readonly IListingPlanValidationService _listingPlanValidationService;
        private readonly IEquipmentCategoryQueryService _equipmentCategoryQueryService;

        public PropertyController(
            ZuHauseContext context, 
            ILogger<PropertyController> logger, 
            IPropertyImageService propertyImageService,
            IListingPlanValidationService listingPlanValidationService,
            IEquipmentCategoryQueryService equipmentCategoryQueryService)
        {
            _context = context;
            _logger = logger;
            _propertyImageService = propertyImageService;
            _listingPlanValidationService = listingPlanValidationService;
            _equipmentCategoryQueryService = equipmentCategoryQueryService;
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
                    .Include(p => p.PropertyImages)
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
                        .Select(img => new PropertyImageViewModel
                    {
                        PropertyImageId = (int)img.ImageId,
                        ImagePath = _propertyImageService.GeneratePropertyImageUrl(img.StoredFileName!),
                        ImageDescription = PropertyImageCategoryHelper.GetChineseCategory(img.Category), // 使用中文分類標籤取代檔案名稱
                        IsMainImage = img.DisplayOrder == 1,
                        SortOrder = img.DisplayOrder ?? 0
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
                var properties = await _context.Properties
                    .Include(p => p.LandlordMember)
                    .Include(p => p.PropertyImages)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(20)
                    .Select(p => new PropertySummaryViewModel
                    {
                        PropertyId = p.PropertyId,
                        Title = p.Title,
                        Price = p.MonthlyRent,
                        Address = p.AddressLine ?? "",
                        CityName = "台北市",
                        DistrictName = "大安區",
                        MainImagePath = p.PropertyImages.Any() ? p.PropertyImages.First().ImagePath : "/images/default-property.jpg",
                        CreatedDate = p.CreatedAt,
                        IsFavorite = false,
                        ViewCount = 158
                    })
                    .ToListAsync();

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
        public async Task<IActionResult> Create()
        {
            // 強制禁用快取
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            _logger.LogInformation("用戶訪問房源創建頁面 - IP: {IpAddress}", 
                HttpContext.Connection.RemoteIpAddress);
            
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyCreateDto dto)
        {
            try
            {
                // TODO: 實作真實身份驗證系統
                // 從 Session、JWT Token 或其他認證方式取得當前使用者 ID
                var currentUserId = await GetCurrentUserIdAsync();
                
                if (currentUserId == null)
                {
                    TempData["ErrorMessage"] = "請先登入才能創建房源";
                    return RedirectToAction("Login", "Member");
                }
                
                // 驗證使用者是否為房東
                var isLandlord = await IsUserLandlordAsync(currentUserId.Value);
                if (!isLandlord)
                {
                    TempData["ErrorMessage"] = "只有房東會員才能創建房源";
                    return RedirectToAction("Index", "Home");
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

                    _logger.LogInformation("成功創建房源，房源ID: {PropertyId}, 房東ID: {LandlordId}", 
                        property.PropertyId, currentUserId.Value);

                    TempData["SuccessMessageTitle"] = "房源創建成功！";
                    TempData["SuccessMessageContent"] = "您的房源已成功提交審核，預計 2-3 個工作天完成審核。";
                    return RedirectToAction("CreationSuccess", new { id = property.PropertyId });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建房源時發生錯誤，房東ID: {LandlordId}", 1);
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

            return new Property
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
                Images = new List<PropertyImageViewModel>
                {
                    new PropertyImageViewModel
                    {
                        PropertyImageId = 0,
                        ImagePath = "/images/property-preview-placeholder.jpg",
                        ImageDescription = "預覽圖片",
                        IsMainImage = true,
                        SortOrder = 1
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

    }
}