using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 房源資料表
/// </summary>
[Table("properties")]
[Index("CityId", "DistrictId", Name = "IX_properties_location")]
[Index("StatusCode", Name = "IX_properties_status")]
public partial class Property
{
    /// <summary>
    /// 房源ID
    /// </summary>
    [Key]
    [Column("propertyID")]
    public int PropertyId { get; set; }

    /// <summary>
    /// 房東會員ID
    /// </summary>
    [Column("landlordMemberID")]
    public int LandlordMemberId { get; set; }

    /// <summary>
    /// 房源標題
    /// </summary>
    [Column("title")]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 詳細描述
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 縣市ID
    /// </summary>
    [Column("cityID")]
    public int CityId { get; set; }

    /// <summary>
    /// 區域ID
    /// </summary>
    [Column("districtID")]
    public int DistrictId { get; set; }

    /// <summary>
    /// 詳細地址
    /// </summary>
    [Column("addressLine")]
    [StringLength(200)]
    public string? AddressLine { get; set; }

    /// <summary>
    /// 月租金
    /// </summary>
    [Column("monthlyRent", TypeName = "decimal(10, 2)")]
    public decimal MonthlyRent { get; set; }

    /// <summary>
    /// 押金金額
    /// </summary>
    [Column("depositAmount", TypeName = "decimal(10, 2)")]
    public decimal DepositAmount { get; set; }

    /// <summary>
    /// 押金月數
    /// </summary>
    [Column("depositMonths")]
    public int DepositMonths { get; set; }

    /// <summary>
    /// 房數
    /// </summary>
    [Column("roomCount")]
    public int RoomCount { get; set; }

    /// <summary>
    /// 廳數
    /// </summary>
    [Column("livingRoomCount")]
    public int LivingRoomCount { get; set; }

    /// <summary>
    /// 衛數
    /// </summary>
    [Column("bathroomCount")]
    public int BathroomCount { get; set; }

    /// <summary>
    /// 所在樓層
    /// </summary>
    [Column("currentFloor")]
    public int CurrentFloor { get; set; }

    /// <summary>
    /// 總樓層
    /// </summary>
    [Column("totalFloors")]
    public int TotalFloors { get; set; }

    /// <summary>
    /// 坪數
    /// </summary>
    [Column("area", TypeName = "decimal(8, 2)")]
    public decimal Area { get; set; }

    /// <summary>
    /// 最短租期(月)
    /// </summary>
    [Column("minimumRentalMonths")]
    public int MinimumRentalMonths { get; set; }

    /// <summary>
    /// 特殊守則
    /// </summary>
    [Column("specialRules")]
    public string? SpecialRules { get; set; }

    /// <summary>
    /// 水費計算方式
    /// </summary>
    [Column("waterFeeType")]
    [StringLength(20)]
    public string WaterFeeType { get; set; } = null!;

    /// <summary>
    /// 自訂水費
    /// </summary>
    [Column("customWaterFee", TypeName = "decimal(8, 2)")]
    public decimal? CustomWaterFee { get; set; }

    /// <summary>
    /// 電費計算方式
    /// </summary>
    [Column("electricityFeeType")]
    [StringLength(20)]
    public string ElectricityFeeType { get; set; } = null!;

    /// <summary>
    /// 自訂電費
    /// </summary>
    [Column("customElectricityFee", TypeName = "decimal(8, 2)")]
    public decimal? CustomElectricityFee { get; set; }

    /// <summary>
    /// 管理費含租金
    /// </summary>
    [Column("managementFeeIncluded")]
    public bool ManagementFeeIncluded { get; set; }

    /// <summary>
    /// 管理費金額
    /// </summary>
    [Column("managementFeeAmount", TypeName = "decimal(8, 2)")]
    public decimal? ManagementFeeAmount { get; set; }

    /// <summary>
    /// 有停車位
    /// </summary>
    [Column("parkingAvailable")]
    public bool ParkingAvailable { get; set; }

