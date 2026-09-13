using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGameMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "SyncRuns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConsoleId",
                table: "Games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsoleName",
                table: "Games",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Developer",
                table: "Games",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Games",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageBoxArt",
                table: "Games",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageIcon",
                table: "Games",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageIngame",
                table: "Games",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageTitle",
                table: "Games",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MetadataSyncedAt",
                table: "Games",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "Games",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReleasedAt",
                table: "Games",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "SyncRuns");

            migrationBuilder.DropColumn(
                name: "ConsoleId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ConsoleName",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Developer",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ImageBoxArt",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ImageIcon",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ImageIngame",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ImageTitle",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "MetadataSyncedAt",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ReleasedAt",
                table: "Games");
        }
    }
}
