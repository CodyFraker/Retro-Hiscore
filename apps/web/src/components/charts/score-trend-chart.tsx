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
import { chartYDomain } from "@/lib/chart-y-domain";
import type { MemberSeries } from "@/lib/history-series";
import { countDistinctSyncTimestamps } from "@/lib/history-series";
import {
  formatLeaderboardScore,
  isTimeLeaderboardFormat,
} from "@/lib/leaderboard-score-format";
import { ChartEmptyState } from "@/components/charts/chart-empty-state";
import { MemberAvatar } from "@/components/members/member-avatar";

const CHART_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

type Props = {
  series: MemberSeries[];
  scoreFormat?: string | null;
};

function formatTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

export function ScoreTrendChart({ series, scoreFormat }: Props) {
  const [hidden, setHidden] = useState<Record<string, boolean>>({});
  const timeFormat = isTimeLeaderboardFormat(scoreFormat);

  const formattedScoreByKey = useMemo(() => {
    const map = new Map<string, string>();
    for (const member of series) {
      for (const point of member.points) {
        map.set(`${point.syncedAt}:${member.memberId}`, point.formattedScore);
      }
    }
    return map;
  }, [series]);

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

  const yDomain = useMemo(() => {
    if (!timeFormat) {
      return undefined;
    }
    const values: number[] = [];
    for (const row of chartData) {
      for (const member of series) {
        const value = row[member.memberId];
        if (typeof value === "number") {
          values.push(value);
        }
      }
    }
    return chartYDomain(values, "tight");
  }, [chartData, series, timeFormat]);

  if (countDistinctSyncTimestamps(series) < 2) {
    return <ChartEmptyState />;
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
            width={timeFormat ? 64 : 56}
            domain={yDomain}
            allowDataOverflow={timeFormat}
            tickFormatter={(value) =>
              timeFormat
                ? formatLeaderboardScore(Number(value), scoreFormat)
                : Number(value).toLocaleString()
            }
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
              const formatted =
                formattedScoreByKey.get(`${syncedAt}:${memberId}`) ??
                (typeof value === "number"
                  ? formatLeaderboardScore(value, scoreFormat)
                  : String(value ?? ""));
              const member = series.find((entry) => entry.memberId === memberId);
              return [formatted, member?.displayName ?? "Score"];
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
