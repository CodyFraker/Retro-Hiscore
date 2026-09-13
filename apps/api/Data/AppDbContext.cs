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
    public DbSet<SyncRun> SyncRuns => Set<SyncRun>();
    public DbSet<RaConsole> Consoles => Set<RaConsole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(e =>
        {
            e.HasIndex(x => x.RaUsername).IsUnique();
            e.Property(x => x.RaUsername).HasMaxLength(128);
            e.Property(x => x.RaUlid).HasMaxLength(64);
            e.Property(x => x.DisplayName).HasMaxLength(128);
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

        modelBuilder.Entity<RaConsole>(e =>
        {
            e.HasKey(x => x.RaConsoleId);
            e.Property(x => x.RaConsoleId).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.IconFileName).HasMaxLength(64);
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

        modelBuilder.Entity<SyncRun>(e =>
        {
            e.Property(x => x.Error).HasMaxLength(4000);
        });
    }
}
