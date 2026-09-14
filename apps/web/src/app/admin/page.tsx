import { getServerSession } from "next-auth";
import { AdminOpsView } from "@/components/admin/admin-ops-view";
import { authOptions } from "@/lib/auth-options";
import { getServerApiClient } from "@/lib/api";
import { isDiscordUserAdmin } from "@/lib/allowed-discord-users";

export const dynamic = "force-dynamic";

function resolveHangfireHref(pathOrUrl: string) {
  if (pathOrUrl.startsWith("http://") || pathOrUrl.startsWith("https://")) {
    return pathOrUrl;
  }
  const base = (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:18943").replace(/\/$/, "");
  return `${base}${pathOrUrl.startsWith("/") ? pathOrUrl : `/${pathOrUrl}`}`;
}

export default async function AdminPage() {
  const session = await getServerSession(authOptions);
  const discordId = session?.user?.discordId;

  if (!isDiscordUserAdmin(discordId)) {
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          Not authorized
        </h1>
        <p className="text-muted-foreground">
          This page is only available to administrators configured in{" "}
          <code className="text-sm">AUTH_ADMIN_DISCORD_USER_IDS</code>.
        </p>
      </div>
    );
  }

  try {
    const api = await getServerApiClient();
    const ops = await api.getAdminOps();
    return <AdminOpsView ops={ops} hangfireDashboardHref={resolveHangfireHref(ops.config.hangfireDashboardUrl)} />;
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to load admin metrics";
    const forbidden = message.includes("403");
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          {forbidden ? "Not authorized" : "Could not load admin metrics"}
        </h1>
        <p className="text-muted-foreground">{forbidden ? "Your account is not in the admin list on the API." : message}</p>
      </div>
    );
  }
}
