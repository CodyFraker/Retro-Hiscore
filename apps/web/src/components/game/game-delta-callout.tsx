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
      <h2 className="text-lg font-medium">Since last sync</h2>
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
                  Rank{" "}
                  <span className="text-foreground">
                    {delta.friendRankDelta > 0
                      ? `↑${delta.friendRankDelta}`
                      : delta.friendRankDelta < 0
                        ? `↓${Math.abs(delta.friendRankDelta)}`
                        : "—"}
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
