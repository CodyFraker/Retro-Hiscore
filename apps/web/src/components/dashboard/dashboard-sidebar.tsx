import { ChampionshipStandings } from "@/components/dashboard/championship-standings";
import { DashboardAchievementTrends } from "@/components/dashboard/dashboard-achievement-trends";
import { DashboardRecentAchievementsCard } from "@/components/dashboard/dashboard-recent-achievements-card";
import { GroupRecentGamesCard } from "@/components/dashboard/group-recent-games-card";
import { GameOfTheWeekDashboardCard } from "@/components/game-of-the-week/game-of-the-week-dashboard-card";
import type {
  ChampionshipRowDto,
  DashboardAchievementActivityItemDto,
  DashboardAchievementHistoryItemDto,
  GameOfTheWeekCurrentPollDto,
  RecentGroupGameDto,
} from "@/generated/api-client";

type Props = {
  championship: ChampionshipRowDto[];
  recentGroupGames: RecentGroupGameDto[];
  achievementActivity: DashboardAchievementActivityItemDto[];
  achievementHistory: DashboardAchievementHistoryItemDto[];
  achievementsSyncedAt?: string | null;
  gameOfTheWeekPoll?: GameOfTheWeekCurrentPollDto | null;
};

export function DashboardSidebar({
  championship,
  recentGroupGames,
  achievementActivity,
  achievementHistory,
  achievementsSyncedAt,
  gameOfTheWeekPoll,
}: Props) {
  return (
    <>
      {gameOfTheWeekPoll ? <GameOfTheWeekDashboardCard poll={gameOfTheWeekPoll} /> : null}
      <ChampionshipStandings rows={championship} />
      <DashboardRecentAchievementsCard
        items={achievementActivity}
        achievementsSyncedAt={achievementsSyncedAt}
      />
      <DashboardAchievementTrends items={achievementHistory} />
      <GroupRecentGamesCard games={recentGroupGames} />
    </>
  );
}
