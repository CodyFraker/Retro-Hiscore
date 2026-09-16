import type { Metadata } from "next";
import { Suspense } from "react";
import { ArrowLeft, Clock, ImageOff } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ConsoleName } from "@/components/console-name";
import { GameAchievementsSection } from "@/components/game/game-achievements-section";
import { BoardWinSummary } from "@/components/game/board-win-summary";
import { BoardWinRivalryLinks } from "@/components/game/board-win-rivalry-links";
import { GameDeltaCallout } from "@/components/game/game-delta-callout";
import { GameDetailTabs } from "@/components/game/game-detail-tabs";
import { GameMetadata } from "@/components/game/game-metadata";
import { GamePopulationTrendCharts } from "@/components/game/game-population-trend-charts";
import { GameRefreshButton } from "@/components/game/game-refresh-button";
import { LeaderboardSyncTierBadge } from "@/components/sync/leaderboard-sync-tier-badge";
import { GameSourcesSection } from "@/components/game/game-sources-section";
import { GameStandingsWithFilters } from "@/components/game/game-standings-with-filters";
import { GameStatsStrip } from "@/components/game/game-stats-strip";
import { GameTrendCharts } from "@/components/game/game-trend-charts";
import { RetroachievementsLink } from "@/components/game/retroachievements-link";
import { getServerApiClient } from "@/lib/api";
import { recentlyUpdatedBoards, summarizeBoardWins } from "@/lib/board-wins";
import { formatSyncTime } from "@/lib/format";
import { toGameDeltas } from "@/lib/game-history-series";
import { summarizeGameStats } from "@/lib/game-stats";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raGameId: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { raGameId: raw } = await params;
  const raGameId = Number(raw);
  if (!Number.isFinite(raGameId)) {
    return { title: "Retro Hiscore" };
  }

  try {
    const api = await getServerApiClient();
    const data = await api.getGameLeaderboards(raGameId);
    const title = `${data.title} · Retro Hiscore`;
    const consolePart = data.consoleName ? ` on ${data.consoleName}` : "";
    const boardCount = data.leaderboards.length;
    const description =
      boardCount > 0
        ? `Friend leaderboard standings for ${data.title}${consolePart} — ${boardCount} tracked board${boardCount === 1 ? "" : "s"}.`
        : `Tracked game ${data.title}${consolePart} on Retro Hiscore.`;
    return {
      title,
      description,
      openGraph: { title, description },
      twitter: { title, description },
    };
  } catch {
    return { title: "Retro Hiscore" };
  }
}

