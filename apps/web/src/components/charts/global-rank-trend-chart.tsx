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
import type { LeaderboardHistoryItemDto } from "@/generated/api-client";
import { formatGlobalRank } from "@/lib/format-global-rank";
import type { MemberSeries } from "@/lib/history-series";
import {
  countDistinctSyncTimestamps,
  memberSeriesHasGlobalRankVariation,
} from "@/lib/history-series";

const CHART_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

type Props = {
  series: MemberSeries[];
  historyItems: LeaderboardHistoryItemDto[];
};

function formatTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

export function GlobalRankTrendChart({ series, historyItems }: Props) {
  const [hidden, setHidden] = useState<Record<string, boolean>>({});

  const globalRankContextByKey = useMemo(() => {
    const map = new Map<string, { rank: number; entryCount: number | null }>();
    for (const item of historyItems) {
      if (item.globalRank == null) {
        continue;
      }
      map.set(`${item.syncedAt}:${item.memberId}`, {
        rank: item.globalRank,
        entryCount: item.globalEntryCount ?? null,
      });
    }
    return map;
  }, [historyItems]);

  const chartData = useMemo(() => {
    const byTime = new Map<string, Record<string, string | number>>();
    for (const member of series) {
      for (const point of member.points) {
        if (point.globalRank == null) {
          continue;
        }
        const row = byTime.get(point.syncedAt) ?? { syncedAt: point.syncedAt };
        row[member.memberId] = point.globalRank;
        byTime.set(point.syncedAt, row);
      }
    }
    return [...byTime.values()].sort(
      (a, b) =>
        new Date(String(a.syncedAt)).getTime() - new Date(String(b.syncedAt)).getTime(),
    );
  }, [series]);

  const maxGlobalRank = useMemo(() => {
    let max = 1;
    for (const member of series) {
      for (const point of member.points) {
        if (point.globalRank != null && point.globalRank > max) {
          max = point.globalRank;
        }
      }
    }
    return max;
  }, [series]);

  if (countDistinctSyncTimestamps(series) < 2) {
    return <ChartEmptyState />;
  }

  if (!memberSeriesHasGlobalRankVariation(series)) {
    return (
      <ChartEmptyState message="No global rank movement yet. Charts unlock after rank changes across syncs." />
    );
  }

  return (
    <div className="min-w-0 space-y-2">
      <ul className="flex flex-wrap gap-3 px-1 text-xs text-muted-foreground">
        {series.map((member, index) => (
          <li key={member.memberId} className="inline-flex items-center gap-1.5">
            <MemberAvatar avatarUrl={member.avatarUrl} displayName={member.displayName} size={18} />
            <span style={{ color: CHART_COLORS[index % CHART_COLORS.length] }}>{member.displayName}</span>
          </li>
        ))}
      </ul>
      <div className="min-w-0 h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
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
            <YAxis
              stroke="var(--muted-foreground)"
              fontSize={11}
              width={48}
              reversed
              domain={[1, maxGlobalRank]}
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
              formatter={(value, _name, item) => {
                const memberId = String(item?.dataKey ?? "");
                const syncedAt = String(item?.payload?.syncedAt ?? "");
                const context = globalRankContextByKey.get(`${syncedAt}:${memberId}`);
                const rank = typeof value === "number" ? value : context?.rank;
                const formatted =
                  rank != null
                    ? formatGlobalRank(rank, context?.entryCount ?? null) ?? `#${rank}`
                    : "—";
                const member = series.find((entry) => entry.memberId === memberId);
                return [formatted, member?.displayName ?? "Global rank"];
              }}
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
    </div>
  );
}
