using zuHause.Models;
using Microsoft.EntityFrameworkCore;

namespace zuHause.Data
{
    public class RealDataSeeder
    {
        private readonly ZuHauseContext _context;

        public RealDataSeeder(ZuHauseContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Clear existing data
            await ResetTestDataAsync();

            // Create test data in logical order
            await CreateSystemData();
            await CreateTestMembersAsync();
            await CreateTestPropertiesAsync();
            await CreateTestApplicationsAsync();
        }

        public async Task ResetTestDataAsync()
        {
            // Delete in reverse dependency order
            _context.RentalApplications.RemoveRange(_context.RentalApplications);
            _context.PropertyImages.RemoveRange(_context.PropertyImages);
            _context.Properties.RemoveRange(_context.Properties);
            _context.Members.RemoveRange(_context.Members);
            _context.Districts.RemoveRange(_context.Districts);
            _context.Cities.RemoveRange(_context.Cities);
            _context.MemberTypes.RemoveRange(_context.MemberTypes);

            await _context.SaveChangesAsync();
        }

        private async Task CreateSystemData()
        {
            // Create Member Types
            var memberTypes = new[]
            {
                new MemberType { TypeID = 1, TypeName = "租客" },
                new MemberType { TypeID = 2, TypeName = "房東" },
                new MemberType { TypeID = 3, TypeName = "管理員" }
            };

            _context.MemberTypes.AddRange(memberTypes);

            // Create Cities
            var cities = new[]
            {
                new City { CityID = 1, CityName = "台北市" },
                new City { CityID = 2, CityName = "新北市" },
                new City { CityID = 3, CityName = "桃園市" }
            };

            _context.Cities.AddRange(cities);

            // Create Districts
            var districts = new[]
            {
                new District { DistrictID = 1, DistrictName = "中正區", CityID = 1 },
                new District { DistrictID = 2, DistrictName = "大安區", CityID = 1 },
                new District { DistrictID = 3, DistrictName = "信義區", CityID = 1 },
                new District { DistrictID = 4, DistrictName = "板橋區", CityID = 2 },
                new District { DistrictID = 5, DistrictName = "中壢區", CityID = 3 }
            };

            _context.Districts.AddRange(districts);
            await _context.SaveChangesAsync();
        }

        private async Task CreateTestMembersAsync()
        {
            var members = new[]
            {
                // 房東
                new Member
                {
                    MemberID = 1001,
                    MemberName = "王大明",
                    Phone = "0912345678",
                    Email = "landlord1@example.com",
                    Password = "password123",
                    TypeID = 2,
                    IsActive = true,
                    CreateTime = DateTime.Now.AddDays(-30),
                    LastLoginTime = DateTime.Now.AddDays(-1)
                },
                new Member
                {
                    MemberID = 1002,
                    MemberName = "李小華",
                    Phone = "0923456789",
                    Email = "landlord2@example.com",
                    Password = "password123",
                    TypeID = 2,
                    IsActive = true,
                    CreateTime = DateTime.Now.AddDays(-25),
                    LastLoginTime = DateTime.Now.AddDays(-2)
                },
                // 租客
                new Member
                {
                    MemberID = 2001,
                    MemberName = "張小美",
                    Phone = "0934567890",
                    Email = "tenant1@example.com",
                    Password = "password123",
                    TypeID = 1,
                    IsActive = true,
                    CreateTime = DateTime.Now.AddDays(-20),
                    LastLoginTime = DateTime.Now
                },
                new Member
                {
                    MemberID = 2002,
                    MemberName = "陳志強",
                    Phone = "0945678901",
                    Email = "tenant2@example.com",
                    Password = "password123",
                    TypeID = 1,
                    IsActive = true,
                    CreateTime = DateTime.Now.AddDays(-15),
                    LastLoginTime = DateTime.Now.AddHours(-2)
                }
            };

            _context.Members.AddRange(members);
            await _context.SaveChangesAsync();
        }

