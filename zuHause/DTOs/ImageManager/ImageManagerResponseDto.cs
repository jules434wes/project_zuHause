using zuHause.Enums;

namespace zuHause.DTOs.ImageManager
{
    public class ImageManagerResponseDto
    {
        public long ImageId { get; set; }
        public Guid ImageGuid { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public EntityType EntityType { get; set; }
        public int EntityId { get; set; }
        public ImageCategory Category { get; set; }
        public bool IsMainImage { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? OriginalUrl { get; set; }
        public string? LargeUrl { get; set; }
        public string? MediumUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public class ImageManagerListResponseDto
    {
        public List<ImageManagerResponseDto> Images { get; set; } = new List<ImageManagerResponseDto>();
        public int TotalCount { get; set; }
        public EntityType EntityType { get; set; }
        public int EntityId { get; set; }
        public ImageCategory? Category { get; set; }
        public ImageManagerResponseDto? MainImage { get; set; }
    }

    public class ImageManagerUploadResultDto
    {
        public List<ImageManagerResponseDto> SuccessfulUploads { get; set; } = new List<ImageManagerResponseDto>();
        public List<ImageUploadErrorDto> FailedUploads { get; set; } = new List<ImageUploadErrorDto>();
        public int SuccessCount => SuccessfulUploads.Count;
        public int FailureCount => FailedUploads.Count;
        public bool HasErrors => FailedUploads.Any();
    }

    public class ImageUploadErrorDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public Dictionary<string, string>? ValidationErrors { get; set; }
    }

    public class ImageManagerOperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<long>? AffectedImageIds { get; set; }
        public ImageManagerListResponseDto? UpdatedImageList { get; set; }
    }
}