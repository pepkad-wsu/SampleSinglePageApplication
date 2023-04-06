using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleSinglePageApplication.EFModels.Migrations
{
    /// <inheritdoc />
    public partial class _001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentGroups",
                columns: table => new {
                    DepartmentGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DepartmentGroupName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_DepartmentGroups", x => x.DepartmentGroupId);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new {
                    DepartmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DepartmentName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ActiveDirectoryNames = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: true),
                    DepartmentGroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new {
                    SettingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SettingName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SettingType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SettingNotes = table.Column<string>(type: "TEXT", nullable: true),
                    SettingText = table.Column<string>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Settings_1", x => x.SettingId);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new {
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TenantCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "UDFLabels",
                columns: table => new {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Module = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    UDF = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Label = table.Column<string>(type: "TEXT", nullable: true),
                    ShowColumn = table.Column<bool>(type: "INTEGER", nullable: true),
                    ShowInFilter = table.Column<bool>(type: "INTEGER", nullable: true),
                    IncludeInSearch = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_UDFLabels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime", nullable: true),
                    Admin = table.Column<bool>(type: "INTEGER", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    PreventPasswordChange = table.Column<bool>(type: "INTEGER", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "INTEGER", nullable: true),
                    LastLockoutDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(100", maxLength: 100, nullable: true),
                    UDF01 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UDF02 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UDF03 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UDF04 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UDF05 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UDF06 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UDF07 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UDF08 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UDF09 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UDF10 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Departments",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_Users_Tenants",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId");
                });

            migrationBuilder.CreateTable(
                name: "FileStorage",
                columns: table => new {
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Extension = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    Bytes = table.Column<long>(type: "INTEGER", nullable: true),
                    Value = table.Column<byte[]>(type: "BLOB", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceFileId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_FileStorage", x => x.FileId);
                    table.ForeignKey(
                        name: "FK_FileStorage_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileStorage_UserId",
                table: "FileStorage",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentGroups");

            migrationBuilder.DropTable(
                name: "FileStorage");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "UDFLabels");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
