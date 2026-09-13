import Link from "next/link";
import { FormatBadge } from "@/components/format-badge";
import { FriendRank } from "@/components/friend-rank";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { MemberStandingDto } from "@/generated/api-client";

type Props = {
  standings: MemberStandingDto[];
};

export function MemberStandingsSection({ standings }: Props) {
  return (
    <>
      <div className="hidden md:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Game</TableHead>
              <TableHead>Board</TableHead>
              <TableHead className="text-right">Friend</TableHead>
              <TableHead className="text-right">Score</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {standings.map((standing) => (
              <TableRow key={`${standing.raLeaderboardId}-${standing.raGameId}`}>
                <TableCell>
                  <Link
                    href={`/games/${standing.raGameId}`}
                    className="hover:text-[var(--accent-retro)]"
                  >
                    {standing.gameTitle}
                  </Link>
                </TableCell>
                <TableCell>
                  <div className="flex flex-wrap items-center gap-2">
                    <Link
                      href={`/leaderboards/${standing.raLeaderboardId}`}
                      className="hover:text-[var(--accent-retro)]"
                    >
                      {standing.leaderboardTitle}
                    </Link>
                    <FormatBadge format={standing.format} />
                  </div>
                </TableCell>
                <TableCell className="text-right font-mono">
                  <FriendRank rank={standing.friendRank} className="justify-end" />
                </TableCell>
                <TableCell className="text-right font-mono">{standing.formattedScore}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <ul className="space-y-3 md:hidden">
        {standings.map((standing) => (
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
              <FriendRank rank={standing.friendRank} />
              <span className="font-mono text-lg">{standing.formattedScore}</span>
            </div>
          </li>
        ))}
      </ul>
    </>
  );
}
