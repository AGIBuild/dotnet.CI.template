using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChengYuan.SettingManagement.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SettingValues",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                Scope = table.Column<int>(type: "INTEGER", nullable: false),
                SerializedValue = table.Column<string>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                UserId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SettingValues", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SettingValues_Name_Scope_TenantId_UserId",
            table: "SettingValues",
            columns: new[] { "Name", "Scope", "TenantId", "UserId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SettingValues");
    }
}
