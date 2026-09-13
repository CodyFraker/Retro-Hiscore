import Link from "next/link";
import { FriendRank } from "@/components/friend-rank";
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

type Props = {
  standings: FriendStandingDto[];
};

export function LeaderboardStandingsSection({ standings }: Props) {
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
                    className="hover:text-[var(--accent-retro)]"
                  >
                    {standing.displayName}
                  </Link>
                </TableCell>
                <TableCell className="text-right font-mono">
                  {formatStandingScore(standing)}
                </TableCell>
                <TableCell className="text-right font-mono text-muted-foreground">
                  {standing.globalRank != null ? `#${standing.globalRank}` : "—"}
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
                  className="block font-medium hover:text-[var(--accent-retro)]"
                >
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
                  {standing.globalRank != null ? `#${standing.globalRank}` : "—"}
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