    /// <summary>
    /// 停車費需額外收費
    /// </summary>
    [Column("parkingFeeRequired")]
    public bool ParkingFeeRequired { get; set; }

    /// <summary>
    /// 停車位費用
    /// </summary>
    [Column("parkingFeeAmount", TypeName = "decimal(8, 2)")]
    public decimal? ParkingFeeAmount { get; set; }

    /// <summary>
    /// 清潔費需額外收費
    /// </summary>
    [Column("cleaningFeeRequired")]
    public bool CleaningFeeRequired { get; set; }

    /// <summary>
    /// 清潔費金額
    /// </summary>
    [Column("cleaningFeeAmount", TypeName = "decimal(8, 2)")]
    public decimal? CleaningFeeAmount { get; set; }

    /// <summary>
    /// 刊登天數
    /// </summary>
    [Column("listingDays")]
    public int? ListingDays { get; set; }

    /// <summary>
    /// 刊登費用
    /// </summary>
    [Column("listingFeeAmount", TypeName = "decimal(10, 2)")]
    public decimal? ListingFeeAmount { get; set; }

    /// <summary>
    /// 刊登費方案ID
    /// </summary>
    [Column("listingPlanID")]
    public int? ListingPlanId { get; set; }

    /// <summary>
    /// 付款狀態
    /// </summary>
    [Column("isPaid")]
    public bool IsPaid { get; set; }

    /// <summary>
    /// 完成付款時間
    /// </summary>
    [Column("paidAt")]
    [Precision(0)]
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// 上架到期時間
    /// </summary>
    [Column("expireAt")]
    [Precision(0)]
    public DateTime? ExpireAt { get; set; }

    /// <summary>
    /// 房產證明文件URL
    /// </summary>
    [Column("propertyProofURL")]
    [StringLength(500)]
    public string? PropertyProofUrl { get; set; }

    /// <summary>
    /// 預覽圖連結
    /// </summary>
    [Column("previewImageURL")]
    [StringLength(500)]
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// 房源狀態代碼
    /// </summary>
    [Column("statusCode")]
    [StringLength(20)]
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 上架日期
    /// </summary>
    [Column("publishedAt")]
    [Precision(0)]
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// 最後修改日期
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    [Column("deletedAt")]
    [Precision(0)]
    public DateTime? DeletedAt { get; set; }

    [Column("latitude", TypeName = "decimal(10, 8)")]
    public decimal? Latitude { get; set; }

    [Column("longitude", TypeName = "decimal(11, 8)")]
    public decimal? Longitude { get; set; }

    [InverseProperty("SourceProperty")]
    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    [InverseProperty("Property")]
    public virtual ICollection<Chatroom> Chatrooms { get; set; } = new List<Chatroom>();

    [InverseProperty("Property")]
    public virtual ICollection<CustomerServiceTicket> CustomerServiceTickets { get; set; } = new List<CustomerServiceTicket>();

    [InverseProperty("Property")]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    [InverseProperty("Property")]
    public virtual ICollection<FurnitureCart> FurnitureCarts { get; set; } = new List<FurnitureCart>();

    [InverseProperty("Property")]
    public virtual ICollection<FurnitureOrder> FurnitureOrders { get; set; } = new List<FurnitureOrder>();

    [ForeignKey("LandlordMemberId")]
    [InverseProperty("Properties")]
    public virtual Member LandlordMember { get; set; } = null!;

    [ForeignKey("ListingPlanId")]
    [InverseProperty("Properties")]
    public virtual ListingPlan? ListingPlan { get; set; }

    [InverseProperty("Property")]
    public virtual ICollection<PropertyComplaint> PropertyComplaints { get; set; } = new List<PropertyComplaint>();

    [InverseProperty("Property")]
    public virtual ICollection<PropertyEquipmentRelation> PropertyEquipmentRelations { get; set; } = new List<PropertyEquipmentRelation>();

    [InverseProperty("Property")]
    public virtual ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();

    [InverseProperty("Property")]
    public virtual ICollection<RentalApplication> RentalApplications { get; set; } = new List<RentalApplication>();
}
