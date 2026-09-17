"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMemo, useState, useTransition } from "react";
import type {
  GameOfTheWeekCurrentPollDto,
  GameOfTheWeekHistoryItemDto,
} from "@/generated/api-client";
import { AdminGameOfTheWeekDiscordPreview } from "@/components/admin/admin-game-of-the-week-discord-preview";
import { GameOfTheWeekPastWeeks } from "@/components/game-of-the-week/game-of-the-week-past-weeks";
import { GameOfTheWeekWinnerLink } from "@/components/game-of-the-week/game-of-the-week-winner-link";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  postAdminGameOfTheWeekClosePollAction,
  postAdminGameOfTheWeekPollAction,
} from "@/lib/actions/admin";
import { GameOfTheWeekParticipation } from "@/components/game-of-the-week/game-of-the-week-participation";
import { GameOfTheWeekVoterList } from "@/components/game-of-the-week/game-of-the-week-voter-list";
import { parseRaGameIdInput } from "@/lib/dashboard-games";
import {
  formatGameOfTheWeekPhase,
  formatGameOfTheWeekTrackingStatus,
} from "@/lib/game-of-the-week-labels";

type Props = {
  currentPoll: GameOfTheWeekCurrentPollDto | null;
  history: GameOfTheWeekHistoryItemDto[];
};

