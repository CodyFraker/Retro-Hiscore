"use client";

import { useState } from "react";
import Link from "next/link";
import type {
  DashboardAchievementActivityItemDto,
  GameAchievementDistributionResponse,
  GameAchievementsResponse,
} from "@/generated/api-client";
import { AchievementsActivityFeed } from "@/components/achievements/achievements-activity-feed";
import { buildAchievementsQueryString } from "@/lib/achievements-params";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import { GameAchievementDistributionChart } from "@/components/game/game-achievement-distribution-chart";
import { GameAchievementGrid } from "@/components/game/game-achievement-grid";
import { GameAchievementSummary } from "@/components/game/game-achievement-summary";

type Props = {
  achievements: GameAchievementsResponse;
  distribution: GameAchievementDistributionResponse;
  recentUnlockActivity: DashboardAchievementActivityItemDto[];
  raGameId: number;
};

export function GameAchievementsSection({
  achievements,
  distribution,
  recentUnlockActivity,
  raGameId,
}: Props) {
  const [distMode, setDistMode] = useState<"softcore" | "hardcore">("softcore");

  return (
    <div className="space-y-10">
      <section className="space-y-3">
        <div className="flex flex-wrap items-baseline justify-between gap-2">
          <h2 className="steam-section-heading">Friend set progress</h2>
          <LastSyncedLabel at={achievements.achievementProgressSyncedAt} />
        </div>
        <GameAchievementSummary
          members={achievements.members}
          summaries={achievements.memberSummaries}
          membersMastered={achievements.membersMastered}
        />
      </section>

      <section className="space-y-3">
        <div className="flex flex-wrap items-baseline justify-between gap-2">
          <h2 className="steam-section-heading">Recent friend unlocks</h2>
          {recentUnlockActivity.length > 0 ? (
            <Link
              href={`/achievements${buildAchievementsQueryString({ game: raGameId })}`}
              className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]"
            >
              View all →
            </Link>
          ) : null}
        </div>
        <AchievementsActivityFeed
          items={recentUnlockActivity}
          variant="embedded"
          showGameLink={false}
        />
      </section>

      <section className="space-y-3">
        <h2 className="steam-section-heading">Achievement grid</h2>
        <GameAchievementGrid
          achievements={achievements.achievements}
          members={achievements.members}
          unlocks={achievements.unlocks}
        />
      </section>

      <section className="space-y-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h2 className="steam-section-heading">Global mastery</h2>
          <div className="flex gap-1 rounded-md border border-border p-0.5 text-xs">
            <button
              type="button"
              className={`rounded px-2 py-1 ${distMode === "softcore" ? "bg-secondary" : ""}`}
              onClick={() => setDistMode("softcore")}
            >
              All unlocks
            </button>
            <button
              type="button"
              className={`rounded px-2 py-1 ${distMode === "hardcore" ? "bg-secondary" : ""}`}
              onClick={() => setDistMode("hardcore")}
            >
              Hardcore
            </button>
          </div>
        </div>
        <LastSyncedLabel at={distribution.syncedAt} prefix="Distribution last synced" />
        <GameAchievementDistributionChart
          softcoreBuckets={distribution.softcoreBuckets}
          hardcoreBuckets={distribution.hardcoreBuckets}
          memberSummaries={achievements.memberSummaries}
          mode={distMode}
        />
      </section>
    </div>
  );
}
