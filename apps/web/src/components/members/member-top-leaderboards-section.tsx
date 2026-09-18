import { MemberStandingsSection } from "@/components/members/member-standings-section";
import type { MemberStandingDto } from "@/generated/api-client";
import { filterTopLeaderboardStandings } from "@/lib/member-top-leaderboards";

type Props = {
  standings: MemberStandingDto[];
};

export function MemberTopLeaderboardsSection({ standings }: Props) {
  const topStandings = filterTopLeaderboardStandings(standings);

  return (
    <section className="space-y-3">
      <div className="space-y-1">
        <h2 className="steam-section-heading">Top leaderboards</h2>
        <p className="text-xs text-muted-foreground">Global top 20% on tracked leaderboards.</p>
      </div>
      {topStandings.length === 0 ? (
        <p className="text-muted-foreground">No global top 20% boards yet.</p>
      ) : (
        <div className="md:overflow-x-auto">
          <MemberStandingsSection standings={topStandings} />
        </div>
      )}
    </section>
  );
}
