"use client";

import { Card, CardContent } from "@/components/ui/card";
import type { MemberRaAchievementHistoryItemDto } from "@/generated/api-client";
import { summarizeAchievementActivity } from "@/lib/member-ra-achievement-history";

type Props = {
  items: MemberRaAchievementHistoryItemDto[];
};

function StatTile({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <Card size="sm" className="min-w-0 flex-1">
      <CardContent className="space-y-1">
        <p className="text-xs uppercase tracking-wide text-muted-foreground">{label}</p>
        <p className="font-mono text-lg font-medium tabular-nums">{value}</p>
        {hint ? <p className="text-xs text-muted-foreground">{hint}</p> : null}
      </CardContent>
    </Card>
  );
}

export function MemberRaAchievementActivitySummary({ items }: Props) {
  const summary = summarizeAchievementActivity(items);

  if (items.length === 0) {
    return null;
  }

  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
      <StatTile
        label="Total unlocks"
        value={summary.totalUnlocks.toLocaleString()}
        hint="In loaded history"
      />
      <StatTile
        label="Unlocks (30 days)"
        value={summary.unlocksLast30Days.toLocaleString()}
      />
      <StatTile
        label="HC points (30 days)"
        value={summary.pointsEarnedLast30Days.toLocaleString()}
      />
    </div>
  );
}
