"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type {
  GameOfTheWeekCurrentPollDto,
  GameOfTheWeekHistoryItemDto,
} from "@/generated/api-client";
import { GameOfTheWeekBallotCard } from "@/components/game-of-the-week/game-of-the-week-ballot-card";
import { GameOfTheWeekParticipation } from "@/components/game-of-the-week/game-of-the-week-participation";
import { GameOfTheWeekPastWeeks } from "@/components/game-of-the-week/game-of-the-week-past-weeks";
import { Button } from "@/components/ui/button";
import { parseRaGameIdInput } from "@/lib/dashboard-games";
import {
  postGameOfTheWeekBallotAction,
  putGameOfTheWeekVoteAction,
} from "@/lib/actions/game-of-the-week";

const PHASE_SCHEDULED = 0;
const PHASE_OPEN = 1;
const PHASE_CLOSED = 2;
const TRACKING_PENDING = 1;

type Props = {
  initialPoll: GameOfTheWeekCurrentPollDto;
  history?: GameOfTheWeekHistoryItemDto[];
  currentMemberId?: string | null;
};

export function GameOfTheWeekView({ initialPoll, history = [], currentMemberId }: Props) {
  const router = useRouter();
  const [poll, setPoll] = useState(initialPoll);
  const [ballotInput, setBallotInput] = useState("");
  const [selectedVote, setSelectedVote] = useState<number | null>(poll.myVoteRaGameId ?? null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const awaitingFinalization =
    poll.phase === PHASE_CLOSED && poll.closedAt == null && poll.winnerRaGameId == null;

  const phaseLabel =
    awaitingFinalization
      ? "Waiting for results"
      : poll.phase === PHASE_SCHEDULED
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
        {poll.phase === PHASE_SCHEDULED ? (
          <p className="text-sm text-muted-foreground">
            Voting opens {new Date(poll.startsAt).toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" })}
          </p>
        ) : null}
        {poll.phase === PHASE_OPEN ? <GameOfTheWeekParticipation poll={poll} /> : null}
        {poll.allEligibleVotesCast && poll.phase === PHASE_OPEN ? (
          <p className="text-sm text-muted-foreground">
            All members have voted. An admin can close the poll to declare the winner.
          </p>
        ) : null}
        {poll.winnerRaGameId != null && poll.closedAt != null ? (
          <p className="text-sm">
            Winner:{" "}
            <span className="font-medium">
              {poll.ballot.find((b) => b.raGameId === poll.winnerRaGameId)?.title ??
                `RA #${poll.winnerRaGameId}`}
            </span>
            {poll.trackingStatus === TRACKING_PENDING ? (
              <span className="text-muted-foreground"> (syncing leaderboards…)</span>
            ) : null}
          </p>
        ) : null}
        {awaitingFinalization ? (
          <p className="text-sm text-muted-foreground">
            Voting time has ended. Results will appear after the poll is finalized.
          </p>
        ) : null}
      </header>

      <section className="space-y-4">
        <h2 className="text-lg font-semibold">Ballot</h2>
        <ul className="grid gap-4 sm:grid-cols-2">
          {poll.ballot.map((item) => (
            <li key={item.raGameId}>
              <GameOfTheWeekBallotCard
                item={item}
                mode={poll.phase === PHASE_OPEN ? "voting" : "readOnly"}
                selectedVote={selectedVote}
                onSelectVote={setSelectedVote}
                votePending={pending}
                currentMemberId={currentMemberId}
              />
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

      {history.length > 0 ? (
        <section className="space-y-3 border-t border-border pt-8">
          <h2 className="text-lg font-semibold">Past weeks</h2>
          <GameOfTheWeekPastWeeks items={history} />
        </section>
      ) : null}
    </div>
  );
}
