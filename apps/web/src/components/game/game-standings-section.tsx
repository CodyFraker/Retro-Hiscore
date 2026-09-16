import Link from "next/link";
import { FormatBadge } from "@/components/format-badge";
import { FriendRank } from "@/components/friend-rank";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { MemberAvatar } from "@/components/members/member-avatar";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type {
  FriendStandingDto,
  GameLeaderboardDto,
  StandingMemberDto,
} from "@/generated/api-client";
import { formatStandingScore } from "@/lib/format";
import { formatGlobalRank } from "@/lib/format-global-rank";

function hasNoScore(standing: FriendStandingDto | null | undefined): boolean {
  return !standing || standing.score == null;
}

function StandingScoreCell({
  standing,
  compactEmptyScores,
}: {
  standing: FriendStandingDto | undefined;
  compactEmptyScores?: boolean;
}) {
  const scoreText = formatStandingScore(standing);
  if (hasNoScore(standing)) {
    if (compactEmptyScores) {
      return <div>{scoreText}</div>;
    }
    return (
      <div>
        <div>{scoreText}</div>
        <p className="text-xs text-muted-foreground">No score yet</p>
      </div>
    );
  }

  return <div>{scoreText}</div>;
}

function leaderCellClass(standing: FriendStandingDto | undefined): string {
  if (standing?.friendRank === 1) {
    return "bg-[var(--accent-retro)]/10 ring-1 ring-inset ring-[var(--accent-retro)]/25";
  }
  return "";
}

type Props = {
  leaderboards: GameLeaderboardDto[];
  members: StandingMemberDto[];
  compactEmptyScores?: boolean;
};

export function GameStandingsSection({
  leaderboards,
  members,
  compactEmptyScores = false,
}: Props) {
  if (members.length === 0) {
    return (
      <p className="mt-8 text-sm text-muted-foreground">
        No friend scores synced for this game yet. Refresh scores or wait for the next sync after
        someone plays.
      </p>
    );
  }

  return (
    <div className="mt-8 mb-8">
    <ResponsiveTable
      rows={leaderboards}
      rowKey={(board) => String(board.id)}
      desktopClassName="overflow-x-auto rounded border border-border"
      renderDesktop={() => (
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
                    <MemberAvatar
                      avatarUrl={member.avatarUrl}
                      displayName={member.displayName}
                      size={20}
                    />
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
                  {board.globalEntryCount != null && (
                    <p className="mt-1 text-xs text-muted-foreground">
                      {board.globalEntryCount.toLocaleString()} total entries
                    </p>
                  )}
                </TableCell>
                {members.map((member) => {
                  const standing = board.standings.find((s) => s.memberId === member.id);
                  return (
                    <TableCell
                      key={member.id}
                      className={`text-right font-mono text-sm ${leaderCellClass(standing)}`}
                    >
                      <StandingScoreCell
                        standing={standing}
                        compactEmptyScores={compactEmptyScores}
                      />
                      <div className="text-xs text-muted-foreground">
                        <FriendRank
                          rank={standing?.friendRank}
                          className="justify-end"
                          iconClassName="size-3"
                        />
                      </div>
                      {formatGlobalRank(standing?.globalRank, board.globalEntryCount) && (
                        <div className="text-xs text-muted-foreground">
                          {formatGlobalRank(standing?.globalRank, board.globalEntryCount)}
                        </div>
                      )}
                    </TableCell>
                  );
                })}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
      renderMobileCard={(board) => (
        <li key={board.id} className="rounded border border-border bg-card p-4">
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
          {board.globalEntryCount != null && (
            <p className="mt-1 text-xs text-muted-foreground">
              {board.globalEntryCount.toLocaleString()} ranked players on RA
            </p>
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
                    className="flex min-w-0 items-center gap-2 font-medium hover:text-[var(--accent-retro)]"
                  >
                    <MemberAvatar
                      avatarUrl={member.avatarUrl}
                      displayName={member.displayName}
                      size={24}
                    />
                    <span className="truncate">{member.displayName}</span>
                  </Link>
                  <div
                    className={`shrink-0 rounded px-2 py-1 text-right font-mono text-sm ${leaderCellClass(standing)}`}
                  >
                    <StandingScoreCell
                      standing={standing}
                      compactEmptyScores={compactEmptyScores}
                    />
                    <div className="text-xs text-muted-foreground">
                      <FriendRank
                        rank={standing?.friendRank}
                        className="justify-end"
                        iconClassName="size-3"
                      />
                    </div>
                    {formatGlobalRank(standing?.globalRank, board.globalEntryCount) && (
                      <div className="text-xs text-muted-foreground">
                        {formatGlobalRank(standing?.globalRank, board.globalEntryCount)}
                      </div>
                    )}
                  </div>
                </li>
              );
            })}
          </ul>
        </li>
      )}
    />
    </div>
  );
}
