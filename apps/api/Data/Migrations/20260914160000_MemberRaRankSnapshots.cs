using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260914160000_MemberRaRankSnapshots")]
    public partial class MemberRaRankSnapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberRaRankSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    TotalRanked = table.Column<int>(type: "integer", nullable: true),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRaRankSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberRaRankSnapshots_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberRaRankSnapshots_MemberId_SyncedAt",
                table: "MemberRaRankSnapshots",
                columns: new[] { "MemberId", "SyncedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberRaRankSnapshots");
        }
    }
}
