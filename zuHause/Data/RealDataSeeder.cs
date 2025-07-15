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
        /// 重置測試資料 - 清除所有測試資料並重新播種
        /// </summary>
        public async Task ResetTestDataAsync()
        {
            _logger.LogInformation("開始重置測試資料...");

            try
            {
                // 清除現有測試資料 (保持資料表結構)
                await ClearTestDataAsync();

                // 重新播種基礎資料
                await SeedBaseDataAsync();

                await _context.SaveChangesAsync();
                _logger.LogInformation("測試資料重置完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置測試資料時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 清除測試資料
        /// </summary>
        private async Task ClearTestDataAsync()
        {
            // 按照外鍵關係順序清除資料
            _context.RentalApplications.RemoveRange(_context.RentalApplications);
            _context.PropertyImages.RemoveRange(_context.PropertyImages);
            _context.Favorites.RemoveRange(_context.Favorites);
            _context.Properties.RemoveRange(_context.Properties);
            _context.Members.RemoveRange(_context.Members);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 播種基礎資料
        /// </summary>
        private async Task SeedBaseDataAsync()
        {
            // 確保基礎參考資料存在
            await EnsureReferenceDataAsync();
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
        }

        /// <summary>
        /// 建立測試房東與其房源 - 供其他測試使用
        /// </summary>
        /// <param name="isDraft">是否建立為草稿狀態</param>
        /// <returns>建立的房東會員</returns>
        public async Task<Member> CreateTestLandlordWithPropertyAsync(bool isDraft = false)
        {
            // 此方法將在階段 A2 和 A3 中實作
            throw new NotImplementedException("將在後續階段實作");
        }

        /// <summary>
        /// 建立測試租客 - 供其他測試使用
        /// </summary>
        /// <returns>建立的租客會員</returns>
        public async Task<Member> CreateTestTenantAsync()
        {
            // 此方法將在階段 A4 中實作
            throw new NotImplementedException("將在後續階段實作");
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