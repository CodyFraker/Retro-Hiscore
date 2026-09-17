"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { Send } from "lucide-react";
import { useState, useTransition } from "react";
import { toast } from "sonner";
import type { GameTrackRequestQuotaDto } from "@/generated/api-client";
import { Button } from "@/components/ui/button";
import { postGameTrackRequestAction } from "@/lib/actions/track-requests";
import { parseRaGameIdInput } from "@/lib/dashboard-games";

type Props = {
  initialQuota: GameTrackRequestQuotaDto | null;
};

function formatNextSlot(iso: string | null | undefined): string | null {
  if (!iso) {
    return null;
  }
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) {
    return null;
  }
  return date.toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" });
}

export function GameTrackRequestForm({ initialQuota }: Props) {
  const router = useRouter();
  const [input, setInput] = useState("");
  const [quota, setQuota] = useState(initialQuota);
  const [pending, startTransition] = useTransition();

  const quotaLine =
    quota === null
      ? null
      : quota.remaining > 0
        ? `${quota.remaining} of ${quota.limit} requests left in the next 24 hours.`
        : `No requests left. Next slot ${formatNextSlot(quota.nextSlotAt ?? undefined) ?? "soon"}.`;

  return (
    <section className="space-y-3 rounded border border-border p-5">
      <div>
        <h2 className="text-lg font-semibold">Request a game</h2>
        <p className="text-sm text-muted-foreground">
          Suggest a RetroAchievements title for admins to track. Up to five requests per rolling 24 hours.
        </p>
      </div>
      <form
        className="flex flex-col gap-2"
        onSubmit={(event) => {
          event.preventDefault();
          const parsed = parseRaGameIdInput(input);
          if (parsed === null) {
            toast.error("Enter a valid RetroAchievements game id or URL.");
            return;
          }

          startTransition(async () => {
            const result = await postGameTrackRequestAction(parsed);
            if (!result.ok) {
              toast.error(result.error);
              return;
            }

            const { response } = result;
            if (response.code === "alreadyTracked") {
              toast.message(`${response.title} is already tracked.`, {
                action: {
                  label: "View game",
                  onClick: () => router.push(`/games/${response.raGameId}`),
                },
              });
              return;
            }

            if (response.code === "quotaExceeded") {
              toast.error(response.message ?? "Request limit reached.");
              if (quota) {
                setQuota({
                  ...quota,
                  remaining: 0,
                  used: quota.limit,
                  nextSlotAt: response.nextSlotAt ?? quota.nextSlotAt,
                });
              }
              return;
            }

            if (response.code === "created") {
              toast.success(`Requested ${response.title}. Admins will review the track queue.`);
              setInput("");
              if (quota) {
                setQuota({
                  ...quota,
                  used: quota.used + 1,
                  remaining: Math.max(0, quota.remaining - 1),
                });
              }
            } else if (response.code === "alreadyPending") {
              toast.message(`${response.title} is already on the track queue.`);
            }

            router.refresh();
          });
        }}
      >
        <label htmlFor="track-request-input" className="text-xs font-medium text-muted-foreground">
          RetroAchievements game id or URL
        </label>
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:gap-3">
          <input
            id="track-request-input"
            type="text"
            value={input}
            disabled={pending}
            onChange={(event) => setInput(event.target.value)}
            placeholder="e.g. 38130 or retroachievements.org/game/38130"
            className="h-8 min-w-0 flex-1 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50 disabled:opacity-50"
          />
          <Button type="submit" className="w-full shrink-0 sm:w-auto" disabled={pending || !input.trim()}>
            <Send />
            {pending ? "Submitting…" : "Request"}
          </Button>
        </div>
        {quotaLine ? <p className="text-xs text-muted-foreground">{quotaLine}</p> : null}
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
          . Requests appear in{" "}
          <Link href="#pending-track" className="text-foreground hover:text-[var(--accent-retro)]">
            pending track requests
          </Link>
          .
        </span>
      </form>
    </section>
  );
}
