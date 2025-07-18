using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels.MemberViewModel
{
    public class ForgotPasswordViewModel
    {


        [Required(ErrorMessage = "請輸入原密碼")]
        [Display(Name = "原始密碼")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "請輸入8至30位英數字密碼")]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{}|:;'"",.?/]{8,30}$", ErrorMessage = "密碼格式錯誤")]
        public string? OriginalPassword { get; set; }

        [Required(ErrorMessage = "請輸入新密碼")]
        [Display(Name = "新密碼")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "請輸入8至30位英數字密碼")]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{}|:;'"",.?/]{8,30}$", ErrorMessage = "密碼格式錯誤")]
        public string? UserPassword { get; set; }
        [Required(ErrorMessage = "請再次輸入新密碼")]
        [Display(Name = "確認新密碼")]
        [Compare("UserPassword", ErrorMessage = "兩次輸入的密碼不一致")]
        public string? ConfirmPassword { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
