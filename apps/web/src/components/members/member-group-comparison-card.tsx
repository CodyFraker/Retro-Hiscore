import type { ChampionshipRowDto } from "@/generated/api-client";

type Props = {
  memberId: string;
  friendRankOnes: number;
  championship: ChampionshipRowDto[];
};

export function MemberGroupComparisonCard({ memberId, friendRankOnes, championship }: Props) {
  if (championship.length < 2) {
    return null;
  }

  const totalLeads = championship.reduce((sum, row) => sum + row.friendRankOnes, 0);
  const groupAvg = totalLeads / championship.length;
  const memberRow = championship.find((row) => row.memberId === memberId);
  const rankAmongFriends =
    memberRow != null
      ? championship.findIndex((row) => row.memberId === memberId) + 1
      : null;
  const delta = friendRankOnes - groupAvg;
  const deltaLabel =
    Math.abs(delta) < 0.05
      ? "about average"
      : delta > 0
        ? `${delta.toFixed(1)} above avg`
        : `${Math.abs(delta).toFixed(1)} below avg`;

  return (
    <section className="rounded border border-border bg-card p-5">
      <h2 className="text-sm font-semibold">Vs group</h2>
      <p className="mt-2 text-sm text-muted-foreground">
        <span className="text-foreground font-medium">{friendRankOnes}</span> board leads · group avg{" "}
        <span className="text-foreground font-medium">{groupAvg.toFixed(1)}</span>
        <span className="mx-2 text-border">·</span>
        {deltaLabel}
      </p>
      {rankAmongFriends != null ? (
        <p className="mt-1 text-xs text-muted-foreground">
          #{rankAmongFriends} in the friend championship by board leads
        </p>
      ) : null}
    </section>
  );
}
