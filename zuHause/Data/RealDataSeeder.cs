using Microsoft.EntityFrameworkCore;
using zuHause.Models;

namespace zuHause.Data
{
    /// <summary>
    /// 真實資料播種器 - 用於建立測試環境的真實資料
    /// 遵循真實用戶操作流程，確保資料的業務邏輯正確性
    /// </summary>
    public class RealDataSeeder
    {
        private readonly ZuHauseContext _context;
        private readonly ILogger<RealDataSeeder> _logger;

        public RealDataSeeder(ZuHauseContext context, ILogger<RealDataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 確保基礎資料存在 - 只新增缺失的資料，不刪除現有資料
        /// </summary>
        public async Task EnsureDataAsync()
        {
            _logger.LogInformation("開始確保基礎資料存在...");

            try
            {
                // 確保基礎參考資料存在
                await EnsureReferenceDataAsync();

                _logger.LogInformation("基礎資料確保完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "確保基礎資料時發生錯誤");
                throw;
            }
        }


        /// <summary>
        /// 確保參考資料存在 (城市、區域、會員類型等)
        /// </summary>
        private async Task EnsureReferenceDataAsync()
        {
            // 檢查並建立會員類型
            if (!await _context.MemberTypes.AnyAsync())
            {
                _context.MemberTypes.AddRange(
                    new MemberType 
                    { 
                        MemberTypeId = 1, 
                        TypeName = "租客", 
                        Description = "尋找租屋的用戶",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new MemberType 
                    { 
                        MemberTypeId = 2, 
                        TypeName = "房東", 
                        Description = "提供租屋的用戶",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                );
            }

            // 檢查並建立城市資料
            if (!await _context.Cities.AnyAsync())
            {
                _context.Cities.AddRange(
                    new City 
                    { 
                        CityId = 1,
                        CityCode = "TPE", 
                        CityName = "台北市",
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new City 
                    { 
                        CityId = 2,
                        CityCode = "NTP", 
                        CityName = "新北市",
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new City 
                    { 
                        CityId = 3,
                        CityCode = "TYC", 
                        CityName = "桃園市",
                        DisplayOrder = 3,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                );
            }

            // 檢查並建立區域資料
            if (!await _context.Districts.AnyAsync())
            {
                _context.Districts.AddRange(
                    // 台北市區域
                    new District 
                    { 
                        DistrictId = 1,
                        DistrictCode = "TPE01",
                        DistrictName = "中正區", 
                        CityId = 1,
                        CityCode = "TPE",
                        ZipCode = "100",
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new District 
                    { 
                        DistrictId = 2,
                        DistrictCode = "TPE02",
                        DistrictName = "大同區", 
                        CityId = 1,
                        CityCode = "TPE",
                        ZipCode = "103",
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new District 
                    { 
                        DistrictId = 3,
                        DistrictCode = "TPE03",
                        DistrictName = "中山區", 
                        CityId = 1,
                        CityCode = "TPE",
                        ZipCode = "104",
                        DisplayOrder = 3,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                );
            }

            await _context.SaveChangesAsync();

            // === 確保房源 2001 至少有一筆設備關聯 ===
            try
            {
                var targetProperty = await _context.Properties
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PropertyId == 2001);

                // 只在房源存在且關聯尚未建立時插入
                if (targetProperty != null &&
                    !await _context.PropertyEquipmentRelations.AnyAsync(r => r.PropertyId == 2001))
                {
                    // 取任一啟用中的設備分類作為示範；若無則跳過
                    var firstCategory = await _context.PropertyEquipmentCategories
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.IsActive);

                    if (firstCategory != null)
                    {
                        _context.PropertyEquipmentRelations.Add(new PropertyEquipmentRelation
                        {
                            PropertyId = 2001,
                            CategoryId = firstCategory.CategoryId,
                            Quantity   = 1,
                            CreatedAt  = DateTime.Now,
                            UpdatedAt  = DateTime.Now
                        });

                        await _context.SaveChangesAsync();
                        _logger.LogInformation("已為房源 2001 新增設備分類 {CategoryName} (ID: {CategoryId})", firstCategory.CategoryName, firstCategory.CategoryId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "嘗試為房源 2001 新增設備關聯時發生非致命錯誤");
            }
        }

        /// <summary>
        /// 建立測試房東與其房源 - 供其他測試使用
        /// </summary>
        /// <param name="isDraft">是否建立為草稿狀態</param>
        /// <returns>建立的房東會員</returns>
        public async Task<Member> CreateTestLandlordWithPropertyAsync(bool isDraft = false)
        {
            await EnsureReferenceDataAsync();

            // 建立房東會員
            var landlord = new Member
            {
                MemberName = "張房東",
                Gender = 1, // 男性
                BirthDate = new DateOnly(1980, 5, 15),
                Password = "Test123!",
                PhoneNumber = "0912345678",
                Email = $"landlord_{DateTime.Now:yyyyMMddHHmmss}@test.com",
                MemberTypeId = 2, // 房東
                IsLandlord = true,
                IsActive = true,
                NationalIdNo = $"A12345678{DateTime.Now.Millisecond % 10}",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Members.Add(landlord);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"建立測試房東: {landlord.MemberName} (ID: {landlord.MemberId})");

            // 建立房源 (將在階段 A3 實作)
            if (!isDraft)
            {
                await CreateTestPropertyForLandlordAsync(landlord);
            }

            return landlord;
        }

        /// <summary>
        /// 為房東建立測試房源
        /// </summary>
        private async Task CreateTestPropertyForLandlordAsync(Member landlord)
        {
            var property = new Property
            {
                LandlordMemberId = landlord.MemberId,
                Title = "溫馨套房出租",
                Description = "鄰近捷運站，交通便利，室內空間寬敞明亮，附近有便利商店、餐廳等生活機能完善。",
                MonthlyRent = 15000,
                DepositAmount = 30000,
                DepositMonths = 2,
                CityId = 1, // 台北市
                DistrictId = 1, // 中正區
                AddressLine = "台北市中正區中山南路1號",
                RoomCount = 1,
                LivingRoomCount = 0,
                BathroomCount = 1,
                CurrentFloor = 3,
                TotalFloors = 5,
                Area = 12.5m,
                MinimumRentalMonths = 6,
                WaterFeeType = "按度計算",
                ElectricityFeeType = "按度計算",
                ManagementFeeIncluded = true,
                ParkingAvailable = false,
                ParkingFeeRequired = false,
                CleaningFeeRequired = false,
                StatusCode = "PUBLISHED",
                PublishedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"為房東 {landlord.MemberName} 建立測試房源: {property.Title} (ID: {property.PropertyId})");
        }

        /// <summary>
        /// 建立測試租客 - 供其他測試使用
        /// </summary>
        /// <returns>建立的租客會員</returns>
        public async Task<Member> CreateTestTenantAsync()
        {
            await EnsureReferenceDataAsync();

            // 建立租客會員
            var tenant = new Member
            {
                MemberName = "李租客",
                Gender = 2, // 女性
                BirthDate = new DateOnly(1990, 8, 25),
                Password = "Test123!",
                PhoneNumber = "0987654321",
                Email = $"tenant_{DateTime.Now:yyyyMMddHHmmss}@test.com",
                MemberTypeId = 1, // 租客
                IsLandlord = false,
                IsActive = true,
                PrimaryRentalCityId = 1, // 台北市
                PrimaryRentalDistrictId = 1, // 中正區
                NationalIdNo = $"B98765432{DateTime.Now.Millisecond % 10}",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Members.Add(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"建立測試租客: {tenant.MemberName} (ID: {tenant.MemberId})");

            return tenant;
        }

        /// <summary>
        /// 建立測試圖片檔案 - 供圖片上傳測試使用
        /// </summary>
        /// <returns>測試圖片檔案路徑清單</returns>
        public async Task<List<string>> CreateTestImageFilesAsync()
        {
            // 此方法將在後續圖片上傳功能階段實作
            throw new NotImplementedException("將在後續階段實作");
        }
    }
}