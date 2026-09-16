import { PageHero } from "@/components/layout/page-hero";
import { RivalryPicker } from "@/components/rivalry/rivalry-picker";
import { RivalryQuickMatchups } from "@/components/rivalry/rivalry-quick-matchups";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function RivalryHubPage() {
  const api = await getServerApiClient();
  const [members, dashboard] = await Promise.all([api.getMembers(), api.getDashboard()]);

  return (
    <div className="mx-auto max-w-3xl space-y-8">
      <PageHero
        title="Head-to-head"
        description="Compare friend ranks and scores on every tracked game between two members."
      />
      <section className="space-y-3 rounded border border-border p-5">
        <h2 className="text-lg font-semibold">Pick a matchup</h2>
        <RivalryPicker members={members} />
      </section>
      <RivalryQuickMatchups championship={dashboard.championship} />
      <section className="space-y-2 text-sm text-muted-foreground">
        <p>
          Rivalry pages list board-by-board leads, closest battles, and links into each game. URLs use
          alphabetical usernames so either order works.
        </p>
      </section>
    </div>
  );
}
