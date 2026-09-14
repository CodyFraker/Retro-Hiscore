import { notFound } from "next/navigation";
import { MemberProfileHero } from "@/components/members/member-profile-hero";
import { MemberRaSummarySection } from "@/components/members/member-ra-summary-section";
import { MemberStandingsSection } from "@/components/members/member-standings-section";
import { MemberTrendCharts } from "@/components/members/member-trend-charts";
import type {
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
  let raAchievementHistory: MemberRaAchievementHistoryResponse = { items: [] };

  try {
    [detail, history] = await Promise.all([
      api.getMember(raUsername),
      api.getMemberHistory(raUsername, 200, 0),
    ]);
  } catch {
    notFound();
  }

  try {
    [raRankHistory, raAchievementHistory] = await Promise.all([
      api.getMemberRaRankHistory(raUsername, 200),
      api.getMemberRaAchievementHistory(raUsername, 2000),
    ]);
  } catch {
    raRankHistory = { items: [] };
    raAchievementHistory = { items: [] };
  }

  try {
    raSummary = await api.getMemberRaSummary(raUsername);
  } catch {
    raSummary = unavailableRaSummary;
  }

  return (
    <div className="space-y-8">
      <MemberProfileHero detail={detail} raSummary={raSummary} />

      <MemberRaSummarySection
        data={raSummary}
        rankHistory={raRankHistory.items}
        achievementHistory={raAchievementHistory.items}
      />

      <MemberTrendCharts items={history.items} />

      <section className="space-y-3">
        <h2 className="steam-section-heading">Current standings</h2>
        {detail.standings.length === 0 ? (
          <p className="text-muted-foreground">No scores synced yet.</p>
        ) : (
          <div className="md:overflow-x-auto">
            <MemberStandingsSection standings={detail.standings} />
          </div>
        )}
      </section>
    </div>
  );
}
