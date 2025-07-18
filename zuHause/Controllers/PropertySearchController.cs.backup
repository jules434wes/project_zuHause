using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using zuHause.Data;
using zuHause.DTOs;
using zuHause.Models;

namespace zuHause.Controllers
{
    /// <summary>
    /// 房源搜尋 API 控制器
    /// </summary>
    [ApiController]
    [Route("api/properties")]
    public class PropertySearchController : ControllerBase
    {
        private readonly ZuHauseContext _context;
        private readonly ILogger<PropertySearchController> _logger;

        public PropertySearchController(ZuHauseContext context, ILogger<PropertySearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 搜尋房源 - 基礎版本 (階段 B1-B2)
        /// </summary>
        /// <param name="request">搜尋條件</param>
        /// <returns>搜尋結果</returns>
        [HttpGet("search")]
        public async Task<ActionResult<PropertySearchResponseDto>> SearchProperties([FromQuery] PropertySearchRequestDto request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("開始房源搜尋，關鍵字: {Keyword}, 縣市: {CityId}, 區域: {DistrictId}", 
                    request.Keyword, request.CityId, request.DistrictId);

                // 基本查詢 - 只查詢已發布的房源
                var query = _context.Properties
                    .Include(p => p.LandlordMember)
                    .Where(p => p.StatusCode == "PUBLISHED" && p.DeletedAt == null)
                    .AsQueryable();

                // 階段 B3 將在此加入篩選功能
                // 階段 B4 將在此加入排序和分頁功能

                // 目前先使用簡單的分頁和預設排序
                var totalItems = await query.CountAsync();
                var properties = await query
                    .OrderByDescending(p => p.PublishedAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                stopwatch.Stop();

                var response = new PropertySearchResponseDto
                {
                    Properties = properties.Select(MapToSummaryDto).ToList(),
                    Pagination = CreatePaginationDto(request.Page, request.PageSize, totalItems),
                    Stats = new SearchStatsDto
                    {
                        TotalResults = totalItems,
                        SearchTimeMs = stopwatch.ElapsedMilliseconds,
                        AverageRent = properties.Any() ? properties.Average(p => p.MonthlyRent) : null,
                        MinRent = properties.Any() ? properties.Min(p => p.MonthlyRent) : null,
                        MaxRent = properties.Any() ? properties.Max(p => p.MonthlyRent) : null
                    }
                };

                _logger.LogInformation("房源搜尋完成，找到 {Count} 筆結果，耗時 {ElapsedMs}ms", 
                    totalItems, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "房源搜尋發生錯誤");
                return StatusCode(500, new { message = "搜尋時發生內部錯誤", error = ex.Message });
            }
        }

        /// <summary>
        /// 取得房源詳細資訊
        /// </summary>
        /// <param name="id">房源ID</param>
        /// <returns>房源詳細資訊</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Property>> GetPropertyDetail(int id)
        {
            try
            {
                var property = await _context.Properties
                    .Include(p => p.LandlordMember)
                    .Include(p => p.PropertyImages)
                    .FirstOrDefaultAsync(p => p.PropertyId == id && p.DeletedAt == null);

                if (property == null)
                {
                    return NotFound(new { message = "房源不存在或已刪除" });
                }

                return Ok(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得房源詳細資訊時發生錯誤，房源ID: {PropertyId}", id);
                return StatusCode(500, new { message = "取得房源詳細資訊時發生內部錯誤", error = ex.Message });
            }
        }

        /// <summary>
        /// 將房源實體轉換為摘要 DTO
        /// </summary>
        private static PropertySummaryDto MapToSummaryDto(Property property)
        {
            return new PropertySummaryDto
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                MonthlyRent = property.MonthlyRent,
                Area = property.Area,
                RoomLayout = $"{property.RoomCount}房{property.LivingRoomCount}廳{property.BathroomCount}衛",
                CityName = "待實作", // 階段 B3 將加入 City/District 查詢
                DistrictName = "待實作",
                PreviewImageUrl = property.PreviewImageUrl,
                LandlordName = property.LandlordMember?.MemberName ?? "未知房東",
                PublishedAt = property.PublishedAt
            };
        }

        /// <summary>
        /// 建立分頁資訊 DTO
        /// </summary>
        private static PaginationDto CreatePaginationDto(int currentPage, int pageSize, int totalItems)
        {
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            return new PaginationDto
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasPreviousPage = currentPage > 1,
                HasNextPage = currentPage < totalPages
            };
        }
    }
}