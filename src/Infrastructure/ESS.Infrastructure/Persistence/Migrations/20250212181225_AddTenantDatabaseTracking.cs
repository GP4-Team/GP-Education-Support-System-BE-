using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESS.Infrastructure.Persistence.Migrations;

public partial class AddTenantDatabaseTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "DatabaseStatus",
            table: "Tenants",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "DatabaseCreatedAt",
            table: "Tenants",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DatabaseError",
            table: "Tenants",
            type: "text",
            nullable: true);

        // Add index for faster queries on database status
        migrationBuilder.CreateIndex(
            name: "IX_Tenants_DatabaseStatus",
            table: "Tenants",
            column: "DatabaseStatus");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Tenants_DatabaseStatus",
            table: "Tenants");

        migrationBuilder.DropColumn(
            name: "DatabaseStatus",
            table: "Tenants");

        migrationBuilder.DropColumn(
            name: "DatabaseCreatedAt",
            table: "Tenants");

        migrationBuilder.DropColumn(
            name: "DatabaseError",
            table: "Tenants");
    }
}