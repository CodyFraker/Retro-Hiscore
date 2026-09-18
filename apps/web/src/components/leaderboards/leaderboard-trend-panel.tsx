import { Clock } from "lucide-react";
import { DeltaCallout } from "@/components/charts/delta-callout";
import { GlobalRankTrendChart } from "@/components/charts/global-rank-trend-chart";
import { LeaderboardEntryCountTrendChart } from "@/components/charts/leaderboard-entry-count-trend-chart";
import { ScoreTrendChart } from "@/components/charts/score-trend-chart";
import { LeaderboardHistoryPagination } from "@/components/leaderboards/leaderboard-history-pagination";
import { LeaderboardHistorySection } from "@/components/leaderboards/leaderboard-history-section";
import type { LeaderboardHistoryItemDto } from "@/generated/api-client";
import {
  computeLeaderboardPopulationDelta,
  computeMemberDeltas,
  dedupeLeaderboardHistoryItems,
  enrichLeaderboardHistoryRows,
  LEADERBOARD_HISTORY_PAGE_SIZE,
  parseLeaderboardHistoryPage,
  toChartSeries,
  toLeaderboardPopulationSeries,
} from "@/lib/history-series";

type Props = {
  format?: string | null;
  historyItems: LeaderboardHistoryItemDto[];
  historyPage: number;
  historyPageHref: (page: number) => string;
  recentHistoryMemberId?: string;
  showDeltaCallout?: boolean;
};

export function LeaderboardTrendPanel({
  format,
  historyItems,
  historyPage: historyPageParam,
  historyPageHref,
  recentHistoryMemberId,
  showDeltaCallout = false,
}: Props) {
  const series = toChartSeries(historyItems);
  const populationSeries = toLeaderboardPopulationSeries(historyItems);
  const deltas = computeMemberDeltas(series);
  const populationDelta = computeLeaderboardPopulationDelta(historyItems);
  const dedupedHistory = dedupeLeaderboardHistoryItems(historyItems);
  const dedupedHistoryIds = new Set(dedupedHistory.map((row) => row.id));
  let enrichedHistory = enrichLeaderboardHistoryRows(historyItems).filter((row) =>
    dedupedHistoryIds.has(row.id),
  );

  if (recentHistoryMemberId) {
    enrichedHistory = enrichedHistory.filter((row) => row.memberId === recentHistoryMemberId);
  }

  const historyPageCount = Math.max(
    1,
    Math.ceil(enrichedHistory.length / LEADERBOARD_HISTORY_PAGE_SIZE),
  );
  const historyPage = parseLeaderboardHistoryPage(String(historyPageParam), historyPageCount);
  const historyOffset = (historyPage - 1) * LEADERBOARD_HISTORY_PAGE_SIZE;
  const historyPageItems = enrichedHistory.slice(
    historyOffset,
    historyOffset + LEADERBOARD_HISTORY_PAGE_SIZE,
  );

  return (
    <div className="space-y-10">
      <section className="space-y-3">
        <h2 className="steam-section-heading">Score trend</h2>
        <ScoreTrendChart series={series} scoreFormat={format} />
      </section>

      <section className="space-y-3">
        <h2 className="steam-section-heading">Global rank over time</h2>
        <GlobalRankTrendChart series={series} historyItems={historyItems} />
      </section>

      <section className="space-y-3">
        <h2 className="steam-section-heading">Total entries over time</h2>
        <p className="text-xs text-muted-foreground">
          Ranked players on RetroAchievements at each sync—growth shows new submissions on this
          board.
        </p>
        <LeaderboardEntryCountTrendChart points={populationSeries} />
      </section>

      {showDeltaCallout ? (
        <DeltaCallout
          deltas={deltas}
          scoreFormat={format}
          globalEntryCountDelta={populationDelta?.delta}
        />
      ) : null}

      <section className="space-y-3">
        <h2 className="steam-section-heading">Recent history</h2>
        {historyItems.length === 0 ? (
          <p className="flex items-center gap-2 text-muted-foreground">
            <Clock className="size-4 shrink-0" />
            No snapshots yet.
          </p>
        ) : enrichedHistory.length === 0 ? (
          <p className="flex items-center gap-2 text-muted-foreground">
            <Clock className="size-4 shrink-0" />
            No history rows for this view yet.
          </p>
        ) : (
          <div className="space-y-4">
            <p className="text-xs text-muted-foreground">
              Rows where score and global rank are unchanged since the previous sync are hidden.
            </p>
            <div className="md:overflow-x-auto">
              <LeaderboardHistorySection items={historyPageItems} />
            </div>
            <LeaderboardHistoryPagination
              total={enrichedHistory.length}
              offset={historyOffset}
              limit={LEADERBOARD_HISTORY_PAGE_SIZE}
              historyPage={historyPage}
              historyPageHref={historyPageHref}
            />
          </div>
        )}
      </section>
    </div>
  );
}
