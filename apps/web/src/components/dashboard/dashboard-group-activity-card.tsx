import Link from "next/link";
import { GroupActivityFeed } from "@/components/activity/group-activity-feed";
import type { DashboardGroupActivityResponse } from "@/generated/api-client";

type Props = {
  activity: DashboardGroupActivityResponse;
};

export function DashboardGroupActivityCard({ activity }: Props) {
  return (
    <section className="rounded-md border border-border bg-card p-5">
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <h2 className="text-lg font-semibold">What&apos;s new</h2>
        <Link href="/activity" className="text-sm text-[var(--accent-retro)] hover:underline">
          See all →
        </Link>
      </div>
      <p className="mt-1 text-xs text-muted-foreground">
        {activity.windowUnavailable
          ? "Needs at least two leaderboard syncs before changes can be compared."
          : activity.windowStart && activity.windowEnd
            ? `Since the previous group leaderboard sync`
            : null}
      </p>
      <div className="mt-4">
        <GroupActivityFeed
          items={activity.items.slice(0, 5)}
          emptyMessage="No score moves, unlocks, or new games in the latest sync window."
        />
      </div>
    </section>
  );
}
