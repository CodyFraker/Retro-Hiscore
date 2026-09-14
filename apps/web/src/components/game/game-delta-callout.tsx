import type { GameDelta } from "@/lib/game-history-series";

type Props = {
  deltas: GameDelta[];
};

function formatSigned(value: number) {
  const abs = Math.abs(value).toLocaleString();
  if (value > 0) return `+${abs}`;
  if (value < 0) return `−${abs}`;
  return "0";
}

export function GameDeltaCallout({ deltas }: Props) {
  if (deltas.length === 0) {
    return null;
  }

  return (
    <section className="space-y-3">
      <h2 className="steam-section-heading">Since last sync</h2>
      <ul className="divide-y divide-border border-y border-border">
        {deltas.map((delta) => (
          <li
            key={`${delta.raLeaderboardId}-${delta.memberId}`}
            className="flex flex-col gap-2 py-3 text-sm sm:flex-row sm:items-center sm:justify-between"
          >
            <div>
              <span className="font-medium">{delta.displayName}</span>
              <span className="text-muted-foreground"> · {delta.leaderboardTitle}</span>
            </div>
            <div className="flex flex-wrap gap-4 font-mono text-xs text-muted-foreground">
              {delta.scoreDelta != null && (
                <span>
                  Score <span className="text-foreground">{formatSigned(delta.scoreDelta)}</span>
                </span>
              )}
              {delta.friendRankDelta != null && (
                <span>
                  Friend rank{" "}
                  <span className="text-foreground">
                    {delta.friendRankDelta > 0
                      ? `↑${delta.friendRankDelta}`
                      : delta.friendRankDelta < 0
                        ? `↓${Math.abs(delta.friendRankDelta)}`
                        : "—"}
                  </span>
                </span>
              )}
              {delta.globalRankDelta != null && delta.globalRankDelta !== 0 && (
                <span>
                  Global{" "}
                  <span className="text-foreground">
                    {delta.globalRankDelta > 0
                      ? `↑${delta.globalRankDelta}`
                      : `↓${Math.abs(delta.globalRankDelta)}`}
                  </span>
                </span>
              )}
              {delta.globalEntryCountDelta != null && delta.globalEntryCountDelta !== 0 && (
                <span>
                  Field{" "}
                  <span className="text-foreground">
                    {delta.globalEntryCountDelta > 0
                      ? `+${delta.globalEntryCountDelta.toLocaleString()}`
                      : delta.globalEntryCountDelta.toLocaleString()}
                  </span>
                </span>
              )}
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
}
