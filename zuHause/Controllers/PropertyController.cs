using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.ViewModels;
using zuHause.Models;
using zuHause.Interfaces;
using zuHause.Helpers;
using zuHause.Enums;
using zuHause.DTOs;
using System.ComponentModel.DataAnnotations;

namespace zuHause.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ZuHauseContext _context;
        private readonly ILogger<PropertyController> _logger;
        private readonly IPropertyImageService _propertyImageService;

        public PropertyController(ZuHauseContext context, ILogger<PropertyController> logger, IPropertyImageService propertyImageService)
        {
            _context = context;
            _logger = logger;
            _propertyImageService = propertyImageService;
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
        /// 顯示房源創建表單
        /// </summary>
        /// <returns>房源創建視圖</returns>
        [HttpGet]
        [Route("property/create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // TODO: 添加身份驗證 - 檢查使用者是否為房東
                // if (!User.IsInRole("Landlord")) return Forbid();

                // 準備下拉選單資料
                var cities = await GetActiveCitiesAsync();
                var listingPlans = await GetActiveListingPlansAsync();
                var equipmentCategories = await GetActiveEquipmentCategoriesAsync();

                var viewModel = new PropertyCreateViewModel
                {
                    Cities = cities,
                    ListingPlans = listingPlans,
                    EquipmentCategories = equipmentCategories,
                    AvailableChineseCategories = PropertyImageCategoryHelper.GetAllPropertyChineseCategories()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入房源創建頁面時發生錯誤");
                TempData["ErrorMessage"] = "載入頁面時發生錯誤，請稍後再試";
                return RedirectToAction("Index");
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
                    return RedirectToAction("Login", "Account");
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
                    // 重新載入下拉選單資料
                    var cities = await GetActiveCitiesAsync();
                    var listingPlans = await GetActiveListingPlansAsync();
                    var equipmentCategories = await GetActiveEquipmentCategoriesAsync();

                    var viewModel = new PropertyCreateViewModel
                    {
                        PropertyData = dto,
                        Cities = cities,
                        ListingPlans = listingPlans,
                        EquipmentCategories = equipmentCategories,
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

                    TempData["SuccessMessage"] = "房源創建成功！您可以繼續上傳房屋圖片。";
                    return RedirectToAction("Detail", new { id = property.PropertyId });
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
                ListingFeeAmount = listingFee,
                ListingPlanId = dto.ListingPlanId,
                PropertyProofUrl = dto.PropertyProofUrl,
                StatusCode = "DRAFT", // 預設為草稿狀態
                IsPaid = false, // 預設未付款
                ExpireAt = expireDate,
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
    }
}