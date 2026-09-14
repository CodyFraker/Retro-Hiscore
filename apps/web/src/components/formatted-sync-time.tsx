"use client";

import { useSyncExternalStore } from "react";
import { formatSyncTime, formatSyncTimeUtc } from "@/lib/format";

type Props = {
  value: string;
};

function emptySubscribe() {
  return () => {};
}

export function FormattedSyncTime({ value }: Props) {
  const formatted = useSyncExternalStore(
    emptySubscribe,
    () => formatSyncTime(value),
    () => formatSyncTimeUtc(value),
  );

  return <>{formatted}</>;
}
