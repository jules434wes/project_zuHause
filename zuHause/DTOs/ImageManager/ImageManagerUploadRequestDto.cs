using System.ComponentModel.DataAnnotations;
using zuHause.Enums;

namespace zuHause.DTOs.ImageManager
{
    public class ImageManagerUploadRequestDto
    {
        [Required(ErrorMessage = "實體類型為必填")]
        public EntityType EntityType { get; set; }

        [Required(ErrorMessage = "實體 ID 為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "實體 ID 必須大於 0")]
        public int EntityId { get; set; }

        [Required(ErrorMessage = "圖片分類為必填")]
        public ImageCategory Category { get; set; }

        [Required(ErrorMessage = "圖片檔案為必填")]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();

        [Range(1, 50, ErrorMessage = "顯示順序必須介於 1 到 50 之間")]
        public int? StartDisplayOrder { get; set; }

        public bool SetAsMainImage { get; set; } = false;

        public bool SkipEntityValidation { get; set; } = false;

        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    public class ImageManagerBatchUploadRequestDto
    {
        [Required(ErrorMessage = "實體類型為必填")]
        public EntityType EntityType { get; set; }

        [Required(ErrorMessage = "實體 ID 為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "實體 ID 必須大於 0")]
        public int EntityId { get; set; }

        [Required(ErrorMessage = "圖片分類為必填")]
        public ImageCategory Category { get; set; }

        [Required(ErrorMessage = "圖片檔案為必填")]
        [MinLength(1, ErrorMessage = "至少需要一個圖片檔案")]
        [MaxLength(20, ErrorMessage = "最多只能上傳 20 個圖片檔案")]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();

        public List<int>? DisplayOrders { get; set; }

        public bool PreserveExistingOrder { get; set; } = true;
    }
}