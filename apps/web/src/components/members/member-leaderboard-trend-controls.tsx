"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import type { MemberStandingDto } from "@/generated/api-client";
import {
  listBoardsForGameFromStandings,
  listGamesFromStandings,
  memberLeaderboardsPath,
} from "@/lib/member-leaderboards-params";

type Props = {
  raUsername: string;
  standings: MemberStandingDto[];
  selectedGameId: number;
  selectedBoardId: number;
  standingsPage: number;
};

export function MemberLeaderboardTrendControls({
  raUsername,
  standings,
  selectedGameId,
  selectedBoardId,
  standingsPage,
}: Props) {
  const router = useRouter();
  const games = listGamesFromStandings(standings);
  const boards = listBoardsForGameFromStandings(standings, selectedGameId);
  const selectedBoard = boards.find((board) => board.raLeaderboardId === selectedBoardId);

  function navigate(next: { gameId: number; boardId: number }) {
    router.push(
      memberLeaderboardsPath(raUsername, {
        tab: "leaderboards",
        standingsPage,
        game: next.gameId,
        board: next.boardId,
        trendHistoryPage: 1,
      }),
    );
  }

  if (standings.length === 0) {
    return null;
  }

  return (
    <section className="space-y-4">
      <div className="space-y-1">
        <h2 className="steam-section-heading">Leaderboard trends</h2>
        <p className="text-xs text-muted-foreground">
          Charts for one board at a time—the same views as the standalone leaderboard page.
        </p>
      </div>
      <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end">
        <label className="flex min-w-0 flex-1 flex-col gap-1 sm:max-w-xs">
          <span className="text-xs font-medium text-muted-foreground">Game</span>
          <select
            value={selectedGameId}
            onChange={(event) => {
              const gameId = Number.parseInt(event.target.value, 10);
              const nextBoards = listBoardsForGameFromStandings(standings, gameId);
              const boardId = nextBoards[0]?.raLeaderboardId ?? selectedBoardId;
              navigate({ gameId, boardId });
            }}
            className="h-9 min-w-0 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
          >
            {games.map((game) => (
              <option key={game.raGameId} value={game.raGameId}>
                {game.gameTitle}
              </option>
            ))}
          </select>
        </label>
        <label className="flex min-w-0 flex-1 flex-col gap-1 sm:max-w-md">
          <span className="text-xs font-medium text-muted-foreground">Leaderboard</span>
          <select
            value={selectedBoardId}
            onChange={(event) => {
              navigate({
                gameId: selectedGameId,
                boardId: Number.parseInt(event.target.value, 10),
              });
            }}
            className="h-9 min-w-0 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
          >
            {boards.map((board) => (
              <option key={board.raLeaderboardId} value={board.raLeaderboardId}>
                {board.leaderboardTitle}
              </option>
            ))}
          </select>
        </label>
        {selectedBoard ? (
          <Link
            href={`/leaderboards/${selectedBoardId}`}
            className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]"
          >
            Open board page
          </Link>
        ) : null}
      </div>
    </section>
  );
}
