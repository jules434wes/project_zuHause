using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

public partial class TempDbContext : DbContext
{
    public TempDbContext()
    {
    }

    public TempDbContext(DbContextOptions<TempDbContext> options)
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
        => optionsBuilder.UseSqlServer("Server=tcp:zuhause.database.windows.net,1433;Initial Catalog=zuHause;Persist Security Info=False;User ID=zuHause_dev;Password=DB$MSIT67;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("admins", tb => tb.HasComment("管理員資料"));

            entity.Property(e => e.AdminId)
                .ValueGeneratedNever()
                .HasComment("管理員ID");
            entity.Property(e => e.Account).HasComment("帳號");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.LastLoginAt).HasComment("最後登入時間");
            entity.Property(e => e.Name).HasComment("姓名");
            entity.Property(e => e.PasswordHash).HasComment("密碼雜湊");
            entity.Property(e => e.PasswordSalt).HasComment("密碼 Salt");
            entity.Property(e => e.PasswordUpdatedAt).HasComment("密碼更新時間");
            entity.Property(e => e.RoleCode).HasComment("角色代碼");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.RoleCodeNavigation).WithMany(p => p.Admins)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_admins_role");
        });

        modelBuilder.Entity<AdminMessageTemplate>(entity =>
        {
            entity.ToTable("adminMessageTemplates", tb => tb.HasComment("後台訊息模板表"));

            entity.Property(e => e.TemplateId).HasComment("模板ID(自動遞增)");
            entity.Property(e => e.CategoryCode).HasComment("模板類別代碼");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.TemplateContent).HasComment("內容");
            entity.Property(e => e.Title).HasComment("標題");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
        });

        modelBuilder.Entity<AdminRole>(entity =>
        {
            entity.ToTable("adminRoles", tb => tb.HasComment("管理員角色資料"));

            entity.Property(e => e.RoleCode).HasComment("角色代碼");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.PermissionsJson).HasComment("權限JSON");
            entity.Property(e => e.RoleName).HasComment("角色名稱");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
        });

        modelBuilder.Entity<ApplicationStatusLog>(entity =>
        {
            entity.ToTable("applicationStatusLogs", tb => tb.HasComment("申請狀態歷程表"));

            entity.Property(e => e.StatusLogId).HasComment("狀態歷程ID");
            entity.Property(e => e.ApplicationId).HasComment("申請ID");
            entity.Property(e => e.ChangedAt).HasComment("進入狀態時間");
            entity.Property(e => e.Note).HasComment("備註");
            entity.Property(e => e.StatusCode).HasComment("狀態代碼");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Application).WithMany(p => p.ApplicationStatusLogs)
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

            entity.Property(e => e.ApprovalId).HasComment("審核ID (自動遞增，從701開始)");
            entity.Property(e => e.ApplicantMemberId).HasComment("申請會員ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.CurrentApproverId).HasComment("審核人員ID");
            entity.Property(e => e.ModuleCode).HasComment("模組代碼");
            entity.Property(e => e.SourcePropertyId).HasComment("審核房源ID");
            entity.Property(e => e.StatusCategory).HasComment("狀態類別 (計算欄位)");
            entity.Property(e => e.StatusCode).HasComment("審核狀態碼");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.ApplicantMember).WithMany(p => p.Approvals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_approvals_applicant");

            entity.HasOne(d => d.SourceProperty).WithMany(p => p.Approvals).HasConstraintName("FK_approvals_Property");

            entity.HasOne(d => d.SystemCode).WithMany(p => p.Approvals).HasConstraintName("FK_approvals_status");
        });

        modelBuilder.Entity<ApprovalItem>(entity =>
        {
            entity.ToTable("approvalItems", tb => tb.HasComment("審核明細"));

            entity.Property(e => e.ApprovalItemId).HasComment("審核明細ID (自動遞增，從801開始)");
            entity.Property(e => e.ActionBy).HasComment("操作者ID");
            entity.Property(e => e.ActionCategory).HasComment("操作類別 (計算欄位)");
            entity.Property(e => e.ActionNote).HasComment("內部操作備註");
            entity.Property(e => e.ActionType).HasComment("操作類型");
            entity.Property(e => e.ApprovalId).HasComment("審核ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.SnapshotJson).HasComment("審核快照JSON");

            entity.HasOne(d => d.Approval).WithMany(p => p.ApprovalItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_approvalItems_approval");

            entity.HasOne(d => d.SystemCode).WithMany(p => p.ApprovalItems).HasConstraintName("FK_approvalItems_action");
        });

        modelBuilder.Entity<CarouselImage>(entity =>
        {
            entity.ToTable("carouselImages", tb => tb.HasComment("輪播圖片表"));

            entity.Property(e => e.CarouselImageId).HasComment("圖片ID");
            entity.Property(e => e.Category).HasComment("分類");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.DisplayOrder).HasComment("顯示順序");
            entity.Property(e => e.EndAt).HasComment("結束時間");
            entity.Property(e => e.ImageUrl).HasComment("圖片URL");
            entity.Property(e => e.ImagesName).HasComment("名稱");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.PageCode).HasComment("頁面識別碼");
            entity.Property(e => e.StartAt).HasComment("開始時間");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
            entity.Property(e => e.WebUrl).HasComment("圖片超連結");

            entity.HasOne(d => d.PageCodeNavigation).WithMany(p => p.CarouselImages).HasConstraintName("FK_carouselImages_page");
        });

        modelBuilder.Entity<Chatroom>(entity =>
        {
            entity.ToTable("chatrooms", tb => tb.HasComment("聊天室"));

            entity.Property(e => e.ChatroomId).HasComment("聊天室ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.InitiatorMemberId).HasComment("發起人會員ID");
            entity.Property(e => e.LastMessageAt).HasComment("最後訊息時間");
            entity.Property(e => e.ParticipantMemberId).HasComment("參與者會員ID");
            entity.Property(e => e.PropertyId).HasComment("房源ID");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.InitiatorMember).WithMany(p => p.ChatroomInitiatorMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_chatrooms_initiator");

            entity.HasOne(d => d.ParticipantMember).WithMany(p => p.ChatroomParticipantMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_chatrooms_participant");

            entity.HasOne(d => d.Property).WithMany(p => p.Chatrooms).HasConstraintName("FK_chatrooms_property");
        });

        modelBuilder.Entity<ChatroomMessage>(entity =>
        {
            entity.ToTable("chatroomMessages", tb => tb.HasComment("聊天室訊息"));

            entity.Property(e => e.MessageId).HasComment("訊息ID");
            entity.Property(e => e.ChatroomId).HasComment("聊天室ID");
            entity.Property(e => e.IsRead).HasComment("是否已讀");
            entity.Property(e => e.MessageContent).HasComment("內容");
            entity.Property(e => e.ReadAt).HasComment("已讀時間");
            entity.Property(e => e.SenderMemberId).HasComment("發送者會員ID");
            entity.Property(e => e.SentAt).HasComment("傳送時間");

            entity.HasOne(d => d.Chatroom).WithMany(p => p.ChatroomMessages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_chatroomMessages_chatroom");

            entity.HasOne(d => d.SenderMember).WithMany(p => p.ChatroomMessages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_chatroomMessages_sender");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("cities", tb => tb.HasComment("縣市代碼表"));

            entity.Property(e => e.CityCode).HasComment("縣市代碼");
            entity.Property(e => e.CityName).HasComment("縣市名稱");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DisplayOrder).HasComment("排序");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("contracts", tb => tb.HasComment("合約欄位儲存表"));

            entity.Property(e => e.ContractId).HasComment("合約ID");
            entity.Property(e => e.CleaningFee).HasComment("清潔費");
            entity.Property(e => e.CourtJurisdiction).HasComment("管轄法院");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.CustomName).HasComment("合約自訂名稱");
            entity.Property(e => e.DepositAmount).HasComment("押金金額");
            entity.Property(e => e.EndDate).HasComment("合約結束日");
            entity.Property(e => e.IsSublettable).HasComment("是否可轉租");
            entity.Property(e => e.ManagementFee).HasComment("管理費");
            entity.Property(e => e.ParkingFee).HasComment("停車費");
            entity.Property(e => e.PenaltyAmount).HasComment("違約金金額");
            entity.Property(e => e.RentalApplicationId).HasComment("申請編號ID");
            entity.Property(e => e.StartDate).HasComment("合約起始日");
            entity.Property(e => e.Status).HasComment("合約狀態");
            entity.Property(e => e.TemplateId).HasComment("合約範本編號");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
            entity.Property(e => e.UsagePurpose).HasComment("使用目的");

            entity.HasOne(d => d.RentalApplication).WithMany(p => p.Contracts).HasConstraintName("FK_contracts_rentalApp");

            entity.HasOne(d => d.Template).WithMany(p => p.Contracts).HasConstraintName("FK_contracts_template");
        });

        modelBuilder.Entity<ContractComment>(entity =>
        {
            entity.ToTable("contractComments", tb => tb.HasComment("合約備註表"));

            entity.Property(e => e.ContractCommentId).HasComment("合約備註ID");
            entity.Property(e => e.CommentText).HasComment("內容");
            entity.Property(e => e.CommentType).HasComment("備註類型");
            entity.Property(e => e.ContractId).HasComment("合約ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.CreatedById).HasComment("建立者");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractComments).HasConstraintName("FK_contractComments_contract");
        });

        modelBuilder.Entity<ContractCustomField>(entity =>
        {
            entity.ToTable("contractCustomFields", tb => tb.HasComment("合約附表(動態欄位)"));

            entity.Property(e => e.ContractCustomFieldId).HasComment("動態欄位ID");
            entity.Property(e => e.ContractId).HasComment("合約ID");
            entity.Property(e => e.DisplayOrder).HasComment("顯示順序");
            entity.Property(e => e.FieldKey).HasComment("動態欄位名稱");
            entity.Property(e => e.FieldType).HasComment("動態欄位型態");
            entity.Property(e => e.FieldValue).HasComment("動態欄位值");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractCustomFields).HasConstraintName("FK_contractCustomFields_contract");
        });

        modelBuilder.Entity<ContractFurnitureItem>(entity =>
        {
            entity.ToTable("contractFurnitureItems", tb => tb.HasComment("合約內容家具表"));

            entity.Property(e => e.ContractFurnitureItemId).HasComment("家具清單ID");
            entity.Property(e => e.Amount).HasComment("小計");
            entity.Property(e => e.ContractId).HasComment("合約ID");
            entity.Property(e => e.FurnitureCondition).HasComment("家具狀況");
            entity.Property(e => e.FurnitureName).HasComment("家具名稱");
            entity.Property(e => e.Quantity).HasComment("數量");
            entity.Property(e => e.RepairChargeOwner).HasComment("修繕費負責人");
            entity.Property(e => e.RepairResponsibility).HasComment("維修權責");
            entity.Property(e => e.UnitPrice).HasComment("單價");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractFurnitureItems).HasConstraintName("FK_contractFurnitureItems_contract");
        });

        modelBuilder.Entity<ContractSignature>(entity =>
        {
            entity.ToTable("contractSignatures", tb => tb.HasComment("電子簽名儲存表"));

            entity.Property(e => e.IdcontractSignatureId).HasComment("電子簽名ID");
            entity.Property(e => e.ContractId).HasComment("合約ID");
            entity.Property(e => e.SignMethod).HasComment("簽名方式");
            entity.Property(e => e.SignVerifyInfo).HasComment("簽署驗證資訊");
            entity.Property(e => e.SignatureFileUrl).HasComment("簽名檔URL");
            entity.Property(e => e.SignedAt).HasComment("時間戳");
            entity.Property(e => e.SignerId).HasComment("簽約人ID");
            entity.Property(e => e.SignerRole).HasComment("簽署人身份");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractSignatures).HasConstraintName("FK_contractSignatures_contract");

            entity.HasOne(d => d.Signer).WithMany(p => p.ContractSignatures)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_contractSignatures_member");
        });

        modelBuilder.Entity<ContractTemplate>(entity =>
        {
            entity.ToTable("contractTemplates", tb => tb.HasComment("合約範本表"));

            entity.Property(e => e.ContractTemplateId)
                .ValueGeneratedNever()
                .HasComment("範本ID");
            entity.Property(e => e.TemplateContent).HasComment("範本內容");
            entity.Property(e => e.TemplateName).HasComment("範本名稱");
            entity.Property(e => e.UploadedAt).HasComment("範本上傳時間");
        });

        modelBuilder.Entity<CustomerServiceTicket>(entity =>
        {
            entity.ToTable("customerServiceTickets", tb => tb.HasComment("客服聯繫表"));

            entity.Property(e => e.TicketId).HasComment("客服單ID (自動遞增，從201開始)");
            entity.Property(e => e.CategoryCode).HasComment("主分類代碼");
            entity.Property(e => e.ContractId).HasComment("租約ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.FurnitureOrderId).HasComment("家具訂單ID");
            entity.Property(e => e.HandledBy).HasComment("客服人員ID");
            entity.Property(e => e.IsResolved).HasComment("是否結案");
            entity.Property(e => e.MemberId).HasComment("使用者ID");
            entity.Property(e => e.PropertyId).HasComment("房源ID");
            entity.Property(e => e.ReplyAt).HasComment("最後回覆時間");
            entity.Property(e => e.ReplyContent).HasComment("客服最後回覆");
            entity.Property(e => e.StatusCode).HasComment("狀態代碼");
            entity.Property(e => e.Subject).HasComment("主旨");
            entity.Property(e => e.TicketContent).HasComment("需求內容");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Contract).WithMany(p => p.CustomerServiceTickets).HasConstraintName("FK_custTickets_contract");

            entity.HasOne(d => d.FurnitureOrder).WithMany(p => p.CustomerServiceTickets).HasConstraintName("FK_custTickets_furnOrder");

            entity.HasOne(d => d.Member).WithMany(p => p.CustomerServiceTickets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_custTickets_member");

            entity.HasOne(d => d.Property).WithMany(p => p.CustomerServiceTickets).HasConstraintName("FK_custTickets_property");
        });

        modelBuilder.Entity<DeliveryFeePlan>(entity =>
        {
            entity.ToTable("deliveryFeePlans", tb => tb.HasComment("家具配送費方案表"));

            entity.Property(e => e.PlanId)
                .ValueGeneratedNever()
                .HasComment("配送方案ID");
            entity.Property(e => e.BaseFee).HasComment("基本費用");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.CurrencyCode).HasComment("幣別");
            entity.Property(e => e.EndAt).HasComment("結束時間");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.MaxWeightKg).HasComment("重量上限KG");
            entity.Property(e => e.PlanName).HasComment("方案名稱");
            entity.Property(e => e.RemoteAreaSurcharge).HasComment("偏遠加收");
            entity.Property(e => e.StartAt).HasComment("生效時間");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("districts", tb => tb.HasComment("鄉鎮區代碼表"));

            entity.Property(e => e.DistrictId).HasComment("鄉鎮區ID");
            entity.Property(e => e.CityCode).HasComment("縣市代碼");
            entity.Property(e => e.CityId).HasComment("縣市ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DisplayOrder).HasComment("排序");
            entity.Property(e => e.DistrictCode).HasComment("區代碼");
            entity.Property(e => e.DistrictName).HasComment("區名稱");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
            entity.Property(e => e.ZipCode).HasComment("郵遞區號");

            entity.HasOne(d => d.City).WithMany(p => p.Districts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_districts_cities");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.ToTable("favorites", tb => tb.HasComment("收藏表"));

            entity.Property(e => e.MemberId).HasComment("會員ID");
            entity.Property(e => e.PropertyId).HasComment("房源ID");
            entity.Property(e => e.FavoritedAt).HasComment("收藏時間");
            entity.Property(e => e.IsActive).HasComment("是否有效");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Member).WithMany(p => p.Favorites)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorites_member");

            entity.HasOne(d => d.Property).WithMany(p => p.Favorites)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorites_property");
        });

        modelBuilder.Entity<FileApproval>(entity =>
        {
            entity.ToTable("fileApprovals", tb => tb.HasComment("檔案審核"));

            entity.Property(e => e.ApprovalId)
                .ValueGeneratedNever()
                .HasComment("審核ID");
            entity.Property(e => e.AppliedAt).HasComment("申請時間");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.MemberId).HasComment("會員ID");
            entity.Property(e => e.ResultDescription).HasComment("審核說明");
            entity.Property(e => e.ReviewedAt).HasComment("審核時間");
            entity.Property(e => e.ReviewerAdminId).HasComment("審核人員ID");
            entity.Property(e => e.StatusCode).HasComment("審核狀態代碼");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
            entity.Property(e => e.UploadId).HasComment("上傳ID");

            entity.HasOne(d => d.Upload).WithMany(p => p.FileApprovals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_fileApprovals_upload");
        });

        modelBuilder.Entity<FurnitureCart>(entity =>
        {
            entity.ToTable("furnitureCarts", tb => tb.HasComment("家具購物車表"));

            entity.Property(e => e.FurnitureCartId).HasComment("購物車ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.MemberId).HasComment("會員ID");
            entity.Property(e => e.PropertyId).HasComment("房源ID");
            entity.Property(e => e.Status).HasComment("狀態");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Member).WithMany(p => p.FurnitureCarts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureCarts_member");

            entity.HasOne(d => d.Property).WithMany(p => p.FurnitureCarts).HasConstraintName("FK_furnitureCarts_property");
        });

        modelBuilder.Entity<FurnitureCartItem>(entity =>
        {
            entity.ToTable("furnitureCartItems", tb => tb.HasComment("家具購物車明細表"));

            entity.Property(e => e.CartItemId).HasComment("明細ID");
            entity.Property(e => e.CartId).HasComment("購物車ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.ProductId).HasComment("商品ID");
            entity.Property(e => e.Quantity).HasComment("數量");
            entity.Property(e => e.RentalDays).HasComment("租期(天)");
            entity.Property(e => e.SubTotal).HasComment("小計");
            entity.Property(e => e.UnitPriceSnapshot).HasComment("單價快照");

            entity.HasOne(d => d.Cart).WithMany(p => p.FurnitureCartItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureCartItems_cart");

            entity.HasOne(d => d.Product).WithMany(p => p.FurnitureCartItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureCartItems_product");
        });

        modelBuilder.Entity<FurnitureCategory>(entity =>
        {
            entity.ToTable("furnitureCategories", tb => tb.HasComment("家具商品分類表"));

            entity.Property(e => e.FurnitureCategoriesId).HasComment("分類ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.Depth).HasComment("階層層級");
            entity.Property(e => e.DisplayOrder).HasComment("顯示排序");
            entity.Property(e => e.Name).HasComment("分類名稱");
            entity.Property(e => e.ParentId).HasComment("上層分類ID");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasConstraintName("FK_furnCate_parent");
        });

        modelBuilder.Entity<FurnitureInventory>(entity =>
        {
            entity.ToTable("furnitureInventory", tb => tb.HasComment("家具庫存表"));

            entity.Property(e => e.FurnitureInventoryId).HasComment("庫存ID");
            entity.Property(e => e.AvailableQuantity).HasComment("可用庫存");
            entity.Property(e => e.ProductId).HasComment("商品ID");
            entity.Property(e => e.RentedQuantity).HasComment("已出租數量");
            entity.Property(e => e.SafetyStock).HasComment("安全庫存");
            entity.Property(e => e.TotalQuantity).HasComment("總庫存數量");
            entity.Property(e => e.UpdatedAt).HasComment("最後更新時間");

            entity.HasOne(d => d.Product).WithMany(p => p.FurnitureInventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureInventory_product");
        });

        modelBuilder.Entity<FurnitureOrder>(entity =>
        {
            entity.ToTable("furnitureOrders", tb => tb.HasComment("家具訂單查詢表"));

            entity.Property(e => e.FurnitureOrderId).HasComment("訂單ID");
            entity.Property(e => e.ContractLink).HasComment("合約連結");
            entity.Property(e => e.CreatedAt).HasComment("成立時間");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.MemberId).HasComment("會員ID");
            entity.Property(e => e.PaymentStatus).HasComment("付款狀態");
            entity.Property(e => e.PropertyId).HasComment("房源ID");
            entity.Property(e => e.Status).HasComment("訂單狀態");
            entity.Property(e => e.TotalAmount).HasComment("總金額");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Member).WithMany(p => p.FurnitureOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrders_member");

            entity.HasOne(d => d.Property).WithMany(p => p.FurnitureOrders).HasConstraintName("FK_furnitureOrders_property");
        });

        modelBuilder.Entity<FurnitureOrderHistory>(entity =>
        {
            entity.ToTable("furnitureOrderHistory", tb => tb.HasComment("家具歷史訂單清單"));

            entity.Property(e => e.FurnitureOrderHistoryId).HasComment("流水號");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DailyRentalSnapshot).HasComment("單日租金快照");
            entity.Property(e => e.DamageNote).HasComment("損壞說明");
            entity.Property(e => e.DescriptionSnapshot).HasComment("商品描述快照");
            entity.Property(e => e.ItemStatus).HasComment("明細狀態");
            entity.Property(e => e.OrderId).HasComment("訂單ID");
            entity.Property(e => e.ProductId).HasComment("商品ID");
            entity.Property(e => e.ProductNameSnapshot).HasComment("商品名稱快照");
            entity.Property(e => e.Quantity).HasComment("數量");
            entity.Property(e => e.RentalEnd).HasComment("租借結束日");
            entity.Property(e => e.RentalStart).HasComment("租借開始日");
            entity.Property(e => e.ReturnedAt).HasComment("實際歸還日期");
            entity.Property(e => e.SubTotal).HasComment("小計");

            entity.HasOne(d => d.Order).WithMany(p => p.FurnitureOrderHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrderHistory_order");

            entity.HasOne(d => d.Product).WithMany(p => p.FurnitureOrderHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrderHistory_product");
        });

        modelBuilder.Entity<FurnitureOrderItem>(entity =>
        {
            entity.ToTable("furnitureOrderItems", tb => tb.HasComment("家具訂單查詢明細表"));

            entity.Property(e => e.FurnitureOrderItemId).HasComment("明細ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DailyRentalSnapshot).HasComment("單日租金快照");
            entity.Property(e => e.OrderId).HasComment("訂單ID");
            entity.Property(e => e.ProductId).HasComment("商品ID");
            entity.Property(e => e.Quantity).HasComment("數量");
            entity.Property(e => e.RentalDays).HasComment("租期(天)");
            entity.Property(e => e.SubTotal).HasComment("小計");

            entity.HasOne(d => d.Order).WithMany(p => p.FurnitureOrderItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrderItems_order");

            entity.HasOne(d => d.Product).WithMany(p => p.FurnitureOrderItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureOrderItems_product");
        });

        modelBuilder.Entity<FurnitureProduct>(entity =>
        {
            entity.ToTable("furnitureProducts", tb => tb.HasComment("家具商品資料表"));

            entity.Property(e => e.FurnitureProductId).HasComment("商品ID");
            entity.Property(e => e.CategoryId).HasComment("分類ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DailyRental).HasComment("每日租金");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.DelistedAt).HasComment("下架時間");
            entity.Property(e => e.Description).HasComment("商品描述");
            entity.Property(e => e.ImageUrl).HasComment("商品圖片URL");
            entity.Property(e => e.ListPrice).HasComment("原價");
            entity.Property(e => e.ListedAt).HasComment("上架時間");
            entity.Property(e => e.ProductName).HasComment("商品名稱");
            entity.Property(e => e.Status).HasComment("上下架狀態");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Category).WithMany(p => p.FurnitureProducts).HasConstraintName("FK_furnitureProducts_category");
        });

        modelBuilder.Entity<FurnitureRentalContract>(entity =>
        {
            entity.ToTable("furnitureRentalContracts", tb => tb.HasComment("家具租賃合約表"));

            entity.Property(e => e.FurnitureRentalContractsId).HasComment("合約ID");
            entity.Property(e => e.ContractJson).HasComment("合約 JSON");
            entity.Property(e => e.ContractLink).HasComment("簽章連結");
            entity.Property(e => e.CreatedAt).HasComment("簽約日期");
            entity.Property(e => e.DeliveryDate).HasComment("配送日期");
            entity.Property(e => e.ESignatureValue).HasComment("電子簽章值");
            entity.Property(e => e.OrderId).HasComment("訂單ID");
            entity.Property(e => e.SignStatus).HasComment("簽署狀態");
            entity.Property(e => e.SignedAt).HasComment("簽署完成時間");
            entity.Property(e => e.TerminationPolicy).HasComment("退租政策");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Order).WithMany(p => p.FurnitureRentalContracts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_furnitureRentalContracts_order");
        });

        modelBuilder.Entity<GoogleMapsApiUsage>(entity =>
        {
            entity.HasKey(e => e.GoogleMapsApiId).HasName("PK__GoogleMa__955E5478366F8596");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("pk_images");

            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.UploadedByMember).WithMany(p => p.Images).HasConstraintName("fk_images_members");
        });

        modelBuilder.Entity<InventoryEvent>(entity =>
        {
            entity.ToTable("inventoryEvents", tb => tb.HasComment("庫存事件表"));

            entity.Property(e => e.FurnitureInventoryId)
                .ValueGeneratedNever()
                .HasComment("事件ID");
            entity.Property(e => e.EventType).HasComment("事件類型");
            entity.Property(e => e.OccurredAt).HasComment("發生時間");
            entity.Property(e => e.ProductId).HasComment("商品ID");
            entity.Property(e => e.Quantity).HasComment("異動數量");
            entity.Property(e => e.RecordedAt).HasComment("寫入時間");
            entity.Property(e => e.SourceId).HasComment("來源編號");
            entity.Property(e => e.SourceType).HasComment("來源類型");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryEvents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_inventoryEvents_product");
        });

        modelBuilder.Entity<ListingPlan>(entity =>
        {
            entity.ToTable("listingPlans", tb => tb.HasComment("刊登費方案表"));

            entity.Property(e => e.PlanId).HasComment("刊登費方案ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.CurrencyCode).HasComment("幣別");
            entity.Property(e => e.EndAt).HasComment("結束時間");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.MinListingDays).HasComment("最小上架天數");
            entity.Property(e => e.PlanName).HasComment("方案名稱");
            entity.Property(e => e.PricePerDay).HasComment("每日刊登費");
            entity.Property(e => e.StartAt).HasComment("生效時間");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("members", tb => tb.HasComment("會員資料表"));

            entity.Property(e => e.MemberId)
                .ValueGeneratedNever()
                .HasComment("會員ID");
            entity.Property(e => e.AddressLine).HasComment("詳細地址");
            entity.Property(e => e.BirthDate).HasComment("生日");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.Email).HasComment("電子信箱");
            entity.Property(e => e.EmailVerifiedAt).HasComment("Email驗證通過時間");
            entity.Property(e => e.Gender).HasComment("性別");
            entity.Property(e => e.IdentityVerifiedAt).HasComment("身份驗證通過時間");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.IsLandlord).HasComment("是否為房東");
            entity.Property(e => e.LastLoginAt).HasComment("最後登入時間");
            entity.Property(e => e.MemberName).HasComment("會員姓名");
            entity.Property(e => e.MemberTypeId).HasComment("會員身份別ID");
            entity.Property(e => e.NationalIdNo)
                .IsFixedLength()
                .HasComment("身分證號");
            entity.Property(e => e.Password).HasComment("密碼雜湊");
            entity.Property(e => e.PhoneNumber).HasComment("手機號碼");
            entity.Property(e => e.PhoneVerifiedAt).HasComment("手機驗證通過時間");
            entity.Property(e => e.PrimaryRentalCityId).HasComment("主要承租縣市ID");
            entity.Property(e => e.PrimaryRentalDistrictId).HasComment("主要承租區域ID");
            entity.Property(e => e.ResidenceCityId).HasComment("居住縣市ID");
            entity.Property(e => e.ResidenceDistrictId).HasComment("居住區域ID");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.MemberType).WithMany(p => p.Members).HasConstraintName("FK_members_memberType");

            entity.HasOne(d => d.PrimaryRentalCity).WithMany(p => p.MemberPrimaryRentalCities).HasConstraintName("FK_members_primaryCity");

            entity.HasOne(d => d.PrimaryRentalDistrict).WithMany(p => p.MemberPrimaryRentalDistricts).HasConstraintName("FK_members_primaryDistrict");

            entity.HasOne(d => d.ResidenceCity).WithMany(p => p.MemberResidenceCities).HasConstraintName("FK_members_resCity");

            entity.HasOne(d => d.ResidenceDistrict).WithMany(p => p.MemberResidenceDistricts).HasConstraintName("FK_members_resDistrict");
        });

        modelBuilder.Entity<MemberType>(entity =>
        {
            entity.ToTable("memberTypes", tb => tb.HasComment("會員身分表"));

            entity.Property(e => e.MemberTypeId).HasComment("身份ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.Description).HasComment("描述");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.TypeName).HasComment("身分名稱");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
        });

        modelBuilder.Entity<MemberVerification>(entity =>
        {
            entity.ToTable("memberVerifications", tb => tb.HasComment("通訊相關驗證表"));

            entity.Property(e => e.VerificationId).HasComment("驗證ID");
            entity.Property(e => e.IsSuccessful).HasComment("是否驗證成功");
            entity.Property(e => e.MemberId).HasComment("會員ID");
            entity.Property(e => e.SentAt).HasComment("發送時間");
            entity.Property(e => e.VerificationCode).HasComment("驗證碼");
            entity.Property(e => e.VerificationTypeCode).HasComment("驗證類型代碼");
            entity.Property(e => e.VerifiedAt).HasComment("驗證完成時間");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberVerifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_memberVerifications_member");
        });

        modelBuilder.Entity<MessagePlacement>(entity =>
        {
            entity.ToTable("messagePlacements", tb => tb.HasComment("訊息位置"));

            entity.Property(e => e.PageCode).HasComment("頁面識別碼");
            entity.Property(e => e.SectionCode).HasComment("區段代碼");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DisplayOrder).HasComment("顯示順序");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.MessageId).HasComment("訊息ID");
            entity.Property(e => e.Subtitle).HasComment("小標題");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Message).WithMany(p => p.MessagePlacements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_msgPlacements_message");
        });

        modelBuilder.Entity<OrderEvent>(entity =>
        {
            entity.ToTable("orderEvents", tb => tb.HasComment("訂單事件表"));

            entity.Property(e => e.OrderEventsId)
                .ValueGeneratedNever()
                .HasComment("事件ID");
            entity.Property(e => e.EventType).HasComment("事件類型");
            entity.Property(e => e.OccurredAt).HasComment("發生時間");
            entity.Property(e => e.OrderId).HasComment("訂單ID");
            entity.Property(e => e.Payload).HasComment("事件內容");
            entity.Property(e => e.RecordedAt).HasComment("寫入時間");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderEvents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orderEvents_order");
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("pages", tb => tb.HasComment("共用頁面代碼表"));

            entity.Property(e => e.PageCode).HasComment("頁面識別碼");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DisplayOrder).HasComment("顯示順序");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.ModuleScope).HasComment("模組");
            entity.Property(e => e.PageName).HasComment("頁面名稱");
            entity.Property(e => e.RoutePath).HasComment("路徑");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("properties", tb =>
                {
                    tb.HasComment("房源資料表");
                    tb.HasTrigger("trg_properties_validate_landlord");
                });

            entity.Property(e => e.PropertyId)
                .ValueGeneratedNever()
                .HasComment("房源ID");
            entity.Property(e => e.AddressLine).HasComment("詳細地址");
            entity.Property(e => e.Area).HasComment("坪數");
            entity.Property(e => e.BathroomCount).HasComment("衛數");
            entity.Property(e => e.CityId).HasComment("縣市ID");
            entity.Property(e => e.CleaningFeeAmount).HasComment("清潔費金額");
            entity.Property(e => e.CleaningFeeRequired).HasComment("清潔費需額外收費");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.CurrentFloor).HasComment("所在樓層");
            entity.Property(e => e.CustomElectricityFee).HasComment("自訂電費");
            entity.Property(e => e.CustomWaterFee).HasComment("自訂水費");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.DepositAmount).HasComment("押金金額");
            entity.Property(e => e.DepositMonths).HasComment("押金月數");
            entity.Property(e => e.Description).HasComment("詳細描述");
            entity.Property(e => e.DistrictId).HasComment("區域ID");
            entity.Property(e => e.ElectricityFeeType).HasComment("電費計算方式");
            entity.Property(e => e.ExpireAt).HasComment("上架到期時間");
            entity.Property(e => e.IsPaid).HasComment("付款狀態");
            entity.Property(e => e.LandlordMemberId).HasComment("房東會員ID");
            entity.Property(e => e.ListingDays).HasComment("刊登天數");
            entity.Property(e => e.ListingFeeAmount).HasComment("刊登費用");
            entity.Property(e => e.ListingPlanId).HasComment("刊登費方案ID");
            entity.Property(e => e.LivingRoomCount).HasComment("廳數");
            entity.Property(e => e.ManagementFeeAmount).HasComment("管理費金額");
            entity.Property(e => e.ManagementFeeIncluded).HasComment("管理費含租金");
            entity.Property(e => e.MinimumRentalMonths).HasComment("最短租期(月)");
            entity.Property(e => e.MonthlyRent).HasComment("月租金");
            entity.Property(e => e.PaidAt).HasComment("完成付款時間");
            entity.Property(e => e.ParkingAvailable).HasComment("有停車位");
            entity.Property(e => e.ParkingFeeAmount).HasComment("停車位費用");
            entity.Property(e => e.ParkingFeeRequired).HasComment("停車費需額外收費");
            entity.Property(e => e.PreviewImageUrl).HasComment("預覽圖連結");
            entity.Property(e => e.PropertyProofUrl).HasComment("房產證明文件URL");
            entity.Property(e => e.PublishedAt).HasComment("上架日期");
            entity.Property(e => e.RoomCount).HasComment("房數");
            entity.Property(e => e.SpecialRules).HasComment("特殊守則");
            entity.Property(e => e.StatusCode).HasComment("房源狀態代碼");
            entity.Property(e => e.Title).HasComment("房源標題");
            entity.Property(e => e.TotalFloors).HasComment("總樓層");
            entity.Property(e => e.UpdatedAt).HasComment("最後修改日期");
            entity.Property(e => e.WaterFeeType).HasComment("水費計算方式");

            entity.HasOne(d => d.LandlordMember).WithMany(p => p.Properties)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_properties_landlord");

            entity.HasOne(d => d.ListingPlan).WithMany(p => p.Properties).HasConstraintName("FK_properties_listingPlan");
        });

        modelBuilder.Entity<PropertyComplaint>(entity =>
        {
            entity.ToTable("propertyComplaints", tb => tb.HasComment("房源投訴表"));

            entity.Property(e => e.ComplaintId).HasComment("投訴ID (自動遞增，從301開始)");
            entity.Property(e => e.ComplainantId).HasComment("投訴人ID");
            entity.Property(e => e.ComplaintContent).HasComment("投訴內容");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.HandledBy).HasComment("處理人員ID");
            entity.Property(e => e.InternalNote).HasComment("內部註記");
            entity.Property(e => e.LandlordId).HasComment("被投訴房東ID");
            entity.Property(e => e.PropertyId).HasComment("房源ID");
            entity.Property(e => e.ResolvedAt).HasComment("結案時間");
            entity.Property(e => e.StatusCode).HasComment("處理狀態代碼");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Complainant).WithMany(p => p.PropertyComplaintComplainants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propComplaints_complainant");

            entity.HasOne(d => d.Landlord).WithMany(p => p.PropertyComplaintLandlords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propComplaints_landlord");

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyComplaints)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propComplaints_property");
        });

        modelBuilder.Entity<PropertyEquipmentCategory>(entity =>
        {
            entity.ToTable("propertyEquipmentCategories", tb => tb.HasComment("房源設備分類資料表"));

            entity.Property(e => e.CategoryName).HasComment("設備名稱");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.ParentCategoryId).HasComment("上層分類ID");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory).HasConstraintName("FK_propEquipCate_parent");
        });

        modelBuilder.Entity<PropertyEquipmentRelation>(entity =>
        {
            entity.ToTable("propertyEquipmentRelations", tb => tb.HasComment("房源設備關聯資料表"));

            entity.Property(e => e.RelationId)
                .ValueGeneratedNever()
                .HasComment("關聯ID");
            entity.Property(e => e.CategoryId).HasComment("設備分類ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.PropertyId).HasComment("房源ID");
            entity.Property(e => e.Quantity).HasComment("數量");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Category).WithMany(p => p.PropertyEquipmentRelations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propEquipRel_category");

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyEquipmentRelations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propEquipRel_property");
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.ToTable("propertyImages", tb => tb.HasComment("房源圖片表"));

            entity.Property(e => e.ImageId).HasComment("圖片ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DisplayOrder).HasComment("顯示順序");
            entity.Property(e => e.ImagePath).HasComment("圖片路徑");
            entity.Property(e => e.PropertyId).HasComment("房源ID");

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyImages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_propertyImages_property");
        });

        modelBuilder.Entity<RentalApplication>(entity =>
        {
            entity.ToTable("rentalApplications", tb => tb.HasComment("租賃/看房申請資料表"));

            entity.Property(e => e.ApplicationId).HasComment("申請ID");
            entity.Property(e => e.ApplicationType).HasComment("申請類型");
            entity.Property(e => e.CreatedAt).HasComment("申請時間");
            entity.Property(e => e.CurrentStatus).HasComment("目前狀態");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.HouseholdAddress).HasComment("戶籍地址");
            entity.Property(e => e.IsActive).HasComment("是否有效");
            entity.Property(e => e.MemberId).HasComment("申請會員ID");
            entity.Property(e => e.Message).HasComment("申請留言");
            entity.Property(e => e.PropertyId).HasComment("房源ID");
            entity.Property(e => e.RentalEndDate).HasComment("租期結束");
            entity.Property(e => e.RentalStartDate).HasComment("租期開始");
            entity.Property(e => e.ScheduleTime).HasComment("預約看房時間");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Member).WithMany(p => p.RentalApplications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rentalApplications_member");

            entity.HasOne(d => d.Property).WithMany(p => p.RentalApplications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rentalApplications_property");
        });

        modelBuilder.Entity<RenterPost>(entity =>
        {
            entity.ToTable("renterPosts", tb => tb.HasComment("尋租文章資料表"));

            entity.Property(e => e.PostId)
                .ValueGeneratedNever()
                .HasComment("文章ID");
            entity.Property(e => e.BudgetMax).HasComment("預算上限");
            entity.Property(e => e.BudgetMin).HasComment("預算下限");
            entity.Property(e => e.CityId).HasComment("希望縣市ID");
            entity.Property(e => e.CreatedAt).HasComment("發布時間");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.DistrictId).HasComment("希望區域ID");
            entity.Property(e => e.HouseType).HasComment("房屋類型");
            entity.Property(e => e.IsActive).HasComment("是否有效");
            entity.Property(e => e.MemberId).HasComment("租客會員ID");
            entity.Property(e => e.PostContent).HasComment("詳細需求");
            entity.Property(e => e.ReplyCount).HasComment("回覆數");
            entity.Property(e => e.UpdatedAt).HasComment("最後編輯時間");
            entity.Property(e => e.ViewCount).HasComment("瀏覽數");

            entity.HasOne(d => d.Member).WithMany(p => p.RenterPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_renterPosts_member");
        });

        modelBuilder.Entity<RenterPostReply>(entity =>
        {
            entity.ToTable("renterPostReplies", tb => tb.HasComment("尋租文章回覆資料表"));

            entity.Property(e => e.ReplyId)
                .ValueGeneratedNever()
                .HasComment("回覆ID");
            entity.Property(e => e.CreatedAt).HasComment("回覆時間");
            entity.Property(e => e.IsWithinBudget).HasComment("符合預算");
            entity.Property(e => e.LandlordMemberId).HasComment("房東會員ID");
            entity.Property(e => e.PostId).HasComment("文章ID");
            entity.Property(e => e.ReplyContent).HasComment("回覆內容");
            entity.Property(e => e.SuggestPropertyId).HasComment("推薦房源ID");
            entity.Property(e => e.Tags).HasComment("符合條件標籤");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.LandlordMember).WithMany(p => p.RenterPostReplies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_renterPostReplies_landlord");

            entity.HasOne(d => d.Post).WithMany(p => p.RenterPostReplies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_renterPostReplies_post");
        });

        modelBuilder.Entity<RenterRequirementList>(entity =>
        {
            entity.ToTable("renterRequirementList", tb => tb.HasComment("尋租文章需求條件清單"));

            entity.Property(e => e.RequirementId)
                .ValueGeneratedNever()
                .HasComment("條件ID");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.RequirementName).HasComment("條件名稱");
        });

        modelBuilder.Entity<RenterRequirementRelation>(entity =>
        {
            entity.ToTable("renterRequirementRelations", tb => tb.HasComment("尋租條件關聯"));

            entity.Property(e => e.RelationId)
                .ValueGeneratedNever()
                .HasComment("關聯ID");
            entity.Property(e => e.PostId).HasComment("文章ID");
            entity.Property(e => e.RequirementId).HasComment("條件ID");

            entity.HasOne(d => d.Post).WithMany(p => p.RenterRequirementRelations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rentReqRel_post");

            entity.HasOne(d => d.Requirement).WithMany(p => p.RenterRequirementRelations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rentReqRel_requirement");
        });

        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.ToTable("searchHistory", tb => tb.HasComment("搜索歷史表"));

            entity.Property(e => e.HistoryId)
                .ValueGeneratedNever()
                .HasComment("歷史ID");
            entity.Property(e => e.DeviceType).HasComment("裝置");
            entity.Property(e => e.IpAddress).HasComment("IP");
            entity.Property(e => e.Keyword).HasComment("搜尋關鍵字");
            entity.Property(e => e.MemberId).HasComment("會員ID");
            entity.Property(e => e.ResultCount).HasComment("結果筆數");
            entity.Property(e => e.SearchedAt).HasComment("搜尋時間");

            entity.HasOne(d => d.Member).WithMany(p => p.SearchHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_searchHistory_member");
        });

        modelBuilder.Entity<SiteMessage>(entity =>
        {
            entity.ToTable("siteMessages", tb => tb.HasComment("網站訊息表"));

            entity.Property(e => e.AttachmentUrl).HasComment("圖片/附件URL");
            entity.Property(e => e.Category).HasComment("分類");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.DisplayOrder).HasComment("顯示順序");
            entity.Property(e => e.EndAt).HasComment("結束時間");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.MessageType).HasComment("訊息類型");
            entity.Property(e => e.ModuleScope).HasComment("模組");
            entity.Property(e => e.SiteMessageContent).HasComment("內文");
            entity.Property(e => e.StartAt).HasComment("開始時間");
            entity.Property(e => e.Title).HasComment("標題");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
        });

        modelBuilder.Entity<SystemCode>(entity =>
        {
            entity.ToTable("systemCodes", tb => tb.HasComment("代碼總表"));

            entity.Property(e => e.CodeCategory).HasComment("代碼類別");
            entity.Property(e => e.Code).HasComment("代碼");
            entity.Property(e => e.CodeName).HasComment("代碼名稱");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DisplayOrder).HasComment("排序");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.CodeCategoryNavigation).WithMany(p => p.SystemCodes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_systemCodes_category");
        });

        modelBuilder.Entity<SystemCodeCategory>(entity =>
        {
            entity.ToTable("systemCodeCategories", tb => tb.HasComment("代碼類別表"));

            entity.Property(e => e.CodeCategory).HasComment("代碼類別");
            entity.Property(e => e.CategoryName).HasComment("類別名稱");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.Description).HasComment("描述");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
        });

        modelBuilder.Entity<SystemMessage>(entity =>
        {
            entity.ToTable("systemMessages", tb => tb.HasComment("系統訊息表"));

            entity.HasIndex(e => new { e.IsActive, e.CategoryCode }, "IX_systemMessages_active_category").HasFillFactor(100);

            entity.Property(e => e.MessageId).HasComment("系統訊息ID (自動遞增，從401開始)");
            entity.Property(e => e.AdminId).HasComment("發送管理員ID");
            entity.Property(e => e.AttachmentUrl).HasComment("附件URL");
            entity.Property(e => e.AudienceTypeCode).HasComment("接受者類型代碼");
            entity.Property(e => e.CategoryCode).HasComment("訊息類別代碼");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.IsActive).HasComment("是否啟用");
            entity.Property(e => e.MessageContent).HasComment("發送內容");
            entity.Property(e => e.ReceiverId).HasComment("個別接受者ID");
            entity.Property(e => e.SentAt).HasComment("發送時間");
            entity.Property(e => e.Title).HasComment("發送標題");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Admin).WithMany(p => p.SystemMessages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_systemMessages_admin");

            entity.HasOne(d => d.Receiver).WithMany(p => p.SystemMessages).HasConstraintName("FK_systemMessages_receiver");
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("userNotifications", tb => tb.HasComment("使用者通知資料表"));

            entity.Property(e => e.NotificationId).HasComment("通知ID");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.IsRead).HasComment("是否已讀");
            entity.Property(e => e.LinkUrl).HasComment("相關連結URL");
            entity.Property(e => e.ModuleCode).HasComment("來源模組代碼");
            entity.Property(e => e.NotificationContent).HasComment("訊息內容");
            entity.Property(e => e.ReadAt).HasComment("閱讀時間");
            entity.Property(e => e.ReceiverId).HasComment("接收者ID");
            entity.Property(e => e.SenderId).HasComment("發送者ID");
            entity.Property(e => e.SentAt).HasComment("發送時間");
            entity.Property(e => e.SourceEntityId).HasComment("來源資料ID");
            entity.Property(e => e.StatusCode).HasComment("狀態代碼");
            entity.Property(e => e.Title).HasComment("訊息標題");
            entity.Property(e => e.TypeCode).HasComment("通知類型代碼");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");

            entity.HasOne(d => d.Receiver).WithMany(p => p.UserNotifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_userNotifications_receiver");
        });

        modelBuilder.Entity<UserUpload>(entity =>
        {
            entity.ToTable("userUploads", tb => tb.HasComment("會員上傳資料紀錄表"));

            entity.Property(e => e.UploadId).HasComment("上傳ID");
            entity.Property(e => e.ApprovalId).HasComment("審核ID");
            entity.Property(e => e.Checksum).HasComment("檔案雜湊");
            entity.Property(e => e.CreatedAt).HasComment("建立時間");
            entity.Property(e => e.DeletedAt).HasComment("刪除時間");
            entity.Property(e => e.FileExt).HasComment("副檔名");
            entity.Property(e => e.FilePath).HasComment("檔案路徑");
            entity.Property(e => e.FileSize).HasComment("檔案大小(Byte)");
            entity.Property(e => e.IsActive).HasComment("是否有效");
            entity.Property(e => e.MemberId).HasComment("會員ID");
            entity.Property(e => e.MimeType).HasComment("MIME 類型");
            entity.Property(e => e.ModuleCode).HasComment("來源模組代碼");
            entity.Property(e => e.OriginalFileName).HasComment("原始檔名");
            entity.Property(e => e.SourceEntityId).HasComment("來源資料ID");
            entity.Property(e => e.StoredFileName).HasComment("實體檔名");
            entity.Property(e => e.UpdatedAt).HasComment("更新時間");
            entity.Property(e => e.UploadTypeCode).HasComment("上傳種類代碼");
            entity.Property(e => e.UploadedAt).HasComment("上傳時間");

            entity.HasOne(d => d.Member).WithMany(p => p.UserUploads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_userUploads_member");
        });
        modelBuilder.HasSequence<int>("seq_memberID");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
