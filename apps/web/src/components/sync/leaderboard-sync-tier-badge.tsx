"use client";

import { Flame, Snowflake } from "lucide-react";
import { useRouter } from "next/navigation";
import { useSyncExternalStore } from "react";
import type { GameLeaderboardSyncStatusDto } from "@/generated/api-client";
import { Badge } from "@/components/ui/badge";
import { formatSyncTime, formatSyncTimeUtc } from "@/lib/format";
import {
  leaderboardSyncTierDescription,
  leaderboardSyncTierLabel,
} from "@/lib/leaderboard-sync-tier";
import { cn } from "@/lib/utils";

type Props = {
  status: GameLeaderboardSyncStatusDto;
  className?: string;
  learnMoreHref?: string;
};

function emptySubscribe() {
  return () => {};
}

export function LeaderboardSyncTierBadge({
  status,
  className,
  learnMoreHref = "/settings",
}: Props) {
  const router = useRouter();
  const isHot = status.tier === "Hot" && !status.leaderboardSyncForcedCold;
  const label = leaderboardSyncTierLabel(status.tier, status.leaderboardSyncForcedCold);
  const description = useSyncExternalStore(
    emptySubscribe,
    () => leaderboardSyncTierDescription(status, formatSyncTime),
    () => leaderboardSyncTierDescription(status, formatSyncTimeUtc),
  );
  const detailsId = `lb-sync-tier-${status.tier}-${status.leaderboardSyncIsDue ? "due" : "ok"}`;

  return (
    <div className={cn("inline-flex max-w-full flex-col items-start gap-1", className)}>
      <div className="flex flex-wrap items-center gap-1.5">
        <Badge
          variant="outline"
          aria-label={label}
          title={label}
          className={cn(
            "border-2 bg-card/80 font-semibold",
            isHot
              ? "border-[var(--accent-retro)] text-[var(--accent-retro)]"
              : "border-border text-foreground",
          )}
        >
          {isHot ? (
            <Flame className="size-3.5 shrink-0" aria-hidden />
          ) : (
            <Snowflake className="size-3.5 shrink-0" aria-hidden />
          )}
        </Badge>
        {status.leaderboardSyncIsDue ? (
          <Badge
            variant="outline"
            className="border-2 border-[var(--accent-retro)] bg-[color-mix(in_oklab,var(--accent-retro),var(--steam-olive-dark)_55%)] font-semibold text-[var(--steam-cream)]"
          >
            Due
          </Badge>
        ) : null}
        <details className="text-xs">
          <summary className="cursor-pointer list-none text-[var(--accent-retro)] underline-offset-2 hover:underline [&::-webkit-details-marker]:hidden">
            Schedule
          </summary>
          <p id={detailsId} className="mt-1 max-w-sm text-muted-foreground">
            {description}{" "}
            <button
              type="button"
              className="text-[var(--accent-retro)] hover:underline"
              onClick={(event) => {
                event.preventDefault();
                event.stopPropagation();
                router.push(learnMoreHref);
              }}
            >
              Learn more
            </button>
          </p>
        </details>
      </div>
    </div>
  );
}
