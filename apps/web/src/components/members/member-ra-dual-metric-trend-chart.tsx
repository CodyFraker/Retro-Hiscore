"use client";

import { useMemo } from "react";
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
import { chartYDomain } from "@/lib/chart-y-domain";

type SeriesInput = {
  syncedAt: string;
  value: number | null | undefined;
};

type SeriesConfig = {
  dataKey: string;
  label: string;
  stroke: string;
  items: SeriesInput[];
};

type Props = {
  series: SeriesConfig[];
};

function formatSyncTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

const defaultFormatValue = (value: number) => value.toLocaleString();

export function MemberRaDualMetricTrendChart({ series }: Props) {
  const chartData = useMemo(() => {
    const byTime = new Map<string, Record<string, number | string>>();

    for (const entry of series) {
      for (const point of entry.items) {
        if (point.value == null) {
          continue;
        }
        const row = byTime.get(point.syncedAt) ?? { syncedAt: point.syncedAt };
        row[entry.dataKey] = point.value;
        byTime.set(point.syncedAt, row);
      }
    }

    return [...byTime.values()].sort(
      (a, b) =>
        new Date(String(a.syncedAt)).getTime() - new Date(String(b.syncedAt)).getTime(),
    );
  }, [series]);

  const yDomain = useMemo(() => {
    const values: number[] = [];
    for (const row of chartData) {
      for (const entry of series) {
        const value = row[entry.dataKey];
        if (typeof value === "number") {
          values.push(value);
        }
      }
    }
    return chartYDomain(values, "tight");
  }, [chartData, series]);

  if (chartData.length < 2) {
    return null;
  }

  return (
    <div className="space-y-2">
      <h4 className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
        Hardcore and true points
      </h4>
      <div className="h-44 min-w-0">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chartData} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
            <CartesianGrid stroke="var(--border)" strokeDasharray="3 3" vertical={false} />
            <XAxis
              dataKey="syncedAt"
              tickFormatter={formatSyncTick}
              tick={{ fill: "var(--muted-foreground)", fontSize: 10 }}
              tickLine={false}
              axisLine={false}
              minTickGap={24}
            />
            <YAxis
              type="number"
              domain={yDomain}
              allowDataOverflow
              width={52}
              tick={{ fill: "var(--muted-foreground)", fontSize: 10 }}
              tickLine={false}
              axisLine={false}
              tickFormatter={(value) => defaultFormatValue(Number(value))}
            />
            <Tooltip
              labelFormatter={(tick) => formatSyncTick(String(tick))}
              contentStyle={{
                background: "var(--card)",
                border: "1px solid var(--border)",
                borderRadius: "var(--radius-md)",
              }}
              formatter={(value, name) => [
                defaultFormatValue(Number(value)),
                series.find((entry) => entry.dataKey === name)?.label ?? String(name),
              ]}
            />
            <Legend
              wrapperStyle={{ fontSize: 11 }}
              formatter={(value) =>
                series.find((entry) => entry.dataKey === value)?.label ?? value
              }
            />
            {series.map((entry) => (
              <Line
                key={entry.dataKey}
                type="monotone"
                dataKey={entry.dataKey}
                name={entry.dataKey}
                stroke={entry.stroke}
                strokeWidth={2}
                dot={chartData.length <= 24}
                activeDot={{ r: 4 }}
              />
            ))}
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
