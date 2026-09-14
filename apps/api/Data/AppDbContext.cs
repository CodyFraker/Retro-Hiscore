using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Leaderboard> Leaderboards => Set<Leaderboard>();
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();
    public DbSet<LeaderboardEntrySnapshot> LeaderboardEntrySnapshots => Set<LeaderboardEntrySnapshot>();
    public DbSet<LeaderboardPopulationSnapshot> LeaderboardPopulationSnapshots => Set<LeaderboardPopulationSnapshot>();
    public DbSet<MemberRaRankSnapshot> MemberRaRankSnapshots => Set<MemberRaRankSnapshot>();
    public DbSet<RaAchievement> RaAchievements => Set<RaAchievement>();
    public DbSet<MemberRaAchievement> MemberRaAchievements => Set<MemberRaAchievement>();
    public DbSet<SyncRun> SyncRuns => Set<SyncRun>();
    public DbSet<RaConsole> Consoles => Set<RaConsole>();
    public DbSet<GameSource> GameSources => Set<GameSource>();
    public DbSet<MemberRecentGamePlay> MemberRecentGamePlays => Set<MemberRecentGamePlay>();
    public DbSet<GameTrackQueue> GameTrackQueues => Set<GameTrackQueue>();
    public DbSet<SyncLeaderboardSettings> SyncLeaderboardSettings => Set<SyncLeaderboardSettings>();
    public DbSet<SyncRecurringJob> SyncRecurringJobs => Set<SyncRecurringJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(e =>
        {
            e.HasIndex(x => x.RaUsername).IsUnique();
            e.HasIndex(x => x.DiscordId).IsUnique();
            e.Property(x => x.RaUsername).HasMaxLength(128);
            e.Property(x => x.RaUlid).HasMaxLength(64);
            e.Property(x => x.DisplayName).HasMaxLength(128);
            e.Property(x => x.DiscordId).HasMaxLength(32);
            e.Property(x => x.AvatarUrl).HasMaxLength(512);
            e.Property(x => x.RaApiKey).HasMaxLength(256);
        });

        modelBuilder.Entity<Game>(e =>
        {
            e.HasIndex(x => x.RaGameId).IsUnique();
            e.Property(x => x.Title).HasMaxLength(256);
            e.Property(x => x.ConsoleName).HasMaxLength(128);
            e.Property(x => x.ImageIcon).HasMaxLength(256);
            e.Property(x => x.ImageTitle).HasMaxLength(256);
            e.Property(x => x.ImageIngame).HasMaxLength(256);
            e.Property(x => x.ImageBoxArt).HasMaxLength(256);
            e.Property(x => x.Publisher).HasMaxLength(256);
            e.Property(x => x.Developer).HasMaxLength(256);
            e.Property(x => x.Genre).HasMaxLength(256);
        });

        modelBuilder.Entity<GameSource>(e =>
        {
            e.HasIndex(x => x.GameId);
            e.Property(x => x.Url).HasMaxLength(2048).IsRequired();
            e.Property(x => x.Label).HasMaxLength(128);
            e.Property(x => x.Note).HasMaxLength(512);
            e.HasOne(x => x.Game)
                .WithMany(x => x.Sources)
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RaConsole>(e =>
        {
            e.HasKey(x => x.RaConsoleId);
            e.Property(x => x.RaConsoleId).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.IconContentType).HasMaxLength(64);
            e.ToTable("Consoles");
        });

        modelBuilder.Entity<Leaderboard>(e =>
        {
            e.HasIndex(x => x.RaLeaderboardId).IsUnique();
            e.Property(x => x.Title).HasMaxLength(256);
            e.Property(x => x.Description).HasMaxLength(1024);
            e.Property(x => x.Format).HasMaxLength(64);
            e.HasOne(x => x.Game).WithMany(x => x.Leaderboards).HasForeignKey(x => x.GameId);
        });

        modelBuilder.Entity<LeaderboardEntry>(e =>
        {
            e.HasIndex(x => new { x.LeaderboardId, x.MemberId }).IsUnique();
            e.Property(x => x.FormattedScore).HasMaxLength(64);
            e.HasOne(x => x.Leaderboard).WithMany(x => x.Entries).HasForeignKey(x => x.LeaderboardId);
            e.HasOne(x => x.Member).WithMany(x => x.Entries).HasForeignKey(x => x.MemberId);
        });

        modelBuilder.Entity<LeaderboardEntrySnapshot>(e =>
        {
            e.HasIndex(x => new { x.LeaderboardId, x.MemberId, x.SyncedAt });
            e.Property(x => x.FormattedScore).HasMaxLength(64);
            e.HasOne(x => x.Leaderboard).WithMany(x => x.Snapshots).HasForeignKey(x => x.LeaderboardId);
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
        });

        modelBuilder.Entity<LeaderboardPopulationSnapshot>(e =>
        {
            e.HasIndex(x => new { x.LeaderboardId, x.SyncedAt });
            e.HasOne(x => x.Leaderboard)
                .WithMany(x => x.PopulationSnapshots)
                .HasForeignKey(x => x.LeaderboardId);
        });

        modelBuilder.Entity<MemberRaRankSnapshot>(e =>
        {
            e.HasIndex(x => new { x.MemberId, x.SyncedAt });
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
        });

        modelBuilder.Entity<RaAchievement>(e =>
        {
            e.HasKey(x => x.RaAchievementId);
            e.Property(x => x.RaAchievementId).ValueGeneratedNever();
            e.HasIndex(x => x.RaGameId);
            e.Property(x => x.Title).HasMaxLength(256).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1024);
            e.Property(x => x.BadgeName).HasMaxLength(64);
            e.Property(x => x.Type).HasMaxLength(64);
            e.Property(x => x.BadgeContentType).HasMaxLength(64);
        });

        modelBuilder.Entity<MemberRaAchievement>(e =>
        {
            e.HasIndex(x => x.RaAchievementId);
            e.HasIndex(x => new { x.MemberId, x.RaAchievementId }).IsUnique();
            e.HasIndex(x => new { x.MemberId, x.DateEarned });
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
            e.HasOne(x => x.Achievement)
                .WithMany()
                .HasForeignKey(x => x.RaAchievementId);
        });

        modelBuilder.Entity<SyncRun>(e =>
        {
            e.Property(x => x.Error).HasMaxLength(4000);
            e.HasIndex(x => new { x.Kind, x.GameId, x.StartedAt });
            e.HasOne(x => x.Game).WithMany().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MemberRecentGamePlay>(e =>
        {
            e.HasKey(x => new { x.MemberId, x.RaGameId });
            e.Property(x => x.Title).HasMaxLength(256);
            e.Property(x => x.ConsoleName).HasMaxLength(128);
            e.Property(x => x.ImageIcon).HasMaxLength(256);
            e.Property(x => x.ImageBoxArt).HasMaxLength(256);
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
            e.HasIndex(x => x.RaGameId);
            e.HasIndex(x => x.LastPlayedAt);
        });

        modelBuilder.Entity<GameTrackQueue>(e =>
        {
            e.HasIndex(x => x.RaGameId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => new { x.RaGameId, x.Status });
            e.Property(x => x.Title).HasMaxLength(256);
            e.Property(x => x.ConsoleName).HasMaxLength(128);
            e.Property(x => x.FailureMessage).HasMaxLength(2000);
            e.HasOne(x => x.ResolvedByMember).WithMany().HasForeignKey(x => x.ResolvedByMemberId);
        });

        modelBuilder.Entity<SyncLeaderboardSettings>(e =>
        {
            e.Property(x => x.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<SyncRecurringJob>(e =>
        {
            e.HasKey(x => x.JobId);
            e.Property(x => x.JobId).HasMaxLength(64);
            e.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
        });
    }
}
