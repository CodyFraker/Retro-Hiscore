import Link from "next/link";
import { FriendRank } from "@/components/friend-rank";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { MemberAvatar } from "@/components/members/member-avatar";
import type { LeaderboardHistoryItemDto } from "@/generated/api-client";
import { formatSyncTime } from "@/lib/format";
import { formatGlobalRank } from "@/lib/format-global-rank";

type Props = {
  items: LeaderboardHistoryItemDto[];
};

export function LeaderboardHistorySection({ items }: Props) {
  return (
    <ResponsiveTable
      rows={items}
      rowKey={(item) => item.id}
      columns={[
        {
          header: "Synced",
          cellClassName: "text-xs text-muted-foreground",
          render: (item) => formatSyncTime(item.syncedAt),
        },
        {
          header: "Player",
          render: (item) => (
            <Link
              href={`/members/${encodeURIComponent(item.raUsername)}`}
              className="flex items-center gap-2 hover:text-[var(--accent-retro)]"
            >
              <MemberAvatar avatarUrl={item.avatarUrl} displayName={item.displayName} size={24} />
              {item.displayName}
            </Link>
          ),
        },
        {
          header: "Score",
          headerClassName: "text-right",
          cellClassName: "text-right font-mono",
          render: (item) => item.formattedScore,
        },
        {
          header: "Friend",
          headerClassName: "text-right",
          cellClassName: "text-right font-mono",
          render: (item) => <FriendRank rank={item.friendRank} className="justify-end" />,
        },
        {
          header: "Global",
          headerClassName: "text-right",
          cellClassName: "text-right font-mono text-muted-foreground",
          render: (item) => formatGlobalRank(item.globalRank, item.globalEntryCount) ?? "—",
        },
      ]}
      renderMobileCard={(item) => (
        <li key={item.id} className="rounded border border-border bg-card p-4 text-sm">
          <div className="flex items-start justify-between gap-3">
            <Link
              href={`/members/${encodeURIComponent(item.raUsername)}`}
              className="flex items-center gap-2 font-medium hover:text-[var(--accent-retro)]"
            >
              <MemberAvatar avatarUrl={item.avatarUrl} displayName={item.displayName} size={24} />
              {item.displayName}
            </Link>
            <span className="font-mono text-lg">{item.formattedScore}</span>
          </div>
          <div className="mt-2 flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
            <span>{formatSyncTime(item.syncedAt)}</span>
            <span>
              Friend{" "}
              <FriendRank rank={item.friendRank} className="inline-flex" iconClassName="size-3" />
            </span>
            <span>
              Global{" "}
              <span className="font-mono text-foreground">
                {formatGlobalRank(item.globalRank, item.globalEntryCount) ?? "—"}
              </span>
            </span>
          </div>
        </li>
      )}
    />
  );
}
