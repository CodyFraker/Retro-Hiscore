using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260917120000_GameTrackQueueMemberRequests")]
    public partial class GameTrackQueueMemberRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestCount",
                table: "GameTrackQueues",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedByMemberId",
                table: "GameTrackQueues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "GameTrackQueues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GameTrackQueueRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaGameId = table.Column<int>(type: "integer", nullable: false),
                    GameTrackQueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameTrackQueueRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameTrackQueueRequests_GameTrackQueues_GameTrackQueueId",
                        column: x => x.GameTrackQueueId,
                        principalTable: "GameTrackQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GameTrackQueueRequests_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameTrackQueues_RequestedByMemberId",
                table: "GameTrackQueues",
                column: "RequestedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_GameTrackQueueRequests_GameTrackQueueId",
                table: "GameTrackQueueRequests",
                column: "GameTrackQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_GameTrackQueueRequests_MemberId_CreatedAt",
                table: "GameTrackQueueRequests",
                columns: new[] { "MemberId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GameTrackQueueRequests_RaGameId",
                table: "GameTrackQueueRequests",
                column: "RaGameId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameTrackQueues_Members_RequestedByMemberId",
                table: "GameTrackQueues",
                column: "RequestedByMemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameTrackQueues_Members_RequestedByMemberId",
                table: "GameTrackQueues");

            migrationBuilder.DropTable(
                name: "GameTrackQueueRequests");

            migrationBuilder.DropIndex(
                name: "IX_GameTrackQueues_RequestedByMemberId",
                table: "GameTrackQueues");

            migrationBuilder.DropColumn(
                name: "RequestCount",
                table: "GameTrackQueues");

            migrationBuilder.DropColumn(
                name: "RequestedByMemberId",
                table: "GameTrackQueues");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "GameTrackQueues");
        }
    }
}
