import { ActivityFeed } from "@/components/dashboard/activity-feed";
import { ChampionshipStandings } from "@/components/dashboard/championship-standings";
import type { ActivityItemDto, ChampionshipRowDto } from "@/generated/api-client";

type Props = {
  championship: ChampionshipRowDto[];
  activity: ActivityItemDto[];
};

export function DashboardSidebar({ championship, activity }: Props) {
  return (
    <>
      <ChampionshipStandings rows={championship} />
      <ActivityFeed items={activity} />
    </>
  );
}
