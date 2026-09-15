import { AlertCircle } from "lucide-react";
import Link from "next/link";
import { AchievementPointsLegend } from "@/components/achievements/achievement-points-legend";
import { AchievementsActivityFeed } from "@/components/achievements/achievements-activity-feed";
import { AchievementsHubFilters } from "@/components/achievements/achievements-hub-filters";
import { AchievementsHubPagination } from "@/components/achievements/achievements-hub-pagination";
import { AchievementsHubSummary } from "@/components/achievements/achievements-hub-summary";
import { DashboardAchievementTrends } from "@/components/dashboard/dashboard-achievement-trends";
import { PageHero } from "@/components/layout/page-hero";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type {
  DashboardAchievementActivityResponse,
  DashboardAchievementHistoryResponse,
  DashboardAchievementSummaryResponse,
  DashboardGamesResponse,
  MemberDto,
} from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";
import {
  ACHIEVEMENTS_PAGE_SIZE,
  achievementsOffset,
  parseAchievementsGameId,
  parseAchievementsPage,
  type AchievementsSearchParams,
} from "@/lib/achievements-params";

export const dynamic = "force-dynamic";

type Props = {
  searchParams: Promise<AchievementsSearchParams>;
};

export default async function AchievementsPage({ searchParams }: Props) {
  const params = await searchParams;
  const pageNum = parseAchievementsPage(params.page);
  const gameId = parseAchievementsGameId(params.game);
  const member = params.member?.trim() ?? "";

  let summary: DashboardAchievementSummaryResponse | null = null;
  let activity: DashboardAchievementActivityResponse | null = null;
  let history: DashboardAchievementHistoryResponse | null = null;
  let gamesPage: DashboardGamesResponse | null = null;
  let members: MemberDto[] = [];
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    [summary, activity, history, gamesPage, members] = await Promise.all([
      api.getDashboardAchievementSummary(),
      api.getDashboardAchievementActivity(
        ACHIEVEMENTS_PAGE_SIZE,
        achievementsOffset(pageNum),
        gameId,
        member || undefined,
      ),
      api.getDashboardAchievementHistory(2000),
      api.getDashboardGames(100, 0),
      api.getMembers(),
    ]);
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load achievements";
  }

  const hasFilters = gameId != null || member.length > 0;
  const gameTitle =
    gameId != null ? gamesPage?.items.find((g) => g.raGameId === gameId)?.title : undefined;
  const memberLabel =
    member.length > 0
      ? members.find((m) => m.raUsername === member)?.displayName ?? member
      : undefined;

  const filterGames =
    gamesPage?.items.map((g) => ({ raGameId: g.raGameId, title: g.title })) ?? [];
  const filterMembers =
    members
      .filter((m) => m.raUsername)
      .map((m) => ({
        raUsername: m.raUsername!,
        displayName: m.displayName,
      })) ?? [];

  return (
    <div className="space-y-8">
      <PageHero
        title="Achievements"
        titleClassName="text-2xl sm:text-3xl md:text-4xl"
        description="Recent unlocks and trends from tracked games. Data updates when achievement sync runs."
      />

      {!error && summary?.achievementsSyncedAt ? (
        <LastSyncedLabel at={summary.achievementsSyncedAt} />
      ) : null}

      {error ? (
        <p className="flex items-center gap-2 rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">
          <AlertCircle className="size-4 shrink-0" />
          {error}
        </p>
      ) : null}

      {!error && summary && activity && history ? (
        <>
          <AchievementsHubFilters
            games={filterGames}
            members={filterMembers}
            gameId={gameId}
            member={member || undefined}
          />

          <AchievementsHubSummary summary={summary} />

          {hasFilters ? (
            <p className="text-sm text-muted-foreground">
              Filtered feed
              {gameTitle ? ` · ${gameTitle}` : gameId != null ? ` · game ${gameId}` : ""}
              {memberLabel ? ` · ${memberLabel}` : ""}
              {" · "}
              <Link
                href="/achievements"
                className="text-foreground underline-offset-4 hover:underline"
              >
                Clear filters
              </Link>
            </p>
          ) : null}

          <div className="grid gap-8 lg:grid-cols-[1fr_minmax(280px,360px)]">
            <Card>
              <CardHeader className="space-y-2 pb-2">
                <CardTitle>Unlock feed</CardTitle>
                <AchievementPointsLegend />
              </CardHeader>
              <CardContent className="space-y-4">
                <AchievementsActivityFeed items={activity.items} variant="page" />
                <AchievementsHubPagination
                  total={activity.total}
                  offset={activity.offset}
                  limit={activity.limit}
                  gameId={gameId}
                  member={member || undefined}
                />
              </CardContent>
            </Card>

            <div className="space-y-4">
              <DashboardAchievementTrends items={history.items} />
            </div>
          </div>
        </>
      ) : null}
    </div>
  );
}
