import { PageHero } from "@/components/layout/page-hero";
import { MembersHubSummary } from "@/components/members/members-hub-summary";
import { MembersRosterTable } from "@/components/members/members-roster-table";
import { HeadToHeadSection } from "@/components/rivalry/head-to-head-section";
import type { MemberDto, MembersSummaryDto } from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function MembersPage() {
  let members: MemberDto[] = [];
  let summary: MembersSummaryDto | null = null;
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    [members, summary] = await Promise.all([api.getMembers(), api.getMembersSummary()]);
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load members";
  }

  return (
    <div className="space-y-8">
      <PageHero
        title="Members"
        description="Friend roster with board standings, RetroAchievements ranks, and recent activity."
      />

      {error && (
        <p className="rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">
          {error}
        </p>
      )}

      {!error && summary ? (
        <MembersHubSummary
          summary={summary}
          runnerUpFriendRankOnes={
            members.length >= 2 ? members[1]?.friendRankOnes ?? null : null
          }
        />
      ) : null}

      {!error && members.length > 0 ? <HeadToHeadSection members={members} /> : null}

      {!error && members.length === 0 && (
        <p className="text-muted-foreground">No members seeded yet.</p>
      )}

      {!error && members.length > 0 && (
        <section className="space-y-3">
          <h2 className="steam-section-heading">Roster</h2>
          <MembersRosterTable members={members} />
        </section>
      )}
    </div>
  );
}