        private async Task CreateTestPropertiesAsync()
        {
            var properties = new[]
            {
                new Property
                {
                    PropertyID = 3001,
                    LandlordID = 1001,
                    PropertyType = "套房",
                    Title = "台北市中正區溫馨套房",
                    Description = "位於台北市中正區的溫馨套房，交通便利，近捷運站",
                    Address = "台北市中正區忠孝東路一段100號",
                    CityID = 1,
                    DistrictID = 1,
                    Rent = 15000,
                    Deposit = 30000,
                    ManagementFee = 1000,
                    ParkingFee = 2000,
                    Area = 12.5m,
                    Floor = 3,
                    TotalFloors = 10,
                    RoomCount = 1,
                    BathroomCount = 1,
                    BalconyCount = 1,
                    IsActive = true,
                    IsFeatured = true,
                    CreateTime = DateTime.Now.AddDays(-10),
                    UpdateTime = DateTime.Now.AddDays(-5)
                },
                new Property
                {
                    PropertyID = 3002,
                    LandlordID = 1001,
                    PropertyType = "雅房",
                    Title = "大安區優質雅房",
                    Description = "大安區精緻雅房，生活機能佳，適合學生或上班族",
                    Address = "台北市大安區敦化南路二段200號",
                    CityID = 1,
                    DistrictID = 2,
                    Rent = 12000,
                    Deposit = 24000,
                    ManagementFee = 800,
                    ParkingFee = null,
                    Area = 8.0m,
                    Floor = 2,
                    TotalFloors = 5,
                    RoomCount = 1,
                    BathroomCount = 1,
                    BalconyCount = 0,
                    IsActive = true,
                    IsFeatured = false,
                    CreateTime = DateTime.Now.AddDays(-8),
                    UpdateTime = DateTime.Now.AddDays(-3)
                },
                new Property
                {
                    PropertyID = 3003,
                    LandlordID = 1002,
                    PropertyType = "整層住家",
                    Title = "信義區豪華三房兩廳",
                    Description = "信義區高級住宅，三房兩廳兩衛，適合家庭居住",
                    Address = "台北市信義區信義路五段300號",
                    CityID = 1,
                    DistrictID = 3,
                    Rent = 45000,
                    Deposit = 90000,
                    ManagementFee = 3000,
                    ParkingFee = 3500,
                    Area = 35.0m,
                    Floor = 15,
                    TotalFloors = 20,
                    RoomCount = 3,
                    BathroomCount = 2,
                    BalconyCount = 2,
                    IsActive = true,
                    IsFeatured = true,
                    CreateTime = DateTime.Now.AddDays(-6),
                    UpdateTime = DateTime.Now.AddDays(-1)
                }
            };

            _context.Properties.AddRange(properties);
            await _context.SaveChangesAsync();

            // Create Property Images
            var propertyImages = new[]
            {
                new PropertyImage
                {
                    ImageID = 4001,
                    PropertyID = 3001,
                    ImageURL = "/images/property_3001_1.jpg",
                    IsMainImage = true,
                    UploadTime = DateTime.Now.AddDays(-10)
                },
                new PropertyImage
                {
                    ImageID = 4002,
                    PropertyID = 3001,
                    ImageURL = "/images/property_3001_2.jpg",
                    IsMainImage = false,
                    UploadTime = DateTime.Now.AddDays(-10)
                },
                new PropertyImage
                {
                    ImageID = 4003,
                    PropertyID = 3002,
                    ImageURL = "/images/property_3002_1.jpg",
                    IsMainImage = true,
                    UploadTime = DateTime.Now.AddDays(-8)
                },
                new PropertyImage
                {
                    ImageID = 4004,
                    PropertyID = 3003,
                    ImageURL = "/images/property_3003_1.jpg",
                    IsMainImage = true,
                    UploadTime = DateTime.Now.AddDays(-6)
                }
            };

            _context.PropertyImages.AddRange(propertyImages);
            await _context.SaveChangesAsync();
        }

        private async Task CreateTestApplicationsAsync()
        {
            var applications = new[]
            {
                new RentalApplication
                {
                    ApplicationID = 5001,
                    PropertyID = 3001,
                    ApplicantID = 2001,
                    ApplicationDate = DateTime.Now.AddDays(-5),
                    Status = "待審核",
                    Message = "我對這間套房很有興趣，希望能安排看房時間。",
                    CreateTime = DateTime.Now.AddDays(-5)
                },
                new RentalApplication
                {
                    ApplicationID = 5002,
                    PropertyID = 3002,
                    ApplicantID = 2002,
                    ApplicationDate = DateTime.Now.AddDays(-3),
                    Status = "已接受",
                    Message = "希望能盡快入住，可配合看房時間。",
                    CreateTime = DateTime.Now.AddDays(-3),
                    UpdateTime = DateTime.Now.AddDays(-1)
                }
            };

            _context.RentalApplications.AddRange(applications);
            await _context.SaveChangesAsync();
        }

        public async Task<Member> CreateTestLandlordWithPropertyAsync()
        {
            var landlord = new Member
            {
                MemberID = 9001,
                MemberName = "測試房東",
                Phone = "0987654321",
                Email = "testlandlord@example.com",
                Password = "password123",
                TypeID = 2,
                IsActive = true,
                CreateTime = DateTime.Now,
                LastLoginTime = DateTime.Now
            };

            _context.Members.Add(landlord);
            await _context.SaveChangesAsync();

            var property = new Property
            {
                PropertyID = 9001,
                LandlordID = landlord.MemberID,
                PropertyType = "套房",
                Title = "測試房源",
                Description = "這是一個測試用的房源",
                Address = "台北市測試區測試路123號",
                CityID = 1,
                DistrictID = 1,
                Rent = 20000,
                Deposit = 40000,
                ManagementFee = 1500,
                Area = 15.0m,
                Floor = 5,
                TotalFloors = 10,
                RoomCount = 1,
                BathroomCount = 1,
                BalconyCount = 1,
                IsActive = true,
                CreateTime = DateTime.Now
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            return landlord;
        }

        public async Task<Member> CreateTestTenantAsync()
        {
            var tenant = new Member
            {
                MemberID = 9002,
                MemberName = "測試租客",
                Phone = "0976543210",
                Email = "testtenant@example.com",
                Password = "password123",
                TypeID = 1,
                IsActive = true,
                CreateTime = DateTime.Now,
                LastLoginTime = DateTime.Now
            };

            _context.Members.Add(tenant);
            await _context.SaveChangesAsync();

            return tenant;
        }
    }
}