"use client";

import { RefreshCw } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import { Button } from "@/components/ui/button";
import { triggerGameRefreshAction } from "@/lib/actions/game-sync";

type Props = {
  raGameId: number;
  hasApiKey: boolean;
  leaderboardScoresSyncedAt?: string | null;
};

export function GameRefreshButton({ raGameId, hasApiKey, leaderboardScoresSyncedAt }: Props) {
  const router = useRouter();
  const [message, setMessage] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  if (!hasApiKey) {
    return (
      <p className="text-sm text-muted-foreground">
        <Link href="/settings" className="text-[var(--accent-retro)] hover:underline">
          Add your RA API key
        </Link>{" "}
        to refresh scores for this game.
      </p>
    );
  }

  return (
    <div className="flex w-full flex-col items-stretch gap-1 sm:items-end">
      <Button
        type="button"
        className="w-full sm:w-auto"
        disabled={pending}
        onClick={() => {
        startTransition(async () => {
          const result = await triggerGameRefreshAction(raGameId);
          setMessage(result.message);
          if (result.ok) {
            router.refresh();
          }
        });
      }}>
        <RefreshCw className={pending ? "animate-spin" : undefined} />
        {pending ? "Refreshing…" : "Refresh scores"}
      </Button>
      {message ? <p className="text-xs text-muted-foreground">{message}</p> : null}
      <LastSyncedLabel at={leaderboardScoresSyncedAt} />
    </div>
  );
}
