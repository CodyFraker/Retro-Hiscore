import Link from "next/link";
import { FriendRank } from "@/components/friend-rank";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { MemberAvatar } from "@/components/members/member-avatar";
import type { MemberSummaryDto, RivalryBoardDto, RivalryGameDto } from "@/generated/api-client";
import { formatGlobalRank } from "@/lib/format-global-rank";

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

function memberScoreCell(board: RivalryBoardDto, side: "A" | "B") {
  const score = side === "A" ? board.memberAScore : board.memberBScore;
  const formatted = side === "A" ? board.memberAFormattedScore : board.memberBFormattedScore;
  const friendRank = side === "A" ? board.memberAFriendRank : board.memberBFriendRank;
  const globalRank = side === "A" ? board.memberAGlobalRank : board.memberBGlobalRank;

  return (
    <>
      <div>{formatScore(score, formatted)}</div>
      <div className="text-xs text-muted-foreground">
        <FriendRank rank={friendRank} className="justify-end" />
      </div>
      <div className="text-xs text-muted-foreground">
        {formatGlobalRank(globalRank, board.globalEntryCount) ?? ""}
      </div>
    </>
  );
}

export function RivalryGameSection({ game, memberA, memberB }: Props) {
  return (
    <section className="space-y-3">
      <h2 className="text-lg font-medium">
        <Link href={`/games/${game.raGameId}`} className="hover:text-[var(--accent-retro)]">
          {game.gameTitle}
        </Link>
      </h2>

      <ResponsiveTable
        rows={game.boards}
        rowKey={(board) => String(board.raLeaderboardId)}
        desktopClassName="overflow-x-auto rounded border border-border"
        columns={[
          {
            header: "Board",
            render: (board) => (
              <Link
                href={`/leaderboards/${board.raLeaderboardId}`}
                className="hover:text-[var(--accent-retro)]"
              >
                {board.title}
              </Link>
            ),
          },
          {
            header: (
              <span className="inline-flex items-center justify-end gap-2">
                <MemberAvatar avatarUrl={memberA.avatarUrl} displayName={memberA.displayName} size={20} />
                {memberA.displayName}
              </span>
            ),
            headerClassName: "text-right",
            cellClassName: "text-right font-mono text-sm",
            render: (board) => memberScoreCell(board, "A"),
          },
          {
            header: (
              <span className="inline-flex items-center justify-end gap-2">
                <MemberAvatar avatarUrl={memberB.avatarUrl} displayName={memberB.displayName} size={20} />
                {memberB.displayName}
              </span>
            ),
            headerClassName: "text-right",
            cellClassName: "text-right font-mono text-sm",
            render: (board) => memberScoreCell(board, "B"),
          },
          {
            header: "Leader",
            headerClassName: "text-right",
            cellClassName: "text-right text-sm text-muted-foreground",
            render: (board) => board.leaderUsername ?? "Tied",
          },
        ]}
        renderMobileCard={(board) => (
          <li key={board.raLeaderboardId} className="rounded border border-border bg-card p-4 text-sm">
            <Link
              href={`/leaderboards/${board.raLeaderboardId}`}
              className="font-medium hover:text-[var(--accent-retro)]"
            >
              {board.title}
            </Link>
            <div className="mt-3 grid grid-cols-2 gap-3 border-t border-border pt-3">
              <div>
                <p className="flex items-center gap-2 text-xs text-muted-foreground">
                  <MemberAvatar
                    avatarUrl={memberA.avatarUrl}
                    displayName={memberA.displayName}
                    size={20}
                  />
                  {memberA.displayName}
                </p>
                <p className="font-mono text-lg">
                  {formatScore(board.memberAScore, board.memberAFormattedScore)}
                </p>
                <FriendRank rank={board.memberAFriendRank} iconClassName="size-3" />
                <p className="text-xs text-muted-foreground">
                  {formatGlobalRank(board.memberAGlobalRank, board.globalEntryCount)}
                </p>
              </div>
              <div className="text-right">
                <p className="flex items-center justify-end gap-2 text-xs text-muted-foreground">
                  <MemberAvatar
                    avatarUrl={memberB.avatarUrl}
                    displayName={memberB.displayName}
                    size={20}
                  />
                  {memberB.displayName}
                </p>
                <p className="font-mono text-lg">
                  {formatScore(board.memberBScore, board.memberBFormattedScore)}
                </p>
                <FriendRank
                  rank={board.memberBFriendRank}
                  className="justify-end"
                  iconClassName="size-3"
                />
                <p className="text-xs text-muted-foreground">
                  {formatGlobalRank(board.memberBGlobalRank, board.globalEntryCount)}
                </p>
              </div>
            </div>
            <p className="mt-2 text-xs text-muted-foreground">
              Leader: <span className="text-foreground">{board.leaderUsername ?? "Tied"}</span>
            </p>
          </li>
        )}
      />
    </section>
  );
}
