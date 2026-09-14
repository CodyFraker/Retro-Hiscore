import { getServerSession } from "next-auth";
import { AdminMemberInvitesSection } from "@/components/admin/admin-member-invites-section";
import { AdminOpsView } from "@/components/admin/admin-ops-view";
import { AdminSyncSettingsSection } from "@/components/admin/admin-sync-settings-section";
import { authOptions } from "@/lib/auth-options";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

function resolveHangfireHref(pathOrUrl: string) {
  if (pathOrUrl.startsWith("http://") || pathOrUrl.startsWith("https://")) {
    return pathOrUrl;
  }
  return pathOrUrl.startsWith("/") ? pathOrUrl : `/${pathOrUrl}`;
}

export default async function AdminPage() {
  const session = await getServerSession(authOptions);

  if (!session?.isAdmin) {
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          Not authorized
        </h1>
        <p className="text-muted-foreground">This page is only available to site administrators.</p>
      </div>
    );
  }

  let ops;
  let invites;
  let syncSettings;
  try {
    const api = await getServerApiClient();
    [ops, invites, syncSettings] = await Promise.all([
      api.getAdminOps(),
      api.getAdminMemberInvites(),
      api.getAdminSyncSettings(),
    ]);
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to load admin metrics";
    const forbidden = message.includes("403");
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          {forbidden ? "Not authorized" : "Could not load admin metrics"}
        </h1>
        <p className="text-muted-foreground">
          {forbidden ? "Your account is not an administrator on the API." : message}
        </p>
      </div>
    );
  }

  const syncSettingsKey = JSON.stringify(syncSettings);

  return (
    <div className="space-y-10">
      <section className="rounded border border-border p-5">
        <AdminMemberInvitesSection initialInvites={invites} />
      </section>
      <AdminSyncSettingsSection key={syncSettingsKey} initialSettings={syncSettings} />
      <AdminOpsView ops={ops} hangfireDashboardHref={resolveHangfireHref(ops.config.hangfireDashboardUrl)} />
    </div>
  );
}
