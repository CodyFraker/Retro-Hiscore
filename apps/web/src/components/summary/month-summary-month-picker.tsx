"use client";

import { useRouter } from "next/navigation";
import type { SeasonMonthDto } from "@/generated/api-client";

type Props = {
  year: number;
  month: number;
  availableMonths: SeasonMonthDto[];
};

function monthLabel(year: number, month: number) {
  return new Date(Date.UTC(year, month - 1, 1)).toLocaleString(undefined, {
    month: "long",
    year: "numeric",
    timeZone: "UTC",
  });
}

export function MonthSummaryMonthPicker({ year, month, availableMonths }: Props) {
  const router = useRouter();

  if (availableMonths.length <= 1) {
    return (
      <span className="text-sm text-muted-foreground">
        {monthLabel(year, month)} (UTC)
      </span>
    );
  }

  return (
    <label className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
      <span className="sr-only">Month</span>
      <select
        className="h-9 rounded-md border border-border bg-input px-2 text-sm outline-none"
        value={`${year}-${month}`}
        onChange={(event) => {
          const [y, m] = event.target.value.split("-").map(Number);
          if (Number.isFinite(y) && Number.isFinite(m)) {
            router.push(`/summary/${y}/${m}`);
          }
        }}
      >
        {availableMonths.map((entry) => (
          <option key={`${entry.year}-${entry.month}`} value={`${entry.year}-${entry.month}`}>
            {monthLabel(entry.year, entry.month)}
          </option>
        ))}
      </select>
      <span>(UTC)</span>
    </label>
  );
}
