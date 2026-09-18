"use client";

import Link from "next/link";
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
import { LeaderboardEntryCountTrendChart } from "@/components/charts/leaderboard-entry-count-trend-chart";
import type { GameLeaderboardPopulationHistoryResponse } from "@/generated/api-client";
import { formatPopulationDelta } from "@/lib/game-delta-format";
import {
  expectedPopulationBoardCount,
  hasAnyPopulationSnapshots,
  hasPartialPopulationSnapshots,
  toBoardPopulationRows,
  toTotalPopulationSeries,
} from "@/lib/game-population-series";

const CHART_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

const MAX_COMPARE_BOARDS = 5;

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
  const expectedBoardCount = expectedPopulationBoardCount(data);
  const totalSeries = useMemo(() => toTotalPopulationSeries(data), [data]);
  const chartPoints = useMemo(
    () =>
      totalSeries.map((point) => ({
        syncedAt: point.syncedAt,
        entryCount: point.totalEntryCount,
      })),
    [totalSeries],
  );
  const boardRows = useMemo(() => toBoardPopulationRows(data), [data]);
  const [compareIds, setCompareIds] = useState<number[]>([]);

  const latestCompleteKpi = useMemo(() => {
    if (totalSeries.length === 0) {
      return null;
    }
    const latest = totalSeries.at(-1)!;
    const previous = totalSeries.length >= 2 ? totalSeries.at(-2) : undefined;
    const delta =
      previous != null ? latest.totalEntryCount - previous.totalEntryCount : null;
    return { total: latest.totalEntryCount, delta };
  }, [totalSeries]);

  const perBoardChartData = useMemo(() => {
    const byTime = new Map<string, Record<string, string | number>>();

    for (const board of data.boards) {
      if (!compareIds.includes(board.raLeaderboardId)) {
        continue;
      }
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
  }, [compareIds, data.boards]);

  const compareBoards = boardRows.filter((row) => compareIds.includes(row.raLeaderboardId));

  function toggleCompare(raLeaderboardId: number) {
    setCompareIds((current) => {
      if (current.includes(raLeaderboardId)) {
        return current.filter((id) => id !== raLeaderboardId);
      }
      if (current.length >= MAX_COMPARE_BOARDS) {
        return current;
      }
      return [...current, raLeaderboardId];
    });
  }

  function renderTotalEntriesSection() {
    const heading = (
      <div>
        <h2 className="steam-section-heading">Total entries over time</h2>
        <p className="text-xs text-muted-foreground">
          Ranked players on RetroAchievements at each full-game sync—growth shows new submissions
          across this title. Summing across boards counts the same player once per board they
          appear on. Only syncs where all {expectedBoardCount} tracked leaderboards were captured
          are shown; partial syncs are hidden.
        </p>
      </div>
    );

    if (!hasAnyPopulationSnapshots(data)) {
      return (
        <section className="space-y-3">
          {heading}
          <ChartEmptyState message="No population snapshots yet. Run a leaderboard sync to start tracking." />
        </section>
      );
    }

    if (totalSeries.length === 0 && hasPartialPopulationSnapshots(data)) {
      return (
        <section className="space-y-3">
          {heading}
          <ChartEmptyState
            message={`No full capture yet—all ${expectedBoardCount} leaderboards must sync in the same run. Partial captures are hidden.`}
          />
        </section>
      );
    }

    return (
      <section className="space-y-3">
        {heading}
        {latestCompleteKpi != null && (
          <dl className="flex flex-wrap gap-6 rounded border border-border bg-secondary/20 px-4 py-3 text-sm">
            <div>
              <dt className="text-xs text-muted-foreground">Latest total (full sync)</dt>
              <dd className="font-mono text-lg text-foreground">
                {latestCompleteKpi.total.toLocaleString()}
              </dd>
            </div>
            {latestCompleteKpi.delta != null && (
              <div>
                <dt className="text-xs text-muted-foreground">Since prior full sync</dt>
                <dd className="font-mono text-lg text-foreground">
                  {formatPopulationDelta(latestCompleteKpi.delta)}
                </dd>
              </div>
            )}
          </dl>
        )}
        <LeaderboardEntryCountTrendChart points={chartPoints} />
      </section>
    );
  }

  return (
    <div className="space-y-8">
      {renderTotalEntriesSection()}

      {boardRows.length > 0 && (
        <section className="space-y-3">
          <div>
            <h2 className="steam-section-heading">Population by leaderboard</h2>
            <p className="text-xs text-muted-foreground">
              Ranked player count on each board. Select up to {MAX_COMPARE_BOARDS} boards to
              compare trends.
            </p>
          </div>
          <div className="overflow-x-auto rounded border border-border">
            <table className="w-full min-w-[32rem] text-sm">
              <thead>
                <tr className="border-b border-border text-left text-xs text-muted-foreground">
                  <th className="px-3 py-2 font-medium">Compare</th>
                  <th className="px-3 py-2 font-medium">Leaderboard</th>
                  <th className="px-3 py-2 text-right font-medium">Players</th>
                  <th className="px-3 py-2 text-right font-medium">Since last sync</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {boardRows.map((row) => {
                  const selected = compareIds.includes(row.raLeaderboardId);
                  const atCap = !selected && compareIds.length >= MAX_COMPARE_BOARDS;
                  return (
                    <tr key={row.raLeaderboardId}>
                      <td className="px-3 py-2">
                        <input
                          type="checkbox"
                          checked={selected}
                          disabled={atCap}
                          onChange={() => toggleCompare(row.raLeaderboardId)}
                          aria-label={`Compare ${row.title}`}
                          className="size-4 rounded border-border"
                        />
                      </td>
                      <td className="px-3 py-2">
                        <Link
                          href={`/leaderboards/${row.raLeaderboardId}`}
                          className="font-medium hover:text-[var(--accent-retro)]"
                        >
                          {row.title}
                        </Link>
                      </td>
                      <td className="px-3 py-2 text-right font-mono">
                        {row.latestEntryCount.toLocaleString()}
                      </td>
                      <td className="px-3 py-2 text-right font-mono text-muted-foreground">
                        {row.entryCountDelta == null ? (
                          "—"
                        ) : row.entryCountDelta === 0 ? (
                          "0"
                        ) : (
                          <span className="text-foreground">
                            {formatPopulationDelta(row.entryCountDelta)}
                          </span>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {compareIds.length > 0 && (
            <div className="min-w-0 h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
              {perBoardChartData.length >= 2 ? (
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart
                    data={perBoardChartData}
                    margin={{ top: 8, right: 4, left: 0, bottom: 0 }}
                  >
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
                    <Legend wrapperStyle={{ flexWrap: "wrap", paddingTop: 8 }} />
                    {compareBoards.map((board, index) => (
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
              ) : (
                <ChartEmptyState message="Need at least two syncs to compare board population trends." />
              )}
            </div>
          )}
        </section>
      )}
    </div>
  );
}
