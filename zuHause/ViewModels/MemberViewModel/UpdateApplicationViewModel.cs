using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels.MemberViewModel
{
    public class UpdateApplicationViewModel
    {

        [Required]
        public int ApplicationId { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
