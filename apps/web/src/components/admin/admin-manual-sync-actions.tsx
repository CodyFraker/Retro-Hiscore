"use client";

import { ImageIcon, RefreshCw, Users, Activity } from "lucide-react";
import Link from "next/link";
import { useState } from "react";
import { AdminSyncActionRow } from "@/components/admin/admin-sync-action-row";
import { AdminSyncTriggerButton } from "@/components/admin/admin-sync-trigger-button";
import type { AdminLastSyncByKindDto } from "@/generated/api-client";
import {
  triggerConsoleIconSyncAction,
  triggerDueDispatchSyncAction,
  triggerMemberActivitySyncAction,
  triggerMemberRankSyncAction,
  triggerMemberAchievementSyncAction,
  triggerMetadataSyncAction,
  triggerScoreSyncAction,
} from "@/lib/actions/sync";
import { FormattedLastSyncRun } from "@/components/formatted-last-sync-run";
import { findLastSyncByKind } from "@/lib/sync-last-run";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";

type Props = {
  lastSyncByKind: AdminLastSyncByKindDto[];
  scoreRefreshDisabledReason?: string | null;
};

export function AdminManualSyncActions({ lastSyncByKind, scoreRefreshDisabledReason }: Props) {
  const [forceConsoleIcons, setForceConsoleIcons] = useState(false);
  const [rowMessage, setRowMessage] = useState<string | null>(null);

  const lastRun = (kind: string) => (
    <FormattedLastSyncRun entry={findLastSyncByKind(lastSyncByKind, kind)} />
  );

  const leaderboardDisabled = Boolean(scoreRefreshDisabledReason);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Manual sync</CardTitle>
        <CardDescription>
          Trigger sync jobs on demand. Per-game metadata and score refresh is available under{" "}
          <Link href="/admin/games" className="text-[var(--accent-retro)] hover:underline">
            Admin → Games
          </Link>
          .
        </CardDescription>
      </CardHeader>
      <CardContent className="pt-0">
        <p className="steam-section-heading mb-2 pt-2">Leaderboards</p>
        <AdminSyncActionRow
          title="Refresh scores (all games)"
          description="Enqueue a full leaderboard sync for every tracked game."
          lastRun={lastRun("LeaderboardScores")}
          footerMessage={leaderboardDisabled ? scoreRefreshDisabledReason : rowMessage}
          actions={
            <AdminSyncTriggerButton
              label="Refresh scores"
              pendingLabel="Refreshing…"
              icon={RefreshCw}
              disabled={leaderboardDisabled}
              onTrigger={triggerScoreSyncAction}
              onMessage={setRowMessage}
            />
          }
        />
        <AdminSyncActionRow
          title="Sync due games only"
          description="Enqueue leaderboard sync only for games that are due (same as the scheduled dispatcher)."
          lastRun={lastRun("LeaderboardScores")}
          footerMessage={leaderboardDisabled ? scoreRefreshDisabledReason : undefined}
          actions={
            <AdminSyncTriggerButton
              label="Sync due games"
              pendingLabel="Queueing…"
              icon={RefreshCw}
              variant="secondary"
              disabled={leaderboardDisabled}
              onTrigger={triggerDueDispatchSyncAction}
            />
          }
        />
        <p className="steam-section-heading mb-2 pt-4">Members</p>
        <AdminSyncActionRow
          title="Sync member activity"
          description="Refresh recently played games for all members and update the game track queue."
          lastRun={lastRun("MemberActivity")}
          actions={
            <AdminSyncTriggerButton
              label="Sync activity"
              pendingLabel="Syncing…"
              icon={Activity}
              variant="secondary"
              onTrigger={triggerMemberActivitySyncAction}
            />
          }
        />
        <AdminSyncActionRow
          title="Sync member achievements"
          description="Refresh achievement catalog and unlocks for all members on every tracked game."
          lastRun={lastRun("MemberAchievements")}
          actions={
            <AdminSyncTriggerButton
              label="Sync achievements"
              pendingLabel="Syncing…"
              icon={Activity}
              variant="secondary"
              onTrigger={triggerMemberAchievementSyncAction}
            />
          }
        />
        <AdminSyncActionRow
          title="Sync member RA ranks"
          description="Refresh RetroAchievements site rank snapshots used in championship standings."
          lastRun={lastRun("MemberRank")}
          actions={
            <AdminSyncTriggerButton
              label="Sync ranks"
              pendingLabel="Syncing…"
              icon={Users}
              variant="secondary"
              onTrigger={triggerMemberRankSyncAction}
            />
          }
        />
        <p className="steam-section-heading mb-2 pt-4">Catalog</p>
        <AdminSyncActionRow
          title="Refresh game art"
          description="Update titles, box art, and metadata for all tracked games from RetroAchievements."
          lastRun={lastRun("GameMetadata")}
          actions={
            <AdminSyncTriggerButton
              label="Refresh game art"
              pendingLabel="Updating art…"
              icon={ImageIcon}
              variant="secondary"
              onTrigger={triggerMetadataSyncAction}
            />
          }
        />
        <AdminSyncActionRow
          title="Refresh console icons"
          description="Download system icons for consoles used by tracked games and recent activity."
          lastRun={lastRun("ConsoleIcons")}
          actions={
            <div className="flex flex-col items-stretch gap-2 sm:items-end">
              <label className="flex items-center gap-2 text-xs font-normal text-muted-foreground">
                <input
                  type="checkbox"
                  checked={forceConsoleIcons}
                  onChange={(event) => setForceConsoleIcons(event.target.checked)}
                  className="size-4 rounded border-border"
                />
                Force re-download
              </label>
              <AdminSyncTriggerButton
                label="Refresh console icons"
                pendingLabel="Syncing icons…"
                variant="secondary"
                onTrigger={() => triggerConsoleIconSyncAction(forceConsoleIcons)}
              />
            </div>
          }
        />
      </CardContent>
      <CardFooter className="text-xs text-muted-foreground">
        Leaderboard manual refresh and due-only dispatch share the same cooldown.
      </CardFooter>
    </Card>
  );
}
