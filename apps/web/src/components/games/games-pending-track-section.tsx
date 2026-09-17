import type { GameTrackQueueItemDto } from "@/generated/api-client";
import { FormattedSyncTime } from "@/components/formatted-sync-time";

type Props = {
  items: GameTrackQueueItemDto[];
};

export function GamesPendingTrackSection({ items }: Props) {
  const pending = items.filter((item) => item.status === "Pending");
  if (pending.length === 0) {
    return null;
  }

  return (
    <section id="pending-track" className="space-y-3 rounded border border-border p-5">
      <h2 className="text-lg font-semibold">Pending track requests</h2>
      <p className="text-sm text-muted-foreground">
        Member requests and games from recent friend play waiting for an admin to track or dismiss.
      </p>
      <ul className="divide-y divide-border text-sm">
        {pending.map((item) => (
          <li key={item.raGameId} className="flex flex-wrap items-baseline justify-between gap-2 py-2">
            <a
              href={`https://retroachievements.org/game/${item.raGameId}`}
              target="_blank"
              rel="noopener noreferrer"
              className="font-medium hover:text-[var(--accent-retro)]"
            >
              {item.title ?? `RA #${item.raGameId}`}
            </a>
            <span className="text-xs text-muted-foreground">
              {item.consoleName ?? "Unknown platform"}
              {item.source === "MemberRequest" ? " · Member request" : null}
              {item.requestCount > 1 ? ` · ${item.requestCount} requests` : null}
              {item.enqueuedAt ? (
                <>
                  {" · "}
                  <FormattedSyncTime value={item.enqueuedAt} />
                </>
              ) : null}
            </span>
          </li>
        ))}
      </ul>
    </section>
  );
}
