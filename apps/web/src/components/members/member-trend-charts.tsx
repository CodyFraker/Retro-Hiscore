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
import { ChartEmptyState } from "@/components/charts/chart-empty-state";
import type { MemberHistoryItemDto } from "@/generated/api-client";
import {
  countDistinctSyncTimestampsForAggregate,
  countDistinctSyncTimestampsForBoards,
  toAggregateSeries,
  toBoardRankSeries,
} from "@/lib/member-history-series";

const CHART_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

type Props = {
  items: MemberHistoryItemDto[];
};

function formatTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

function boardSeriesKey(raLeaderboardId: number) {
  return `board-${raLeaderboardId}`;
}

export function MemberTrendCharts({ items }: Props) {
  const aggregateSeries = useMemo(() => toAggregateSeries(items), [items]);
  const boardSeries = useMemo(() => toBoardRankSeries(items), [items]);
  const [hiddenBoards, setHiddenBoards] = useState<Record<string, boolean>>({});

  const leadChartData = useMemo(
    () =>
      aggregateSeries.map((point) => ({
        syncedAt: point.syncedAt,
        friendRankOnes: point.friendRankOnes,
      })),
    [aggregateSeries],
  );

  const rankChartData = useMemo(() => {
    const byTime = new Map<string, Record<string, string | number>>();
    for (const board of boardSeries) {
      const key = boardSeriesKey(board.raLeaderboardId);
      for (const point of board.points) {
        if (point.friendRank == null) {
          continue;
        }
        const row = byTime.get(point.syncedAt) ?? { syncedAt: point.syncedAt };
        row[key] = point.friendRank;
        byTime.set(point.syncedAt, row);
      }
    }
    return [...byTime.values()].sort(
      (a, b) =>
        new Date(String(a.syncedAt)).getTime() - new Date(String(b.syncedAt)).getTime(),
    );
  }, [boardSeries]);

  const maxFriendRank = useMemo(() => {
    let max = 1;
    for (const board of boardSeries) {
      for (const point of board.points) {
        if (point.friendRank != null && point.friendRank > max) {
          max = point.friendRank;
        }
      }
    }
    return max;
  }, [boardSeries]);

  const hasLeadTrend = countDistinctSyncTimestampsForAggregate(aggregateSeries) >= 2;
  const hasRankTrend = countDistinctSyncTimestampsForBoards(boardSeries) >= 2;

  return (
    <div className="space-y-10">
      <section className="space-y-3">
        <h2 className="text-lg font-medium">Board leads over time</h2>
        {hasLeadTrend ? (
          <div className="h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={leadChartData} margin={{ top: 8, right: 4, left: 0, bottom: 0 }}>
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
                  width={40}
                  allowDecimals={false}
                />
                <Tooltip
                  contentStyle={{
                    background: "var(--card)",
                    border: "1px solid var(--border)",
                    borderRadius: "0.4rem",
                    color: "var(--foreground)",
                  }}
                  labelFormatter={(label) => formatTick(String(label))}
                  formatter={(value) => [value, "Friend #1 boards"]}
                />
                <Line
                  type="monotone"
                  dataKey="friendRankOnes"
                  name="Friend #1 boards"
                  stroke={CHART_COLORS[0]}
                  strokeWidth={2}
                  dot={{ r: 3 }}
                  connectNulls
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        ) : (
          <ChartEmptyState />
        )}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-medium">Friend rank by board</h2>
        {hasRankTrend ? (
          <div className="h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={rankChartData} margin={{ top: 8, right: 4, left: 0, bottom: 0 }}>
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
                  width={40}
                  reversed
                  domain={[1, maxFriendRank]}
                  allowDecimals={false}
                />
                <Tooltip
                  contentStyle={{
                    background: "var(--card)",
                    border: "1px solid var(--border)",
                    borderRadius: "0.4rem",
                    color: "var(--foreground)",
                  }}
                  labelFormatter={(label) => formatTick(String(label))}
                  formatter={(value, name) => {
                    const board = boardSeries.find(
                      (item) => boardSeriesKey(item.raLeaderboardId) === String(name),
                    );
                    const label = board
                      ? `${board.gameTitle} — ${board.leaderboardTitle}`
                      : String(name);
                    return [`#${value}`, label];
                  }}
                />
                <Legend
                  wrapperStyle={{ flexWrap: "wrap", paddingTop: 8 }}
                  onClick={(payload) => {
                    const id = String(payload.dataKey ?? "");
                    if (!id) return;
                    setHiddenBoards((prev) => ({ ...prev, [id]: !prev[id] }));
                  }}
                />
                {boardSeries.map((board, index) => {
                  const key = boardSeriesKey(board.raLeaderboardId);
                  return (
                    <Line
                      key={key}
                      type="monotone"
                      dataKey={key}
                      name={`${board.gameTitle} — ${board.leaderboardTitle}`}
                      stroke={CHART_COLORS[index % CHART_COLORS.length]}
                      strokeWidth={2}
                      dot={{ r: 3 }}
                      connectNulls
                      hide={Boolean(hiddenBoards[key])}
                    />
                  );
                })}
              </LineChart>
            </ResponsiveContainer>
          </div>
        ) : (
          <ChartEmptyState />
        )}
      </section>
    </div>
  );
}
