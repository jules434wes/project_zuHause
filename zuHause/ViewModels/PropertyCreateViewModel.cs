using zuHause.DTOs;
using zuHause.Models;
using zuHause.Services.Interfaces;
using zuHause.Enums;
using zuHause.Constants;

namespace zuHause.ViewModels
{
    /// <summary>
    /// 房源創建頁面ViewModel
    /// </summary>
    public class PropertyCreateViewModel
    {
        /// <summary>
        /// 房源創建數據（用於表單綁定）
        /// </summary>
        public PropertyCreateDto PropertyData { get; set; } = new PropertyCreateDto();

        /// <summary>
        /// 城市區域選項
        /// </summary>
        public List<CityDistrictDto> Cities { get; set; } = new List<CityDistrictDto>();

        /// <summary>
        /// 刊登方案選項
        /// </summary>
        public List<ListingPlan> ListingPlans { get; set; } = new List<ListingPlan>();

        /// <summary>
        /// 設備分類選項
        /// </summary>
        public List<PropertyEquipmentSelectionDto> EquipmentCategories { get; set; } = new List<PropertyEquipmentSelectionDto>();

        /// <summary>
        /// 設備分類三層階層結構
        /// </summary>
        public List<PropertyEquipmentCategoryHierarchy> EquipmentCategoriesHierarchy { get; set; } = new List<PropertyEquipmentCategoryHierarchy>();

        /// <summary>
        /// 可用的中文圖片分類列表
        /// </summary>
        public List<string> AvailableChineseCategories { get; set; } = new List<string>();

        /// <summary>
        /// 表單提交後的錯誤訊息
        /// </summary>
        public Dictionary<string, List<string>> ValidationErrors { get; set; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// 成功或錯誤訊息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 是否為成功狀態
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 最短租期選項
        /// </summary>
        public List<KeyValuePair<int, string>> MinimumRentalOptions { get; set; } = new List<KeyValuePair<int, string>>
        {
            new KeyValuePair<int, string>(6, "6個月"),
            new KeyValuePair<int, string>(12, "1年"),
            new KeyValuePair<int, string>(24, "2年")
        };

        /// <summary>
        /// 水費計算方式選項
        /// </summary>
        public List<string> WaterFeeOptions { get; set; } = new List<string> { "台水", "自訂金額" };

        /// <summary>
        /// 電費計算方式選項
        /// </summary>
        public List<string> ElectricityFeeOptions { get; set; } = new List<string> { "台電", "自訂金額" };

        /// <summary>
        /// 表單模式
        /// </summary>
        public PropertyFormMode FormMode { get; set; } = PropertyFormMode.Create;

        /// <summary>
        /// 是否為編輯模式
        /// </summary>
        public bool IsEditMode { get; set; } = false;

        /// <summary>
        /// 房源ID (編輯模式使用)
        /// </summary>
        public int? PropertyId => IsEditMode ? PropertyData?.PropertyId : null;

        /// <summary>
        /// 判斷房產證明文件是否已鎖定（基於現有欄位）
        /// 條件：編輯模式 + 有證明文件 + 房源已上架或出租
        /// </summary>
        public bool IsProofDocumentLocked => 
            IsEditMode && 
            !string.IsNullOrEmpty(PropertyData?.PropertyProofUrl) && 
            (PropertyData?.StatusCode == PropertyStatusConstants.LISTED || PropertyData?.StatusCode == PropertyStatusConstants.ALREADY_RENTED);
    }
}