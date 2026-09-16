import type { ReactNode } from "react";
import Link from "next/link";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import type { MembersSummaryDto } from "@/generated/api-client";

type Props = {
  summary: MembersSummaryDto;
  runnerUpFriendRankOnes?: number | null;
};

function Stat({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="rounded-md border border-border bg-card px-4 py-3">
      <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">{label}</p>
      <p className="mt-1 text-2xl font-semibold tabular-nums">{value}</p>
    </div>
  );
}

export function MembersHubSummary({ summary, runnerUpFriendRankOnes }: Props) {
  const leader = summary.championshipLeaderRaUsername;
  const top = summary.topGameLast7Days;

  return (
    <div className="space-y-3">
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
        <Stat label="Members" value={summary.memberCount} />
        <Stat label="Unlocks (7 days)" value={summary.unlocksLast7Days} />
        <Stat label="Unlocks (30 days)" value={summary.unlocksLast30Days} />
        <Stat label="Active (7 days)" value={summary.activeMembersLast7Days} />
        <Stat label="Playing now" value={summary.playingNowCount} />
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        <div className="rounded-md border border-border bg-card px-4 py-3 text-sm">
          <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
            Last unlock
          </p>
          <p className="mt-1">
            {summary.lastUnlockAt ? (
              <FormattedSyncTime value={summary.lastUnlockAt} />
            ) : (
              <span className="text-muted-foreground">—</span>
            )}
          </p>
        </div>
        {summary.raMetricsFreshnessAt ? (
          <div className="rounded-md border border-border bg-card px-4 py-3 text-sm">
            <LastSyncedLabel at={summary.raMetricsFreshnessAt} />
          </div>
        ) : null}
      </div>

      {summary.memberCount >= 2 && leader && summary.championshipLeaderDisplayName ? (
        <div className="rounded-md border border-border bg-card px-4 py-3 text-sm">
          <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
            Friend championship leader
          </p>
          <p className="mt-1">
            <Link
              href={`/members/${encodeURIComponent(leader)}`}
              className="font-medium hover:text-[var(--accent-retro)]"
            >
              {summary.championshipLeaderDisplayName}
            </Link>
            {summary.championshipLeaderFriendRankOnes != null ? (
              <span className="text-muted-foreground">
                {" "}
                · {summary.championshipLeaderFriendRankOnes} board lead
                {summary.championshipLeaderFriendRankOnes === 1 ? "" : "s"}
                {runnerUpFriendRankOnes != null &&
                summary.championshipLeaderFriendRankOnes > runnerUpFriendRankOnes ? (
                  <>
                    {" "}
                    ·{" "}
                    <span className="font-mono">
                      +{summary.championshipLeaderFriendRankOnes - runnerUpFriendRankOnes}
                    </span>{" "}
                    over 2nd
                  </>
                ) : null}
              </span>
            ) : null}
          </p>
        </div>
      ) : null}

      {top ? (
        <div className="rounded-md border border-border bg-card px-4 py-3 text-sm">
          <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
            Top game (7 days)
          </p>
          <p className="mt-1">
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
