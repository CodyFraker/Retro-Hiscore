import { ChampionshipStandings } from "@/components/dashboard/championship-standings";
import { DashboardAchievementTrends } from "@/components/dashboard/dashboard-achievement-trends";
import { DashboardRecentAchievementsCard } from "@/components/dashboard/dashboard-recent-achievements-card";
import { GroupRecentGamesCard } from "@/components/dashboard/group-recent-games-card";
import type {
  ChampionshipRowDto,
  DashboardAchievementActivityItemDto,
  DashboardAchievementHistoryItemDto,
  RecentGroupGameDto,
} from "@/generated/api-client";

type Props = {
  championship: ChampionshipRowDto[];
  recentGroupGames: RecentGroupGameDto[];
  achievementActivity: DashboardAchievementActivityItemDto[];
  achievementHistory: DashboardAchievementHistoryItemDto[];
  achievementsSyncedAt?: string | null;
};

export function DashboardSidebar({
  championship,
  recentGroupGames,
  achievementActivity,
  achievementHistory,
  achievementsSyncedAt,
}: Props) {
  return (
    <>
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
