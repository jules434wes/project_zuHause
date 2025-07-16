using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zuHause.Migrations
{
    /// <inheritdoc />
    public partial class AddImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_approvals_Property",
                table: "approvals");

            migrationBuilder.DropIndex(
                name: "IX_systemMessages_active_category",
                table: "systemMessages");

            migrationBuilder.DropIndex(
                name: "IX_members_nationalIdNo",
                table: "members");

            migrationBuilder.DropIndex(
                name: "IX_approvals_sourceID",
                table: "approvals");

            migrationBuilder.DropIndex(
                name: "UQ_approvals_module_source",
                table: "approvals");

            migrationBuilder.DropColumn(
                name: "sourceID",
                table: "approvals");

            migrationBuilder.RenameIndex(
                name: "IX_approvals_statusCategory_statusCode",
                table: "approvals",
                newName: "IX_approvals_status_category");

            migrationBuilder.RenameIndex(
                name: "IX_approvalItems_actionCategory_actionType",
                table: "approvalItems",
                newName: "IX_approvalItems_action_category");

            migrationBuilder.AlterColumn<int>(
                name: "messageID",
                table: "systemMessages",
                type: "int",
                nullable: false,
                comment: "系統訊息ID (自動遞增，從401開始)",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "系統訊息ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "categoryID",
                table: "propertyEquipmentCategories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "設備分類ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "complaintId",
                table: "propertyComplaints",
                type: "int",
                nullable: false,
                comment: "投訴ID (自動遞增，從301開始)",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "投訴ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "nationalIdNo",
                table: "members",
                type: "char(10)",
                unicode: false,
                fixedLength: true,
                maxLength: 10,
                nullable: true,
                comment: "身分證號",
                oldClrType: typeof(string),
                oldType: "char(10)",
                oldUnicode: false,
                oldFixedLength: true,
                oldMaxLength: 10,
                oldComment: "身分證號");

            migrationBuilder.AlterColumn<int>(
                name: "planId",
                table: "listingPlans",
                type: "int",
                nullable: false,
                comment: "刊登費方案ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "方案ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ticketId",
                table: "customerServiceTickets",
                type: "int",
                nullable: false,
                comment: "客服單ID (自動遞增，從201開始)",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "客服單ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "uploadId",
                table: "contractSignatures",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "approvalID",
                table: "approvals",
                type: "int",
                nullable: false,
                comment: "審核ID (自動遞增，從701開始)",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "審核ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "sourcePropertyID",
                table: "approvals",
                type: "int",
                nullable: true,
                comment: "審核房源ID");

            migrationBuilder.AlterColumn<string>(
                name: "actionNote",
                table: "approvalItems",
                type: "nvarchar(max)",
                nullable: true,
                comment: "內部操作備註",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "操作備註");

            migrationBuilder.AlterColumn<int>(
                name: "actionBy",
                table: "approvalItems",
                type: "int",
                nullable: true,
                comment: "操作者ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "操作者ID");

            migrationBuilder.AlterColumn<int>(
                name: "approvalItemID",
                table: "approvalItems",
                type: "int",
                nullable: false,
                comment: "審核明細ID (自動遞增，從801開始)",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "審核明細ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "templateID",
                table: "adminMessageTemplates",
                type: "int",
                nullable: false,
                comment: "模板ID(自動遞增)",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "模板ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "statusCategory",
                table: "approvals",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                computedColumnSql: "(CONVERT([nvarchar](20),N'APPROVAL_STATUS'))",
                stored: true,
                comment: "狀態類別 (計算欄位)",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComputedColumnSql: "(CONVERT([nvarchar](20),N'ApprovalStatus'))",
                oldStored: true,
                oldComment: "審核狀態分類");

            migrationBuilder.AlterColumn<string>(
                name: "actionCategory",
                table: "approvalItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                computedColumnSql: "(CONVERT([nvarchar](20),N'APPROVAL_ACTION'))",
                stored: true,
                comment: "操作類別 (計算欄位)",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComputedColumnSql: "(CONVERT([nvarchar](20),N'ApprovalAction'))",
                oldStored: true);

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    imageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    imageGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    entityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entityId = table.Column<int>(type: "int", nullable: false),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    mimeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    originalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    storedFileName = table.Column<string>(type: "nvarchar(max)", nullable: false, computedColumnSql: "lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end", stored: true),
                    fileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    width = table.Column<int>(type: "int", nullable: false),
                    height = table.Column<int>(type: "int", nullable: false),
                    displayOrder = table.Column<int>(type: "int", nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    uploadedByMemberId = table.Column<int>(type: "int", nullable: true),
                    uploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.imageId);
                    table.CheckConstraint("ck_images_category", "[category] IN ('BedRoom','Kitchen','Living','Balcony','Avatar','Gallery','Product')");
                    table.CheckConstraint("ck_images_entityType", "[entityType] IN ('Member','Property','Furniture','Announcement')");
                    table.ForeignKey(
                        name: "fk_images_members",
                        column: x => x.uploadedByMemberId,
                        principalTable: "members",
                        principalColumn: "memberID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_systemMessages_active_category",
                table: "systemMessages",
                columns: new[] { "isActive", "categoryCode" })
                .Annotation("SqlServer:FillFactor", 100);

            migrationBuilder.CreateIndex(
                name: "IX_systemMessages_audienceTypeCode",
                table: "systemMessages",
                column: "audienceTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_systemMessages_deletedAt",
                table: "systemMessages",
                column: "deletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_systemMessages_sentAt",
                table: "systemMessages",
                column: "sentAt");

            migrationBuilder.CreateIndex(
                name: "IX_propertyComplaints_createdAt",
                table: "propertyComplaints",
                column: "createdAt");

            migrationBuilder.CreateIndex(
                name: "IX_propertyComplaints_handledBy",
                table: "propertyComplaints",
                column: "handledBy");

            migrationBuilder.CreateIndex(
                name: "IX_propertyComplaints_resolvedAt",
                table: "propertyComplaints",
                column: "resolvedAt");

            migrationBuilder.CreateIndex(
                name: "IX_propertyComplaints_statusCode",
                table: "propertyComplaints",
                column: "statusCode");

            migrationBuilder.CreateIndex(
                name: "IX_customerServiceTickets_categoryCode",
                table: "customerServiceTickets",
                column: "categoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_customerServiceTickets_createdAt",
                table: "customerServiceTickets",
                column: "createdAt");

            migrationBuilder.CreateIndex(
                name: "IX_customerServiceTickets_isResolved",
                table: "customerServiceTickets",
                column: "isResolved");

            migrationBuilder.CreateIndex(
                name: "IX_customerServiceTickets_statusCode",
                table: "customerServiceTickets",
                column: "statusCode");

            migrationBuilder.CreateIndex(
                name: "IX_approvals_createdAt",
                table: "approvals",
                column: "createdAt");

            migrationBuilder.CreateIndex(
                name: "IX_approvals_currentApproverID",
                table: "approvals",
                column: "currentApproverID");

            migrationBuilder.CreateIndex(
                name: "IX_approvals_moduleCode",
                table: "approvals",
                column: "moduleCode");

            migrationBuilder.CreateIndex(
                name: "IX_approvals_sourcePropertyID",
                table: "approvals",
                column: "sourcePropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_approvals_statusCode",
                table: "approvals",
                column: "statusCode");

            migrationBuilder.CreateIndex(
                name: "UQ_approvals_member_module",
                table: "approvals",
                columns: new[] { "moduleCode", "applicantMemberID", "sourcePropertyID" },
                unique: true,
                filter: "[sourcePropertyID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_approvalItems_actionBy",
                table: "approvalItems",
                column: "actionBy");

            migrationBuilder.CreateIndex(
                name: "IX_approvalItems_actionType",
                table: "approvalItems",
                column: "actionType");

            migrationBuilder.CreateIndex(
                name: "IX_approvalItems_createdAt",
                table: "approvalItems",
                column: "createdAt");

            migrationBuilder.CreateIndex(
                name: "ix_images_entity",
                table: "images",
                columns: new[] { "entityType", "entityId", "category", "displayOrder", "isActive" })
                .Annotation("SqlServer:Include", new[] { "imageGuid", "storedFileName", "fileSizeBytes", "width", "height", "uploadedAt", "mimeType", "originalFileName" });

            migrationBuilder.CreateIndex(
                name: "IX_images_uploadedByMemberId",
                table: "images",
                column: "uploadedByMemberId");

            migrationBuilder.CreateIndex(
                name: "uq_images_imageGuid",
                table: "images",
                column: "imageGuid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_approvals_Property",
                table: "approvals",
                column: "sourcePropertyID",
                principalTable: "properties",
                principalColumn: "propertyID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_approvals_Property",
                table: "approvals");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropIndex(
                name: "IX_systemMessages_active_category",
                table: "systemMessages");

            migrationBuilder.DropIndex(
                name: "IX_systemMessages_audienceTypeCode",
                table: "systemMessages");

            migrationBuilder.DropIndex(
                name: "IX_systemMessages_deletedAt",
                table: "systemMessages");

            migrationBuilder.DropIndex(
                name: "IX_systemMessages_sentAt",
                table: "systemMessages");

            migrationBuilder.DropIndex(
                name: "IX_propertyComplaints_createdAt",
                table: "propertyComplaints");

            migrationBuilder.DropIndex(
                name: "IX_propertyComplaints_handledBy",
                table: "propertyComplaints");

            migrationBuilder.DropIndex(
                name: "IX_propertyComplaints_resolvedAt",
                table: "propertyComplaints");

            migrationBuilder.DropIndex(
                name: "IX_propertyComplaints_statusCode",
                table: "propertyComplaints");

            migrationBuilder.DropIndex(
                name: "IX_customerServiceTickets_categoryCode",
                table: "customerServiceTickets");

            migrationBuilder.DropIndex(
                name: "IX_customerServiceTickets_createdAt",
                table: "customerServiceTickets");

            migrationBuilder.DropIndex(
                name: "IX_customerServiceTickets_isResolved",
                table: "customerServiceTickets");

            migrationBuilder.DropIndex(
                name: "IX_customerServiceTickets_statusCode",
                table: "customerServiceTickets");

            migrationBuilder.DropIndex(
                name: "IX_approvals_createdAt",
                table: "approvals");

            migrationBuilder.DropIndex(
                name: "IX_approvals_currentApproverID",
                table: "approvals");

            migrationBuilder.DropIndex(
                name: "IX_approvals_moduleCode",
                table: "approvals");

            migrationBuilder.DropIndex(
                name: "IX_approvals_sourcePropertyID",
                table: "approvals");

            migrationBuilder.DropIndex(
                name: "IX_approvals_statusCode",
                table: "approvals");

            migrationBuilder.DropIndex(
                name: "UQ_approvals_member_module",
                table: "approvals");

            migrationBuilder.DropIndex(
                name: "IX_approvalItems_actionBy",
                table: "approvalItems");

            migrationBuilder.DropIndex(
                name: "IX_approvalItems_actionType",
                table: "approvalItems");

            migrationBuilder.DropIndex(
                name: "IX_approvalItems_createdAt",
                table: "approvalItems");

            migrationBuilder.DropColumn(
                name: "uploadId",
                table: "contractSignatures");

            migrationBuilder.DropColumn(
                name: "sourcePropertyID",
                table: "approvals");

            migrationBuilder.RenameIndex(
                name: "IX_approvals_status_category",
                table: "approvals",
                newName: "IX_approvals_statusCategory_statusCode");

            migrationBuilder.RenameIndex(
                name: "IX_approvalItems_action_category",
                table: "approvalItems",
                newName: "IX_approvalItems_actionCategory_actionType");

            migrationBuilder.AlterColumn<int>(
                name: "messageID",
                table: "systemMessages",
                type: "int",
                nullable: false,
                comment: "系統訊息ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "系統訊息ID (自動遞增，從401開始)")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "categoryID",
                table: "propertyEquipmentCategories",
                type: "int",
                nullable: false,
                comment: "設備分類ID",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "complaintId",
                table: "propertyComplaints",
                type: "int",
                nullable: false,
                comment: "投訴ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "投訴ID (自動遞增，從301開始)")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "nationalIdNo",
                table: "members",
                type: "char(10)",
                unicode: false,
                fixedLength: true,
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                comment: "身分證號",
                oldClrType: typeof(string),
                oldType: "char(10)",
                oldUnicode: false,
                oldFixedLength: true,
                oldMaxLength: 10,
                oldNullable: true,
                oldComment: "身分證號");

            migrationBuilder.AlterColumn<int>(
                name: "planId",
                table: "listingPlans",
                type: "int",
                nullable: false,
                comment: "方案ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "刊登費方案ID")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ticketId",
                table: "customerServiceTickets",
                type: "int",
                nullable: false,
                comment: "客服單ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "客服單ID (自動遞增，從201開始)")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "approvalID",
                table: "approvals",
                type: "int",
                nullable: false,
                comment: "審核ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "審核ID (自動遞增，從701開始)")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "sourceID",
                table: "approvals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "來源ID");

            migrationBuilder.AlterColumn<string>(
                name: "actionNote",
                table: "approvalItems",
                type: "nvarchar(max)",
                nullable: true,
                comment: "操作備註",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "內部操作備註");

            migrationBuilder.AlterColumn<int>(
                name: "actionBy",
                table: "approvalItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "操作者ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "操作者ID");

            migrationBuilder.AlterColumn<int>(
                name: "approvalItemID",
                table: "approvalItems",
                type: "int",
                nullable: false,
                comment: "審核明細ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "審核明細ID (自動遞增，從801開始)")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "templateID",
                table: "adminMessageTemplates",
                type: "int",
                nullable: false,
                comment: "模板ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "模板ID(自動遞增)")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "statusCategory",
                table: "approvals",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                computedColumnSql: "(CONVERT([nvarchar](20),N'ApprovalStatus'))",
                stored: true,
                comment: "審核狀態分類",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComputedColumnSql: "(CONVERT([nvarchar](20),N'APPROVAL_STATUS'))",
                oldStored: true,
                oldComment: "狀態類別 (計算欄位)");

            migrationBuilder.AlterColumn<string>(
                name: "actionCategory",
                table: "approvalItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                computedColumnSql: "(CONVERT([nvarchar](20),N'ApprovalAction'))",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComputedColumnSql: "(CONVERT([nvarchar](20),N'APPROVAL_ACTION'))",
                oldStored: true,
                oldComment: "操作類別 (計算欄位)");

            migrationBuilder.CreateIndex(
                name: "IX_systemMessages_active_category",
                table: "systemMessages",
                columns: new[] { "isActive", "categoryCode" });

            migrationBuilder.CreateIndex(
                name: "IX_members_nationalIdNo",
                table: "members",
                column: "nationalIdNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_approvals_sourceID",
                table: "approvals",
                column: "sourceID");

            migrationBuilder.CreateIndex(
                name: "UQ_approvals_module_source",
                table: "approvals",
                columns: new[] { "moduleCode", "sourceID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_approvals_Property",
                table: "approvals",
                column: "sourceID",
                principalTable: "properties",
                principalColumn: "propertyID");
        }
    }
}
