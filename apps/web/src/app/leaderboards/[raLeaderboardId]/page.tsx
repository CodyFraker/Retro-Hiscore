import { ArrowLeft, Clock } from "lucide-react";
import Link from "next/link";
import { notFound } from "next/navigation";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { LeaderboardTrendPanel } from "@/components/leaderboards/leaderboard-trend-panel";
import { LeaderboardStandingsSection } from "@/components/leaderboards/leaderboard-standings-section";
import { getServerApiClient } from "@/lib/api";
import { parseLeaderboardHistoryPage } from "@/lib/history-series";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raLeaderboardId: string }>;
  searchParams: Promise<{ historyPage?: string }>;
};

function boardHistoryPageHref(raLeaderboardId: number, page: number) {
  if (page <= 1) {
    return `/leaderboards/${raLeaderboardId}`;
  }
  return `/leaderboards/${raLeaderboardId}?historyPage=${page}`;
}

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

  const historyPage = parseLeaderboardHistoryPage(historyPageParam, Number.MAX_SAFE_INTEGER);

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

      <LeaderboardTrendPanel
        format={detail.format}
        historyItems={history.items}
        historyPage={historyPage}
        historyPageHref={(page) => boardHistoryPageHref(raLeaderboardId, page)}
        showDeltaCallout
      />
    </div>
  );
}
