import type { GameOfTheWeekHistoryItemDto } from "@/generated/api-client";
import { GameOfTheWeekWinnerLink } from "@/components/game-of-the-week/game-of-the-week-winner-link";

type Props = {
  items: GameOfTheWeekHistoryItemDto[];
};

export function GameOfTheWeekPastWeeks({ items }: Props) {
  if (items.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">No completed polls yet. The first winner will show up here.</p>
    );
  }

  return (
    <ul className="divide-y divide-border rounded border border-border">
      {items.map((item) => {
        const winnerTitle =
          item.winnerTitle ?? (item.winnerRaGameId != null ? `RA #${item.winnerRaGameId}` : "No winner");
        const weekLabel = item.closedAt
          ? new Date(item.closedAt).toLocaleDateString(undefined, {
              month: "short",
              day: "numeric",
              year: "numeric",
            })
          : new Date(item.endsAt).toLocaleDateString();

        return (
          <li key={item.pollId} className="flex flex-wrap items-baseline justify-between gap-2 px-4 py-3 text-sm">
            <div>
              <span className="text-muted-foreground">{weekLabel}</span>
              <span className="mx-2 text-muted-foreground">·</span>
              {item.winnerRaGameId != null ? (
                <GameOfTheWeekWinnerLink
                  raGameId={item.winnerRaGameId}
                  title={winnerTitle}
                  isTracked={item.isTracked}
                />
              ) : (
                <span>{winnerTitle}</span>
              )}
              {item.winnerConsoleName ? (
                <span className="ml-2 text-xs text-muted-foreground">{item.winnerConsoleName}</span>
              ) : null}
            </div>
            <span className="text-xs text-muted-foreground">
              {item.totalVotes} vote{item.totalVotes === 1 ? "" : "s"}
            </span>
          </li>
        );
      })}
    </ul>
  );
}
