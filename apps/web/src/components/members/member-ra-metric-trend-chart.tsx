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
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

type Props = {
  title: string;
  emptyMessage: string;
  dataKey: string;
  label: string;
  items: { syncedAt: string; value: number | null | undefined }[];
  reversedY?: boolean;
  formatValue?: (value: number) => string;
  stroke?: string;
};

function formatTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

const defaultFormatValue = (value: number) => value.toLocaleString();

export function MemberRaMetricTrendChart({
  title,
  emptyMessage,
  dataKey,
  label,
  items,
  reversedY = false,
  formatValue = defaultFormatValue,
  stroke = "var(--chart-1)",
}: Props) {
  const chartData = useMemo(
    () =>
      items
        .filter((item) => item.value != null)
        .map((item) => ({
          syncedAt: item.syncedAt,
          [dataKey]: item.value as number,
        })),
    [items, dataKey],
  );

  if (chartData.length < 2) {
    return (
      <Card size="sm" className="min-w-0">
        <CardHeader>
          <CardTitle>{title}</CardTitle>
        </CardHeader>
        <CardContent>
          <ChartEmptyState message={emptyMessage} />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card size="sm">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent className="h-44">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chartData} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
            <CartesianGrid stroke="var(--border)" strokeDasharray="3 3" vertical={false} />
            <XAxis
              dataKey="syncedAt"
              tickFormatter={formatTick}
              tick={{ fill: "var(--muted-foreground)", fontSize: 10 }}
              tickLine={false}
              axisLine={false}
              minTickGap={24}
            />
            <YAxis
              reversed={reversedY}
              width={48}
              tick={{ fill: "var(--muted-foreground)", fontSize: 10 }}
              tickLine={false}
              axisLine={false}
              tickFormatter={(value) => formatValue(Number(value))}
            />
            <Tooltip
              labelFormatter={(tick) => formatTick(String(tick))}
              formatter={(value) => [formatValue(Number(value)), label]}
              contentStyle={{
                background: "var(--card)",
                border: "1px solid var(--border)",
                borderRadius: "var(--radius-md)",
              }}
            />
            <Line
              type="monotone"
              dataKey={dataKey}
              stroke={stroke}
              strokeWidth={2}
              dot={chartData.length <= 24}
              activeDot={{ r: 4 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
