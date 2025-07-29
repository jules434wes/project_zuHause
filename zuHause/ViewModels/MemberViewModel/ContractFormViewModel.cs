using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels.MemberViewModel
{
    public class ContractFormViewModel
    {
        // --- 合約基本欄位 (Contract) ---
        public int? RentalApplicationId { get; set; }
        public int? PropertyId { get; set; }
        public string Title { get; set; } = null!;
        public string? LandlordName { get; set; }
        public string? LandlordNationalIdNo { get; set; }
        public DateOnly LandlordBirthDate { get; set; }
        public int LandlordMemberId { get; set; }
        public decimal MonthlyRent { get; set; }
        public string? AddressLine { get; set; }

        public DateOnly? RentalStartDate { get; set; }
        public DateOnly? RentalEndDate { get; set; }

        [Required]
        public string? AddressDetail { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇縣市")]
        public int SelectedCityId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇區域")]
        public int SelectedDistrictId { get; set; }
        public List<SelectListItem> CityOptions { get; set; } = new();




        public string Status { get; set; } = "BESIGNED";

        [Display(Name = "若發生糾紛，以何法院為管轄法院：")]
        public string? CourtJurisdiction { get; set; }
        public bool IsSublettable { get; set; } // 是否可轉租

        [Display(Name = "使用目的：")]
        public string? UsagePurpose { get; set; } // 使用目的

        [Display(Name = "押金：")]
        public int? DepositAmount { get; set; } // 押金
        [Display(Name = "清潔費：")]
        public int? CleaningFee { get; set; } // 清潔費
        [Display(Name = "管理費：")]
        public int? ManagementFee { get; set; } // 管理費
        [Display(Name = "停車位費用：")]
        public int? ParkingFee { get; set; } // 停車費
        [Display(Name = "若提前解約，需另行給付甲方相當於：")]
        public int? PenaltyAmount { get; set; } // 違約金金額

        /// <summary>
        /// 圖片位址
        /// </summary>
        public string? imgPath {  get; set; }

        [Required(ErrorMessage = "請選擇合約範本")]
        [Display(Name = "合約範本")]
        public int SelectedTemplateId { get; set; }

        // --- 備註 (ContractComment) ---
        public List<CommentItem> Comments { get; set; } = new();

        // --- 家具清單 (ContractFurnitureItem) ---
        public List<FurnitureItem> FurnitureItems { get; set; } = new();

        // --- 上傳檔案（可多筆）---
        public List<IFormFile> UploadFiles { get; set; } = new();

        // --- 簽名檔案（單筆）---
        //public IFormFile? SignatureFile { get; set; }

        // --- 簽名方式（手寫/上傳）---
        public string? SignMethod { get; set; }

        // --- 簽署角色（房東 or 房客）---
        public string? SignerRole { get; set; }


        public string? SignatureDataUrl { get; set; }
    }
    public class FurnitureItem
    {
        public string FurnitureName { get; set; } = null!;
        public string? FurnitureCondition { get; set; }
        public int Quantity { get; set; }

        public string? RepairChargeOwner { get; set; }
        public string? RepairResponsibility { get; set; }
    }

    public class CommentItem
    {
        public string CommentType { get; set; } = "USER";
        public string CommentText { get; set; } = null!;
    }

}
