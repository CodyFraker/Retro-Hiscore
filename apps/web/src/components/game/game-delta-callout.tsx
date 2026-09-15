"use client";

import { useMemo, useState } from "react";
import type { GameDelta } from "@/lib/game-history-series";
import {
  formatFriendRankDelta,
  formatPopulationDelta,
  formatSignedDelta,
} from "@/lib/game-delta-format";
import { isGameDeltaMover, sortGameDeltasByMovement } from "@/lib/game-history-series";

type Props = {
  deltas: GameDelta[];
};

export function GameDeltaCallout({ deltas }: Props) {
  const [moversOnly, setMoversOnly] = useState(true);

  const visibleDeltas = useMemo(() => {
    const sorted = sortGameDeltasByMovement(deltas);
    if (!moversOnly) {
      return sorted;
    }
    return sorted.filter(isGameDeltaMover);
  }, [deltas, moversOnly]);

  if (deltas.length === 0) {
    return null;
  }

  return (
    <section className="space-y-3">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h2 className="steam-section-heading">Since last sync</h2>
          <p className="text-xs text-muted-foreground">
            Score and rank changes per member and board since the previous sync.
          </p>
        </div>
        <label className="inline-flex items-center gap-2 text-sm text-muted-foreground">
          <input
            type="checkbox"
            checked={moversOnly}
            onChange={(event) => setMoversOnly(event.target.checked)}
            className="size-4 rounded border-border"
          />
          Movers only
        </label>
      </div>
      {visibleDeltas.length === 0 ? (
        <p className="rounded border border-dashed border-border px-4 py-6 text-sm text-muted-foreground">
          No score or rank movement since the last sync.
        </p>
      ) : (
        <ul className="divide-y divide-border border-y border-border">
          {visibleDeltas.map((delta) => (
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
                    Score <span className="text-foreground">{formatSignedDelta(delta.scoreDelta)}</span>
                  </span>
                )}
                {delta.friendRankDelta != null && (
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
                {delta.globalEntryCountDelta != null && delta.globalEntryCountDelta !== 0 && (
                  <span>
                    Field{" "}
                    <span className="text-foreground">
                      {formatPopulationDelta(delta.globalEntryCountDelta)}
                    </span>
                  </span>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
