import { ApiKeyForm } from "@/components/settings/api-key-form";
import { MemberSetupChecklist } from "@/components/settings/member-setup-checklist";
import { SettingsOnboardingWizard } from "@/components/settings/settings-onboarding-wizard";
import { SettingsMemberSyncSection } from "@/components/settings/settings-member-sync-section";
import { SyncHealthAlert } from "@/components/settings/sync-health-alert";
import { PageHero } from "@/components/layout/page-hero";
import { ThemeSettingsSection } from "@/components/settings/theme-settings-section";
import type { MemberSelfSyncStatusDto } from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function SettingsPage() {
  let linked = false;
  let raUsername = "";
  let hasApiKey = false;
  let needsOnboarding = false;
  let onboardingStep = "Complete";
  let syncStatus: MemberSelfSyncStatusDto | null = null;
  let syncHealth: Awaited<ReturnType<Awaited<ReturnType<typeof getServerApiClient>>["getSyncHealth"]>> | null =
    null;
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    const member = await api.getCurrentMember();
    linked = true;
    raUsername = member.raUsername ?? "";
    hasApiKey = member.hasApiKey ?? false;
    needsOnboarding = member.needsOnboarding ?? false;
    onboardingStep = member.onboardingStep ?? "Complete";
    if (!needsOnboarding) {
      try {
        syncStatus = await api.getMemberSelfSyncStatus();
      } catch {
        syncStatus = null;
      }
    }
    try {
      syncHealth = await api.getSyncHealth();
    } catch {
      syncHealth = null;
    }
  } catch (err) {
    if (err instanceof Error && !err.message.includes("404")) {
      error = err.message;
    }
  }

  return (
    <div className="mx-auto max-w-xl space-y-8">
      <PageHero
        title="Settings"
        description="Link your RetroAchievements account so your leaderboard scores stay in sync."
      />

      <ThemeSettingsSection syncToAccount={linked && !error} />

      {error && (
        <p className="rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">
          {error}
        </p>
      )}

      {!error && !linked && (
        <p className="text-muted-foreground">
          Your Discord account is not registered on this site. Contact an administrator for an invite.
        </p>
      )}

      {!error && linked && needsOnboarding && (
        <>
          <MemberSetupChecklist
            needsOnboarding={needsOnboarding}
            hasApiKey={hasApiKey}
            syncStatus={syncStatus}
          />
          <SettingsOnboardingWizard
            onboardingStep={onboardingStep}
            raUsername={raUsername}
            hasApiKey={hasApiKey}
          />
        </>
      )}

      {!error && linked && !needsOnboarding && (
        <>
          {syncHealth ? <SyncHealthAlert health={syncHealth} /> : null}
          <MemberSetupChecklist
            needsOnboarding={needsOnboarding}
            hasApiKey={hasApiKey}
            syncStatus={syncStatus}
            emphasizeSync
          />
          <section className="space-y-4 rounded border border-border p-5">
            <div>
              <p className="text-sm text-muted-foreground">Linked RetroAchievements account</p>
              <p className="font-mono text-lg">@{raUsername}</p>
            </div>
            <ApiKeyForm hasApiKey={hasApiKey} raUsername={raUsername} />
          </section>
          {syncStatus ? (
            <SettingsMemberSyncSection hasApiKey={hasApiKey} status={syncStatus} />
          ) : null}
        </>
      )}
    </div>
  );
}
