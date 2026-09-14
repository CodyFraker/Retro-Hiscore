using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260914210000_AddLeaderboardPopulationSnapshots")]
    public partial class AddLeaderboardPopulationSnapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlobalEntryCount",
                table: "Leaderboards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GlobalEntryCountSyncedAt",
                table: "Leaderboards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeaderboardPopulationSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaderboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryCount = table.Column<int>(type: "integer", nullable: false),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardPopulationSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardPopulationSnapshots_Leaderboards_LeaderboardId",
                        column: x => x.LeaderboardId,
                        principalTable: "Leaderboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardPopulationSnapshots_LeaderboardId_SyncedAt",
                table: "LeaderboardPopulationSnapshots",
                columns: new[] { "LeaderboardId", "SyncedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaderboardPopulationSnapshots");

            migrationBuilder.DropColumn(
                name: "GlobalEntryCount",
                table: "Leaderboards");

            migrationBuilder.DropColumn(
                name: "GlobalEntryCountSyncedAt",
                table: "Leaderboards");
        }
    }
}
