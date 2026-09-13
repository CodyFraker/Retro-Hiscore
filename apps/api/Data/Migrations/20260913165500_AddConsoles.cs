using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConsoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consoles",
                columns: table => new
                {
                    RaConsoleId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IconFileName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IconSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consoles", x => x.RaConsoleId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consoles");
        }
    }
}
