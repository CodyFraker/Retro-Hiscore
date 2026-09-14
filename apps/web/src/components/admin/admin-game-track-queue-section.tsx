"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { AdminGameTrackQueueItemDto } from "@/generated/api-client";
import { DataFieldList } from "@/components/layout/data-field-list";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { Button } from "@/components/ui/button";
import {
  approveAdminGameTrackQueueItemAction,
  rejectAdminGameTrackQueueItemAction,
} from "@/lib/actions/admin";

type Props = {
  items: AdminGameTrackQueueItemDto[];
};

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

export function AdminGameTrackQueueSection({ items }: Props) {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const pendingItems = items.filter((i) => i.status === 0);

  if (items.length === 0) {
    return null;
  }

  function runAction(action: () => Promise<{ ok: true } | { ok: false; error: string }>) {
    setError(null);
    startTransition(async () => {
      const result = await action();
      if (!result.ok) {
        setError(result.error);
        return;
      }
      router.refresh();
    });
  }

  return (
    <section className="space-y-3 rounded border border-border p-5">
      <div>
        <h2 className="text-lg font-semibold">Track queue</h2>
        <p className="text-sm text-muted-foreground">
          Games discovered from friend recent play. Approve to track on the site.
        </p>
      </div>
      {error ? <p className="text-sm text-destructive">{error}</p> : null}
      <ResponsiveTable
        rows={items}
        rowKey={(item) => item.id}
        columns={[
          { header: "Title", cellClassName: "font-medium", render: (item) => item.title },
          {
            header: "Platform",
            cellClassName: "text-muted-foreground",
            render: (item) => item.consoleName ?? "—",
          },
          {
            header: "RA ID",
            cellClassName: "tabular-nums",
            render: (item) => item.raGameId,
          },
          {
            header: "Enqueued",
            cellClassName: "text-sm text-muted-foreground",
            render: (item) => <FormattedSyncTime value={item.enqueuedAt} />,
          },
          {
            header: "Status",
            cellClassName: "text-sm",
            render: (item) => statusLabel(item.status),
          },
          {
            header: "Actions",
            headerClassName: "text-right",
            cellClassName: "text-right",
            render: (item) =>
              item.status === 0 ? (
                <QueueActions
                  pending={pending}
                  stacked={false}
                  onApprove={() => runAction(() => approveAdminGameTrackQueueItemAction(item.id))}
                  onReject={() => runAction(() => rejectAdminGameTrackQueueItemAction(item.id))}
                />
              ) : (
                <span className="text-sm text-muted-foreground">—</span>
              ),
          },
        ]}
        renderMobileCard={(item) => (
          <li key={item.id} className="rounded border border-border bg-card p-4 text-sm">
            <p className="font-medium">{item.title}</p>
            <DataFieldList
              className="mt-2"
              fields={[
                { label: "Platform", value: item.consoleName ?? "—" },
                { label: "RA ID", value: item.raGameId },
                {
                  label: "Enqueued",
                  value: <FormattedSyncTime value={item.enqueuedAt} />,
                },
                { label: "Status", value: statusLabel(item.status) },
              ]}
            />
            {item.status === 0 ? (
              <QueueActions
                className="mt-3"
                pending={pending}
                stacked
                onApprove={() => runAction(() => approveAdminGameTrackQueueItemAction(item.id))}
                onReject={() => runAction(() => rejectAdminGameTrackQueueItemAction(item.id))}
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

function QueueActions({
  pending,
  onApprove,
  onReject,
  stacked = false,
  className,
}: {
  pending: boolean;
  onApprove: () => void;
  onReject: () => void;
  stacked?: boolean;
  className?: string;
}) {
  return (
    <div
      className={
        stacked
          ? `flex flex-col gap-2 ${className ?? ""}`
          : `flex justify-end gap-2 ${className ?? ""}`
      }
    >
      <Button
        type="button"
        size="sm"
        className={stacked ? "w-full" : undefined}
        disabled={pending}
        onClick={onApprove}
      >
        Approve
      </Button>
      <Button
        type="button"
        size="sm"
        variant="outline"
        className={stacked ? "w-full" : undefined}
        disabled={pending}
        onClick={onReject}
      >
        Reject
      </Button>
    </div>
  );
}
