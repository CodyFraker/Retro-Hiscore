import Link from "next/link";
import { FormatBadge } from "@/components/format-badge";
import { MemberAvatar } from "@/components/members/member-avatar";
import { FriendRank } from "@/components/friend-rank";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { GameLeaderboardDto, StandingMemberDto } from "@/generated/api-client";
import { formatStandingScore } from "@/lib/format";

type Props = {
  leaderboards: GameLeaderboardDto[];
  members: StandingMemberDto[];
};

export function GameStandingsSection({ leaderboards, members }: Props) {
  return (
    <>
      <div className="hidden md:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="min-w-48">Leaderboard</TableHead>
              {members.map((member) => (
                <TableHead key={member.id} className="min-w-36 text-right">
                  <Link
                    href={`/members/${encodeURIComponent(member.raUsername)}`}
                    className="inline-flex items-center justify-end gap-2 hover:text-[var(--accent-retro)]"
                  >
                    <MemberAvatar avatarUrl={member.avatarUrl} displayName={member.displayName} size={20} />
                    {member.displayName}
                  </Link>
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {leaderboards.map((board) => (
              <TableRow key={board.id}>
                <TableCell>
                  <div className="flex flex-wrap items-center gap-2">
                    <Link
                      href={`/leaderboards/${board.raLeaderboardId}`}
                      className="font-medium hover:text-[var(--accent-retro)]"
                    >
                      {board.title}
                    </Link>
                    <FormatBadge format={board.format} rankAsc={board.rankAsc} />
                  </div>
                  {board.description && (
                    <p className="mt-1 text-xs text-muted-foreground">{board.description}</p>
                  )}
                </TableCell>
                {members.map((member) => {
                  const standing = board.standings.find((s) => s.memberId === member.id);
                  return (
                    <TableCell key={member.id} className="text-right font-mono text-sm">
                      <div>{formatStandingScore(standing)}</div>
                      <div className="text-xs text-muted-foreground">
                        <FriendRank
                          rank={standing?.friendRank}
                          className="justify-end"
                          iconClassName="size-3"
                        />
                      </div>
                      {standing?.globalRank != null && (
                        <div className="text-xs text-muted-foreground">
                          #{standing.globalRank} globally
                        </div>
                      )}
                    </TableCell>
                  );
                })}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <ul className="space-y-3 md:hidden">
        {leaderboards.map((board) => (
          <li
            key={board.id}
            className="rounded border border-border bg-card p-4"
          >
            <div className="flex flex-wrap items-center gap-2">
              <Link
                href={`/leaderboards/${board.raLeaderboardId}`}
                className="font-medium hover:text-[var(--accent-retro)]"
              >
                {board.title}
              </Link>
              <FormatBadge format={board.format} rankAsc={board.rankAsc} />
            </div>
            {board.description && (
              <p className="mt-1 text-xs text-muted-foreground">{board.description}</p>
            )}
            <ul className="mt-3 divide-y divide-border border-t border-border">
              {members.map((member) => {
                const standing = board.standings.find((s) => s.memberId === member.id);
                return (
                  <li
                    key={member.id}
                    className="flex items-center justify-between gap-3 py-2.5 text-sm"
                  >
                    <Link
                      href={`/members/${encodeURIComponent(member.raUsername)}`}
                      className="flex items-center gap-2 font-medium hover:text-[var(--accent-retro)]"
                    >
                      <MemberAvatar avatarUrl={member.avatarUrl} displayName={member.displayName} size={24} />
                      {member.displayName}
                    </Link>
                    <div className="text-right font-mono text-sm">
                      <div>{formatStandingScore(standing)}</div>
                      <div className="text-xs text-muted-foreground">
                        <FriendRank
                          rank={standing?.friendRank}
                          className="justify-end"
                          iconClassName="size-3"
                        />
                      </div>
                      {standing?.globalRank != null && (
                        <div className="text-xs text-muted-foreground">
                          #{standing.globalRank} globally
                        </div>
                      )}
                    </div>
                  </li>
                );
              })}
            </ul>
          </li>
        ))}
      </ul>
    </>
  );
}
