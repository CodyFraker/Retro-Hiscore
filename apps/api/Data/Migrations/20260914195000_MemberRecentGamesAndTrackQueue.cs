using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260914195000_MemberRecentGamesAndTrackQueue")]
    public partial class MemberRecentGamesAndTrackQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameTrackQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaGameId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ConsoleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EnqueuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameTrackQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameTrackQueues_Members_ResolvedByMemberId",
                        column: x => x.ResolvedByMemberId,
                        principalTable: "Members",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MemberRecentGamePlays",
                columns: table => new
                {
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaGameId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ConsoleId = table.Column<int>(type: "integer", nullable: false),
                    ConsoleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ImageIcon = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ImageBoxArt = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastPlayedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NumAchieved = table.Column<int>(type: "integer", nullable: false),
                    NumPossibleAchievements = table.Column<int>(type: "integer", nullable: false),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRecentGamePlays", x => new { x.MemberId, x.RaGameId });
                    table.ForeignKey(
                        name: "FK_MemberRecentGamePlays_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameTrackQueues_RaGameId",
                table: "GameTrackQueues",
                column: "RaGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameTrackQueues_RaGameId_Status",
                table: "GameTrackQueues",
                columns: new[] { "RaGameId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GameTrackQueues_ResolvedByMemberId",
                table: "GameTrackQueues",
                column: "ResolvedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_GameTrackQueues_Status",
                table: "GameTrackQueues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MemberRecentGamePlays_LastPlayedAt",
                table: "MemberRecentGamePlays",
                column: "LastPlayedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MemberRecentGamePlays_RaGameId",
                table: "MemberRecentGamePlays",
                column: "RaGameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberRecentGamePlays");

            migrationBuilder.DropTable(
                name: "GameTrackQueues");
        }
    }
}
