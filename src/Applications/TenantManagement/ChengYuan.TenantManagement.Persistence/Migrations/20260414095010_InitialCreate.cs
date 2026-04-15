using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChengYuan.TenantManagement.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                NormalizedName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenants", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Tenants_NormalizedName",
            table: "Tenants",
            column: "NormalizedName",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Tenants");
    }
}
