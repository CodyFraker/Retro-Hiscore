import { ArrowLeft, Clock } from "lucide-react";
import Link from "next/link";
import { notFound } from "next/navigation";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { DeltaCallout } from "@/components/charts/delta-callout";
import { ScoreTrendChart } from "@/components/charts/score-trend-chart";
import { LeaderboardHistorySection } from "@/components/leaderboards/leaderboard-history-section";
import { LeaderboardStandingsSection } from "@/components/leaderboards/leaderboard-standings-section";
import { getServerApiClient } from "@/lib/api";
import { computeMemberDeltas, toChartSeries } from "@/lib/history-series";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raLeaderboardId: string }>;
};

export default async function LeaderboardPage({ params }: Props) {
  const { raLeaderboardId: raw } = await params;
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
      api.getLeaderboardHistory(raLeaderboardId, 100, 0),
    ]);
  } catch {
    notFound();
  }

  const series = toChartSeries(history.items);
  const deltas = computeMemberDeltas(series);

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
        <h1 className="font-[family-name:var(--font-display)] text-3xl text-[var(--accent-retro)]">
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
        <div className="rounded border border-border p-3 md:p-0 md:overflow-x-auto">
          <LeaderboardStandingsSection
            standings={detail.standings}
            globalEntryCount={detail.globalEntryCount}
          />
        </div>
      </section>

      <section className="space-y-3">
        <h2 className="steam-section-heading">Score trend</h2>
        <ScoreTrendChart series={series} />
      </section>

      <DeltaCallout deltas={deltas} />

      <section className="space-y-3">
        <h2 className="steam-section-heading">Recent history</h2>
        {history.items.length === 0 ? (
          <p className="flex items-center gap-2 text-muted-foreground">
            <Clock className="size-4 shrink-0" />
            No snapshots yet.
          </p>
        ) : (
          <div className="rounded border border-border p-3 md:p-0 md:overflow-x-auto">
            <LeaderboardHistorySection items={history.items} />
          </div>
        )}
      </section>
    </div>
  );
}
