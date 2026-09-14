"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { Button } from "@/components/ui/button";
import { saveMemberApiKeyAction } from "@/lib/actions/settings";

type Props = {
  hasApiKey: boolean;
  raUsername: string;
};

export function ApiKeyForm({ hasApiKey, raUsername }: Props) {
  const router = useRouter();
  const [apiKey, setApiKey] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  return (
    <form
      className="space-y-4"
      onSubmit={(event) => {
        event.preventDefault();

        startTransition(async () => {
          setError(null);
          setMessage(null);
          const result = await saveMemberApiKeyAction(apiKey);
          if (!result.ok) {
            setError(result.error);
            return;
          }
          setApiKey("");
          setMessage(`API key saved for ${raUsername}. Score sync will use your key.`);
          router.refresh();
        });
      }}
    >
      <div className="space-y-2">
        <label htmlFor="ra-api-key" className="text-sm font-medium">
          RetroAchievements API key
        </label>
        <input
          id="ra-api-key"
          type="password"
          autoComplete="off"
          value={apiKey}
          onChange={(event) => setApiKey(event.target.value)}
          placeholder={hasApiKey ? "Enter a new key to replace the saved one" : "Paste your API key from RetroAchievements"}
          className="w-full rounded-md bg-input px-3 py-2 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
        />
        <p className="text-xs text-muted-foreground">
          Copy your key from RetroAchievements profile settings. Your scores sync with your own key.
        </p>
      </div>
      {hasApiKey && (
        <p className="text-sm text-muted-foreground">A key is already saved for this account.</p>
      )}
      {message && (
        <p className="rounded border border-border bg-secondary/40 px-3 py-2 text-sm">{message}</p>
      )}
      {error && (
        <p className="rounded border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm">{error}</p>
      )}
      <Button type="submit" disabled={pending}>
        {pending ? "Saving…" : hasApiKey ? "Update API key" : "Save API key"}
      </Button>
    </form>
  );
}
