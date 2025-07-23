using System.Text.Json;
using zuHause.DTOs.GoogleMaps;
using zuHause.Interfaces;

namespace zuHause.Services
{
    /// <summary>
    /// Google Maps 服務實作
    /// </summary>
    public class GoogleMapsService : IGoogleMapsService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiUsageTracker _usageTracker;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleMapsService> _logger;
        
        private readonly string _apiKey;
        private const string BASE_URL = "https://maps.googleapis.com/maps/api";

        public GoogleMapsService(
            HttpClient httpClient, 
            IApiUsageTracker usageTracker,
            IConfiguration configuration,
            ILogger<GoogleMapsService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _usageTracker = usageTracker ?? throw new ArgumentNullException(nameof(usageTracker));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _apiKey = _configuration["GoogleMaps:ApiKey"] ?? "AIzaSyCIMrRHlR6LL2jayjfau4w9UH5V61WT9zM";
        }

        /// <summary>
        /// 地理編碼 - 將地址轉換為座標
        /// </summary>
        public async Task<GeocodingResult> GeocodeAsync(GeocodingRequest request)
        {
            const string apiType = "Geocoding";
            
            try
            {
                // 檢查 API 使用量限制
                if (!await _usageTracker.CanUseApiAsync(apiType))
                {
                    _logger.LogWarning("Geocoding API 已達每日使用量限制");
                    return new GeocodingResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "API 使用量已達每日限制",
                        Status = "QUOTA_EXCEEDED"
                    };
                }

                if (string.IsNullOrWhiteSpace(request.Address))
                {
                    return new GeocodingResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "地址不可為空",
                        Status = "INVALID_REQUEST"
                    };
                }

