using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zuHause.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_images",
                table: "images");

            migrationBuilder.RenameIndex(
                name: "uq_images_imageGuid",
                table: "images",
                newName: "UQ_Images_ImageGuid");

            migrationBuilder.AlterTable(
                name: "images",
                comment: "圖片表");

            migrationBuilder.AddColumn<byte[]>(
                name: "rowVersion",
                table: "images",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

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
                oldType: "nvarchar(max)",
                oldComputedColumnSql: "lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end",
                oldStored: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Images__7516F70C0063CFAC",
                table: "images",
                column: "imageId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_EntityType_EntityId_Covering",
                table: "images",
                columns: new[] { "entityType", "entityId", "category", "displayOrder", "isActive" });

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

            migrationBuilder.CreateIndex(
                name: "UQ_approvals_module_source",
                table: "approvals",
                columns: new[] { "moduleCode", "sourcePropertyID" },
                unique: true,
                filter: "[sourcePropertyID] IS NOT NULL")
                .Annotation("SqlServer:FillFactor", 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__Images__7516F70C0063CFAC",
                table: "images");

            migrationBuilder.DropIndex(
                name: "IX_Images_EntityType_EntityId_Covering",
                table: "images");

            migrationBuilder.DropIndex(
                name: "IX_Images_UQ_DisplayOrder",
                table: "images");

            migrationBuilder.DropIndex(
                name: "uq_images_imageGuid",
                table: "images");

            migrationBuilder.DropIndex(
                name: "UQ_approvals_module_source",
                table: "approvals");

            migrationBuilder.DropColumn(
                name: "rowVersion",
                table: "images");

            migrationBuilder.RenameIndex(
                name: "UQ_Images_ImageGuid",
                table: "images",
                newName: "uq_images_imageGuid");

            migrationBuilder.AlterTable(
                name: "images",
                oldComment: "圖片表");

            migrationBuilder.AlterColumn<string>(
                name: "storedFileName",
                table: "images",
                type: "nvarchar(max)",
                nullable: false,
                computedColumnSql: "lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end",
                stored: true,
                oldClrType: typeof(string),
                oldType: "varchar(41)",
                oldUnicode: false,
                oldMaxLength: 41,
                oldComputedColumnSql: "lower(CONVERT([char](36),[imageGuid]))+case [mimeType] when 'image/webp' then '.webp' when 'image/jpeg' then '.jpg' when 'image/png' then '.png' else '.bin' end",
                oldStored: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_images",
                table: "images",
                column: "imageId");
        }
    }
}
