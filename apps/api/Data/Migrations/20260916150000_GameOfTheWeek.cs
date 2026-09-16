using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260916150000_GameOfTheWeek")]
    public partial class GameOfTheWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameOfTheWeekPolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WinnerRaGameId = table.Column<int>(type: "integer", nullable: true),
                    TrackingStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByMemberId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameOfTheWeekPolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameOfTheWeekPolls_Members_CreatedByMemberId",
                        column: x => x.CreatedByMemberId,
                        principalTable: "Members",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GameOfTheWeekBallotEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PollId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaGameId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ConsoleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ImageIcon = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    AddedByMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameOfTheWeekBallotEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameOfTheWeekBallotEntries_GameOfTheWeekPolls_PollId",
                        column: x => x.PollId,
                        principalTable: "GameOfTheWeekPolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameOfTheWeekBallotEntries_Members_AddedByMemberId",
                        column: x => x.AddedByMemberId,
                        principalTable: "Members",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GameOfTheWeekVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PollId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaGameId = table.Column<int>(type: "integer", nullable: false),
                    CastAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameOfTheWeekVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameOfTheWeekVotes_GameOfTheWeekPolls_PollId",
                        column: x => x.PollId,
                        principalTable: "GameOfTheWeekPolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameOfTheWeekVotes_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameOfTheWeekBallotEntries_AddedByMemberId",
                table: "GameOfTheWeekBallotEntries",
                column: "AddedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_GameOfTheWeekBallotEntries_PollId_RaGameId",
                table: "GameOfTheWeekBallotEntries",
                columns: new[] { "PollId", "RaGameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameOfTheWeekBallotEntries_PollId_SortOrder",
                table: "GameOfTheWeekBallotEntries",
                columns: new[] { "PollId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameOfTheWeekPolls_ClosedAt",
                table: "GameOfTheWeekPolls",
                column: "ClosedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GameOfTheWeekPolls_CreatedByMemberId",
                table: "GameOfTheWeekPolls",
                column: "CreatedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_GameOfTheWeekPolls_StartsAt_EndsAt",
                table: "GameOfTheWeekPolls",
                columns: new[] { "StartsAt", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GameOfTheWeekVotes_MemberId",
                table: "GameOfTheWeekVotes",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_GameOfTheWeekVotes_PollId_MemberId",
                table: "GameOfTheWeekVotes",
                columns: new[] { "PollId", "MemberId" },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "SyncRecurringJobs" ("JobId", "DisplayName", "IntervalMinutes", "IntervalDays", "UpdatedAt")
                VALUES ('ra-game-of-the-week', 'Game of the week', 5, NULL, NOW())
                ON CONFLICT ("JobId") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "SyncRecurringJobs" WHERE "JobId" = 'ra-game-of-the-week';
                """);

            migrationBuilder.DropTable(name: "GameOfTheWeekVotes");
            migrationBuilder.DropTable(name: "GameOfTheWeekBallotEntries");
            migrationBuilder.DropTable(name: "GameOfTheWeekPolls");
        }
    }
}
