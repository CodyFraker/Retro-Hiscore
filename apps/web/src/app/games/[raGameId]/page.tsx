import { ArrowLeft, Clock, ImageOff } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ConsoleName } from "@/components/console-name";
import { GameDeltaCallout } from "@/components/game/game-delta-callout";
import { GameStandingsSection } from "@/components/game/game-standings-section";
import { GameMetadata } from "@/components/game/game-metadata";
import { GameStatsStrip } from "@/components/game/game-stats-strip";
import { GamePopulationTrendCharts } from "@/components/game/game-population-trend-charts";
import { GameTrendCharts } from "@/components/game/game-trend-charts";
import { BoardWinSummary } from "@/components/game/board-win-summary";
import { RetroachievementsLink } from "@/components/game/retroachievements-link";
import { GameSourcesSection } from "@/components/game/game-sources-section";
import { getServerApiClient } from "@/lib/api";
import { recentlyUpdatedBoards, summarizeBoardWins } from "@/lib/board-wins";
import { toGameDeltas } from "@/lib/game-history-series";
import { summarizeGameStats } from "@/lib/game-stats";
import { formatSyncTime } from "@/lib/format";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raGameId: string }>;
};

export default async function GamePage({ params }: Props) {
  const { raGameId: raw } = await params;
  const raGameId = Number(raw);
  if (!Number.isFinite(raGameId)) {
    notFound();
  }

  const api = await getServerApiClient();
  let data: Awaited<ReturnType<typeof api.getGameLeaderboards>>;
  let history: Awaited<ReturnType<typeof api.getGameHistory>>;
  let populationHistory: Awaited<ReturnType<typeof api.getGameLeaderboardPopulationHistory>>;
  let sources: Awaited<ReturnType<typeof api.getGameSources>>;
  try {
    [data, history, populationHistory, sources] = await Promise.all([
      api.getGameLeaderboards(raGameId),
      api.getGameHistory(raGameId, 200, 0),
      api.getGameLeaderboardPopulationHistory(raGameId, 200),
      api.getGameSources(raGameId),
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
          href="/"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="size-4 shrink-0" />
          All games
        </Link>

        {data.imageTitleUrl && (
          <div className="relative hidden h-28 w-full overflow-hidden rounded border border-border sm:block">
            <Image
              src={data.imageTitleUrl}
              alt=""
              fill
              className="object-cover object-center"
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
          <RetroachievementsLink raGameId={data.raGameId} />
        </div>
      </div>

      <GameSourcesSection sources={sources} />

      {data.leaderboards.length === 0 ? (
        <p className="flex items-center gap-2 text-muted-foreground">
          <Clock className="size-4 shrink-0" />
          Waiting for first sync.
        </p>
      ) : (
        <>
          <GameStatsStrip stats={stats} />

          <div className="md:overflow-x-auto">
            <GameStandingsSection leaderboards={data.leaderboards} members={data.members} />
          </div>

          <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
            <BoardWinSummary rows={winRows} />

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
                          <> · {board.globalEntryCount.toLocaleString()} on RA</>
                        )}
                      </span>
                    </li>
                  ))}
                </ul>
              </section>
            )}
          </div>

          <GameTrendCharts items={history.items} />
          <GamePopulationTrendCharts data={populationHistory} />
          <GameDeltaCallout deltas={deltas} />
        </>
      )}
    </div>
  );
}
