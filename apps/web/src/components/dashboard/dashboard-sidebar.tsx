import { ChampionshipStandings } from "@/components/dashboard/championship-standings";
import { GroupRecentGamesCard } from "@/components/dashboard/group-recent-games-card";
import type { ChampionshipRowDto, RecentGroupGameDto } from "@/generated/api-client";

type Props = {
  championship: ChampionshipRowDto[];
  recentGroupGames: RecentGroupGameDto[];
};

export function DashboardSidebar({ championship, recentGroupGames }: Props) {
  return (
    <>
      <ChampionshipStandings rows={championship} />
      <GroupRecentGamesCard games={recentGroupGames} />
    </>
  );
}
