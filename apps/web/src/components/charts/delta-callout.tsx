import type { MemberDelta } from "@/lib/history-series";

type Props = {
  deltas: MemberDelta[];
};

function formatSigned(value: number) {
  const abs = Math.abs(value).toLocaleString();
  if (value > 0) return `+${abs}`;
  if (value < 0) return `−${abs}`;
  return "0";
}

export function DeltaCallout({ deltas }: Props) {
  const actionable = deltas.filter((d) => d.scoreDelta != null || d.friendRankDelta != null);
  if (actionable.length === 0) {
    return null;
  }

  return (
    <section className="space-y-3">
      <h2 className="text-lg font-medium">Since last sync</h2>
      <ul className="divide-y divide-border border-y border-border">
        {actionable.map((delta) => (
          <li
            key={delta.memberId}
            className="flex flex-wrap items-center justify-between gap-2 py-3 text-sm"
          >
            <span className="font-medium">{delta.displayName}</span>
            <div className="flex gap-4 font-mono text-xs text-muted-foreground">
              {delta.scoreDelta != null && (
                <span>
                  Score{" "}
                  <span className="text-foreground">{formatSigned(delta.scoreDelta)}</span>
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
