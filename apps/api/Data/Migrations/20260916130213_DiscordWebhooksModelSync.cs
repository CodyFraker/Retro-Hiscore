using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroHiscore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class DiscordWebhooksModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordWebhookConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    WebhookUrlProtected = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    DigestIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    PayloadTemplateJson = table.Column<string>(type: "text", nullable: false),
                    AllowedRaGameIds = table.Column<int[]>(type: "integer[]", nullable: true),
                    LastDispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordWebhookConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationDispatchRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EventsProcessed = table.Column<int>(type: "integer", nullable: false),
                    PostsSucceeded = table.Column<int>(type: "integer", nullable: false),
                    PostsFailed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDispatchRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationOutbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordWebhookEventSubscriptions",
                columns: table => new
                {
                    WebhookConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordWebhookEventSubscriptions", x => new { x.WebhookConfigId, x.EventKind });
                    table.ForeignKey(
                        name: "FK_DiscordWebhookEventSubscriptions_DiscordWebhookConfigs_Webh~",
                        column: x => x.WebhookConfigId,
                        principalTable: "DiscordWebhookConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationOutboxDeliveries",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    WebhookConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationOutboxDeliveries", x => new { x.OutboxId, x.WebhookConfigId });
                    table.ForeignKey(
                        name: "FK_NotificationOutboxDeliveries_DiscordWebhookConfigs_WebhookC~",
                        column: x => x.WebhookConfigId,
                        principalTable: "DiscordWebhookConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationOutboxDeliveries_NotificationOutbox_OutboxId",
                        column: x => x.OutboxId,
                        principalTable: "NotificationOutbox",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatchRuns_StartedAt",
                table: "NotificationDispatchRuns",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_DispatchedAt_OccurredAt",
                table: "NotificationOutbox",
                columns: new[] { "DispatchedAt", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutboxDeliveries_WebhookConfigId",
                table: "NotificationOutboxDeliveries",
                column: "WebhookConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordWebhookEventSubscriptions");

            migrationBuilder.DropTable(
                name: "NotificationDispatchRuns");

            migrationBuilder.DropTable(
                name: "NotificationOutboxDeliveries");

            migrationBuilder.DropTable(
                name: "DiscordWebhookConfigs");

            migrationBuilder.DropTable(
                name: "NotificationOutbox");
        }
    }
}
