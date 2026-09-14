using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260914170000_MemberRaRankSnapshotPoints")]
    public partial class MemberRaRankSnapshotPoints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Rank",
                table: "MemberRaRankSnapshots",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "TotalPoints",
                table: "MemberRaRankSnapshots",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSoftcorePoints",
                table: "MemberRaRankSnapshots",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalTruePoints",
                table: "MemberRaRankSnapshots",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPoints",
                table: "MemberRaRankSnapshots");

            migrationBuilder.DropColumn(
                name: "TotalSoftcorePoints",
                table: "MemberRaRankSnapshots");

            migrationBuilder.DropColumn(
                name: "TotalTruePoints",
                table: "MemberRaRankSnapshots");

            migrationBuilder.AlterColumn<int>(
                name: "Rank",
                table: "MemberRaRankSnapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