                // 建立 API 請求 URL
                var url = $"{BASE_URL}/geocode/json?" +
                         $"address={Uri.EscapeDataString(request.Address)}&" +
                         $"language={request.Language}&" +
                         $"region={request.Region}&" +
                         $"key={_apiKey}";

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                _logger.LogDebug("發送 Geocoding 請求: {Address}", request.Address);
                
                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Geocoding API 請求失敗: {StatusCode}, {Content}", 
                        response.StatusCode, responseContent);
                    return new GeocodingResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API 請求失敗: {response.StatusCode}",
                        Status = "REQUEST_FAILED"
                    };
                }

                // 記錄 API 使用量
                await _usageTracker.RecordApiUsageAsync(apiType, 0.005m); // 約 $0.005 per request

                // 解析回應
                return ParseGeocodingResponse(responseContent);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Geocoding 請求超時: {Address}", request.Address);
                return new GeocodingResult
                {
                    IsSuccess = false,
                    ErrorMessage = "請求超時",
                    Status = "TIMEOUT"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geocoding 請求發生錯誤: {Address}", request.Address);
                return new GeocodingResult
                {
                    IsSuccess = false,
                    ErrorMessage = "系統錯誤",
                    Status = "SYSTEM_ERROR"
                };
            }
        }

        /// <summary>
        /// 地點搜尋 - 搜尋指定類型的附近地點
        /// </summary>
        public async Task<PlaceSearchResult> SearchPlacesAsync(PlaceSearchRequest request)
        {
            const string apiType = "Places";
            
            try
            {
                // 檢查 API 使用量限制
                if (!await _usageTracker.CanUseApiAsync(apiType))
                {
                    _logger.LogWarning("Places API 已達每日使用量限制");
                    return new PlaceSearchResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "API 使用量已達每日限制",
                        Status = "QUOTA_EXCEEDED"
                    };
                }

                // 驗證請求參數
                if (request.Latitude == 0 && request.Longitude == 0)
                {
                    return new PlaceSearchResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "座標不可為空",
                        Status = "INVALID_REQUEST"
                    };
                }

                // 建立 API 請求 URL
                var url = $"{BASE_URL}/place/nearbysearch/json?" +
                         $"location={request.Latitude},{request.Longitude}&" +
                         $"radius={request.Radius}&" +
                         $"language={request.Language}&" +
                         $"key={_apiKey}";

                if (!string.IsNullOrEmpty(request.Type))
                    url += $"&type={Uri.EscapeDataString(request.Type)}";
                
                if (!string.IsNullOrEmpty(request.Keyword))
                    url += $"&keyword={Uri.EscapeDataString(request.Keyword)}";

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                _logger.LogDebug("發送 Places 搜尋請求: {Type} near {Lat},{Lng}", 
                    request.Type, request.Latitude, request.Longitude);
                
                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Places API 請求失敗: {StatusCode}, {Content}", 
                        response.StatusCode, responseContent);
                    return new PlaceSearchResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API 請求失敗: {response.StatusCode}",
                        Status = "REQUEST_FAILED"
                    };
                }

                // 記錄 API 使用量
                await _usageTracker.RecordApiUsageAsync(apiType, 0.032m); // 約 $0.032 per request

                // 解析回應
                return ParsePlaceSearchResponse(responseContent, request.MinRating);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Places 搜尋請求超時: {Type}", request.Type);
                return new PlaceSearchResult
                {
                    IsSuccess = false,
                    ErrorMessage = "請求超時",
                    Status = "TIMEOUT"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Places 搜尋請求發生錯誤: {Type}", request.Type);
                return new PlaceSearchResult
                {
                    IsSuccess = false,
                    ErrorMessage = "系統錯誤",
                    Status = "SYSTEM_ERROR"
                };
            }
        }

        /// <summary>
        /// 距離計算 - 計算起點到多個目的地的距離
        /// </summary>
        public async Task<DistanceMatrixResult> CalculateDistancesAsync(DistanceMatrixRequest request)
        {
            const string apiType = "DistanceMatrix";
            
            try
            {
                // 檢查 API 使用量限制
                if (!await _usageTracker.CanUseApiAsync(apiType))
                {
                    _logger.LogWarning("Distance Matrix API 已達每日使用量限制");
                    return new DistanceMatrixResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "API 使用量已達每日限制",
                        Status = "QUOTA_EXCEEDED"
                    };
                }

                // 驗證請求參數
                if (request.Origin.Latitude == 0 && request.Origin.Longitude == 0)
                {
                    return new DistanceMatrixResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "起點座標不可為空",
                        Status = "INVALID_REQUEST"
                    };
                }

                if (!request.Destinations.Any())
                {
                    return new DistanceMatrixResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "目的地清單不可為空",
                        Status = "INVALID_REQUEST"
                    };
                }

                // 建立 API 請求 URL
                var origins = request.Origin.ToString();
                var destinations = string.Join("|", request.Destinations.Select(d => d.ToString()));

                var url = $"{BASE_URL}/distancematrix/json?" +
                         $"origins={Uri.EscapeDataString(origins)}&" +
                         $"destinations={Uri.EscapeDataString(destinations)}&" +
                         $"mode={request.TravelMode}&" +
                         $"units={request.Units}&" +
                         $"language={request.Language}&" +
                         $"key={_apiKey}";

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                _logger.LogDebug("發送 Distance Matrix 請求: from {Origin} to {Count} destinations", 
                    origins, request.Destinations.Count);
                
                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Distance Matrix API 請求失敗: {StatusCode}, {Content}", 
                        response.StatusCode, responseContent);
                    return new DistanceMatrixResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API 請求失敗: {response.StatusCode}",
                        Status = "REQUEST_FAILED"
                    };
                }

                // 記錄 API 使用量（根據目的地數量計算）
                var elements = request.Destinations.Count;
                var cost = elements * 0.01m; // 約 $0.01 per element
                await _usageTracker.RecordApiUsageAsync(apiType, cost);

                // 解析回應
                return ParseDistanceMatrixResponse(responseContent);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Distance Matrix 請求超時");
                return new DistanceMatrixResult
                {
                    IsSuccess = false,
                    ErrorMessage = "請求超時",
                    Status = "TIMEOUT"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Distance Matrix 請求發生錯誤");
                return new DistanceMatrixResult
                {
                    IsSuccess = false,
                    ErrorMessage = "系統錯誤",
                    Status = "SYSTEM_ERROR"
                };
            }
        }

        /// <summary>
        /// 批次地理編碼 - 處理多個地址
        /// </summary>
        public async Task<List<GeocodingResult>> BatchGeocodeAsync(List<string> addresses)
        {
            var results = new List<GeocodingResult>();
            
            foreach (var address in addresses)
            {
                var request = new GeocodingRequest { Address = address };
                var result = await GeocodeAsync(request);
                results.Add(result);
                
                // 避免過度頻繁請求，加入小延遲
                await Task.Delay(100);
            }
            
            return results;
        }

        #region Private Helper Methods

        private GeocodingResult ParseGeocodingResponse(string responseContent)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                
                var status = root.GetProperty("status").GetString() ?? "";
                
                if (status != "OK")
                {
                    return new GeocodingResult
                    {
                        IsSuccess = false,
                        ErrorMessage = GetErrorMessage(status),
                        Status = status
                    };
                }

                var results = root.GetProperty("results");
                if (results.GetArrayLength() == 0)
                {
                    return new GeocodingResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "找不到對應的地理位置",
                        Status = "ZERO_RESULTS"
                    };
                }

                var firstResult = results[0];
                var location = firstResult.GetProperty("geometry").GetProperty("location");
                var formattedAddress = firstResult.GetProperty("formatted_address").GetString() ?? "";

                return new GeocodingResult
                {
                    IsSuccess = true,
                    Latitude = (decimal)location.GetProperty("lat").GetDouble(),
                    Longitude = (decimal)location.GetProperty("lng").GetDouble(),
                    FormattedAddress = formattedAddress,
                    Status = status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析 Geocoding 回應失敗: {ResponseContent}", responseContent);
                return new GeocodingResult
                {
                    IsSuccess = false,
                    ErrorMessage = "回應解析錯誤",
                    Status = "PARSE_ERROR"
                };
            }
        }

        private PlaceSearchResult ParsePlaceSearchResponse(string responseContent, decimal? minRating)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                
                var status = root.GetProperty("status").GetString() ?? "";
                
                if (status != "OK")
                {
                    return new PlaceSearchResult
                    {
                        IsSuccess = false,
                        ErrorMessage = GetErrorMessage(status),
                        Status = status
                    };
                }

                var results = root.GetProperty("results");
                var places = new List<PlaceInfo>();

                foreach (var result in results.EnumerateArray())
                {
                    var place = ParsePlaceInfo(result);
                    
                    // 套用評分篩選
                    if (minRating.HasValue && (!place.Rating.HasValue || place.Rating < minRating))
                        continue;
                        
                    places.Add(place);
                }

                return new PlaceSearchResult
                {
                    IsSuccess = true,
                    Places = places,
                    Status = status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析 Places 回應失敗: {ResponseContent}", responseContent);
                return new PlaceSearchResult
                {
                    IsSuccess = false,
                    ErrorMessage = "回應解析錯誤",
                    Status = "PARSE_ERROR"
                };
            }
        }

        private PlaceInfo ParsePlaceInfo(JsonElement result)
        {
            var location = result.GetProperty("geometry").GetProperty("location");
            var placeId = result.GetProperty("place_id").GetString() ?? "";
            var name = result.GetProperty("name").GetString() ?? "";
            var types = result.GetProperty("types");
            var type = types.GetArrayLength() > 0 ? types[0].GetString() ?? "" : "";
            var address = result.TryGetProperty("vicinity", out var vicinity) ? 
                vicinity.GetString() ?? "" : "";

            decimal? rating = null;
            if (result.TryGetProperty("rating", out var ratingElement))
            {
                rating = (decimal)ratingElement.GetDouble();
            }

            bool? isOpen = null;
            if (result.TryGetProperty("opening_hours", out var openingHours) &&
                openingHours.TryGetProperty("open_now", out var openNow))
            {
                isOpen = openNow.GetBoolean();
            }

            return new PlaceInfo
            {
                PlaceId = placeId,
                Name = name,
                Type = type,
                Latitude = (decimal)location.GetProperty("lat").GetDouble(),
                Longitude = (decimal)location.GetProperty("lng").GetDouble(),
                Address = address,
                Rating = rating,
                IsOpen = isOpen
            };
        }

        private DistanceMatrixResult ParseDistanceMatrixResponse(string responseContent)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                
                var status = root.GetProperty("status").GetString() ?? "";
                
                if (status != "OK")
                {
                    return new DistanceMatrixResult
                    {
                        IsSuccess = false,
                        ErrorMessage = GetErrorMessage(status),
                        Status = status
                    };
                }

                var rows = root.GetProperty("rows");
                var distances = new List<DistanceInfo>();

                if (rows.GetArrayLength() > 0)
                {
                    var elements = rows[0].GetProperty("elements");
                    var destinationAddresses = root.GetProperty("destination_addresses");

                    for (int i = 0; i < elements.GetArrayLength(); i++)
                    {
                        var element = elements[i];
                        var destinationAddress = i < destinationAddresses.GetArrayLength() ? 
                            destinationAddresses[i].GetString() ?? "" : "";
                        
                        var distanceInfo = ParseDistanceInfo(element, destinationAddress);
                        distances.Add(distanceInfo);
                    }
                }

                return new DistanceMatrixResult
                {
                    IsSuccess = true,
                    Distances = distances,
                    Status = status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析 Distance Matrix 回應失敗: {ResponseContent}", responseContent);
                return new DistanceMatrixResult
                {
                    IsSuccess = false,
                    ErrorMessage = "回應解析錯誤",
                    Status = "PARSE_ERROR"
                };
            }
        }

        private DistanceInfo ParseDistanceInfo(JsonElement element, string destinationAddress)
        {
            var elementStatus = element.GetProperty("status").GetString() ?? "";
            
            if (elementStatus != "OK")
            {
                return new DistanceInfo
                {
                    DestinationAddress = destinationAddress,
                    Status = elementStatus
                };
            }

            var distance = element.GetProperty("distance");
            var duration = element.GetProperty("duration");

            return new DistanceInfo
            {
                DestinationAddress = destinationAddress,
                DistanceInMeters = distance.GetProperty("value").GetInt32(),
                DistanceText = distance.GetProperty("text").GetString() ?? "",
                DurationInSeconds = duration.GetProperty("value").GetInt32(),
                DurationText = duration.GetProperty("text").GetString() ?? "",
                Status = elementStatus
            };
        }

        private string GetErrorMessage(string status)
        {
            return status switch
            {
                "ZERO_RESULTS" => "查無結果",
                "OVER_QUERY_LIMIT" => "API 查詢限制已達上限",
                "REQUEST_DENIED" => "API 請求被拒絕",
                "INVALID_REQUEST" => "無效的請求參數",
                "UNKNOWN_ERROR" => "未知錯誤",
                _ => $"API 錯誤: {status}"
            };
        }

        #endregion
    }
}