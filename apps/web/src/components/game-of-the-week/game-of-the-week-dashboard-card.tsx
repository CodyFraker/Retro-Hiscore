import Link from "next/link";
import type {
  GameOfTheWeekCurrentPollDto,
  GameOfTheWeekHistoryItemDto,
} from "@/generated/api-client";
import { GameOfTheWeekWinnerLink } from "@/components/game-of-the-week/game-of-the-week-winner-link";

const PHASE_SCHEDULED = 0;
const PHASE_OPEN = 1;
const PHASE_CLOSED = 2;
const TRACKING_PENDING = 1;

type Props = {
  poll?: GameOfTheWeekCurrentPollDto | null;
  lastWinner?: GameOfTheWeekHistoryItemDto | null;
};

export function GameOfTheWeekDashboardCard({ poll, lastWinner }: Props) {
  if (poll) {
    if (poll.phase === PHASE_OPEN || poll.phase === PHASE_SCHEDULED) {
      const label = poll.phase === PHASE_OPEN ? "Vote now" : "Opens soon";
      const opensAt = new Date(poll.startsAt);
      const countdown =
        poll.phase === PHASE_SCHEDULED
          ? `Opens ${opensAt.toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" })}`
          : null;

      return (
        <section className="rounded border border-border bg-card p-5">
          <h2 className="text-lg font-semibold">Game of the week</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            {poll.ballot.length} on the ballot · {label}
          </p>
          {countdown ? <p className="mt-1 text-xs text-muted-foreground">{countdown}</p> : null}
          <Link
            href="/game-of-the-week"
            className="mt-3 inline-block text-sm font-medium text-[var(--accent-retro)] hover:underline"
          >
            Go to voting →
          </Link>
        </section>
      );
    }

    if (poll.phase === PHASE_CLOSED && poll.trackingStatus !== TRACKING_PENDING) {
      const winnerTitle =
        poll.ballot.find((b) => b.raGameId === poll.winnerRaGameId)?.title ??
        (poll.winnerRaGameId != null ? `RA #${poll.winnerRaGameId}` : "Winner");
      const winnerTracked =
        poll.winnerRaGameId != null
          ? (poll.ballot.find((b) => b.raGameId === poll.winnerRaGameId)?.isTracked ?? false)
          : false;

      return (
        <section className="rounded border border-border bg-card p-5">
          <h2 className="text-lg font-semibold">Game of the week</h2>
          <p className="mt-1 text-sm text-muted-foreground">Voting closed</p>
          {poll.winnerRaGameId != null ? (
            <p className="mt-2 text-sm">
              Winner:{" "}
              <GameOfTheWeekWinnerLink
                raGameId={poll.winnerRaGameId}
                title={winnerTitle}
                isTracked={winnerTracked}
              />
            </p>
          ) : null}
          <Link
            href="/game-of-the-week"
            className="mt-3 inline-block text-sm font-medium text-[var(--accent-retro)] hover:underline"
          >
            View results →
          </Link>
        </section>
      );
    }

    if (poll.phase === PHASE_CLOSED && poll.trackingStatus === TRACKING_PENDING) {
      const winnerTitle =
        poll.ballot.find((b) => b.raGameId === poll.winnerRaGameId)?.title ??
        (poll.winnerRaGameId != null ? `RA #${poll.winnerRaGameId}` : "Winner");
      const winnerTracked =
        poll.winnerRaGameId != null
          ? (poll.ballot.find((b) => b.raGameId === poll.winnerRaGameId)?.isTracked ?? false)
          : false;

      return (
        <section className="rounded border border-border bg-card p-5">
          <h2 className="text-lg font-semibold">Game of the week</h2>
          <p className="mt-1 text-sm text-muted-foreground">Winner syncing leaderboards…</p>
          {poll.winnerRaGameId != null ? (
            <p className="mt-2 text-sm">
              <GameOfTheWeekWinnerLink
                raGameId={poll.winnerRaGameId}
                title={winnerTitle}
                isTracked={winnerTracked}
              />
            </p>
          ) : null}
          <Link
            href="/game-of-the-week"
            className="mt-3 inline-block text-sm font-medium text-[var(--accent-retro)] hover:underline"
          >
            View results →
          </Link>
        </section>
      );
    }
  }

  return (
    <section className="rounded border border-border bg-card p-5">
      <h2 className="text-lg font-semibold">Game of the week</h2>
      <p className="mt-1 text-sm text-muted-foreground">No vote open</p>
      {lastWinner && lastWinner.winnerRaGameId != null ? (
        <p className="mt-2 text-sm">
          Last winner:{" "}
          <GameOfTheWeekWinnerLink
            raGameId={lastWinner.winnerRaGameId}
            title={lastWinner.winnerTitle ?? `RA #${lastWinner.winnerRaGameId}`}
            isTracked={lastWinner.isTracked}
          />
        </p>
      ) : null}
      <Link
        href="/game-of-the-week"
        className="mt-3 inline-block text-sm font-medium text-[var(--accent-retro)] hover:underline"
      >
        Past weeks & how it works →
      </Link>
    </section>
  );
}
