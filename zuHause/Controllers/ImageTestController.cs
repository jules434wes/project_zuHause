using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Interfaces;
using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Controllers
{
    /// <summary>
    /// 圖片管理系統測試控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ImageTestController : ControllerBase
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly IImageQueryService _imageQueryService;
        private readonly IPropertyImageService _propertyImageService;
        private readonly ZuHauseContext _context;

        public ImageTestController(
            IImageUploadService imageUploadService,
            IImageQueryService imageQueryService,
            IPropertyImageService propertyImageService,
            ZuHauseContext context)
        {
            _imageUploadService = imageUploadService;
            _imageQueryService = imageQueryService;
            _propertyImageService = propertyImageService;
            _context = context;
        }

        /// <summary>
        /// 測試圖片管理系統是否正常運作
        /// </summary>
        [HttpPost("test-image-system")]
        public async Task<IActionResult> TestImageSystem()
        {
            try
            {
                // 1. 測試創建測試圖片記錄 (只設置必要欄位，讓資料庫自動產生其他值)
                var testImage = new Image
                {
                    EntityType = EntityType.Property,
                    EntityId = 1,
                    Category = ImageCategory.Living,
                    MimeType = "image/jpeg",
                    OriginalFileName = "test-image.jpg",
                    FileSizeBytes = 1024,
                    Width = 800,
                    Height = 600,
                    DisplayOrder = 1
                    // 注意：不設置 ImageGuid, UploadedAt, IsActive, RowVersion - 讓資料庫使用預設值
                };

                // 2. 插入到資料庫
                _context.Images.Add(testImage);
                await _context.SaveChangesAsync();

                // 3. 查詢剛插入的圖片
                var insertedImage = await _imageQueryService.GetImageByGuidAsync(testImage.ImageGuid);

                // 4. 測試房源圖片查詢
                var propertyImages = await _imageQueryService.GetImagesByEntityAsync(EntityType.Property, 1);

                var result = new
                {
                    Success = true,
                    Message = "統一圖片管理系統測試成功",
                    TestResults = new
                    {
                        ImageInserted = insertedImage != null,
                        ImageId = insertedImage?.ImageId,
                        ImageGuid = insertedImage?.ImageGuid,
                        PropertyImagesCount = propertyImages.Count,
                        ServicesRegistered = new
                        {
                            ImageUploadService = _imageUploadService != null,
                            ImageQueryService = _imageQueryService != null,
                            PropertyImageService = _propertyImageService != null
                        }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "統一圖片管理系統測試失敗",
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// 測試所有 DI 服務是否正確註冊
        /// </summary>
        [HttpGet("test-di-services")]
        public IActionResult TestDIServices()
        {
            try
            {
                var result = new
                {
                    Success = true,
                    Message = "DI 服務檢查完成",
                    Services = new
                    {
                        ImageUploadService = _imageUploadService?.GetType().Name ?? "NOT_REGISTERED",
                        ImageQueryService = _imageQueryService?.GetType().Name ?? "NOT_REGISTERED",
                        PropertyImageService = _propertyImageService?.GetType().Name ?? "NOT_REGISTERED",
                        DatabaseContext = _context?.GetType().Name ?? "NOT_REGISTERED"
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "DI 服務檢查失敗",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 測試資料庫連線和 Image 表格
        /// </summary>
        [HttpGet("test-database")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // 測試資料庫連線
                var canConnect = await _context.Database.CanConnectAsync();
                
                // 計算現有圖片數量
                var imageCount = _context.Images.Count();

                var result = new
                {
                    Success = true,
                    Message = "資料庫連線測試完成",
                    Database = new
                    {
                        CanConnect = canConnect,
                        ExistingImagesCount = imageCount,
                        ConnectionString = "已連接到資料庫"
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "資料庫連線測試失敗",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 檢查 images 表結構
        /// </summary>
        [HttpGet("check-table-structure")]
        public async Task<IActionResult> CheckTableStructure()
        {
            try
            {
                // 查詢 images 表的欄位結構
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        COLUMN_NAME,
                        DATA_TYPE,
                        IS_NULLABLE,
                        COLUMN_DEFAULT
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'images' 
                    ORDER BY ORDINAL_POSITION";

                var columns = new List<object>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new
                    {
                        ColumnName = reader["COLUMN_NAME"].ToString(),
                        DataType = reader["DATA_TYPE"].ToString(),
                        IsNullable = reader["IS_NULLABLE"].ToString(),
                        DefaultValue = reader["COLUMN_DEFAULT"]?.ToString()
                    });
                }

                var result = new
                {
                    Success = true,
                    Message = "表結構檢查完成",
                    TableStructure = columns
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "表結構檢查失敗",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 檢查時間設定問題
        /// </summary>
        [HttpGet("check-time-settings")]
        public async Task<IActionResult> CheckTimeSettings()
        {
            try
            {
                // 查詢最新插入的圖片時間
                var latestImage = await _context.Images
                    .OrderByDescending(i => i.ImageId)
                    .FirstOrDefaultAsync();

                // 獲取伺服器時間資訊
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        GETDATE() as LocalTime,
                        GETUTCDATE() as UTCTime,
                        SYSUTCDATETIME() as SysUTCTime,
                        SYSDATETIME() as SysLocalTime,
                        DATEDIFF(HOUR, GETUTCDATE(), GETDATE()) as UTCOffset
                ";

                var timeInfo = new Dictionary<string, object>();
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    timeInfo["LocalTime"] = reader["LocalTime"];
                    timeInfo["UTCTime"] = reader["UTCTime"];
                    timeInfo["SysUTCTime"] = reader["SysUTCTime"];
                    timeInfo["SysLocalTime"] = reader["SysLocalTime"];
                    timeInfo["UTCOffset"] = reader["UTCOffset"];
                }

                var result = new
                {
                    Success = true,
                    Message = "時間設定檢查完成",
                    DatabaseTimes = timeInfo,
                    ApplicationTimes = new
                    {
                        CurrentUTC = DateTime.UtcNow,
                        CurrentLocal = DateTime.Now,
                        TimeZone = TimeZoneInfo.Local.DisplayName,
                        UTCOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours
                    },
                    LatestImageInfo = latestImage != null ? new
                    {
                        ImageId = latestImage.ImageId,
                        UploadedAt = latestImage.UploadedAt,
                        UploadedAtTaipei = latestImage.UploadedAt.AddHours(8)
                    } : null,
                    DatabaseDefaultValue = "sysutcdatetime() - 這會插入 UTC 時間"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "時間設定檢查失敗",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 修正資料庫時間預設值為台北時間
        /// </summary>
        [HttpPost("fix-database-timezone")]
        public async Task<IActionResult> FixDatabaseTimezone()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    -- 更新 images 表的 uploadedAt 預設值為台北時間 (UTC+8)
                    ALTER TABLE images 
                    DROP CONSTRAINT IF EXISTS DF_images_uploadedAt;
                    
                    ALTER TABLE images 
                    ADD CONSTRAINT DF_images_uploadedAt 
                    DEFAULT DATEADD(HOUR, 8, sysutcdatetime()) FOR uploadedAt;
                ";

                await command.ExecuteNonQueryAsync();

                var result = new
                {
                    Success = true,
                    Message = "資料庫時間預設值已修正為台北時間 (UTC+8)",
                    NewDefaultValue = "DATEADD(HOUR, 8, sysutcdatetime())"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "修正資料庫時間預設值失敗",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 檢查 EF Core 模型配置
        /// </summary>
        [HttpGet("check-ef-model")]
        public IActionResult CheckEfModel()
        {
            try
            {
                var model = _context.Model;
                var imageEntity = model.FindEntityType(typeof(Image));
                
                if (imageEntity == null)
                {
                    return BadRequest("找不到 Image 實體類型");
                }

                var entityTypeProperty = imageEntity.FindProperty("EntityType");
                var categoryProperty = imageEntity.FindProperty("Category");

                var result = new
                {
                    Success = true,
                    Message = "EF Core 模型檢查完成",
                    EntityTypeProperty = new
                    {
                        Name = entityTypeProperty?.Name,
                        ClrType = entityTypeProperty?.ClrType?.Name,
                        HasConversion = entityTypeProperty?.GetTypeMapping()?.Converter != null,
                        ConverterType = entityTypeProperty?.GetTypeMapping()?.Converter?.GetType()?.Name,
                        ColumnType = entityTypeProperty?.GetColumnType()
                    },
                    CategoryProperty = new
                    {
                        Name = categoryProperty?.Name,
                        ClrType = categoryProperty?.ClrType?.Name,
                        HasConversion = categoryProperty?.GetTypeMapping()?.Converter != null,
                        ConverterType = categoryProperty?.GetTypeMapping()?.Converter?.GetType()?.Name,
                        ColumnType = categoryProperty?.GetColumnType()
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "EF Core 模型檢查失敗",
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}