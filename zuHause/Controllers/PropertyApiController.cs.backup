using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Data;
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
                    .Include(p => p.City)
                    .Include(p => p.District)
                    .Include(p => p.PropertyImages)
                    .Where(p => p.IsActive == true);

                // Location filter
                if (!string.IsNullOrEmpty(location))
                {
                    query = query.Where(p => 
                        p.City.CityName.Contains(location) ||
                        p.District.DistrictName.Contains(location) ||
                        p.Address.Contains(location));
                }

                // Price range filter
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Rent >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Rent <= maxPrice.Value);
                }

                // Property type filter
                if (!string.IsNullOrEmpty(propertyType))
                {
                    query = query.Where(p => p.PropertyType == propertyType);
                }

                // Keywords search
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(p => 
                        p.Title.Contains(keywords) ||
                        p.Description.Contains(keywords) ||
                        p.Address.Contains(keywords));
                }

                // Sorting
                query = sortBy.ToLower() switch
                {
                    "priceasc" => query.OrderBy(p => p.Rent),
                    "pricedesc" => query.OrderByDescending(p => p.Rent),
                    "newest" => query.OrderByDescending(p => p.CreateTime),
                    "oldest" => query.OrderBy(p => p.CreateTime),
                    _ => query.OrderByDescending(p => p.CreateTime)
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
                        Id = p.PropertyID,
                        Title = p.Title,
                        Location = $"{p.City?.CityName}{p.District?.DistrictName}",
                        Address = p.Address,
                        Price = p.Rent,
                        PropertyType = p.PropertyType,
                        MainImageUrl = p.PropertyImages?.FirstOrDefault(img => img.IsMainImage)?.ImageURL ?? "/images/placeholder.jpg",
                        Bedrooms = p.RoomCount,
                        Bathrooms = p.BathroomCount,
                        Area = p.Area,
                        IsFeatured = p.IsFeatured,
                        CreateTime = p.CreateTime
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
                    .FirstOrDefaultAsync(p => p.PropertyID == propertyId && p.IsActive == true);

                if (property == null)
                {
                    return NotFound(new { message = "房源不存在或已下架" });
                }

                // TODO: Add authentication - get actual user ID from JWT token
                // For now, using test tenant ID
                var applicantId = 2001; // This should come from authenticated user

                // Check for duplicate application
                var existingApplication = await _context.RentalApplications
                    .FirstOrDefaultAsync(ra => ra.PropertyID == propertyId && ra.ApplicantID == applicantId);

                if (existingApplication != null)
                {
                    return BadRequest(new { message = "您已經申請過這個房源" });
                }

                // Create new application
                var application = new RentalApplication
                {
                    PropertyID = propertyId,
                    ApplicantID = applicantId,
                    ApplicationDate = DateTime.Now,
                    Status = "待審核",
                    Message = createDto.Message,
                    CreateTime = DateTime.Now
                };

                _context.RentalApplications.Add(application);
                await _context.SaveChangesAsync();

                var resultDto = new ApplicationDto
                {
                    ApplicationId = application.ApplicationID,
                    PropertyId = application.PropertyID,
                    ApplicantId = application.ApplicantID,
                    ApplicationDate = application.ApplicationDate,
                    Status = application.Status,
                    Message = application.Message
                };

                return CreatedAtAction(nameof(GetApplication), new { id = application.ApplicationID }, resultDto);
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
                    .FirstOrDefaultAsync(ra => ra.ApplicationID == id);

                if (application == null)
                {
                    return NotFound(new { message = "申請記錄不存在" });
                }

                var dto = new ApplicationDto
                {
                    ApplicationId = application.ApplicationID,
                    PropertyId = application.PropertyID,
                    ApplicantId = application.ApplicantID,
                    ApplicationDate = application.ApplicationDate,
                    Status = application.Status,
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
                    .FirstOrDefaultAsync(p => p.PropertyID == propertyId);

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

                // Check if this is the first image (will be main image)
                var existingImageCount = await _context.PropertyImages
                    .CountAsync(pi => pi.PropertyID == propertyId);

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
                        PropertyID = propertyId,
                        ImageURL = $"/uploads/properties/{propertyId}/{fileName}",
                        IsMainImage = existingImageCount == 0 && index == 0, // First image of property is main
                        UploadTime = DateTime.Now
                    };

                    _context.PropertyImages.Add(propertyImage);
                    await _context.SaveChangesAsync();

                    uploadedImages.Add(new ImageDto
                    {
                        ImageId = propertyImage.ImageID,
                        ImageUrl = propertyImage.ImageURL,
                        IsMainImage = propertyImage.IsMainImage,
                        UploadTime = propertyImage.UploadTime
                    });

                    existingImageCount++;
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