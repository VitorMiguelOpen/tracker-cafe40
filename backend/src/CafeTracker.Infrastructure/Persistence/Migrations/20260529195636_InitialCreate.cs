using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CafeTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coffee_session",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    machine_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    is_open = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coffee_session", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status_event",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    machine_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    tag = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<short>(type: "smallint", nullable: false),
                    event_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    raw_payload = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_status_event", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_coffee_session_machine_start",
                table: "coffee_session",
                columns: new[] { "machine_code", "started_at" });

            migrationBuilder.CreateIndex(
                name: "ix_coffee_session_open",
                table: "coffee_session",
                columns: new[] { "machine_code", "is_open" });

            migrationBuilder.CreateIndex(
                name: "ix_status_event_machine_time",
                table: "status_event",
                columns: new[] { "machine_code", "event_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coffee_session");

            migrationBuilder.DropTable(
                name: "status_event");
        }
    }
}
