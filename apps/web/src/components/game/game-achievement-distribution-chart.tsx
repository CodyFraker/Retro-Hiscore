"use client";

import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { ChartEmptyState } from "@/components/charts/chart-empty-state";
import type {
  AchievementDistributionBucketDto,
  GameAchievementMemberSummaryDto,
} from "@/generated/api-client";

type Props = {
  softcoreBuckets: AchievementDistributionBucketDto[];
  hardcoreBuckets: AchievementDistributionBucketDto[];
  memberSummaries: GameAchievementMemberSummaryDto[];
  mode: "softcore" | "hardcore";
};

export function GameAchievementDistributionChart({
  softcoreBuckets,
  hardcoreBuckets,
  memberSummaries,
  mode,
}: Props) {
  const buckets = mode === "hardcore" ? hardcoreBuckets : softcoreBuckets;

  const chartData = buckets.map((b) => ({
    label: String(b.achievementsEarned),
    players: b.playerCount,
    achievementsEarned: b.achievementsEarned,
  }));

  const friendMarkers = memberSummaries
    .filter((s) => s.achievementsEarned > 0)
    .map((s) => s.achievementsEarned);

  if (chartData.length === 0) {
    return (
      <ChartEmptyState message="Global mastery distribution is not synced yet. Refresh game metadata or scores." />
    );
  }

  return (
    <div className="space-y-2">
      <p className="text-xs text-muted-foreground">
        RA players by unlock count ({mode}). Friend progress:{" "}
        {friendMarkers.length > 0 ? friendMarkers.join(", ") : "none synced"}.
      </p>
      <div className="h-56 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={chartData} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="label" tick={{ fontSize: 11 }} label={{ value: "Unlocks", position: "insideBottom", offset: -2, fontSize: 11 }} />
            <YAxis tick={{ fontSize: 11 }} width={40} />
            <Tooltip
              formatter={(value) => [
                typeof value === "number" ? value.toLocaleString() : String(value ?? ""),
                "Players",
              ]}
              labelFormatter={(label) => `${label} achievements earned`}
            />
            <Bar dataKey="players" fill="var(--chart-2)" radius={[2, 2, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
