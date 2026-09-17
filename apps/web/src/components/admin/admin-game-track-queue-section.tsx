"use client";

import { Plus, X } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState, useTransition, useSyncExternalStore } from "react";
import type { AdminGameTrackQueueItemDto } from "@/generated/api-client";
import { DataFieldList } from "@/components/layout/data-field-list";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  rejectAdminGameTrackQueueItemAction,
  trackAdminGameTrackQueueItemAction,
} from "@/lib/actions/admin";
import { formatRelativeTime, formatSyncTime, formatSyncTimeUtc } from "@/lib/format";

type Props = {
  items: AdminGameTrackQueueItemDto[];
};

const stickyActionClassName =
  "sticky right-0 z-10 bg-background shadow-[-6px_0_10px_-8px_rgba(0,0,0,0.35)] dark:shadow-[-6px_0_10px_-8px_rgba(0,0,0,0.6)]";

function emptySubscribe() {
  return () => {};
}

function statusLabel(status: number) {
  switch (status) {
    case 0:
      return "Pending";
    case 2:
      return "Rejected";
    case 3:
      return "Completed";
    case 4:
      return "Failed";
    default:
      return String(status);
  }
}

function statusBadgeVariant(status: number): "outline" | "secondary" | "destructive" {
  switch (status) {
    case 4:
      return "destructive";
    case 0:
      return "outline";
    default:
      return "secondary";
  }
}

function sourceLabel(source: number) {
  switch (source) {
    case 1:
      return "Member request";
    default:
      return "Recent play";
  }
}

function QueuedAt({ value }: { value: string }) {
  const relative = useSyncExternalStore(
    emptySubscribe,
    () => formatRelativeTime(value),
    () => formatRelativeTime(value),
  );
  const full = useSyncExternalStore(
    emptySubscribe,
    () => formatSyncTime(value),
    () => formatSyncTimeUtc(value),
  );

  return (
    <time dateTime={value} title={full} className="text-sm text-muted-foreground">
      {relative}
    </time>
  );
}

function GameCell({ item }: { item: AdminGameTrackQueueItemDto }) {
  return (
    <div className="min-w-0 max-w-xs space-y-0.5 whitespace-normal">
      <p className="font-medium leading-snug">{item.title}</p>
      <p className="text-xs text-muted-foreground">
        {item.consoleName ?? "Unknown platform"}
        {" · "}
        <a
          href={`https://retroachievements.org/game/${item.raGameId}`}
          target="_blank"
          rel="noopener noreferrer"
          className="tabular-nums hover:text-[var(--accent-retro)]"
        >
          RA #{item.raGameId}
        </a>
        {item.requestCount > 1 ? (
          <>
            {" · "}
            {item.requestCount} requests
          </>
        ) : null}
      </p>
    </div>
  );
}

function SourceCell({ item }: { item: AdminGameTrackQueueItemDto }) {
  const isMemberRequest = item.source === 1;
  return (
    <div className="min-w-0 space-y-0.5 whitespace-normal text-sm">
      <p className="text-muted-foreground">{sourceLabel(item.source)}</p>
      {isMemberRequest && item.requestedByRaUsername ? (
        <p className="text-xs text-muted-foreground">@{item.requestedByRaUsername}</p>
      ) : null}
    </div>
  );
}

function TrackQueueActions({
  item,
  pending,
  onTrack,
  onReject,
  className,
}: {
  item: AdminGameTrackQueueItemDto;
  pending: boolean;
  onTrack: (id: string) => void;
  onReject: (id: string) => void;
  className?: string;
}) {
  if (item.status !== 0) {
    return <span className="text-sm text-muted-foreground">—</span>;
  }

  return (
    <div className={className}>
      <Button
        type="button"
        size="icon-sm"
        disabled={pending}
        title="Track game"
        aria-label="Track game"
        onClick={() => onTrack(item.id)}
      >
        <Plus aria-hidden />
      </Button>
      <Button
        type="button"
        size="icon-sm"
        variant="outline"
        disabled={pending}
        title="Dismiss from queue"
        aria-label="Dismiss from queue"
        onClick={() => onReject(item.id)}
      >
        <X aria-hidden />
      </Button>
    </div>
  );
}

export function AdminGameTrackQueueSection({ items }: Props) {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const pendingItems = items.filter((i) => i.status === 0);

  if (items.length === 0) {
    return null;
  }

  function runReject(id: string) {
    setError(null);
    startTransition(async () => {
      const result = await rejectAdminGameTrackQueueItemAction(id);
      if (!result.ok) {
        setError(result.error);
        return;
      }
      router.refresh();
    });
  }

  function runTrack(id: string) {
    setError(null);
    startTransition(async () => {
      const result = await trackAdminGameTrackQueueItemAction(id);
      if (!result.ok) {
        setError(result.error);
        return;
      }
      router.push(`/admin/games/${result.game.raGameId}`);
      router.refresh();
    });
  }

  return (
    <section className="space-y-3 rounded border border-border p-5">
      <div>
        <h2 className="text-lg font-semibold">Track queue</h2>
        <p className="text-sm text-muted-foreground">
          Member requests and games from recent friend play. Track adds the game to the catalog; dismiss removes
          items you do not want on the site.
        </p>
      </div>
      {error ? <p className="text-sm text-destructive">{error}</p> : null}
      <ResponsiveTable
        rows={items}
        rowKey={(item) => item.id}
        columns={[
          {
            header: "Game",
            cellClassName: "whitespace-normal",
            render: (item) => <GameCell item={item} />,
          },
          {
            header: "Source",
            cellClassName: "whitespace-normal",
            render: (item) => <SourceCell item={item} />,
          },
          {
            header: "Queued",
            cellClassName: "whitespace-normal",
            render: (item) => <QueuedAt value={item.enqueuedAt} />,
          },
          {
            header: "Status",
            render: (item) => (
              <Badge variant={statusBadgeVariant(item.status)}>{statusLabel(item.status)}</Badge>
            ),
          },
          {
            header: <span className="sr-only">Actions</span>,
            headerClassName: `w-0 text-right ${stickyActionClassName}`,
            cellClassName: `text-right ${stickyActionClassName}`,
            render: (item) => (
              <TrackQueueActions
                item={item}
                pending={pending}
                onTrack={runTrack}
                onReject={runReject}
                className="inline-flex shrink-0 justify-end gap-1"
              />
            ),
          },
        ]}
        renderMobileCard={(item) => (
          <li key={item.id} className="rounded border border-border bg-card p-4 text-sm">
            <GameCell item={item} />
            <DataFieldList
              className="mt-2"
              fields={[
                {
                  label: "Source",
                  value: (
                    <span>
                      {sourceLabel(item.source)}
                      {item.source === 1 && item.requestedByRaUsername
                        ? ` (@${item.requestedByRaUsername})`
                        : null}
                    </span>
                  ),
                },
                {
                  label: "Queued",
                  value: <QueuedAt value={item.enqueuedAt} />,
                },
                {
                  label: "Status",
                  value: (
                    <Badge variant={statusBadgeVariant(item.status)}>{statusLabel(item.status)}</Badge>
                  ),
                },
              ]}
            />
            {item.status === 0 ? (
              <TrackQueueActions
                item={item}
                pending={pending}
                onTrack={runTrack}
                onReject={runReject}
                className="mt-3 flex gap-2"
              />
            ) : null}
          </li>
        )}
      />
      {pendingItems.length === 0 && items.length > 0 ? (
        <p className="text-sm text-muted-foreground">No pending items.</p>
      ) : null}
    </section>
  );
}
