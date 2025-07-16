using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels.MemberViewModel
{
    public class LoginViewModel
    {
        [Display(Name = "手機號碼")]
        [Required(ErrorMessage = "請輸入手機號碼")]
        [StringLength(10, MinimumLength = 10, ErrorMessage ="手機號碼必須是10碼")]
        [RegularExpression(@"^09\d{8}$",ErrorMessage = "手機號碼格式錯誤")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "密碼")]
        [Required(ErrorMessage = "請輸入密碼")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "請輸入8至30位英數字密碼")]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{}|:;'"",.?/]{8,30}$", ErrorMessage = "密碼格式錯誤")]
        public string? UserPassword { get; set; }

        public string? LoginStatus { get; set; }

        public string? ReturnUrl { get; set; }

    }
}
