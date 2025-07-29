using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace zuHause.Models;

public partial class ZuHauseContext : DbContext
{
    public ZuHauseContext()
    {
    }

    public ZuHauseContext(DbContextOptions<ZuHauseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<AdminMessageTemplate> AdminMessageTemplates { get; set; }

    public virtual DbSet<AdminRole> AdminRoles { get; set; }

    public virtual DbSet<ApplicationStatusLog> ApplicationStatusLogs { get; set; }

    public virtual DbSet<Approval> Approvals { get; set; }

    public virtual DbSet<ApprovalItem> ApprovalItems { get; set; }

    public virtual DbSet<CarouselImage> CarouselImages { get; set; }

    public virtual DbSet<Chatroom> Chatrooms { get; set; }

    public virtual DbSet<ChatroomMessage> ChatroomMessages { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<ContractComment> ContractComments { get; set; }

    public virtual DbSet<ContractCustomField> ContractCustomFields { get; set; }

    public virtual DbSet<ContractFurnitureItem> ContractFurnitureItems { get; set; }

    public virtual DbSet<ContractSignature> ContractSignatures { get; set; }

    public virtual DbSet<ContractTemplate> ContractTemplates { get; set; }

    public virtual DbSet<CustomerServiceTicket> CustomerServiceTickets { get; set; }

    public virtual DbSet<DeliveryFeePlan> DeliveryFeePlans { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<FileApproval> FileApprovals { get; set; }

    public virtual DbSet<FurnitureCart> FurnitureCarts { get; set; }

    public virtual DbSet<FurnitureCartItem> FurnitureCartItems { get; set; }

    public virtual DbSet<FurnitureCategory> FurnitureCategories { get; set; }

    public virtual DbSet<FurnitureInventory> FurnitureInventories { get; set; }

    public virtual DbSet<FurnitureOrder> FurnitureOrders { get; set; }

    public virtual DbSet<FurnitureOrderHistory> FurnitureOrderHistories { get; set; }

    public virtual DbSet<FurnitureOrderItem> FurnitureOrderItems { get; set; }

    public virtual DbSet<FurnitureProduct> FurnitureProducts { get; set; }

    public virtual DbSet<FurnitureRentalContract> FurnitureRentalContracts { get; set; }

    public virtual DbSet<GoogleMapsApiUsage> GoogleMapsApiUsages { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<InventoryEvent> InventoryEvents { get; set; }

    public virtual DbSet<ListingPlan> ListingPlans { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MemberType> MemberTypes { get; set; }

    public virtual DbSet<MemberVerification> MemberVerifications { get; set; }

    public virtual DbSet<MessagePlacement> MessagePlacements { get; set; }

    public virtual DbSet<OrderEvent> OrderEvents { get; set; }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<PropertyComplaint> PropertyComplaints { get; set; }

    public virtual DbSet<PropertyEquipmentCategory> PropertyEquipmentCategories { get; set; }

    public virtual DbSet<PropertyEquipmentRelation> PropertyEquipmentRelations { get; set; }

    public virtual DbSet<PropertyImage> PropertyImages { get; set; }

    public virtual DbSet<RentalApplication> RentalApplications { get; set; }

    public virtual DbSet<RenterPost> RenterPosts { get; set; }

    public virtual DbSet<RenterPostReply> RenterPostReplies { get; set; }

    public virtual DbSet<RenterRequirementList> RenterRequirementLists { get; set; }

    public virtual DbSet<RenterRequirementRelation> RenterRequirementRelations { get; set; }

    public virtual DbSet<SearchHistory> SearchHistories { get; set; }

    public virtual DbSet<SiteMessage> SiteMessages { get; set; }

    public virtual DbSet<SystemCode> SystemCodes { get; set; }

    public virtual DbSet<SystemCodeCategory> SystemCodeCategories { get; set; }

    public virtual DbSet<SystemMessage> SystemMessages { get; set; }

    public virtual DbSet<UserNotification> UserNotifications { get; set; }

    public virtual DbSet<UserUpload> UserUploads { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:zuhause.database.windows.net,1433;Initial Catalog=zuHause;Persist Security Info=False;User ID=zuhause;Password=DB$MSIT67;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("admins", tb => tb.HasComment("管理員資料"));

            entity.Property(e => e.AdminId)
                .ValueGeneratedNever()
                .HasComment("管理員ID")
                .HasColumnName("adminID");
            entity.Property(e => e.Account)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("帳號")
                .HasColumnName("account");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.LastLoginAt)
                .HasPrecision(0)
                .HasComment("最後登入時間")
                .HasColumnName("lastLoginAt");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("姓名")
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasComment("密碼雜湊")
                .HasColumnName("passwordHash");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(64)
                .HasComment("密碼 Salt")
                .HasColumnName("passwordSalt");
            entity.Property(e => e.PasswordUpdatedAt)
                .HasPrecision(0)
                .HasComment("密碼更新時間")
                .HasColumnName("passwordUpdatedAt");
            entity.Property(e => e.RoleCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("角色代碼")
                .HasColumnName("roleCode");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.RoleCodeNavigation).WithMany(p => p.Admins)
                .HasForeignKey(d => d.RoleCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_admins_role");
        });

        modelBuilder.Entity<AdminMessageTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId);

            entity.ToTable("adminMessageTemplates", tb => tb.HasComment("後台訊息模板表"));

            entity.Property(e => e.TemplateId)
                .HasComment("模板ID(自動遞增)")
                .HasColumnName("templateID");
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("模板類別代碼")
                .HasColumnName("categoryCode");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.TemplateContent)
                .HasComment("內容")
                .HasColumnName("templateContent");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasComment("標題")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<AdminRole>(entity =>
        {
            entity.HasKey(e => e.RoleCode);

            entity.ToTable("adminRoles", tb => tb.HasComment("管理員角色資料"));

            entity.Property(e => e.RoleCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("角色代碼")
                .HasColumnName("roleCode");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.PermissionsJson)
                .HasComment("權限JSON")
                .HasColumnName("permissionsJSON");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasComment("角色名稱")
                .HasColumnName("roleName");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<ApplicationStatusLog>(entity =>
        {
            entity.HasKey(e => e.StatusLogId);

            entity.ToTable("applicationStatusLogs", tb => tb.HasComment("申請狀態歷程表"));

            entity.Property(e => e.StatusLogId)
                .HasComment("狀態歷程ID")
                .HasColumnName("statusLogID");
            entity.Property(e => e.ApplicationId)
                .HasComment("申請ID")
                .HasColumnName("applicationID");
            entity.Property(e => e.ChangedAt)
                .HasPrecision(0)
                .HasComment("進入狀態時間")
                .HasColumnName("changedAt");
            entity.Property(e => e.Note)
                .HasComment("備註")
                .HasColumnName("note");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(20)
                .HasComment("狀態代碼")
                .HasColumnName("statusCode");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Application).WithMany(p => p.ApplicationStatusLogs)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_appStatusLogs_application");
        });

        modelBuilder.Entity<Approval>(entity =>
        {
            entity.ToTable("approvals", tb =>
                {
                    tb.HasComment("審核主檔");
                    tb.HasTrigger("trg_approvals_identity_sync");
                    tb.HasTrigger("trg_approvals_landlord_sync");
                    tb.HasTrigger("trg_approvals_property_sync");
                });

            entity.HasIndex(e => e.ApplicantMemberId, "IX_approvals_applicantMemberID");

            entity.HasIndex(e => e.CreatedAt, "IX_approvals_createdAt");

            entity.HasIndex(e => e.CurrentApproverId, "IX_approvals_currentApproverID");

            entity.HasIndex(e => e.ModuleCode, "IX_approvals_moduleCode");

            entity.HasIndex(e => e.StatusCode, "IX_approvals_statusCode");

            entity.HasIndex(e => new { e.StatusCategory, e.StatusCode }, "IX_approvals_status_category");

            entity.HasIndex(e => new { e.ModuleCode, e.SourcePropertyId }, "UQ_approvals_module_source")
                .IsUnique()
                .HasFillFactor(100);


            entity.HasIndex(e => new { e.ModuleCode, e.ApplicantMemberId, e.SourcePropertyId }, "UQ_approvals_member_module").IsUnique();


            entity.Property(e => e.ApprovalId)
                .HasComment("審核ID (自動遞增，從701開始)")
                .HasColumnName("approvalID");
            entity.Property(e => e.ApplicantMemberId)
                .HasComment("申請會員ID")
                .HasColumnName("applicantMemberID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrentApproverId)
                .HasComment("審核人員ID")
                .HasColumnName("currentApproverID");
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(20)
                .HasComment("模組代碼")
                .HasColumnName("moduleCode");
            entity.Property(e => e.SourcePropertyId)
                .HasComment("審核房源ID")
                .HasColumnName("sourcePropertyID");
            entity.Property(e => e.StatusCategory)
                .HasMaxLength(20)
                .HasComputedColumnSql("(CONVERT([nvarchar](20),N'APPROVAL_STATUS'))", true)
                .HasComment("狀態類別 (計算欄位)")
                .HasColumnName("statusCategory");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(20)
                .HasComment("審核狀態碼")
                .HasColumnName("statusCode");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.ApplicantMember).WithMany(p => p.Approvals)
                .HasForeignKey(d => d.ApplicantMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_approvals_applicant");

            entity.HasOne(d => d.SourceProperty).WithMany(p => p.Approvals)
                .HasForeignKey(d => d.SourcePropertyId)
                .HasConstraintName("FK_approvals_Property");

            entity.HasOne(d => d.SystemCode).WithMany(p => p.Approvals)
                .HasForeignKey(d => new { d.StatusCategory, d.StatusCode })
                .HasConstraintName("FK_approvals_status");
        });

        modelBuilder.Entity<ApprovalItem>(entity =>
        {
            entity.ToTable("approvalItems", tb => tb.HasComment("審核明細"));

            entity.HasIndex(e => e.ActionBy, "IX_approvalItems_actionBy");

            entity.HasIndex(e => e.ActionType, "IX_approvalItems_actionType");

            entity.HasIndex(e => new { e.ActionCategory, e.ActionType }, "IX_approvalItems_action_category");

            entity.HasIndex(e => e.ApprovalId, "IX_approvalItems_approvalID");

            entity.HasIndex(e => e.CreatedAt, "IX_approvalItems_createdAt");

            entity.Property(e => e.ApprovalItemId)
                .HasComment("審核明細ID (自動遞增，從801開始)")
                .HasColumnName("approvalItemID");
            entity.Property(e => e.ActionBy)
                .HasComment("操作者ID")
                .HasColumnName("actionBy");
            entity.Property(e => e.ActionCategory)
                .HasMaxLength(20)
                .HasComputedColumnSql("(CONVERT([nvarchar](20),N'APPROVAL_ACTION'))", true)
                .HasComment("操作類別 (計算欄位)")
                .HasColumnName("actionCategory");
            entity.Property(e => e.ActionNote)
                .HasComment("內部操作備註")
                .HasColumnName("actionNote");
            entity.Property(e => e.ActionType)
                .HasMaxLength(20)
                .HasComment("操作類型")
                .HasColumnName("actionType");
            entity.Property(e => e.ApprovalId)
                .HasComment("審核ID")
                .HasColumnName("approvalID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.SnapshotJson)
                .HasComment("審核快照JSON")
                .HasColumnName("snapshotJSON");

            entity.HasOne(d => d.Approval).WithMany(p => p.ApprovalItems)
                .HasForeignKey(d => d.ApprovalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_approvalItems_approval");

            entity.HasOne(d => d.SystemCode).WithMany(p => p.ApprovalItems)
                .HasForeignKey(d => new { d.ActionCategory, d.ActionType })
                .HasConstraintName("FK_approvalItems_action");
        });

        modelBuilder.Entity<CarouselImage>(entity =>
        {
            entity.ToTable("carouselImages", tb => tb.HasComment("輪播圖片表"));

            entity.HasIndex(e => new { e.StartAt, e.EndAt }, "IX_carouselImages_active_time");

            entity.Property(e => e.CarouselImageId)
                .HasComment("圖片ID")
                .HasColumnName("carouselImageId");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("分類")
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.DisplayOrder)
                .HasComment("顯示順序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.EndAt)
                .HasPrecision(0)
                .HasComment("結束時間")
                .HasColumnName("endAt");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasComment("圖片URL")
                .HasColumnName("imageUrl");
            entity.Property(e => e.ImagesName)
                .HasMaxLength(50)
                .HasComment("名稱")
                .HasColumnName("imagesName");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.PageCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("頁面識別碼")
                .HasColumnName("pageCode");
            entity.Property(e => e.StartAt)
                .HasPrecision(0)
                .HasComment("開始時間")
                .HasColumnName("startAt");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.PageCodeNavigation).WithMany(p => p.CarouselImages)
                .HasForeignKey(d => d.PageCode)
                .HasConstraintName("FK_carouselImages_page");
        });

        modelBuilder.Entity<Chatroom>(entity =>
        {
            entity.ToTable("chatrooms", tb => tb.HasComment("聊天室"));

            entity.HasIndex(e => e.LastMessageAt, "IX_chatrooms_lastmsg");

            entity.HasIndex(e => new { e.InitiatorMemberId, e.ParticipantMemberId }, "UQ_chatrooms_members").IsUnique();

            entity.Property(e => e.ChatroomId)
                .HasComment("聊天室ID")
                .HasColumnName("chatroomID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.InitiatorMemberId)
                .HasComment("發起人會員ID")
                .HasColumnName("initiatorMemberID");
            entity.Property(e => e.LastMessageAt)
                .HasPrecision(0)
                .HasComment("最後訊息時間")
                .HasColumnName("lastMessageAt");
            entity.Property(e => e.ParticipantMemberId)
                .HasComment("參與者會員ID")
                .HasColumnName("participantMemberID");
            entity.Property(e => e.PropertyId)
                .HasComment("房源ID")
                .HasColumnName("propertyID");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.InitiatorMember).WithMany(p => p.ChatroomInitiatorMembers)
                .HasForeignKey(d => d.InitiatorMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_chatrooms_initiator");

            entity.HasOne(d => d.ParticipantMember).WithMany(p => p.ChatroomParticipantMembers)
                .HasForeignKey(d => d.ParticipantMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_chatrooms_participant");

            entity.HasOne(d => d.Property).WithMany(p => p.Chatrooms)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("FK_chatrooms_property");
        });

        modelBuilder.Entity<ChatroomMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);

            entity.ToTable("chatroomMessages", tb => tb.HasComment("聊天室訊息"));

            entity.HasIndex(e => new { e.ChatroomId, e.SentAt }, "IX_chatroomMessages_chat_time");

            entity.Property(e => e.MessageId)
                .HasComment("訊息ID")
                .HasColumnName("messageID");
            entity.Property(e => e.ChatroomId)
                .HasComment("聊天室ID")
                .HasColumnName("chatroomID");
            entity.Property(e => e.IsRead)
                .HasComment("是否已讀")
                .HasColumnName("isRead");
            entity.Property(e => e.MessageContent)
                .HasMaxLength(1000)
                .HasComment("內容")
                .HasColumnName("messageContent");
            entity.Property(e => e.ReadAt)
                .HasPrecision(0)
                .HasComment("已讀時間")
                .HasColumnName("readAt");
            entity.Property(e => e.SenderMemberId)
                .HasComment("發送者會員ID")
                .HasColumnName("senderMemberID");
            entity.Property(e => e.SentAt)
                .HasPrecision(0)
                .HasComment("傳送時間")
                .HasColumnName("sentAt");

            entity.HasOne(d => d.Chatroom).WithMany(p => p.ChatroomMessages)
                .HasForeignKey(d => d.ChatroomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_chatroomMessages_chatroom");

            entity.HasOne(d => d.SenderMember).WithMany(p => p.ChatroomMessages)
                .HasForeignKey(d => d.SenderMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_chatroomMessages_sender");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("cities", tb => tb.HasComment("縣市代碼表"));

            entity.HasIndex(e => e.CityCode, "UQ_cities_cityCode").IsUnique();

            entity.Property(e => e.CityId).HasColumnName("cityId");
            entity.Property(e => e.CityCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasComment("縣市代碼")
                .HasColumnName("cityCode");
            entity.Property(e => e.CityName)
                .HasMaxLength(50)
                .HasComment("縣市名稱")
                .HasColumnName("cityName");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DisplayOrder)
                .HasComment("排序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("contracts", tb => tb.HasComment("合約欄位儲存表"));

            entity.HasIndex(e => new { e.Status, e.StartDate, e.EndDate }, "IX_contracts_status_dates");

            entity.Property(e => e.ContractId)
                .HasComment("合約ID")
                .HasColumnName("contractId");
            entity.Property(e => e.CleaningFee)
                .HasComment("清潔費")
                .HasColumnName("cleaningFee");
            entity.Property(e => e.CourtJurisdiction)
                .HasMaxLength(50)
                .HasComment("管轄法院")
                .HasColumnName("courtJurisdiction");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.CustomName)
                .HasMaxLength(50)
                .HasComment("合約自訂名稱")
                .HasColumnName("customName");
            entity.Property(e => e.DepositAmount)
                .HasComment("押金金額")
                .HasColumnName("depositAmount");
            entity.Property(e => e.EndDate)
                .HasComment("合約結束日")
                .HasColumnName("endDate");
            entity.Property(e => e.IsSublettable)
                .HasComment("是否可轉租")
                .HasColumnName("isSublettable");
            entity.Property(e => e.LandlordHouseholdAddress).HasMaxLength(100);
            entity.Property(e => e.ManagementFee)
                .HasComment("管理費")
                .HasColumnName("managementFee");
            entity.Property(e => e.ParkingFee)
                .HasComment("停車費")
                .HasColumnName("parkingFee");
            entity.Property(e => e.PenaltyAmount)
                .HasComment("違約金金額")
                .HasColumnName("penaltyAmount");
            entity.Property(e => e.RentalApplicationId)
                .HasComment("申請編號ID")
                .HasColumnName("rentalApplicationId");
            entity.Property(e => e.StartDate)
                .HasComment("合約起始日")
                .HasColumnName("startDate");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("合約狀態")
                .HasColumnName("status");
            entity.Property(e => e.TemplateId)
                .HasComment("合約範本編號")
                .HasColumnName("templateId");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
            entity.Property(e => e.UsagePurpose)
                .HasMaxLength(20)
                .HasComment("使用目的")
                .HasColumnName("usagePurpose");

            entity.HasOne(d => d.RentalApplication).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.RentalApplicationId)
                .HasConstraintName("FK_contracts_rentalApp");

            entity.HasOne(d => d.Template).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("FK_contracts_template");
        });

        modelBuilder.Entity<ContractComment>(entity =>
        {
            entity.ToTable("contractComments", tb => tb.HasComment("合約備註表"));

            entity.Property(e => e.ContractCommentId)
                .HasComment("合約備註ID")
                .HasColumnName("contractCommentId");
            entity.Property(e => e.CommentText)
                .HasMaxLength(100)
                .HasComment("內容")
                .HasColumnName("commentText");
            entity.Property(e => e.CommentType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("備註類型")
                .HasColumnName("commentType");
            entity.Property(e => e.ContractId)
                .HasComment("合約ID")
                .HasColumnName("contractId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.CreatedById)
                .HasComment("建立者")
                .HasColumnName("createdById");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractComments)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_contractComments_contract");
        });

        modelBuilder.Entity<ContractCustomField>(entity =>
        {
            entity.ToTable("contractCustomFields", tb => tb.HasComment("合約附表(動態欄位)"));

            entity.Property(e => e.ContractCustomFieldId)
                .HasComment("動態欄位ID")
                .HasColumnName("contractCustomFieldId");
            entity.Property(e => e.ContractId)
                .HasComment("合約ID")
                .HasColumnName("contractId");
            entity.Property(e => e.DisplayOrder)
                .HasComment("顯示順序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.FieldKey)
                .HasMaxLength(30)
                .HasComment("動態欄位名稱")
                .HasColumnName("fieldKey");
            entity.Property(e => e.FieldType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("動態欄位型態")
                .HasColumnName("fieldType");
            entity.Property(e => e.FieldValue)
                .HasMaxLength(100)
                .HasComment("動態欄位值")
                .HasColumnName("fieldValue");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractCustomFields)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_contractCustomFields_contract");
        });

        modelBuilder.Entity<ContractFurnitureItem>(entity =>
        {
            entity.ToTable("contractFurnitureItems", tb => tb.HasComment("合約內容家具表"));

            entity.Property(e => e.ContractFurnitureItemId)
                .HasComment("家具清單ID")
                .HasColumnName("contractFurnitureItemId");
            entity.Property(e => e.Amount)
                .HasComment("小計")
                .HasColumnName("amount");
            entity.Property(e => e.ContractId)
                .HasComment("合約ID")
                .HasColumnName("contractId");
            entity.Property(e => e.FurnitureCondition)
                .HasMaxLength(30)
                .HasComment("家具狀況")
                .HasColumnName("furnitureCondition");
            entity.Property(e => e.FurnitureName)
                .HasMaxLength(50)
                .HasComment("家具名稱")
                .HasColumnName("furnitureName");
            entity.Property(e => e.Quantity)
                .HasComment("數量")
                .HasColumnName("quantity");
            entity.Property(e => e.RepairChargeOwner)
                .HasMaxLength(20)
                .HasComment("修繕費負責人")
                .HasColumnName("repairChargeOwner");
            entity.Property(e => e.RepairResponsibility)
                .HasMaxLength(20)
                .HasComment("維修權責")
                .HasColumnName("repairResponsibility");
            entity.Property(e => e.UnitPrice)
                .HasComment("單價")
                .HasColumnName("unitPrice");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractFurnitureItems)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_contractFurnitureItems_contract");
        });

        modelBuilder.Entity<ContractSignature>(entity =>
        {
            entity.HasKey(e => e.IdcontractSignatureId);

            entity.ToTable("contractSignatures", tb => tb.HasComment("電子簽名儲存表"));

            entity.Property(e => e.IdcontractSignatureId)
                .HasComment("電子簽名ID")
                .HasColumnName("idcontractSignatureId");
            entity.Property(e => e.ContractId)
                .HasComment("合約ID")
                .HasColumnName("contractId");
            entity.Property(e => e.SignMethod)
                .HasMaxLength(20)
                .HasComment("簽名方式")
                .HasColumnName("signMethod");
            entity.Property(e => e.SignVerifyInfo)
                .HasMaxLength(255)
                .HasComment("簽署驗證資訊")
                .HasColumnName("signVerifyInfo");
            entity.Property(e => e.SignatureFileUrl)
                .HasMaxLength(255)
                .HasComment("簽名檔URL")
                .HasColumnName("signatureFileUrl");
            entity.Property(e => e.SignedAt)
                .HasPrecision(0)
                .HasComment("時間戳")
                .HasColumnName("signedAt");
            entity.Property(e => e.SignerId)
                .HasComment("簽約人ID")
                .HasColumnName("signerId");
            entity.Property(e => e.SignerRole)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("簽署人身份")
                .HasColumnName("signerRole");
            entity.Property(e => e.UploadId).HasColumnName("uploadId");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractSignatures)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_contractSignatures_contract");

            entity.HasOne(d => d.Signer).WithMany(p => p.ContractSignatures)
                .HasForeignKey(d => d.SignerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_contractSignatures_member");
        });

        modelBuilder.Entity<ContractTemplate>(entity =>
        {
            entity.ToTable("contractTemplates", tb => tb.HasComment("合約範本表"));

            entity.Property(e => e.ContractTemplateId)
                .ValueGeneratedNever()
                .HasComment("範本ID")
                .HasColumnName("contractTemplateId");
            entity.Property(e => e.TemplateContent)
                .HasComment("範本內容")
                .HasColumnName("templateContent");
            entity.Property(e => e.TemplateName)
                .HasMaxLength(30)
                .HasComment("範本名稱")
                .HasColumnName("templateName");
            entity.Property(e => e.UploadedAt)
                .HasPrecision(0)
                .HasComment("範本上傳時間")
                .HasColumnName("uploadedAt");
        });

        modelBuilder.Entity<CustomerServiceTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId);

            entity.ToTable("customerServiceTickets", tb => tb.HasComment("客服聯繫表"));

            entity.HasIndex(e => e.CategoryCode, "IX_customerServiceTickets_categoryCode");

            entity.HasIndex(e => e.ContractId, "IX_customerServiceTickets_contractId");

            entity.HasIndex(e => e.CreatedAt, "IX_customerServiceTickets_createdAt");

            entity.HasIndex(e => e.IsResolved, "IX_customerServiceTickets_isResolved");

            entity.HasIndex(e => e.MemberId, "IX_customerServiceTickets_memberId");

            entity.HasIndex(e => e.PropertyId, "IX_customerServiceTickets_propertyId");

            entity.HasIndex(e => e.StatusCode, "IX_customerServiceTickets_statusCode");

            entity.Property(e => e.TicketId)
                .HasComment("客服單ID (自動遞增，從201開始)")
                .HasColumnName("ticketId");
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("主分類代碼")
                .HasColumnName("categoryCode");
            entity.Property(e => e.ContractId)
                .HasComment("租約ID")
                .HasColumnName("contractId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.FurnitureOrderId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("家具訂單ID")
                .HasColumnName("furnitureOrderId");
            entity.Property(e => e.HandledBy)
                .HasComment("客服人員ID")
                .HasColumnName("handledBy");
            entity.Property(e => e.IsResolved)
                .HasComment("是否結案")
                .HasColumnName("isResolved");
            entity.Property(e => e.MemberId)
                .HasComment("使用者ID")
                .HasColumnName("memberId");
            entity.Property(e => e.PropertyId)
                .HasComment("房源ID")
                .HasColumnName("propertyId");
            entity.Property(e => e.ReplyAt)
                .HasPrecision(0)
                .HasComment("最後回覆時間")
                .HasColumnName("replyAt");
            entity.Property(e => e.ReplyContent)
                .HasComment("客服最後回覆")
                .HasColumnName("replyContent");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("狀態代碼")
                .HasColumnName("statusCode");
            entity.Property(e => e.Subject)
                .HasMaxLength(100)
                .HasComment("主旨")
                .HasColumnName("subject");
            entity.Property(e => e.TicketContent)
                .HasComment("需求內容")
                .HasColumnName("ticketContent");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Contract).WithMany(p => p.CustomerServiceTickets)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_custTickets_contract");

            entity.HasOne(d => d.FurnitureOrder).WithMany(p => p.CustomerServiceTickets)
                .HasForeignKey(d => d.FurnitureOrderId)
                .HasConstraintName("FK_custTickets_furnOrder");

            entity.HasOne(d => d.Member).WithMany(p => p.CustomerServiceTickets)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_custTickets_member");

            entity.HasOne(d => d.Property).WithMany(p => p.CustomerServiceTickets)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("FK_custTickets_property");
        });

        modelBuilder.Entity<DeliveryFeePlan>(entity =>
        {
            entity.HasKey(e => e.PlanId);

            entity.ToTable("deliveryFeePlans", tb => tb.HasComment("家具配送費方案表"));

            entity.Property(e => e.PlanId)
                .ValueGeneratedNever()
                .HasComment("配送方案ID")
                .HasColumnName("planId");
            entity.Property(e => e.BaseFee)
                .HasComment("基本費用")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("baseFee");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasComment("幣別")
                .HasColumnName("currencyCode");
            entity.Property(e => e.EndAt)
                .HasPrecision(0)
                .HasComment("結束時間")
                .HasColumnName("endAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.MaxWeightKg)
                .HasComment("重量上限KG")
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("maxWeightKG");
            entity.Property(e => e.PlanName)
                .HasMaxLength(50)
                .HasComment("方案名稱")
                .HasColumnName("planName");
            entity.Property(e => e.RemoteAreaSurcharge)
                .HasComment("偏遠加收")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("remoteAreaSurcharge");
            entity.Property(e => e.StartAt)
                .HasPrecision(0)
                .HasComment("生效時間")
                .HasColumnName("startAt");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("districts", tb => tb.HasComment("鄉鎮區代碼表"));

            entity.HasIndex(e => new { e.CityId, e.DistrictCode }, "UQ_districts_city_districtCode").IsUnique();

            entity.Property(e => e.DistrictId)
                .HasComment("鄉鎮區ID")
                .HasColumnName("districtId");
            entity.Property(e => e.CityCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasComment("縣市代碼")
                .HasColumnName("cityCode");
            entity.Property(e => e.CityId)
                .HasComment("縣市ID")
                .HasColumnName("cityId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DisplayOrder)
                .HasComment("排序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.DistrictCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasComment("區代碼")
                .HasColumnName("districtCode");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(50)
                .HasComment("區名稱")
                .HasColumnName("districtName");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasComment("郵遞區號")
                .HasColumnName("zipCode");

            entity.HasOne(d => d.City).WithMany(p => p.Districts)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_districts_cities");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.MemberId, e.PropertyId });

            entity.ToTable("favorites", tb => tb.HasComment("收藏表"));

            entity.HasIndex(e => e.PropertyId, "IX_favorites_property");

            entity.Property(e => e.MemberId)
                .HasComment("會員ID")
                .HasColumnName("memberID");
            entity.Property(e => e.PropertyId)
                .HasComment("房源ID")
                .HasColumnName("propertyID");
            entity.Property(e => e.FavoritedAt)
                .HasPrecision(0)
                .HasComment("收藏時間")
                .HasColumnName("favoritedAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否有效")
                .HasColumnName("isActive");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Member).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorites_member");

            entity.HasOne(d => d.Property).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorites_property");
        });

        modelBuilder.Entity<FileApproval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId);

            entity.ToTable("fileApprovals", tb => tb.HasComment("檔案審核"));

            entity.Property(e => e.ApprovalId)
                .ValueGeneratedNever()
                .HasComment("審核ID")
                .HasColumnName("approvalID");
            entity.Property(e => e.AppliedAt)
                .HasPrecision(0)
                .HasComment("申請時間")
                .HasColumnName("appliedAt");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.MemberId)
                .HasComment("會員ID")
                .HasColumnName("memberID");
            entity.Property(e => e.ResultDescription)
                .HasMaxLength(500)
                .HasComment("審核說明")
                .HasColumnName("resultDescription");
            entity.Property(e => e.ReviewedAt)
                .HasPrecision(0)
                .HasComment("審核時間")
                .HasColumnName("reviewedAt");
            entity.Property(e => e.ReviewerAdminId)
                .HasComment("審核人員ID")
                .HasColumnName("reviewerAdminID");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("審核狀態代碼")
                .HasColumnName("statusCode");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
            entity.Property(e => e.UploadId)
                .HasComment("上傳ID")
                .HasColumnName("uploadID");

            entity.HasOne(d => d.Upload).WithMany(p => p.FileApprovals)
                .HasForeignKey(d => d.UploadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_fileApprovals_upload");
        });

        modelBuilder.Entity<FurnitureCart>(entity =>
        {
            entity.ToTable("furnitureCarts", tb => tb.HasComment("家具購物車表"));

            entity.Property(e => e.FurnitureCartId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("購物車ID")
                .HasColumnName("furnitureCartId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.MemberId)
                .HasComment("會員ID")
                .HasColumnName("memberId");
            entity.Property(e => e.PropertyId)
                .HasComment("房源ID")
                .HasColumnName("propertyId");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("狀態")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Member).WithMany(p => p.FurnitureCarts)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureCarts_member");

            entity.HasOne(d => d.Property).WithMany(p => p.FurnitureCarts)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("FK_furnitureCarts_property");
        });

        modelBuilder.Entity<FurnitureCartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId);

            entity.ToTable("furnitureCartItems", tb => tb.HasComment("家具購物車明細表"));

            entity.Property(e => e.CartItemId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("明細ID")
                .HasColumnName("cartItemId");
            entity.Property(e => e.CartId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("購物車ID")
                .HasColumnName("cartId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("商品ID")
                .HasColumnName("productId");
            entity.Property(e => e.Quantity)
                .HasComment("數量")
                .HasColumnName("quantity");
            entity.Property(e => e.RentalDays)
                .HasComment("租期(天)")
                .HasColumnName("rentalDays");
            entity.Property(e => e.SubTotal)
                .HasComment("小計")
                .HasColumnType("decimal(12, 0)")
                .HasColumnName("subTotal");
            entity.Property(e => e.UnitPriceSnapshot)
                .HasComment("單價快照")
                .HasColumnType("decimal(10, 0)")
                .HasColumnName("unitPriceSnapshot");

            entity.HasOne(d => d.Cart).WithMany(p => p.FurnitureCartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureCartItems_cart");

            entity.HasOne(d => d.Product).WithMany(p => p.FurnitureCartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureCartItems_product");
        });

        modelBuilder.Entity<FurnitureCategory>(entity =>
        {
            entity.HasKey(e => e.FurnitureCategoriesId);

            entity.ToTable("furnitureCategories", tb => tb.HasComment("家具商品分類表"));

            entity.Property(e => e.FurnitureCategoriesId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("分類ID")
                .HasColumnName("furnitureCategoriesId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.Depth)
                .HasComment("階層層級")
                .HasColumnName("depth");
            entity.Property(e => e.DisplayOrder)
                .HasComment("顯示排序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("分類名稱")
                .HasColumnName("name");
            entity.Property(e => e.ParentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("上層分類ID")
                .HasColumnName("parentId");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_furnCate_parent");
        });

        modelBuilder.Entity<FurnitureInventory>(entity =>
        {
            entity.ToTable("furnitureInventory", tb => tb.HasComment("家具庫存表"));

            entity.HasIndex(e => new { e.ProductId, e.AvailableQuantity }, "IX_furnInventory_available");

            entity.Property(e => e.FurnitureInventoryId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("庫存ID")
                .HasColumnName("furnitureInventoryId");
            entity.Property(e => e.AvailableQuantity)
                .HasComment("可用庫存")
                .HasColumnName("availableQuantity");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("商品ID")
                .HasColumnName("productId");
            entity.Property(e => e.RentedQuantity)
                .HasComment("已出租數量")
                .HasColumnName("rentedQuantity");
            entity.Property(e => e.SafetyStock)
                .HasComment("安全庫存")
                .HasColumnName("safetyStock");
            entity.Property(e => e.TotalQuantity)
                .HasComment("總庫存數量")
                .HasColumnName("totalQuantity");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("最後更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Product).WithMany(p => p.FurnitureInventories)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureInventory_product");
        });

        modelBuilder.Entity<FurnitureOrder>(entity =>
        {
            entity.ToTable("furnitureOrders", tb => tb.HasComment("家具訂單查詢表"));

            entity.HasIndex(e => new { e.MemberId, e.Status }, "IX_furnOrders_member_status");

            entity.Property(e => e.FurnitureOrderId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("訂單ID")
                .HasColumnName("furnitureOrderId");
            entity.Property(e => e.ContractLink)
                .HasMaxLength(255)
                .HasComment("合約連結")
                .HasColumnName("contractLink");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("成立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.MemberId)
                .HasComment("會員ID")
                .HasColumnName("memberId");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("付款狀態")
                .HasColumnName("paymentStatus");
            entity.Property(e => e.PropertyId)
                .HasComment("房源ID")
                .HasColumnName("propertyId");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("訂單狀態")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasComment("總金額")
                .HasColumnType("decimal(12, 0)")
                .HasColumnName("totalAmount");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Member).WithMany(p => p.FurnitureOrders)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrders_member");

            entity.HasOne(d => d.Property).WithMany(p => p.FurnitureOrders)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("FK_furnitureOrders_property");
        });

        modelBuilder.Entity<FurnitureOrderHistory>(entity =>
        {
            entity.ToTable("furnitureOrderHistory", tb => tb.HasComment("家具歷史訂單清單"));

            entity.Property(e => e.FurnitureOrderHistoryId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("流水號")
                .HasColumnName("furnitureOrderHistoryId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DailyRentalSnapshot)
                .HasComment("單日租金快照")
                .HasColumnType("decimal(10, 0)")
                .HasColumnName("dailyRentalSnapshot");
            entity.Property(e => e.DamageNote)
                .HasMaxLength(255)
                .HasComment("損壞說明")
                .HasColumnName("damageNote");
            entity.Property(e => e.DescriptionSnapshot)
                .HasComment("商品描述快照")
                .HasColumnName("descriptionSnapshot");
            entity.Property(e => e.ItemStatus)
                .HasMaxLength(50)
                .HasComment("明細狀態")
                .HasColumnName("itemStatus");
            entity.Property(e => e.OrderId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("訂單ID")
                .HasColumnName("orderId");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("商品ID")
                .HasColumnName("productId");
            entity.Property(e => e.ProductNameSnapshot)
                .HasMaxLength(100)
                .HasComment("商品名稱快照")
                .HasColumnName("productNameSnapshot");
            entity.Property(e => e.Quantity)
                .HasComment("數量")
                .HasColumnName("quantity");
            entity.Property(e => e.RentalEnd)
                .HasComment("租借結束日")
                .HasColumnName("rentalEnd");
            entity.Property(e => e.RentalStart)
                .HasComment("租借開始日")
                .HasColumnName("rentalStart");
            entity.Property(e => e.ReturnedAt)
                .HasComment("實際歸還日期")
                .HasColumnName("returnedAt");
            entity.Property(e => e.SubTotal)
                .HasComment("小計")
                .HasColumnType("decimal(12, 0)")
                .HasColumnName("subTotal");

            entity.HasOne(d => d.Order).WithMany(p => p.FurnitureOrderHistories)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrderHistory_order");

            entity.HasOne(d => d.Product).WithMany(p => p.FurnitureOrderHistories)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrderHistory_product");
        });

        modelBuilder.Entity<FurnitureOrderItem>(entity =>
        {
            entity.ToTable("furnitureOrderItems", tb => tb.HasComment("家具訂單查詢明細表"));

            entity.Property(e => e.FurnitureOrderItemId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("明細ID")
                .HasColumnName("furnitureOrderItemId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DailyRentalSnapshot)
                .HasComment("單日租金快照")
                .HasColumnType("decimal(10, 0)")
                .HasColumnName("dailyRentalSnapshot");
            entity.Property(e => e.OrderId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("訂單ID")
                .HasColumnName("orderId");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("商品ID")
                .HasColumnName("productId");
            entity.Property(e => e.Quantity)
                .HasComment("數量")
                .HasColumnName("quantity");
            entity.Property(e => e.RentalDays)
                .HasComment("租期(天)")
                .HasColumnName("rentalDays");
            entity.Property(e => e.SubTotal)
                .HasComment("小計")
                .HasColumnType("decimal(12, 0)")
                .HasColumnName("subTotal");

            entity.HasOne(d => d.Order).WithMany(p => p.FurnitureOrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrderItems_order");

            entity.HasOne(d => d.Product).WithMany(p => p.FurnitureOrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrderItems_product");
        });

        modelBuilder.Entity<FurnitureProduct>(entity =>
        {
            entity.ToTable("furnitureProducts", tb => tb.HasComment("家具商品資料表"));

            entity.HasIndex(e => new { e.Status, e.CategoryId }, "IX_furnProducts_status_cat");

            entity.Property(e => e.FurnitureProductId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("商品ID")
                .HasColumnName("furnitureProductId");
            entity.Property(e => e.CategoryId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("分類ID")
                .HasColumnName("categoryId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DailyRental)
                .HasComment("每日租金")
                .HasColumnType("decimal(10, 0)")
                .HasColumnName("dailyRental");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.DelistedAt)
                .HasComment("下架時間")
                .HasColumnName("delistedAt");
            entity.Property(e => e.Description)
                .HasComment("商品描述")
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasComment("商品圖片URL")
                .HasColumnName("imageUrl");
            entity.Property(e => e.ListPrice)
                .HasComment("原價")
                .HasColumnType("decimal(10, 0)")
                .HasColumnName("listPrice");
            entity.Property(e => e.ListedAt)
                .HasComment("上架時間")
                .HasColumnName("listedAt");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasComment("商品名稱")
                .HasColumnName("productName");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasComment("上下架狀態")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Category).WithMany(p => p.FurnitureProducts)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_furnitureProducts_category");
        });

        modelBuilder.Entity<FurnitureRentalContract>(entity =>
        {
            entity.HasKey(e => e.FurnitureRentalContractsId);

            entity.ToTable("furnitureRentalContracts", tb => tb.HasComment("家具租賃合約表"));

            entity.Property(e => e.FurnitureRentalContractsId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("合約ID")
                .HasColumnName("furnitureRentalContractsId");
            entity.Property(e => e.ContractJson)
                .HasComment("合約 JSON")
                .HasColumnName("contractJson");
            entity.Property(e => e.ContractLink)
                .HasMaxLength(255)
                .HasComment("簽章連結")
                .HasColumnName("contractLink");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("簽約日期")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeliveryDate)
                .HasComment("配送日期")
                .HasColumnName("deliveryDate");
            entity.Property(e => e.ESignatureValue)
                .HasMaxLength(255)
                .HasComment("電子簽章值")
                .HasColumnName("eSignatureValue");
            entity.Property(e => e.OrderId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("訂單ID")
                .HasColumnName("orderId");
            entity.Property(e => e.SignStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("簽署狀態")
                .HasColumnName("signStatus");
            entity.Property(e => e.SignedAt)
                .HasPrecision(0)
                .HasComment("簽署完成時間")
                .HasColumnName("signedAt");
            entity.Property(e => e.TerminationPolicy)
                .HasComment("退租政策")
                .HasColumnName("terminationPolicy");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Order).WithMany(p => p.FurnitureRentalContracts)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureRentalContracts_order");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("pk_images");

            entity.ToTable("images");

            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.Category, e.DisplayOrder, e.IsActive }, "ix_images_entity");

            entity.HasIndex(e => e.ImageGuid, "uq_images_imageGuid").IsUnique();

            entity.Property(e => e.ImageId).HasColumnName("imageId");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category")
                .HasConversion<string>();
            entity.Property(e => e.DisplayOrder).HasColumnName("displayOrder");
            entity.Property(e => e.EntityId).HasColumnName("entityId");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .HasColumnName("entityType")
                .HasConversion<string>();
            entity.Property(e => e.FileSizeBytes).HasColumnName("fileSizeBytes");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.ImageGuid)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("imageGuid");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.MimeType)
                .HasMaxLength(50)
                .HasColumnName("mimeType");
            entity.Property(e => e.OriginalFileName)
                .HasMaxLength(255)
                .HasColumnName("originalFileName");
            entity.Property(e => e.StoredFileName)
                .HasMaxLength(41)
                .IsUnicode(false)
                .HasComputedColumnSql("(lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end)", true)
                .HasColumnName("storedFileName");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(DATEADD(HOUR, 8, sysutcdatetime()))")
                .HasColumnName("uploadedAt");
            entity.Property(e => e.UploadedByMemberId).HasColumnName("uploadedByMemberId");
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.RowVersion)
                .HasColumnName("rowVersion")
                .IsRowVersion()
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate()
                .Metadata.SetBeforeSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            entity.HasOne(d => d.UploadedByMember).WithMany(p => p.Images)
                .HasForeignKey(d => d.UploadedByMemberId)
                .HasConstraintName("fk_images_members");
        });

        modelBuilder.Entity<InventoryEvent>(entity =>
        {
            entity.HasKey(e => e.FurnitureInventoryId);

            entity.ToTable("inventoryEvents", tb => tb.HasComment("庫存事件表"));

            entity.Property(e => e.FurnitureInventoryId)
                .HasDefaultValueSql("(newid())")
                .HasComment("事件ID")
                .HasColumnName("furnitureInventoryId");
            entity.Property(e => e.EventType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("事件類型")
                .HasColumnName("eventType");
            entity.Property(e => e.OccurredAt)
                .HasPrecision(0)
                .HasComment("發生時間")
                .HasColumnName("occurredAt");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("商品ID")
                .HasColumnName("productId");
            entity.Property(e => e.Quantity)
                .HasComment("異動數量")
                .HasColumnName("quantity");
            entity.Property(e => e.RecordedAt)
                .HasPrecision(0)
                .HasComment("寫入時間")
                .HasColumnName("recordedAt");
            entity.Property(e => e.SourceId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("來源編號")
                .HasColumnName("sourceId");
            entity.Property(e => e.SourceType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("來源類型")
                .HasColumnName("sourceType");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryEvents)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_inventoryEvents_product");
        });

        modelBuilder.Entity<ListingPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId);

            entity.ToTable("listingPlans", tb => tb.HasComment("刊登費方案表"));

            entity.Property(e => e.PlanId)
                .HasComment("刊登費方案ID")
                .HasColumnName("planId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasComment("幣別")
                .HasColumnName("currencyCode");
            entity.Property(e => e.EndAt)
                .HasPrecision(0)
                .HasComment("結束時間")
                .HasColumnName("endAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.MinListingDays)
                .HasComment("最小上架天數")
                .HasColumnName("minListingDays");
            entity.Property(e => e.PlanName)
                .HasMaxLength(50)
                .HasComment("方案名稱")
                .HasColumnName("planName");
            entity.Property(e => e.PricePerDay)
                .HasComment("每日刊登費")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("pricePerDay");
            entity.Property(e => e.StartAt)
                .HasPrecision(0)
                .HasComment("生效時間")
                .HasColumnName("startAt");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("members", tb => tb.HasComment("會員資料表"));

            entity.HasIndex(e => e.Email, "IX_members_email");

            entity.HasIndex(e => e.PhoneNumber, "IX_members_phone");

            entity.Property(e => e.MemberId)
                .HasDefaultValueSql("(NEXT VALUE FOR [dbo].[seq_memberID])")
                .HasComment("會員ID")
                .HasColumnName("memberID");
            entity.Property(e => e.AddressLine)
                .HasMaxLength(200)
                .HasComment("詳細地址")
                .HasColumnName("addressLine");
            entity.Property(e => e.BirthDate)
                .HasComment("生日")
                .HasColumnName("birthDate");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasComment("電子信箱")
                .HasColumnName("email");
            entity.Property(e => e.EmailVerifiedAt)
                .HasPrecision(0)
                .HasComment("Email驗證通過時間")
                .HasColumnName("emailVerifiedAt");
            entity.Property(e => e.Gender)
                .HasComment("性別")
                .HasColumnName("gender");
            entity.Property(e => e.IdentityVerifiedAt)
                .HasPrecision(0)
                .HasComment("身份驗證通過時間")
                .HasColumnName("identityVerifiedAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.IsLandlord)
                .HasComment("是否為房東")
                .HasColumnName("isLandlord");
            entity.Property(e => e.LastLoginAt)
                .HasPrecision(0)
                .HasComment("最後登入時間")
                .HasColumnName("lastLoginAt");
            entity.Property(e => e.MemberName)
                .HasMaxLength(50)
                .HasComment("會員姓名")
                .HasColumnName("memberName");
            entity.Property(e => e.MemberTypeId)
                .HasComment("會員身份別ID")
                .HasColumnName("memberTypeID");
            entity.Property(e => e.NationalIdNo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasComment("身分證號")
                .HasColumnName("nationalIdNo");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasComment("密碼雜湊")
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasComment("手機號碼")
                .HasColumnName("phoneNumber");
            entity.Property(e => e.PhoneVerifiedAt)
                .HasPrecision(0)
                .HasComment("手機驗證通過時間")
                .HasColumnName("phoneVerifiedAt");
            entity.Property(e => e.PrimaryRentalCityId)
                .HasComment("主要承租縣市ID")
                .HasColumnName("primaryRentalCityID");
            entity.Property(e => e.PrimaryRentalDistrictId)
                .HasComment("主要承租區域ID")
                .HasColumnName("primaryRentalDistrictID");
            entity.Property(e => e.ResidenceCityId)
                .HasComment("居住縣市ID")
                .HasColumnName("residenceCityID");
            entity.Property(e => e.ResidenceDistrictId)
                .HasComment("居住區域ID")
                .HasColumnName("residenceDistrictID");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.MemberType).WithMany(p => p.Members)
                .HasForeignKey(d => d.MemberTypeId)
                .HasConstraintName("FK_members_memberType");

            entity.HasOne(d => d.PrimaryRentalCity).WithMany(p => p.MemberPrimaryRentalCities)
                .HasForeignKey(d => d.PrimaryRentalCityId)
                .HasConstraintName("FK_members_primaryCity");

            entity.HasOne(d => d.PrimaryRentalDistrict).WithMany(p => p.MemberPrimaryRentalDistricts)
                .HasForeignKey(d => d.PrimaryRentalDistrictId)
                .HasConstraintName("FK_members_primaryDistrict");

            entity.HasOne(d => d.ResidenceCity).WithMany(p => p.MemberResidenceCities)
                .HasForeignKey(d => d.ResidenceCityId)
                .HasConstraintName("FK_members_resCity");

            entity.HasOne(d => d.ResidenceDistrict).WithMany(p => p.MemberResidenceDistricts)
                .HasForeignKey(d => d.ResidenceDistrictId)
                .HasConstraintName("FK_members_resDistrict");
        });

        modelBuilder.Entity<MemberType>(entity =>
        {
            entity.ToTable("memberTypes", tb => tb.HasComment("會員身分表"));

            entity.Property(e => e.MemberTypeId)
                .HasComment("身份ID")
                .HasColumnName("memberTypeID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasComment("描述")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.TypeName)
                .HasMaxLength(30)
                .HasComment("身分名稱")
                .HasColumnName("typeName");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<MemberVerification>(entity =>
        {
            entity.HasKey(e => e.VerificationId);

            entity.ToTable("memberVerifications", tb => tb.HasComment("通訊相關驗證表"));

            entity.Property(e => e.VerificationId)
                .HasComment("驗證ID")
                .HasColumnName("verificationID");
            entity.Property(e => e.IsSuccessful)
                .HasComment("是否驗證成功")
                .HasColumnName("isSuccessful");
            entity.Property(e => e.MemberId)
                .HasComment("會員ID")
                .HasColumnName("memberID");
            entity.Property(e => e.SentAt)
                .HasPrecision(0)
                .HasComment("發送時間")
                .HasColumnName("sentAt");
            entity.Property(e => e.VerificationCode)
                .HasMaxLength(10)
                .HasComment("驗證碼")
                .HasColumnName("verificationCode");
            entity.Property(e => e.VerificationTypeCode)
                .HasMaxLength(20)
                .HasComment("驗證類型代碼")
                .HasColumnName("verificationTypeCode");
            entity.Property(e => e.VerifiedAt)
                .HasPrecision(0)
                .HasComment("驗證完成時間")
                .HasColumnName("verifiedAt");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberVerifications)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_memberVerifications_member");
        });

        modelBuilder.Entity<MessagePlacement>(entity =>
        {
            entity.HasKey(e => new { e.PageCode, e.SectionCode });

            entity.ToTable("messagePlacements", tb => tb.HasComment("訊息位置"));

            entity.Property(e => e.PageCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("頁面識別碼")
                .HasColumnName("pageCode");
            entity.Property(e => e.SectionCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("區段代碼")
                .HasColumnName("sectionCode");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DisplayOrder)
                .HasComment("顯示順序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.MessageId)
                .HasComment("訊息ID")
                .HasColumnName("messageID");
            entity.Property(e => e.Subtitle)
                .HasMaxLength(100)
                .HasComment("小標題")
                .HasColumnName("subtitle");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Message).WithMany(p => p.MessagePlacements)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK_msgPlacements_message");
        });

        modelBuilder.Entity<OrderEvent>(entity =>
        {
            entity.HasKey(e => e.OrderEventsId);

            entity.ToTable("orderEvents", tb => tb.HasComment("訂單事件表"));

            entity.Property(e => e.OrderEventsId)
                .HasDefaultValueSql("(newid())")
                .HasComment("事件ID")
                .HasColumnName("orderEventsId");
            entity.Property(e => e.EventType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasComment("事件類型")
                .HasColumnName("eventType");
            entity.Property(e => e.OccurredAt)
                .HasPrecision(0)
                .HasComment("發生時間")
                .HasColumnName("occurredAt");
            entity.Property(e => e.OrderId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("訂單ID")
                .HasColumnName("orderId");
            entity.Property(e => e.Payload)
                .HasComment("事件內容")
                .HasColumnName("payload");
            entity.Property(e => e.RecordedAt)
                .HasPrecision(0)
                .HasComment("寫入時間")
                .HasColumnName("recordedAt");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderEvents)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orderEvents_order");
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.PageCode);

            entity.ToTable("pages", tb => tb.HasComment("共用頁面代碼表"));

            entity.Property(e => e.PageCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("頁面識別碼")
                .HasColumnName("pageCode");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DisplayOrder)
                .HasComment("顯示順序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.ModuleScope)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("模組")
                .HasColumnName("moduleScope");
            entity.Property(e => e.PageName)
                .HasMaxLength(50)
                .HasComment("頁面名稱")
                .HasColumnName("pageName");
            entity.Property(e => e.RoutePath)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasComment("路徑")
                .HasColumnName("routePath");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("properties", tb =>
                {
                    tb.HasComment("房源資料表");

                    tb.HasTrigger("trg_properties_status_protection");

                    tb.HasTrigger("trg_properties_validate_landlord");
                });

            entity.HasIndex(e => new { e.CityId, e.DistrictId }, "IX_properties_location");

            entity.HasIndex(e => e.StatusCode, "IX_properties_status");

            entity.Property(e => e.PropertyId)
                .ValueGeneratedOnAdd()
                .HasComment("房源ID")
                .HasColumnName("propertyID");
            entity.Property(e => e.AddressLine)
                .HasMaxLength(200)
                .HasComment("詳細地址")
                .HasColumnName("addressLine");
            entity.Property(e => e.Area)
                .HasComment("坪數")
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("area");
            entity.Property(e => e.BathroomCount)
                .HasComment("衛數")
                .HasColumnName("bathroomCount");
            entity.Property(e => e.CityId)
                .HasComment("縣市ID")
                .HasColumnName("cityID");
            entity.Property(e => e.CleaningFeeAmount)
                .HasComment("清潔費金額")
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("cleaningFeeAmount");
            entity.Property(e => e.CleaningFeeRequired)
                .HasComment("清潔費需額外收費")
                .HasColumnName("cleaningFeeRequired");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrentFloor)
                .HasComment("所在樓層")
                .HasColumnName("currentFloor");
            entity.Property(e => e.CustomElectricityFee)
                .HasComment("自訂電費")
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("customElectricityFee");
            entity.Property(e => e.CustomWaterFee)
                .HasComment("自訂水費")
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("customWaterFee");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.DepositAmount)
                .HasComment("押金金額")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("depositAmount");
            entity.Property(e => e.DepositMonths)
                .HasComment("押金月數")
                .HasColumnName("depositMonths");
            entity.Property(e => e.Description)
                .HasComment("詳細描述")
                .HasColumnName("description");
            entity.Property(e => e.DistrictId)
                .HasComment("區域ID")
                .HasColumnName("districtID");
            entity.Property(e => e.ElectricityFeeType)
                .HasMaxLength(20)
                .HasComment("電費計算方式")
                .HasColumnName("electricityFeeType");
            entity.Property(e => e.ExpireAt)
                .HasPrecision(0)
                .HasComment("上架到期時間")
                .HasColumnName("expireAt");
            entity.Property(e => e.IsPaid)
                .HasComment("付款狀態")
                .HasColumnName("isPaid");
            entity.Property(e => e.LandlordMemberId)
                .HasComment("房東會員ID")
                .HasColumnName("landlordMemberID");
            entity.Property(e => e.ListingDays)
                .HasComment("刊登天數")
                .HasColumnName("listingDays");
            entity.Property(e => e.ListingFeeAmount)
                .HasComment("刊登費用")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("listingFeeAmount");
            entity.Property(e => e.ListingPlanId)
                .HasComment("刊登費方案ID")
                .HasColumnName("listingPlanID");
            entity.Property(e => e.LivingRoomCount)
                .HasComment("廳數")
                .HasColumnName("livingRoomCount");
            entity.Property(e => e.ManagementFeeAmount)
                .HasComment("管理費金額")
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("managementFeeAmount");
            entity.Property(e => e.ManagementFeeIncluded)
                .HasComment("管理費含租金")
                .HasColumnName("managementFeeIncluded");
            entity.Property(e => e.MinimumRentalMonths)
                .HasComment("最短租期(月)")
                .HasColumnName("minimumRentalMonths");
            entity.Property(e => e.MonthlyRent)
                .HasComment("月租金")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monthlyRent");
            entity.Property(e => e.PaidAt)
                .HasPrecision(0)
                .HasComment("完成付款時間")
                .HasColumnName("paidAt");
            entity.Property(e => e.ParkingAvailable)
                .HasComment("有停車位")
                .HasColumnName("parkingAvailable");
            entity.Property(e => e.ParkingFeeAmount)
                .HasComment("停車位費用")
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("parkingFeeAmount");
            entity.Property(e => e.ParkingFeeRequired)
                .HasComment("停車費需額外收費")
                .HasColumnName("parkingFeeRequired");
            entity.Property(e => e.PreviewImageUrl)
                .HasMaxLength(500)
                .HasComment("預覽圖連結")
                .HasColumnName("previewImageURL");
            entity.Property(e => e.PropertyProofUrl)
                .HasMaxLength(500)
                .HasComment("房產證明文件URL")
                .HasColumnName("propertyProofURL");
            entity.Property(e => e.PublishedAt)
                .HasPrecision(0)
                .HasComment("上架日期")
                .HasColumnName("publishedAt");
            entity.Property(e => e.RoomCount)
                .HasComment("房數")
                .HasColumnName("roomCount");
            entity.Property(e => e.SpecialRules)
                .HasComment("特殊守則")
                .HasColumnName("specialRules");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(20)
                .HasComment("房源狀態代碼")
                .HasColumnName("statusCode");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasComment("房源標題")
                .HasColumnName("title");
            entity.Property(e => e.TotalFloors)
                .HasComment("總樓層")
                .HasColumnName("totalFloors");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("最後修改日期")
                .HasColumnName("updatedAt");
            entity.Property(e => e.WaterFeeType)
                .HasMaxLength(20)
                .HasComment("水費計算方式")
                .HasColumnName("waterFeeType");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 8)
                .HasComment("緯度")
                .HasColumnName("Latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(11, 8)
                .HasComment("經度")
                .HasColumnName("Longitude");

            entity.HasOne(d => d.LandlordMember).WithMany(p => p.Properties)
                .HasForeignKey(d => d.LandlordMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_properties_landlord");

            entity.HasOne(d => d.ListingPlan).WithMany(p => p.Properties)
                .HasForeignKey(d => d.ListingPlanId)
                .HasConstraintName("FK_properties_listingPlan");

            entity.HasOne(d => d.City).WithMany(p => p.Properties)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_properties_city");

            entity.HasOne(d => d.District).WithMany(p => p.Properties)
                .HasForeignKey(d => d.DistrictId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_properties_district");
        });

        modelBuilder.Entity<PropertyComplaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId);

            entity.ToTable("propertyComplaints", tb => tb.HasComment("房源投訴表"));

            entity.HasIndex(e => e.ComplainantId, "IX_propertyComplaints_complainantId");

            entity.HasIndex(e => e.CreatedAt, "IX_propertyComplaints_createdAt");

            entity.HasIndex(e => e.HandledBy, "IX_propertyComplaints_handledBy");

            entity.HasIndex(e => e.LandlordId, "IX_propertyComplaints_landlordId");

            entity.HasIndex(e => e.PropertyId, "IX_propertyComplaints_propertyId");

            entity.HasIndex(e => e.ResolvedAt, "IX_propertyComplaints_resolvedAt");

            entity.HasIndex(e => e.StatusCode, "IX_propertyComplaints_statusCode");

            entity.Property(e => e.ComplaintId)
                .HasComment("投訴ID (自動遞增，從301開始)")
                .HasColumnName("complaintId");
            entity.Property(e => e.ComplainantId)
                .HasComment("投訴人ID")
                .HasColumnName("complainantId");
            entity.Property(e => e.ComplaintContent)
                .HasComment("投訴內容")
                .HasColumnName("complaintContent");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.HandledBy)
                .HasComment("處理人員ID")
                .HasColumnName("handledBy");
            entity.Property(e => e.InternalNote)
                .HasComment("內部註記")
                .HasColumnName("internalNote");
            entity.Property(e => e.LandlordId)
                .HasComment("被投訴房東ID")
                .HasColumnName("landlordId");
            entity.Property(e => e.PropertyId)
                .HasComment("房源ID")
                .HasColumnName("propertyId");
            entity.Property(e => e.ResolvedAt)
                .HasPrecision(0)
                .HasComment("結案時間")
                .HasColumnName("resolvedAt");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("處理狀態代碼")
                .HasColumnName("statusCode");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Complainant).WithMany(p => p.PropertyComplaintComplainants)
                .HasForeignKey(d => d.ComplainantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propComplaints_complainant");

            entity.HasOne(d => d.Landlord).WithMany(p => p.PropertyComplaintLandlords)
                .HasForeignKey(d => d.LandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propComplaints_landlord");

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyComplaints)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propComplaints_property");
        });

        modelBuilder.Entity<PropertyEquipmentCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.ToTable("propertyEquipmentCategories", tb => tb.HasComment("房源設備分類資料表"));

            entity.Property(e => e.CategoryId).HasColumnName("categoryID");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasComment("設備名稱")
                .HasColumnName("categoryName");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.ParentCategoryId)
                .HasComment("上層分類ID")
                .HasColumnName("parentCategoryID");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_propEquipCate_parent");
        });

        modelBuilder.Entity<PropertyEquipmentRelation>(entity =>
        {
            entity.HasKey(e => e.RelationId);

            entity.ToTable("propertyEquipmentRelations", tb => tb.HasComment("房源設備關聯資料表"));

            entity.Property(e => e.RelationId)
                .ValueGeneratedNever()
                .HasComment("關聯ID")
                .HasColumnName("relationID");
            entity.Property(e => e.CategoryId)
                .HasComment("設備分類ID")
                .HasColumnName("categoryID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.PropertyId)
                .HasComment("房源ID")
                .HasColumnName("propertyID");
            entity.Property(e => e.Quantity)
                .HasComment("數量")
                .HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Category).WithMany(p => p.PropertyEquipmentRelations)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propEquipRel_category");

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyEquipmentRelations)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propEquipRel_property");
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.ToTable("propertyImages", tb => tb.HasComment("房源圖片表"));

            entity.HasIndex(e => new { e.PropertyId, e.DisplayOrder }, "UQ_propertyImages_property_order").IsUnique();

            entity.Property(e => e.ImageId)
                .HasComment("圖片ID")
                .HasColumnName("imageID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DisplayOrder)
                .HasComment("顯示順序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasComment("圖片路徑")
                .HasColumnName("imagePath");
            entity.Property(e => e.PropertyId)
                .HasComment("房源ID")
                .HasColumnName("propertyID");

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyImages)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propertyImages_property");
        });

        modelBuilder.Entity<RentalApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId);

            entity.ToTable("rentalApplications", tb => tb.HasComment("租賃/看房申請資料表"));

            entity.Property(e => e.ApplicationId)
                .HasComment("申請ID")
                .HasColumnName("applicationID");
            entity.Property(e => e.ApplicationType)
                .HasMaxLength(20)
                .HasComment("申請類型")
                .HasColumnName("applicationType");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("申請時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrentStatus)
                .HasMaxLength(20)
                .HasComment("目前狀態")
                .HasColumnName("currentStatus");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.HouseholdAddress)
                .HasMaxLength(100)
                .HasComment("戶籍地址")
                .HasColumnName("householdAddress");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否有效")
                .HasColumnName("isActive");
            entity.Property(e => e.MemberId)
                .HasComment("申請會員ID")
                .HasColumnName("memberID");
            entity.Property(e => e.Message)
                .HasMaxLength(300)
                .HasComment("申請留言")
                .HasColumnName("message");
            entity.Property(e => e.PropertyId)
                .HasComment("房源ID")
                .HasColumnName("propertyID");
            entity.Property(e => e.RentalEndDate)
                .HasComment("租期結束")
                .HasColumnName("rentalEndDate");
            entity.Property(e => e.RentalStartDate)
                .HasComment("租期開始")
                .HasColumnName("rentalStartDate");
            entity.Property(e => e.ScheduleTime)
                .HasPrecision(0)
                .HasComment("預約看房時間")
                .HasColumnName("scheduleTime");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Member).WithMany(p => p.RentalApplications)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rentalApplications_member");

            entity.HasOne(d => d.Property).WithMany(p => p.RentalApplications)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rentalApplications_property");
        });

        modelBuilder.Entity<RenterPost>(entity =>
        {
            entity.HasKey(e => e.PostId);

            entity.ToTable("renterPosts", tb => tb.HasComment("尋租文章資料表"));

            entity.HasIndex(e => new { e.CityId, e.DistrictId, e.HouseType }, "IX_renterPosts_location");

            entity.Property(e => e.PostId)
                .ValueGeneratedNever()
                .HasComment("文章ID")
                .HasColumnName("postID");
            entity.Property(e => e.BudgetMax)
                .HasComment("預算上限")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("budgetMax");
            entity.Property(e => e.BudgetMin)
                .HasComment("預算下限")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("budgetMin");
            entity.Property(e => e.CityId)
                .HasComment("希望縣市ID")
                .HasColumnName("cityID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("發布時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.DistrictId)
                .HasComment("希望區域ID")
                .HasColumnName("districtID");
            entity.Property(e => e.HouseType)
                .HasMaxLength(20)
                .HasComment("房屋類型")
                .HasColumnName("houseType");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否有效")
                .HasColumnName("isActive");
            entity.Property(e => e.MemberId)
                .HasComment("租客會員ID")
                .HasColumnName("memberID");
            entity.Property(e => e.PostContent)
                .HasComment("詳細需求")
                .HasColumnName("postContent");
            entity.Property(e => e.ReplyCount)
                .HasComment("回覆數")
                .HasColumnName("replyCount");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("最後編輯時間")
                .HasColumnName("updatedAt");
            entity.Property(e => e.ViewCount)
                .HasComment("瀏覽數")
                .HasColumnName("viewCount");

            entity.HasOne(d => d.Member).WithMany(p => p.RenterPosts)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_renterPosts_member");
        });

        modelBuilder.Entity<RenterPostReply>(entity =>
        {
            entity.HasKey(e => e.ReplyId);

            entity.ToTable("renterPostReplies", tb => tb.HasComment("尋租文章回覆資料表"));

            entity.Property(e => e.ReplyId)
                .ValueGeneratedNever()
                .HasComment("回覆ID")
                .HasColumnName("replyID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("回覆時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsWithinBudget)
                .HasDefaultValue(true)
                .HasComment("符合預算")
                .HasColumnName("isWithinBudget");
            entity.Property(e => e.LandlordMemberId)
                .HasComment("房東會員ID")
                .HasColumnName("landlordMemberID");
            entity.Property(e => e.PostId)
                .HasComment("文章ID")
                .HasColumnName("postID");
            entity.Property(e => e.ReplyContent)
                .HasComment("回覆內容")
                .HasColumnName("replyContent");
            entity.Property(e => e.SuggestPropertyId)
                .HasComment("推薦房源ID")
                .HasColumnName("suggestPropertyID");
            entity.Property(e => e.Tags)
                .HasMaxLength(100)
                .HasComment("符合條件標籤")
                .HasColumnName("tags");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.LandlordMember).WithMany(p => p.RenterPostReplies)
                .HasForeignKey(d => d.LandlordMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_renterPostReplies_landlord");

            entity.HasOne(d => d.Post).WithMany(p => p.RenterPostReplies)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_renterPostReplies_post");
        });

        modelBuilder.Entity<RenterRequirementList>(entity =>
        {
            entity.HasKey(e => e.RequirementId);

            entity.ToTable("renterRequirementList", tb => tb.HasComment("尋租文章需求條件清單"));

            entity.Property(e => e.RequirementId)
                .ValueGeneratedNever()
                .HasComment("條件ID")
                .HasColumnName("requirementID");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.RequirementName)
                .HasMaxLength(100)
                .HasComment("條件名稱")
                .HasColumnName("requirementName");
        });

        modelBuilder.Entity<RenterRequirementRelation>(entity =>
        {
            entity.HasKey(e => e.RelationId);

            entity.ToTable("renterRequirementRelations", tb => tb.HasComment("尋租條件關聯"));

            entity.HasIndex(e => new { e.PostId, e.RequirementId }, "UQ_rentReqRel_post_req").IsUnique();

            entity.Property(e => e.RelationId)
                .ValueGeneratedNever()
                .HasComment("關聯ID")
                .HasColumnName("relationID");
            entity.Property(e => e.PostId)
                .HasComment("文章ID")
                .HasColumnName("postID");
            entity.Property(e => e.RequirementId)
                .HasComment("條件ID")
                .HasColumnName("requirementID");

            entity.HasOne(d => d.Post).WithMany(p => p.RenterRequirementRelations)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rentReqRel_post");

            entity.HasOne(d => d.Requirement).WithMany(p => p.RenterRequirementRelations)
                .HasForeignKey(d => d.RequirementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rentReqRel_requirement");
        });

        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId);

            entity.ToTable("searchHistory", tb => tb.HasComment("搜索歷史表"));

            entity.HasIndex(e => new { e.MemberId, e.SearchedAt }, "IX_searchHistory_member_time");

            entity.Property(e => e.HistoryId)
                .ValueGeneratedNever()
                .HasComment("歷史ID")
                .HasColumnName("historyID");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasComment("裝置")
                .HasColumnName("deviceType");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasComment("IP")
                .HasColumnName("ipAddress");
            entity.Property(e => e.Keyword)
                .HasMaxLength(255)
                .HasComment("搜尋關鍵字")
                .HasColumnName("keyword");
            entity.Property(e => e.MemberId)
                .HasComment("會員ID")
                .HasColumnName("memberID");
            entity.Property(e => e.ResultCount)
                .HasComment("結果筆數")
                .HasColumnName("resultCount");
            entity.Property(e => e.SearchedAt)
                .HasPrecision(0)
                .HasComment("搜尋時間")
                .HasColumnName("searchedAt");

            entity.HasOne(d => d.Member).WithMany(p => p.SearchHistories)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_searchHistory_member");
        });

        modelBuilder.Entity<SiteMessage>(entity =>
        {
            entity.HasKey(e => e.SiteMessagesId);

            entity.ToTable("siteMessages", tb => tb.HasComment("網站訊息表"));

            entity.Property(e => e.SiteMessagesId)
                .ValueGeneratedNever()
                .HasComment("訊息ID")
                .HasColumnName("siteMessagesId")
                .ValueGeneratedOnAdd(); // <-- 強制告訴 EF：這是 Identity 欄位
            entity.Property(e => e.AttachmentUrl)
                .HasMaxLength(255)
                .HasComment("圖片/附件URL")
                .HasColumnName("attachmentUrl");
            entity.Property(e => e.Category)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasComment("分類")
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.DisplayOrder)
                .HasComment("顯示順序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.EndAt)
                .HasPrecision(0)
                .HasComment("結束時間")
                .HasColumnName("endAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.MessageType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("訊息類型")
                .HasColumnName("messageType");
            entity.Property(e => e.ModuleScope)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasComment("模組")
                .HasColumnName("moduleScope");
            entity.Property(e => e.SiteMessageContent)
                .HasMaxLength(500)
                .HasComment("內文")
                .HasColumnName("siteMessageContent");
            entity.Property(e => e.StartAt)
                .HasPrecision(0)
                .HasComment("開始時間")
                .HasColumnName("startAt");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasComment("標題")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<SystemCode>(entity =>
        {
            entity.HasKey(e => new { e.CodeCategory, e.Code });

            entity.ToTable("systemCodes", tb => tb.HasComment("代碼總表"));

            entity.Property(e => e.CodeCategory)
                .HasMaxLength(20)
                .HasComment("代碼類別")
                .HasColumnName("codeCategory");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasComment("代碼")
                .HasColumnName("code");
            entity.Property(e => e.CodeName)
                .HasMaxLength(50)
                .HasComment("代碼名稱")
                .HasColumnName("codeName");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DisplayOrder)
                .HasComment("排序")
                .HasColumnName("displayOrder");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.CodeCategoryNavigation).WithMany(p => p.SystemCodes)
                .HasForeignKey(d => d.CodeCategory)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_systemCodes_category");
        });

        modelBuilder.Entity<SystemCodeCategory>(entity =>
        {
            entity.HasKey(e => e.CodeCategory);

            entity.ToTable("systemCodeCategories", tb => tb.HasComment("代碼類別表"));

            entity.Property(e => e.CodeCategory)
                .HasMaxLength(20)
                .HasComment("代碼類別")
                .HasColumnName("codeCategory");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasComment("類別名稱")
                .HasColumnName("categoryName");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasComment("描述")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<SystemMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);

            entity.ToTable("systemMessages", tb => tb.HasComment("系統訊息表"));

            entity.HasIndex(e => new { e.IsActive, e.CategoryCode }, "IX_systemMessages_active_category").HasFillFactor(100);

            entity.HasIndex(e => e.AdminId, "IX_systemMessages_adminID");

            entity.HasIndex(e => e.AudienceTypeCode, "IX_systemMessages_audienceTypeCode");

            entity.HasIndex(e => e.DeletedAt, "IX_systemMessages_deletedAt");

            entity.HasIndex(e => e.ReceiverId, "IX_systemMessages_receiverID");

            entity.HasIndex(e => e.SentAt, "IX_systemMessages_sentAt");

            entity.Property(e => e.MessageId)
                .HasComment("系統訊息ID (自動遞增，從401開始)")
                .HasColumnName("messageID");
            entity.Property(e => e.AdminId)
                .HasComment("發送管理員ID")
                .HasColumnName("adminID");
            entity.Property(e => e.AttachmentUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasComment("附件URL")
                .HasColumnName("attachmentUrl");
            entity.Property(e => e.AudienceTypeCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("接受者類型代碼")
                .HasColumnName("audienceTypeCode");
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("訊息類別代碼")
                .HasColumnName("categoryCode");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否啟用")
                .HasColumnName("isActive");
            entity.Property(e => e.MessageContent)
                .HasComment("發送內容")
                .HasColumnName("messageContent");
            entity.Property(e => e.ReceiverId)
                .HasComment("個別接受者ID")
                .HasColumnName("receiverID");
            entity.Property(e => e.SentAt)
                .HasPrecision(0)
                .HasComment("發送時間")
                .HasColumnName("sentAt");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasComment("發送標題")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Admin).WithMany(p => p.SystemMessages)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_systemMessages_admin");

            entity.HasOne(d => d.Receiver).WithMany(p => p.SystemMessages)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK_systemMessages_receiver");
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);

            entity.ToTable("userNotifications", tb => tb.HasComment("使用者通知資料表"));

            entity.HasIndex(e => new { e.ReceiverId, e.IsRead }, "IX_userNotifications_receiver_isRead");

            entity.Property(e => e.NotificationId)
                .HasComment("通知ID")
                .HasColumnName("notificationID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.IsRead)
                .HasComment("是否已讀")
                .HasColumnName("isRead");
            entity.Property(e => e.LinkUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasComment("相關連結URL")
                .HasColumnName("linkUrl");
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("來源模組代碼")
                .HasColumnName("moduleCode");
            entity.Property(e => e.NotificationContent)
                .HasComment("訊息內容")
                .HasColumnName("notificationContent");
            entity.Property(e => e.ReadAt)
                .HasPrecision(0)
                .HasComment("閱讀時間")
                .HasColumnName("readAt");
            entity.Property(e => e.ReceiverId)
                .HasComment("接收者ID")
                .HasColumnName("receiverID");
            entity.Property(e => e.SenderId)
                .HasComment("發送者ID")
                .HasColumnName("senderID");
            entity.Property(e => e.SentAt)
                .HasPrecision(0)
                .HasComment("發送時間")
                .HasColumnName("sentAt");
            entity.Property(e => e.SourceEntityId)
                .HasComment("來源資料ID")
                .HasColumnName("sourceEntityID");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("狀態代碼")
                .HasColumnName("statusCode");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasComment("訊息標題")
                .HasColumnName("title");
            entity.Property(e => e.TypeCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("通知類型代碼")
                .HasColumnName("typeCode");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Receiver).WithMany(p => p.UserNotifications)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_userNotifications_receiver");
        });

        modelBuilder.Entity<UserUpload>(entity =>
        {
            entity.HasKey(e => e.UploadId);

            entity.ToTable("userUploads", tb => tb.HasComment("會員上傳資料紀錄表"));

            entity.Property(e => e.UploadId)
                .HasComment("上傳ID")
                .HasColumnName("uploadID");
            entity.Property(e => e.ApprovalId)
                .HasComment("審核ID")
                .HasColumnName("approvalID");
            entity.Property(e => e.Checksum)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasComment("檔案雜湊")
                .HasColumnName("checksum");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("建立時間")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(0)
                .HasComment("刪除時間")
                .HasColumnName("deletedAt");
            entity.Property(e => e.FileExt)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasComment("副檔名")
                .HasColumnName("fileExt");
            entity.Property(e => e.FilePath)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasComment("檔案路徑")
                .HasColumnName("filePath");
            entity.Property(e => e.FileSize)
                .HasComment("檔案大小(Byte)")
                .HasColumnName("fileSize");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("是否有效")
                .HasColumnName("isActive");
            entity.Property(e => e.MemberId)
                .HasComment("會員ID")
                .HasColumnName("memberID");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasComment("MIME 類型")
                .HasColumnName("mimeType");
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("來源模組代碼")
                .HasColumnName("moduleCode");
            entity.Property(e => e.OriginalFileName)
                .HasMaxLength(255)
                .HasComment("原始檔名")
                .HasColumnName("originalFileName");
            entity.Property(e => e.SourceEntityId)
                .HasComment("來源資料ID")
                .HasColumnName("sourceEntityID");
            entity.Property(e => e.StoredFileName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasComment("實體檔名")
                .HasColumnName("storedFileName");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(CONVERT([datetime2](0),sysdatetime()))")
                .HasComment("更新時間")
                .HasColumnName("updatedAt");
            entity.Property(e => e.UploadTypeCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("上傳種類代碼")
                .HasColumnName("uploadTypeCode");
            entity.Property(e => e.UploadedAt)
                .HasPrecision(0)
                .HasComment("上傳時間")
                .HasColumnName("uploadedAt");

            entity.HasOne(d => d.Member).WithMany(p => p.UserUploads)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_userUploads_member");
        });
        modelBuilder.HasSequence<int>("seq_memberID");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
