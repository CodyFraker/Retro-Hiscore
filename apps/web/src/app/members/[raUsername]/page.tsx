import { Suspense } from "react";
import { notFound } from "next/navigation";
import { MemberProfileHero } from "@/components/members/member-profile-hero";
import { MemberProfileTabs } from "@/components/members/member-profile-tabs";
import { MemberRaAchievementTrends } from "@/components/members/member-ra-achievement-trends";
import { MemberRaSummarySection } from "@/components/members/member-ra-summary-section";
import { MemberStandingsSection } from "@/components/members/member-standings-section";
import { MemberTrackedAchievementsTable } from "@/components/members/member-tracked-achievements-table";
import { MemberTrendCharts } from "@/components/members/member-trend-charts";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import type {
  MemberAchievementsListResponse,
  MemberRaAchievementHistoryResponse,
  MemberRaRankHistoryResponse,
  MemberRaSummaryResponse,
} from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raUsername: string }>;
};

const unavailableRaSummary: MemberRaSummaryResponse = {
  available: false,
  unavailableReason: "Could not load RetroAchievements summary.",
  summary: null,
};

export default async function MemberProfilePage({ params }: Props) {
  const { raUsername: raw } = await params;
  const raUsername = decodeURIComponent(raw);

  const api = await getServerApiClient();
  let detail: Awaited<ReturnType<typeof api.getMember>>;
  let history: Awaited<ReturnType<typeof api.getMemberHistory>>;
  let raSummary: MemberRaSummaryResponse = unavailableRaSummary;
  let raRankHistory: MemberRaRankHistoryResponse = { items: [] };
  let raTrackedAchievementHistory: MemberRaAchievementHistoryResponse = { items: [] };
  let trackedAchievements: MemberAchievementsListResponse = {
    total: 0,
    offset: 0,
    limit: 0,
    items: [],
  };

  try {
    [detail, history] = await Promise.all([
      api.getMember(raUsername),
      api.getMemberHistory(raUsername, 200, 0),
    ]);
  } catch {
    notFound();
  }

  try {
    [raRankHistory, raTrackedAchievementHistory, trackedAchievements] = await Promise.all([
      api.getMemberRaRankHistory(raUsername, 200),
      api.getMemberRaAchievementHistory(raUsername, 2000, true),
      api.getMemberAchievements(raUsername, 200, 0, true),
    ]);
  } catch {
    raRankHistory = { items: [] };
    raTrackedAchievementHistory = { items: [] };
    trackedAchievements = { total: 0, offset: 0, limit: 0, items: [] };
  }

  try {
    raSummary = await api.getMemberRaSummary(raUsername);
  } catch {
    raSummary = unavailableRaSummary;
  }

  let leaderboardScoresSyncedAt: string | null = null;
  for (const item of history.items) {
    if (
      !leaderboardScoresSyncedAt ||
      new Date(item.syncedAt) > new Date(leaderboardScoresSyncedAt)
    ) {
      leaderboardScoresSyncedAt = item.syncedAt;
    }
  }

  return (
    <div className="space-y-8">
      <MemberProfileHero detail={detail} raSummary={raSummary} />

      <Suspense fallback={null}>
        <MemberProfileTabs
          overview={
            <MemberRaSummarySection data={raSummary} rankHistory={raRankHistory.items} />
          }
          leaderboards={
            <>
              <MemberTrendCharts items={history.items} />
              <section className="space-y-3">
                <h2 className="steam-section-heading">Current standings</h2>
                <LastSyncedLabel at={leaderboardScoresSyncedAt} prefix="Scores last synced" />
                {detail.standings.length === 0 ? (
                  <p className="text-muted-foreground">No scores synced yet.</p>
                ) : (
                  <div className="md:overflow-x-auto">
                    <MemberStandingsSection standings={detail.standings} />
                  </div>
                )}
              </section>
            </>
          }
          achievements={
            <>
              <section className="space-y-3">
                <h2 className="steam-section-heading">Tracked game unlocks</h2>
                <LastSyncedLabel at={trackedAchievements.achievementsProgressSyncedAt} />
                <MemberTrackedAchievementsTable items={trackedAchievements.items} />
              </section>
              <section className="space-y-3">
                <div className="space-y-1">
                  <h2 className="steam-section-heading">Unlock activity</h2>
                  <p className="text-sm text-muted-foreground">Tracked games only.</p>
                </div>
                <MemberRaAchievementTrends items={raTrackedAchievementHistory.items} />
              </section>
            </>
          }
        />
      </Suspense>
    </div>
  );
}
