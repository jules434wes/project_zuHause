namespace zuHause.DTOs
{
    public class PropertySearchResultDto
    {
        public List<PropertyDto> Properties { get; set; } = new List<PropertyDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PropertyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string MainImageUrl { get; set; } = string.Empty;
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public decimal? Area { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class CreateApplicationDto
    {
        public string? Message { get; set; }
    }

    public class ApplicationDto
    {
        public int ApplicationId { get; set; }
        public int PropertyId { get; set; }
        public int ApplicantId { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class ImageDto
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMainImage { get; set; }
        public DateTime UploadTime { get; set; }
    }
}