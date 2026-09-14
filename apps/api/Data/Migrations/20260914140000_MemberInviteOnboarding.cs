using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260914140000_MemberInviteOnboarding")]
    public partial class MemberInviteOnboarding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Members",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "RaUsername",
                table: "Members",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RaUsername",
                table: "Members",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Members");
        }
    }
}
