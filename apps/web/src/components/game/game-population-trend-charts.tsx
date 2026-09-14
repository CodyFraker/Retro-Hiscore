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
import { ChartEmptyState } from "@/components/charts/chart-empty-state";
import type { GameLeaderboardPopulationHistoryResponse } from "@/generated/api-client";
import {
  type TotalPopulationPoint,
  toTotalPopulationSeries,
} from "@/lib/game-population-series";

const CHART_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

type Props = {
  data: GameLeaderboardPopulationHistoryResponse;
};

function formatTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

function seriesKey(raLeaderboardId: number) {
  return `board-${raLeaderboardId}`;
}

const CHART_TOOLTIP_CONTENT_STYLE = {
  background: "var(--card)",
  border: "1px solid var(--border)",
  borderRadius: "0.4rem",
  color: "var(--foreground)",
} as const;

export function GamePopulationTrendCharts({ data }: Props) {
  const totalSeries = useMemo(() => toTotalPopulationSeries(data), [data]);

  const perBoardChartData = useMemo(() => {
    const byTime = new Map<string, Record<string, string | number>>();

    for (const board of data.boards) {
      const key = seriesKey(board.raLeaderboardId);
      for (const point of board.points) {
        const row = byTime.get(point.syncedAt) ?? { syncedAt: point.syncedAt };
        row[key] = point.entryCount;
        byTime.set(point.syncedAt, row);
      }
    }

    return [...byTime.values()].sort(
      (a, b) =>
        new Date(String(a.syncedAt)).getTime() - new Date(String(b.syncedAt)).getTime(),
    );
  }, [data.boards]);

  const boardsWithPoints = data.boards.filter((board) => board.points.length > 0);

  if (totalSeries.length === 0) {
    return (
      <section className="space-y-3">
        <h2 className="steam-section-heading">Total ranked entries over time</h2>
        <ChartEmptyState message="No population snapshots yet. Run a leaderboard sync to start tracking." />
      </section>
    );
  }

  return (
    <div className="space-y-8">
      <section className="space-y-3">
        <div>
          <h2 className="steam-section-heading">Total ranked entries over time</h2>
          <p className="text-xs text-muted-foreground">
            Sum of ranked players across all leaderboards at each sync (same player on multiple
            boards is counted more than once).
          </p>
        </div>
        <div className="min-w-0 h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={totalSeries} margin={{ top: 8, right: 4, left: 0, bottom: 0 }}>
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
                tickFormatter={(value) => value.toLocaleString()}
              />
              <Tooltip
                contentStyle={CHART_TOOLTIP_CONTENT_STYLE}
                labelFormatter={(label) => formatTick(String(label))}
                formatter={(value, _name, item) => {
                  if (typeof value !== "number" || item == null) {
                    return ["—", "Total"];
                  }
                  const payload = item.payload as TotalPopulationPoint;
                  return [
                    `${value.toLocaleString()} entries (${payload.boardCount} boards)`,
                    "Total",
                  ];
                }}
              />
              <Line
                type="monotone"
                dataKey="totalEntryCount"
                name="Total ranked entries"
                stroke="var(--accent-retro)"
                dot={totalSeries.length <= 24}
                strokeWidth={2}
                connectNulls
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </section>

      {boardsWithPoints.length > 0 && (
        <section className="space-y-3">
          <h2 className="steam-section-heading">By leaderboard</h2>
          <div className="min-w-0 h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={perBoardChartData} margin={{ top: 8, right: 4, left: 0, bottom: 0 }}>
                <CartesianGrid stroke="var(--border)" strokeDasharray="3 3" />
                <XAxis
                  dataKey="syncedAt"
                  tickFormatter={formatTick}
                  stroke="var(--muted-foreground)"
                  fontSize={11}
                  minTickGap={32}
                />
                <YAxis stroke="var(--muted-foreground)" fontSize={11} width={48} />
                <Tooltip
                  contentStyle={CHART_TOOLTIP_CONTENT_STYLE}
                  labelFormatter={(label) => formatTick(String(label))}
                  formatter={(value) =>
                    typeof value === "number"
                      ? [value.toLocaleString(), "Players"]
                      : ["—", "Players"]
                  }
                />
                <Legend />
                {boardsWithPoints.map((board, index) => (
                  <Line
                    key={board.raLeaderboardId}
                    type="monotone"
                    dataKey={seriesKey(board.raLeaderboardId)}
                    name={board.title}
                    stroke={CHART_COLORS[index % CHART_COLORS.length]}
                    dot={false}
                    strokeWidth={2}
                    connectNulls
                  />
                ))}
              </LineChart>
            </ResponsiveContainer>
          </div>
        </section>
      )}
    </div>
  );
}
