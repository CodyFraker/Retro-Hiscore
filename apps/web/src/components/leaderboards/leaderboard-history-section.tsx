import Link from "next/link";
import { FriendRank } from "@/components/friend-rank";
import { MemberAvatar } from "@/components/members/member-avatar";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { LeaderboardHistoryItemDto } from "@/generated/api-client";
import { formatSyncTime } from "@/lib/format";
import { formatGlobalRank } from "@/lib/format-global-rank";

type Props = {
  items: LeaderboardHistoryItemDto[];
};

export function LeaderboardHistorySection({ items }: Props) {
  return (
    <>
      <div className="hidden md:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Synced</TableHead>
              <TableHead>Player</TableHead>
              <TableHead className="text-right">Score</TableHead>
              <TableHead className="text-right">Friend</TableHead>
              <TableHead className="text-right">Global</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.map((item) => (
              <TableRow key={item.id}>
                <TableCell className="text-xs text-muted-foreground">
                  {formatSyncTime(item.syncedAt)}
                </TableCell>
                <TableCell>
                  <Link
                    href={`/members/${encodeURIComponent(item.raUsername)}`}
                    className="flex items-center gap-2 hover:text-[var(--accent-retro)]"
                  >
                    <MemberAvatar avatarUrl={item.avatarUrl} displayName={item.displayName} size={24} />
                    {item.displayName}
                  </Link>
                </TableCell>
                <TableCell className="text-right font-mono">{item.formattedScore}</TableCell>
                <TableCell className="text-right font-mono">
                  <FriendRank rank={item.friendRank} className="justify-end" />
                </TableCell>
                <TableCell className="text-right font-mono text-muted-foreground">
                  {formatGlobalRank(item.globalRank, item.globalEntryCount) ?? "—"}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <ul className="space-y-3 md:hidden">
        {items.map((item) => (
          <li
            key={item.id}
            className="rounded border border-border bg-card p-4 text-sm"
          >
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
        ))}
      </ul>
    </>
  );
}
