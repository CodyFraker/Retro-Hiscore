import { Card, CardContent } from "@/components/ui/card";
import type { MemberRaSummaryDto } from "@/generated/api-client";
import { formatRankLabel } from "@/lib/ra-member-metrics";

type Props = {
  summary?: MemberRaSummaryDto | null;
  friendRankOnes?: number;
  boardsWithScore?: number;
};

function StatTile({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <Card size="sm" className="min-w-[9.5rem] shrink-0 sm:min-w-[10.5rem] sm:flex-1 sm:shrink">
      <CardContent className="space-y-1">
        <p className="text-xs uppercase tracking-wide text-muted-foreground">{label}</p>
        <p className="font-mono text-base font-medium leading-snug tabular-nums sm:text-lg">{value}</p>
        {hint ? <p className="text-xs leading-snug text-muted-foreground">{hint}</p> : null}
      </CardContent>
    </Card>
  );
}

export function MemberRaStatsRow({ summary, friendRankOnes, boardsWithScore }: Props) {
  const showFriendStats = friendRankOnes != null && boardsWithScore != null;
  const showSoftcore = summary != null && (summary.totalSoftcorePoints ?? 0) > 0;

  if (!showFriendStats && !summary) {
    return null;
  }

  return (
    <div className="-mx-4 flex gap-3 overflow-x-auto px-4 scroll-smooth sm:mx-0 sm:flex-wrap sm:overflow-visible sm:px-0">
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
