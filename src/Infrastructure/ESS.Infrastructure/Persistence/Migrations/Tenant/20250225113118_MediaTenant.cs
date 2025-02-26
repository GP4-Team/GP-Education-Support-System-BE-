using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESS.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class MediaTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                schema: "public",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                schema: "public",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "public",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "public",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "MediaCollections",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AllowedTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MaxFileSize = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Collection = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsTemporary = table.Column<bool>(type: "boolean", nullable: false),
                    TempGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    MediaCollectionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_MediaCollections_MediaCollectionId",
                        column: x => x.MediaCollectionId,
                        principalSchema: "public",
                        principalTable: "MediaCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                schema: "public",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Username",
                schema: "public",
                table: "Users",
                columns: new[] { "TenantId", "Username" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_MediaCollectionId",
                schema: "public",
                table: "Media",
                column: "MediaCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_TenantId_ResourceId_ResourceType",
                schema: "public",
                table: "Media",
                columns: new[] { "TenantId", "ResourceId", "ResourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_Media_TenantId_TempGuid",
                schema: "public",
                table: "Media",
                columns: new[] { "TenantId", "TempGuid" },
                filter: "\"IsTemporary\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_MediaCollections_TenantId_Name",
                schema: "public",
                table: "MediaCollections",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Media",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MediaCollections",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Email",
                schema: "public",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Username",
                schema: "public",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                schema: "public",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                schema: "public",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "public",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "public",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);
        }
    }
}
