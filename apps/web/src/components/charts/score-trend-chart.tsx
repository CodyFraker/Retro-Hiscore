"use client";

import { useMemo, useState } from "react";
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import type { MemberSeries } from "@/lib/history-series";
import { countDistinctSyncTimestamps } from "@/lib/history-series";
import { ChartEmptyState } from "@/components/charts/chart-empty-state";

const CHART_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

type Props = {
  series: MemberSeries[];
};

function formatTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

export function ScoreTrendChart({ series }: Props) {
  const [hidden, setHidden] = useState<Record<string, boolean>>({});

  const chartData = useMemo(() => {
    const byTime = new Map<string, Record<string, string | number>>();
    for (const member of series) {
      for (const point of member.points) {
        const row = byTime.get(point.syncedAt) ?? { syncedAt: point.syncedAt };
        row[member.memberId] = point.score;
        byTime.set(point.syncedAt, row);
      }
    }
    return [...byTime.values()].sort(
      (a, b) =>
        new Date(String(a.syncedAt)).getTime() - new Date(String(b.syncedAt)).getTime(),
    );
  }, [series]);

  if (countDistinctSyncTimestamps(series) < 2) {
    return <ChartEmptyState />;
  }

  return (
    <div className="h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={chartData} margin={{ top: 8, right: 4, left: 0, bottom: 0 }}>
          <CartesianGrid stroke="var(--border)" strokeDasharray="3 3" />
          <XAxis
            dataKey="syncedAt"
            tickFormatter={formatTick}
            stroke="var(--muted-foreground)"
            fontSize={11}
            minTickGap={32}
          />
          <YAxis stroke="var(--muted-foreground)" fontSize={11} width={56} />
          <Tooltip
            contentStyle={{
              background: "var(--card)",
              border: "1px solid var(--border)",
              borderRadius: "0.4rem",
              color: "var(--foreground)",
            }}
            labelFormatter={(label) => formatTick(String(label))}
          />
          <Legend
            wrapperStyle={{ flexWrap: "wrap", paddingTop: 8 }}
            onClick={(payload) => {
              const id = String(payload.dataKey ?? "");
              if (!id) return;
              setHidden((prev) => ({ ...prev, [id]: !prev[id] }));
            }}
          />
          {series.map((member, index) => (
            <Line
              key={member.memberId}
              type="monotone"
              dataKey={member.memberId}
              name={member.displayName}
              stroke={CHART_COLORS[index % CHART_COLORS.length]}
              strokeWidth={2}
              dot={{ r: 3 }}
              connectNulls
              hide={Boolean(hidden[member.memberId])}
            />
          ))}
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
