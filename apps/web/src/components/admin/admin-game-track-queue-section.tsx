"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { AdminGameTrackQueueItemDto } from "@/generated/api-client";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useApiClient } from "@/lib/use-api-client";

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
  const api = useApiClient();
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const pendingItems = items.filter((i) => i.status === 0);

  if (items.length === 0) {
    return null;
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
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Title</TableHead>
            <TableHead className="hidden sm:table-cell">Platform</TableHead>
            <TableHead>RA ID</TableHead>
            <TableHead className="hidden md:table-cell">Enqueued</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((item) => (
            <TableRow key={item.id}>
              <TableCell className="font-medium">{item.title}</TableCell>
              <TableCell className="hidden sm:table-cell text-muted-foreground">
                {item.consoleName ?? "—"}
              </TableCell>
              <TableCell className="tabular-nums">{item.raGameId}</TableCell>
              <TableCell className="hidden text-sm text-muted-foreground md:table-cell">
                <FormattedSyncTime value={item.enqueuedAt} />
              </TableCell>
              <TableCell className="text-sm">{statusLabel(item.status)}</TableCell>
              <TableCell className="text-right">
                {item.status === 0 ? (
                  <div className="flex justify-end gap-2">
                    <Button
                      type="button"
                      size="sm"
                      disabled={pending}
                      onClick={() => {
                        setError(null);
                        startTransition(async () => {
                          try {
                            await api.postAdminGameTrackQueueApprove(item.id);
                            router.refresh();
                          } catch (err) {
                            setError(err instanceof Error ? err.message : "Approve failed");
                          }
                        });
                      }}
                    >
                      Approve
                    </Button>
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      disabled={pending}
                      onClick={() => {
                        setError(null);
                        startTransition(async () => {
                          try {
                            await api.postAdminGameTrackQueueReject(item.id);
                            router.refresh();
                          } catch (err) {
                            setError(err instanceof Error ? err.message : "Reject failed");
                          }
                        });
                      }}
                    >
                      Reject
                    </Button>
                  </div>
                ) : (
                  <span className="text-sm text-muted-foreground">—</span>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      {pendingItems.length === 0 && items.length > 0 ? (
        <p className="text-sm text-muted-foreground">No pending items.</p>
      ) : null}
    </section>
  );
}
