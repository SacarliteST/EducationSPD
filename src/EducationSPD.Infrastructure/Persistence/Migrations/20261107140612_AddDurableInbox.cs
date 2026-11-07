using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationSPD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDurableInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxMessages",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_type = table.Column<short>(type: "smallint", nullable: false),
                    schema_version = table.Column<int>(type: "integer", nullable: false),
                    message_key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    payload_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    next_retry_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.event_id);
                    table.CheckConstraint("CK_InboxMessages_MessageType", "message_type IN (1, 2)");
                    table.CheckConstraint("CK_InboxMessages_RetryCount", "retry_count >= 0");
                    table.CheckConstraint("CK_InboxMessages_SchemaVersion", "schema_version > 0");
                    table.CheckConstraint("CK_InboxMessages_Status", "status IN (1, 2, 3, 4)");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_ReadyBatch",
                table: "InboxMessages",
                columns: new[] { "status", "next_retry_at", "received_at", "event_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages");
        }
    }
}
