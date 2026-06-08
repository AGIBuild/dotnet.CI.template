using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChengYuan.Outbox.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PayloadContent = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PayloadTypeName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedAtUtcTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DispatchedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastError = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_CreatedAtUtcTicks",
                table: "OutboxMessages",
                columns: new[] { "Status", "CreatedAtUtcTicks" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages");
        }
    }
}
