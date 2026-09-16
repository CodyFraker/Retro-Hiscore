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
        These games showed up in recent friend play and are waiting for an admin to add them to the catalog.
      </p>
      <ul className="divide-y divide-border text-sm">
        {pending.map((item) => (
          <li key={item.raGameId} className="flex flex-wrap items-baseline justify-between gap-2 py-2">
            <span className="font-medium">{item.title ?? `RA #${item.raGameId}`}</span>
            <span className="text-xs text-muted-foreground">
              {item.consoleName ?? "Unknown platform"}
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
