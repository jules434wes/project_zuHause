using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels
{
    /// <summary>
    /// 房源投訴 ViewModel
    /// </summary>
    public class PropertyComplaintViewModel
    {
        /// <summary>
        /// 房源ID
        /// </summary>
        public int PropertyId { get; set; }

        /// <summary>
        /// 投訴人ID
        /// </summary>
        public int ComplainantId { get; set; }

        /// <summary>
        /// 被投訴房東ID
        /// </summary>
        public int LandlordId { get; set; }

        /// <summary>
        /// 房源標題
        /// </summary>
        public string PropertyTitle { get; set; } = string.Empty;

        /// <summary>
        /// 房源地址
        /// </summary>
        public string PropertyAddress { get; set; } = string.Empty;

        /// <summary>
        /// 房東姓名
        /// </summary>
        public string LandlordName { get; set; } = string.Empty;

        /// <summary>
        /// 投訴內容
        /// </summary>
        [Required(ErrorMessage = "請填寫投訴內容")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "投訴內容長度需介於 10 到 1000 字之間")]
        public string ComplaintContent { get; set; } = string.Empty;

        /// <summary>
        /// 投訴類型代碼
        /// </summary>
        [Required(ErrorMessage = "請選擇投訴類型")]
        public string ComplaintTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// 投訴類型選項
        /// </summary>
        public List<SelectListItem> ComplaintTypes { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 投訴人姓名（顯示用）
        /// </summary>
        public string ComplainantName { get; set; } = string.Empty;

        /// <summary>
        /// 投訴人電話（顯示用）
        /// </summary>
        public string ComplainantPhone { get; set; } = string.Empty;

        /// <summary>
        /// 投訴人Email（顯示用）
        /// </summary>
        public string ComplainantEmail { get; set; } = string.Empty;
    }
}