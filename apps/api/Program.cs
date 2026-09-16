using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Admin;
using RetroHiscore.Api.Features.Auth;
using RetroHiscore.Api.Features.Dashboard;
using RetroHiscore.Api.Features.GameOfTheWeek;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Leaderboards;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Features.Admin.DiscordWebhooks;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Features.Rivalry;
using RetroHiscore.Api.Features.Search;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

builder.Services.Configure<RaOptions>(builder.Configuration.GetSection(RaOptions.SectionName));
builder.Services.Configure<SyncOptions>(builder.Configuration.GetSection(SyncOptions.SectionName));
builder.Services.Configure<GameMetadataSyncOptions>(builder.Configuration.GetSection(GameMetadataSyncOptions.SectionName));
builder.Services.Configure<ConsoleIconSyncOptions>(builder.Configuration.GetSection(ConsoleIconSyncOptions.SectionName));
builder.Services.Configure<HangfireDashboardOptions>(builder.Configuration.GetSection(HangfireDashboardOptions.SectionName));
builder.Services.Configure<ScalarDashboardOptions>(builder.Configuration.GetSection(ScalarDashboardOptions.SectionName));
builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection(NotificationOptions.SectionName));
builder.Services.AddDataProtection();
builder.Services.AddRetroHiscoreAuth(builder.Configuration, isTesting);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient<IRaApiClient, RaApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHttpClient<IConsoleIconDownloader, ConsoleIconDownloader>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient(nameof(DiscordWebhookClient));

builder.Services.AddScoped<IRaApiKeyPool, RaApiKeyPool>();
builder.Services.AddScoped<ILeaderboardSyncService, LeaderboardSyncService>();
builder.Services.AddScoped<ILeaderboardSyncDispatcher, LeaderboardSyncDispatcher>();
builder.Services.AddScoped<IMemberActivitySyncService, MemberActivitySyncService>();
builder.Services.AddScoped<IMemberRaRankSnapshotSync, MemberRaRankSnapshotSync>();
builder.Services.AddScoped<IMemberRankSyncService, MemberRankSyncService>();
builder.Services.AddScoped<IMemberSelfSyncService, MemberSelfSyncService>();
builder.Services.AddScoped<IMemberRaGameProgressSyncService, MemberRaGameProgressSyncService>();
builder.Services.AddScoped<IMemberAchievementSyncService, MemberAchievementSyncService>();
builder.Services.AddScoped<IGameAchievementDistributionSyncService, GameAchievementDistributionSyncService>();
builder.Services.AddScoped<IGameTrackQueueService, GameTrackQueueService>();
builder.Services.AddScoped<IMemberRecentGamesSyncService, MemberRecentGamesSyncService>();
builder.Services.AddScoped<IGameMetadataSyncService, GameMetadataSyncService>();
builder.Services.AddScoped<IConsoleIconSyncService, ConsoleIconSyncService>();
builder.Services.AddSingleton<IWebhookUrlProtector, WebhookUrlProtector>();
builder.Services.AddScoped<IDiscordWebhookTemplateRenderer, DiscordWebhookTemplateRenderer>();
builder.Services.AddScoped<IDiscordWebhookPayloadValidator, DiscordWebhookPayloadValidator>();
builder.Services.AddScoped<IDiscordWebhookClient, DiscordWebhookClient>();
builder.Services.AddScoped<INotificationOutboxWriter, NotificationOutboxWriter>();
builder.Services.AddScoped<INotificationOutboxReadinessService, NotificationOutboxReadinessService>();
builder.Services.AddScoped<ILeaderboardSyncNotificationService, LeaderboardSyncNotificationService>();
builder.Services.AddScoped<IDiscordWebhookDispatchService, DiscordWebhookDispatchService>();
builder.Services.AddScoped<IDiscordWebhookStore, DiscordWebhookStore>();
builder.Services.AddScoped<AdminOpsBuilder>();
builder.Services.AddScoped<ISyncSettingsStore, SyncSettingsStore>();
builder.Services.AddScoped<IGameOfTheWeekRaGameResolver, GameOfTheWeekRaGameResolver>();
builder.Services.AddScoped<IGameOfTheWeekPollService, GameOfTheWeekPollService>();
builder.Services.AddScoped<IGameOfTheWeekBallotService, GameOfTheWeekBallotService>();
builder.Services.AddScoped<IGameOfTheWeekVoteService, GameOfTheWeekVoteService>();
builder.Services.AddScoped<IGameOfTheWeekCloseService, GameOfTheWeekCloseService>();
builder.Services.AddScoped<IGameOfTheWeekWinnerTrackingService, GameOfTheWeekWinnerTrackingService>();
if (isTesting)
{
    builder.Services.AddSingleton<IAdminSchedulerReader, NullAdminSchedulerReader>();
    builder.Services.AddSingleton<IRecurringSyncJobRegistrar, NullRecurringSyncJobRegistrar>();
}
else
{
    builder.Services.AddSingleton<IAdminSchedulerReader, HangfireAdminSchedulerReader>();
    builder.Services.AddScoped<IRecurringSyncJobRegistrar, RecurringSyncJobRegistrar>();
}
builder.Services.AddTransient<MemberActivitySyncJob>();
builder.Services.AddTransient<MemberRankSyncJob>();
builder.Services.AddTransient<MemberAchievementSyncJob>();
builder.Services.AddTransient<LeaderboardSyncDispatchJob>();
builder.Services.AddTransient<GameLeaderboardSyncJob>();
builder.Services.AddTransient<MemberGameLeaderboardSyncJob>();
builder.Services.AddTransient<GameMetadataSyncJob>();
builder.Services.AddTransient<DiscordNotificationDispatchJob>();
builder.Services.AddTransient<GameOfTheWeekPollJob>();
if (isTesting)
{
    builder.Services.AddSingleton<ILeaderboardSyncJobEnqueuer, NullLeaderboardSyncJobEnqueuer>();
}
else
{
    builder.Services.AddSingleton<ILeaderboardSyncJobEnqueuer, HangfireLeaderboardSyncJobEnqueuer>();
}

