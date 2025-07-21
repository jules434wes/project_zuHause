using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels.MemberViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入手機號碼")]
        [Display(Name ="手機號碼")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "手機號碼必須是10碼")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式錯誤")]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "請輸入密碼")]
        [Display(Name = "密碼")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "請輸入8至30位英數字密碼")]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{}|:;'"",.?/]{8,30}$", ErrorMessage = "密碼格式錯誤")]
        public string? UserPassword { get; set; }
        [Required(ErrorMessage = "請再次輸入密碼")]
        [Display(Name = "確認密碼")]
        [Compare("UserPassword",ErrorMessage ="兩次輸入的密碼不一致")]
        public string? ConfirmPassword { get; set; }
        [Required(ErrorMessage = "請輸入姓名")]
        [Display(Name = "姓名")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "請輸入正確性名")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "請輸入電子信箱")]
        [Display(Name = "電子信箱")]
        [EmailAddress]
        public string? Email { get; set; }
        [Required(ErrorMessage = "請輸入性別")]
        [Display(Name = "性別")]
        public byte Gender { get; set; }
        [Required(ErrorMessage = "請輸入生日")]
        [Display(Name = "生日")]
        [DataType(DataType.Date, ErrorMessage = "請輸入正確日期")]
        public string? Birthday { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇居住縣市")]
        [Display(Name = "居住縣市")]
        public int? ResidenceCity { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇居住區域")]
        [Display(Name = "居住區域")]
        public int? ResidenceDistrictID { get; set; }

        [Required(ErrorMessage = "請輸入地址")]
        [Display(Name = "地址")]
        public string? AddressLine { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇欲承租縣市")]
        [Display(Name = "欲承租縣市")]
        public int? PrimaryRentalCityID { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇欲承租區域")]
        [Display(Name = "欲承租區域")]
        public int? PrimaryRentalDistrictID { get; set; }

        public int SelectedCityId { get; set; }
        public List<SelectListItem> CityOptions { get; set; } = new();
        // 等同 public List<SelectListItem> CityOptions { get; set; } = new List<SelectListItem>();

        public int SelectedDistrictId { get; set; }

        public int Verify { get; set; }


        public List<SelectListItem> ResidenceDistrictOptions { get; set; } = new();
        public List<SelectListItem> PrimaryRentalDistrictOptions { get; set; } = new();


    }

    public class VerifyPhoneViewModel
    {
        [Required(ErrorMessage = "請輸入手機號碼")]
        [Display(Name = "手機號碼")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "手機號碼必須是10碼")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式錯誤")]
        public string? PhoneNumber { get; set; }
    }
    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = "請輸入手機號碼")]
        [Display(Name = "手機號碼")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "手機號碼必須是10碼")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式錯誤")]
        public string? PhoneNumber { get; set; }
        public int Verify { get; set; }
    }
}
