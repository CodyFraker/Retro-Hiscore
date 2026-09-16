import { Suspense } from "react";
import { redirect } from "next/navigation";
import { AlertCircle } from "lucide-react";
import { DashboardActivityPrimary } from "@/components/dashboard/dashboard-activity-primary";
import { DashboardHero } from "@/components/dashboard/dashboard-hero";
import { DashboardLayout } from "@/components/dashboard/dashboard-layout";
import { SetupReminderBanner } from "@/components/dashboard/setup-reminder-banner";
import { DashboardPageSkeleton } from "@/components/layout/dashboard-page-skeleton";
import type {
  DashboardAchievementActivityResponse,
  DashboardAchievementSummaryResponse,
  MembersSummaryDto,
  DashboardGroupActivityResponse,
  DashboardResponse,
  GameOfTheWeekCurrentPollDto,
  GameOfTheWeekHistoryItemDto,
} from "@/generated/api-client";
import { getServerSession } from "next-auth";
import { getServerApiClient } from "@/lib/api";
import { authOptions } from "@/lib/auth-options";
import {
  buildTrackedGamesQueryString,
  parseTrackedGamesPage,
  parseTrackedGamesSort,
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

function memberNeedsSyncReminder(
  hasApiKey: boolean,
  syncStatus: {
    leaderboards: { lastSyncedAt?: string | null };
    profile: { lastSyncedAt?: string | null };
    achievements: { lastSyncedAt?: string | null };
  } | null,
): boolean {
  if (!hasApiKey || !syncStatus) {
    return false;
  }
  return (
    !syncStatus.leaderboards.lastSyncedAt
    && !syncStatus.profile.lastSyncedAt
    && !syncStatus.achievements.lastSyncedAt
  );
}

async function HomePageContent({ searchParams }: Props) {
  const params = await searchParams;
  if (params.page || params.q || params.sort) {
    const qs = buildTrackedGamesQueryString({
      page: parseTrackedGamesPage(params.page),
      q: params.q?.trim() || undefined,
      sort: parseTrackedGamesSort(params.sort),
    });
    redirect(qs ? `/games?${qs}` : "/games");
  }

  let dashboard: DashboardResponse | null = null;
  let achievementActivity: DashboardAchievementActivityResponse | null = null;
  let achievementSummary: DashboardAchievementSummaryResponse | null = null;
  let membersSummary: MembersSummaryDto | null = null;
  let isAdmin = false;
  let gameOfTheWeekPoll: GameOfTheWeekCurrentPollDto | null = null;
  let gameOfTheWeekLastWinner: GameOfTheWeekHistoryItemDto | null = null;
  let groupActivity: DashboardGroupActivityResponse | null = null;
  let showSetupReminder = false;
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    const session = await getServerSession(authOptions);
    isAdmin = session?.isAdmin === true;
    [dashboard, achievementActivity, achievementSummary, membersSummary] = await Promise.all([
      api.getDashboard(),
      api.getDashboardAchievementActivity(5),
      api.getDashboardAchievementSummary(),
      api.getMembersSummary(),
    ]);
    try {
      gameOfTheWeekPoll = await api.getGameOfTheWeekCurrent();
    } catch {
      gameOfTheWeekPoll = null;
    }
    try {
      const history = await api.getGameOfTheWeekHistory(1, 0);
      gameOfTheWeekLastWinner = history.items[0] ?? null;
    } catch {
      gameOfTheWeekLastWinner = null;
    }
    try {
      groupActivity = await api.getDashboardGroupActivity(8);
    } catch {
      groupActivity = null;
    }
    try {
      const member = await api.getCurrentMember();
      if (!member.needsOnboarding && member.hasApiKey) {
        const syncStatus = await api.getMemberSelfSyncStatus();
        showSetupReminder = memberNeedsSyncReminder(true, syncStatus);
      }
    } catch {
      showSetupReminder = false;
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

      {!error && dashboard && (
        <>
          {showSetupReminder ? <SetupReminderBanner /> : null}
          <DashboardLayout
            header={<DashboardHero />}
            primary={
              <DashboardActivityPrimary
                championship={dashboard.championship}
                memberCount={membersSummary?.memberCount ?? 0}
                recentGroupGames={dashboard.recentGroupGames}
                achievementActivity={achievementActivity?.items ?? []}
                achievementsSyncedAt={achievementSummary?.achievementsSyncedAt}
                gameOfTheWeekPoll={gameOfTheWeekPoll}
                gameOfTheWeekLastWinner={gameOfTheWeekLastWinner}
                groupActivity={groupActivity}
                isAdmin={isAdmin}
              />
            }
          />
        </>
      )}
    </div>
  );
}
