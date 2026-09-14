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
import type { MemberSummaryDto, RivalryBoardDto, RivalryGameDto } from "@/generated/api-client";

type Props = {
  game: RivalryGameDto;
  memberA: MemberSummaryDto;
  memberB: MemberSummaryDto;
};

function formatScore(score: number | null | undefined, formatted?: string | null) {
  if (formatted) {
    return formatted;
  }

  if (score == null) {
    return "—";
  }

  return score.toLocaleString();
}

export function RivalryGameSection({ game, memberA, memberB }: Props) {
  return (
    <section className="space-y-3">
      <h2 className="text-lg font-medium">
        <Link href={`/games/${game.raGameId}`} className="hover:text-[var(--accent-retro)]">
          {game.gameTitle}
        </Link>
      </h2>

      <div className="hidden overflow-x-auto rounded border border-border md:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Board</TableHead>
              <TableHead className="text-right">
                <span className="inline-flex items-center justify-end gap-2">
                  <MemberAvatar avatarUrl={memberA.avatarUrl} displayName={memberA.displayName} size={20} />
                  {memberA.displayName}
                </span>
              </TableHead>
              <TableHead className="text-right">
                <span className="inline-flex items-center justify-end gap-2">
                  <MemberAvatar avatarUrl={memberB.avatarUrl} displayName={memberB.displayName} size={20} />
                  {memberB.displayName}
                </span>
              </TableHead>
              <TableHead className="text-right">Leader</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {game.boards.map((board) => (
              <RivalryBoardTableRow
                key={board.raLeaderboardId}
                board={board}
              />
            ))}
          </TableBody>
        </Table>
      </div>

      <ul className="space-y-3 md:hidden">
        {game.boards.map((board) => (
          <RivalryBoardCard
            key={board.raLeaderboardId}
            board={board}
            memberA={memberA}
            memberB={memberB}
          />
        ))}
      </ul>
    </section>
  );
}

function RivalryBoardTableRow({ board }: { board: RivalryBoardDto }) {
  return (
    <TableRow>
      <TableCell>
        <Link
          href={`/leaderboards/${board.raLeaderboardId}`}
          className="hover:text-[var(--accent-retro)]"
        >
          {board.title}
        </Link>
      </TableCell>
      <TableCell className="text-right font-mono text-sm">
        <div>{formatScore(board.memberAScore, board.memberAFormattedScore)}</div>
        <div className="text-xs text-muted-foreground">
          <FriendRank rank={board.memberAFriendRank} className="justify-end" />
        </div>
      </TableCell>
      <TableCell className="text-right font-mono text-sm">
        <div>{formatScore(board.memberBScore, board.memberBFormattedScore)}</div>
        <div className="text-xs text-muted-foreground">
          <FriendRank rank={board.memberBFriendRank} className="justify-end" />
        </div>
      </TableCell>
      <TableCell className="text-right text-sm text-muted-foreground">
        {board.leaderUsername ?? "Tied"}
      </TableCell>
    </TableRow>
  );
}

function RivalryBoardCard({
  board,
  memberA,
  memberB,
}: {
  board: RivalryBoardDto;
  memberA: MemberSummaryDto;
  memberB: MemberSummaryDto;
}) {
  return (
    <li className="rounded border border-border bg-card p-4 text-sm">
      <Link
        href={`/leaderboards/${board.raLeaderboardId}`}
        className="font-medium hover:text-[var(--accent-retro)]"
      >
        {board.title}
      </Link>
      <div className="mt-3 grid grid-cols-2 gap-3 border-t border-border pt-3">
        <div>
          <p className="flex items-center gap-2 text-xs text-muted-foreground">
            <MemberAvatar avatarUrl={memberA.avatarUrl} displayName={memberA.displayName} size={20} />
            {memberA.displayName}
          </p>
          <p className="font-mono text-lg">
            {formatScore(board.memberAScore, board.memberAFormattedScore)}
          </p>
          <FriendRank rank={board.memberAFriendRank} iconClassName="size-3" />
        </div>
        <div className="text-right">
          <p className="flex items-center justify-end gap-2 text-xs text-muted-foreground">
            <MemberAvatar avatarUrl={memberB.avatarUrl} displayName={memberB.displayName} size={20} />
            {memberB.displayName}
          </p>
          <p className="font-mono text-lg">
            {formatScore(board.memberBScore, board.memberBFormattedScore)}
          </p>
          <FriendRank rank={board.memberBFriendRank} className="justify-end" iconClassName="size-3" />
        </div>
      </div>
      <p className="mt-2 text-xs text-muted-foreground">
        Leader: <span className="text-foreground">{board.leaderUsername ?? "Tied"}</span>
      </p>
    </li>
  );
}
