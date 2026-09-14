import { ApiKeyForm } from "@/components/settings/api-key-form";
import { RaAccountForm } from "@/components/settings/ra-account-form";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function SettingsPage() {
  let linked = false;
  let raUsername = "";
  let hasApiKey = false;
  let needsOnboarding = false;
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    const member = await api.getCurrentMember();
    linked = true;
    raUsername = member.raUsername ?? "";
    hasApiKey = member.hasApiKey ?? false;
    needsOnboarding = member.needsOnboarding ?? false;
  } catch (err) {
    if (err instanceof Error && !err.message.includes("404")) {
      error = err.message;
    }
  }

  return (
    <div className="mx-auto max-w-lg space-y-8">
      <section className="space-y-2">
        <h1 className="font-[family-name:var(--font-display)] text-3xl tracking-tight text-[var(--accent-retro)]">
          Settings
        </h1>
        <p className="text-muted-foreground">
          Link your RetroAchievements account so your leaderboard scores stay in sync.
        </p>
      </section>

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
        <section className="space-y-4 rounded border border-border p-5">
          <div>
            <p className="text-sm text-muted-foreground">Linked RetroAchievements account</p>
            <p className="font-mono text-lg">@{raUsername}</p>
          </div>
          <ApiKeyForm hasApiKey={hasApiKey} raUsername={raUsername} />
        </section>
      )}
    </div>
  );
}
