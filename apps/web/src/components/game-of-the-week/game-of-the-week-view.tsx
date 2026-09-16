"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { GameOfTheWeekCurrentPollDto } from "@/generated/api-client";
import { Button } from "@/components/ui/button";
import { parseRaGameIdInput } from "@/lib/dashboard-games";
import {
  postGameOfTheWeekBallotAction,
  putGameOfTheWeekVoteAction,
} from "@/lib/actions/game-of-the-week";

const PHASE_SCHEDULED = 0;
const PHASE_OPEN = 1;

type Props = {
  initialPoll: GameOfTheWeekCurrentPollDto;
};

export function GameOfTheWeekView({ initialPoll }: Props) {
  const router = useRouter();
  const [poll, setPoll] = useState(initialPoll);
  const [ballotInput, setBallotInput] = useState("");
  const [selectedVote, setSelectedVote] = useState<number | null>(poll.myVoteRaGameId ?? null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const phaseLabel =
    poll.phase === PHASE_SCHEDULED
      ? "Scheduled"
      : poll.phase === PHASE_OPEN
        ? "Voting open"
        : "Closed";

  return (
    <div className="space-y-8">
      <header className="space-y-2">
        <p className="text-sm font-medium text-[var(--accent-retro)]">{phaseLabel}</p>
        <p className="text-sm text-muted-foreground">
          {new Date(poll.startsAt).toLocaleString()} – {new Date(poll.endsAt).toLocaleString()}
        </p>
        {poll.winnerRaGameId != null && poll.phase !== PHASE_OPEN && poll.phase !== PHASE_SCHEDULED ? (
          <p className="text-sm">
            Winner:{" "}
            <span className="font-medium">
              {poll.ballot.find((b) => b.raGameId === poll.winnerRaGameId)?.title ??
                `RA #${poll.winnerRaGameId}`}
            </span>
            {poll.trackingStatus === 1 ? (
              <span className="text-muted-foreground"> (syncing leaderboards…)</span>
            ) : null}
          </p>
        ) : null}
      </header>

      <section className="space-y-4">
        <h2 className="text-lg font-semibold">Ballot</h2>
        <ul className="grid gap-4 sm:grid-cols-2">
          {poll.ballot.map((item) => (
            <li key={item.raGameId} className="rounded border border-border p-4">
              <div className="flex gap-3">
                {item.imageIcon ? (
                  <Image
                    src={item.imageIcon.startsWith("http") ? item.imageIcon : `https://media.retroachievements.org${item.imageIcon}`}
                    alt=""
                    width={48}
                    height={48}
                    className="rounded"
                    unoptimized
                  />
                ) : null}
                <div className="min-w-0 flex-1">
                  <BallotTitle raGameId={item.raGameId} title={item.title} isTracked={item.isTracked} />
                  <p className="text-xs text-muted-foreground">{item.consoleName ?? "Unknown platform"}</p>
                  <p className="mt-1 text-xs text-muted-foreground">
                    {item.voteCount} vote{item.voteCount === 1 ? "" : "s"}
                    {item.isTracked ? " · On site" : " · Not tracked yet"}
                  </p>
                </div>
              </div>
              {poll.phase === PHASE_OPEN ? (
                <label className="mt-3 flex cursor-pointer items-center gap-2 text-sm">
                  <input
                    type="radio"
                    name="gotw-vote"
                    checked={selectedVote === item.raGameId}
                    disabled={pending}
                    onChange={() => setSelectedVote(item.raGameId)}
                  />
                  My vote
                </label>
              ) : null}
            </li>
          ))}
        </ul>
        {poll.phase === PHASE_OPEN ? (
          <Button
            type="button"
            disabled={pending || selectedVote == null}
            onClick={() => {
              if (selectedVote == null) {
                return;
              }
              setError(null);
              startTransition(async () => {
                const result = await putGameOfTheWeekVoteAction(selectedVote);
                if (!result.ok) {
                  setError(result.error);
                  return;
                }
                setPoll(result.poll);
                setMessage("Vote saved.");
              });
            }}
          >
            Save vote
          </Button>
        ) : null}
      </section>

      {poll.phase === PHASE_OPEN && poll.ballotSlotsRemaining > 0 ? (
        <section className="space-y-3 rounded border border-border p-5">
          <h2 className="text-lg font-semibold">Add a game</h2>
          <p className="text-sm text-muted-foreground">
            {poll.ballotSlotsRemaining} slot{poll.ballotSlotsRemaining === 1 ? "" : "s"} left on the ballot.
          </p>
          <form
            className="flex flex-col gap-2 sm:flex-row sm:items-end"
            onSubmit={(event) => {
              event.preventDefault();
              const parsed = parseRaGameIdInput(ballotInput);
              if (parsed === null) {
                setError("Enter a valid RetroAchievements game id or URL.");
                return;
              }
              setError(null);
              startTransition(async () => {
                const result = await postGameOfTheWeekBallotAction(parsed);
                if (!result.ok) {
                  setError(result.error);
                  return;
                }
                setPoll(result.poll);
                setBallotInput("");
                setMessage("Game added to the ballot.");
                router.refresh();
              });
            }}
          >
            <input
              type="text"
              value={ballotInput}
              disabled={pending}
              onChange={(e) => setBallotInput(e.target.value)}
              placeholder="RA game id or URL"
              className="h-9 min-w-0 flex-1 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
            />
            <Button type="submit" disabled={pending || !ballotInput.trim()}>Add to ballot</Button>
          </form>
        </section>
      ) : null}

      {error ? <p className="text-sm text-destructive">{error}</p> : null}
      {message ? <p className="text-sm text-muted-foreground">{message}</p> : null}
    </div>
  );
}

function BallotTitle({
  raGameId,
  title,
  isTracked,
}: {
  raGameId: number;
  title: string;
  isTracked: boolean;
}) {
  if (isTracked) {
    return (
      <Link href={`/games/${raGameId}`} className="font-medium hover:text-[var(--accent-retro)]">
        {title}
      </Link>
    );
  }

  return (
    <a
      href={`https://retroachievements.org/game/${raGameId}`}
      target="_blank"
      rel="noopener noreferrer"
      className="font-medium hover:text-[var(--accent-retro)]"
    >
      {title}
    </a>
  );
}
