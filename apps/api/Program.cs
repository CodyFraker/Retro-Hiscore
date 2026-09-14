using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Dashboard;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Leaderboards;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Features.Rivalry;
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
builder.Services.AddHttpClient(nameof(DiscordNotificationService));

builder.Services.AddScoped<IRaApiKeyPool, RaApiKeyPool>();
builder.Services.AddScoped<ILeaderboardSyncService, LeaderboardSyncService>();
builder.Services.AddScoped<IGameMetadataSyncService, GameMetadataSyncService>();
builder.Services.AddScoped<IConsoleIconSyncService, ConsoleIconSyncService>();
builder.Services.AddScoped<IDiscordNotificationService, DiscordNotificationService>();
builder.Services.AddTransient<SyncJob>();
builder.Services.AddTransient<GameMetadataSyncJob>();

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
    var raOptions = scope.ServiceProvider.GetRequiredService<IOptions<RaOptions>>().Value;
    await SeedData.EnsureSeededAsync(db, raOptions);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

var consoleIconOptions = app.Services.GetRequiredService<IOptions<ConsoleIconSyncOptions>>().Value;
var systemIconStoragePath = Path.GetFullPath(consoleIconOptions.StoragePath);
Directory.CreateDirectory(systemIconStoragePath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(systemIconStoragePath),
    RequestPath = ConsoleIconSyncService.RequestPath
});

if (!isTesting)
{
    var hangfireOptions = app.Services.GetRequiredService<IOptions<HangfireDashboardOptions>>().Value;
    app.UseHangfireDashboard(hangfireOptions.DashboardPath, new DashboardOptions
    {
        Authorization = [new DashboardSecretAuthorizationFilter(hangfireOptions.DashboardSecret)]
    });

    var syncOptions = app.Services.GetRequiredService<IOptions<SyncOptions>>().Value;
    RecurringJob.AddOrUpdate<SyncJob>(
        "ra-leaderboard-sync",
        job => job.RunScheduledAsync(CancellationToken.None),
        Cron.MinuteInterval(Math.Clamp(syncOptions.IntervalMinutes, 1, 60)));

    RecurringJob.AddOrUpdate<GameMetadataSyncJob>(
        "ra-game-metadata-sync",
        job => job.RunScheduledAsync(CancellationToken.None),
        Cron.Weekly);
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
app.MapGetMembers();
app.MapGetCurrentMember();
app.MapPutMemberApiKey();
app.MapPutMemberProfile();
app.MapGetRivalry();
app.MapGetMember();
app.MapGetMemberHistory();
app.MapGetGames();
app.MapAddGame();
app.MapDeleteGame();
app.MapGetGameLeaderboards();
app.MapGetGameHistory();
app.MapGetLeaderboard();
app.MapGetLeaderboardHistory();
app.MapTriggerSync();
app.MapGetSyncStatus();
app.MapTriggerMetadataSync();
app.MapGetMetadataSyncStatus();
app.MapTriggerConsoleIconSync();
app.MapGetConsoleIconSyncStatus();

app.Run();

public partial class Program;
