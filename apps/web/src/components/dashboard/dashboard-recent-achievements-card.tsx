import Link from "next/link";
import { AchievementPointsLegend } from "@/components/achievements/achievement-points-legend";
import { AchievementsActivityFeed } from "@/components/achievements/achievements-activity-feed";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { DashboardAchievementActivityItemDto } from "@/generated/api-client";

type Props = {
  items: DashboardAchievementActivityItemDto[];
  achievementsSyncedAt?: string | null;
};

export function DashboardRecentAchievementsCard({ items, achievementsSyncedAt }: Props) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle>Recent unlocks</CardTitle>
        <Link
          href="/achievements"
          className="text-xs font-medium text-muted-foreground hover:text-[var(--accent-retro)]"
        >
          View all →
        </Link>
      </CardHeader>
      <CardContent className="space-y-3">
        <AchievementsActivityFeed items={items} variant="card" />
        <AchievementPointsLegend />
        <LastSyncedLabel at={achievementsSyncedAt} />
      </CardContent>
    </Card>
  );
}
