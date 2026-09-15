import { ApiKeyForm } from "@/components/settings/api-key-form";
import { RaAccountForm } from "@/components/settings/ra-account-form";
import { SettingsMemberSyncSection } from "@/components/settings/settings-member-sync-section";
import { PageHero } from "@/components/layout/page-hero";
import type { MemberSelfSyncStatusDto } from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function SettingsPage() {
  let linked = false;
  let raUsername = "";
  let hasApiKey = false;
  let needsOnboarding = false;
  let syncStatus: MemberSelfSyncStatusDto | null = null;
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    const member = await api.getCurrentMember();
    linked = true;
    raUsername = member.raUsername ?? "";
    hasApiKey = member.hasApiKey ?? false;
    needsOnboarding = member.needsOnboarding ?? false;
    if (!needsOnboarding) {
      syncStatus = await api.getMemberSelfSyncStatus();
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
        <section className="space-y-4 rounded border border-border p-5">
          <p className="text-sm text-muted-foreground">
            Finish setup with your RetroAchievements username and API key.
          </p>
          <RaAccountForm />
        </section>
      )}

      {!error && linked && !needsOnboarding && (
        <>
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
