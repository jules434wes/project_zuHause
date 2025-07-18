using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 會員資料表
/// </summary>
public partial class Member
{
    /// <summary>
    /// 會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 會員姓名
    /// </summary>
    public string MemberName { get; set; } = null!;

    /// <summary>
    /// 性別
    /// </summary>
    public byte Gender { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// 密碼雜湊
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// 手機號碼
    /// </summary>
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// 手機驗證通過時間
    /// </summary>
    public DateTime? PhoneVerifiedAt { get; set; }

    /// <summary>
    /// 電子信箱
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Email驗證通過時間
    /// </summary>
    public DateTime? EmailVerifiedAt { get; set; }

    /// <summary>
    /// 身份驗證通過時間
    /// </summary>
    public DateTime? IdentityVerifiedAt { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 主要承租縣市ID
    /// </summary>
    public int? PrimaryRentalCityId { get; set; }

    /// <summary>
    /// 主要承租區域ID
    /// </summary>
    public int? PrimaryRentalDistrictId { get; set; }

    /// <summary>
    /// 居住縣市ID
    /// </summary>
    public int? ResidenceCityId { get; set; }

    /// <summary>
    /// 居住區域ID
    /// </summary>
    public int? ResidenceDistrictId { get; set; }

    /// <summary>
    /// 會員身份別ID
    /// </summary>
    public int? MemberTypeId { get; set; }

    /// <summary>
    /// 是否為房東
    /// </summary>
    public bool IsLandlord { get; set; }

    /// <summary>
    /// 詳細地址
    /// </summary>
    public string? AddressLine { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 身分證號
    /// </summary>
    public string? NationalIdNo { get; set; }

    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    public virtual ICollection<Chatroom> ChatroomInitiatorMembers { get; set; } = new List<Chatroom>();

    public virtual ICollection<ChatroomMessage> ChatroomMessages { get; set; } = new List<ChatroomMessage>();

    public virtual ICollection<Chatroom> ChatroomParticipantMembers { get; set; } = new List<Chatroom>();

    public virtual ICollection<ContractSignature> ContractSignatures { get; set; } = new List<ContractSignature>();

    public virtual ICollection<CustomerServiceTicket> CustomerServiceTickets { get; set; } = new List<CustomerServiceTicket>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<FurnitureCart> FurnitureCarts { get; set; } = new List<FurnitureCart>();

    public virtual ICollection<FurnitureOrder> FurnitureOrders { get; set; } = new List<FurnitureOrder>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual MemberType? MemberType { get; set; }

    public virtual ICollection<MemberVerification> MemberVerifications { get; set; } = new List<MemberVerification>();

    public virtual City? PrimaryRentalCity { get; set; }

    public virtual District? PrimaryRentalDistrict { get; set; }

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<PropertyComplaint> PropertyComplaintComplainants { get; set; } = new List<PropertyComplaint>();

    public virtual ICollection<PropertyComplaint> PropertyComplaintLandlords { get; set; } = new List<PropertyComplaint>();

    public virtual ICollection<RentalApplication> RentalApplications { get; set; } = new List<RentalApplication>();

    public virtual ICollection<RenterPostReply> RenterPostReplies { get; set; } = new List<RenterPostReply>();

    public virtual ICollection<RenterPost> RenterPosts { get; set; } = new List<RenterPost>();

    public virtual City? ResidenceCity { get; set; }

    public virtual District? ResidenceDistrict { get; set; }

    public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();

    public virtual ICollection<SystemMessage> SystemMessages { get; set; } = new List<SystemMessage>();

    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();

    public virtual ICollection<UserUpload> UserUploads { get; set; } = new List<UserUpload>();
}
