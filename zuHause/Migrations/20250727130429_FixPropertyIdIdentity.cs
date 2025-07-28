using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zuHause.Migrations
{
    /// <inheritdoc />
    public partial class FixPropertyIdIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_images_members",
                table: "images");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Images__7516F70C0063CFAC",
                table: "images");

            migrationBuilder.DropIndex(
                name: "ix_images_entity",
                table: "images");

            migrationBuilder.DropIndex(
                name: "IX_Images_UQ_DisplayOrder",
                table: "images");

            migrationBuilder.DropIndex(
                name: "uq_images_imageGuid",
                table: "images");

            migrationBuilder.DropCheckConstraint(
                name: "ck_images_category",
                table: "images");

            migrationBuilder.DropCheckConstraint(
                name: "ck_images_entityType",
                table: "images");

            migrationBuilder.RenameIndex(
                name: "UQ_Images_ImageGuid",
                table: "images",
                newName: "uq_images_imageGuid");

            migrationBuilder.RenameIndex(
                name: "IX_Images_EntityType_EntityId_Covering",
                table: "images",
                newName: "ix_images_entity");

            migrationBuilder.AlterTable(
                name: "images",
                oldComment: "圖片表");

            migrationBuilder.AlterColumn<int>(
                name: "siteMessagesId",
                table: "siteMessages",
                type: "int",
                nullable: false,
                comment: "訊息ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "訊息ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "householdAddress",
                table: "rentalApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "戶籍地址");

            migrationBuilder.AlterColumn<int>(
                name: "propertyID",
                table: "properties",
                type: "int",
                nullable: false,
                comment: "房源ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "房源ID")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "properties",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "properties",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "uploadedAt",
                table: "images",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(DATEADD(HOUR, 8, sysutcdatetime()))",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "sysutcdatetime()");

            migrationBuilder.AlterColumn<Guid>(
                name: "imageGuid",
                table: "images",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "(newsequentialid())",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "newsequentialid()");

            migrationBuilder.AddColumn<string>(
                name: "LandlordHouseholdAddress",
                table: "contracts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customName",
                table: "contracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "合約自訂名稱");

            migrationBuilder.AddColumn<string>(
                name: "WebUrl",
                table: "carouselImages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "storedFileName",
                table: "images",
                type: "varchar(41)",
                unicode: false,
                maxLength: 41,
                nullable: false,
                computedColumnSql: "(lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end)",
                stored: true,
                oldClrType: typeof(string),
                oldType: "varchar(41)",
                oldUnicode: false,
                oldMaxLength: 41,
                oldComputedColumnSql: "lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end",
                oldStored: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_images",
                table: "images",
                column: "imageId");

            migrationBuilder.CreateTable(
                name: "GoogleMapsApiUsage",
                columns: table => new
                {
                    googleMapsApiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    apiType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    requestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    requestCount = table.Column<int>(type: "int", nullable: false),
                    estimatedCost = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    isLimitReached = table.Column<bool>(type: "bit", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleMapsApiUsage", x => x.googleMapsApiId);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_images_members",
                table: "images",
                column: "uploadedByMemberId",
                principalTable: "members",
                principalColumn: "memberID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_images_members",
                table: "images");

            migrationBuilder.DropTable(
                name: "GoogleMapsApiUsage");

            migrationBuilder.DropPrimaryKey(
                name: "pk_images",
                table: "images");

            migrationBuilder.DropColumn(
                name: "householdAddress",
                table: "rentalApplications");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "LandlordHouseholdAddress",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "customName",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "WebUrl",
                table: "carouselImages");

            migrationBuilder.RenameIndex(
                name: "uq_images_imageGuid",
                table: "images",
                newName: "UQ_Images_ImageGuid");

            migrationBuilder.RenameIndex(
                name: "ix_images_entity",
                table: "images",
                newName: "IX_Images_EntityType_EntityId_Covering");

            migrationBuilder.AlterTable(
                name: "images",
                comment: "圖片表");

            migrationBuilder.AlterColumn<int>(
                name: "siteMessagesId",
                table: "siteMessages",
                type: "int",
                nullable: false,
                comment: "訊息ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "訊息ID")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "propertyID",
                table: "properties",
                type: "int",
                nullable: false,
                comment: "房源ID",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "房源ID")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "uploadedAt",
                table: "images",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "sysutcdatetime()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(DATEADD(HOUR, 8, sysutcdatetime()))");

            migrationBuilder.AlterColumn<Guid>(
                name: "imageGuid",
                table: "images",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "newsequentialid()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "(newsequentialid())");

            migrationBuilder.AlterColumn<string>(
                name: "storedFileName",
                table: "images",
                type: "varchar(41)",
                unicode: false,
                maxLength: 41,
                nullable: false,
                computedColumnSql: "lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end",
                stored: true,
                oldClrType: typeof(string),
                oldType: "varchar(41)",
                oldUnicode: false,
                oldMaxLength: 41,
                oldComputedColumnSql: "(lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end)",
                oldStored: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Images__7516F70C0063CFAC",
                table: "images",
                column: "imageId");

            migrationBuilder.CreateIndex(
                name: "ix_images_entity",
                table: "images",
                columns: new[] { "entityType", "entityId", "category", "displayOrder", "isActive" })
                .Annotation("SqlServer:Include", new[] { "imageGuid", "storedFileName", "fileSizeBytes", "width", "height", "uploadedAt", "mimeType", "originalFileName" });

            migrationBuilder.CreateIndex(
                name: "IX_Images_UQ_DisplayOrder",
                table: "images",
                columns: new[] { "entityType", "entityId", "category", "displayOrder" },
                unique: true,
                filter: "([DisplayOrder] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "uq_images_imageGuid",
                table: "images",
                column: "imageGuid",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_images_category",
                table: "images",
                sql: "[category] IN ('BedRoom','Kitchen','Living','Balcony','Avatar','Gallery','Product')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_images_entityType",
                table: "images",
                sql: "[entityType] IN ('Member','Property','Furniture','Announcement')");

            migrationBuilder.AddForeignKey(
                name: "fk_images_members",
                table: "images",
                column: "uploadedByMemberId",
                principalTable: "members",
                principalColumn: "memberID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
