import type { ReactNode } from "react";
import Link from "next/link";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import type { DashboardAchievementSummaryResponse } from "@/generated/api-client";

type Props = {
  summary: DashboardAchievementSummaryResponse;
};

function Stat({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="rounded-md border border-border bg-card px-4 py-3">
      <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">{label}</p>
      <p className="mt-1 text-2xl font-semibold tabular-nums">{value}</p>
    </div>
  );
}

export function AchievementsHubSummary({ summary }: Props) {
  const top = summary.topGameLast7Days;

  return (
    <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
      <Stat label="Unlocks (7 days)" value={summary.unlocksLast7Days} />
      <Stat label="Unlocks (30 days)" value={summary.unlocksLast30Days} />
      <Stat label="Active members (7 days)" value={summary.activeMembersLast7Days} />
      <Stat
        label="Last unlock"
        value={
          summary.lastUnlockAt ? (
            <span className="text-lg font-semibold">
              <FormattedSyncTime value={summary.lastUnlockAt} />
            </span>
          ) : (
            "—"
          )
        }
      />
      {top ? (
        <div className="rounded-md border border-border bg-card px-4 py-3 sm:col-span-2 lg:col-span-4">
          <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
            Top game (7 days)
          </p>
          <p className="mt-1 text-sm">
            <Link
              href={`/games/${top.raGameId}`}
              className="font-medium hover:text-[var(--accent-retro)]"
            >
              {top.title}
            </Link>
            <span className="text-muted-foreground"> · </span>
            <span className="tabular-nums">{top.unlockCount} unlocks</span>
          </p>
        </div>
      ) : null}
    </div>
  );
}
