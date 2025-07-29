using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels.MemberViewModel
{
    public class TenantSignViewModel
    {
        public int? RentalApplicationId { get; set; }
        public int ContractId { get; set; }

        ////上傳檔案
        //[Required]
        //public IFormFile? SignatureFile { get; set; }

        public string? SignatureDataUrl { get; set; } // 新增欄位
        public string? SignMethod { get; set; } = "UPLOAD";

        // --- 簽署角色（房東 or 房客）---
        public string? SignerRole { get; set; } = "TENANT";

    }
}
