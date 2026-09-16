import Link from "next/link";
import { ChampionshipStandings } from "@/components/dashboard/championship-standings";
import { DashboardRecentAchievementsCard } from "@/components/dashboard/dashboard-recent-achievements-card";
import { GroupRecentGamesCard } from "@/components/dashboard/group-recent-games-card";
import { DashboardGroupActivityCard } from "@/components/dashboard/dashboard-group-activity-card";
import { GameOfTheWeekDashboardCard } from "@/components/game-of-the-week/game-of-the-week-dashboard-card";
import type {
  ChampionshipRowDto,
  DashboardAchievementActivityItemDto,
  DashboardGroupActivityResponse,
  GameOfTheWeekCurrentPollDto,
  GameOfTheWeekHistoryItemDto,
  RecentGroupGameDto,
} from "@/generated/api-client";

type Props = {
  championship: ChampionshipRowDto[];
  memberCount: number;
  recentGroupGames: RecentGroupGameDto[];
  achievementActivity: DashboardAchievementActivityItemDto[];
  achievementsSyncedAt?: string | null;
  gameOfTheWeekPoll?: GameOfTheWeekCurrentPollDto | null;
  gameOfTheWeekLastWinner?: GameOfTheWeekHistoryItemDto | null;
  groupActivity?: DashboardGroupActivityResponse | null;
  isAdmin?: boolean;
};

export function DashboardActivityPrimary({
  championship,
  memberCount,
  recentGroupGames,
  achievementActivity,
  achievementsSyncedAt,
  gameOfTheWeekPoll,
  gameOfTheWeekLastWinner,
  groupActivity,
  isAdmin = false,
}: Props) {
  return (
    <div className="space-y-8">
      <GameOfTheWeekDashboardCard poll={gameOfTheWeekPoll} lastWinner={gameOfTheWeekLastWinner} />
      {groupActivity ? <DashboardGroupActivityCard activity={groupActivity} /> : null}
      <ChampionshipStandings rows={championship} memberCount={memberCount} limit={5} />
      <DashboardRecentAchievementsCard
        items={achievementActivity}
        achievementsSyncedAt={achievementsSyncedAt}
      />
      <GroupRecentGamesCard games={recentGroupGames} isAdmin={isAdmin} />
      <section className="rounded border border-dashed border-border p-6 text-center">
        <p className="text-sm text-muted-foreground">
          Search, sort, and compare every tracked game in the catalog.
        </p>
        <Link
          href="/games"
          className="mt-3 inline-block text-sm font-medium text-[var(--accent-retro)] hover:underline"
        >
          Browse all tracked games →
        </Link>
      </section>
    </div>
  );
}
