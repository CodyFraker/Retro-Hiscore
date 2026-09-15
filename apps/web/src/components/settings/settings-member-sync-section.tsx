"use client";

import { RefreshCw } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import { Button } from "@/components/ui/button";
import type { MemberSelfSyncStatusDto } from "@/generated/api-client";
import {
  triggerMemberSelfSyncAchievementsAction,
  triggerMemberSelfSyncAllAction,
  triggerMemberSelfSyncLeaderboardsAction,
  triggerMemberSelfSyncProfileAction,
} from "@/lib/actions/member-sync";
import { formatSyncTime } from "@/lib/format";

type Props = {
  hasApiKey: boolean;
  status: MemberSelfSyncStatusDto;
};

function cooldownMessage(until?: string | null): string | null {
  if (!until) {
    return null;
  }
  const at = new Date(until);
  if (at.getTime() <= Date.now()) {
    return null;
  }
  return `Available after ${formatSyncTime(until)}`;
}

type RowProps = {
  title: string;
  description: string;
  lastSyncedAt?: string | null;
  cooldownUntil?: string | null;
  pending: boolean;
  onSync: () => void;
};

function SyncRow({
  title,
  description,
  lastSyncedAt,
  cooldownUntil,
  pending,
  onSync,
}: RowProps) {
  const cooldown = cooldownMessage(cooldownUntil);

  return (
    <div className="flex flex-col gap-3 border-b border-border py-4 last:border-b-0 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-w-0 space-y-1">
        <p className="font-medium">{title}</p>
        <p className="text-sm text-muted-foreground">{description}</p>
        <LastSyncedLabel at={lastSyncedAt} />
        {cooldown ? <p className="text-xs text-amber-600 dark:text-amber-400">{cooldown}</p> : null}
      </div>
      <Button
        type="button"
        variant="secondary"
        size="sm"
        className="shrink-0"
        disabled={pending || Boolean(cooldown)}
        onClick={onSync}
      >
        <RefreshCw className={pending ? "animate-spin" : undefined} />
        {pending ? "Syncing…" : "Sync"}
      </Button>
    </div>
  );
}

export function SettingsMemberSyncSection({ hasApiKey, status }: Props) {
  const router = useRouter();
  const [message, setMessage] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  if (!hasApiKey) {
    return (
      <section className="space-y-2 rounded border border-border p-5">
        <h2 className="text-lg font-medium">Sync your data</h2>
        <p className="text-sm text-muted-foreground">
          Add your RetroAchievements username and API key above to refresh your scores, profile, and achievements
          on demand.
        </p>
      </section>
    );
  }

  function run(action: () => Promise<{ ok: boolean; message: string }>) {
    startTransition(async () => {
      const result = await action();
      setMessage(result.message);
      if (result.ok) {
        router.refresh();
      }
    });
  }

  return (
    <section className="space-y-4 rounded border border-border p-5">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="space-y-1">
          <h2 className="text-lg font-medium">Sync your data</h2>
          <p className="text-sm text-muted-foreground">
            Pull your latest RetroAchievements leaderboards, profile, and tracked-game achievements. Site-wide catalog
            syncs are admin-only.
          </p>
        </div>
        <Button
          type="button"
          disabled={pending}
          onClick={() => run(triggerMemberSelfSyncAllAction)}
        >
          <RefreshCw className={pending ? "animate-spin" : undefined} />
          Sync all
        </Button>
      </div>

      <SyncRow
        title="Leaderboards"
        description="Queue score refresh on every tracked game (uses your API key)."
        lastSyncedAt={status.leaderboards.lastSyncedAt}
        cooldownUntil={status.leaderboards.cooldownUntil}
        pending={pending}
        onSync={() => run(triggerMemberSelfSyncLeaderboardsAction)}
      />
      <SyncRow
        title="Profile"
        description="Site rank, presence, and recently played games."
        lastSyncedAt={status.profile.lastSyncedAt}
        cooldownUntil={status.profile.cooldownUntil}
        pending={pending}
        onSync={() => run(triggerMemberSelfSyncProfileAction)}
      />
      <SyncRow
        title="Achievements"
        description="Your unlock progress on games this group tracks."
        lastSyncedAt={status.achievements.lastSyncedAt}
        cooldownUntil={status.achievements.cooldownUntil}
        pending={pending}
        onSync={() => run(triggerMemberSelfSyncAchievementsAction)}
      />

      {message ? <p className="text-sm text-muted-foreground">{message}</p> : null}
      <p className="text-xs text-muted-foreground">
        Per-game refresh is also available on each{" "}
        <Link href="/games" className="text-[var(--accent-retro)] hover:underline">
          game page
        </Link>
        .
      </p>
    </section>
  );
}
