import Link from "next/link";
import { FriendRank } from "@/components/friend-rank";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { MemberAvatar } from "@/components/members/member-avatar";
import type { FriendStandingDto } from "@/generated/api-client";
import { formatStandingScore, formatSyncTime } from "@/lib/format";
import { formatGlobalRank } from "@/lib/format-global-rank";

type Props = {
  standings: FriendStandingDto[];
  globalEntryCount?: number | null;
};

export function LeaderboardStandingsSection({ standings, globalEntryCount }: Props) {
  return (
    <ResponsiveTable
      rows={standings}
      rowKey={(standing) => standing.memberId}
      columns={[
        {
          header: "Friend rank",
          render: (standing) => (
            <FriendRank rank={standing.friendRank} className="font-mono" />
          ),
        },
        {
          header: "Player",
          render: (standing) => (
            <Link
              href={`/members/${encodeURIComponent(standing.raUsername)}`}
              className="flex items-center gap-2 hover:text-[var(--accent-retro)]"
            >
              <MemberAvatar
                avatarUrl={standing.avatarUrl}
                displayName={standing.displayName}
                size={24}
              />
              {standing.displayName}
            </Link>
          ),
        },
        {
          header: "Score",
          headerClassName: "text-right",
          cellClassName: "text-right font-mono",
          render: (standing) => formatStandingScore(standing),
        },
        {
          header: "Global",
          headerClassName: "text-right",
          cellClassName: "text-right font-mono text-muted-foreground",
          render: (standing) => formatGlobalRank(standing.globalRank, globalEntryCount) ?? "—",
        },
        {
          header: "Updated",
          headerClassName: "text-right",
          cellClassName: "text-right text-xs text-muted-foreground",
          render: (standing) =>
            standing.scoreUpdatedAt ? formatSyncTime(standing.scoreUpdatedAt) : "No score",
        },
      ]}
      renderMobileCard={(standing) => (
        <li key={standing.memberId} className="rounded border border-border bg-card p-4">
          <div className="flex items-start justify-between gap-3">
            <div className="space-y-1">
              <FriendRank rank={standing.friendRank} />
              <Link
                href={`/members/${encodeURIComponent(standing.raUsername)}`}
                className="flex items-center gap-2 font-medium hover:text-[var(--accent-retro)]"
              >
                <MemberAvatar
                  avatarUrl={standing.avatarUrl}
                  displayName={standing.displayName}
                  size={24}
                />
                {standing.displayName}
              </Link>
            </div>
            <div className="text-right font-mono text-lg">{formatStandingScore(standing)}</div>
          </div>
          <div className="mt-2 flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
            <span>
              Global{" "}
              <span className="font-mono text-foreground">
                {formatGlobalRank(standing.globalRank, globalEntryCount) ?? "—"}
              </span>
            </span>
            <span>
              Updated{" "}
              <span className="text-foreground">
                {standing.scoreUpdatedAt ? formatSyncTime(standing.scoreUpdatedAt) : "No score"}
              </span>
            </span>
          </div>
        </li>
      )}
    />
  );
}
