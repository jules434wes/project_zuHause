using Microsoft.AspNetCore.Http;
using zuHause.DTOs;

namespace zuHause.Tests.TestHelpers
{
    /// <summary>
    /// 擴充版 PropertyCreateDto - 支援檔案上傳欄位
    /// 用於測試房源圖片和 PDF 分離上傳功能
    /// </summary>
    public class PropertyCreateDtoExtended : PropertyCreateDto
    {
        /// <summary>
        /// 房源圖片集合 - 支援多檔案上傳（覆蓋基類定義）
        /// </summary>
        public new List<IFormFile> PropertyImages { get; set; } = new List<IFormFile>();

        /// <summary>
        /// 房產所有權狀PDF文件 - 單檔上傳（覆蓋基類定義）
        /// </summary>
        public new IFormFile? PropertyProofDocument { get; set; }

        /// <summary>
        /// 建立標準的 PropertyCreateDto (不含檔案)
        /// </summary>
        /// <returns>不含檔案欄位的標準 DTO</returns>
        public PropertyCreateDto ToStandardDto()
        {
            return new PropertyCreateDto
            {
                // 基本房源資訊
                Title = this.Title,
                MonthlyRent = this.MonthlyRent,
                CityId = this.CityId,
                DistrictId = this.DistrictId,
                AddressLine = this.AddressLine,
                RoomCount = this.RoomCount,
                LivingRoomCount = this.LivingRoomCount,
                BathroomCount = this.BathroomCount,
                Area = this.Area,
                CurrentFloor = this.CurrentFloor,
                TotalFloors = this.TotalFloors,
                
                // 租賃條件
                DepositMonths = this.DepositMonths,
                MinimumRentalMonths = this.MinimumRentalMonths,
                
                // 費用設定
                ManagementFeeIncluded = this.ManagementFeeIncluded,
                ManagementFeeAmount = this.ManagementFeeAmount,
                ParkingAvailable = this.ParkingAvailable,
                ParkingFeeRequired = this.ParkingFeeRequired,
                ParkingFeeAmount = this.ParkingFeeAmount,
                
                // 其他欄位
                Description = this.Description,
                ListingPlanId = this.ListingPlanId,
                PropertyProofUrl = this.PropertyProofUrl,
                SelectedEquipmentIds = this.SelectedEquipmentIds,
                EquipmentQuantities = this.EquipmentQuantities,
                PropertyId = this.PropertyId
            };
        }

        /// <summary>
        /// 取得總檔案數量
        /// </summary>
        public int TotalFileCount => PropertyImages.Count + (PropertyProofDocument != null ? 1 : 0);

        /// <summary>
        /// 檢查是否有任何檔案
        /// </summary>
        public bool HasAnyFiles => PropertyImages.Any() || PropertyProofDocument != null;

        /// <summary>
        /// 檢查是否有圖片檔案
        /// </summary>
        public bool HasImages => PropertyImages.Any();

        /// <summary>
        /// 檢查是否有PDF文件
        /// </summary>
        public bool HasPdfDocument => PropertyProofDocument != null;
    }
}