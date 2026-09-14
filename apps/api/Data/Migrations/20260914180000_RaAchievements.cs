using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260914180000_RaAchievements")]
    public partial class RaAchievements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RaAchievements",
                columns: table => new
                {
                    RaAchievementId = table.Column<int>(type: "integer", nullable: false),
                    RaGameId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    TrueRatio = table.Column<int>(type: "integer", nullable: false),
                    BadgeName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BadgeData = table.Column<byte[]>(type: "bytea", nullable: true),
                    BadgeContentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaAchievements", x => x.RaAchievementId);
                });

            migrationBuilder.CreateTable(
                name: "MemberRaAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaAchievementId = table.Column<int>(type: "integer", nullable: false),
                    DateEarned = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateEarnedHardcore = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FirstDetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRaAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberRaAchievements_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberRaAchievements_RaAchievements_RaAchievementId",
                        column: x => x.RaAchievementId,
                        principalTable: "RaAchievements",
                        principalColumn: "RaAchievementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberRaAchievements_MemberId_DateEarned",
                table: "MemberRaAchievements",
                columns: new[] { "MemberId", "DateEarned" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberRaAchievements_RaAchievementId",
                table: "MemberRaAchievements",
                column: "RaAchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberRaAchievements_MemberId_RaAchievementId",
                table: "MemberRaAchievements",
                columns: new[] { "MemberId", "RaAchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaAchievements_RaGameId",
                table: "RaAchievements",
                column: "RaGameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberRaAchievements");

            migrationBuilder.DropTable(
                name: "RaAchievements");
        }
    }
}
