import { Card, CardContent } from "@/components/ui/card";
import type { MemberRaSummaryDto } from "@/generated/api-client";

type Props = {
  summary?: MemberRaSummaryDto | null;
  friendRankOnes?: number;
  boardsWithScore?: number;
};

function formatRankLabel(rank?: number | null, totalRanked?: number | null) {
  if (rank == null) {
    return "—";
  }

  if (totalRanked == null || totalRanked <= 0) {
    return `#${rank.toLocaleString()}`;
  }

  const topPercent = (rank / totalRanked) * 100;
  return `#${rank.toLocaleString()} · top ${topPercent.toFixed(1)}%`;
}

function StatTile({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <Card size="sm" className="min-w-0 flex-1">
      <CardContent className="space-y-1">
        <p className="text-xs uppercase tracking-wide text-muted-foreground">{label}</p>
        <p className="font-mono text-lg font-medium tabular-nums">{value}</p>
        {hint ? <p className="text-xs text-muted-foreground">{hint}</p> : null}
      </CardContent>
    </Card>
  );
}

export function MemberRaStatsRow({ summary, friendRankOnes, boardsWithScore }: Props) {
  const showFriendStats = friendRankOnes != null && boardsWithScore != null;
  const showSoftcore = summary != null && (summary.totalSoftcorePoints ?? 0) > 0;

  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
      {showFriendStats ? (
        <>
          <StatTile label="Board leads" value={friendRankOnes.toLocaleString()} />
          <StatTile label="Boards scored" value={boardsWithScore.toLocaleString()} />
        </>
      ) : null}
      {summary ? (
        <>
          <StatTile
            label="Hardcore points"
            value={summary.totalPoints?.toLocaleString() ?? "—"}
          />
          <StatTile
            label="True points"
            value={summary.totalTruePoints?.toLocaleString() ?? "—"}
            hint="Weighted by difficulty"
          />
          <StatTile
            label="Site rank"
            value={formatRankLabel(summary.rank, summary.totalRanked)}
            hint={
              summary.totalRanked != null
                ? `of ${summary.totalRanked.toLocaleString()} ranked players`
                : undefined
            }
          />
          {showSoftcore ? (
            <StatTile
              label="Softcore points"
              value={summary.totalSoftcorePoints!.toLocaleString()}
            />
          ) : null}
        </>
      ) : null}
    </div>
  );
}
