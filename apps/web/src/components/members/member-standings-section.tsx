import Link from "next/link";
import { FormatBadge } from "@/components/format-badge";
import { FriendRank } from "@/components/friend-rank";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import type { MemberStandingDto } from "@/generated/api-client";
import { formatGlobalRank } from "@/lib/format-global-rank";

type Props = {
  standings: MemberStandingDto[];
};

export function MemberStandingsSection({ standings }: Props) {
  return (
    <ResponsiveTable
      rows={standings}
      rowKey={(standing) => `${standing.raLeaderboardId}-${standing.raGameId}`}
      columns={[
        {
          header: "Game",
          render: (standing) => (
            <Link href={`/games/${standing.raGameId}`} className="hover:text-[var(--accent-retro)]">
              {standing.gameTitle}
            </Link>
          ),
        },
        {
          header: "Board",
          render: (standing) => (
            <div className="flex flex-wrap items-center gap-2">
              <Link
                href={`/leaderboards/${standing.raLeaderboardId}`}
                className="hover:text-[var(--accent-retro)]"
              >
                {standing.leaderboardTitle}
              </Link>
              <FormatBadge format={standing.format} />
            </div>
          ),
        },
        {
          header: "Friend",
          headerClassName: "text-right",
          cellClassName: "text-right font-mono",
          render: (standing) => <FriendRank rank={standing.friendRank} className="justify-end" />,
        },
        {
          header: "Global",
          headerClassName: "text-right",
          cellClassName: "text-right font-mono text-xs text-muted-foreground",
          render: (standing) =>
            formatGlobalRank(standing.globalRank, standing.globalEntryCount) ?? "—",
        },
        {
          header: "Score",
          headerClassName: "text-right",
          cellClassName: "text-right font-mono",
          render: (standing) => standing.formattedScore,
        },
      ]}
      renderMobileCard={(standing) => (
        <li
          key={`${standing.raLeaderboardId}-${standing.raGameId}`}
          className="rounded border border-border bg-card p-4 text-sm"
        >
          <Link
            href={`/games/${standing.raGameId}`}
            className="font-medium hover:text-[var(--accent-retro)]"
          >
            {standing.gameTitle}
          </Link>
          <div className="mt-1 flex flex-wrap items-center gap-2">
            <Link
              href={`/leaderboards/${standing.raLeaderboardId}`}
              className="text-muted-foreground hover:text-[var(--accent-retro)]"
            >
              {standing.leaderboardTitle}
            </Link>
            <FormatBadge format={standing.format} />
          </div>
          <div className="mt-3 flex items-center justify-between gap-3 border-t border-border pt-3">
            <div className="space-y-1">
              <FriendRank rank={standing.friendRank} />
              <p className="text-xs text-muted-foreground">
                {formatGlobalRank(standing.globalRank, standing.globalEntryCount) ?? "No global rank"}
              </p>
            </div>
            <span className="font-mono text-lg">{standing.formattedScore}</span>
          </div>
        </li>
      )}
    />
  );
}
