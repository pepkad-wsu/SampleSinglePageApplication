CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "DepartmentGroups" (
    "DepartmentGroupId" uniqueidentifier NOT NULL,
    "DepartmentGroupName" nvarchar(200) NULL,
    "TenantId" uniqueidentifier NULL,
    CONSTRAINT "PK_DepartmentGroups" PRIMARY KEY ("DepartmentGroupId")
);

CREATE TABLE "Departments" (
    "DepartmentId" uniqueidentifier NOT NULL,
    "DepartmentName" nvarchar(100) NULL,
    "ActiveDirectoryNames" nvarchar(100) NULL,
    "Enabled" bit NULL,
    "DepartmentGroupId" uniqueidentifier NULL,
    "TenantId" uniqueidentifier NULL,
    CONSTRAINT "PK_Departments" PRIMARY KEY ("DepartmentId")
);

CREATE TABLE "Settings" (
    "SettingId" int NOT NULL,
    "SettingName" nvarchar(100) NOT NULL,
    "SettingType" nvarchar(100) NULL,
    "SettingNotes" nvarchar(max) NULL,
    "SettingText" nvarchar(max) NULL,
    "TenantId" uniqueidentifier NULL,
    "UserId" uniqueidentifier NULL,
    CONSTRAINT "PK_Settings" PRIMARY KEY ("SettingId")
);

CREATE TABLE "Tenants" (
    "TenantId" uniqueidentifier NOT NULL,
    "Name" nvarchar(200) NOT NULL,
    "TenantCode" nvarchar(50) NOT NULL,
    "Enabled" bit NOT NULL,
    CONSTRAINT "PK_Tenants" PRIMARY KEY ("TenantId")
);

CREATE TABLE "UDFLabels" (
    "Id" uniqueidentifier NOT NULL,
    "Module" nvarchar(20) NULL,
    "UDF" nvarchar(10) NULL,
    "Label" nvarchar(max) NULL,
    "ShowColumn" bit NULL,
    "ShowInFilter" bit NULL,
    "IncludeInSearch" bit NULL,
    "TenantId" uniqueidentifier NULL,
    CONSTRAINT "PK_UDFLabels" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "UserId" uniqueidentifier NOT NULL,
    "TenantId" uniqueidentifier NOT NULL,
    "FirstName" nvarchar(100) NULL,
    "LastName" nvarchar(100) NULL,
    "Email" nvarchar(100) NULL,
    "Phone" nvarchar(20) NULL,
    "Username" nvarchar(100) NOT NULL,
    "EmployeeId" nvarchar(50) NULL,
    "DepartmentId" uniqueidentifier NULL,
    "Title" nvarchar(255) NULL,
    "Location" nvarchar(255) NULL,
    "Enabled" bit NULL,
    "LastLogin" datetime NULL,
    "Admin" bit NULL,
    "Password" nvarchar(max) NULL,
    "PreventPasswordChange" bit NULL,
    "FailedLoginAttempts" int NULL,
    "LastLockoutDate" datetime NULL,
    "Source" nvarchar(100) NULL,
    "UDF01" nvarchar(500) NULL,
    "UDF02" nvarchar(500) NULL,
    "UDF03" nvarchar(500) NULL,
    "UDF04" nvarchar(500) NULL,
    "UDF05" nvarchar(500) NULL,
    "UDF06" nvarchar(500) NULL,
    "UDF07" nvarchar(500) NULL,
    "UDF08" nvarchar(500) NULL,
    "UDF09" nvarchar(500) NULL,
    "UDF10" nvarchar(500) NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("UserId"),
    CONSTRAINT "FK_Users_Departments" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("DepartmentId")
);

CREATE TABLE "FileStorage" (
    "FileId" uniqueidentifier NOT NULL,
    "ItemId" uniqueidentifier NULL,
    "FileName" nvarchar(255) NULL,
    "Extension" nvarchar(15) NULL,
    "Bytes" bigint NULL,
    "Value" varbinary(max) NULL,
    "UploadDate" datetime NULL,
    "UserId" uniqueidentifier NULL,
    "SourceFileId" nvarchar(100) NULL,
    "TenantId" uniqueidentifier NULL,
    CONSTRAINT "PK_FileStorage" PRIMARY KEY ("FileId"),
    CONSTRAINT "FK_FileStorage_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
);

CREATE INDEX "IX_FileStorage_UserId" ON "FileStorage" ("UserId");

CREATE INDEX "IX_Users_DepartmentId" ON "Users" ("DepartmentId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20230213213705_001', '7.0.2');

COMMIT;

