"use client";

import { ApiKeyForm } from "@/components/settings/api-key-form";
import { RaAccountForm } from "@/components/settings/ra-account-form";
import { leaderboardSyncTierDescription } from "@/lib/leaderboard-sync-tier";
import type { GameLeaderboardSyncStatusDto } from "@/generated/api-client";

type Props = {
  onboardingStep: string;
  raUsername: string;
  hasApiKey: boolean;
};

const SAMPLE_SYNC_STATUS: GameLeaderboardSyncStatusDto = {
  tier: "Hot",
  leaderboardSyncForcedCold: false,
  leaderboardSyncIntervalMinutes: 15,
  leaderboardSyncIsDue: false,
  leaderboardSyncNextDueAt: null,
  groupLastPlayedAt: null,
};

function stepIndex(step: string): number {
  if (step === "NeedsRaAccount") {
    return 1;
  }
  if (step === "NeedsApiKey") {
    return 2;
  }
  return 3;
}

export function SettingsOnboardingWizard({ onboardingStep, raUsername, hasApiKey }: Props) {
  const current = stepIndex(onboardingStep);

  return (
    <div className="space-y-6">
      <ol className="flex flex-wrap gap-2 text-xs font-medium uppercase tracking-wide text-muted-foreground">
        <li className={current >= 1 ? "text-[var(--accent-retro)]" : ""}>1. RA account</li>
        <li className={current >= 2 ? "text-[var(--accent-retro)]" : ""}>2. API key</li>
        <li className={current >= 3 ? "text-[var(--accent-retro)]" : ""}>3. How sync works</li>
      </ol>

      {onboardingStep === "NeedsRaAccount" ? (
        <section className="space-y-4 rounded border border-border p-5">
          <p className="text-sm text-muted-foreground">
            Link the RetroAchievements account you use with this Discord login. You need your site API key
            from{" "}
            <a
              href="https://retroachievements.org/controlpanel.php"
              target="_blank"
              rel="noopener noreferrer"
              className="text-[var(--accent-retro)] hover:underline"
            >
              RetroAchievements → Settings → Keys
            </a>
            .
          </p>
          <RaAccountForm />
        </section>
      ) : null}

      {onboardingStep === "NeedsApiKey" ? (
        <section className="space-y-4 rounded border border-border p-5">
          <p className="text-sm text-muted-foreground">
            Add your RetroAchievements API key so the site can refresh your scores on demand. Group
            leaderboards still sync on a shared schedule when you are not syncing manually.
          </p>
          <ApiKeyForm hasApiKey={hasApiKey} raUsername={raUsername} />
        </section>
      ) : null}

      {onboardingStep === "Complete" ? null : (
        <section className="space-y-2 rounded border border-border bg-muted/20 p-5 text-sm text-muted-foreground">
          <h3 className="font-medium text-foreground">What sync does</h3>
          <p>
            Tracked games use hot and cold leaderboard schedules based on group play.{" "}
            {leaderboardSyncTierDescription(SAMPLE_SYNC_STATUS)}
          </p>
          <p>
            After onboarding, use Settings → Sync to refresh your profile, achievements, and scores across
            every tracked game.
          </p>
        </section>
      )}
    </div>
  );
}
