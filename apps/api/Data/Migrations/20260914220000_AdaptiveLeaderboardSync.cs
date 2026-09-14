using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260914220000_AdaptiveLeaderboardSync")]
    public partial class AdaptiveLeaderboardSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LeaderboardScoresSyncedAt",
                table: "Games",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GameId",
                table: "SyncRuns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "SyncRuns",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncRuns_Kind_GameId_StartedAt",
                table: "SyncRuns",
                columns: new[] { "Kind", "GameId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncRuns_GameId",
                table: "SyncRuns",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncRuns_MemberId",
                table: "SyncRuns",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_SyncRuns_Games_GameId",
                table: "SyncRuns",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SyncRuns_Members_MemberId",
                table: "SyncRuns",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SyncRuns_Games_GameId",
                table: "SyncRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_SyncRuns_Members_MemberId",
                table: "SyncRuns");

            migrationBuilder.DropIndex(
                name: "IX_SyncRuns_Kind_GameId_StartedAt",
                table: "SyncRuns");

            migrationBuilder.DropIndex(
                name: "IX_SyncRuns_GameId",
                table: "SyncRuns");

            migrationBuilder.DropIndex(
                name: "IX_SyncRuns_MemberId",
                table: "SyncRuns");

            migrationBuilder.DropColumn(
                name: "LeaderboardScoresSyncedAt",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "SyncRuns");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "SyncRuns");
        }
    }
}
