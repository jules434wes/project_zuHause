using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.DTOs;

namespace zuHause.Controllers
{
    [ApiController]
    [Route("api/properties")]
    public class PropertyApiController : ControllerBase
    {
        private readonly ZuHauseContext _context;

        public PropertyApiController(ZuHauseContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<ActionResult<PropertySearchResultDto>> SearchProperties(
            [FromQuery] string? location = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? propertyType = null,
            [FromQuery] string? keywords = null,
            [FromQuery] string sortBy = "newest",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Properties
                    .Include(p => p.PropertyImages)
                    .Where(p => p.StatusCode == "上架"); // 使用 StatusCode 而非 IsActive

                // Location filter - 需要包含 City 和 District 的 Include
                if (!string.IsNullOrEmpty(location))
                {
                    query = query.Where(p => 
                        p.AddressLine != null && p.AddressLine.Contains(location));
                }

                // Price range filter
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.MonthlyRent >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.MonthlyRent <= maxPrice.Value);
                }

                // Property type filter - 需要檢查 Property model 是否有 PropertyType 欄位
                if (!string.IsNullOrEmpty(propertyType))
                {
                    // 暫時註解，因為 Property model 中沒有看到 PropertyType 欄位
                    // query = query.Where(p => p.PropertyType == propertyType);
                }

                // Keywords search
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(p => 
                        p.Title.Contains(keywords) ||
                        (p.Description != null && p.Description.Contains(keywords)) ||
                        (p.AddressLine != null && p.AddressLine.Contains(keywords)));
                }

                // Sorting
                query = sortBy.ToLower() switch
                {
                    "priceasc" => query.OrderBy(p => p.MonthlyRent),
                    "pricedesc" => query.OrderByDescending(p => p.MonthlyRent),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    "oldest" => query.OrderBy(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var properties = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PropertySearchResultDto
                {
                    Properties = properties.Select(p => new PropertyDto
                    {
                        Id = p.PropertyId,
                        Title = p.Title,
                        Location = "台北市", // 暫時固定值，需要查詢 City 和 District 表
                        Address = p.AddressLine ?? "",
                        Price = p.MonthlyRent,
                        PropertyType = "一般", // 暫時固定值，需要確認 Property model 中的房型欄位
                        MainImageUrl = p.PropertyImages?.FirstOrDefault()?.ImagePath ?? "/images/placeholder.jpg",
                        Bedrooms = p.RoomCount,
                        Bathrooms = p.BathroomCount,
                        Area = p.Area,
                        IsFeatured = false, // 暫時固定值，需要確認 Property model 中的精選欄位
                        CreateTime = p.CreatedAt
                    }).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "搜尋房源時發生錯誤", error = ex.Message });
            }
        }

        [HttpPost("{propertyId}/applications")]
        public async Task<ActionResult<ApplicationDto>> CreateApplication(
            int propertyId,
            [FromBody] CreateApplicationDto createDto)
        {
            try
            {
                // Validate property exists and is active
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId && p.StatusCode == "上架");

                if (property == null)
                {
                    return NotFound(new { message = "房源不存在或已下架" });
                }

                // TODO: Add authentication - get actual user ID from JWT token
                // For now, using test tenant ID
                var applicantId = 1; // This should come from authenticated user

                // Check for duplicate application
                var existingApplication = await _context.RentalApplications
                    .FirstOrDefaultAsync(ra => ra.PropertyId == propertyId && ra.MemberId == applicantId);

                if (existingApplication != null)
                {
                    return BadRequest(new { message = "您已經申請過這個房源" });
                }

                // Create new application
                var application = new RentalApplication
                {
                    PropertyId = propertyId,
                    MemberId = applicantId,
                    ApplicationType = "租屋申請", // 根據 RentalApplication model
                    CurrentStatus = "待審核", // 使用 CurrentStatus 而非 Status
                    Message = createDto.Message,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.RentalApplications.Add(application);
                await _context.SaveChangesAsync();

                var resultDto = new ApplicationDto
                {
                    ApplicationId = application.ApplicationId,
                    PropertyId = application.PropertyId,
                    ApplicantId = application.MemberId,
                    ApplicationDate = application.CreatedAt, // 使用 CreatedAt
                    Status = application.CurrentStatus,
                    Message = application.Message
                };

                return CreatedAtAction(nameof(GetApplication), new { id = application.ApplicationId }, resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "申請房源時發生錯誤", error = ex.Message });
            }
        }

        [HttpGet("applications/{id}")]
        public async Task<ActionResult<ApplicationDto>> GetApplication(int id)
        {
            try
            {
                var application = await _context.RentalApplications
                    .FirstOrDefaultAsync(ra => ra.ApplicationId == id);

                if (application == null)
                {
                    return NotFound(new { message = "申請記錄不存在" });
                }

                var dto = new ApplicationDto
                {
                    ApplicationId = application.ApplicationId,
                    PropertyId = application.PropertyId,
                    ApplicantId = application.MemberId,
                    ApplicationDate = application.CreatedAt, // 使用 CreatedAt
                    Status = application.CurrentStatus,
                    Message = application.Message
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得申請記錄時發生錯誤", error = ex.Message });
            }
        }

        [HttpPost("{propertyId}/images")]
        public async Task<ActionResult<List<ImageDto>>> UploadImages(
            int propertyId,
            [FromForm] List<IFormFile> images)
        {
            try
            {
                // Validate property exists
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

                if (property == null)
                {
                    return NotFound(new { message = "房源不存在" });
                }

                // TODO: Add authentication - check if user owns this property
                // For now, skip ownership check

                // Validate file count
                if (images == null || images.Count == 0)
                {
                    return BadRequest(new { message = "請選擇要上傳的圖片" });
                }

                if (images.Count > 10)
                {
                    return BadRequest(new { message = "最多只能上傳10張圖片" });
                }

                var uploadedImages = new List<ImageDto>();
                var uploadsPath = Path.Combine("wwwroot", "uploads", "properties", propertyId.ToString());
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Get next display order
                var maxDisplayOrder = await _context.PropertyImages
                    .Where(pi => pi.PropertyId == propertyId)
                    .MaxAsync(pi => (int?)pi.DisplayOrder) ?? 0;

                foreach (var (image, index) in images.Select((img, idx) => (img, idx)))
                {
                    // Validate file
                    if (image.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        return BadRequest(new { message = $"圖片 {image.FileName} 超過5MB大小限制" });
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new { message = $"圖片 {image.FileName} 格式不支援，僅支援 JPG, JPEG, PNG" });
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // Create database record
                    var propertyImage = new PropertyImage
                    {
                        PropertyId = propertyId,
                        ImagePath = $"/uploads/properties/{propertyId}/{fileName}",
                        DisplayOrder = maxDisplayOrder + index + 1,
                        CreatedAt = DateTime.Now
                    };

                    _context.PropertyImages.Add(propertyImage);
                    await _context.SaveChangesAsync();

                    uploadedImages.Add(new ImageDto
                    {
                        ImageId = propertyImage.ImageId,
                        ImageUrl = propertyImage.ImagePath,
                        IsMainImage = maxDisplayOrder == 0 && index == 0, // First image is main
                        UploadTime = propertyImage.CreatedAt
                    });
                }

                return Ok(uploadedImages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "上傳圖片時發生錯誤", error = ex.Message });
            }
        }
    }
}