"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type {
  GameOfTheWeekCurrentPollDto,
  GameOfTheWeekHistoryItemDto,
} from "@/generated/api-client";
import { GameOfTheWeekBallotCard } from "@/components/game-of-the-week/game-of-the-week-ballot-card";
import { GameOfTheWeekParticipation } from "@/components/game-of-the-week/game-of-the-week-participation";
import { GameOfTheWeekWinnerLink } from "@/components/game-of-the-week/game-of-the-week-winner-link";
import { Button } from "@/components/ui/button";
import { postAdminGameOfTheWeekClosePollAction } from "@/lib/actions/admin";
import { gameOfTheWeekImageUrl } from "@/lib/game-of-the-week-media";

const PHASE_SCHEDULED = 0;
const PHASE_OPEN = 1;
const PHASE_CLOSED = 2;
const TRACKING_PENDING = 1;

type Props = {
  poll?: GameOfTheWeekCurrentPollDto | null;
  lastWinner?: GameOfTheWeekHistoryItemDto | null;
  isAdmin?: boolean;
  currentMemberId?: string | null;
};

export function GameOfTheWeekDashboardShowcase({
  poll,
  lastWinner,
  isAdmin = false,
  currentMemberId,
}: Props) {
  const router = useRouter();
  const [closeError, setCloseError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  if (poll) {
    if (poll.phase === PHASE_OPEN || poll.phase === PHASE_SCHEDULED) {
      const label = poll.phase === PHASE_OPEN ? "Vote now" : "Opens soon";
      const opensAt = new Date(poll.startsAt);

      return (
        <section className="rounded border border-border bg-card p-5 space-y-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h2 className="text-lg font-semibold">Game of the week</h2>
              <p className="mt-1 text-sm text-muted-foreground">
                {poll.ballot.length} on the ballot · {label}
              </p>
              {poll.phase === PHASE_SCHEDULED ? (
                <p className="mt-1 text-xs text-muted-foreground">
                  Opens {opensAt.toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" })}
                </p>
              ) : null}
            </div>
            <Link
              href="/game-of-the-week"
              className="text-sm font-medium text-[var(--accent-retro)] hover:underline"
            >
              Full ballot →
            </Link>
          </div>
          {poll.phase === PHASE_OPEN ? (
            <>
              <GameOfTheWeekParticipation poll={poll} />
              {poll.allEligibleVotesCast ? (
                <p className="text-sm text-muted-foreground">
                  All votes in — waiting for admin to close the poll.
                </p>
              ) : null}
              {isAdmin ? (
                <div className="flex flex-wrap items-center gap-2">
                  <Button
                    type="button"
                    size="sm"
                    variant="outline"
                    disabled={pending}
                    onClick={() => {
                      if (!window.confirm("Close this poll and declare the winner?")) {
                        return;
                      }
                      setCloseError(null);
                      startTransition(async () => {
                        const result = await postAdminGameOfTheWeekClosePollAction();
                        if (!result.ok) {
                          setCloseError(result.error);
                          return;
                        }
                        router.refresh();
                      });
                    }}
                  >
                    Close poll
                  </Button>
                  <Link href="/admin/game-of-the-week" className="text-xs text-muted-foreground hover:underline">
                    Admin panel
                  </Link>
                </div>
              ) : null}
              {closeError ? <p className="text-sm text-destructive">{closeError}</p> : null}
            </>
          ) : null}
          <ul className="grid gap-3 sm:grid-cols-2">
            {poll.ballot.map((item) => (
              <li key={item.raGameId}>
                <GameOfTheWeekBallotCard
                  item={item}
                  mode="readOnly"
                  currentMemberId={currentMemberId}
                  compactVoters
                />
              </li>
            ))}
          </ul>
        </section>
      );
    }

    if (poll.phase === PHASE_CLOSED && poll.trackingStatus !== TRACKING_PENDING) {
      const winner = poll.ballot.find((b) => b.raGameId === poll.winnerRaGameId);
      const winnerTitle =
        winner?.title ?? (poll.winnerRaGameId != null ? `RA #${poll.winnerRaGameId}` : "Winner");
      const winnerImage = gameOfTheWeekImageUrl(winner?.imageIcon);

      return (
        <section className="rounded border border-border bg-card p-5">
          <h2 className="text-lg font-semibold">Game of the week</h2>
          <p className="mt-1 text-sm text-muted-foreground">Voting closed</p>
          {poll.winnerRaGameId != null ? (
            <div className="mt-3 flex gap-3">
              {winnerImage ? (
                <Image src={winnerImage} alt="" width={56} height={56} className="rounded" unoptimized />
              ) : null}
              <p className="text-sm">
                Winner:{" "}
                <GameOfTheWeekWinnerLink
                  raGameId={poll.winnerRaGameId}
                  title={winnerTitle}
                  isTracked={winner?.isTracked ?? false}
                />
              </p>
            </div>
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
      const winner = poll.ballot.find((b) => b.raGameId === poll.winnerRaGameId);
      const winnerTitle =
        winner?.title ?? (poll.winnerRaGameId != null ? `RA #${poll.winnerRaGameId}` : "Winner");

      return (
        <section className="rounded border border-border bg-card p-5">
          <h2 className="text-lg font-semibold">Game of the week</h2>
          <p className="mt-1 text-sm text-muted-foreground">Winner syncing leaderboards…</p>
          {poll.winnerRaGameId != null ? (
            <p className="mt-2 text-sm">
              <GameOfTheWeekWinnerLink
                raGameId={poll.winnerRaGameId}
                title={winnerTitle}
                isTracked={winner?.isTracked ?? false}
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
