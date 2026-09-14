"use client";

import { useEffect, useState } from "react";
import { formatSyncTime, formatSyncTimeUtc } from "@/lib/format";

type Props = {
  value: string;
};

export function FormattedSyncTime({ value }: Props) {
  const [formatted, setFormatted] = useState(() => formatSyncTimeUtc(value));

  useEffect(() => {
    setFormatted(formatSyncTime(value));
  }, [value]);

  return <>{formatted}</>;
}
