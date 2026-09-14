import { MemberRaPresenceCard } from "@/components/members/member-ra-presence-card";
import { MemberRaRecentAchievements } from "@/components/members/member-ra-recent-achievements";
import { MemberRaRecentlyPlayed } from "@/components/members/member-ra-recently-played";
import { MemberRaSummaryTrends } from "@/components/members/member-ra-summary-trends";
import { MemberRaAchievementTrends } from "@/components/members/member-ra-achievement-trends";
import type {
  MemberRaAchievementHistoryItemDto,
  MemberRaRankHistoryItemDto,
  MemberRaSummaryResponse,
} from "@/generated/api-client";

type Props = {
  data: MemberRaSummaryResponse;
  rankHistory: MemberRaRankHistoryItemDto[];
  achievementHistory: MemberRaAchievementHistoryItemDto[];
};

export function MemberRaSummarySection({ data, rankHistory, achievementHistory }: Props) {
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
          <MemberRaAchievementTrends items={achievementHistory} />
        </div>
      </section>
    );
  }

  const summary = data.summary;

  return (
    <section className="space-y-8">
      <h2 className="steam-section-heading">RetroAchievements</h2>

      <div className="min-w-0 space-y-6">
        {summary.presence ? <MemberRaPresenceCard presence={summary.presence} /> : null}
        <MemberRaRecentlyPlayed
          games={summary.recentlyPlayed}
          excludeGameId={summary.presence?.raGameId}
        />
        <MemberRaRecentAchievements achievements={summary.recentAchievements} />
      </div>

      <div className="space-y-3">
        <h3 className="text-sm font-medium uppercase tracking-wide text-muted-foreground">
          Progress over time
        </h3>
        <MemberRaSummaryTrends items={rankHistory} showSoftcorePoints={showSoftcorePoints} />
        <MemberRaAchievementTrends items={achievementHistory} />
      </div>
    </section>
  );
}
