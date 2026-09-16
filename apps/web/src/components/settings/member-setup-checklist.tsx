import { Check } from "lucide-react";
import type { MemberSelfSyncStatusDto } from "@/generated/api-client";
import { cn } from "@/lib/utils";

type Props = {
  needsOnboarding: boolean;
  hasApiKey: boolean;
  syncStatus: MemberSelfSyncStatusDto | null;
  emphasizeSync?: boolean;
};

function hasCompletedSync(status: MemberSelfSyncStatusDto | null): boolean {
  if (!status) {
    return false;
  }
  return Boolean(
    status.leaderboards.lastSyncedAt
      || status.profile.lastSyncedAt
      || status.achievements.lastSyncedAt,
  );
}

function StepRow({ done, label, detail }: { done: boolean; label: string; detail?: string }) {
  return (
    <li className="flex gap-3 text-sm">
      <span
        className={cn(
          "mt-0.5 flex size-5 shrink-0 items-center justify-center rounded-full border",
          done
            ? "border-[var(--accent-retro)] bg-[var(--accent-retro)]/15 text-[var(--accent-retro)]"
            : "border-border text-muted-foreground",
        )}
        aria-hidden
      >
        {done ? <Check className="size-3" /> : null}
      </span>
      <div>
        <p className={done ? "text-foreground" : "text-muted-foreground"}>{label}</p>
        {detail ? <p className="text-xs text-muted-foreground">{detail}</p> : null}
      </div>
    </li>
  );
}

export function MemberSetupChecklist({
  needsOnboarding,
  hasApiKey,
  syncStatus,
  emphasizeSync = false,
}: Props) {
  const discordDone = true;
  const raDone = !needsOnboarding && hasApiKey;
  const syncDone = hasCompletedSync(syncStatus);

  return (
    <section
      className={cn(
        "rounded border border-border p-5",
        emphasizeSync && !syncDone ? "border-[var(--accent-retro)]/40 bg-[var(--accent-retro)]/5" : "",
      )}
    >
      <h2 className="text-sm font-semibold">Getting started</h2>
      <ol className="mt-4 space-y-4">
        <StepRow done={discordDone} label="Sign in with Discord" />
        <StepRow
          done={raDone}
          label="Link RetroAchievements username and API key"
          detail={needsOnboarding ? "Complete the form below." : undefined}
        />
        <StepRow
          done={syncDone}
          label="Run your first sync"
          detail={
            !syncDone && raDone
              ? "Use the sync buttons below after saving your API key. Group scores may take a scheduled sync."
              : undefined
          }
        />
      </ol>
    </section>
  );
}
