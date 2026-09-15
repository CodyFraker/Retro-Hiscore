using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260915120000_GameAchievementAnalytics")]
    public partial class GameAchievementAnalytics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AchievementDistributionSyncedAt",
                table: "Games",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AchievementDistributionHardcoreJson",
                table: "Games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AchievementDistributionSoftcoreJson",
                table: "Games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AchievementProgressSyncedAt",
                table: "Games",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "SyncRecurringJobs" ("JobId", "DisplayName", "IntervalMinutes", "IntervalDays", "UpdatedAt")
                SELECT 'ra-member-achievements-sync', 'Member achievements', 360, NULL, NOW()
                WHERE NOT EXISTS (
                    SELECT 1 FROM "SyncRecurringJobs" WHERE "JobId" = 'ra-member-achievements-sync'
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """DELETE FROM "SyncRecurringJobs" WHERE "JobId" = 'ra-member-achievements-sync';""");

            migrationBuilder.DropColumn(
                name: "AchievementDistributionSyncedAt",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "AchievementDistributionHardcoreJson",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "AchievementDistributionSoftcoreJson",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "AchievementProgressSyncedAt",
                table: "Games");
        }
    }
}
