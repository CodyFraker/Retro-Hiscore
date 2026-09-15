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
import { GameRankTrendSection } from "@/components/game/game-rank-trend-section";
import { MemberAvatar } from "@/components/members/member-avatar";
import type {
  GameDelta,
  GameHistoryItemDto,
  GameLeaderboardDto,
  StandingMemberDto,
} from "@/generated/api-client";
import { chartYDomain } from "@/lib/chart-y-domain";
import {
  countDistinctSyncTimestampsForMemberLeads,
  memberLeadSeriesHasVariation,
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
  leaderboards: GameLeaderboardDto[];
  members: StandingMemberDto[];
  deltas: GameDelta[];
  defaultMemberId?: string | null;
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

export function GameTrendCharts({
  items,
  leaderboards,
  members,
  deltas,
  defaultMemberId,
}: Props) {
  const memberLeadSeries = useMemo(() => toMemberLeadSeries(items), [items]);

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

  const leadYDomain = useMemo(() => {
    const values: number[] = [];
    for (const row of leadChartData) {
      for (const member of memberLeadSeries) {
        const value = row[memberSeriesKey(member.memberId)];
        if (typeof value === "number") {
          values.push(value);
        }
      }
    }
    return chartYDomain(values, "tight");
  }, [leadChartData, memberLeadSeries]);

  const hasLeadTrend =
    countDistinctSyncTimestampsForMemberLeads(memberLeadSeries) >= 2 &&
    memberLeadSeriesHasVariation(memberLeadSeries);

  return (
    <div className="space-y-10">
      <section className="space-y-3">
        <div>
          <h2 className="steam-section-heading">Board leads over time</h2>
          <p className="text-xs text-muted-foreground">
            How many leaderboards each member leads among friends at each sync.
          </p>
        </div>
        {hasLeadTrend ? (
          <div className="space-y-2">
            <ul className="flex flex-wrap gap-3 px-1 text-xs text-muted-foreground">
              {memberLeadSeries.map((member, index) => (
                <li key={member.memberId} className="inline-flex items-center gap-1.5">
                  <MemberAvatar
                    avatarUrl={member.avatarUrl}
                    displayName={member.displayName}
                    size={18}
                  />
                  <span style={{ color: CHART_COLORS[index % CHART_COLORS.length] }}>
                    {member.displayName}
                  </span>
                </li>
              ))}
            </ul>
            <div className="min-w-0 h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
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
                    domain={leadYDomain}
                    allowDataOverflow
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
                      const member = memberLeadSeries.find(
                        (entry) => memberSeriesKey(entry.memberId) === String(item?.dataKey),
                      );
                      return [value, member?.displayName ?? "Friend #1 boards"];
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
                      />
                    );
                  })}
                </LineChart>
              </ResponsiveContainer>
            </div>
          </div>
        ) : (
          <ChartEmptyState message="No change in board leads between syncs yet." />
        )}
      </section>

      <GameRankTrendSection
        items={items}
        leaderboards={leaderboards}
        members={members}
        deltas={deltas}
        defaultMemberId={defaultMemberId}
      />
    </div>
  );
}
