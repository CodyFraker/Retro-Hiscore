"use client";

import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { Button } from "@/components/ui/button";
import { saveMemberRaAccountAction } from "@/lib/actions/settings";

export function RaAccountForm() {
  const router = useRouter();
  const { update } = useSession();
  const [raUsername, setRaUsername] = useState("");
  const [apiKey, setApiKey] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  return (
    <form
      className="space-y-4"
      onSubmit={(event) => {
        event.preventDefault();

        startTransition(async () => {
          setError(null);
          const result = await saveMemberRaAccountAction(raUsername, apiKey);
          if (!result.ok) {
            setError(result.error);
            return;
          }
          await update({ needsOnboarding: false });
          setApiKey("");
          router.refresh();
        });
      }}
    >
      <div className="space-y-2">
        <label htmlFor="ra-username" className="text-sm font-medium">
          RetroAchievements username
        </label>
        <input
          id="ra-username"
          type="text"
          autoComplete="username"
          value={raUsername}
          onChange={(event) => setRaUsername(event.target.value)}
          placeholder="Your RA username"
          className="w-full rounded-md bg-input px-3 py-2 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
        />
      </div>
      <div className="space-y-2">
        <label htmlFor="ra-api-key-onboard" className="text-sm font-medium">
          RetroAchievements API key
        </label>
        <input
          id="ra-api-key-onboard"
          type="password"
          autoComplete="off"
          value={apiKey}
          onChange={(event) => setApiKey(event.target.value)}
          placeholder="Paste your API key from RetroAchievements"
          className="w-full rounded-md bg-input px-3 py-2 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
        />
      </div>
      {error && (
        <p className="rounded border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm">{error}</p>
      )}
      <Button type="submit" disabled={pending}>
        {pending ? "Saving…" : "Link RetroAchievements account"}
      </Button>
    </form>
  );
}
