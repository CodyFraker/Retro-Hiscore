using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260915130000_MemberRaPresenceCache")]
    public partial class MemberRaPresenceCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RaPresenceGameTitle",
                table: "Members",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RaPresenceRaGameId",
                table: "Members",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RaPresenceRichPresenceAt",
                table: "Members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RaPresenceSyncedAt",
                table: "Members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RaStatus",
                table: "Members",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RaPresenceGameTitle",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "RaPresenceRaGameId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "RaPresenceRichPresenceAt",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "RaPresenceSyncedAt",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "RaStatus",
                table: "Members");
        }
    }
}
