"use client";

import { Plus } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { Button } from "@/components/ui/button";
import { useApiClient } from "@/lib/use-api-client";
import { parseRaGameIdInput } from "@/lib/dashboard-games";

function formatAddGameError(error: unknown): string {
  if (!(error instanceof Error)) {
    return "Failed to add game";
  }

  const match = error.message.match(/^API (\d+):\s*(.*)$/s);
  if (!match) {
    return error.message;
  }

  const status = Number(match[1]);
  const body = match[2];
  const messageMatch = body.match(/"message"\s*:\s*"([^"]+)"/);
  if (messageMatch?.[1]) {
    return messageMatch[1];
  }

  if (status === 409) {
    return "That game is already tracked.";
  }
  if (status === 404) {
    return "Game not found on RetroAchievements.";
  }
  return error.message;
}

export function AddGameForm() {
  const api = useApiClient();
  const router = useRouter();
  const [input, setInput] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  return (
    <form
      className="flex flex-col gap-2 sm:flex-row sm:items-end sm:gap-3"
      onSubmit={(event) => {
        event.preventDefault();
        const parsed = parseRaGameIdInput(input);
        if (parsed === null) {
          setError("Enter a valid RetroAchievements game id or URL.");
          setMessage(null);
          return;
        }

        startTransition(async () => {
          setError(null);
          setMessage(null);
          try {
            const game = await api.addGame(parsed);
            setInput("");
            setMessage(`Added ${game.title}`);
            router.refresh();
            document.getElementById("tracked-games")?.scrollIntoView({ behavior: "smooth" });
          } catch (err) {
            setError(formatAddGameError(err));
          }
        });
      }}
    >
      <label className="flex min-w-0 flex-1 flex-col gap-1.5">
        <span className="text-xs font-medium text-muted-foreground">
          RetroAchievements game id or URL
        </span>
        <input
          type="text"
          inputMode="text"
          value={input}
          disabled={pending}
          onChange={(event) => setInput(event.target.value)}
          placeholder="e.g. 38130 or retroachievements.org/game/38130"
          className="h-8 w-full rounded-lg border border-input bg-background px-2.5 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:opacity-50"
        />
        <span className="text-xs text-muted-foreground">
          Find games on{" "}
          <a
            href="https://retroachievements.org/games"
            target="_blank"
            rel="noopener noreferrer"
            className="text-foreground hover:text-[var(--accent-retro)]"
          >
            RetroAchievements
          </a>
        </span>
      </label>
      <div className="flex flex-col items-stretch gap-1 sm:items-end">
        <Button type="submit" disabled={pending || !input.trim()}>
          <Plus />
          {pending ? "Adding…" : "Add game"}
        </Button>
        {error && <p className="text-xs text-destructive">{error}</p>}
        {!error && message && <p className="text-xs text-muted-foreground">{message}</p>}
      </div>
    </form>
  );
}
