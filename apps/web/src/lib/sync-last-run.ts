import type { AdminLastSyncByKindDto } from "@/generated/api-client";
import { formatSyncTime } from "@/lib/format";

export function findLastSyncByKind(
  entries: AdminLastSyncByKindDto[],
  kind: string,
): AdminLastSyncByKindDto | undefined {
  return entries.find((entry) => entry.kind === kind);
}

export function formatLastSyncByKind(
  entry: AdminLastSyncByKindDto | undefined,
  formatTime: (value: string | null | undefined) => string = formatSyncTime,
): string {
  if (!entry?.startedAt && !entry?.finishedAt) {
    return "Never synced";
  }
  const at = entry.finishedAt ?? entry.startedAt;
  if (!at) {
    return "Never synced";
  }
  const time = formatTime(at);
  return entry.status ? `${time} (${entry.status})` : time;
}
