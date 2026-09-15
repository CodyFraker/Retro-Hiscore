import { MemberRaPresenceCard } from "@/components/members/member-ra-presence-card";
import { MemberRaRecentAchievements } from "@/components/members/member-ra-recent-achievements";
import { MemberRaRecentlyPlayed } from "@/components/members/member-ra-recently-played";
import { MemberRaSummaryTrends } from "@/components/members/member-ra-summary-trends";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import type { MemberRaRankHistoryItemDto, MemberRaSummaryResponse } from "@/generated/api-client";

type Props = {
  data: MemberRaSummaryResponse;
  rankHistory: MemberRaRankHistoryItemDto[];
};

export function MemberRaSummarySection({ data, rankHistory }: Props) {
  const showSoftcorePoints = (data.summary?.totalSoftcorePoints ?? 0) > 0;

  if (!data.available || !data.summary) {
    return (
      <section className="space-y-6">
        <h2 className="steam-section-heading">RetroAchievements</h2>
        <p className="text-sm text-muted-foreground">
          {data.unavailableReason ?? "RetroAchievements summary is unavailable."}
        </p>
        <div className="space-y-3">
          <h3 className="text-sm font-medium uppercase tracking-wide text-muted-foreground">
            Progress over time
          </h3>
          <MemberRaSummaryTrends items={rankHistory} showSoftcorePoints={showSoftcorePoints} />
        </div>
      </section>
    );
  }

  const summary = data.summary;
  const profileSyncedAt = rankHistory[0]?.syncedAt ?? null;

  return (
    <section className="space-y-8">
      <div className="flex flex-col gap-1 sm:flex-row sm:items-baseline sm:justify-between">
        <h2 className="steam-section-heading">RetroAchievements</h2>
        <LastSyncedLabel at={profileSyncedAt} />
      </div>

      <div className="min-w-0 space-y-6">
        {summary.presence ? <MemberRaPresenceCard presence={summary.presence} /> : null}
        <MemberRaRecentlyPlayed
          games={summary.recentlyPlayed}
          excludeGameId={summary.presence?.raGameId}
        />
        <MemberRaRecentAchievements achievements={summary.recentAchievements} />
        <MemberRaRecentAchievements
          achievements={summary.recentAchievements}
          trackedOnly
          heading="Recent unlocks (tracked games)"
        />
      </div>

      <div className="space-y-3">
        <h3 className="text-sm font-medium uppercase tracking-wide text-muted-foreground">
          Progress over time
        </h3>
        <MemberRaSummaryTrends items={rankHistory} showSoftcorePoints={showSoftcorePoints} />
      </div>
    </section>
  );
}