export default async function GamePage({ params }: Props) {
  const { raGameId: raw } = await params;
  const raGameId = Number(raw);
  if (!Number.isFinite(raGameId)) {
    notFound();
  }

  const api = await getServerApiClient();
  const member = await api.getCurrentMember();
  let data: Awaited<ReturnType<typeof api.getGameLeaderboards>>;
  let history: Awaited<ReturnType<typeof api.getGameHistory>>;
  let populationHistory: Awaited<ReturnType<typeof api.getGameLeaderboardPopulationHistory>>;
  let sources: Awaited<ReturnType<typeof api.getGameSources>>;
  let achievements: Awaited<ReturnType<typeof api.getGameAchievements>>;
  let achievementDistribution: Awaited<ReturnType<typeof api.getGameAchievementDistribution>>;
  let recentUnlockActivity: Awaited<ReturnType<typeof api.getDashboardAchievementActivity>>;
  let memberCount = 0;
  try {
    [data, history, populationHistory, sources, achievements, achievementDistribution, recentUnlockActivity, memberCount] =
      await Promise.all([
        api.getGameLeaderboards(raGameId),
        api.getGameHistory(raGameId, 200, 0),
        api.getGameLeaderboardPopulationHistory(raGameId, 200),
        api.getGameSources(raGameId),
        api.getGameAchievements(raGameId),
        api.getGameAchievementDistribution(raGameId),
        api.getDashboardAchievementActivity(10, 0, raGameId),
        api.getMembers().then((members) => members.length),
      ]);
  } catch {
    notFound();
  }

  const artUrl = data.imageBoxArtUrl ?? data.imageIconUrl;
  const winRows = summarizeBoardWins(data.leaderboards, data.members);
  const recentBoards = recentlyUpdatedBoards(data.leaderboards, 3);
  const stats = summarizeGameStats(data);
  const deltas = toGameDeltas(history.items);

  return (
    <div className="space-y-8">
      <div className="space-y-4">
        <Link
          href="/games"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="size-4 shrink-0" />
          All games
        </Link>

        {data.imageTitleUrl && (
          <div className="relative hidden aspect-[21/9] max-h-32 w-full overflow-hidden rounded border border-border bg-secondary/30 sm:block sm:max-h-36">
            <Image
              src={data.imageTitleUrl}
              alt=""
              fill
              className="object-contain object-center"
              sizes="(max-width: 768px) 100vw, 1152px"
              priority
            />
          </div>
        )}

        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
            <div className="relative h-36 w-36 shrink-0 overflow-hidden rounded border border-border bg-secondary/40">
              {artUrl ? (
                <Image src={artUrl} alt="" fill className="object-cover" sizes="144px" priority />
              ) : (
                <div className="flex h-full items-center justify-center text-muted-foreground">
                  <ImageOff className="size-8" />
                </div>
              )}
            </div>
            <div className="space-y-2">
              <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)] sm:text-3xl">
                {data.title}
              </h1>
              {data.consoleName && (
                <p>
                  <ConsoleName name={data.consoleName} iconUrl={data.consoleIconUrl} />
                </p>
              )}
              <GameMetadata data={data} />
              <p className="text-sm text-muted-foreground">
                Friend standings across every tracked leaderboard for this game.
              </p>
            </div>
          </div>
          <div className="flex w-full flex-col items-stretch gap-2 sm:w-auto sm:items-end">
            <LeaderboardSyncTierBadge status={data.leaderboardSyncStatus} />
            <GameRefreshButton
              raGameId={data.raGameId}
              hasApiKey={member.hasApiKey}
              leaderboardScoresSyncedAt={data.leaderboardScoresSyncedAt}
            />
            <RetroachievementsLink raGameId={data.raGameId} />
          </div>
        </div>
      </div>

      <GameSourcesSection sources={sources} />

      {data.leaderboards.length === 0 ? (
        <p className="flex items-center gap-2 text-muted-foreground">
          <Clock className="size-4 shrink-0" />
          Waiting for first sync.
        </p>
      ) : (
        <Suspense fallback={null}>
          <GameDetailTabs
            standings={
              <>
                <GameStatsStrip stats={stats} />
                <div className="md:overflow-x-auto">
                  <GameStandingsWithFilters leaderboards={data.leaderboards} members={data.members} />
                </div>
                <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
                  <div className="space-y-3">
                    <BoardWinSummary rows={winRows} />
                    <BoardWinRivalryLinks
                      rows={winRows}
                      currentRaUsername={member.raUsername}
                      memberCount={memberCount}
                    />
                  </div>
                  {recentBoards.length > 0 && (
                    <section className="space-y-3">
                      <h2 className="steam-section-heading">Recently updated</h2>
                      <ul className="divide-y divide-border border-y border-border">
                        {recentBoards.map((board) => (
                          <li
                            key={board.raLeaderboardId}
                            className="flex items-center justify-between gap-4 py-3 text-sm"
                          >
                            <Link
                              href={`/leaderboards/${board.raLeaderboardId}`}
                              className="font-medium hover:text-[var(--accent-retro)]"
                            >
                              {board.title}
                            </Link>
                            <span className="text-xs text-muted-foreground">
                              {formatSyncTime(board.latestScoreUpdatedAt)}
                              {board.globalEntryCount != null && (
                                <> · {board.globalEntryCount.toLocaleString()} Total Entries</>
                              )}
                            </span>
                          </li>
                        ))}
                      </ul>
                    </section>
                  )}
                </div>
              </>
            }
            trends={
              <>
                <GameDeltaCallout deltas={deltas} />
                <GameTrendCharts
                  items={history.items}
                  leaderboards={data.leaderboards}
                  members={data.members}
                  deltas={deltas}
                  defaultMemberId={member.id}
                />
                <GamePopulationTrendCharts data={populationHistory} />
              </>
            }
            achievements={
              <GameAchievementsSection
                achievements={achievements}
                distribution={achievementDistribution}
                recentUnlockActivity={recentUnlockActivity.items}
                raGameId={raGameId}
              />
            }
          />
        </Suspense>
      )}
    </div>
  );
}
