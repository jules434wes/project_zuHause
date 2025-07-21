using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels.MemberViewModel
{
    public class RentalApplicationViewModel
    {
        // 申請租賃


        public string? ApplicationType { get; set; }

        [Required]
        public int PropertyId { get; set; }

        public decimal MonthlyRent { get; set; }

        public string? AddressLine { get; set; }

        public int MemberId { get; set; }

        public string MemberName { get; set; } = null!;

        public DateOnly BirthDate { get; set; }
        public string? NationalIdNo { get; set; }

        public string? Message { get; set; }

        [Required]
        public DateOnly? RentalStartDate { get; set; }

        [Required]
        public DateOnly? RentalEndDate { get; set; }

        public string? ReturnUrl { get; set; }

        [Required]
        public string? AddressDetail { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇縣市")]
        public int SelectedCityId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇區域")]
        public int SelectedDistrictId { get; set; }
        public List<SelectListItem> CityOptions { get; set; } = new();



        public List<IFormFile> UploadFiles { get; set; } = new();



    }
}




