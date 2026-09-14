using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Options;
using Testcontainers.PostgreSql;

namespace RetroHiscore.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private string? _connectionString;

    public IRaApiClient RaApiClient { get; } = Substitute.For<IRaApiClient>();
    public IConsoleIconDownloader ConsoleIconDownloader { get; } = Substitute.For<IConsoleIconDownloader>();

    public static readonly byte[] SamplePngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public const string SignInServiceKey = "test-sign-in-service-key";

    public async Task InitializeAsync()
    {
        await EnsurePostgresStartedAsync();
        ResetMocks();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        EnsurePostgresStartedAsync().GetAwaiter().GetResult();
        var connectionString = _connectionString!;

        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString,
                ["Sync:ManualCooldownSeconds"] = "60",
                ["GameMetadataSync:ManualCooldownSeconds"] = "300",
                ["ConsoleIconSync:ManualCooldownSeconds"] = "300",
                ["ConsoleIconSync:ForceRefresh"] = "false",
                ["RA:ApiKey"] = "test-key",
                ["RA:Username"] = "test-user",
                ["RA:MediaBaseUrl"] = "https://media.retroachievements.org",
                ["Auth:JwtSigningKey"] = AuthTestHelper.TestSigningKey,
                ["Auth:SignInServiceKey"] = SignInServiceKey,
                ["Auth:AdminDiscordUserIds:0"] = AuthTestHelper.AdminDiscordUserId,
                ["AUTH_ADMIN_DISCORD_USER_IDS"] = AuthTestHelper.AdminDiscordUserId,
                ["AUTH_SIGN_IN_SERVICE_KEY"] = SignInServiceKey,
                ["Auth:WebOrigin"] = "http://localhost",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            services.PostConfigure<AuthOptions>(options =>
            {
                options.JwtSigningKey = AuthTestHelper.TestSigningKey;
                options.WebOrigin = "http://localhost";
                options.SignInServiceKey = SignInServiceKey;
                options.AdminDiscordUserIds = [AuthTestHelper.AdminDiscordUserId];
            });

            services.PostConfigure<RaOptions>(options =>
            {
                options.ApiKey = "test-key";
                options.Username = "test-user";
                options.MediaBaseUrl = "https://media.retroachievements.org";
            });

            services.PostConfigure<SyncOptions>(options => options.ManualCooldownSeconds = 60);
            services.PostConfigure<GameMetadataSyncOptions>(options => options.ManualCooldownSeconds = 300);
            services.PostConfigure<ConsoleIconSyncOptions>(options =>
            {
                options.ManualCooldownSeconds = 300;
                options.ForceRefresh = false;
            });

            services.RemoveAll<IRaApiClient>();
            services.AddSingleton(RaApiClient);
            services.RemoveAll<IConsoleIconDownloader>();
            services.AddSingleton(ConsoleIconDownloader);
        });
    }

    public HttpClient CreateAuthenticatedClient(string? discordUserId = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            AuthTestHelper.CreateToken(discordUserId ?? AuthTestHelper.AllowedDiscordUserId));
        return client;
    }

    public HttpClient CreateSignInCheckClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-SignIn-Service-Key", SignInServiceKey);
        return client;
    }

    public void ResetMocks()
    {
        RaApiClient.ClearSubstitute();
        ConsoleIconDownloader.ClearSubstitute();

        RaApiClient
            .GetConsoleIdsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaConsoleIdDto>>([]));

        RaApiClient
            .GetUserSummaryAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var username = call.ArgAt<string>(0);
                return Task.FromResult<RaUserSummaryDto?>(new RaUserSummaryDto
                {
                    User = username,
                    Ulid = "test-ulid"
                });
            });

        RaApiClient
            .GetUserRecentlyPlayedGamesAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaUserRecentlyPlayedGameDto>>([]));

        RaApiClient
            .GetLeaderboardEntryCountAsync(Arg.Any<long>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<int?>(0));

        ConsoleIconDownloader
            .DownloadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ConsoleIconDownload(SamplePngBytes, "image/png")));
    }

    public async Task SetMemberApiKeyAsync(string raUsername, string? apiKey)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == raUsername);
        member.RaApiKey = apiKey;
        await db.SaveChangesAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        ResetMocks();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        await db.Database.ExecuteSqlRawAsync(
            """
            DO $$ DECLARE
                table_name text;
            BEGIN
                FOR table_name IN
                    SELECT tablename FROM pg_tables
                    WHERE schemaname = 'public' AND tablename <> '__EFMigrationsHistory'
                LOOP
                    EXECUTE format('TRUNCATE TABLE %I RESTART IDENTITY CASCADE', table_name);
                END LOOP;
            END $$;
            """);

        await SeedData.EnsureSeededAsync(db, new RaOptions());
        await EnsureDefaultTestMembersAsync(db);
    }

    private static async Task EnsureDefaultTestMembersAsync(AppDbContext db)
    {
        var admin = await db.Members.FirstOrDefaultAsync(m => m.DiscordId == AuthTestHelper.AdminDiscordUserId);
        if (admin is null)
        {
            admin = new Member
            {
                DiscordId = AuthTestHelper.AdminDiscordUserId,
                IsAdmin = true,
            };
            db.Members.Add(admin);
        }

        admin.RaUsername = "ShrimpPoboy";
        admin.DisplayName = "ShrimpPoboy";
        admin.IsAdmin = true;

        if (!await db.Members.AnyAsync(m => m.DiscordId == AuthTestHelper.SecondAllowedDiscordUserId))
        {
            db.Members.Add(new Member
            {
                DiscordId = AuthTestHelper.SecondAllowedDiscordUserId,
                RaUsername = "beefboybilly",
                DisplayName = "beefboybilly"
            });
        }

        if (!await db.Members.AnyAsync(m => m.RaUsername == "xXScubXx"))
        {
            db.Members.Add(new Member
            {
                RaUsername = "xXScubXx",
                DisplayName = "xXScubXx"
            });
        }

        await db.SaveChangesAsync();
    }

    private async Task EnsurePostgresStartedAsync()
    {
        if (_connectionString is not null)
        {
            return;
        }

        await _postgres.StartAsync();
        _connectionString = _postgres.GetConnectionString();
    }
}
