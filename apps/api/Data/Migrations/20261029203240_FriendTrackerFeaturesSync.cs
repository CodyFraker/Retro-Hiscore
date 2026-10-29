using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class FriendTrackerFeaturesSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    ActorMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    RaGameId = table.Column<int>(type: "integer", nullable: true),
                    GameTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RaLeaderboardId = table.Column<long>(type: "bigint", nullable: true),
                    LeaderboardTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AchievementTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AchievementPoints = table.Column<int>(type: "integer", nullable: true),
                    Hardcore = table.Column<bool>(type: "boolean", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberAlerts_Members_ActorMemberId",
                        column: x => x.ActorMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MemberAlerts_Members_RecipientMemberId",
                        column: x => x.RecipientMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberGamePins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    PinnedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberGamePins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberGamePins_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberGamePins_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonBoardTakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaderboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    TakenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonBoardTakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonBoardTakes_Leaderboards_LeaderboardId",
                        column: x => x.LeaderboardId,
                        principalTable: "Leaderboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonBoardTakes_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberAlerts_ActorMemberId",
                table: "MemberAlerts",
                column: "ActorMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberAlerts_RecipientMemberId_OccurredAt",
                table: "MemberAlerts",
                columns: new[] { "RecipientMemberId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberGamePins_GameId",
                table: "MemberGamePins",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberGamePins_MemberId_GameId",
                table: "MemberGamePins",
                columns: new[] { "MemberId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeasonBoardTakes_LeaderboardId",
                table: "SeasonBoardTakes",
                column: "LeaderboardId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonBoardTakes_MemberId_LeaderboardId_TakenAt",
                table: "SeasonBoardTakes",
                columns: new[] { "MemberId", "LeaderboardId", "TakenAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeasonBoardTakes_TakenAt",
                table: "SeasonBoardTakes",
                column: "TakenAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberAlerts");

            migrationBuilder.DropTable(
                name: "MemberGamePins");

            migrationBuilder.DropTable(
                name: "SeasonBoardTakes");
        }
    }
}
