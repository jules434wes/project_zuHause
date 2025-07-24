using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;

namespace zuHause.Controllers
{
    /// <summary>
    /// 測試資料查詢控制器 - 僅用於開發測試階段
    /// </summary>
    [ApiController]
    [Route("api/test-data")]
    public class TestDataController : ControllerBase
    {
        private readonly ZuHauseContext _context;

        public TestDataController(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 獲取資料庫測試資料報告
        /// </summary>
        [HttpGet("report")]
        public async Task<ActionResult> GetTestDataReport()
        {
            try
            {
                // 1. 查詢城市數量
                var cityCount = await _context.Cities.CountAsync();
                var cities = await _context.Cities
                    .Where(c => c.IsActive == true)
                    .OrderBy(c => c.DisplayOrder)
                    .Take(5)
                    .Select(c => new { c.CityId, c.CityName, c.CityCode })
                    .ToListAsync();

                // 2. 查詢區域數量
                var districtCount = await _context.Districts.CountAsync();
                var districts = await _context.Districts
                    .Where(d => d.IsActive == true)
                    .OrderBy(d => d.CityId)
                    .ThenBy(d => d.DisplayOrder)
                    .Take(5)
                    .Select(d => new { d.DistrictId, d.DistrictName, d.CityId })
                    .ToListAsync();

                // 3. 查詢 memberID = 108
                var member108 = await _context.Members
                    .Where(m => m.MemberId == 108)
                    .Select(m => new { 
                        m.MemberId, 
                        m.MemberName, 
                        m.MemberTypeId, 
                        m.IsLandlord, 
                        m.IsActive,
                        m.Email 
                    })
                    .FirstOrDefaultAsync();

                // 4. 查詢刊登方案
                var listingPlans = await _context.ListingPlans
                    .Where(lp => lp.IsActive == true)
                    .OrderBy(lp => lp.PricePerDay * lp.MinListingDays)
                    .Select(lp => new { 
                        lp.PlanId, 
                        lp.PlanName, 
                        lp.MinListingDays, 
                        lp.PricePerDay,
                        TotalPrice = lp.PricePerDay * lp.MinListingDays
                    })
                    .ToListAsync();

                // 5. 查詢設備分類
                var equipmentCount = await _context.PropertyEquipmentCategories
                    .Where(pec => pec.IsActive == true)
                    .CountAsync();
                
                var equipment = await _context.PropertyEquipmentCategories
                    .Where(pec => pec.IsActive == true)
                    .OrderBy(pec => pec.ParentCategoryId ?? 0)
                    .ThenBy(pec => pec.CategoryName)
                    .Take(10)
                    .Select(pec => new { 
                        pec.CategoryId, 
                        pec.CategoryName, 
                        pec.ParentCategoryId 
                    })
                    .ToListAsync();

                // 6. 查詢現有房源
                var properties = await _context.Properties
                    .Select(p => new { 
                        p.PropertyId, 
                        p.Title, 
                        p.StatusCode,
                        p.LandlordMemberId,
                        p.CreatedAt 
                    })
                    .ToListAsync();

                // 7. 產生測試參數組合建議
                var recommendations = new
                {
                    CityId = cities.FirstOrDefault()?.CityId,
                    CityName = cities.FirstOrDefault()?.CityName,
                    DistrictId = districts.FirstOrDefault(d => d.CityId == cities.FirstOrDefault()?.CityId)?.DistrictId,
                    DistrictName = districts.FirstOrDefault(d => d.CityId == cities.FirstOrDefault()?.CityId)?.DistrictName,
                    ListingPlanId = listingPlans.FirstOrDefault()?.PlanId,
                    ListingPlanName = listingPlans.FirstOrDefault()?.PlanName,
                    EquipmentIds = equipment.Take(3).Select(e => e.CategoryId).ToList(),
                    ExistingPropertyId = properties.FirstOrDefault()?.PropertyId
                };

                var report = new
                {
                    Timestamp = DateTime.Now,
                    Summary = new
                    {
                        CitiesCount = cityCount,
                        DistrictsCount = districtCount,
                        Member108Exists = member108 != null,
                        Member108Valid = member108 != null && member108.MemberTypeId == 2 && member108.IsLandlord && member108.IsActive,
                        ListingPlansCount = listingPlans.Count,
                        EquipmentCategoriesCount = equipmentCount,
                        PropertiesCount = properties.Count
                    },
                    Data = new
                    {
                        Cities = cities,
                        Districts = districts,
                        Member108 = member108,
                        ListingPlans = listingPlans,
                        Equipment = equipment,
                        Properties = properties
                    },
                    TestRecommendations = recommendations,
                    ValidationStatus = new
                    {
                        CitiesOk = cityCount > 0,
                        DistrictsOk = districtCount > 0,
                        Member108Ok = member108 != null && member108.MemberTypeId == 2 && member108.IsLandlord && member108.IsActive,
                        ListingPlansOk = listingPlans.Any(),
                        EquipmentOk = equipmentCount > 0,
                        PropertiesExist = properties.Any(),
                        ReadyForTesting = cityCount > 0 && districtCount > 0 && 
                                        (member108 != null && member108.MemberTypeId == 2 && member108.IsLandlord && member108.IsActive) &&
                                        listingPlans.Any() && equipmentCount > 0
                    }
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = ex.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }

        /// <summary>
        /// 測試創建房源功能（詳細診斷版本）
        /// </summary>
        [HttpPost("test-create-property")]
        public async Task<ActionResult> TestCreateProperty()
        {
            try
            {
                // 使用經過驗證的測試參數
                var testDto = new zuHause.DTOs.PropertyCreateDto
                {
                    Title = $"測試房源API診斷-{DateTime.Now:HHmmss}",
                    Description = "這是一個測試用的房源描述",
                    CityId = 2,
                    DistrictId = 2,
                    AddressLine = $"測試地址-{DateTime.Now:HHmmss}號",
                    MonthlyRent = 15000,
                    DepositAmount = 30000,
                    DepositMonths = 2,
                    RoomCount = 1,
                    LivingRoomCount = 1,
                    BathroomCount = 1,
                    CurrentFloor = 2,
                    TotalFloors = 4,
                    Area = 12.0m,
                    MinimumRentalMonths = 6,
                    WaterFeeType = "台水",
                    ElectricityFeeType = "台電",
                    ManagementFeeIncluded = true,
                    ParkingAvailable = false,
                    ParkingFeeRequired = false,
                    CleaningFeeRequired = false,
                    ListingPlanId = 2,
                    SelectedEquipmentIds = new List<int> { 5 },
                    EquipmentQuantities = new Dictionary<int, int> { { 5, 1 } }
                };

                var currentUserId = 108;

                // Step 1: 驗證基礎資料
                var cityExists = await _context.Cities.AnyAsync(c => c.CityId == testDto.CityId && c.IsActive);
                var districtExists = await _context.Districts.AnyAsync(d => d.DistrictId == testDto.DistrictId && d.CityId == testDto.CityId && d.IsActive);
                var memberExists = await _context.Members.AnyAsync(m => m.MemberId == currentUserId && m.IsLandlord && m.IsActive);
                var planExists = await _context.ListingPlans.AnyAsync(lp => lp.PlanId == testDto.ListingPlanId && lp.IsActive);

                // Step 2: 嘗試創建 Property 實體 (讓 EF 自動生成 ID)
                var listingPlan = await _context.ListingPlans.FirstAsync(lp => lp.PlanId == testDto.ListingPlanId);
                var now = DateTime.Now;
                var listingFee = listingPlan.PricePerDay * listingPlan.MinListingDays;
                var expireDate = now.AddDays(listingPlan.MinListingDays + 1).Date;

                var property = new zuHause.Models.Property
                {
                    // 不設置 PropertyId，讓資料庫自動生成
                    LandlordMemberId = currentUserId,
                    Title = testDto.Title,
                    Description = testDto.Description,
                    CityId = testDto.CityId,
                    DistrictId = testDto.DistrictId,
                    AddressLine = testDto.AddressLine,
                    MonthlyRent = testDto.MonthlyRent,
                    DepositAmount = testDto.DepositAmount,
                    DepositMonths = testDto.DepositMonths,
                    RoomCount = testDto.RoomCount,
                    LivingRoomCount = testDto.LivingRoomCount,
                    BathroomCount = testDto.BathroomCount,
                    CurrentFloor = testDto.CurrentFloor,
                    TotalFloors = testDto.TotalFloors,
                    Area = testDto.Area,
                    MinimumRentalMonths = testDto.MinimumRentalMonths,
                    WaterFeeType = testDto.WaterFeeType,
                    ElectricityFeeType = testDto.ElectricityFeeType,
                    ManagementFeeIncluded = testDto.ManagementFeeIncluded,
                    ParkingAvailable = testDto.ParkingAvailable,
                    ParkingFeeRequired = testDto.ParkingFeeRequired,
                    CleaningFeeRequired = testDto.CleaningFeeRequired,
                    ListingDays = listingPlan.MinListingDays,
                    ListingFeeAmount = listingFee,
                    ListingPlanId = testDto.ListingPlanId,
                    StatusCode = "DRAFT",
                    IsPaid = false,
                    ExpireAt = expireDate,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                var result = new
                {
                    success = true,
                    message = "測試創建房源成功",
                    propertyId = property.PropertyId,
                    diagnostics = new
                    {
                        cityExists,
                        districtExists,
                        memberExists,
                        planExists,
                        currentUserId,
                        listingPlan = new { listingPlan.PlanId, listingPlan.PlanName, listingPlan.MinListingDays, listingPlan.PricePerDay },
                        calculatedValues = new { listingFee, expireDate }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "測試創建房源時發生錯誤",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace?.Split('\n').Take(10).ToArray()
                });
            }
        }

        /// <summary>
        /// 建立測試用房東 (memberID = 108，如果不存在)
        /// </summary>
        [HttpPost("create-test-landlord")]
        public async Task<ActionResult> CreateTestLandlord()
        {
            try
            {
                // 檢查是否已存在
                var existingMember = await _context.Members
                    .FirstOrDefaultAsync(m => m.MemberId == 108);

                if (existingMember != null)
                {
                    return Ok(new 
                    { 
                        success = false,
                        message = "memberID = 108 已存在",
                        member = new 
                        {
                            existingMember.MemberId,
                            existingMember.MemberName,
                            existingMember.MemberTypeId,
                            existingMember.IsLandlord,
                            existingMember.IsActive
                        }
                    });
                }

                // 確保會員類型 2 (房東) 存在
                var landlordType = await _context.MemberTypes
                    .FirstOrDefaultAsync(mt => mt.MemberTypeId == 2);

                if (landlordType == null)
                {
                    return BadRequest(new 
                    { 
                        success = false,
                        message = "會員類型 2 (房東) 不存在，無法建立測試房東" 
                    });
                }

                // 建立測試房東
                var testLandlord = new Member
                {
                    MemberId = 108,
                    MemberName = "測試房東108",
                    Gender = 1,
                    BirthDate = new DateOnly(1980, 1, 1),
                    Password = "TestPassword123!",
                    PhoneNumber = "0912345678",
                    Email = "test.landlord.108@zuhause.test",
                    MemberTypeId = 2,
                    IsLandlord = true,
                    IsActive = true,
                    NationalIdNo = "A123456789",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Members.Add(testLandlord);
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    success = true,
                    message = "成功建立測試房東 memberID = 108",
                    member = new 
                    {
                        testLandlord.MemberId,
                        testLandlord.MemberName,
                        testLandlord.MemberTypeId,
                        testLandlord.IsLandlord,
                        testLandlord.IsActive,
                        testLandlord.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "建立測試房東時發生錯誤",
                    error = ex.Message 
                });
            }
        }
    }
}