"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { GameOfTheWeekCurrentPollDto } from "@/generated/api-client";
import { Button } from "@/components/ui/button";
import { postAdminGameOfTheWeekPollAction } from "@/lib/actions/admin";
import { parseRaGameIdInput } from "@/lib/dashboard-games";

type Props = {
  currentPoll: GameOfTheWeekCurrentPollDto | null;
};

export function AdminGameOfTheWeekSection({ currentPoll }: Props) {
  const router = useRouter();
  const [startsAt, setStartsAt] = useState("");
  const [endsAt, setEndsAt] = useState("");
  const [gameInputs, setGameInputs] = useState(["", ""]);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  if (currentPoll) {
    return (
      <section className="space-y-3 rounded border border-border p-5">
        <h2 className="text-lg font-semibold">Current poll</h2>
        <p className="text-sm text-muted-foreground">
          Phase {currentPoll.phase} · {new Date(currentPoll.startsAt).toLocaleString()} –{" "}
          {new Date(currentPoll.endsAt).toLocaleString()}
        </p>
        <p className="text-sm">
          Ballot: {currentPoll.ballot.length}/5 · Tracking status {currentPoll.trackingStatus}
        </p>
        {currentPoll.winnerRaGameId != null ? (
          <p className="text-sm">Winner RA id: {currentPoll.winnerRaGameId}</p>
        ) : null}
      </section>
    );
  }

  return (
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
          const raGameIds: number[] = [];
          for (const raw of gameInputs) {
            const trimmed = raw.trim();
            if (!trimmed) {
              continue;
            }
            const parsed = parseRaGameIdInput(trimmed);
            if (parsed === null) {
              setError(`Invalid game id: ${trimmed}`);
              return;
            }
            raGameIds.push(parsed);
          }
          const unique = [...new Set(raGameIds)];
          if (unique.length < 2) {
            setError("Enter at least two valid RetroAchievements game ids.");
            return;
          }
          setError(null);
          startTransition(async () => {
            const result = await postAdminGameOfTheWeekPollAction(
              new Date(startsAt).toISOString(),
              new Date(endsAt).toISOString(),
              unique,
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
        {error ? <p className="text-sm text-destructive">{error}</p> : null}
        <Button type="submit" disabled={pending}>Start poll</Button>
      </form>
    </section>
  );
}
