import { MemberRaPresenceCard } from "@/components/members/member-ra-presence-card";
import Link from "next/link";
import { AchievementsActivityFeed } from "@/components/achievements/achievements-activity-feed";
import { buildAchievementsQueryString } from "@/lib/achievements-params";
import { MemberRaRecentlyPlayed } from "@/components/members/member-ra-recently-played";
import { MemberRaSummaryTrends } from "@/components/members/member-ra-summary-trends";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import { MemberGroupComparisonCard } from "@/components/members/member-group-comparison-card";
import type {
  ChampionshipRowDto,
  DashboardAchievementActivityItemDto,
  MemberRaRankHistoryItemDto,
  MemberRaSummaryResponse,
} from "@/generated/api-client";

type Props = {
  data: MemberRaSummaryResponse;
  rankHistory: MemberRaRankHistoryItemDto[];
  championship?: ChampionshipRowDto[];
  memberCount?: number;
  memberId?: string;
  friendRankOnes?: number;
  profileRaUsername?: string;
  recentUnlockActivity?: DashboardAchievementActivityItemDto[];
  pendingRaGameIds?: ReadonlySet<number>;
};

export function MemberRaSummarySection({
  data,
  rankHistory,
  championship,
  memberCount = 0,
  memberId,
  friendRankOnes,
  profileRaUsername,
  recentUnlockActivity = [],
  pendingRaGameIds,
}: Props) {
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
      {memberCount >= 2 && championship && memberId != null && friendRankOnes != null ? (
        <MemberGroupComparisonCard
          memberId={memberId}
          friendRankOnes={friendRankOnes}
          championship={championship}
        />
      ) : null}
      <div className="flex flex-col gap-1 sm:flex-row sm:items-baseline sm:justify-between">
        <h2 className="steam-section-heading">RetroAchievements</h2>
        <LastSyncedLabel at={profileSyncedAt} />
      </div>

      <div className="min-w-0 space-y-6">
        {summary.presence ? <MemberRaPresenceCard presence={summary.presence} /> : null}
        <MemberRaRecentlyPlayed
          games={summary.recentlyPlayed}
          excludeGameId={summary.presence?.raGameId}
          pendingRaGameIds={pendingRaGameIds}
        />
        <div className="space-y-3">
          <div className="flex flex-wrap items-baseline justify-between gap-2">
            <h3 className="text-sm font-medium uppercase tracking-wide text-muted-foreground">
              Recent unlocks
            </h3>
            {profileRaUsername && recentUnlockActivity.length > 0 ? (
              <Link
                href={`/achievements${buildAchievementsQueryString({ member: profileRaUsername })}`}
                className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]"
              >
                View all →
              </Link>
            ) : null}
          </div>
          <AchievementsActivityFeed
            items={recentUnlockActivity}
            variant="embedded"
            showMember={false}
          />
        </div>
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
