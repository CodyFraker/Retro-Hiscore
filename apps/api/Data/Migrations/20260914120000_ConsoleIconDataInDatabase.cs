using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260914120000_ConsoleIconDataInDatabase")]
    public partial class ConsoleIconDataInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconFileName",
                table: "Consoles");

            migrationBuilder.AddColumn<byte[]>(
                name: "IconData",
                table: "Consoles",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconContentType",
                table: "Consoles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconData",
                table: "Consoles");

            migrationBuilder.DropColumn(
                name: "IconContentType",
                table: "Consoles");

            migrationBuilder.AddColumn<string>(
                name: "IconFileName",
                table: "Consoles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}
