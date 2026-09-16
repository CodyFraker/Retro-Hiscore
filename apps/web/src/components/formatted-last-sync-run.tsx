"use client";

import { useSyncExternalStore } from "react";
import type { AdminLastSyncByKindDto } from "@/generated/api-client";
import { formatSyncTime, formatSyncTimeUtc } from "@/lib/format";
import { formatLastSyncByKind } from "@/lib/sync-last-run";

type Props = {
  entry: AdminLastSyncByKindDto | undefined;
};

function emptySubscribe() {
  return () => {};
}

export function FormattedLastSyncRun({ entry }: Props) {
  const label = useSyncExternalStore(
    emptySubscribe,
    () => formatLastSyncByKind(entry, formatSyncTime),
    () => formatLastSyncByKind(entry, formatSyncTimeUtc),
  );

  return <span title={label}>{label}</span>;
}
