"use client";

import { Plus } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { Button } from "@/components/ui/button";
import { postAdminGameAction } from "@/lib/actions/admin";
import { parseRaGameIdInput } from "@/lib/dashboard-games";

export function AddGameForm() {
  const router = useRouter();
  const [input, setInput] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  return (
    <form
      className="flex flex-col gap-2"
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
          const result = await postAdminGameAction(parsed);
          if (!result.ok) {
            setError(result.error);
            return;
          }
          setInput("");
          setMessage(`Added ${result.game.title}`);
          router.push(`/admin/games/${result.game.raGameId}`);
          router.refresh();
        });
      }}
    >
      <label htmlFor="add-game-input" className="text-xs font-medium text-muted-foreground">
        RetroAchievements game id or URL
      </label>
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:gap-3">
        <input
          id="add-game-input"
          type="text"
          inputMode="text"
          value={input}
          disabled={pending}
          onChange={(event) => setInput(event.target.value)}
          placeholder="e.g. 38130 or retroachievements.org/game/38130"
          className="h-8 min-w-0 flex-1 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50 disabled:opacity-50"
        />
        <Button type="submit" className="w-full shrink-0 sm:w-auto" disabled={pending || !input.trim()}>
          <Plus />
          {pending ? "Adding…" : "Add game"}
        </Button>
      </div>
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
      {error && <p className="text-xs text-destructive">{error}</p>}
      {!error && message && <p className="text-xs text-muted-foreground">{message}</p>}
    </form>
  );
}
