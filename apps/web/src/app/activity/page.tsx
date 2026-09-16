import { PageHero } from "@/components/layout/page-hero";
import { GroupActivityFeed } from "@/components/activity/group-activity-feed";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function ActivityPage() {
  const api = await getServerApiClient();
  const activity = await api.getDashboardGroupActivity(50);

  return (
    <div className="mx-auto max-w-2xl space-y-8">
      <PageHero
        title="What's new"
        description="Group activity between the last two leaderboard snapshot syncs — score moves, achievements, and newly tracked games."
      />
      <section className="rounded border border-border p-5">
        <GroupActivityFeed items={activity.items} />
      </section>
    </div>
  );
}
