import { AdminDiscordWebhooksSection } from "@/components/admin/admin-discord-webhooks-section";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function AdminWebhooksPage() {
  try {
    const api = await getServerApiClient();
    const [webhooks, tokenCatalog, dispatchRuns] = await Promise.all([
      api.getAdminDiscordWebhooks(),
      api.getAdminDiscordWebhookTokenCatalog(),
      api.getAdminDiscordWebhookDispatchRuns(15),
    ]);

    return (
      <AdminDiscordWebhooksSection
        initialWebhooks={webhooks}
        tokenCatalog={tokenCatalog}
        dispatchRuns={dispatchRuns}
      />
    );
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to load webhooks";
    const forbidden = message.includes("403");
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          {forbidden ? "Not authorized" : "Could not load webhooks"}
        </h1>
        <p className="text-muted-foreground">{message}</p>
      </div>
    );
  }
}
