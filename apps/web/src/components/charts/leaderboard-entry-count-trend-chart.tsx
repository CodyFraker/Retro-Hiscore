"use client";

import { useMemo } from "react";
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { ChartEmptyState } from "@/components/charts/chart-empty-state";
import { formatPopulationDelta } from "@/lib/game-delta-format";
import type { LeaderboardPopulationPoint } from "@/lib/history-series";

type Props = {
  points: LeaderboardPopulationPoint[];
};

function formatTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

export function LeaderboardEntryCountTrendChart({ points }: Props) {
  const previousCountBySync = useMemo(() => {
    const map = new Map<string, number | null>();
    for (let index = 0; index < points.length; index++) {
      map.set(points[index].syncedAt, index > 0 ? points[index - 1].entryCount : null);
    }
    return map;
  }, [points]);

  const yDomain = useMemo(() => {
    const values = points.map((point) => point.entryCount);
    const min = Math.min(...values);
    const max = Math.max(...values);
    if (min === max) {
      return [Math.max(1, min - 1), max + 1];
    }
    const padding = Math.max(1, Math.ceil((max - min) * 0.05));
    return [Math.max(1, min - padding), max + padding];
  }, [points]);

  if (points.length < 2) {
    return <ChartEmptyState message="Need at least two syncs with entry counts to chart field growth." />;
  }

  return (
    <div className="min-w-0 h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={points} margin={{ top: 8, right: 4, left: 0, bottom: 0 }}>
          <CartesianGrid stroke="var(--border)" strokeDasharray="3 3" />
          <XAxis
            dataKey="syncedAt"
            tickFormatter={formatTick}
            stroke="var(--muted-foreground)"
            fontSize={11}
            minTickGap={32}
          />
          <YAxis
            stroke="var(--muted-foreground)"
            fontSize={11}
            width={56}
            domain={yDomain}
            allowDecimals={false}
            tickFormatter={(value) => Number(value).toLocaleString()}
          />
          <Tooltip
            contentStyle={{
              background: "var(--card)",
              border: "1px solid var(--border)",
              borderRadius: "0.4rem",
              color: "var(--foreground)",
            }}
            labelFormatter={(label) => formatTick(String(label))}
            formatter={(value, _name, item) => {
              const syncedAt = String(item?.payload?.syncedAt ?? "");
              const count = typeof value === "number" ? value : Number(value);
              const previous = previousCountBySync.get(syncedAt);
              const delta =
                previous != null && Number.isFinite(count) ? count - previous : null;
              const label =
                delta != null && delta !== 0
                  ? `${count.toLocaleString()} (${formatPopulationDelta(delta)} since prior sync)`
                  : count.toLocaleString();
              return [label, "Ranked players"];
            }}
          />
          <Line
            type="monotone"
            dataKey="entryCount"
            name="Ranked players"
            stroke="var(--chart-1)"
            strokeWidth={2}
            dot={{ r: 3 }}
            connectNulls
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
