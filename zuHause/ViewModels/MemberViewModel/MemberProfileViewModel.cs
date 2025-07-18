using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels.MemberViewModel
{
    public class MemberProfileViewModel
    {
        //不可修改

        [Display(Name = "手機號碼")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "身分證號")]
        public string? NationalIdNo { get; set; }

        [Display(Name = "註冊日期")] // 後台修改
        public DateTime CreatedAt { get; set; }

        [Display(Name = "手機驗證通過時間")] // 目前未實作，先放不可改
        public DateTime? PhoneVerifiedAt { get; set; }

        [Display(Name = "Email驗證通過時間")] // 目前未實作，先放不可改
        public DateTime? EmailVerifiedAt { get; set; }

        [Display(Name = "身份驗證通過時間")] // 目前未實作，先放不可改
        public DateTime? IdentityVerifiedAt { get; set; }

        [Display(Name = "是否開通房東身分")]
        public bool IsLandlord { get; set; }

        [Display(Name = "現身分別")]
        public int? MemberTypeId { get; set; }

        // 身分驗證前可改，驗證後不可改

        [Required(ErrorMessage = "請輸入姓名")]
        [Display(Name = "姓名")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "請輸入正確性名")]
        public string? MemberName { get; set; }

        [Required(ErrorMessage = "請輸入性別")]
        [Display(Name = "性別")]
        public byte Gender { get; set; }

        [Required(ErrorMessage = "請輸入生日")]
        [Display(Name = "生日")]
        [DataType(DataType.Date,ErrorMessage ="請輸入正確日期")]
        public DateOnly BirthDate { get; set; }

        // 電子信箱認證後會改

        [Required(ErrorMessage = "請輸入電子信箱")]
        [Display(Name = "電子信箱")]
        [EmailAddress]
        public string? Email { get; set; }

        // 可修改
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Required]
        [Display(Name = "居住縣市")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇居住縣市")]
        public int? PrimaryRentalCityId { get; set; }
        [Required]
        [Display(Name = "居住區域")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇居住區域")]
        public int? PrimaryRentalDistrictId { get; set; }
        [Required(ErrorMessage = "請輸入地址")]
        [Display(Name = "地址")]
        public string? AddressLine { get; set; }
        [Required]
        [Display(Name = "欲承租縣市")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇欲承租縣市")]
        public int? ResidenceCityId { get; set; }
        [Required]
        [Display(Name = "欲承租區域")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇欲承租區域")]
        public int? ResidenceDistrictId { get; set; }

        public int SelectedCityId { get; set; }
        public List<SelectListItem> CityOptions { get; set; } = new();
        // 等同 public List<SelectListItem> CityOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ResidenceDistrictOptions { get; set; } = new();
        public List<SelectListItem> PrimaryRentalDistrictOptions { get; set; } = new();

        public int SelectedDistrictId { get; set; }


    }
}
