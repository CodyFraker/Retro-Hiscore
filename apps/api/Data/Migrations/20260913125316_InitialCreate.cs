using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaGameId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaUsername = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RaUlid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leaderboards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaLeaderboardId = table.Column<long>(type: "bigint", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Format = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RankAsc = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaderboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaderboards_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaderboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<long>(type: "bigint", nullable: false),
                    FormattedScore = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GlobalRank = table.Column<int>(type: "integer", nullable: true),
                    FriendRank = table.Column<int>(type: "integer", nullable: true),
                    ScoreUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_Leaderboards_LeaderboardId",
                        column: x => x.LeaderboardId,
                        principalTable: "Leaderboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardEntrySnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaderboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<long>(type: "bigint", nullable: false),
                    FormattedScore = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GlobalRank = table.Column<int>(type: "integer", nullable: true),
                    FriendRank = table.Column<int>(type: "integer", nullable: true),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardEntrySnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntrySnapshots_Leaderboards_LeaderboardId",
                        column: x => x.LeaderboardId,
                        principalTable: "Leaderboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntrySnapshots_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_RaGameId",
                table: "Games",
                column: "RaGameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_LeaderboardId_MemberId",
                table: "LeaderboardEntries",
                columns: new[] { "LeaderboardId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_MemberId",
                table: "LeaderboardEntries",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntrySnapshots_LeaderboardId_MemberId_SyncedAt",
                table: "LeaderboardEntrySnapshots",
                columns: new[] { "LeaderboardId", "MemberId", "SyncedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntrySnapshots_MemberId",
                table: "LeaderboardEntrySnapshots",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_GameId",
                table: "Leaderboards",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_RaLeaderboardId",
                table: "Leaderboards",
                column: "RaLeaderboardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_RaUsername",
                table: "Members",
                column: "RaUsername",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaderboardEntries");

            migrationBuilder.DropTable(
                name: "LeaderboardEntrySnapshots");

            migrationBuilder.DropTable(
                name: "SyncRuns");

            migrationBuilder.DropTable(
                name: "Leaderboards");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
