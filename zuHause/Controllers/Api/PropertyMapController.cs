using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.DTOs;
using zuHause.DTOs.GoogleMaps;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Controllers.Api
{
    /// <summary>
    /// 房源地圖 API 控制器
    /// </summary>
    [ApiController]
    [Route("api/property/{propertyId:int}/map")]
    public class PropertyMapController : ControllerBase
    {
        private readonly ZuHauseContext _context;
        private readonly IGoogleMapsService _googleMapsService;
        private readonly IApiUsageTracker _usageTracker;
        private readonly IPropertyMapCacheService _cacheService;
        private readonly ILogger<PropertyMapController> _logger;

        private static readonly Dictionary<string, int> PlaceTypePriority = new Dictionary<string, int>
        {
            // 交通設施 (最高優先級)
            { "subway_station", 1 },
            { "bus_station", 2 },
            { "train_station", 3 },
            
            // 生活機能
            { "supermarket", 10 },
            { "convenience_store", 11 },
            { "post_office", 12 },
            { "bank", 13 },
            { "pharmacy", 14 },
            
            // 公共服務
            { "police", 20 },
            { "fire_station", 21 },
            { "hospital", 22 },
            
            // 休閒設施
            { "park", 30 },
            { "gym", 31 },
            { "library", 32 },
            
            // 教育
            { "school", 40 },
            { "university", 41 }
        };

        public PropertyMapController(
            ZuHauseContext context,
            IGoogleMapsService googleMapsService,
            IApiUsageTracker usageTracker,
            IPropertyMapCacheService cacheService,
            ILogger<PropertyMapController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _googleMapsService = googleMapsService ?? throw new ArgumentNullException(nameof(googleMapsService));
            _usageTracker = usageTracker ?? throw new ArgumentNullException(nameof(usageTracker));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 取得房源地圖資料（整合雙層快取機制）
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>房源地圖資料</returns>
        [HttpGet]
        public async Task<ActionResult<PropertyMapDto>> GetMapData(int propertyId)
        {
            try
            {
                _logger.LogInformation("開始載入房源 {PropertyId} 的地圖資料", propertyId);

                // 1. 驗證房源存在及座標完整性
                var property = await _context.Properties
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    _logger.LogWarning("房源 {PropertyId} 不存在", propertyId);
                    return NotFound($"房源 {propertyId} 不存在");
                }

                if (property.Latitude == null || property.Longitude == null)
                {
                    _logger.LogWarning("房源 {PropertyId} 座標資料不完整", propertyId);
                    return BadRequest(PropertyMapDto.CreateErrorResponse(propertyId, "房源座標資料不完整"));
                }

                // 2. 檢查座標變化並清除過期快取
                await _cacheService.CheckAndClearIfLocationChangedAsync(
                    propertyId, property.Latitude.Value, property.Longitude.Value);

                // 3. 嘗試從快取載入
                var cachedData = await _cacheService.GetFromCacheAsync(propertyId);
                if (cachedData != null)
                {
                    _logger.LogInformation("房源 {PropertyId} 快取命中，返回快取資料", propertyId);
                    return Ok(cachedData);
                }

                // 4. 建立基本回應物件（快取未命中）
                var response = new PropertyMapDto
                {
                    PropertyId = propertyId,
                    Latitude = (double)property.Latitude,
                    Longitude = (double)property.Longitude
                };

                // 5. 檢查 Places API 使用量限制
                if (!await _usageTracker.CanUseApiAsync("Places"))
                {
                    _logger.LogWarning("Places API 已達每日使用量限制，房源 {PropertyId}", propertyId);
                    response.IsLimited = true;
                    response.LimitMessage = "地圖服務暫時繁忙，僅顯示房源位置";
                    return Ok(response);
                }

                // 6. 搜尋附近設施
                try
                {
                    response.NearbyPlaces = await SearchAllPlaceTypes(
                        property.Latitude.Value, 
                        property.Longitude.Value);
                    
                    _logger.LogInformation("房源 {PropertyId} 載入 {Count} 個附近設施", 
                        propertyId, response.NearbyPlaces.Count);

                    // 7. 將新資料儲存到快取
                    await _cacheService.SetCacheAsync(propertyId, response);
                    _logger.LogInformation("房源 {PropertyId} 地圖資料已快取", propertyId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "房源 {PropertyId} 附近設施搜尋發生異常", propertyId);
                    response.LimitMessage = "附近設施資訊載入中，請稍候重新整理";
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入房源 {PropertyId} 地圖資料時發生系統錯誤", propertyId);
                return StatusCode(500, PropertyMapDto.CreateErrorResponse(propertyId, "系統錯誤，請稍後再試"));
            }
        }

        /// <summary>
        /// 搜尋所有類型的附近設施
        /// </summary>
        private async Task<List<NearbyPlaceDto>> SearchAllPlaceTypes(decimal propertyLat, decimal propertyLng)
        {
            var allPlaces = new List<NearbyPlaceDto>();

            foreach (var placeType in PlaceTypePriority.Keys)
            {
                if (!await _usageTracker.CanUseApiAsync("Places"))
                {
                    _logger.LogWarning("Places API 使用量已達限制，停止搜尋設施類型: {PlaceType}", placeType);
                    break;
                }

                try
                {
                    var placeRequest = new PlaceSearchRequest
                    {
                        Latitude = propertyLat,
                        Longitude = propertyLng,
                        Radius = 1000,
                        Type = placeType
                    };

                    var placesResult = await _googleMapsService.SearchPlacesAsync(placeRequest);

                    if (placesResult.IsSuccess && placesResult.Places.Any())
                    {
                        var nearbyPlaces = ConvertToNearbyPlaces(placesResult.Places, propertyLat, propertyLng);
                        allPlaces.AddRange(nearbyPlaces);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "搜尋設施類型 {PlaceType} 時發生錯誤", placeType);
                }
            }

            return allPlaces
                .OrderBy(p => GetPlaceTypePriority(p.Type))
                .ThenBy(p => p.Distance)
                .Take(20)
                .ToList();
        }

        /// <summary>
        /// 轉換 Google Places 結果為 DTO 物件
        /// </summary>
        private List<NearbyPlaceDto> ConvertToNearbyPlaces(List<PlaceInfo> places, decimal propertyLat, decimal propertyLng)
        {
            return places
                .Where(p => !string.IsNullOrWhiteSpace(p.Name) && p.Latitude != 0 && p.Longitude != 0)
                .Select(p => {
                    var distance = CalculateDistance(propertyLat, propertyLng, p.Latitude, p.Longitude);
                    return new NearbyPlaceDto
                    {
                        Name = p.Name,
                        Type = p.Type,
                        TypeDisplayName = NearbyPlaceDto.GetTypeDisplayName(p.Type),
                        Distance = distance,
                        WalkingTimeMinutes = NearbyPlaceDto.CalculateWalkingTime(distance),
                        Latitude = (double)p.Latitude,
                        Longitude = (double)p.Longitude,
                        Rating = (double?)p.Rating,
                        Address = p.Address
                    };
                })
                .ToList();
        }

        /// <summary>
        /// 使用 Haversine 公式計算兩點間距離（公尺）
        /// </summary>
        private double CalculateDistance(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            const double R = 6371000; // 地球半徑（公尺）
            
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLng = ToRadians((double)(lng2 - lng1));
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
                    
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return R * c;
        }

        /// <summary>
        /// 度數轉弧度
        /// </summary>
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// 取得設施類型優先級
        /// </summary>
        private int GetPlaceTypePriority(string placeType)
        {
            return PlaceTypePriority.TryGetValue(placeType, out var priority) ? priority : 999;
        }
    }
}