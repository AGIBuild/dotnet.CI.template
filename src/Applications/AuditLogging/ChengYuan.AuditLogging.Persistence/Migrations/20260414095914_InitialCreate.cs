using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChengYuan.AuditLogging.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    IsAuthenticated = table.Column<bool>(type: "INTEGER", nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Succeeded = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    SerializedProperties = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogEntityChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuditLogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityTypeFullName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ChangeType = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangeTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SerializedPropertyChanges = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogEntityChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogEntityChanges_AuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogEntityChanges_AuditLogId",
                table: "AuditLogEntityChanges",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogEntityChanges_EntityTypeFullName",
                table: "AuditLogEntityChanges",
                column: "EntityTypeFullName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_StartedAtUtc",
                table: "AuditLogs",
                column: "StartedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogEntityChanges");

            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
