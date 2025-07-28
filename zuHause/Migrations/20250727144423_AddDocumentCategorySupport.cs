using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zuHause.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentCategorySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "properties",
                type: "decimal(11,8)",
                precision: 11,
                scale: 8,
                nullable: true,
                comment: "經度",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "properties",
                type: "decimal(10,8)",
                precision: 10,
                scale: 8,
                nullable: true,
                comment: "緯度",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "properties",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(11,8)",
                oldPrecision: 11,
                oldScale: 8,
                oldNullable: true,
                oldComment: "經度");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "properties",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,8)",
                oldPrecision: 10,
                oldScale: 8,
                oldNullable: true,
                oldComment: "緯度");
        }
    }
}
