import type { GameStats } from "@/lib/game-stats";
import { formatSyncTime } from "@/lib/format";

type Props = {
  stats: GameStats;
};

export function GameStatsStrip({ stats }: Props) {
  return (
    <section className="grid grid-cols-2 gap-3 sm:grid-cols-4">
      <StatTile label="Leaderboards" value={String(stats.leaderboardCount)} />
      <StatTile label="Friends scored" value={String(stats.membersWithScores)} />
      <StatTile label="Total scores" value={String(stats.totalScores)} />
      <StatTile
        label="Last activity"
        value={stats.lastActivityAt ? formatSyncTime(stats.lastActivityAt) : "—"}
        compact
      />
    </section>
  );
}

function StatTile({
  label,
  value,
  compact = false,
}: {
  label: string;
  value: string;
  compact?: boolean;
}) {
  return (
    <div className="rounded border border-border bg-secondary/20 px-3 py-3">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className={`mt-1 font-mono text-foreground ${compact ? "text-xs" : "text-lg"}`}>
        {value}
      </p>
    </div>
  );
}
