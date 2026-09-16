using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetroHiscore.Api.Data;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260916140000_NotificationOutboxReadyAt")]
    public partial class NotificationOutboxReadyAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReadyAt",
                table: "NotificationOutbox",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceSyncRunId",
                table: "NotificationOutbox",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "NotificationOutbox"
                SET "ReadyAt" = "OccurredAt"
                WHERE "ReadyAt" IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_DispatchedAt_ReadyAt",
                table: "NotificationOutbox",
                columns: new[] { "DispatchedAt", "ReadyAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_SourceSyncRunId",
                table: "NotificationOutbox",
                column: "SourceSyncRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationOutbox_SyncRuns_SourceSyncRunId",
                table: "NotificationOutbox",
                column: "SourceSyncRunId",
                principalTable: "SyncRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationOutbox_SyncRuns_SourceSyncRunId",
                table: "NotificationOutbox");

            migrationBuilder.DropIndex(
                name: "IX_NotificationOutbox_SourceSyncRunId",
                table: "NotificationOutbox");

            migrationBuilder.DropIndex(
                name: "IX_NotificationOutbox_DispatchedAt_ReadyAt",
                table: "NotificationOutbox");

            migrationBuilder.DropColumn(
                name: "ReadyAt",
                table: "NotificationOutbox");

            migrationBuilder.DropColumn(
                name: "SourceSyncRunId",
                table: "NotificationOutbox");
        }
    }
}