if (!isTesting)
{
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

    builder.Services.AddHangfireServer();
}

builder.Services.AddOpenApi();
builder.Services.AddRetroHiscoreCors(builder.Configuration);

if (!isTesting)
{
    builder.Services.AddHealthChecks().AddNpgSql(connectionString);
}
else
{
    builder.Services.AddHealthChecks();
}

var app = builder.Build();

if (!isTesting)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var registrar = scope.ServiceProvider.GetRequiredService<IRecurringSyncJobRegistrar>();
    await registrar.RegisterAllAsync();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (!isTesting)
{
    var hangfireOptions = app.Services.GetRequiredService<IOptions<HangfireDashboardOptions>>().Value;
    app.UseHangfireDashboard(hangfireOptions.DashboardPath, new DashboardOptions
    {
        Authorization = [new DashboardSecretAuthorizationFilter(hangfireOptions.DashboardSecret)]
    });

}

app.MapOpenApi();
app.MapScalarApiReference("/scalar", options =>
{
    options.WithTitle("Retro Hiscore API");
});

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/scalar"))
    {
        var scalarOptions = context.RequestServices.GetRequiredService<IOptions<ScalarDashboardOptions>>().Value;
        if (!string.IsNullOrWhiteSpace(scalarOptions.DashboardSecret))
        {
            var ok = context.Request.Headers.TryGetValue("X-Dashboard-Secret", out var header)
                && string.Equals(header.ToString(), scalarOptions.DashboardSecret, StringComparison.Ordinal);
            var queryOk = context.Request.Query.TryGetValue("secret", out var query)
                && string.Equals(query.ToString(), scalarOptions.DashboardSecret, StringComparison.Ordinal);
            if (!ok && !queryOk)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }
    }

    await next();
});

app.MapHealthChecks("/health");
app.MapGetDashboard();
app.MapGetDashboardGames();
app.MapGetDashboardAchievementActivity();
app.MapGetDashboardAchievementSummary();
app.MapGetDashboardAchievementHistory();
app.MapGetDashboardGroupActivity();
app.MapGetMembers();
app.MapGetMembersSummary();
app.MapGetCurrentMember();
app.MapGetMemberSelfSyncStatus();
app.MapPostMemberSelfSyncLeaderboards();
app.MapPostMemberSelfSyncProfile();
app.MapPostMemberSelfSyncAchievements();
app.MapPutMemberApiKey();
app.MapPutMemberProfile();
app.MapGetRivalry();
app.MapGetSearch();
app.MapGetMember();
app.MapGetMemberRaSummary();
app.MapGetMemberRaRankHistory();
app.MapGetMemberRaAchievementHistory();
app.MapGetMemberAchievements();
app.MapGetMemberHistory();
app.MapGetGames();
app.MapGetGameTrackQueue();
app.MapGetGameLeaderboards();
app.MapGetGameHistory();
app.MapGetGameLeaderboardPopulationHistory();
app.MapGetGameSources();
app.MapGetGameAchievements();
app.MapGetGameAchievementDistribution();
app.MapPostGameRefresh();
app.MapGetLeaderboard();
app.MapGetLeaderboardHistory();
app.MapTriggerSync();
app.MapGetSyncStatus();
app.MapGetSyncHealth();
app.MapTriggerDueDispatchSync();
app.MapTriggerMetadataSync();
app.MapGetMetadataSyncStatus();
app.MapTriggerConsoleIconSync();
app.MapGetConsoleIconSyncStatus();
app.MapTriggerMemberActivitySync();
app.MapGetMemberActivitySyncStatus();
app.MapTriggerMemberRankSync();
app.MapGetMemberRankSyncStatus();
app.MapTriggerMemberAchievementSync();
app.MapGetMemberAchievementSyncStatus();
app.MapGetAdminOps();
app.MapGetAdminSyncSettings();
app.MapPatchAdminSyncSettings();
app.MapGetAdminGames();
app.MapPostAdminGame();
app.MapGetAdminGameTrackQueue();
app.MapPostAdminGameTrackQueueReject();
app.MapDeleteAdminGame();
app.MapPostAdminGameRefresh();
app.MapPatchAdminGameLeaderboardSync();
app.MapGetAdminGameSources();
app.MapPostAdminGameSource();
app.MapPutAdminGameSource();
app.MapDeleteAdminGameSource();
app.MapGetSignInCheck();
app.MapGetAdminMemberInvites();
app.MapPostAdminMemberInvite();
app.MapDeleteAdminMemberInvite();
app.MapPatchAdminMemberIsAdmin();
app.MapGetAdminDiscordWebhookTokenCatalog();
app.MapGetAdminDiscordWebhookDispatchRuns();
app.MapGetAdminDiscordWebhooks();
app.MapPostAdminDiscordWebhook();
app.MapGetAdminDiscordWebhook();
app.MapPutAdminDiscordWebhook();
app.MapDeleteAdminDiscordWebhook();
app.MapPostAdminDiscordWebhookPreview();
app.MapPostAdminDiscordWebhookTest();
app.MapPutMemberRaAccount();
app.MapGetGameOfTheWeekCurrent();
app.MapGetGameOfTheWeekHistory();
app.MapPostGameOfTheWeekBallot();
app.MapPutGameOfTheWeekVote();
app.MapPostAdminGameOfTheWeekPoll();
app.MapGetAdminGameOfTheWeekCurrentPoll();

app.Run();

public partial class Program;
