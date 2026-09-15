import { AdminManualSyncActions } from "@/components/admin/admin-manual-sync-actions";
import { AdminOpsView } from "@/components/admin/admin-ops-view";
import { AdminRecentSyncRunsCard } from "@/components/admin/admin-recent-sync-runs-card";
import { AdminSyncHealthStrip } from "@/components/admin/admin-sync-health-strip";
import { AdminSyncSettingsSection } from "@/components/admin/admin-sync-settings-section";
import { formatSyncTime } from "@/lib/format";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

function resolveHangfireHref(pathOrUrl: string) {
  if (pathOrUrl.startsWith("http://") || pathOrUrl.startsWith("https://")) {
    return pathOrUrl;
  }
  return pathOrUrl.startsWith("/") ? pathOrUrl : `/${pathOrUrl}`;
}

function scoreRefreshDisabledReason(manualCooldownUntil: string | null | undefined): string | null {
  if (!manualCooldownUntil) {
    return null;
  }
  const until = new Date(manualCooldownUntil);
  if (until.getTime() <= Date.now()) {
    return null;
  }
  return `Manual refresh on cooldown until ${formatSyncTime(manualCooldownUntil)}`;
}

export default async function AdminSyncPage() {
  let ops;
  let syncSettings;
  try {
    const api = await getServerApiClient();
    [ops, syncSettings] = await Promise.all([api.getAdminOps(), api.getAdminSyncSettings()]);
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to load sync admin data";
    const forbidden = message.includes("403");
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          {forbidden ? "Not authorized" : "Could not load sync settings"}
        </h1>
        <p className="text-muted-foreground">
          {forbidden ? "Your account is not an administrator on the API." : message}
        </p>
      </div>
    );
  }

  const syncSettingsKey = JSON.stringify(syncSettings);
  const cooldownReason = scoreRefreshDisabledReason(ops.health.manualCooldownUntil);

  const hangfireHref = resolveHangfireHref(ops.config.hangfireDashboardUrl);

  return (
    <div className="space-y-8">
      <AdminSyncHealthStrip ops={ops} />

      <div className="grid gap-8 lg:grid-cols-2 lg:items-start">
        <AdminManualSyncActions
          lastSyncByKind={ops.lastSyncByKind}
          scoreRefreshDisabledReason={cooldownReason}
        />
        <AdminRecentSyncRunsCard recentRuns={ops.recentRuns} hangfireDashboardHref={hangfireHref} />
      </div>

      <AdminSyncSettingsSection key={syncSettingsKey} initialSettings={syncSettings} />
      <AdminOpsView ops={ops} hangfireDashboardHref={hangfireHref} showScheduler={false} />
    </div>
  );
}
