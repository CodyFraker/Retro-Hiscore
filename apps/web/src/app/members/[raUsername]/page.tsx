import { Suspense, type ReactNode } from "react";
import { notFound } from "next/navigation";
import { MemberCompareLink } from "@/components/rivalry/member-compare-link";
import { MemberProfileHero } from "@/components/members/member-profile-hero";
import { MemberProfileTabs } from "@/components/members/member-profile-tabs";
import { MemberRaAchievementTrends } from "@/components/members/member-ra-achievement-trends";
import { MemberRaSummarySection } from "@/components/members/member-ra-summary-section";
import { MemberStandingsSection } from "@/components/members/member-standings-section";
import { MemberTrackedAchievementsTable } from "@/components/members/member-tracked-achievements-table";
import { MemberTrendCharts } from "@/components/members/member-trend-charts";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import type {
  ChampionshipRowDto,
  MemberAchievementsListResponse,
  MemberRaAchievementHistoryResponse,
  MemberRaRankHistoryResponse,
  MemberRaSummaryResponse,
} from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";
import { resolveMemberProfileTab } from "@/lib/member-profile-tab";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raUsername: string }>;
  searchParams: Promise<{ tab?: string }>;
};

const unavailableRaSummary: MemberRaSummaryResponse = {
  available: false,
  unavailableReason: "Could not load RetroAchievements summary.",
  summary: null,
};

export default async function MemberProfilePage({ params, searchParams }: Props) {
  const { raUsername: raw } = await params;
  const raUsername = decodeURIComponent(raw);
  const { tab: tabParam } = await searchParams;
  const activeTab = resolveMemberProfileTab(tabParam);

  const api = await getServerApiClient();
  let detail: Awaited<ReturnType<typeof api.getMember>>;

  try {
    detail = await api.getMember(raUsername);
  } catch {
    notFound();
  }

  let raSummary: MemberRaSummaryResponse = unavailableRaSummary;
  try {
    raSummary = await api.getMemberRaSummary(raUsername);
  } catch {
    raSummary = unavailableRaSummary;
  }
  let raRankHistory: MemberRaRankHistoryResponse = { items: [] };
  let history: Awaited<ReturnType<typeof api.getMemberHistory>> | null = null;
  let raTrackedAchievementHistory: MemberRaAchievementHistoryResponse = { items: [] };
  let trackedAchievements: MemberAchievementsListResponse = {
    total: 0,
    offset: 0,
    limit: 0,
    items: [],
  };
  let championship: ChampionshipRowDto[] = [];
  let memberCount = 0;
  let recentUnlockActivity: Awaited<
    ReturnType<typeof api.getDashboardAchievementActivity>
  >["items"] = [];
  let viewerRaUsername: string | null = null;
  let pendingRaGameIds: Set<number> = new Set();
  try {
    const current = await api.getCurrentMember();
    viewerRaUsername = current.raUsername ?? null;
  } catch {
    viewerRaUsername = null;
  }
  try {
    const queue = await api.getGameTrackQueue();
    pendingRaGameIds = new Set(
      queue.filter((item) => item.status === "Pending").map((item) => item.raGameId),
    );
  } catch {
    pendingRaGameIds = new Set();
  }

  if (activeTab === "overview") {
    try {
      const [rankHistory, dashboard, membersSummary, unlockActivity] = await Promise.all([
        api.getMemberRaRankHistory(raUsername, 200),
        api.getDashboard(),
        api.getMembersSummary(),
        api.getDashboardAchievementActivity(8, 0, undefined, raUsername),
      ]);
      raRankHistory = rankHistory;
      championship = dashboard.championship;
      memberCount = membersSummary.memberCount;
      recentUnlockActivity = unlockActivity.items;
    } catch {
      try {
        raRankHistory = await api.getMemberRaRankHistory(raUsername, 200);
      } catch {
        raRankHistory = { items: [] };
      }
    }
  } else if (activeTab === "leaderboards") {
    try {
      history = await api.getMemberHistory(raUsername, 200, 0);
    } catch {
      history = { total: 0, offset: 0, limit: 200, items: [] };
    }
  } else if (activeTab === "achievements") {
    try {
      [raTrackedAchievementHistory, trackedAchievements] = await Promise.all([
        api.getMemberRaAchievementHistory(raUsername, 2000, true),
        api.getMemberAchievements(raUsername, 200, 0, true),
      ]);
    } catch {
      raTrackedAchievementHistory = { items: [] };
      trackedAchievements = { total: 0, offset: 0, limit: 0, items: [] };
    }
  }

  let leaderboardScoresSyncedAt: string | null = null;
  if (history) {
    for (const item of history.items) {
      if (
        !leaderboardScoresSyncedAt ||
        new Date(item.syncedAt) > new Date(leaderboardScoresSyncedAt)
      ) {
        leaderboardScoresSyncedAt = item.syncedAt;
      }
    }
  }

  let panel: ReactNode;
  if (activeTab === "leaderboards") {
    panel = (
      <>
        <MemberTrendCharts items={history?.items ?? []} />
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
    );
  } else if (activeTab === "achievements") {
    panel = (
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
    );
  } else {
    panel = (
      <MemberRaSummarySection
        data={raSummary}
        rankHistory={raRankHistory.items}
        championship={championship}
        memberCount={memberCount}
        memberId={detail.id}
        friendRankOnes={detail.friendRankOnes}
        profileRaUsername={raUsername}
        recentUnlockActivity={recentUnlockActivity}
        pendingRaGameIds={pendingRaGameIds}
      />
    );
  }

  return (
    <div className="space-y-8">
      <MemberProfileHero detail={detail} raSummary={raSummary} />
      {viewerRaUsername && detail.raUsername ? (
        <MemberCompareLink
          currentRaUsername={viewerRaUsername}
          otherRaUsername={detail.raUsername}
          otherDisplayName={detail.displayName}
        />
      ) : null}

      <Suspense fallback={null}>
        <MemberProfileTabs activeTab={activeTab} panel={panel} />
      </Suspense>
    </div>
  );
}
