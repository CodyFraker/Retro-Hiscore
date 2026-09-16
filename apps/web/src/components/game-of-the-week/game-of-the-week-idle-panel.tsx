import Link from "next/link";
import type { GameOfTheWeekHistoryItemDto } from "@/generated/api-client";
import { GameOfTheWeekPastWeeks } from "@/components/game-of-the-week/game-of-the-week-past-weeks";
import { GameOfTheWeekWinnerLink } from "@/components/game-of-the-week/game-of-the-week-winner-link";

type Props = {
  history: GameOfTheWeekHistoryItemDto[];
};

export function GameOfTheWeekIdlePanel({ history }: Props) {
  const lastWinner = history[0];

  return (
    <div className="space-y-10">
      <section className="rounded border border-border bg-card p-6">
        <h2 className="text-lg font-semibold">No vote open right now</h2>
        <p className="mt-2 text-sm text-muted-foreground">
          When an admin starts the next poll, you can nominate games and vote here. Check back soon or watch the
          home page for announcements.
        </p>
        {lastWinner && lastWinner.winnerRaGameId != null ? (
          <div className="mt-6 rounded border border-border bg-muted/30 p-4">
            <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Last winner</p>
            <p className="mt-2 text-base">
              <GameOfTheWeekWinnerLink
                raGameId={lastWinner.winnerRaGameId}
                title={lastWinner.winnerTitle ?? `RA #${lastWinner.winnerRaGameId}`}
                isTracked={lastWinner.isTracked}
                className="text-lg font-semibold hover:text-[var(--accent-retro)]"
              />
            </p>
            {lastWinner.closedAt ? (
              <p className="mt-1 text-xs text-muted-foreground">
                Closed {new Date(lastWinner.closedAt).toLocaleDateString()}
              </p>
            ) : null}
          </div>
        ) : null}
        <Link href="/" className="mt-4 inline-block text-sm text-[var(--accent-retro)] hover:underline">
          Back to home
        </Link>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">How it works</h2>
        <ol className="list-decimal space-y-2 pl-5 text-sm text-muted-foreground">
          <li>An admin seeds two to five games and sets when voting opens and closes.</li>
          <li>Members can add games to the ballot until five slots are filled.</li>
          <li>Everyone gets one vote; you can change it until voting closes.</li>
          <li>The winner is added to the tracked games list for friend leaderboards.</li>
        </ol>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">Past weeks</h2>
        <GameOfTheWeekPastWeeks items={history} />
      </section>
    </div>
  );
}
