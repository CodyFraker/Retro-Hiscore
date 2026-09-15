"use client";

import { useSyncExternalStore } from "react";
import type { GameLeaderboardSyncStatusDto } from "@/generated/api-client";
import { formatSyncTime, formatSyncTimeUtc } from "@/lib/format";
import {
  leaderboardSyncTierDescription,
  leaderboardSyncTierLabel,
} from "@/lib/leaderboard-sync-tier";

type Props = {
  status: GameLeaderboardSyncStatusDto;
  className?: string;
};

function emptySubscribe() {
  return () => {};
}

export function LeaderboardSyncTierBadge({ status, className }: Props) {
  const isHot = status.tier === "Hot";
  const label = leaderboardSyncTierLabel(status.tier, status.leaderboardSyncForcedCold);
  const description = useSyncExternalStore(
    emptySubscribe,
    () => leaderboardSyncTierDescription(status, formatSyncTime),
    () => leaderboardSyncTierDescription(status, formatSyncTimeUtc),
  );

  return (
    <span
      title={description}
      className={
        className ??
        `inline-flex rounded-md px-2 py-0.5 text-xs font-medium ${
          isHot
            ? "bg-orange-500/15 text-orange-700 dark:text-orange-300"
            : "bg-secondary text-secondary-foreground"
        }`
      }
    >
      {label}
    </span>
  );
}
