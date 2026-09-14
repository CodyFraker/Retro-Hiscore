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
import { MemberAvatar } from "@/components/members/member-avatar";
import type { GameHistoryItemDto } from "@/generated/api-client";
import {
  countDistinctSyncTimestampsForBoardRanks,
  countDistinctSyncTimestampsForMemberLeads,
  toBoardRankSeries,
  toMemberLeadSeries,
} from "@/lib/game-history-series";

const CHART_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

type Props = {
  items: GameHistoryItemDto[];
};

function formatTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

function memberSeriesKey(memberId: string) {
  return `member-${memberId}`;
}

function boardMemberSeriesKey(raLeaderboardId: number, memberId: string) {
  return `board-${raLeaderboardId}-${memberId}`;
}

export function GameTrendCharts({ items }: Props) {
  const memberLeadSeries = useMemo(() => toMemberLeadSeries(items), [items]);
  const boardRankSeries = useMemo(() => toBoardRankSeries(items), [items]);
  const [hiddenMembers, setHiddenMembers] = useState<Record<string, boolean>>({});
  const [hiddenBoards, setHiddenBoards] = useState<Record<string, boolean>>({});

  const leadChartData = useMemo(() => {
    const byTime = new Map<string, Record<string, string | number>>();
    for (const member of memberLeadSeries) {
      const key = memberSeriesKey(member.memberId);
      for (const point of member.points) {
        const row = byTime.get(point.syncedAt) ?? { syncedAt: point.syncedAt };
        row[key] = point.friendRankOnes;
        byTime.set(point.syncedAt, row);
      }
    }
    return [...byTime.values()].sort(
      (a, b) =>
        new Date(String(a.syncedAt)).getTime() - new Date(String(b.syncedAt)).getTime(),
    );
  }, [memberLeadSeries]);

  const rankChartData = useMemo(() => {
    const byTime = new Map<string, Record<string, string | number>>();
    for (const board of boardRankSeries) {
      const key = boardMemberSeriesKey(board.raLeaderboardId, board.memberId);
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
  }, [boardRankSeries]);

  const maxFriendRank = useMemo(() => {
    let max = 1;
    for (const board of boardRankSeries) {
      for (const point of board.points) {
        if (point.friendRank != null && point.friendRank > max) {
          max = point.friendRank;
        }
      }
    }
    return max;
  }, [boardRankSeries]);

  const hasLeadTrend = countDistinctSyncTimestampsForMemberLeads(memberLeadSeries) >= 2;
  const hasRankTrend = countDistinctSyncTimestampsForBoardRanks(boardRankSeries) >= 2;

  return (
    <div className="space-y-10">
      <section className="space-y-3">
        <h2 className="text-lg font-medium">Board leads over time</h2>
        {hasLeadTrend ? (
          <div className="space-y-2">
          <ul className="flex flex-wrap gap-3 px-1 text-xs text-muted-foreground">
            {memberLeadSeries.map((member, index) => (
              <li key={member.memberId} className="inline-flex items-center gap-1.5">
                <MemberAvatar avatarUrl={member.avatarUrl} displayName={member.displayName} size={18} />
                <span style={{ color: CHART_COLORS[index % CHART_COLORS.length] }}>{member.displayName}</span>
              </li>
            ))}
          </ul>
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
                <Legend
                  wrapperStyle={{ flexWrap: "wrap", paddingTop: 8 }}
                  onClick={(payload) => {
                    const id = String(payload.dataKey ?? "");
                    if (!id) return;
                    setHiddenMembers((prev) => ({ ...prev, [id]: !prev[id] }));
                  }}
                />
                {memberLeadSeries.map((member, index) => {
                  const key = memberSeriesKey(member.memberId);
                  return (
                    <Line
                      key={key}
                      type="monotone"
                      dataKey={key}
                      name={member.displayName}
                      stroke={CHART_COLORS[index % CHART_COLORS.length]}
                      strokeWidth={2}
                      dot={{ r: 3 }}
                      connectNulls
                      hide={Boolean(hiddenMembers[key])}
                    />
                  );
                })}
              </LineChart>
            </ResponsiveContainer>
          </div>
          </div>
        ) : (
          <ChartEmptyState />
        )}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-medium">Friend rank by board</h2>
        {hasRankTrend ? (
          <div className="space-y-2">
          <ul className="flex flex-wrap gap-3 px-1 text-xs text-muted-foreground">
            {boardRankSeries.map((board, index) => (
              <li key={`${board.raLeaderboardId}-${board.memberId}`} className="inline-flex items-center gap-1.5">
                <MemberAvatar avatarUrl={board.avatarUrl} displayName={board.displayName} size={18} />
                <span style={{ color: CHART_COLORS[index % CHART_COLORS.length] }}>
                  {board.displayName} — {board.leaderboardTitle}
                </span>
              </li>
            ))}
          </ul>
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
                    const board = boardRankSeries.find(
                      (item) =>
                        boardMemberSeriesKey(item.raLeaderboardId, item.memberId) === String(name),
                    );
                    const label = board
                      ? `${board.displayName} — ${board.leaderboardTitle}`
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
                {boardRankSeries.map((board, index) => {
                  const key = boardMemberSeriesKey(board.raLeaderboardId, board.memberId);
                  return (
                    <Line
                      key={key}
                      type="monotone"
                      dataKey={key}
                      name={`${board.displayName} — ${board.leaderboardTitle}`}
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
          </div>
        ) : (
          <ChartEmptyState />
        )}
      </section>
    </div>
  );
}
