using Microsoft.EntityFrameworkCore;
using zuHause.Models;

namespace zuHause.Tests.TestHelpers
{
    /// <summary>
    /// 真實資料庫測試資料管理器 - 建立和管理真實的測試資料
    /// 使用真實的 SQL Server 資料庫，確保測試環境與生產環境一致
    /// </summary>
    public static class RealDatabaseSeeder
    {
        /// <summary>
        /// 建立真實的測試會員資料
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="memberName">會員名稱 (預設為測試用名稱)</param>
        /// <returns>建立的會員物件</returns>
        public static async Task<Member> CreateTestMemberAsync(
            ZuHauseContext context, 
            string memberName = "測試房東")
        {
            var testMember = new Member
            {
                MemberName = memberName,
                PhoneNumber = $"09{Random.Shared.Next(10000000, 99999999)}",
                Email = $"test_{DateTime.Now.Ticks}@test.zuhaus.com",
                Password = "TestPassword123",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailVerifiedAt = DateTime.UtcNow,
                PhoneVerifiedAt = DateTime.UtcNow,
                Gender = 1,
                BirthDate = new DateOnly(1990, 1, 1),
                IsActive = true,
                IsLandlord = true,
                MemberTypeId = 1
            };

            context.Members.Add(testMember);
            await context.SaveChangesAsync();
            
            return testMember;
        }

        /// <summary>
        /// 建立真實的測試房源資料
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="landlordMemberId">房東會員ID</param>
        /// <param name="title">房源標題</param>
        /// <returns>建立的房源物件</returns>
        public static async Task<Property> CreateTestPropertyAsync(
            ZuHauseContext context, 
            int landlordMemberId,
            string title = "測試房源")
        {
            // 取得或建立測試用城市和區域
            var testCity = await GetOrCreateTestCityAsync(context);
            var testDistrict = await GetOrCreateTestDistrictAsync(context, testCity.CityId);
            var testListingPlan = await GetOrCreateTestListingPlanAsync(context);

            var testProperty = new Property
            {
                Title = $"{title} - {DateTime.Now:yyyyMMdd_HHmmss}",
                MonthlyRent = 25000 + Random.Shared.Next(0, 10000), // 隨機租金
                CityId = testCity.CityId,
                DistrictId = testDistrict.DistrictId,
                AddressLine = $"測試路{Random.Shared.Next(1, 999)}號",
                RoomCount = Random.Shared.Next(1, 4),
                LivingRoomCount = 1,
                BathroomCount = 1,
                Area = 20 + Random.Shared.Next(0, 50), // 20-70坪
                CurrentFloor = Random.Shared.Next(1, 10),
                TotalFloors = Random.Shared.Next(5, 15),
                LandlordMemberId = landlordMemberId,
                ListingPlanId = testListingPlan.PlanId,
                DepositMonths = 2,
                MinimumRentalMonths = 12,
                StatusCode = "DRAFT", // 草稿狀態，便於測試
                CreatedAt = DateTime.UtcNow,
                Description = "這是一個用於測試檔案上傳功能的房源資料。"
            };

            context.Properties.Add(testProperty);
            await context.SaveChangesAsync();
            
            return testProperty;
        }

        /// <summary>
        /// 建立完整的測試資料組合 (會員 + 房源)
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="memberName">會員名稱</param>
        /// <param name="propertyTitle">房源標題</param>
        /// <returns>會員和房源的組合</returns>
        public static async Task<(Member member, Property property)> CreateTestMemberWithPropertyAsync(
            ZuHauseContext context,
            string memberName = "測試房東",
            string propertyTitle = "測試房源")
        {
            var member = await CreateTestMemberAsync(context, memberName);
            var property = await CreateTestPropertyAsync(context, member.MemberId, propertyTitle);
            
            return (member, property);
        }