export function AdminGameOfTheWeekSection({ currentPoll, history }: Props) {
  const router = useRouter();
  const [startsAt, setStartsAt] = useState("");
  const [endsAt, setEndsAt] = useState("");
  const [gameInputs, setGameInputs] = useState(["", ""]);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const parsedSeedIds = useMemo(() => {
    const ids: number[] = [];
    for (const raw of gameInputs) {
      const trimmed = raw.trim();
      if (!trimmed) continue;
      const parsed = parseRaGameIdInput(trimmed);
      if (parsed !== null) ids.push(parsed);
    }
    return [...new Set(ids)];
  }, [gameInputs]);

  const previewGameLabels = parsedSeedIds.map((id) => `RA #${id}`);

  const PHASE_OPEN = 1;

  return (
    <div className="space-y-10">
      {currentPoll ? (
        <section className="space-y-4 rounded border border-border p-5">
          <h2 className="text-lg font-semibold">Current poll</h2>
          <p className="text-sm text-muted-foreground">
            {formatGameOfTheWeekPhase(currentPoll.phase)} ·{" "}
            {new Date(currentPoll.startsAt).toLocaleString()} –{" "}
            {new Date(currentPoll.endsAt).toLocaleString()}
          </p>
          <p className="text-sm text-muted-foreground">
            Ballot {currentPoll.ballot.length}/5 · {formatGameOfTheWeekTrackingStatus(currentPoll.trackingStatus)}
          </p>
          {currentPoll.phase === PHASE_OPEN && currentPoll.closedAt == null ? (
            <>
              <GameOfTheWeekParticipation poll={currentPoll} />
              {currentPoll.allEligibleVotesCast ? (
                <p className="rounded border border-[var(--accent-retro)]/30 bg-[var(--accent-retro)]/5 px-3 py-2 text-sm">
                  All members have voted. You can close the poll to declare the winner.
                </p>
              ) : null}
              <div className="flex flex-wrap gap-2">
                <Button
                  type="button"
                  variant="outline"
                  disabled={pending}
                  onClick={() => {
                    if (!window.confirm("Close this poll and declare the winner? You can close early if not everyone has voted.")) {
                      return;
                    }
                    setError(null);
                    startTransition(async () => {
                      const result = await postAdminGameOfTheWeekClosePollAction();
                      if (!result.ok) {
                        setError(result.error);
                        return;
                      }
                      router.refresh();
                    });
                  }}
                >
                  Close poll & declare winner
                </Button>
              </div>
            </>
          ) : null}
          {currentPoll.winnerRaGameId != null ? (
            <p className="text-sm">
              Winner:{" "}
              <GameOfTheWeekWinnerLink
                raGameId={currentPoll.winnerRaGameId}
                title={
                  currentPoll.ballot.find((b) => b.raGameId === currentPoll.winnerRaGameId)?.title ??
                  `RA #${currentPoll.winnerRaGameId}`
                }
                isTracked={
                  currentPoll.ballot.find((b) => b.raGameId === currentPoll.winnerRaGameId)?.isTracked ?? false
                }
              />
            </p>
          ) : null}
          <div className="overflow-x-auto rounded border border-border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Game</TableHead>
                  <TableHead className="text-right">Votes</TableHead>
                  <TableHead>Voters</TableHead>
                  <TableHead>On site</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {currentPoll.ballot.map((item) => (
                  <TableRow key={item.raGameId}>
                    <TableCell>
                      <GameOfTheWeekWinnerLink
                        raGameId={item.raGameId}
                        title={item.title}
                        isTracked={item.isTracked}
                      />
                      {item.consoleName ? (
                        <p className="text-xs text-muted-foreground">{item.consoleName}</p>
                      ) : null}
                      {item.addedByDisplayName ? (
                        <p className="text-xs text-muted-foreground">Nominated by {item.addedByDisplayName}</p>
                      ) : null}
                    </TableCell>
                    <TableCell className="text-right">{item.voteCount}</TableCell>
                    <TableCell>
                      <GameOfTheWeekVoterList voters={item.voters} compact maxNames={5} />
                    </TableCell>
                    <TableCell>{item.isTracked ? "Yes" : "No"}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <Link
            href="/game-of-the-week"
            className="text-sm text-[var(--accent-retro)] hover:underline"
          >
            View public page →
          </Link>
        </section>
      ) : (
        <section className="space-y-4 rounded border border-border p-5">
          <div>
            <h2 className="text-lg font-semibold">Start a poll</h2>
            <p className="text-sm text-muted-foreground">
              Provide at least two games. Members can add more until five are on the ballot.
            </p>
          </div>
          <form
            className="space-y-4"
            onSubmit={(event) => {
              event.preventDefault();
              if (!startsAt || !endsAt) {
                setError("Set start and end date/time.");
                return;
              }
              if (parsedSeedIds.length < 2) {
                setError("Enter at least two valid RetroAchievements game ids.");
                return;
              }
              setError(null);
              startTransition(async () => {
                const result = await postAdminGameOfTheWeekPollAction(
                  new Date(startsAt).toISOString(),
                  new Date(endsAt).toISOString(),
                  parsedSeedIds,
                );
                if (!result.ok) {
                  setError(result.error);
                  return;
                }
                router.refresh();
              });
            }}
          >
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-muted-foreground">Starts</span>
                <input
                  type="datetime-local"
                  value={startsAt}
                  onChange={(e) => setStartsAt(e.target.value)}
                  className="h-9 rounded-md bg-input px-2.5"
                  required
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-muted-foreground">Ends</span>
                <input
                  type="datetime-local"
                  value={endsAt}
                  onChange={(e) => setEndsAt(e.target.value)}
                  className="h-9 rounded-md bg-input px-2.5"
                  required
                />
              </label>
            </div>
            <div className="space-y-2">
              <p className="text-sm text-muted-foreground">Seed games (2–5)</p>
              {gameInputs.map((value, index) => (
                <input
                  key={index}
                  type="text"
                  value={value}
                  placeholder={`Game ${index + 1} RA id or URL`}
                  onChange={(e) => {
                    const next = [...gameInputs];
                    next[index] = e.target.value;
                    setGameInputs(next);
                  }}
                  className="h-9 w-full rounded-md bg-input px-2.5 text-sm"
                />
              ))}
              {gameInputs.length < 5 ? (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setGameInputs([...gameInputs, ""])}
                >
                  Add another seed slot
                </Button>
              ) : null}
            </div>
            <AdminGameOfTheWeekDiscordPreview
              startsAt={startsAt}
              endsAt={endsAt}
              gameLabels={previewGameLabels}
            />
            {error ? <p className="text-sm text-destructive">{error}</p> : null}
            <Button type="submit" disabled={pending}>Start poll</Button>
          </form>
        </section>
      )}

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">Past weeks</h2>
        <GameOfTheWeekPastWeeks items={history} />
      </section>
    </div>
  );
}
