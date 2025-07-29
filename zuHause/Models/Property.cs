using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 房源資料表
/// </summary>
public partial class Property
{
    /// <summary>
    /// 房源ID
    /// </summary>
    public int PropertyId { get; set; }

    /// <summary>
    /// 房東會員ID
    /// </summary>
    public int LandlordMemberId { get; set; }

    /// <summary>
    /// 房源標題
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 詳細描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 縣市ID
    /// </summary>
    public int CityId { get; set; }

    /// <summary>
    /// 區域ID
    /// </summary>
    public int DistrictId { get; set; }

    /// <summary>
    /// 詳細地址
    /// </summary>
    public string? AddressLine { get; set; }

    /// <summary>
    /// 月租金
    /// </summary>
    public decimal MonthlyRent { get; set; }

    /// <summary>
    /// 押金金額
    /// </summary>
    public decimal DepositAmount { get; set; }

    /// <summary>
    /// 押金月數
    /// </summary>
    public int DepositMonths { get; set; }

    /// <summary>
    /// 房數
    /// </summary>
    public int RoomCount { get; set; }

    /// <summary>
    /// 廳數
    /// </summary>
    public int LivingRoomCount { get; set; }

    /// <summary>
    /// 衛數
    /// </summary>
    public int BathroomCount { get; set; }

    /// <summary>
    /// 所在樓層
    /// </summary>
    public int CurrentFloor { get; set; }

    /// <summary>
    /// 總樓層
    /// </summary>
    public int TotalFloors { get; set; }

    /// <summary>
    /// 坪數
    /// </summary>
    public decimal Area { get; set; }

    /// <summary>
    /// 最短租期(月)
    /// </summary>
    public int MinimumRentalMonths { get; set; }

    /// <summary>
    /// 特殊守則
    /// </summary>
    public string? SpecialRules { get; set; }

    /// <summary>
    /// 水費計算方式
    /// </summary>
    public string WaterFeeType { get; set; } = null!;

    /// <summary>
    /// 自訂水費
    /// </summary>
    public decimal? CustomWaterFee { get; set; }

    /// <summary>
    /// 電費計算方式
    /// </summary>
    public string ElectricityFeeType { get; set; } = null!;

    /// <summary>
    /// 自訂電費
    /// </summary>
    public decimal? CustomElectricityFee { get; set; }

    /// <summary>
    /// 管理費含租金
    /// </summary>
    public bool ManagementFeeIncluded { get; set; }

    /// <summary>
    /// 管理費金額
    /// </summary>
    public decimal? ManagementFeeAmount { get; set; }

    /// <summary>
    /// 有停車位
    /// </summary>
    public bool ParkingAvailable { get; set; }

    /// <summary>
    /// 停車費需額外收費
    /// </summary>
    public bool ParkingFeeRequired { get; set; }

    /// <summary>
    /// 停車位費用
    /// </summary>
    public decimal? ParkingFeeAmount { get; set; }

    /// <summary>
    /// 清潔費需額外收費
    /// </summary>
    public bool CleaningFeeRequired { get; set; }

    /// <summary>
    /// 清潔費金額
    /// </summary>
    public decimal? CleaningFeeAmount { get; set; }

    /// <summary>
    /// 刊登天數
    /// </summary>
    public int? ListingDays { get; set; }

    /// <summary>
    /// 刊登費用
    /// </summary>
    public decimal? ListingFeeAmount { get; set; }

    /// <summary>
    /// 刊登費方案ID
    /// </summary>
    public int? ListingPlanId { get; set; }

    /// <summary>
    /// 付款狀態
    /// </summary>
    public bool IsPaid { get; set; }

    /// <summary>
    /// 完成付款時間
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// 上架到期時間
    /// </summary>
    public DateTime? ExpireAt { get; set; }

    /// <summary>
    /// 房產證明文件URL
    /// </summary>
    public string? PropertyProofUrl { get; set; }

    /// <summary>
    /// 預覽圖連結
    /// </summary>
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// 房源狀態代碼
    /// </summary>
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 上架日期
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// 最後修改日期
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 緯度
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// 經度
    /// </summary>
    public decimal? Longitude { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual District District { get; set; } = null!;

    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    public virtual ICollection<Chatroom> Chatrooms { get; set; } = new List<Chatroom>();

    public virtual ICollection<CustomerServiceTicket> CustomerServiceTickets { get; set; } = new List<CustomerServiceTicket>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<FurnitureCart> FurnitureCarts { get; set; } = new List<FurnitureCart>();

    public virtual ICollection<FurnitureOrder> FurnitureOrders { get; set; } = new List<FurnitureOrder>();

    public virtual Member LandlordMember { get; set; } = null!;

    public virtual ListingPlan? ListingPlan { get; set; }

    public virtual ICollection<PropertyComplaint> PropertyComplaints { get; set; } = new List<PropertyComplaint>();

    public virtual ICollection<PropertyEquipmentRelation> PropertyEquipmentRelations { get; set; } = new List<PropertyEquipmentRelation>();

    public virtual ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();

    public virtual ICollection<RentalApplication> RentalApplications { get; set; } = new List<RentalApplication>();
}
