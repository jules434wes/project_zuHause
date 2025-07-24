-- DROP SCHEMA dbo;

CREATE SCHEMA dbo;
-- zuHause.dbo.[__EFMigrationsHistory] definition

-- Drop table

-- DROP TABLE zuHause.dbo.[__EFMigrationsHistory];

CREATE TABLE zuHause.dbo.[__EFMigrationsHistory] (
	MigrationId nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	ProductVersion nvarchar(32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
);


-- zuHause.dbo.adminMessageTemplates definition

-- Drop table

-- DROP TABLE zuHause.dbo.adminMessageTemplates;

CREATE TABLE zuHause.dbo.adminMessageTemplates (
	templateID int IDENTITY(101,1) NOT NULL,
	categoryCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	title nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	templateContent nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_adminMessageTemplates PRIMARY KEY (templateID)
);


-- zuHause.dbo.adminRoles definition

-- Drop table

-- DROP TABLE zuHause.dbo.adminRoles;

CREATE TABLE zuHause.dbo.adminRoles (
	roleCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	roleName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	permissionsJSON nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_adminRoles PRIMARY KEY (roleCode)
);


-- zuHause.dbo.cities definition

-- Drop table

-- DROP TABLE zuHause.dbo.cities;

CREATE TABLE zuHause.dbo.cities (
	cityCode varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	cityName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	cityId int IDENTITY(1,1) NOT NULL,
	CONSTRAINT PK_cities PRIMARY KEY (cityId),
	CONSTRAINT UQ_cities_cityCode UNIQUE (cityCode)
);


-- zuHause.dbo.contractTemplates definition

-- Drop table

-- DROP TABLE zuHause.dbo.contractTemplates;

CREATE TABLE zuHause.dbo.contractTemplates (
	contractTemplateId int NOT NULL,
	templateName nvarchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	templateContent nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	uploadedAt datetime2(0) NOT NULL,
	CONSTRAINT PK_contractTemplates PRIMARY KEY (contractTemplateId)
);


-- zuHause.dbo.deliveryFeePlans definition

-- Drop table

-- DROP TABLE zuHause.dbo.deliveryFeePlans;

CREATE TABLE zuHause.dbo.deliveryFeePlans (
	planId int NOT NULL,
	planName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	baseFee decimal(10,2) NOT NULL,
	remoteAreaSurcharge decimal(10,2) NOT NULL,
	currencyCode varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	startAt datetime2(0) NOT NULL,
	endAt datetime2(0) NULL,
	maxWeightKG decimal(6,2) NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_deliveryFeePlans PRIMARY KEY (planId)
);


-- zuHause.dbo.listingPlans definition

-- Drop table

-- DROP TABLE zuHause.dbo.listingPlans;

CREATE TABLE zuHause.dbo.listingPlans (
	planName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	pricePerDay decimal(10,2) NOT NULL,
	currencyCode varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	minListingDays int NOT NULL,
	startAt datetime2(0) NOT NULL,
	endAt datetime2(0) NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	planId int IDENTITY(1,1) NOT NULL,
	CONSTRAINT PK_listingPlans PRIMARY KEY (planId)
);


-- zuHause.dbo.memberTypes definition

-- Drop table

-- DROP TABLE zuHause.dbo.memberTypes;

CREATE TABLE zuHause.dbo.memberTypes (
	memberTypeID int IDENTITY(1,1) NOT NULL,
	typeName nvarchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	description nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_memberTypes PRIMARY KEY (memberTypeID)
);


-- zuHause.dbo.pages definition

-- Drop table

-- DROP TABLE zuHause.dbo.pages;

CREATE TABLE zuHause.dbo.pages (
	pageCode varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	pageName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	routePath varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	moduleScope varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_pages PRIMARY KEY (pageCode)
);


-- zuHause.dbo.renterRequirementList definition

-- Drop table

-- DROP TABLE zuHause.dbo.renterRequirementList;

CREATE TABLE zuHause.dbo.renterRequirementList (
	requirementID int NOT NULL,
	requirementName nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK_renterRequirementList PRIMARY KEY (requirementID)
);


-- zuHause.dbo.siteMessages definition

-- Drop table

-- DROP TABLE zuHause.dbo.siteMessages;

CREATE TABLE zuHause.dbo.siteMessages (
	siteMessagesId int NOT NULL,
	title nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	siteMessageContent nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	category varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	moduleScope varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	messageType varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	startAt datetime2(0) NULL,
	endAt datetime2(0) NULL,
	isActive bit DEFAULT 1 NOT NULL,
	attachmentUrl nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_siteMessages PRIMARY KEY (siteMessagesId)
);


-- zuHause.dbo.systemCodeCategories definition

-- Drop table

-- DROP TABLE zuHause.dbo.systemCodeCategories;

CREATE TABLE zuHause.dbo.systemCodeCategories (
	codeCategory nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	categoryName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	description nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_systemCodeCategories PRIMARY KEY (codeCategory)
);


-- zuHause.dbo.admins definition

-- Drop table

-- DROP TABLE zuHause.dbo.admins;

CREATE TABLE zuHause.dbo.admins (
	adminID int NOT NULL,
	account varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	passwordHash nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	passwordSalt nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	name nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	roleCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	lastLoginAt datetime2(0) NULL,
	passwordUpdatedAt datetime2(0) NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_admins PRIMARY KEY (adminID),
	CONSTRAINT FK_admins_role FOREIGN KEY (roleCode) REFERENCES zuHause.dbo.adminRoles(roleCode)
);


-- zuHause.dbo.carouselImages definition

-- Drop table

-- DROP TABLE zuHause.dbo.carouselImages;

CREATE TABLE zuHause.dbo.carouselImages (
	carouselImageId int IDENTITY(1,1) NOT NULL,
	imagesName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	category varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	imageUrl nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	pageCode varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	startAt datetime2(0) NULL,
	endAt datetime2(0) NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_carouselImages PRIMARY KEY (carouselImageId),
	CONSTRAINT FK_carouselImages_page FOREIGN KEY (pageCode) REFERENCES zuHause.dbo.pages(pageCode)
);
 CREATE NONCLUSTERED INDEX IX_carouselImages_active_time ON zuHause.dbo.carouselImages (  startAt ASC  , endAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.districts definition

-- Drop table

-- DROP TABLE zuHause.dbo.districts;

CREATE TABLE zuHause.dbo.districts (
	districtCode varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	districtName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	cityCode varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	zipCode varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	districtId int IDENTITY(1,1) NOT NULL,
	cityId int NOT NULL,
	CONSTRAINT PK_districts PRIMARY KEY (districtId),
	CONSTRAINT UQ_districts_city_districtCode UNIQUE (cityId,districtCode),
	CONSTRAINT FK_districts_cities FOREIGN KEY (cityId) REFERENCES zuHause.dbo.cities(cityId)
);


-- zuHause.dbo.furnitureCategories definition

-- Drop table

-- DROP TABLE zuHause.dbo.furnitureCategories;

CREATE TABLE zuHause.dbo.furnitureCategories (
	furnitureCategoriesId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	parentId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	name nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[depth] tinyint DEFAULT 0 NOT NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_furnitureCategories PRIMARY KEY (furnitureCategoriesId),
	CONSTRAINT FK_furnCate_parent FOREIGN KEY (parentId) REFERENCES zuHause.dbo.furnitureCategories(furnitureCategoriesId)
);


-- zuHause.dbo.furnitureProducts definition

-- Drop table

-- DROP TABLE zuHause.dbo.furnitureProducts;

CREATE TABLE zuHause.dbo.furnitureProducts (
	furnitureProductId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	categoryId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	productName nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	description nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	listPrice decimal(10,0) NOT NULL,
	dailyRental decimal(10,0) NOT NULL,
	imageUrl nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	status bit DEFAULT 1 NOT NULL,
	listedAt date NULL,
	delistedAt date NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_furnitureProducts PRIMARY KEY (furnitureProductId),
	CONSTRAINT FK_furnitureProducts_category FOREIGN KEY (categoryId) REFERENCES zuHause.dbo.furnitureCategories(furnitureCategoriesId)
);
 CREATE NONCLUSTERED INDEX IX_furnProducts_status_cat ON zuHause.dbo.furnitureProducts (  status ASC  , categoryId ASC  )  
	 INCLUDE ( listedAt ) 
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.inventoryEvents definition

-- Drop table

-- DROP TABLE zuHause.dbo.inventoryEvents;

CREATE TABLE zuHause.dbo.inventoryEvents (
	furnitureInventoryId uniqueidentifier DEFAULT newid() NOT NULL,
	productId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	eventType varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	quantity int NOT NULL,
	sourceType varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	sourceId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	occurredAt datetime2(0) NOT NULL,
	recordedAt datetime2(0) NOT NULL,
	CONSTRAINT PK_inventoryEvents PRIMARY KEY (furnitureInventoryId),
	CONSTRAINT FK_inventoryEvents_product FOREIGN KEY (productId) REFERENCES zuHause.dbo.furnitureProducts(furnitureProductId)
);


-- zuHause.dbo.members definition

-- Drop table

-- DROP TABLE zuHause.dbo.members;

CREATE TABLE zuHause.dbo.members (
	memberID int DEFAULT NEXT VALUE FOR [dbo].[seq_memberID] NOT NULL,
	memberName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	gender tinyint NOT NULL,
	birthDate date NOT NULL,
	password nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	phoneNumber nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	phoneVerifiedAt datetime2(0) NULL,
	email nvarchar(254) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	emailVerifiedAt datetime2(0) NULL,
	identityVerifiedAt datetime2(0) NULL,
	lastLoginAt datetime2(0) NULL,
	isActive bit DEFAULT 1 NOT NULL,
	primaryRentalCityID int NULL,
	primaryRentalDistrictID int NULL,
	residenceCityID int NULL,
	residenceDistrictID int NULL,
	memberTypeID int NULL,
	isLandlord bit DEFAULT 0 NOT NULL,
	addressLine nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	nationalIdNo char(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CONSTRAINT PK_members PRIMARY KEY (memberID),
	CONSTRAINT FK_members_memberType FOREIGN KEY (memberTypeID) REFERENCES zuHause.dbo.memberTypes(memberTypeID),
	CONSTRAINT FK_members_primaryCity FOREIGN KEY (primaryRentalCityID) REFERENCES zuHause.dbo.cities(cityId),
	CONSTRAINT FK_members_primaryDistrict FOREIGN KEY (primaryRentalDistrictID) REFERENCES zuHause.dbo.districts(districtId),
	CONSTRAINT FK_members_resCity FOREIGN KEY (residenceCityID) REFERENCES zuHause.dbo.cities(cityId),
	CONSTRAINT FK_members_resDistrict FOREIGN KEY (residenceDistrictID) REFERENCES zuHause.dbo.districts(districtId)
);
 CREATE NONCLUSTERED INDEX IX_members_email ON zuHause.dbo.members (  email ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_members_phone ON zuHause.dbo.members (  phoneNumber ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
ALTER TABLE zuHause.dbo.members WITH NOCHECK ADD CONSTRAINT CK_members_nationalId_format CHECK (([nationalIdNo] like '[A-Z][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'));


-- zuHause.dbo.messagePlacements definition

-- Drop table

-- DROP TABLE zuHause.dbo.messagePlacements;

CREATE TABLE zuHause.dbo.messagePlacements (
	pageCode varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	sectionCode varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	messageID int NOT NULL,
	subtitle nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_messagePlacements PRIMARY KEY (pageCode,sectionCode),
	CONSTRAINT FK_msgPlacements_message FOREIGN KEY (messageID) REFERENCES zuHause.dbo.siteMessages(siteMessagesId) ON DELETE CASCADE
);


-- zuHause.dbo.properties definition

-- Drop table

-- DROP TABLE zuHause.dbo.properties;

CREATE TABLE zuHause.dbo.properties (
	propertyID int NOT NULL,
	landlordMemberID int NOT NULL,
	title nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	description nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	cityID int NOT NULL,
	districtID int NOT NULL,
	addressLine nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	monthlyRent decimal(10,2) NOT NULL,
	depositAmount decimal(10,2) NOT NULL,
	depositMonths int NOT NULL,
	roomCount int NOT NULL,
	livingRoomCount int NOT NULL,
	bathroomCount int NOT NULL,
	currentFloor int NOT NULL,
	totalFloors int NOT NULL,
	area decimal(8,2) NOT NULL,
	minimumRentalMonths int NOT NULL,
	specialRules nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	waterFeeType nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	customWaterFee decimal(8,2) NULL,
	electricityFeeType nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	customElectricityFee decimal(8,2) NULL,
	managementFeeIncluded bit DEFAULT 0 NOT NULL,
	managementFeeAmount decimal(8,2) NULL,
	parkingAvailable bit DEFAULT 0 NOT NULL,
	parkingFeeRequired bit DEFAULT 0 NOT NULL,
	parkingFeeAmount decimal(8,2) NULL,
	cleaningFeeRequired bit DEFAULT 0 NOT NULL,
	cleaningFeeAmount decimal(8,2) NULL,
	listingDays int NULL,
	listingFeeAmount decimal(10,2) NULL,
	listingPlanID int NULL,
	isPaid bit DEFAULT 0 NOT NULL,
	paidAt datetime2(0) NULL,
	expireAt datetime2(0) NULL,
	propertyProofURL nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	previewImageURL nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	statusCode nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	publishedAt datetime2(0) NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_properties PRIMARY KEY (propertyID),
	CONSTRAINT FK_properties_landlord FOREIGN KEY (landlordMemberID) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_properties_listingPlan FOREIGN KEY (listingPlanID) REFERENCES zuHause.dbo.listingPlans(planId)
);
 CREATE NONCLUSTERED INDEX IX_properties_location ON zuHause.dbo.properties (  cityID ASC  , districtID ASC  )  
	 INCLUDE ( monthlyRent , statusCode ) 
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_properties_status ON zuHause.dbo.properties (  statusCode ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
ALTER TABLE zuHause.dbo.properties WITH NOCHECK ADD CONSTRAINT CK_properties_IsPaid_01 CHECK (([IsPaid]=(1) OR [IsPaid]=(0)));
ALTER TABLE zuHause.dbo.properties WITH NOCHECK ADD CONSTRAINT CK_properties_PaidAt_Logic CHECK (([IsPaid]=(1) AND [PaidAt] IS NOT NULL OR [IsPaid]=(0)));
ALTER TABLE zuHause.dbo.properties WITH NOCHECK ADD CONSTRAINT CK_properties_ExpireAfterPublish CHECK (([ExpireAt] IS NULL OR [ExpireAt]>[PublishedAt]));
ALTER TABLE zuHause.dbo.properties WITH NOCHECK ADD CONSTRAINT CK_properties_ListingFee_Positive CHECK (([ListingFeeAmount]>=(0)));


-- zuHause.dbo.propertyComplaints definition

-- Drop table

-- DROP TABLE zuHause.dbo.propertyComplaints;

CREATE TABLE zuHause.dbo.propertyComplaints (
	complaintId int IDENTITY(301,1) NOT NULL,
	complainantId int NOT NULL,
	propertyId int NOT NULL,
	landlordId int NOT NULL,
	complaintContent nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	statusCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	resolvedAt datetime2(0) NULL,
	internalNote nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	handledBy int NULL,
	CONSTRAINT PK_propertyComplaints PRIMARY KEY (complaintId),
	CONSTRAINT FK_propComplaints_complainant FOREIGN KEY (complainantId) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_propComplaints_landlord FOREIGN KEY (landlordId) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_propComplaints_property FOREIGN KEY (propertyId) REFERENCES zuHause.dbo.properties(propertyID)
);
 CREATE NONCLUSTERED INDEX IX_propertyComplaints_complainantId ON zuHause.dbo.propertyComplaints (  complainantId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_propertyComplaints_createdAt ON zuHause.dbo.propertyComplaints (  createdAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_propertyComplaints_handledBy ON zuHause.dbo.propertyComplaints (  handledBy ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_propertyComplaints_landlordId ON zuHause.dbo.propertyComplaints (  landlordId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_propertyComplaints_propertyId ON zuHause.dbo.propertyComplaints (  propertyId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_propertyComplaints_resolvedAt ON zuHause.dbo.propertyComplaints (  resolvedAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_propertyComplaints_statusCode ON zuHause.dbo.propertyComplaints (  statusCode ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.propertyEquipmentCategories definition

-- Drop table

-- DROP TABLE zuHause.dbo.propertyEquipmentCategories;

CREATE TABLE zuHause.dbo.propertyEquipmentCategories (
	parentCategoryID int NULL,
	categoryName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	categoryID int IDENTITY(1,1) NOT NULL,
	CONSTRAINT PK_propertyEquipmentCategories PRIMARY KEY (categoryID),
	CONSTRAINT FK_propEquipCate_parent FOREIGN KEY (parentCategoryID) REFERENCES zuHause.dbo.propertyEquipmentCategories(categoryID)
);


-- zuHause.dbo.propertyEquipmentRelations definition

-- Drop table

-- DROP TABLE zuHause.dbo.propertyEquipmentRelations;

CREATE TABLE zuHause.dbo.propertyEquipmentRelations (
	relationID int NOT NULL,
	propertyID int NOT NULL,
	categoryID int NOT NULL,
	quantity int NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_propertyEquipmentRelations PRIMARY KEY (relationID),
	CONSTRAINT FK_propEquipRel_category FOREIGN KEY (categoryID) REFERENCES zuHause.dbo.propertyEquipmentCategories(categoryID),
	CONSTRAINT FK_propEquipRel_property FOREIGN KEY (propertyID) REFERENCES zuHause.dbo.properties(propertyID)
);


-- zuHause.dbo.propertyImages definition

-- Drop table

-- DROP TABLE zuHause.dbo.propertyImages;

CREATE TABLE zuHause.dbo.propertyImages (
	imageID int IDENTITY(1,1) NOT NULL,
	propertyID int NOT NULL,
	imagePath nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_propertyImages PRIMARY KEY (imageID),
	CONSTRAINT FK_propertyImages_property FOREIGN KEY (propertyID) REFERENCES zuHause.dbo.properties(propertyID)
);
 CREATE UNIQUE NONCLUSTERED INDEX UQ_propertyImages_property_order ON zuHause.dbo.propertyImages (  propertyID ASC  , displayOrder ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.rentalApplications definition

-- Drop table

-- DROP TABLE zuHause.dbo.rentalApplications;

CREATE TABLE zuHause.dbo.rentalApplications (
	applicationID int IDENTITY(1,1) NOT NULL,
	applicationType nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	memberID int NOT NULL,
	propertyID int NOT NULL,
	message nvarchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	scheduleTime datetime2(0) NULL,
	rentalStartDate date NULL,
	rentalEndDate date NULL,
	currentStatus nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	isActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK_rentalApplications PRIMARY KEY (applicationID),
	CONSTRAINT FK_rentalApplications_member FOREIGN KEY (memberID) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_rentalApplications_property FOREIGN KEY (propertyID) REFERENCES zuHause.dbo.properties(propertyID)
);


-- zuHause.dbo.renterPosts definition

-- Drop table

-- DROP TABLE zuHause.dbo.renterPosts;

CREATE TABLE zuHause.dbo.renterPosts (
	postID int NOT NULL,
	memberID int NOT NULL,
	cityID int NOT NULL,
	districtID int NOT NULL,
	houseType nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	budgetMin decimal(10,2) NOT NULL,
	budgetMax decimal(10,2) NOT NULL,
	postContent nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	viewCount int DEFAULT 0 NOT NULL,
	replyCount int DEFAULT 0 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	isActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK_renterPosts PRIMARY KEY (postID),
	CONSTRAINT FK_renterPosts_member FOREIGN KEY (memberID) REFERENCES zuHause.dbo.members(memberID)
);
 CREATE NONCLUSTERED INDEX IX_renterPosts_location ON zuHause.dbo.renterPosts (  cityID ASC  , districtID ASC  , houseType ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.renterRequirementRelations definition

-- Drop table

-- DROP TABLE zuHause.dbo.renterRequirementRelations;

CREATE TABLE zuHause.dbo.renterRequirementRelations (
	relationID int NOT NULL,
	postID int NOT NULL,
	requirementID int NOT NULL,
	CONSTRAINT PK_renterRequirementRelations PRIMARY KEY (relationID),
	CONSTRAINT FK_rentReqRel_post FOREIGN KEY (postID) REFERENCES zuHause.dbo.renterPosts(postID),
	CONSTRAINT FK_rentReqRel_requirement FOREIGN KEY (requirementID) REFERENCES zuHause.dbo.renterRequirementList(requirementID)
);
 CREATE UNIQUE NONCLUSTERED INDEX UQ_rentReqRel_post_req ON zuHause.dbo.renterRequirementRelations (  postID ASC  , requirementID ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.searchHistory definition

-- Drop table

-- DROP TABLE zuHause.dbo.searchHistory;

CREATE TABLE zuHause.dbo.searchHistory (
	historyID bigint NOT NULL,
	memberID int NOT NULL,
	keyword nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	resultCount int NULL,
	deviceType varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	ipAddress varchar(45) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	searchedAt datetime2(0) NOT NULL,
	CONSTRAINT PK_searchHistory PRIMARY KEY (historyID),
	CONSTRAINT FK_searchHistory_member FOREIGN KEY (memberID) REFERENCES zuHause.dbo.members(memberID)
);
 CREATE NONCLUSTERED INDEX IX_searchHistory_member_time ON zuHause.dbo.searchHistory (  memberID ASC  , searchedAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.systemCodes definition

-- Drop table

-- DROP TABLE zuHause.dbo.systemCodes;

CREATE TABLE zuHause.dbo.systemCodes (
	codeCategory nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	code nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	codeName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_systemCodes PRIMARY KEY (codeCategory,code),
	CONSTRAINT FK_systemCodes_category FOREIGN KEY (codeCategory) REFERENCES zuHause.dbo.systemCodeCategories(codeCategory)
);


-- zuHause.dbo.systemMessages definition

-- Drop table

-- DROP TABLE zuHause.dbo.systemMessages;

CREATE TABLE zuHause.dbo.systemMessages (
	messageID int IDENTITY(401,1) NOT NULL,
	categoryCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	audienceTypeCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	receiverID int NULL,
	title nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	messageContent nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	attachmentUrl varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	sentAt datetime2(0) NOT NULL,
	adminID int NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_systemMessages PRIMARY KEY (messageID),
	CONSTRAINT FK_systemMessages_admin FOREIGN KEY (adminID) REFERENCES zuHause.dbo.admins(adminID),
	CONSTRAINT FK_systemMessages_receiver FOREIGN KEY (receiverID) REFERENCES zuHause.dbo.members(memberID)
);
 CREATE NONCLUSTERED INDEX IX_systemMessages_active_category ON zuHause.dbo.systemMessages (  isActive ASC  , categoryCode ASC  )  
	 INCLUDE ( sentAt ) 
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_systemMessages_adminID ON zuHause.dbo.systemMessages (  adminID ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_systemMessages_audienceTypeCode ON zuHause.dbo.systemMessages (  audienceTypeCode ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_systemMessages_deletedAt ON zuHause.dbo.systemMessages (  deletedAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_systemMessages_receiverID ON zuHause.dbo.systemMessages (  receiverID ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_systemMessages_sentAt ON zuHause.dbo.systemMessages (  sentAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.userNotifications definition

-- Drop table

-- DROP TABLE zuHause.dbo.userNotifications;

CREATE TABLE zuHause.dbo.userNotifications (
	notificationID int IDENTITY(1,1) NOT NULL,
	receiverID int NOT NULL,
	senderID int NULL,
	typeCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	title nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	notificationContent nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	linkUrl varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	moduleCode varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	sourceEntityID int NULL,
	statusCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isRead bit DEFAULT 0 NOT NULL,
	readAt datetime2(0) NULL,
	sentAt datetime2(0) NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_userNotifications PRIMARY KEY (notificationID),
	CONSTRAINT FK_userNotifications_receiver FOREIGN KEY (receiverID) REFERENCES zuHause.dbo.members(memberID)
);
 CREATE NONCLUSTERED INDEX IX_userNotifications_receiver_isRead ON zuHause.dbo.userNotifications (  receiverID ASC  , isRead ASC  )  
	 INCLUDE ( sentAt , title ) 
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.userUploads definition

-- Drop table

-- DROP TABLE zuHause.dbo.userUploads;

CREATE TABLE zuHause.dbo.userUploads (
	uploadID int IDENTITY(1,1) NOT NULL,
	memberID int NOT NULL,
	moduleCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	sourceEntityID int NULL,
	uploadTypeCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	originalFileName nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	storedFileName varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	fileExt varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	mimeType varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	filePath varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	fileSize bigint NOT NULL,
	checksum varchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	approvalID int NULL,
	isActive bit DEFAULT 1 NOT NULL,
	uploadedAt datetime2(0) NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_userUploads PRIMARY KEY (uploadID),
	CONSTRAINT FK_userUploads_member FOREIGN KEY (memberID) REFERENCES zuHause.dbo.members(memberID)
);


-- zuHause.dbo.applicationStatusLogs definition

-- Drop table

-- DROP TABLE zuHause.dbo.applicationStatusLogs;

CREATE TABLE zuHause.dbo.applicationStatusLogs (
	statusLogID int IDENTITY(1,1) NOT NULL,
	applicationID int NOT NULL,
	statusCode nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	changedAt datetime2(0) NOT NULL,
	note nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_applicationStatusLogs PRIMARY KEY (statusLogID),
	CONSTRAINT FK_appStatusLogs_application FOREIGN KEY (applicationID) REFERENCES zuHause.dbo.rentalApplications(applicationID)
);


-- zuHause.dbo.approvals definition

-- Drop table

-- DROP TABLE zuHause.dbo.approvals;

CREATE TABLE zuHause.dbo.approvals (
	approvalID int IDENTITY(701,1) NOT NULL,
	moduleCode nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	sourcePropertyID int NULL,
	applicantMemberID int NOT NULL,
	statusCode nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	currentApproverID int NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	statusCategory AS (CONVERT([nvarchar](20),N'APPROVAL_STATUS')) PERSISTED,
	CONSTRAINT PK_approvals PRIMARY KEY (approvalID),
	CONSTRAINT FK_approvals_Property FOREIGN KEY (sourcePropertyID) REFERENCES zuHause.dbo.properties(propertyID),
	CONSTRAINT FK_approvals_applicant FOREIGN KEY (applicantMemberID) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_approvals_status FOREIGN KEY (statusCategory,statusCode) REFERENCES zuHause.dbo.systemCodes(codeCategory,code)
);
 CREATE NONCLUSTERED INDEX IX_approvals_applicantMemberID ON zuHause.dbo.approvals (  applicantMemberID ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_approvals_createdAt ON zuHause.dbo.approvals (  createdAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_approvals_currentApproverID ON zuHause.dbo.approvals (  currentApproverID ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_approvals_moduleCode ON zuHause.dbo.approvals (  moduleCode ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_approvals_statusCode ON zuHause.dbo.approvals (  statusCode ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_approvals_status_category ON zuHause.dbo.approvals (  statusCategory ASC  , statusCode ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE UNIQUE NONCLUSTERED INDEX UQ_approvals_module_source ON zuHause.dbo.approvals (  moduleCode ASC  , sourceID ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.chatrooms definition

-- Drop table

-- DROP TABLE zuHause.dbo.chatrooms;

CREATE TABLE zuHause.dbo.chatrooms (
	chatroomID int IDENTITY(1,1) NOT NULL,
	initiatorMemberID int NOT NULL,
	participantMemberID int NOT NULL,
	propertyID int NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	lastMessageAt datetime2(0) NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_chatrooms PRIMARY KEY (chatroomID),
	CONSTRAINT FK_chatrooms_initiator FOREIGN KEY (initiatorMemberID) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_chatrooms_participant FOREIGN KEY (participantMemberID) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_chatrooms_property FOREIGN KEY (propertyID) REFERENCES zuHause.dbo.properties(propertyID)
);
 CREATE NONCLUSTERED INDEX IX_chatrooms_lastmsg ON zuHause.dbo.chatrooms (  lastMessageAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE UNIQUE NONCLUSTERED INDEX UQ_chatrooms_members ON zuHause.dbo.chatrooms (  initiatorMemberID ASC  , participantMemberID ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.contracts definition

-- Drop table

-- DROP TABLE zuHause.dbo.contracts;

CREATE TABLE zuHause.dbo.contracts (
	contractId int IDENTITY(1,1) NOT NULL,
	rentalApplicationId int NULL,
	templateId int NULL,
	startDate date NOT NULL,
	endDate date NULL,
	status varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	courtJurisdiction nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	isSublettable bit DEFAULT 0 NOT NULL,
	usagePurpose nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	depositAmount int NULL,
	cleaningFee int NULL,
	managementFee int NULL,
	parkingFee int NULL,
	penaltyAmount int NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_contracts PRIMARY KEY (contractId),
	CONSTRAINT FK_contracts_rentalApp FOREIGN KEY (rentalApplicationId) REFERENCES zuHause.dbo.rentalApplications(applicationID),
	CONSTRAINT FK_contracts_template FOREIGN KEY (templateId) REFERENCES zuHause.dbo.contractTemplates(contractTemplateId)
);
 CREATE NONCLUSTERED INDEX IX_contracts_status_dates ON zuHause.dbo.contracts (  status ASC  , startDate ASC  , endDate ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.favorites definition

-- Drop table

-- DROP TABLE zuHause.dbo.favorites;

CREATE TABLE zuHause.dbo.favorites (
	memberID int NOT NULL,
	propertyID int NOT NULL,
	isActive bit DEFAULT 1 NOT NULL,
	favoritedAt datetime2(0) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_favorites PRIMARY KEY (memberID,propertyID),
	CONSTRAINT FK_favorites_member FOREIGN KEY (memberID) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_favorites_property FOREIGN KEY (propertyID) REFERENCES zuHause.dbo.properties(propertyID)
);
 CREATE NONCLUSTERED INDEX IX_favorites_property ON zuHause.dbo.favorites (  propertyID ASC  )  
	 INCLUDE ( isActive , memberID ) 
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.fileApprovals definition

-- Drop table

-- DROP TABLE zuHause.dbo.fileApprovals;

CREATE TABLE zuHause.dbo.fileApprovals (
	approvalID int NOT NULL,
	uploadID int NOT NULL,
	memberID int NOT NULL,
	statusCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	resultDescription nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	appliedAt datetime2(0) NOT NULL,
	reviewedAt datetime2(0) NULL,
	reviewerAdminID int NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_fileApprovals PRIMARY KEY (approvalID),
	CONSTRAINT FK_fileApprovals_upload FOREIGN KEY (uploadID) REFERENCES zuHause.dbo.userUploads(uploadID)
);


-- zuHause.dbo.furnitureCarts definition

-- Drop table

-- DROP TABLE zuHause.dbo.furnitureCarts;

CREATE TABLE zuHause.dbo.furnitureCarts (
	furnitureCartId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	memberId int NOT NULL,
	propertyId int NULL,
	status varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_furnitureCarts PRIMARY KEY (furnitureCartId),
	CONSTRAINT FK_furnitureCarts_member FOREIGN KEY (memberId) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_furnitureCarts_property FOREIGN KEY (propertyId) REFERENCES zuHause.dbo.properties(propertyID)
);


-- zuHause.dbo.furnitureInventory definition

-- Drop table

-- DROP TABLE zuHause.dbo.furnitureInventory;

CREATE TABLE zuHause.dbo.furnitureInventory (
	furnitureInventoryId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	productId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	totalQuantity int NOT NULL,
	rentedQuantity int DEFAULT 0 NOT NULL,
	availableQuantity int NOT NULL,
	safetyStock int DEFAULT 0 NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_furnitureInventory PRIMARY KEY (furnitureInventoryId),
	CONSTRAINT FK_furnitureInventory_product FOREIGN KEY (productId) REFERENCES zuHause.dbo.furnitureProducts(furnitureProductId)
);
 CREATE NONCLUSTERED INDEX IX_furnInventory_available ON zuHause.dbo.furnitureInventory (  productId ASC  , availableQuantity ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.furnitureOrders definition

-- Drop table

-- DROP TABLE zuHause.dbo.furnitureOrders;

CREATE TABLE zuHause.dbo.furnitureOrders (
	furnitureOrderId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	memberId int NOT NULL,
	propertyId int NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	totalAmount decimal(12,0) NOT NULL,
	status varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	paymentStatus varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	contractLink nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	deletedAt datetime2(0) NULL,
	CONSTRAINT PK_furnitureOrders PRIMARY KEY (furnitureOrderId),
	CONSTRAINT FK_furnitureOrders_member FOREIGN KEY (memberId) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_furnitureOrders_property FOREIGN KEY (propertyId) REFERENCES zuHause.dbo.properties(propertyID)
);
 CREATE NONCLUSTERED INDEX IX_furnOrders_member_status ON zuHause.dbo.furnitureOrders (  memberId ASC  , status ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.furnitureRentalContracts definition

-- Drop table

-- DROP TABLE zuHause.dbo.furnitureRentalContracts;

CREATE TABLE zuHause.dbo.furnitureRentalContracts (
	furnitureRentalContractsId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	orderId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	contractJson nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	contractLink nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	deliveryDate date NULL,
	terminationPolicy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	signStatus varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	signedAt datetime2(0) NULL,
	eSignatureValue nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_furnitureRentalContracts PRIMARY KEY (furnitureRentalContractsId),
	CONSTRAINT FK_furnitureRentalContracts_order FOREIGN KEY (orderId) REFERENCES zuHause.dbo.furnitureOrders(furnitureOrderId)
);


-- zuHause.dbo.memberVerifications definition

-- Drop table

-- DROP TABLE zuHause.dbo.memberVerifications;

CREATE TABLE zuHause.dbo.memberVerifications (
	verificationID int IDENTITY(1,1) NOT NULL,
	memberID int NOT NULL,
	verificationTypeCode nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	verificationCode nvarchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isSuccessful bit DEFAULT 0 NOT NULL,
	sentAt datetime2(0) NOT NULL,
	verifiedAt datetime2(0) NULL,
	CONSTRAINT PK_memberVerifications PRIMARY KEY (verificationID),
	CONSTRAINT FK_memberVerifications_member FOREIGN KEY (memberID) REFERENCES zuHause.dbo.members(memberID)
);


-- zuHause.dbo.orderEvents definition

-- Drop table

-- DROP TABLE zuHause.dbo.orderEvents;

CREATE TABLE zuHause.dbo.orderEvents (
	orderEventsId uniqueidentifier DEFAULT newid() NOT NULL,
	orderId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	eventType varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	payload nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	occurredAt datetime2(0) NOT NULL,
	recordedAt datetime2(0) NOT NULL,
	CONSTRAINT PK_orderEvents PRIMARY KEY (orderEventsId),
	CONSTRAINT FK_orderEvents_order FOREIGN KEY (orderId) REFERENCES zuHause.dbo.furnitureOrders(furnitureOrderId)
);


-- zuHause.dbo.renterPostReplies definition

-- Drop table

-- DROP TABLE zuHause.dbo.renterPostReplies;

CREATE TABLE zuHause.dbo.renterPostReplies (
	replyID int NOT NULL,
	postID int NOT NULL,
	landlordMemberID int NOT NULL,
	replyContent nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	suggestPropertyID int NULL,
	isWithinBudget bit DEFAULT 1 NOT NULL,
	tags nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_renterPostReplies PRIMARY KEY (replyID),
	CONSTRAINT FK_renterPostReplies_landlord FOREIGN KEY (landlordMemberID) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_renterPostReplies_post FOREIGN KEY (postID) REFERENCES zuHause.dbo.renterPosts(postID)
);


-- zuHause.dbo.approvalItems definition

-- Drop table

-- DROP TABLE zuHause.dbo.approvalItems;

CREATE TABLE zuHause.dbo.approvalItems (
	approvalItemID int IDENTITY(801,1) NOT NULL,
	approvalID int NOT NULL,
	actionBy int NOT NULL,
	actionType nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	actionNote nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	snapshotJSON nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	actionCategory AS (CONVERT([nvarchar](20),N'APPROVAL_ACTION')) PERSISTED,
	CONSTRAINT PK_approvalItems PRIMARY KEY (approvalItemID),
	CONSTRAINT FK_approvalItems_action FOREIGN KEY (actionCategory,actionType) REFERENCES zuHause.dbo.systemCodes(codeCategory,code),
	CONSTRAINT FK_approvalItems_approval FOREIGN KEY (approvalID) REFERENCES zuHause.dbo.approvals(approvalID)
);
 CREATE NONCLUSTERED INDEX IX_approvalItems_actionBy ON zuHause.dbo.approvalItems (  actionBy ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_approvalItems_actionType ON zuHause.dbo.approvalItems (  actionType ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_approvalItems_action_category ON zuHause.dbo.approvalItems (  actionCategory ASC  , actionType ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_approvalItems_approvalID ON zuHause.dbo.approvalItems (  approvalID ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_approvalItems_createdAt ON zuHause.dbo.approvalItems (  createdAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.chatroomMessages definition

-- Drop table

-- DROP TABLE zuHause.dbo.chatroomMessages;

CREATE TABLE zuHause.dbo.chatroomMessages (
	messageID int IDENTITY(1,1) NOT NULL,
	chatroomID int NOT NULL,
	senderMemberID int NOT NULL,
	messageContent nvarchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	isRead bit DEFAULT 0 NOT NULL,
	sentAt datetime2(0) NOT NULL,
	readAt datetime2(0) NULL,
	CONSTRAINT PK_chatroomMessages PRIMARY KEY (messageID),
	CONSTRAINT FK_chatroomMessages_chatroom FOREIGN KEY (chatroomID) REFERENCES zuHause.dbo.chatrooms(chatroomID),
	CONSTRAINT FK_chatroomMessages_sender FOREIGN KEY (senderMemberID) REFERENCES zuHause.dbo.members(memberID)
);
 CREATE NONCLUSTERED INDEX IX_chatroomMessages_chat_time ON zuHause.dbo.chatroomMessages (  chatroomID ASC  , sentAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.contractComments definition

-- Drop table

-- DROP TABLE zuHause.dbo.contractComments;

CREATE TABLE zuHause.dbo.contractComments (
	contractCommentId int IDENTITY(1,1) NOT NULL,
	contractId int NOT NULL,
	commentType varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	commentText nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	createdById int NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_contractComments PRIMARY KEY (contractCommentId),
	CONSTRAINT FK_contractComments_contract FOREIGN KEY (contractId) REFERENCES zuHause.dbo.contracts(contractId) ON DELETE CASCADE
);


-- zuHause.dbo.contractCustomFields definition

-- Drop table

-- DROP TABLE zuHause.dbo.contractCustomFields;

CREATE TABLE zuHause.dbo.contractCustomFields (
	contractCustomFieldId int IDENTITY(1,1) NOT NULL,
	contractId int NOT NULL,
	fieldKey nvarchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	fieldValue nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	fieldType varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	displayOrder int DEFAULT 0 NOT NULL,
	CONSTRAINT PK_contractCustomFields PRIMARY KEY (contractCustomFieldId),
	CONSTRAINT FK_contractCustomFields_contract FOREIGN KEY (contractId) REFERENCES zuHause.dbo.contracts(contractId) ON DELETE CASCADE
);


-- zuHause.dbo.contractFurnitureItems definition

-- Drop table

-- DROP TABLE zuHause.dbo.contractFurnitureItems;

CREATE TABLE zuHause.dbo.contractFurnitureItems (
	contractFurnitureItemId int IDENTITY(1,1) NOT NULL,
	contractId int NOT NULL,
	furnitureName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	furnitureCondition nvarchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	quantity int NOT NULL,
	repairChargeOwner nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	repairResponsibility nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	unitPrice int NULL,
	amount int NULL,
	CONSTRAINT PK_contractFurnitureItems PRIMARY KEY (contractFurnitureItemId),
	CONSTRAINT FK_contractFurnitureItems_contract FOREIGN KEY (contractId) REFERENCES zuHause.dbo.contracts(contractId) ON DELETE CASCADE
);


-- zuHause.dbo.contractSignatures definition

-- Drop table

-- DROP TABLE zuHause.dbo.contractSignatures;

CREATE TABLE zuHause.dbo.contractSignatures (
	idcontractSignatureId int IDENTITY(1,1) NOT NULL,
	contractId int NOT NULL,
	signerId int NOT NULL,
	signerRole varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	signMethod nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	signatureFileUrl nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	signVerifyInfo nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	signedAt datetime2(0) NULL,
	uploadId int NULL,
	CONSTRAINT PK_contractSignatures PRIMARY KEY (idcontractSignatureId),
	CONSTRAINT FK_contractSignatures_contract FOREIGN KEY (contractId) REFERENCES zuHause.dbo.contracts(contractId) ON DELETE CASCADE,
	CONSTRAINT FK_contractSignatures_member FOREIGN KEY (signerId) REFERENCES zuHause.dbo.members(memberID)
);


-- zuHause.dbo.customerServiceTickets definition

-- Drop table

-- DROP TABLE zuHause.dbo.customerServiceTickets;

CREATE TABLE zuHause.dbo.customerServiceTickets (
	ticketId int IDENTITY(201,1) NOT NULL,
	memberId int NOT NULL,
	propertyId int NULL,
	contractId int NULL,
	furnitureOrderId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	subject nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	categoryCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	ticketContent nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	replyContent nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	statusCode varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	replyAt datetime2(0) NULL,
	updatedAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	handledBy int NULL,
	isResolved bit DEFAULT 0 NOT NULL,
	CONSTRAINT PK_customerServiceTickets PRIMARY KEY (ticketId),
	CONSTRAINT FK_custTickets_contract FOREIGN KEY (contractId) REFERENCES zuHause.dbo.contracts(contractId),
	CONSTRAINT FK_custTickets_furnOrder FOREIGN KEY (furnitureOrderId) REFERENCES zuHause.dbo.furnitureOrders(furnitureOrderId),
	CONSTRAINT FK_custTickets_member FOREIGN KEY (memberId) REFERENCES zuHause.dbo.members(memberID),
	CONSTRAINT FK_custTickets_property FOREIGN KEY (propertyId) REFERENCES zuHause.dbo.properties(propertyID)
);
 CREATE NONCLUSTERED INDEX IX_customerServiceTickets_categoryCode ON zuHause.dbo.customerServiceTickets (  categoryCode ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_customerServiceTickets_contractId ON zuHause.dbo.customerServiceTickets (  contractId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_customerServiceTickets_createdAt ON zuHause.dbo.customerServiceTickets (  createdAt ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_customerServiceTickets_isResolved ON zuHause.dbo.customerServiceTickets (  isResolved ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_customerServiceTickets_memberId ON zuHause.dbo.customerServiceTickets (  memberId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_customerServiceTickets_propertyId ON zuHause.dbo.customerServiceTickets (  propertyId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_customerServiceTickets_statusCode ON zuHause.dbo.customerServiceTickets (  statusCode ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- zuHause.dbo.furnitureCartItems definition

-- Drop table

-- DROP TABLE zuHause.dbo.furnitureCartItems;

CREATE TABLE zuHause.dbo.furnitureCartItems (
	cartItemId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	cartId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	productId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	quantity int NOT NULL,
	rentalDays int NOT NULL,
	unitPriceSnapshot decimal(10,0) NOT NULL,
	subTotal decimal(12,0) NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_furnitureCartItems PRIMARY KEY (cartItemId),
	CONSTRAINT FK_furnitureCartItems_cart FOREIGN KEY (cartId) REFERENCES zuHause.dbo.furnitureCarts(furnitureCartId),
	CONSTRAINT FK_furnitureCartItems_product FOREIGN KEY (productId) REFERENCES zuHause.dbo.furnitureProducts(furnitureProductId)
);


-- zuHause.dbo.furnitureOrderHistory definition

-- Drop table

-- DROP TABLE zuHause.dbo.furnitureOrderHistory;

CREATE TABLE zuHause.dbo.furnitureOrderHistory (
	furnitureOrderHistoryId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	orderId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	productId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	productNameSnapshot nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	descriptionSnapshot nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	quantity int NOT NULL,
	dailyRentalSnapshot decimal(10,0) NOT NULL,
	rentalStart date NOT NULL,
	rentalEnd date NOT NULL,
	subTotal decimal(12,0) NOT NULL,
	itemStatus nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	returnedAt date NULL,
	damageNote nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_furnitureOrderHistory PRIMARY KEY (furnitureOrderHistoryId),
	CONSTRAINT FK_furnitureOrderHistory_order FOREIGN KEY (orderId) REFERENCES zuHause.dbo.furnitureOrders(furnitureOrderId),
	CONSTRAINT FK_furnitureOrderHistory_product FOREIGN KEY (productId) REFERENCES zuHause.dbo.furnitureProducts(furnitureProductId)
);


-- zuHause.dbo.furnitureOrderItems definition

-- Drop table

-- DROP TABLE zuHause.dbo.furnitureOrderItems;

CREATE TABLE zuHause.dbo.furnitureOrderItems (
	furnitureOrderItemId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	orderId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	productId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	quantity int NOT NULL,
	dailyRentalSnapshot decimal(10,0) NOT NULL,
	rentalDays int NOT NULL,
	subTotal decimal(12,0) NOT NULL,
	createdAt datetime2(0) DEFAULT CONVERT([datetime2](0),sysdatetime()) NOT NULL,
	CONSTRAINT PK_furnitureOrderItems PRIMARY KEY (furnitureOrderItemId),
	CONSTRAINT FK_furnitureOrderItems_order FOREIGN KEY (orderId) REFERENCES zuHause.dbo.furnitureOrders(furnitureOrderId),
	CONSTRAINT FK_furnitureOrderItems_product FOREIGN KEY (productId) REFERENCES zuHause.dbo.furnitureProducts(furnitureProductId)
);