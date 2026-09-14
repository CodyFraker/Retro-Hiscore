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
import type { FriendStandingDto } from "@/generated/api-client";
import { formatStandingScore, formatSyncTime } from "@/lib/format";
import { formatGlobalRank } from "@/lib/format-global-rank";

type Props = {
  standings: FriendStandingDto[];
  globalEntryCount?: number | null;
};

export function LeaderboardStandingsSection({ standings, globalEntryCount }: Props) {
  return (
    <>
      <div className="hidden md:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Friend rank</TableHead>
              <TableHead>Player</TableHead>
              <TableHead className="text-right">Score</TableHead>
              <TableHead className="text-right">Global</TableHead>
              <TableHead className="text-right">Updated</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {standings.map((standing) => (
              <TableRow key={standing.memberId}>
                <TableCell className="font-mono">
                  <FriendRank rank={standing.friendRank} />
                </TableCell>
                <TableCell>
                  <Link
                    href={`/members/${encodeURIComponent(standing.raUsername)}`}
                    className="flex items-center gap-2 hover:text-[var(--accent-retro)]"
                  >
                    <MemberAvatar avatarUrl={standing.avatarUrl} displayName={standing.displayName} size={24} />
                    {standing.displayName}
                  </Link>
                </TableCell>
                <TableCell className="text-right font-mono">
                  {formatStandingScore(standing)}
                </TableCell>
                <TableCell className="text-right font-mono text-muted-foreground">
                  {formatGlobalRank(standing.globalRank, globalEntryCount) ?? "—"}
                </TableCell>
                <TableCell className="text-right text-xs text-muted-foreground">
                  {standing.scoreUpdatedAt ? formatSyncTime(standing.scoreUpdatedAt) : "No score"}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <ul className="space-y-3 md:hidden">
        {standings.map((standing) => (
          <li
            key={standing.memberId}
            className="rounded border border-border bg-card p-4"
          >
            <div className="flex items-start justify-between gap-3">
              <div className="space-y-1">
                <FriendRank rank={standing.friendRank} />
                <Link
                  href={`/members/${encodeURIComponent(standing.raUsername)}`}
                  className="flex items-center gap-2 font-medium hover:text-[var(--accent-retro)]"
                >
                  <MemberAvatar avatarUrl={standing.avatarUrl} displayName={standing.displayName} size={24} />
                  {standing.displayName}
                </Link>
              </div>
              <div className="text-right font-mono text-lg">
                {formatStandingScore(standing)}
              </div>
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
        ))}
      </ul>
    </>
  );
}