        /// <summary>
        /// 清理測試資料 - 根據房源ID清理相關的所有資料
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="propertyId">房源ID</param>
        public static async Task CleanupTestPropertyDataAsync(ZuHauseContext context, int propertyId)
        {
            // 開始交易確保資料一致性
            using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // 1. 刪除房源相關的圖片紀錄
                var propertyImages = await context.Images
                    .Where(img => img.EntityType == Enums.EntityType.Property && img.EntityId == propertyId)
                    .ToListAsync();
                
                if (propertyImages.Any())
                {
                    context.Images.RemoveRange(propertyImages);
                }

                // 2. 刪除房源
                var property = await context.Properties.FindAsync(propertyId);
                if (property != null)
                {
                    // 取得房東會員ID，稍後清理
                    var landlordMemberId = property.LandlordMemberId;
                    
                    context.Properties.Remove(property);
                    
                    // 3. 檢查是否要清理會員資料 (如果是測試會員)
                    var member = await context.Members.FindAsync(landlordMemberId);
                    if (member != null && member.Email.Contains("@test.zuhaus.com"))
                    {
                        context.Members.Remove(member);
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 清理所有測試資料 - 清理所有測試用的會員、房源和圖片
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public static async Task CleanupAllTestDataAsync(ZuHauseContext context)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // 1. 清理所有測試圖片 (檔名以 TEST_ 開頭)
                var testImages = await context.Images
                    .Where(img => img.StoredFileName.StartsWith("TEST_"))
                    .ToListAsync();
                
                if (testImages.Any())
                {
                    context.Images.RemoveRange(testImages);
                }

                // 2. 清理測試會員的房源
                var testMembers = await context.Members
                    .Where(m => m.Email.Contains("@test.zuhaus.com"))
                    .ToListAsync();

                foreach (var member in testMembers)
                {
                    var memberProperties = await context.Properties
                        .Where(p => p.LandlordMemberId == member.MemberId)
                        .ToListAsync();
                    
                    if (memberProperties.Any())
                    {
                        context.Properties.RemoveRange(memberProperties);
                    }
                }

                // 3. 清理測試會員
                if (testMembers.Any())
                {
                    context.Members.RemoveRange(testMembers);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 取得或建立測試用城市
        /// </summary>
        private static async Task<City> GetOrCreateTestCityAsync(ZuHauseContext context)
        {
            var testCity = await context.Cities
                .FirstOrDefaultAsync(c => c.CityName == "測試市");

            if (testCity == null)
            {
                testCity = new City
                {
                    CityName = "測試市"
                };
                context.Cities.Add(testCity);
                await context.SaveChangesAsync();
            }

            return testCity;
        }

        /// <summary>
        /// 取得或建立測試用區域
        /// </summary>
        private static async Task<District> GetOrCreateTestDistrictAsync(ZuHauseContext context, int cityId)
        {
            var testDistrict = await context.Districts
                .FirstOrDefaultAsync(d => d.DistrictName == "測試區" && d.CityId == cityId);

            if (testDistrict == null)
            {
                testDistrict = new District
                {
                    CityId = cityId,
                    DistrictName = "測試區"
                };
                context.Districts.Add(testDistrict);
                await context.SaveChangesAsync();
            }

            return testDistrict;
        }

        /// <summary>
        /// 取得或建立測試用刊登方案
        /// </summary>
        private static async Task<ListingPlan> GetOrCreateTestListingPlanAsync(ZuHauseContext context)
        {
            var testPlan = await context.ListingPlans
                .FirstOrDefaultAsync(lp => lp.PlanName == "測試方案");

            if (testPlan == null)
            {
                testPlan = new ListingPlan
                {
                    PlanName = "測試方案",
                    PricePerDay = 0,
                    CurrencyCode = "NTD",
                    MinListingDays = 30,
                    StartAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.ListingPlans.Add(testPlan);
                await context.SaveChangesAsync();
            }

            return testPlan;
        }
    }
}