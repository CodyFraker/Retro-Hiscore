import { Suspense } from "react";
import { AlertCircle } from "lucide-react";
import { DashboardHero } from "@/components/dashboard/dashboard-hero";
import { DashboardLayout } from "@/components/dashboard/dashboard-layout";
import { DashboardSidebar } from "@/components/dashboard/dashboard-sidebar";
import { TrackedGamesSection } from "@/components/dashboard/tracked-games-section";
import { DashboardPageSkeleton } from "@/components/layout/dashboard-page-skeleton";
import type {
  DashboardAchievementActivityResponse,
  DashboardAchievementHistoryResponse,
  DashboardAchievementSummaryResponse,
  DashboardGamesResponse,
  DashboardResponse,
  GameOfTheWeekCurrentPollDto,
} from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";
import {
  parseTrackedGamesPage,
  parseTrackedGamesSort,
  trackedGamesOffset,
  TRACKED_GAMES_PAGE_SIZE,
  type TrackedGamesSearchParams,
} from "@/lib/tracked-games-params";

export const dynamic = "force-dynamic";

type Props = {
  searchParams: Promise<TrackedGamesSearchParams>;
};

export default function HomePage({ searchParams }: Props) {
  return (
    <Suspense fallback={<DashboardPageSkeleton />}>
      <HomePageContent searchParams={searchParams} />
    </Suspense>
  );
}

async function HomePageContent({ searchParams }: Props) {
  const params = await searchParams;
  const pageNum = parseTrackedGamesPage(params.page);
  const sort = parseTrackedGamesSort(params.sort);
  const query = params.q?.trim() ?? "";

  let dashboard: DashboardResponse | null = null;
  let gamesPage: DashboardGamesResponse | null = null;
  let achievementActivity: DashboardAchievementActivityResponse | null = null;
  let achievementHistory: DashboardAchievementHistoryResponse | null = null;
  let achievementSummary: DashboardAchievementSummaryResponse | null = null;
  let gameOfTheWeekPoll: GameOfTheWeekCurrentPollDto | null = null;
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    [dashboard, gamesPage, achievementActivity, achievementHistory, achievementSummary] =
      await Promise.all([
      api.getDashboard(),
      api.getDashboardGames(
        TRACKED_GAMES_PAGE_SIZE,
        trackedGamesOffset(pageNum),
        sort,
        query || undefined,
      ),
      api.getDashboardAchievementActivity(5),
      api.getDashboardAchievementHistory(500),
      api.getDashboardAchievementSummary(),
    ]);
    try {
      gameOfTheWeekPoll = await api.getGameOfTheWeekCurrent();
    } catch {
      gameOfTheWeekPoll = null;
    }
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load dashboard";
  }

  return (
    <div className="space-y-8">
      {error && (
        <>
          <DashboardHero />
          <p className="flex items-center gap-2 rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">
            <AlertCircle className="size-4 shrink-0" />
            {error}
          </p>
        </>
      )}

      {!error && dashboard && gamesPage && (
        <DashboardLayout
          header={<DashboardHero />}
          primary={
            <TrackedGamesSection
              basePath="/"
              page={gamesPage}
              query={query}
              sort={sort}
            />
          }
          sidebar={
            <DashboardSidebar
              championship={dashboard.championship}
              recentGroupGames={dashboard.recentGroupGames}
              achievementActivity={achievementActivity?.items ?? []}
              achievementHistory={achievementHistory?.items ?? []}
              achievementsSyncedAt={achievementSummary?.achievementsSyncedAt}
              gameOfTheWeekPoll={gameOfTheWeekPoll}
            />
          }
        />
      )}
    </div>
  );
}
