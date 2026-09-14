import type { GameStats } from "@/lib/game-stats";
import { StatGrid } from "@/components/layout/stat-grid";
import { formatSyncTime } from "@/lib/format";

type Props = {
  stats: GameStats;
};

export function GameStatsStrip({ stats }: Props) {
  return (
    <StatGrid
      items={[
        { label: "Leaderboards", value: String(stats.leaderboardCount) },
        { label: "Friends scored", value: String(stats.membersWithScores) },
        { label: "Total scores", value: String(stats.totalScores) },
        {
          label: "Ranked entries (all boards)",
          value:
            stats.totalRankedEntriesAcrossBoards > 0
              ? stats.totalRankedEntriesAcrossBoards.toLocaleString()
              : "—",
          compact: true,
        },
        {
          label: "Busiest board",
          value: stats.busiestBoard
            ? `${stats.busiestBoard.globalEntryCount.toLocaleString()} · ${stats.busiestBoard.title}`
            : "—",
          compact: true,
        },
        {
          label: "Last activity",
          value: stats.lastActivityAt ? formatSyncTime(stats.lastActivityAt) : "—",
          compact: true,
        },
      ]}
    />
  );
}
