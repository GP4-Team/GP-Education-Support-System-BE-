using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESS.Infrastructure.Persistence.Migrations.Central
{
    /// <inheritdoc />
    public partial class MediaUploaded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AllowedTypes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MaxFileSize = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "text", nullable: false),
                    Collection = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                        principalTable: "MediaCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Media_MediaCollectionId",
                table: "Media",
                column: "MediaCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_TenantId_ResourceId_ResourceType",
                table: "Media",
                columns: new[] { "TenantId", "ResourceId", "ResourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaCollections_TenantId_Name",
                table: "MediaCollections",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "MediaCollections");
        }
    }
}
