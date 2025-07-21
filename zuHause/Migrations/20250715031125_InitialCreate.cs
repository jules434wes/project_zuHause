using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zuHause.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "seq_memberID");

            migrationBuilder.CreateTable(
                name: "adminMessageTemplates",
                columns: table => new
                {
                    templateID = table.Column<int>(type: "int", nullable: false, comment: "模板ID"),
                    categoryCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "模板類別代碼"),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "標題"),
                    templateContent = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "內容"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adminMessageTemplates", x => x.templateID);
                },
                comment: "後台訊息模板表");

            migrationBuilder.CreateTable(
                name: "adminRoles",
                columns: table => new
                {
                    roleCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "角色代碼"),
                    roleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "角色名稱"),
                    permissionsJSON = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "權限JSON"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adminRoles", x => x.roleCode);
                },
                comment: "管理員角色資料");

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    cityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cityCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, comment: "縣市代碼"),
                    cityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "縣市名稱"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "排序"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cities", x => x.cityId);
                },
                comment: "縣市代碼表");

            migrationBuilder.CreateTable(
                name: "contractTemplates",
                columns: table => new
                {
                    contractTemplateId = table.Column<int>(type: "int", nullable: false, comment: "範本ID"),
                    templateName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, comment: "範本名稱"),
                    templateContent = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "範本內容"),
                    uploadedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "範本上傳時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractTemplates", x => x.contractTemplateId);
                },
                comment: "合約範本表");

            migrationBuilder.CreateTable(
                name: "deliveryFeePlans",
                columns: table => new
                {
                    planId = table.Column<int>(type: "int", nullable: false, comment: "配送方案ID"),
                    planName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "方案名稱"),
                    baseFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false, comment: "基本費用"),
                    remoteAreaSurcharge = table.Column<decimal>(type: "decimal(10,2)", nullable: false, comment: "偏遠加收"),
                    currencyCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, comment: "幣別"),
                    startAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "生效時間"),
                    endAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "結束時間"),
                    maxWeightKG = table.Column<decimal>(type: "decimal(6,2)", nullable: true, comment: "重量上限KG"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deliveryFeePlans", x => x.planId);
                },
                comment: "家具配送費方案表");

            migrationBuilder.CreateTable(
                name: "furnitureCategories",
                columns: table => new
                {
                    furnitureCategoriesId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "分類ID"),
                    parentId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, comment: "上層分類ID"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "分類名稱"),
                    depth = table.Column<byte>(type: "tinyint", nullable: false, comment: "階層層級"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "顯示排序"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_furnitureCategories", x => x.furnitureCategoriesId);
                    table.ForeignKey(
                        name: "FK_furnCate_parent",
                        column: x => x.parentId,
                        principalTable: "furnitureCategories",
                        principalColumn: "furnitureCategoriesId");
                },
                comment: "家具商品分類表");

            migrationBuilder.CreateTable(
                name: "listingPlans",
                columns: table => new
                {
                    planId = table.Column<int>(type: "int", nullable: false, comment: "方案ID"),
                    planName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "方案名稱"),
                    pricePerDay = table.Column<decimal>(type: "decimal(10,2)", nullable: false, comment: "每日刊登費"),
                    currencyCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, comment: "幣別"),
                    minListingDays = table.Column<int>(type: "int", nullable: false, comment: "最小上架天數"),
                    startAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "生效時間"),
                    endAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "結束時間"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listingPlans", x => x.planId);
                },
                comment: "刊登費方案表");

            migrationBuilder.CreateTable(
                name: "memberTypes",
                columns: table => new
                {
                    memberTypeID = table.Column<int>(type: "int", nullable: false, comment: "身份ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    typeName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, comment: "身分名稱"),
                    description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "描述"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memberTypes", x => x.memberTypeID);
                },
                comment: "會員身分表");

            migrationBuilder.CreateTable(
                name: "pages",
                columns: table => new
                {
                    pageCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "頁面識別碼"),
                    pageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "頁面名稱"),
                    routePath = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false, comment: "路徑"),
                    moduleScope = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "模組"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "顯示順序"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pages", x => x.pageCode);
                },
                comment: "共用頁面代碼表");

            migrationBuilder.CreateTable(
                name: "propertyEquipmentCategories",
                columns: table => new
                {
                    categoryID = table.Column<int>(type: "int", nullable: false, comment: "設備分類ID"),
                    parentCategoryID = table.Column<int>(type: "int", nullable: true, comment: "上層分類ID"),
                    categoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "設備名稱"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propertyEquipmentCategories", x => x.categoryID);
                    table.ForeignKey(
                        name: "FK_propEquipCate_parent",
                        column: x => x.parentCategoryID,
                        principalTable: "propertyEquipmentCategories",
                        principalColumn: "categoryID");
                },
                comment: "房源設備分類資料表");

            migrationBuilder.CreateTable(
                name: "renterRequirementList",
                columns: table => new
                {
                    requirementID = table.Column<int>(type: "int", nullable: false, comment: "條件ID"),
                    requirementName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "條件名稱"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_renterRequirementList", x => x.requirementID);
                },
                comment: "尋租文章需求條件清單");

            migrationBuilder.CreateTable(
                name: "siteMessages",
                columns: table => new
                {
                    siteMessagesId = table.Column<int>(type: "int", nullable: false, comment: "訊息ID"),
                    title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true, comment: "標題"),
                    siteMessageContent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "內文"),
                    category = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false, comment: "分類"),
                    moduleScope = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false, comment: "模組"),
                    messageType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "訊息類型"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "顯示順序"),
                    startAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "開始時間"),
                    endAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "結束時間"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    attachmentUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "圖片/附件URL"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_siteMessages", x => x.siteMessagesId);
                },
                comment: "網站訊息表");

            migrationBuilder.CreateTable(
                name: "systemCodeCategories",
                columns: table => new
                {
                    codeCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "代碼類別"),
                    categoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "類別名稱"),
                    description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "描述"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_systemCodeCategories", x => x.codeCategory);
                },
                comment: "代碼類別表");

            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    adminID = table.Column<int>(type: "int", nullable: false, comment: "管理員ID"),
                    account = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "帳號"),
                    passwordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "密碼雜湊"),
                    passwordSalt = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "密碼 Salt"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "姓名"),
                    roleCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "角色代碼"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    lastLoginAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "最後登入時間"),
                    passwordUpdatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "密碼更新時間"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.adminID);
                    table.ForeignKey(
                        name: "FK_admins_role",
                        column: x => x.roleCode,
                        principalTable: "adminRoles",
                        principalColumn: "roleCode");
                },
                comment: "管理員資料");

            migrationBuilder.CreateTable(
                name: "districts",
                columns: table => new
                {
                    districtId = table.Column<int>(type: "int", nullable: false, comment: "鄉鎮區ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    districtCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, comment: "區代碼"),
                    districtName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "區名稱"),
                    cityCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, comment: "縣市代碼"),
                    zipCode = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false, comment: "郵遞區號"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "排序"),
                    cityId = table.Column<int>(type: "int", nullable: false, comment: "縣市ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_districts", x => x.districtId);
                    table.ForeignKey(
                        name: "FK_districts_cities",
                        column: x => x.cityId,
                        principalTable: "cities",
                        principalColumn: "cityId");
                },
                comment: "鄉鎮區代碼表");

            migrationBuilder.CreateTable(
                name: "furnitureProducts",
                columns: table => new
                {
                    furnitureProductId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "商品ID"),
                    categoryId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, comment: "分類ID"),
                    productName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "商品名稱"),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "商品描述"),
                    listPrice = table.Column<decimal>(type: "decimal(10,0)", nullable: false, comment: "原價"),
                    dailyRental = table.Column<decimal>(type: "decimal(10,0)", nullable: false, comment: "每日租金"),
                    imageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "商品圖片URL"),
                    status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "上下架狀態"),
                    listedAt = table.Column<DateOnly>(type: "date", nullable: true, comment: "上架時間"),
                    delistedAt = table.Column<DateOnly>(type: "date", nullable: true, comment: "下架時間"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_furnitureProducts", x => x.furnitureProductId);
                    table.ForeignKey(
                        name: "FK_furnitureProducts_category",
                        column: x => x.categoryId,
                        principalTable: "furnitureCategories",
                        principalColumn: "furnitureCategoriesId");
                },
                comment: "家具商品資料表");

            migrationBuilder.CreateTable(
                name: "carouselImages",
                columns: table => new
                {
                    carouselImageId = table.Column<int>(type: "int", nullable: false, comment: "圖片ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    imagesName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "名稱"),
                    category = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "分類"),
                    imageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "圖片URL"),
                    pageCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, comment: "頁面識別碼"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "顯示順序"),
                    startAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "開始時間"),
                    endAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "結束時間"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carouselImages", x => x.carouselImageId);
                    table.ForeignKey(
                        name: "FK_carouselImages_page",
                        column: x => x.pageCode,
                        principalTable: "pages",
                        principalColumn: "pageCode");
                },
                comment: "輪播圖片表");

            migrationBuilder.CreateTable(
                name: "messagePlacements",
                columns: table => new
                {
                    pageCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "頁面識別碼"),
                    sectionCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "區段代碼"),
                    messageID = table.Column<int>(type: "int", nullable: false, comment: "訊息ID"),
                    subtitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "小標題"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "顯示順序"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messagePlacements", x => new { x.pageCode, x.sectionCode });
                    table.ForeignKey(
                        name: "FK_msgPlacements_message",
                        column: x => x.messageID,
                        principalTable: "siteMessages",
                        principalColumn: "siteMessagesId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "訊息位置");

            migrationBuilder.CreateTable(
                name: "systemCodes",
                columns: table => new
                {
                    codeCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "代碼類別"),
                    code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "代碼"),
                    codeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "代碼名稱"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "排序"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_systemCodes", x => new { x.codeCategory, x.code });
                    table.ForeignKey(
                        name: "FK_systemCodes_category",
                        column: x => x.codeCategory,
                        principalTable: "systemCodeCategories",
                        principalColumn: "codeCategory");
                },
                comment: "代碼總表");

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    memberID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "(NEXT VALUE FOR [dbo].[seq_memberID])", comment: "會員ID"),
                    memberName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "會員姓名"),
                    gender = table.Column<byte>(type: "tinyint", nullable: false, comment: "性別"),
                    birthDate = table.Column<DateOnly>(type: "date", nullable: false, comment: "生日"),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "密碼雜湊"),
                    phoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "手機號碼"),
                    phoneVerifiedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "手機驗證通過時間"),
                    email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false, comment: "電子信箱"),
                    emailVerifiedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "Email驗證通過時間"),
                    identityVerifiedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "身份驗證通過時間"),
                    lastLoginAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "最後登入時間"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    primaryRentalCityID = table.Column<int>(type: "int", nullable: true, comment: "主要承租縣市ID"),
                    primaryRentalDistrictID = table.Column<int>(type: "int", nullable: true, comment: "主要承租區域ID"),
                    residenceCityID = table.Column<int>(type: "int", nullable: true, comment: "居住縣市ID"),
                    residenceDistrictID = table.Column<int>(type: "int", nullable: true, comment: "居住區域ID"),
                    memberTypeID = table.Column<int>(type: "int", nullable: true, comment: "會員身份別ID"),
                    isLandlord = table.Column<bool>(type: "bit", nullable: false, comment: "是否為房東"),
                    addressLine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "詳細地址"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    nationalIdNo = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: false, comment: "身分證號")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.memberID);
                    table.ForeignKey(
                        name: "FK_members_memberType",
                        column: x => x.memberTypeID,
                        principalTable: "memberTypes",
                        principalColumn: "memberTypeID");
                    table.ForeignKey(
                        name: "FK_members_primaryCity",
                        column: x => x.primaryRentalCityID,
                        principalTable: "cities",
                        principalColumn: "cityId");
                    table.ForeignKey(
                        name: "FK_members_primaryDistrict",
                        column: x => x.primaryRentalDistrictID,
                        principalTable: "districts",
                        principalColumn: "districtId");
                    table.ForeignKey(
                        name: "FK_members_resCity",
                        column: x => x.residenceCityID,
                        principalTable: "cities",
                        principalColumn: "cityId");
                    table.ForeignKey(
                        name: "FK_members_resDistrict",
                        column: x => x.residenceDistrictID,
                        principalTable: "districts",
                        principalColumn: "districtId");
                },
                comment: "會員資料表");

            migrationBuilder.CreateTable(
                name: "furnitureInventory",
                columns: table => new
                {
                    furnitureInventoryId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "庫存ID"),
                    productId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "商品ID"),
                    totalQuantity = table.Column<int>(type: "int", nullable: false, comment: "總庫存數量"),
                    rentedQuantity = table.Column<int>(type: "int", nullable: false, comment: "已出租數量"),
                    availableQuantity = table.Column<int>(type: "int", nullable: false, comment: "可用庫存"),
                    safetyStock = table.Column<int>(type: "int", nullable: false, comment: "安全庫存"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "最後更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_furnitureInventory", x => x.furnitureInventoryId);
                    table.ForeignKey(
                        name: "FK_furnitureInventory_product",
                        column: x => x.productId,
                        principalTable: "furnitureProducts",
                        principalColumn: "furnitureProductId");
                },
                comment: "家具庫存表");

            migrationBuilder.CreateTable(
                name: "inventoryEvents",
                columns: table => new
                {
                    furnitureInventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())", comment: "事件ID"),
                    productId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "商品ID"),
                    eventType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "事件類型"),
                    quantity = table.Column<int>(type: "int", nullable: false, comment: "異動數量"),
                    sourceType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, comment: "來源類型"),
                    sourceId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, comment: "來源編號"),
                    occurredAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "發生時間"),
                    recordedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "寫入時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventoryEvents", x => x.furnitureInventoryId);
                    table.ForeignKey(
                        name: "FK_inventoryEvents_product",
                        column: x => x.productId,
                        principalTable: "furnitureProducts",
                        principalColumn: "furnitureProductId");
                },
                comment: "庫存事件表");

            migrationBuilder.CreateTable(
                name: "memberVerifications",
                columns: table => new
                {
                    verificationID = table.Column<int>(type: "int", nullable: false, comment: "驗證ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    memberID = table.Column<int>(type: "int", nullable: false, comment: "會員ID"),
                    verificationTypeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "驗證類型代碼"),
                    verificationCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "驗證碼"),
                    isSuccessful = table.Column<bool>(type: "bit", nullable: false, comment: "是否驗證成功"),
                    sentAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "發送時間"),
                    verifiedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "驗證完成時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memberVerifications", x => x.verificationID);
                    table.ForeignKey(
                        name: "FK_memberVerifications_member",
                        column: x => x.memberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                },
                comment: "通訊相關驗證表");

            migrationBuilder.CreateTable(
                name: "properties",
                columns: table => new
                {
                    propertyID = table.Column<int>(type: "int", nullable: false, comment: "房源ID"),
                    landlordMemberID = table.Column<int>(type: "int", nullable: false, comment: "房東會員ID"),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "房源標題"),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "詳細描述"),
                    cityID = table.Column<int>(type: "int", nullable: false, comment: "縣市ID"),
                    districtID = table.Column<int>(type: "int", nullable: false, comment: "區域ID"),
                    addressLine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "詳細地址"),
                    monthlyRent = table.Column<decimal>(type: "decimal(10,2)", nullable: false, comment: "月租金"),
                    depositAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false, comment: "押金金額"),
                    depositMonths = table.Column<int>(type: "int", nullable: false, comment: "押金月數"),
                    roomCount = table.Column<int>(type: "int", nullable: false, comment: "房數"),
                    livingRoomCount = table.Column<int>(type: "int", nullable: false, comment: "廳數"),
                    bathroomCount = table.Column<int>(type: "int", nullable: false, comment: "衛數"),
                    currentFloor = table.Column<int>(type: "int", nullable: false, comment: "所在樓層"),
                    totalFloors = table.Column<int>(type: "int", nullable: false, comment: "總樓層"),
                    area = table.Column<decimal>(type: "decimal(8,2)", nullable: false, comment: "坪數"),
                    minimumRentalMonths = table.Column<int>(type: "int", nullable: false, comment: "最短租期(月)"),
                    specialRules = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "特殊守則"),
                    waterFeeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "水費計算方式"),
                    customWaterFee = table.Column<decimal>(type: "decimal(8,2)", nullable: true, comment: "自訂水費"),
                    electricityFeeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "電費計算方式"),
                    customElectricityFee = table.Column<decimal>(type: "decimal(8,2)", nullable: true, comment: "自訂電費"),
                    managementFeeIncluded = table.Column<bool>(type: "bit", nullable: false, comment: "管理費含租金"),
                    managementFeeAmount = table.Column<decimal>(type: "decimal(8,2)", nullable: true, comment: "管理費金額"),
                    parkingAvailable = table.Column<bool>(type: "bit", nullable: false, comment: "有停車位"),
                    parkingFeeRequired = table.Column<bool>(type: "bit", nullable: false, comment: "停車費需額外收費"),
                    parkingFeeAmount = table.Column<decimal>(type: "decimal(8,2)", nullable: true, comment: "停車位費用"),
                    cleaningFeeRequired = table.Column<bool>(type: "bit", nullable: false, comment: "清潔費需額外收費"),
                    cleaningFeeAmount = table.Column<decimal>(type: "decimal(8,2)", nullable: true, comment: "清潔費金額"),
                    listingDays = table.Column<int>(type: "int", nullable: true, comment: "刊登天數"),
                    listingFeeAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true, comment: "刊登費用"),
                    listingPlanID = table.Column<int>(type: "int", nullable: true, comment: "刊登費方案ID"),
                    isPaid = table.Column<bool>(type: "bit", nullable: false, comment: "付款狀態"),
                    paidAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "完成付款時間"),
                    expireAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "上架到期時間"),
                    propertyProofURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "房產證明文件URL"),
                    previewImageURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "預覽圖連結"),
                    statusCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "房源狀態代碼"),
                    publishedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "上架日期"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "最後修改日期"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_properties", x => x.propertyID);
                    table.ForeignKey(
                        name: "FK_properties_landlord",
                        column: x => x.landlordMemberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_properties_listingPlan",
                        column: x => x.listingPlanID,
                        principalTable: "listingPlans",
                        principalColumn: "planId");
                },
                comment: "房源資料表");

            migrationBuilder.CreateTable(
                name: "renterPosts",
                columns: table => new
                {
                    postID = table.Column<int>(type: "int", nullable: false, comment: "文章ID"),
                    memberID = table.Column<int>(type: "int", nullable: false, comment: "租客會員ID"),
                    cityID = table.Column<int>(type: "int", nullable: false, comment: "希望縣市ID"),
                    districtID = table.Column<int>(type: "int", nullable: false, comment: "希望區域ID"),
                    houseType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "房屋類型"),
                    budgetMin = table.Column<decimal>(type: "decimal(10,2)", nullable: false, comment: "預算下限"),
                    budgetMax = table.Column<decimal>(type: "decimal(10,2)", nullable: false, comment: "預算上限"),
                    postContent = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "詳細需求"),
                    viewCount = table.Column<int>(type: "int", nullable: false, comment: "瀏覽數"),
                    replyCount = table.Column<int>(type: "int", nullable: false, comment: "回覆數"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "發布時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "最後編輯時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否有效")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_renterPosts", x => x.postID);
                    table.ForeignKey(
                        name: "FK_renterPosts_member",
                        column: x => x.memberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                },
                comment: "尋租文章資料表");

            migrationBuilder.CreateTable(
                name: "searchHistory",
                columns: table => new
                {
                    historyID = table.Column<long>(type: "bigint", nullable: false, comment: "歷史ID"),
                    memberID = table.Column<int>(type: "int", nullable: false, comment: "會員ID"),
                    keyword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "搜尋關鍵字"),
                    resultCount = table.Column<int>(type: "int", nullable: true, comment: "結果筆數"),
                    deviceType = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true, comment: "裝置"),
                    ipAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true, comment: "IP"),
                    searchedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "搜尋時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_searchHistory", x => x.historyID);
                    table.ForeignKey(
                        name: "FK_searchHistory_member",
                        column: x => x.memberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                },
                comment: "搜索歷史表");

            migrationBuilder.CreateTable(
                name: "systemMessages",
                columns: table => new
                {
                    messageID = table.Column<int>(type: "int", nullable: false, comment: "系統訊息ID"),
                    categoryCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "訊息類別代碼"),
                    audienceTypeCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "接受者類型代碼"),
                    receiverID = table.Column<int>(type: "int", nullable: true, comment: "個別接受者ID"),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "發送標題"),
                    messageContent = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "發送內容"),
                    attachmentUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true, comment: "附件URL"),
                    sentAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "發送時間"),
                    adminID = table.Column<int>(type: "int", nullable: false, comment: "發送管理員ID"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否啟用"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_systemMessages", x => x.messageID);
                    table.ForeignKey(
                        name: "FK_systemMessages_admin",
                        column: x => x.adminID,
                        principalTable: "admins",
                        principalColumn: "adminID");
                    table.ForeignKey(
                        name: "FK_systemMessages_receiver",
                        column: x => x.receiverID,
                        principalTable: "members",
                        principalColumn: "memberID");
                },
                comment: "系統訊息表");

            migrationBuilder.CreateTable(
                name: "userNotifications",
                columns: table => new
                {
                    notificationID = table.Column<int>(type: "int", nullable: false, comment: "通知ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receiverID = table.Column<int>(type: "int", nullable: false, comment: "接收者ID"),
                    senderID = table.Column<int>(type: "int", nullable: true, comment: "發送者ID"),
                    typeCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "通知類型代碼"),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "訊息標題"),
                    notificationContent = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "訊息內容"),
                    linkUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true, comment: "相關連結URL"),
                    moduleCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, comment: "來源模組代碼"),
                    sourceEntityID = table.Column<int>(type: "int", nullable: true, comment: "來源資料ID"),
                    statusCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "狀態代碼"),
                    isRead = table.Column<bool>(type: "bit", nullable: false, comment: "是否已讀"),
                    readAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "閱讀時間"),
                    sentAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "發送時間"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userNotifications", x => x.notificationID);
                    table.ForeignKey(
                        name: "FK_userNotifications_receiver",
                        column: x => x.receiverID,
                        principalTable: "members",
                        principalColumn: "memberID");
                },
                comment: "使用者通知資料表");

            migrationBuilder.CreateTable(
                name: "userUploads",
                columns: table => new
                {
                    uploadID = table.Column<int>(type: "int", nullable: false, comment: "上傳ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    memberID = table.Column<int>(type: "int", nullable: false, comment: "會員ID"),
                    moduleCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "來源模組代碼"),
                    sourceEntityID = table.Column<int>(type: "int", nullable: true, comment: "來源資料ID"),
                    uploadTypeCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "上傳種類代碼"),
                    originalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "原始檔名"),
                    storedFileName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false, comment: "實體檔名"),
                    fileExt = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, comment: "副檔名"),
                    mimeType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true, comment: "MIME 類型"),
                    filePath = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false, comment: "檔案路徑"),
                    fileSize = table.Column<long>(type: "bigint", nullable: false, comment: "檔案大小(Byte)"),
                    checksum = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true, comment: "檔案雜湊"),
                    approvalID = table.Column<int>(type: "int", nullable: true, comment: "審核ID"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否有效"),
                    uploadedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "上傳時間"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userUploads", x => x.uploadID);
                    table.ForeignKey(
                        name: "FK_userUploads_member",
                        column: x => x.memberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                },
                comment: "會員上傳資料紀錄表");

            migrationBuilder.CreateTable(
                name: "approvals",
                columns: table => new
                {
                    approvalID = table.Column<int>(type: "int", nullable: false, comment: "審核ID"),
                    moduleCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "模組代碼"),
                    sourceID = table.Column<int>(type: "int", nullable: false, comment: "來源ID"),
                    applicantMemberID = table.Column<int>(type: "int", nullable: false, comment: "申請會員ID"),
                    statusCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "審核狀態碼"),
                    currentApproverID = table.Column<int>(type: "int", nullable: true, comment: "審核人員ID"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    statusCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, computedColumnSql: "(CONVERT([nvarchar](20),N'ApprovalStatus'))", stored: true, comment: "審核狀態分類")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approvals", x => x.approvalID);
                    table.ForeignKey(
                        name: "FK_approvals_Property",
                        column: x => x.sourceID,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                    table.ForeignKey(
                        name: "FK_approvals_applicant",
                        column: x => x.applicantMemberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_approvals_status",
                        columns: x => new { x.statusCategory, x.statusCode },
                        principalTable: "systemCodes",
                        principalColumns: new[] { "codeCategory", "code" });
                },
                comment: "審核主檔");

            migrationBuilder.CreateTable(
                name: "chatrooms",
                columns: table => new
                {
                    chatroomID = table.Column<int>(type: "int", nullable: false, comment: "聊天室ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    initiatorMemberID = table.Column<int>(type: "int", nullable: false, comment: "發起人會員ID"),
                    participantMemberID = table.Column<int>(type: "int", nullable: false, comment: "參與者會員ID"),
                    propertyID = table.Column<int>(type: "int", nullable: true, comment: "房源ID"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    lastMessageAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "最後訊息時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatrooms", x => x.chatroomID);
                    table.ForeignKey(
                        name: "FK_chatrooms_initiator",
                        column: x => x.initiatorMemberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_chatrooms_participant",
                        column: x => x.participantMemberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_chatrooms_property",
                        column: x => x.propertyID,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                },
                comment: "聊天室");

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    memberID = table.Column<int>(type: "int", nullable: false, comment: "會員ID"),
                    propertyID = table.Column<int>(type: "int", nullable: false, comment: "房源ID"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否有效"),
                    favoritedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "收藏時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => new { x.memberID, x.propertyID });
                    table.ForeignKey(
                        name: "FK_favorites_member",
                        column: x => x.memberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_favorites_property",
                        column: x => x.propertyID,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                },
                comment: "收藏表");

            migrationBuilder.CreateTable(
                name: "furnitureCarts",
                columns: table => new
                {
                    furnitureCartId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "購物車ID"),
                    memberId = table.Column<int>(type: "int", nullable: false, comment: "會員ID"),
                    propertyId = table.Column<int>(type: "int", nullable: true, comment: "房源ID"),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "狀態"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_furnitureCarts", x => x.furnitureCartId);
                    table.ForeignKey(
                        name: "FK_furnitureCarts_member",
                        column: x => x.memberId,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_furnitureCarts_property",
                        column: x => x.propertyId,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                },
                comment: "家具購物車表");

            migrationBuilder.CreateTable(
                name: "furnitureOrders",
                columns: table => new
                {
                    furnitureOrderId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "訂單ID"),
                    memberId = table.Column<int>(type: "int", nullable: false, comment: "會員ID"),
                    propertyId = table.Column<int>(type: "int", nullable: true, comment: "房源ID"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "成立時間"),
                    totalAmount = table.Column<decimal>(type: "decimal(12,0)", nullable: false, comment: "總金額"),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "訂單狀態"),
                    paymentStatus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "付款狀態"),
                    contractLink = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "合約連結"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_furnitureOrders", x => x.furnitureOrderId);
                    table.ForeignKey(
                        name: "FK_furnitureOrders_member",
                        column: x => x.memberId,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_furnitureOrders_property",
                        column: x => x.propertyId,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                },
                comment: "家具訂單查詢表");

            migrationBuilder.CreateTable(
                name: "propertyComplaints",
                columns: table => new
                {
                    complaintId = table.Column<int>(type: "int", nullable: false, comment: "投訴ID"),
                    complainantId = table.Column<int>(type: "int", nullable: false, comment: "投訴人ID"),
                    propertyId = table.Column<int>(type: "int", nullable: false, comment: "房源ID"),
                    landlordId = table.Column<int>(type: "int", nullable: false, comment: "被投訴房東ID"),
                    complaintContent = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "投訴內容"),
                    statusCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "處理狀態代碼"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    resolvedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "結案時間"),
                    internalNote = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "內部註記"),
                    handledBy = table.Column<int>(type: "int", nullable: true, comment: "處理人員ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propertyComplaints", x => x.complaintId);
                    table.ForeignKey(
                        name: "FK_propComplaints_complainant",
                        column: x => x.complainantId,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_propComplaints_landlord",
                        column: x => x.landlordId,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_propComplaints_property",
                        column: x => x.propertyId,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                },
                comment: "房源投訴表");

            migrationBuilder.CreateTable(
                name: "propertyEquipmentRelations",
                columns: table => new
                {
                    relationID = table.Column<int>(type: "int", nullable: false, comment: "關聯ID"),
                    propertyID = table.Column<int>(type: "int", nullable: false, comment: "房源ID"),
                    categoryID = table.Column<int>(type: "int", nullable: false, comment: "設備分類ID"),
                    quantity = table.Column<int>(type: "int", nullable: false, comment: "數量"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propertyEquipmentRelations", x => x.relationID);
                    table.ForeignKey(
                        name: "FK_propEquipRel_category",
                        column: x => x.categoryID,
                        principalTable: "propertyEquipmentCategories",
                        principalColumn: "categoryID");
                    table.ForeignKey(
                        name: "FK_propEquipRel_property",
                        column: x => x.propertyID,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                },
                comment: "房源設備關聯資料表");

            migrationBuilder.CreateTable(
                name: "propertyImages",
                columns: table => new
                {
                    imageID = table.Column<int>(type: "int", nullable: false, comment: "圖片ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    propertyID = table.Column<int>(type: "int", nullable: false, comment: "房源ID"),
                    imagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "圖片路徑"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "顯示順序"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propertyImages", x => x.imageID);
                    table.ForeignKey(
                        name: "FK_propertyImages_property",
                        column: x => x.propertyID,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                },
                comment: "房源圖片表");

            migrationBuilder.CreateTable(
                name: "rentalApplications",
                columns: table => new
                {
                    applicationID = table.Column<int>(type: "int", nullable: false, comment: "申請ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    applicationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "申請類型"),
                    memberID = table.Column<int>(type: "int", nullable: false, comment: "申請會員ID"),
                    propertyID = table.Column<int>(type: "int", nullable: false, comment: "房源ID"),
                    message = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true, comment: "申請留言"),
                    scheduleTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "預約看房時間"),
                    rentalStartDate = table.Column<DateOnly>(type: "date", nullable: true, comment: "租期開始"),
                    rentalEndDate = table.Column<DateOnly>(type: "date", nullable: true, comment: "租期結束"),
                    currentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "目前狀態"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "申請時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    deletedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "刪除時間"),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否有效")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rentalApplications", x => x.applicationID);
                    table.ForeignKey(
                        name: "FK_rentalApplications_member",
                        column: x => x.memberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_rentalApplications_property",
                        column: x => x.propertyID,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                },
                comment: "租賃/看房申請資料表");

            migrationBuilder.CreateTable(
                name: "renterPostReplies",
                columns: table => new
                {
                    replyID = table.Column<int>(type: "int", nullable: false, comment: "回覆ID"),
                    postID = table.Column<int>(type: "int", nullable: false, comment: "文章ID"),
                    landlordMemberID = table.Column<int>(type: "int", nullable: false, comment: "房東會員ID"),
                    replyContent = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "回覆內容"),
                    suggestPropertyID = table.Column<int>(type: "int", nullable: true, comment: "推薦房源ID"),
                    isWithinBudget = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "符合預算"),
                    tags = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "符合條件標籤"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "回覆時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_renterPostReplies", x => x.replyID);
                    table.ForeignKey(
                        name: "FK_renterPostReplies_landlord",
                        column: x => x.landlordMemberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_renterPostReplies_post",
                        column: x => x.postID,
                        principalTable: "renterPosts",
                        principalColumn: "postID");
                },
                comment: "尋租文章回覆資料表");

            migrationBuilder.CreateTable(
                name: "renterRequirementRelations",
                columns: table => new
                {
                    relationID = table.Column<int>(type: "int", nullable: false, comment: "關聯ID"),
                    postID = table.Column<int>(type: "int", nullable: false, comment: "文章ID"),
                    requirementID = table.Column<int>(type: "int", nullable: false, comment: "條件ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_renterRequirementRelations", x => x.relationID);
                    table.ForeignKey(
                        name: "FK_rentReqRel_post",
                        column: x => x.postID,
                        principalTable: "renterPosts",
                        principalColumn: "postID");
                    table.ForeignKey(
                        name: "FK_rentReqRel_requirement",
                        column: x => x.requirementID,
                        principalTable: "renterRequirementList",
                        principalColumn: "requirementID");
                },
                comment: "尋租條件關聯");

            migrationBuilder.CreateTable(
                name: "fileApprovals",
                columns: table => new
                {
                    approvalID = table.Column<int>(type: "int", nullable: false, comment: "審核ID"),
                    uploadID = table.Column<int>(type: "int", nullable: false, comment: "上傳ID"),
                    memberID = table.Column<int>(type: "int", nullable: false, comment: "會員ID"),
                    statusCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "審核狀態代碼"),
                    resultDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "審核說明"),
                    appliedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "申請時間"),
                    reviewedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "審核時間"),
                    reviewerAdminID = table.Column<int>(type: "int", nullable: true, comment: "審核人員ID"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fileApprovals", x => x.approvalID);
                    table.ForeignKey(
                        name: "FK_fileApprovals_upload",
                        column: x => x.uploadID,
                        principalTable: "userUploads",
                        principalColumn: "uploadID");
                },
                comment: "檔案審核");

            migrationBuilder.CreateTable(
                name: "approvalItems",
                columns: table => new
                {
                    approvalItemID = table.Column<int>(type: "int", nullable: false, comment: "審核明細ID"),
                    approvalID = table.Column<int>(type: "int", nullable: false, comment: "審核ID"),
                    actionBy = table.Column<int>(type: "int", nullable: false, comment: "操作者ID"),
                    actionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "操作類型"),
                    actionNote = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "操作備註"),
                    snapshotJSON = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "審核快照JSON"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    actionCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, computedColumnSql: "(CONVERT([nvarchar](20),N'ApprovalAction'))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approvalItems", x => x.approvalItemID);
                    table.ForeignKey(
                        name: "FK_approvalItems_action",
                        columns: x => new { x.actionCategory, x.actionType },
                        principalTable: "systemCodes",
                        principalColumns: new[] { "codeCategory", "code" });
                    table.ForeignKey(
                        name: "FK_approvalItems_approval",
                        column: x => x.approvalID,
                        principalTable: "approvals",
                        principalColumn: "approvalID");
                },
                comment: "審核明細");

            migrationBuilder.CreateTable(
                name: "chatroomMessages",
                columns: table => new
                {
                    messageID = table.Column<int>(type: "int", nullable: false, comment: "訊息ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chatroomID = table.Column<int>(type: "int", nullable: false, comment: "聊天室ID"),
                    senderMemberID = table.Column<int>(type: "int", nullable: false, comment: "發送者會員ID"),
                    messageContent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, comment: "內容"),
                    isRead = table.Column<bool>(type: "bit", nullable: false, comment: "是否已讀"),
                    sentAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "傳送時間"),
                    readAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "已讀時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatroomMessages", x => x.messageID);
                    table.ForeignKey(
                        name: "FK_chatroomMessages_chatroom",
                        column: x => x.chatroomID,
                        principalTable: "chatrooms",
                        principalColumn: "chatroomID");
                    table.ForeignKey(
                        name: "FK_chatroomMessages_sender",
                        column: x => x.senderMemberID,
                        principalTable: "members",
                        principalColumn: "memberID");
                },
                comment: "聊天室訊息");

            migrationBuilder.CreateTable(
                name: "furnitureCartItems",
                columns: table => new
                {
                    cartItemId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "明細ID"),
                    cartId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "購物車ID"),
                    productId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "商品ID"),
                    quantity = table.Column<int>(type: "int", nullable: false, comment: "數量"),
                    rentalDays = table.Column<int>(type: "int", nullable: false, comment: "租期(天)"),
                    unitPriceSnapshot = table.Column<decimal>(type: "decimal(10,0)", nullable: false, comment: "單價快照"),
                    subTotal = table.Column<decimal>(type: "decimal(12,0)", nullable: false, comment: "小計"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_furnitureCartItems", x => x.cartItemId);
                    table.ForeignKey(
                        name: "FK_furnitureCartItems_cart",
                        column: x => x.cartId,
                        principalTable: "furnitureCarts",
                        principalColumn: "furnitureCartId");
                    table.ForeignKey(
                        name: "FK_furnitureCartItems_product",
                        column: x => x.productId,
                        principalTable: "furnitureProducts",
                        principalColumn: "furnitureProductId");
                },
                comment: "家具購物車明細表");

            migrationBuilder.CreateTable(
                name: "furnitureOrderHistory",
                columns: table => new
                {
                    furnitureOrderHistoryId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "流水號"),
                    orderId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "訂單ID"),
                    productId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "商品ID"),
                    productNameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "商品名稱快照"),
                    descriptionSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "商品描述快照"),
                    quantity = table.Column<int>(type: "int", nullable: false, comment: "數量"),
                    dailyRentalSnapshot = table.Column<decimal>(type: "decimal(10,0)", nullable: false, comment: "單日租金快照"),
                    rentalStart = table.Column<DateOnly>(type: "date", nullable: false, comment: "租借開始日"),
                    rentalEnd = table.Column<DateOnly>(type: "date", nullable: false, comment: "租借結束日"),
                    subTotal = table.Column<decimal>(type: "decimal(12,0)", nullable: false, comment: "小計"),
                    itemStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "明細狀態"),
                    returnedAt = table.Column<DateOnly>(type: "date", nullable: true, comment: "實際歸還日期"),
                    damageNote = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "損壞說明"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_furnitureOrderHistory", x => x.furnitureOrderHistoryId);
                    table.ForeignKey(
                        name: "FK_furnitureOrderHistory_order",
                        column: x => x.orderId,
                        principalTable: "furnitureOrders",
                        principalColumn: "furnitureOrderId");
                    table.ForeignKey(
                        name: "FK_furnitureOrderHistory_product",
                        column: x => x.productId,
                        principalTable: "furnitureProducts",
                        principalColumn: "furnitureProductId");
                },
                comment: "家具歷史訂單清單");

            migrationBuilder.CreateTable(
                name: "furnitureOrderItems",
                columns: table => new
                {
                    furnitureOrderItemId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "明細ID"),
                    orderId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "訂單ID"),
                    productId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "商品ID"),
                    quantity = table.Column<int>(type: "int", nullable: false, comment: "數量"),
                    dailyRentalSnapshot = table.Column<decimal>(type: "decimal(10,0)", nullable: false, comment: "單日租金快照"),
                    rentalDays = table.Column<int>(type: "int", nullable: false, comment: "租期(天)"),
                    subTotal = table.Column<decimal>(type: "decimal(12,0)", nullable: false, comment: "小計"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_furnitureOrderItems", x => x.furnitureOrderItemId);
                    table.ForeignKey(
                        name: "FK_furnitureOrderItems_order",
                        column: x => x.orderId,
                        principalTable: "furnitureOrders",
                        principalColumn: "furnitureOrderId");
                    table.ForeignKey(
                        name: "FK_furnitureOrderItems_product",
                        column: x => x.productId,
                        principalTable: "furnitureProducts",
                        principalColumn: "furnitureProductId");
                },
                comment: "家具訂單查詢明細表");

            migrationBuilder.CreateTable(
                name: "furnitureRentalContracts",
                columns: table => new
                {
                    furnitureRentalContractsId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "合約ID"),
                    orderId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "訂單ID"),
                    contractJson = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "合約 JSON"),
                    contractLink = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "簽章連結"),
                    deliveryDate = table.Column<DateOnly>(type: "date", nullable: true, comment: "配送日期"),
                    terminationPolicy = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "退租政策"),
                    signStatus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, comment: "簽署狀態"),
                    signedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "簽署完成時間"),
                    eSignatureValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "電子簽章值"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "簽約日期"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_furnitureRentalContracts", x => x.furnitureRentalContractsId);
                    table.ForeignKey(
                        name: "FK_furnitureRentalContracts_order",
                        column: x => x.orderId,
                        principalTable: "furnitureOrders",
                        principalColumn: "furnitureOrderId");
                },
                comment: "家具租賃合約表");

            migrationBuilder.CreateTable(
                name: "orderEvents",
                columns: table => new
                {
                    orderEventsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())", comment: "事件ID"),
                    orderId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, comment: "訂單ID"),
                    eventType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, comment: "事件類型"),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "事件內容"),
                    occurredAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "發生時間"),
                    recordedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "寫入時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderEvents", x => x.orderEventsId);
                    table.ForeignKey(
                        name: "FK_orderEvents_order",
                        column: x => x.orderId,
                        principalTable: "furnitureOrders",
                        principalColumn: "furnitureOrderId");
                },
                comment: "訂單事件表");

            migrationBuilder.CreateTable(
                name: "applicationStatusLogs",
                columns: table => new
                {
                    statusLogID = table.Column<int>(type: "int", nullable: false, comment: "狀態歷程ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    applicationID = table.Column<int>(type: "int", nullable: false, comment: "申請ID"),
                    statusCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "狀態代碼"),
                    changedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, comment: "進入狀態時間"),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "備註"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applicationStatusLogs", x => x.statusLogID);
                    table.ForeignKey(
                        name: "FK_appStatusLogs_application",
                        column: x => x.applicationID,
                        principalTable: "rentalApplications",
                        principalColumn: "applicationID");
                },
                comment: "申請狀態歷程表");

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    contractId = table.Column<int>(type: "int", nullable: false, comment: "合約ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rentalApplicationId = table.Column<int>(type: "int", nullable: true, comment: "申請編號ID"),
                    templateId = table.Column<int>(type: "int", nullable: true, comment: "合約範本編號"),
                    startDate = table.Column<DateOnly>(type: "date", nullable: false, comment: "合約起始日"),
                    endDate = table.Column<DateOnly>(type: "date", nullable: true, comment: "合約結束日"),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "合約狀態"),
                    courtJurisdiction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "管轄法院"),
                    isSublettable = table.Column<bool>(type: "bit", nullable: false, comment: "是否可轉租"),
                    usagePurpose = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, comment: "使用目的"),
                    depositAmount = table.Column<int>(type: "int", nullable: true, comment: "押金金額"),
                    cleaningFee = table.Column<int>(type: "int", nullable: true, comment: "清潔費"),
                    managementFee = table.Column<int>(type: "int", nullable: true, comment: "管理費"),
                    parkingFee = table.Column<int>(type: "int", nullable: true, comment: "停車費"),
                    penaltyAmount = table.Column<int>(type: "int", nullable: true, comment: "違約金金額"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.contractId);
                    table.ForeignKey(
                        name: "FK_contracts_rentalApp",
                        column: x => x.rentalApplicationId,
                        principalTable: "rentalApplications",
                        principalColumn: "applicationID");
                    table.ForeignKey(
                        name: "FK_contracts_template",
                        column: x => x.templateId,
                        principalTable: "contractTemplates",
                        principalColumn: "contractTemplateId");
                },
                comment: "合約欄位儲存表");

            migrationBuilder.CreateTable(
                name: "contractComments",
                columns: table => new
                {
                    contractCommentId = table.Column<int>(type: "int", nullable: false, comment: "合約備註ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    contractId = table.Column<int>(type: "int", nullable: false, comment: "合約ID"),
                    commentType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "備註類型"),
                    commentText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "內容"),
                    createdById = table.Column<int>(type: "int", nullable: false, comment: "建立者"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractComments", x => x.contractCommentId);
                    table.ForeignKey(
                        name: "FK_contractComments_contract",
                        column: x => x.contractId,
                        principalTable: "contracts",
                        principalColumn: "contractId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "合約備註表");

            migrationBuilder.CreateTable(
                name: "contractCustomFields",
                columns: table => new
                {
                    contractCustomFieldId = table.Column<int>(type: "int", nullable: false, comment: "動態欄位ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    contractId = table.Column<int>(type: "int", nullable: false, comment: "合約ID"),
                    fieldKey = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, comment: "動態欄位名稱"),
                    fieldValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "動態欄位值"),
                    fieldType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, comment: "動態欄位型態"),
                    displayOrder = table.Column<int>(type: "int", nullable: false, comment: "顯示順序")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractCustomFields", x => x.contractCustomFieldId);
                    table.ForeignKey(
                        name: "FK_contractCustomFields_contract",
                        column: x => x.contractId,
                        principalTable: "contracts",
                        principalColumn: "contractId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "合約附表(動態欄位)");

            migrationBuilder.CreateTable(
                name: "contractFurnitureItems",
                columns: table => new
                {
                    contractFurnitureItemId = table.Column<int>(type: "int", nullable: false, comment: "家具清單ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    contractId = table.Column<int>(type: "int", nullable: false, comment: "合約ID"),
                    furnitureName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "家具名稱"),
                    furnitureCondition = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true, comment: "家具狀況"),
                    quantity = table.Column<int>(type: "int", nullable: false, comment: "數量"),
                    repairChargeOwner = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, comment: "修繕費負責人"),
                    repairResponsibility = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, comment: "維修權責"),
                    unitPrice = table.Column<int>(type: "int", nullable: true, comment: "單價"),
                    amount = table.Column<int>(type: "int", nullable: true, comment: "小計")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractFurnitureItems", x => x.contractFurnitureItemId);
                    table.ForeignKey(
                        name: "FK_contractFurnitureItems_contract",
                        column: x => x.contractId,
                        principalTable: "contracts",
                        principalColumn: "contractId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "合約內容家具表");

            migrationBuilder.CreateTable(
                name: "contractSignatures",
                columns: table => new
                {
                    idcontractSignatureId = table.Column<int>(type: "int", nullable: false, comment: "電子簽名ID")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    contractId = table.Column<int>(type: "int", nullable: false, comment: "合約ID"),
                    signerId = table.Column<int>(type: "int", nullable: false, comment: "簽約人ID"),
                    signerRole = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "簽署人身份"),
                    signMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, comment: "簽名方式"),
                    signatureFileUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "簽名檔URL"),
                    signVerifyInfo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "簽署驗證資訊"),
                    signedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "時間戳")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractSignatures", x => x.idcontractSignatureId);
                    table.ForeignKey(
                        name: "FK_contractSignatures_contract",
                        column: x => x.contractId,
                        principalTable: "contracts",
                        principalColumn: "contractId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_contractSignatures_member",
                        column: x => x.signerId,
                        principalTable: "members",
                        principalColumn: "memberID");
                },
                comment: "電子簽名儲存表");

            migrationBuilder.CreateTable(
                name: "customerServiceTickets",
                columns: table => new
                {
                    ticketId = table.Column<int>(type: "int", nullable: false, comment: "客服單ID"),
                    memberId = table.Column<int>(type: "int", nullable: false, comment: "使用者ID"),
                    propertyId = table.Column<int>(type: "int", nullable: true, comment: "房源ID"),
                    contractId = table.Column<int>(type: "int", nullable: true, comment: "租約ID"),
                    furnitureOrderId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, comment: "家具訂單ID"),
                    subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "主旨"),
                    categoryCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "主分類代碼"),
                    ticketContent = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "需求內容"),
                    replyContent = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "客服最後回覆"),
                    statusCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, comment: "狀態代碼"),
                    createdAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "建立時間"),
                    replyAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true, comment: "最後回覆時間"),
                    updatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(CONVERT([datetime2](0),sysdatetime()))", comment: "更新時間"),
                    handledBy = table.Column<int>(type: "int", nullable: true, comment: "客服人員ID"),
                    isResolved = table.Column<bool>(type: "bit", nullable: false, comment: "是否結案")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customerServiceTickets", x => x.ticketId);
                    table.ForeignKey(
                        name: "FK_custTickets_contract",
                        column: x => x.contractId,
                        principalTable: "contracts",
                        principalColumn: "contractId");
                    table.ForeignKey(
                        name: "FK_custTickets_furnOrder",
                        column: x => x.furnitureOrderId,
                        principalTable: "furnitureOrders",
                        principalColumn: "furnitureOrderId");
                    table.ForeignKey(
                        name: "FK_custTickets_member",
                        column: x => x.memberId,
                        principalTable: "members",
                        principalColumn: "memberID");
                    table.ForeignKey(
                        name: "FK_custTickets_property",
                        column: x => x.propertyId,
                        principalTable: "properties",
                        principalColumn: "propertyID");
                },
                comment: "客服聯繫表");

            migrationBuilder.CreateIndex(
                name: "IX_admins_roleCode",
                table: "admins",
                column: "roleCode");

            migrationBuilder.CreateIndex(
                name: "IX_applicationStatusLogs_applicationID",
                table: "applicationStatusLogs",
                column: "applicationID");

            migrationBuilder.CreateIndex(
                name: "IX_approvalItems_actionCategory_actionType",
                table: "approvalItems",
                columns: new[] { "actionCategory", "actionType" });

            migrationBuilder.CreateIndex(
                name: "IX_approvalItems_approvalID",
                table: "approvalItems",
                column: "approvalID");

            migrationBuilder.CreateIndex(
                name: "IX_approvals_applicantMemberID",
                table: "approvals",
                column: "applicantMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_approvals_sourceID",
                table: "approvals",
                column: "sourceID");

            migrationBuilder.CreateIndex(
                name: "IX_approvals_statusCategory_statusCode",
                table: "approvals",
                columns: new[] { "statusCategory", "statusCode" });

            migrationBuilder.CreateIndex(
                name: "UQ_approvals_module_source",
                table: "approvals",
                columns: new[] { "moduleCode", "sourceID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_carouselImages_active_time",
                table: "carouselImages",
                columns: new[] { "startAt", "endAt" });

            migrationBuilder.CreateIndex(
                name: "IX_carouselImages_pageCode",
                table: "carouselImages",
                column: "pageCode");

            migrationBuilder.CreateIndex(
                name: "IX_chatroomMessages_chat_time",
                table: "chatroomMessages",
                columns: new[] { "chatroomID", "sentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_chatroomMessages_senderMemberID",
                table: "chatroomMessages",
                column: "senderMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_chatrooms_lastmsg",
                table: "chatrooms",
                column: "lastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_chatrooms_participantMemberID",
                table: "chatrooms",
                column: "participantMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_chatrooms_propertyID",
                table: "chatrooms",
                column: "propertyID");

            migrationBuilder.CreateIndex(
                name: "UQ_chatrooms_members",
                table: "chatrooms",
                columns: new[] { "initiatorMemberID", "participantMemberID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_cities_cityCode",
                table: "cities",
                column: "cityCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contractComments_contractId",
                table: "contractComments",
                column: "contractId");

            migrationBuilder.CreateIndex(
                name: "IX_contractCustomFields_contractId",
                table: "contractCustomFields",
                column: "contractId");

            migrationBuilder.CreateIndex(
                name: "IX_contractFurnitureItems_contractId",
                table: "contractFurnitureItems",
                column: "contractId");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_rentalApplicationId",
                table: "contracts",
                column: "rentalApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_status_dates",
                table: "contracts",
                columns: new[] { "status", "startDate", "endDate" });

            migrationBuilder.CreateIndex(
                name: "IX_contracts_templateId",
                table: "contracts",
                column: "templateId");

            migrationBuilder.CreateIndex(
                name: "IX_contractSignatures_contractId",
                table: "contractSignatures",
                column: "contractId");

            migrationBuilder.CreateIndex(
                name: "IX_contractSignatures_signerId",
                table: "contractSignatures",
                column: "signerId");

            migrationBuilder.CreateIndex(
                name: "IX_customerServiceTickets_contractId",
                table: "customerServiceTickets",
                column: "contractId");

            migrationBuilder.CreateIndex(
                name: "IX_customerServiceTickets_furnitureOrderId",
                table: "customerServiceTickets",
                column: "furnitureOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_customerServiceTickets_memberId",
                table: "customerServiceTickets",
                column: "memberId");

            migrationBuilder.CreateIndex(
                name: "IX_customerServiceTickets_propertyId",
                table: "customerServiceTickets",
                column: "propertyId");

            migrationBuilder.CreateIndex(
                name: "UQ_districts_city_districtCode",
                table: "districts",
                columns: new[] { "cityId", "districtCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_favorites_property",
                table: "favorites",
                column: "propertyID");

            migrationBuilder.CreateIndex(
                name: "IX_fileApprovals_uploadID",
                table: "fileApprovals",
                column: "uploadID");

            migrationBuilder.CreateIndex(
                name: "IX_furnitureCartItems_cartId",
                table: "furnitureCartItems",
                column: "cartId");

            migrationBuilder.CreateIndex(
                name: "IX_furnitureCartItems_productId",
                table: "furnitureCartItems",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_furnitureCarts_memberId",
                table: "furnitureCarts",
                column: "memberId");

            migrationBuilder.CreateIndex(
                name: "IX_furnitureCarts_propertyId",
                table: "furnitureCarts",
                column: "propertyId");

            migrationBuilder.CreateIndex(
                name: "IX_furnitureCategories_parentId",
                table: "furnitureCategories",
                column: "parentId");

            migrationBuilder.CreateIndex(
                name: "IX_furnInventory_available",
                table: "furnitureInventory",
                columns: new[] { "productId", "availableQuantity" });

            migrationBuilder.CreateIndex(
                name: "IX_furnitureOrderHistory_orderId",
                table: "furnitureOrderHistory",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_furnitureOrderHistory_productId",
                table: "furnitureOrderHistory",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_furnitureOrderItems_orderId",
                table: "furnitureOrderItems",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_furnitureOrderItems_productId",
                table: "furnitureOrderItems",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_furnitureOrders_propertyId",
                table: "furnitureOrders",
                column: "propertyId");

            migrationBuilder.CreateIndex(
                name: "IX_furnOrders_member_status",
                table: "furnitureOrders",
                columns: new[] { "memberId", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_furnitureProducts_categoryId",
                table: "furnitureProducts",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_furnProducts_status_cat",
                table: "furnitureProducts",
                columns: new[] { "status", "categoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_furnitureRentalContracts_orderId",
                table: "furnitureRentalContracts",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_inventoryEvents_productId",
                table: "inventoryEvents",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_members_email",
                table: "members",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_members_memberTypeID",
                table: "members",
                column: "memberTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_members_nationalIdNo",
                table: "members",
                column: "nationalIdNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_members_phone",
                table: "members",
                column: "phoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_members_primaryRentalCityID",
                table: "members",
                column: "primaryRentalCityID");

            migrationBuilder.CreateIndex(
                name: "IX_members_primaryRentalDistrictID",
                table: "members",
                column: "primaryRentalDistrictID");

            migrationBuilder.CreateIndex(
                name: "IX_members_residenceCityID",
                table: "members",
                column: "residenceCityID");

            migrationBuilder.CreateIndex(
                name: "IX_members_residenceDistrictID",
                table: "members",
                column: "residenceDistrictID");

            migrationBuilder.CreateIndex(
                name: "IX_memberVerifications_memberID",
                table: "memberVerifications",
                column: "memberID");

            migrationBuilder.CreateIndex(
                name: "IX_messagePlacements_messageID",
                table: "messagePlacements",
                column: "messageID");

            migrationBuilder.CreateIndex(
                name: "IX_orderEvents_orderId",
                table: "orderEvents",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_properties_landlordMemberID",
                table: "properties",
                column: "landlordMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_properties_listingPlanID",
                table: "properties",
                column: "listingPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_properties_location",
                table: "properties",
                columns: new[] { "cityID", "districtID" });

            migrationBuilder.CreateIndex(
                name: "IX_properties_status",
                table: "properties",
                column: "statusCode");

            migrationBuilder.CreateIndex(
                name: "IX_propertyComplaints_complainantId",
                table: "propertyComplaints",
                column: "complainantId");

            migrationBuilder.CreateIndex(
                name: "IX_propertyComplaints_landlordId",
                table: "propertyComplaints",
                column: "landlordId");

            migrationBuilder.CreateIndex(
                name: "IX_propertyComplaints_propertyId",
                table: "propertyComplaints",
                column: "propertyId");

            migrationBuilder.CreateIndex(
                name: "IX_propertyEquipmentCategories_parentCategoryID",
                table: "propertyEquipmentCategories",
                column: "parentCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_propertyEquipmentRelations_categoryID",
                table: "propertyEquipmentRelations",
                column: "categoryID");

            migrationBuilder.CreateIndex(
                name: "IX_propertyEquipmentRelations_propertyID",
                table: "propertyEquipmentRelations",
                column: "propertyID");

            migrationBuilder.CreateIndex(
                name: "UQ_propertyImages_property_order",
                table: "propertyImages",
                columns: new[] { "propertyID", "displayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rentalApplications_memberID",
                table: "rentalApplications",
                column: "memberID");

            migrationBuilder.CreateIndex(
                name: "IX_rentalApplications_propertyID",
                table: "rentalApplications",
                column: "propertyID");

            migrationBuilder.CreateIndex(
                name: "IX_renterPostReplies_landlordMemberID",
                table: "renterPostReplies",
                column: "landlordMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_renterPostReplies_postID",
                table: "renterPostReplies",
                column: "postID");

            migrationBuilder.CreateIndex(
                name: "IX_renterPosts_location",
                table: "renterPosts",
                columns: new[] { "cityID", "districtID", "houseType" });

            migrationBuilder.CreateIndex(
                name: "IX_renterPosts_memberID",
                table: "renterPosts",
                column: "memberID");

            migrationBuilder.CreateIndex(
                name: "IX_renterRequirementRelations_requirementID",
                table: "renterRequirementRelations",
                column: "requirementID");

            migrationBuilder.CreateIndex(
                name: "UQ_rentReqRel_post_req",
                table: "renterRequirementRelations",
                columns: new[] { "postID", "requirementID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_searchHistory_member_time",
                table: "searchHistory",
                columns: new[] { "memberID", "searchedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_systemMessages_active_category",
                table: "systemMessages",
                columns: new[] { "isActive", "categoryCode" });

            migrationBuilder.CreateIndex(
                name: "IX_systemMessages_adminID",
                table: "systemMessages",
                column: "adminID");

            migrationBuilder.CreateIndex(
                name: "IX_systemMessages_receiverID",
                table: "systemMessages",
                column: "receiverID");

            migrationBuilder.CreateIndex(
                name: "IX_userNotifications_receiver_isRead",
                table: "userNotifications",
                columns: new[] { "receiverID", "isRead" });

            migrationBuilder.CreateIndex(
                name: "IX_userUploads_memberID",
                table: "userUploads",
                column: "memberID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adminMessageTemplates");

            migrationBuilder.DropTable(
                name: "applicationStatusLogs");

            migrationBuilder.DropTable(
                name: "approvalItems");

            migrationBuilder.DropTable(
                name: "carouselImages");

            migrationBuilder.DropTable(
                name: "chatroomMessages");

            migrationBuilder.DropTable(
                name: "contractComments");

            migrationBuilder.DropTable(
                name: "contractCustomFields");

            migrationBuilder.DropTable(
                name: "contractFurnitureItems");

            migrationBuilder.DropTable(
                name: "contractSignatures");

            migrationBuilder.DropTable(
                name: "customerServiceTickets");

            migrationBuilder.DropTable(
                name: "deliveryFeePlans");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "fileApprovals");

            migrationBuilder.DropTable(
                name: "furnitureCartItems");

            migrationBuilder.DropTable(
                name: "furnitureInventory");

            migrationBuilder.DropTable(
                name: "furnitureOrderHistory");

            migrationBuilder.DropTable(
                name: "furnitureOrderItems");

            migrationBuilder.DropTable(
                name: "furnitureRentalContracts");

            migrationBuilder.DropTable(
                name: "inventoryEvents");

            migrationBuilder.DropTable(
                name: "memberVerifications");

            migrationBuilder.DropTable(
                name: "messagePlacements");

            migrationBuilder.DropTable(
                name: "orderEvents");

            migrationBuilder.DropTable(
                name: "propertyComplaints");

            migrationBuilder.DropTable(
                name: "propertyEquipmentRelations");

            migrationBuilder.DropTable(
                name: "propertyImages");

            migrationBuilder.DropTable(
                name: "renterPostReplies");

            migrationBuilder.DropTable(
                name: "renterRequirementRelations");

            migrationBuilder.DropTable(
                name: "searchHistory");

            migrationBuilder.DropTable(
                name: "systemMessages");

            migrationBuilder.DropTable(
                name: "userNotifications");

            migrationBuilder.DropTable(
                name: "approvals");

            migrationBuilder.DropTable(
                name: "pages");

            migrationBuilder.DropTable(
                name: "chatrooms");

            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "userUploads");

            migrationBuilder.DropTable(
                name: "furnitureCarts");

            migrationBuilder.DropTable(
                name: "furnitureProducts");

            migrationBuilder.DropTable(
                name: "siteMessages");

            migrationBuilder.DropTable(
                name: "furnitureOrders");

            migrationBuilder.DropTable(
                name: "propertyEquipmentCategories");

            migrationBuilder.DropTable(
                name: "renterPosts");

            migrationBuilder.DropTable(
                name: "renterRequirementList");

            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "systemCodes");

            migrationBuilder.DropTable(
                name: "rentalApplications");

            migrationBuilder.DropTable(
                name: "contractTemplates");

            migrationBuilder.DropTable(
                name: "furnitureCategories");

            migrationBuilder.DropTable(
                name: "adminRoles");

            migrationBuilder.DropTable(
                name: "systemCodeCategories");

            migrationBuilder.DropTable(
                name: "properties");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "listingPlans");

            migrationBuilder.DropTable(
                name: "memberTypes");

            migrationBuilder.DropTable(
                name: "districts");

            migrationBuilder.DropTable(
                name: "cities");

            migrationBuilder.DropSequence(
                name: "seq_memberID");
        }
    }
}
