import { ArrowLeft, Clock } from "lucide-react";
import Link from "next/link";
import { notFound } from "next/navigation";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { DeltaCallout } from "@/components/charts/delta-callout";
import { GlobalRankTrendChart } from "@/components/charts/global-rank-trend-chart";
import { LeaderboardEntryCountTrendChart } from "@/components/charts/leaderboard-entry-count-trend-chart";
import { ScoreTrendChart } from "@/components/charts/score-trend-chart";
import { LeaderboardHistoryPagination } from "@/components/leaderboards/leaderboard-history-pagination";
import { LeaderboardHistorySection } from "@/components/leaderboards/leaderboard-history-section";
import { LeaderboardStandingsSection } from "@/components/leaderboards/leaderboard-standings-section";
import { getServerApiClient } from "@/lib/api";
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

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raLeaderboardId: string }>;
  searchParams: Promise<{ historyPage?: string }>;
};

export default async function LeaderboardPage({ params, searchParams }: Props) {
  const { raLeaderboardId: raw } = await params;
  const { historyPage: historyPageParam } = await searchParams;
  const raLeaderboardId = Number(raw);
  if (!Number.isFinite(raLeaderboardId)) {
    notFound();
  }

  const api = await getServerApiClient();
  let detail: Awaited<ReturnType<typeof api.getLeaderboard>>;
  let history: Awaited<ReturnType<typeof api.getLeaderboardHistory>>;

  try {
    [detail, history] = await Promise.all([
      api.getLeaderboard(raLeaderboardId),
      api.getLeaderboardHistory(raLeaderboardId, 200, 0),
    ]);
  } catch {
    notFound();
  }

  const series = toChartSeries(history.items);
  const populationSeries = toLeaderboardPopulationSeries(history.items);
  const deltas = computeMemberDeltas(series);
  const populationDelta = computeLeaderboardPopulationDelta(history.items);
  const dedupedHistory = dedupeLeaderboardHistoryItems(history.items);
  const dedupedHistoryIds = new Set(dedupedHistory.map((row) => row.id));
  const enrichedHistory = enrichLeaderboardHistoryRows(history.items).filter((row) =>
    dedupedHistoryIds.has(row.id),
  );
  const historyPageCount = Math.max(
    1,
    Math.ceil(enrichedHistory.length / LEADERBOARD_HISTORY_PAGE_SIZE),
  );
  const historyPage = parseLeaderboardHistoryPage(historyPageParam, historyPageCount);
  const historyOffset = (historyPage - 1) * LEADERBOARD_HISTORY_PAGE_SIZE;
  const historyPageItems = enrichedHistory.slice(
    historyOffset,
    historyOffset + LEADERBOARD_HISTORY_PAGE_SIZE,
  );

  return (
    <div className="space-y-10">
      <div className="space-y-2">
        <Link
          href={`/games/${detail.raGameId}`}
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="size-4 shrink-0" />
          {detail.gameTitle}
        </Link>
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)] sm:text-3xl">
          {detail.title}
        </h1>
        {detail.description && <p className="text-muted-foreground">{detail.description}</p>}
        <p className="text-xs text-muted-foreground">
          {detail.rankAsc ? "Lower score ranks higher" : "Higher score ranks higher"}
        </p>
        {detail.globalEntryCount != null && (
          <p className="text-sm text-muted-foreground">
            <span className="font-mono text-foreground">
              {detail.globalEntryCount.toLocaleString()}
            </span>{" "}
            ranked players on RetroAchievements
            {detail.globalEntryCountSyncedAt && (
              <>
                {" "}
                · as of{" "}
                <FormattedSyncTime value={detail.globalEntryCountSyncedAt} />
              </>
            )}
          </p>
        )}
      </div>

      <section className="space-y-3">
        <h2 className="steam-section-heading">Friend standings</h2>
        <div className="md:overflow-x-auto">
          <LeaderboardStandingsSection
            standings={detail.standings}
            globalEntryCount={detail.globalEntryCount}
          />
        </div>
      </section>

      <section className="space-y-3">
        <h2 className="steam-section-heading">Score trend</h2>
        <ScoreTrendChart series={series} scoreFormat={detail.format} />
      </section>

      <section className="space-y-3">
        <h2 className="steam-section-heading">Global rank over time</h2>
        <GlobalRankTrendChart series={series} historyItems={history.items} />
      </section>

      <section className="space-y-3">
        <h2 className="steam-section-heading">Total entries over time</h2>
        <p className="text-xs text-muted-foreground">
          Ranked players on RetroAchievements at each sync—growth shows new submissions on this
          board.
        </p>
        <LeaderboardEntryCountTrendChart points={populationSeries} />
      </section>

      <DeltaCallout
        deltas={deltas}
        scoreFormat={detail.format}
        globalEntryCountDelta={populationDelta?.delta}
      />

      <section className="space-y-3">
        <h2 className="steam-section-heading">Recent history</h2>
        {history.items.length === 0 ? (
          <p className="flex items-center gap-2 text-muted-foreground">
            <Clock className="size-4 shrink-0" />
            No snapshots yet.
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
              raLeaderboardId={raLeaderboardId}
              total={enrichedHistory.length}
              offset={historyOffset}
              limit={LEADERBOARD_HISTORY_PAGE_SIZE}
              historyPage={historyPage}
            />
          </div>
        )}
      </section>
    </div>
  );
}
