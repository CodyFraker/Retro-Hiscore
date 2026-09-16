"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { AdminGameTrackQueueItemDto } from "@/generated/api-client";
import { DataFieldList } from "@/components/layout/data-field-list";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { Button } from "@/components/ui/button";
import { rejectAdminGameTrackQueueItemAction } from "@/lib/actions/admin";

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

  return (
    <section className="space-y-3 rounded border border-border p-5">
      <div>
        <h2 className="text-lg font-semibold">Track queue</h2>
        <p className="text-sm text-muted-foreground">
          Games discovered from friend recent play. Nominate winners via Game of the week voting.
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
                <Button
                  type="button"
                  size="sm"
                  variant="outline"
                  disabled={pending}
                  onClick={() => runReject(item.id)}
                >
                  Dismiss
                </Button>
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
              <Button
                type="button"
                size="sm"
                variant="outline"
                className="mt-3 w-full"
                disabled={pending}
                onClick={() => runReject(item.id)}
              >
                Dismiss
              </Button>
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
