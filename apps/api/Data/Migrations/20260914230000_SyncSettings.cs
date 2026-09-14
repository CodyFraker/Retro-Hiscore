using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260914230000_SyncSettings")]
    public partial class SyncSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncLeaderboardSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    HotIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    ColdIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    HotActivityWindowHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLeaderboardSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncRecurringJobs",
                columns: table => new
                {
                    JobId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: ""),
                    IntervalMinutes = table.Column<int>(type: "integer", nullable: true),
                    IntervalDays = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncRecurringJobs", x => x.JobId);
                });

            migrationBuilder.Sql("""
                INSERT INTO "SyncLeaderboardSettings" ("Id", "HotIntervalMinutes", "ColdIntervalMinutes", "HotActivityWindowHours", "UpdatedAt")
                VALUES (1, 15, 1440, 168, TIMESTAMPTZ '2026-09-14 00:00:00+00');

                INSERT INTO "SyncRecurringJobs" ("JobId", "DisplayName", "IntervalMinutes", "IntervalDays", "UpdatedAt")
                VALUES
                    ('ra-member-activity-sync', 'Member activity', 15, NULL, TIMESTAMPTZ '2026-09-14 00:00:00+00'),
                    ('ra-leaderboard-dispatch', 'Leaderboard dispatch', 5, NULL, TIMESTAMPTZ '2026-09-14 00:00:00+00'),
                    ('ra-member-rank-sync', 'Member RA rank', 60, NULL, TIMESTAMPTZ '2026-09-14 00:00:00+00'),
                    ('ra-game-metadata-sync', 'Game metadata', NULL, 7, TIMESTAMPTZ '2026-09-14 00:00:00+00');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SyncRecurringJobs");
            migrationBuilder.DropTable(name: "SyncLeaderboardSettings");
        }
    }
}
