using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 會員資料表
/// </summary>
[Table("members")]
[Index("Email", Name = "IX_members_email")]
[Index("PhoneNumber", Name = "IX_members_phone")]
public partial class Member
{
    /// <summary>
    /// 會員ID
    /// </summary>
    [Key]
    [Column("memberID")]
    public int MemberId { get; set; }

    /// <summary>
    /// 會員姓名
    /// </summary>
    [Column("memberName")]
    [StringLength(50)]
    public string MemberName { get; set; } = null!;

    /// <summary>
    /// 性別
    /// </summary>
    [Column("gender")]
    public byte Gender { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    [Column("birthDate")]
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// 密碼雜湊
    /// </summary>
    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 手機號碼
    /// </summary>
    [Column("phoneNumber")]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// 手機驗證通過時間
    /// </summary>
    [Column("phoneVerifiedAt")]
    [Precision(0)]
    public DateTime? PhoneVerifiedAt { get; set; }

    /// <summary>
    /// 電子信箱
    /// </summary>
    [Column("email")]
    [StringLength(254)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Email驗證通過時間
    /// </summary>
    [Column("emailVerifiedAt")]
    [Precision(0)]
    public DateTime? EmailVerifiedAt { get; set; }

    /// <summary>
    /// 身份驗證通過時間
    /// </summary>
    [Column("identityVerifiedAt")]
    [Precision(0)]
    public DateTime? IdentityVerifiedAt { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    [Column("lastLoginAt")]
    [Precision(0)]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Column("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 主要承租縣市ID
    /// </summary>
    [Column("primaryRentalCityID")]
    public int? PrimaryRentalCityId { get; set; }

    /// <summary>
    /// 主要承租區域ID
    /// </summary>
    [Column("primaryRentalDistrictID")]
    public int? PrimaryRentalDistrictId { get; set; }

    /// <summary>
    /// 居住縣市ID
    /// </summary>
    [Column("residenceCityID")]
    public int? ResidenceCityId { get; set; }

    /// <summary>
    /// 居住區域ID
    /// </summary>
    [Column("residenceDistrictID")]
    public int? ResidenceDistrictId { get; set; }

    /// <summary>
    /// 會員身份別ID
    /// </summary>
    [Column("memberTypeID")]
    public int? MemberTypeId { get; set; }

    /// <summary>
    /// 是否為房東
    /// </summary>
    [Column("isLandlord")]
    public bool IsLandlord { get; set; }

    /// <summary>
    /// 詳細地址
    /// </summary>
    [Column("addressLine")]
    [StringLength(200)]
    public string? AddressLine { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 身分證號
    /// </summary>
    [Column("nationalIdNo")]
    [StringLength(10)]
    [Unicode(false)]
    public string? NationalIdNo { get; set; }

    [InverseProperty("ApplicantMember")]
    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    [InverseProperty("InitiatorMember")]
    public virtual ICollection<Chatroom> ChatroomInitiatorMembers { get; set; } = new List<Chatroom>();

    [InverseProperty("SenderMember")]
    public virtual ICollection<ChatroomMessage> ChatroomMessages { get; set; } = new List<ChatroomMessage>();

    [InverseProperty("ParticipantMember")]
    public virtual ICollection<Chatroom> ChatroomParticipantMembers { get; set; } = new List<Chatroom>();

    [InverseProperty("Signer")]
    public virtual ICollection<ContractSignature> ContractSignatures { get; set; } = new List<ContractSignature>();

    [InverseProperty("Member")]
    public virtual ICollection<CustomerServiceTicket> CustomerServiceTickets { get; set; } = new List<CustomerServiceTicket>();

    [InverseProperty("Member")]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    [InverseProperty("Member")]
    public virtual ICollection<FurnitureCart> FurnitureCarts { get; set; } = new List<FurnitureCart>();

    [InverseProperty("Member")]
    public virtual ICollection<FurnitureOrder> FurnitureOrders { get; set; } = new List<FurnitureOrder>();

    [InverseProperty("UploadedByMember")]
    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    [ForeignKey("MemberTypeId")]
    [InverseProperty("Members")]
    public virtual MemberType? MemberType { get; set; }

    [InverseProperty("Member")]
    public virtual ICollection<MemberVerification> MemberVerifications { get; set; } = new List<MemberVerification>();

    [ForeignKey("PrimaryRentalCityId")]
    [InverseProperty("MemberPrimaryRentalCities")]
    public virtual City? PrimaryRentalCity { get; set; }

    [ForeignKey("PrimaryRentalDistrictId")]
    [InverseProperty("MemberPrimaryRentalDistricts")]
    public virtual District? PrimaryRentalDistrict { get; set; }

    [InverseProperty("LandlordMember")]
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    [InverseProperty("Complainant")]
    public virtual ICollection<PropertyComplaint> PropertyComplaintComplainants { get; set; } = new List<PropertyComplaint>();

    [InverseProperty("Landlord")]
    public virtual ICollection<PropertyComplaint> PropertyComplaintLandlords { get; set; } = new List<PropertyComplaint>();

    [InverseProperty("Member")]
    public virtual ICollection<RentalApplication> RentalApplications { get; set; } = new List<RentalApplication>();

    [InverseProperty("LandlordMember")]
    public virtual ICollection<RenterPostReply> RenterPostReplies { get; set; } = new List<RenterPostReply>();

    [InverseProperty("Member")]
    public virtual ICollection<RenterPost> RenterPosts { get; set; } = new List<RenterPost>();

    [ForeignKey("ResidenceCityId")]
    [InverseProperty("MemberResidenceCities")]
    public virtual City? ResidenceCity { get; set; }

    [ForeignKey("ResidenceDistrictId")]
    [InverseProperty("MemberResidenceDistricts")]
    public virtual District? ResidenceDistrict { get; set; }

    [InverseProperty("Member")]
    public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();

    [InverseProperty("Receiver")]
    public virtual ICollection<SystemMessage> SystemMessages { get; set; } = new List<SystemMessage>();

    [InverseProperty("Receiver")]
    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();

    [InverseProperty("Member")]
    public virtual ICollection<UserUpload> UserUploads { get; set; } = new List<UserUpload>();
}
