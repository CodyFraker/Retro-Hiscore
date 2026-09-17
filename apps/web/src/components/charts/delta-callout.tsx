import type { MemberDelta } from "@/lib/history-series";
import { formatFriendRankDelta, formatPopulationDelta } from "@/lib/game-delta-format";
import { formatLeaderboardScoreDelta } from "@/lib/leaderboard-score-format";

type Props = {
  deltas: MemberDelta[];
  scoreFormat?: string | null;
  globalEntryCountDelta?: number | null;
};

function memberHasMovement(delta: MemberDelta) {
  return (
    (delta.scoreDelta != null && delta.scoreDelta !== 0) ||
    (delta.friendRankDelta != null && delta.friendRankDelta !== 0) ||
    (delta.globalRankDelta != null && delta.globalRankDelta !== 0)
  );
}

export function DeltaCallout({ deltas, scoreFormat, globalEntryCountDelta }: Props) {
  const actionable = deltas.filter(memberHasMovement);
  const showField =
    globalEntryCountDelta != null && globalEntryCountDelta !== 0;

  if (actionable.length === 0 && !showField) {
    return null;
  }

  return (
    <section className="space-y-3">
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <h2 className="steam-section-heading">Since last sync</h2>
        {showField ? (
          <p className="font-mono text-xs text-muted-foreground">
            Field{" "}
            <span className="text-foreground">
              {formatPopulationDelta(globalEntryCountDelta!)}
            </span>
          </p>
        ) : null}
      </div>
      {actionable.length > 0 ? (
        <ul className="divide-y divide-border border-y border-border">
          {actionable.map((delta) => (
            <li
              key={delta.memberId}
              className="flex flex-wrap items-center justify-between gap-2 py-3 text-sm"
            >
              <span className="font-medium">{delta.displayName}</span>
              <div className="flex flex-wrap gap-4 font-mono text-xs text-muted-foreground">
                {delta.scoreDelta != null && delta.scoreDelta !== 0 && (
                  <span>
                    Score{" "}
                    <span className="text-foreground">
                      {formatLeaderboardScoreDelta(delta.scoreDelta, scoreFormat)}
                    </span>
                  </span>
                )}
                {delta.friendRankDelta != null && delta.friendRankDelta !== 0 && (
                  <span>
                    Friend rank{" "}
                    <span className="text-foreground">
                      {formatFriendRankDelta(delta.friendRankDelta)}
                    </span>
                  </span>
                )}
                {delta.globalRankDelta != null && delta.globalRankDelta !== 0 && (
                  <span>
                    Global{" "}
                    <span className="text-foreground">
                      {formatFriendRankDelta(delta.globalRankDelta)}
                    </span>
                  </span>
                )}
              </div>
            </li>
          ))}
        </ul>
      ) : null}
    </section>
  );
}
